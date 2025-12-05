using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Dialog_1 : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    public string[] lines;
    public float textSpeed;
    public float autoAdvanceDelay = 3f; // Tiempo de espera antes de avanzar automáticamente
    public float initialDelay = 1f; // Tiempo de espera del game
    public string nextSceneName = "Level_1_Cerro"; // Nombre de la escena a cargar
    public AudioClip npcVoice; // Sonido que se reproduce con cada letra
    public int soundEveryNLetters = 2; // Reproducir sonido cada N letras (1 = cada letra, 2 = cada 2 letras, etc.)

    private int index;
    private bool isLineComplete = false;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        textComponent.text = string.Empty;
        StartCoroutine(StartDialogueWithDelay());
    }

    void Update()
    {
        // Detectar click del mouse O touch en pantalla
        bool inputDetectado = false;
        
        // Input de mouse (PC)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            inputDetectado = true;
        }
        
        // Input de touch (Móvil) - Detectar cualquier toque en la pantalla
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            inputDetectado = true;
        }
        
        if (inputDetectado)
        {
            if (textComponent.text == lines[index])
            {
                NextLine();
            }
            else
            {
                StopAllCoroutines();
                textComponent.text = lines[index];
                isLineComplete = true;
                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                }
                StartCoroutine(AutoAdvance());
            }
        }
    }
    
    IEnumerator StartDialogueWithDelay()
    {
        yield return new WaitForSeconds(initialDelay);
        StartDialogue();
    }

    void StartDialogue()
    {
        index = 0;
        isLineComplete = false;
        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        isLineComplete = false;
        int letterCount = 0; // Contador de letras
        
        foreach (char c in lines[index].ToCharArray())
        {
            textComponent.text += c;
            letterCount++;
            
            // Reproducir sonido cada N letras si hay un AudioClip asignado
            if (npcVoice != null && audioSource != null && letterCount % soundEveryNLetters == 0)
            {
                audioSource.PlayOneShot(npcVoice);
            }
            
            yield return new WaitForSeconds(textSpeed);
        }
        
        // Detener el sonido cuando termina la línea
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        isLineComplete = true;
        StartCoroutine(AutoAdvance());
    }

    IEnumerator AutoAdvance()
    {
        yield return new WaitForSeconds(autoAdvanceDelay);
        if (isLineComplete && textComponent.text == lines[index])
        {
            NextLine();
        }
    }

    void NextLine()
    {
        if (index < lines.Length - 1)
        {
            index++;
            textComponent.text = string.Empty;
            StopAllCoroutines();
            
            // Detener sonido antes de empezar nueva línea
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            
            StartCoroutine(TypeLine());
        } 
        else
        {
            // Detener sonido antes de cambiar de escena
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            
            // Cargar la siguiente escena
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
