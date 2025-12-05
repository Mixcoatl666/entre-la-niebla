using UnityEngine;
using TMPro;

/// <summary>
/// Trigger que inicia un diálogo automáticamente cuando el jugador entra
/// Compatible con Dialog_2 pero automático (sin presionar E)
/// </summary>
public class TriggerDialogo : MonoBehaviour
{
    [Header("Contenido del Diálogo")]
    [TextArea(3, 10)]
    public string[] lineas; // Las líneas de diálogo
    public string nombreNPC = "Narrador"; // Nombre que aparece (ej: "Narrador", "???")
    
    [Header("Referencias UI (Asignar en el primer trigger)")]
    public GameObject dialogPanelReference; // Panel compartido
    public TextMeshProUGUI dialogTextReference; // Texto compartido
    public TextMeshProUGUI nameTextReference; // Nombre compartido
    
    [Header("Configuración")]
    public float velocidadTexto = 0.05f;
    public bool activarSoloUnaVez = true;
    public bool desactivarDespuesDeUsar = false;
    public bool congelarJugador = true;
    public bool cerrarAlTerminar = true;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip sonidoVoz;
    [Range(0f, 1f)]
    public float volumen = 0.5f;
    public int sonidoCadaNLetras = 2;
    
    [Header("Audio Mixer (Opcional)")]
    [Tooltip("Audio Mixer Group para el diálogo (ej: 'Dialogue', 'SFX')")]
    public UnityEngine.Audio.AudioMixerGroup audioMixerGroup;
    
    [Header("Debug")]
    public bool mostrarMensajesDebug = true;
    
    // Static (compartido)
    private static GameObject dialogPanel;
    private static TextMeshProUGUI dialogText;
    private static TextMeshProUGUI nameText;
    private static TriggerDialogo dialogoActivo;
    
    // Private
    private bool yaActivado = false;
    private bool dialogoEnCurso = false;
    private int lineaActual = 0;
    private bool lineaCompleta = false;
    private PlayerMovement playerMovement;
    private Rigidbody2D playerRb;

    void Start()
    {
        // Compartir referencias UI
        if (dialogPanelReference != null)
            dialogPanel = dialogPanelReference;
        if (dialogTextReference != null)
            dialogText = dialogTextReference;
        if (nameTextReference != null)
            nameText = nameTextReference;
        
        // Configurar AudioSource
        if (audioSource == null && sonidoVoz != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.volume = volumen;
            
            // Asignar Audio Mixer Group si existe
            if (audioMixerGroup != null)
            {
                audioSource.outputAudioMixerGroup = audioMixerGroup;
                if (mostrarMensajesDebug)
                {
                    Debug.Log($"[TriggerDialogo] Audio Mixer Group asignado: {audioMixerGroup.name}");
                }
            }
        }
        
        // Verificar collider
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogError($"[TriggerDialogo] {gameObject.name} necesita un Collider2D!");
        }
        else if (!col.isTrigger)
        {
            col.isTrigger = true;
            if (mostrarMensajesDebug)
            {
                Debug.LogWarning($"[TriggerDialogo] Collider configurado como Trigger automáticamente");
            }
        }
        
        // Ocultar panel al inicio
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(false);
        }
    }

    void Update()
    {
        // Solo procesar input si este diálogo está activo
        if (dialogoEnCurso && dialogoActivo == this)
        {
            // Click, Space o E para avanzar
            bool avanzar = false;
            
            if (UnityEngine.InputSystem.Mouse.current != null && 
                UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
                avanzar = true;
            
            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                if (UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame ||
                    UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame)
                    avanzar = true;
            }
            
            if (avanzar)
            {
                if (lineaCompleta)
                {
                    SiguienteLinea();
                }
                else
                {
                    // Completar línea actual
                    StopAllCoroutines();
                    dialogText.text = lineas[lineaActual];
                    lineaCompleta = true;
                    if (audioSource != null && audioSource.isPlaying)
                    {
                        audioSource.Stop();
                    }
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;
        
        if (mostrarMensajesDebug)
        {
            Debug.Log($"[TriggerDialogo] Jugador detectado en {gameObject.name}");
        }
        
        // Verificar si ya fue activado
        if (activarSoloUnaVez && yaActivado)
        {
            if (mostrarMensajesDebug)
            {
                Debug.Log($"[TriggerDialogo] Ya fue activado anteriormente");
            }
            return;
        }
        
        // No iniciar si hay otro diálogo activo
        if (dialogoActivo != null && dialogoActivo != this)
        {
            if (mostrarMensajesDebug)
            {
                Debug.Log($"[TriggerDialogo] Hay otro diálogo activo");
            }
            return;
        }
        
        IniciarDialogo();
    }

    void IniciarDialogo()
    {
        if (lineas == null || lineas.Length == 0)
        {
            Debug.LogWarning($"[TriggerDialogo] No hay líneas configuradas en {gameObject.name}");
            return;
        }
        
        if (dialogPanel == null || dialogText == null)
        {
            Debug.LogError("[TriggerDialogo] Referencias UI no configuradas. Asigna 'Dialog Panel Reference' y 'Dialog Text Reference'");
            return;
        }
        
        dialogoEnCurso = true;
        dialogoActivo = this;
        lineaActual = 0;
        yaActivado = true;
        
        // Congelar jugador
        if (congelarJugador)
        {
            CongelarJugador();
        }
        
        // Mostrar panel
        dialogPanel.SetActive(true);
        
        // Mostrar nombre
        if (nameText != null)
        {
            nameText.text = nombreNPC;
        }
        
        // Empezar primera línea
        StartCoroutine(EscribirLinea());
        
        if (mostrarMensajesDebug)
        {
            Debug.Log($"[TriggerDialogo] Diálogo iniciado: {nombreNPC}");
        }
    }

    System.Collections.IEnumerator EscribirLinea()
    {
        if (lineaActual >= lineas.Length)
        {
            FinalizarDialogo();
            yield break;
        }
        
        lineaCompleta = false;
        dialogText.text = string.Empty;
        int contadorLetras = 0;
        
        foreach (char c in lineas[lineaActual].ToCharArray())
        {
            dialogText.text += c;
            contadorLetras++;
            
            // Reproducir sonido
            if (sonidoVoz != null && audioSource != null && contadorLetras % sonidoCadaNLetras == 0)
            {
                audioSource.PlayOneShot(sonidoVoz);
            }
            
            yield return new WaitForSeconds(velocidadTexto);
        }
        
        lineaCompleta = true;
        
        // Cortar el sonido en seco cuando termina el texto
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    void SiguienteLinea()
    {
        lineaActual++;
        
        if (lineaActual < lineas.Length)
        {
            StopAllCoroutines();
            StartCoroutine(EscribirLinea());
        }
        else
        {
            if (cerrarAlTerminar)
            {
                FinalizarDialogo();
            }
        }
    }

    void FinalizarDialogo()
    {
        dialogoEnCurso = false;
        dialogoActivo = null;
        
        // Detener sonidos
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        // Descongelar jugador
        if (congelarJugador)
        {
            DescongelarJugador();
        }
        
        // Ocultar panel
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(false);
        }
        
        // Limpiar texto
        if (dialogText != null)
        {
            dialogText.text = string.Empty;
        }
        
        // Desactivar trigger si está configurado
        if (desactivarDespuesDeUsar)
        {
            gameObject.SetActive(false);
        }
        
        if (mostrarMensajesDebug)
        {
            Debug.Log($"[TriggerDialogo] Diálogo finalizado");
        }
    }

    void CongelarJugador()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerMovement = player.GetComponent<PlayerMovement>();
            playerRb = player.GetComponent<Rigidbody2D>();
            
            if (playerMovement != null)
            {
                playerMovement.enabled = false;
            }
            
            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector2.zero;
                playerRb.angularVelocity = 0f;
            }
        }
    }

    void DescongelarJugador()
    {
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }
        
        playerMovement = null;
        playerRb = null;
    }

    // Visualización en editor
    void OnDrawGizmos()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = yaActivado ? new Color(0, 1, 0, 0.3f) : new Color(0, 0.5f, 1f, 0.3f);
            Gizmos.DrawCube(transform.position, col.bounds.size);
            
            Gizmos.color = yaActivado ? Color.green : Color.cyan;
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = new Color(0, 0.5f, 1f, 0.5f);
            Gizmos.DrawCube(transform.position, col.bounds.size);
        }
    }
}
