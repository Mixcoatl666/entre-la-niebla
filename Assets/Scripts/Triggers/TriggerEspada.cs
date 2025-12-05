using UnityEngine;
using System.Collections;

/// <summary>
/// Trigger que otorga la espada al jugador
/// - Muestra un panel con la imagen de la espada por 2 segundos
/// - Activa el bool HasSword en el jugador
/// - Se activa solo una vez
/// </summary>
public class TriggerEspada : MonoBehaviour
{
    [Header("UI de la Espada")]
    [Tooltip("Panel que muestra cuando obtienes la espada")]
    public GameObject panelEspada;
    
    [Tooltip("Tiempo que se muestra el panel (segundos)")]
    public float tiempoPanelVisible = 2f;
    
    [Header("Configuración")]
    [Tooltip("¿Congelar al jugador mientras se muestra el panel?")]
    public bool congelarJugador = true;
    
    [Tooltip("¿Desactivar el trigger después de usarlo?")]
    public bool desactivarDespuesDeUsar = true;
    
    [Header("Audio (Opcional)")]
    public AudioSource audioSource;
    public AudioClip sonidoObtenerEspada;
    [Range(0f, 1f)]
    public float volumen = 1f;
    
    [Header("Audio Mixer (Opcional)")]
    [Tooltip("Audio Mixer Group para el sonido")]
    public UnityEngine.Audio.AudioMixerGroup audioMixerGroup;
    
    [Header("Debug")]
    public bool mostrarMensajesDebug = true;
    
    private bool yaActivado = false;
    private PlayerMovement playerMovement;
    private PlayerCombat playerCombat;
    private Rigidbody2D playerRb;

    void Start()
    {
        // Configurar AudioSource
        if (audioSource == null && sonidoObtenerEspada != null)
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
                    Debug.Log($"[TriggerEspada] Audio Mixer Group asignado: {audioMixerGroup.name}");
                }
            }
        }
        
        // Verificar collider
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogError($"[TriggerEspada] {gameObject.name} necesita un Collider2D!");
        }
        else if (!col.isTrigger)
        {
            col.isTrigger = true;
            if (mostrarMensajesDebug)
            {
                Debug.LogWarning($"[TriggerEspada] Collider configurado como Trigger automáticamente");
            }
        }
        
        // Ocultar panel al inicio
        if (panelEspada != null)
        {
            panelEspada.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[TriggerEspada] No se asignó el panel de la espada. Asígnalo en el Inspector.");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Verificar que sea el jugador
        if (!other.CompareTag("Player"))
            return;
        
        // Verificar si ya fue activado
        if (yaActivado)
        {
            if (mostrarMensajesDebug)
            {
                Debug.Log("[TriggerEspada] Ya fue activado anteriormente");
            }
            return;
        }
        
        // Obtener componentes del jugador
        playerMovement = other.GetComponent<PlayerMovement>();
        playerCombat = other.GetComponent<PlayerCombat>();
        playerRb = other.GetComponent<Rigidbody2D>();
        
        if (playerMovement == null)
        {
            Debug.LogError("[TriggerEspada] El jugador no tiene el componente PlayerMovement");
            return;
        }
        
        // Activar la espada
        ObtenerEspada();
    }

    void ObtenerEspada()
    {
        yaActivado = true;
        
        // Activar la espada en el jugador
        if (playerMovement != null)
        {
            playerMovement.HasSword = true;
        }
        
        if (playerCombat != null)
        {
            playerCombat.ActivarEspada(true);
        }
        
        // Reproducir sonido
        if (sonidoObtenerEspada != null && audioSource != null)
        {
            audioSource.PlayOneShot(sonidoObtenerEspada, volumen);
        }
        
        if (mostrarMensajesDebug)
        {
            Debug.Log("[TriggerEspada] ¡Espada obtenida!");
        }
        
        // Mostrar panel y congelar jugador
        StartCoroutine(MostrarPanelEspada());
    }

    IEnumerator MostrarPanelEspada()
    {
        // Congelar jugador si está configurado
        if (congelarJugador)
        {
            CongelarJugador();
        }
        
        // Mostrar panel
        if (panelEspada != null)
        {
            panelEspada.SetActive(true);
        }
        
        // Esperar el tiempo configurado
        yield return new WaitForSeconds(tiempoPanelVisible);
        
        // Ocultar panel
        if (panelEspada != null)
        {
            panelEspada.SetActive(false);
        }
        
        // Descongelar jugador
        if (congelarJugador)
        {
            DescongelarJugador();
        }
        
        // Desactivar trigger si está configurado
        if (desactivarDespuesDeUsar)
        {
            gameObject.SetActive(false);
        }
        
        if (mostrarMensajesDebug)
        {
            Debug.Log("[TriggerEspada] Panel cerrado");
        }
    }

    void CongelarJugador()
    {
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

    void DescongelarJugador()
    {
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }
        
        playerMovement = null;
        playerCombat = null;
        playerRb = null;
    }

    // Visualización en editor
    void OnDrawGizmos()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = yaActivado ? new Color(0, 1, 0, 0.3f) : new Color(1, 0.5f, 0, 0.3f);
            Gizmos.DrawCube(transform.position, col.bounds.size);
            
            Gizmos.color = yaActivado ? Color.green : new Color(1f, 0.5f, 0f);
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
            Gizmos.DrawCube(transform.position, col.bounds.size);
        }
    }
}
