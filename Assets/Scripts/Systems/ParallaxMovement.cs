using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxMovement : MonoBehaviour
{
    [Header("Camera Reference")]
    Transform cam; // Main Camera
    
    [Header("Parallax Settings")]
    [Range(0.01f, 1f)]
    public float parallaxSpeed = 0.5f;
    public bool followCameraY = false; // Seguir también en Y (opcional)
    public float offsetY = 0f; // Offset vertical fijo

    private GameObject[] backgrounds;
    private Material[] mat;
    private float[] backSpeed;
    private float farthestBack;
    private Vector3 previousCamPos;

    void Start()
    {
        cam = Camera.main.transform;
        previousCamPos = cam.position;

        int backCount = transform.childCount;
        mat = new Material[backCount];
        backSpeed = new float[backCount];
        backgrounds = new GameObject[backCount];

        for (int i = 0; i < backCount; i++)
        {
            backgrounds[i] = transform.GetChild(i).gameObject;
            mat[i] = backgrounds[i].GetComponent<Renderer>().material;
        }

        BackSpeedCalculate(backCount);
    }

    void BackSpeedCalculate(int backCount)
    {
        for (int i = 0; i < backCount; i++) // Find the farthest background
        {
            if ((backgrounds[i].transform.position.z - cam.position.z) > farthestBack)
            {
                farthestBack = backgrounds[i].transform.position.z - cam.position.z;
            }
        }

        for (int i = 0; i < backCount; i++) // Set the speed of background
        {
            backSpeed[i] = 1 - (backgrounds[i].transform.position.z - cam.position.z) / farthestBack;
        }
    }

    void LateUpdate()
    {
        if (cam == null) return;

        // Calcular el movimiento delta de la cámara en este frame
        float deltaMovementX = cam.position.x - previousCamPos.x;
        
        // Actualizar la posición del contenedor de parallax
        float newY = followCameraY ? cam.position.y + offsetY : transform.position.y;
        transform.position = new Vector3(cam.position.x, newY, transform.position.z);

        // Aplicar el offset de textura basado en el movimiento delta
        for (int i = 0; i < backgrounds.Length; i++)
        {
            if (mat[i] != null)
            {
                float speed = backSpeed[i] * parallaxSpeed;
                Vector2 currentOffset = mat[i].GetTextureOffset("_MainTex");
                
                // Usar movimiento delta en lugar de distancia total
                currentOffset.x += deltaMovementX * speed;
                mat[i].SetTextureOffset("_MainTex", currentOffset);
            }
        }

        // Guardar la posición actual de la cámara para el próximo frame
        previousCamPos = cam.position;
    }
}