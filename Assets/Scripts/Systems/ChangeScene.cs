using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class ChangeScene : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Nombre exacto de la escena a cargar (debe estar en Build Settings)")]
    public string targetSceneName = ""; // Nombre de la escena a la que cambiará
    
    [Header("Interaction")]
    [SerializeField] private GameObject interactionMark; // El indicador "W" o flecha
    [Tooltip("Mostrar el marcador cuando el jugador esté en rango")]
    public bool showMark = true;
    
    [Header("Optional Settings")]
    [Tooltip("Delay antes de cambiar la escena (para animaciones/transiciones)")]
    public float sceneChangeDelay = 0f;
    [Tooltip("Sonido al cambiar de escena")]
    public AudioClip transitionSound;
    
    // Private variables
    private bool isPlayerInRange = false;
    private bool isChangingScene = false;
    private AudioSource audioSource;

    void Start()
    {
        // Configurar AudioSource si hay sonido
        if (transitionSound != null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        // Asegurarse de que el marcador esté oculto al inicio
        if (interactionMark != null)
        {
            interactionMark.SetActive(false);
        }

        // Validar que se haya configurado el nombre de la escena
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogWarning($"ChangeScene en '{gameObject.name}': No se ha configurado el nombre de la escena objetivo.");
        }
    }

    void Update()
    {
        // Solo procesar si el jugador está en rango y no está cambiando ya
        if (isPlayerInRange && !isChangingScene)
        {
            // Presionar W para cambiar de escena (teclado)
            if (Keyboard.current != null && Keyboard.current.wKey.wasPressedThisFrame)
            {
                TriggerSceneChange();
            }
        }
    }
    
    /// <summary>
    /// Método público para interactuar desde controles móviles
    /// Simula presionar W
    /// </summary>
    public void OnMobileInteract()
    {
        // Solo permitir si el jugador está en rango y no está cambiando ya
        if (isPlayerInRange && !isChangingScene)
        {
            TriggerSceneChange();
        }
    }

    void TriggerSceneChange()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError($"ChangeScene en '{gameObject.name}': No se puede cambiar de escena porque 'Target Scene Name' está vacío.");
            return;
        }

        isChangingScene = true;

        // Ocultar marcador
        if (interactionMark != null)
        {
            interactionMark.SetActive(false);
        }

        // Reproducir sonido si existe
        if (transitionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(transitionSound);
        }

        // Cambiar escena con o sin delay
        if (sceneChangeDelay > 0f)
        {
            Invoke(nameof(LoadScene), sceneChangeDelay);
        }
        else
        {
            LoadScene();
        }
    }

    void LoadScene()
    {
        // Verificar si la escena existe en Build Settings
        if (Application.CanStreamedLevelBeLoaded(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogError($"ChangeScene: La escena '{targetSceneName}' no existe o no está agregada en Build Settings. " +
                          $"Ve a File > Build Settings y agrega la escena.");
            isChangingScene = false;
        }
    }

    // Detectar cuando el jugador entra en el trigger
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;

            // Mostrar marcador
            if (interactionMark != null && showMark && !isChangingScene)
            {
                interactionMark.SetActive(true);
            }
        }
    }

    // Detectar cuando el jugador sale del trigger
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;

            // Ocultar marcador
            if (interactionMark != null)
            {
                interactionMark.SetActive(false);
            }
        }
    }

    // Visualizar el trigger en el editor
    private void OnDrawGizmos()
    {
        // Dibujar el área del trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.3f); // Cyan transparente
            
            if (col is BoxCollider2D boxCol)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(boxCol.offset, boxCol.size);
            }
            else if (col is CircleCollider2D circleCol)
            {
                Gizmos.DrawSphere(transform.position + (Vector3)circleCol.offset, circleCol.radius);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Dibujar el área del trigger cuando está seleccionado
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = Color.cyan;
            
            if (col is BoxCollider2D boxCol)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(boxCol.offset, boxCol.size);
            }
            else if (col is CircleCollider2D circleCol)
            {
                Gizmos.DrawWireSphere(transform.position + (Vector3)circleCol.offset, circleCol.radius);
            }
        }
    }
}
