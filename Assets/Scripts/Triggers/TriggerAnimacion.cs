using UnityEngine;

/// <summary>
/// Trigger invisible que reproduce una animación cuando el jugador lo toca
/// </summary>
public class TriggerAnimacion : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Animator del objeto que reproducirá la animación")]
    public Animator animatorObjetivo;
    
    [Tooltip("Nombre del trigger/parámetro en el Animator")]
    public string nombreTrigger = "Activar";
    
    [Tooltip("Tipo de parámetro del Animator")]
    public TipoParametro tipoParametro = TipoParametro.Trigger;
    
    [Header("Opciones")]
    [Tooltip("Se puede activar solo una vez")]
    public bool activarSoloUnaVez = true;
    
    [Tooltip("Desactivar el trigger después de usarlo")]
    public bool desactivarDespuesDeUsar = false;
    
    [Tooltip("Tag requerido para activar (dejar vacío para cualquier objeto)")]
    public string tagRequerido = "Player";
    
    [Header("Audio Opcional")]
    public AudioSource audioSource;
    public AudioClip sonidoActivacion;
    [Range(0f, 1f)]
    public float volumen = 1f;
    
    [Header("Audio Mixer (Opcional)")]
    [Tooltip("Audio Mixer Group para el sonido (ej: 'SFX', 'Environment')")]
    public UnityEngine.Audio.AudioMixerGroup audioMixerGroup;
    
    [Header("Debug")]
    public bool mostrarMensajesDebug = true;
    
    private bool yaActivado = false;
    
    public enum TipoParametro
    {
        Trigger,    // animator.SetTrigger()
        Bool,       // animator.SetBool(true)
        Integer,    // animator.SetInteger(1)
        Float       // animator.SetFloat(1f)
    }

    void Start()
    {
        // Si no hay Animator asignado, buscar en el mismo GameObject
        if (animatorObjetivo == null)
        {
            animatorObjetivo = GetComponent<Animator>();
            if (animatorObjetivo == null)
            {
                Debug.LogError($"[TriggerAnimacion] ? No hay Animator asignado en {gameObject.name}");
            }
            else
            {
                Debug.Log($"[TriggerAnimacion] ? Animator encontrado en {gameObject.name}");
            }
        }
        
        // Verificar que el parámetro existe en el Animator
        if (animatorObjetivo != null)
        {
            bool parametroExiste = false;
            
            foreach (AnimatorControllerParameter param in animatorObjetivo.parameters)
            {
                if (param.name == nombreTrigger)
                {
                    parametroExiste = true;
                    
                    // Verificar que el tipo coincida
                    string tipoEsperado = tipoParametro.ToString();
                    string tipoReal = param.type.ToString();
                    
                    if (tipoReal != tipoEsperado)
                    {
                        Debug.LogWarning($"[TriggerAnimacion] ?? El parámetro '{nombreTrigger}' es de tipo {tipoReal}, pero el script está configurado como {tipoEsperado}");
                    }
                    else
                    {
                        Debug.Log($"[TriggerAnimacion] ? Parámetro '{nombreTrigger}' ({tipoReal}) encontrado correctamente");
                    }
                    break;
                }
            }
            
            if (!parametroExiste)
            {
                Debug.LogError($"[TriggerAnimacion] ? El parámetro '{nombreTrigger}' NO existe en el Animator. Revisa el nombre.");
                Debug.Log($"[TriggerAnimacion] Parámetros disponibles:");
                foreach (AnimatorControllerParameter param in animatorObjetivo.parameters)
                {
                    Debug.Log($"  - {param.name} ({param.type})");
                }
            }
        }
        
        // Verificar que tenga un Collider2D configurado como trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogError($"[TriggerAnimacion] ? {gameObject.name} necesita un Collider2D!");
        }
        else if (!col.isTrigger)
        {
            Debug.LogWarning($"[TriggerAnimacion] ?? El Collider2D de {gameObject.name} no está marcado como Trigger. Configurando automáticamente...");
            col.isTrigger = true;
        }
        else
        {
            Debug.Log($"[TriggerAnimacion] ? Collider2D configurado correctamente como Trigger");
        }
        
        // Configurar AudioSource si hay sonido pero no AudioSource
        if (sonidoActivacion != null && audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = volumen;
        }
        
        // Asignar Audio Mixer Group si existe
        if (audioSource != null && audioMixerGroup != null)
        {
            audioSource.outputAudioMixerGroup = audioMixerGroup;
            if (mostrarMensajesDebug)
            {
                Debug.Log($"[TriggerAnimacion] Audio Mixer Group asignado: {audioMixerGroup.name}");
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (mostrarMensajesDebug)
        {
            Debug.Log($"[TriggerAnimacion] ?? Objeto detectado: {other.name} (Tag: {other.tag})");
        }
        
        // Verificar si ya fue activado
        if (activarSoloUnaVez && yaActivado)
        {
            if (mostrarMensajesDebug)
            {
                Debug.Log($"[TriggerAnimacion] ?? Ya fue activado anteriormente");
            }
            return;
        }
        
        // Verificar tag requerido
        if (!string.IsNullOrEmpty(tagRequerido) && !other.CompareTag(tagRequerido))
        {
            if (mostrarMensajesDebug)
            {
                Debug.Log($"[TriggerAnimacion] ? Tag incorrecto. Requerido: '{tagRequerido}', Detectado: '{other.tag}'");
            }
            return;
        }
        
        // Activar la animación
        ActivarAnimacion();
        
        if (mostrarMensajesDebug)
        {
            Debug.Log($"[TriggerAnimacion] ? {gameObject.name} activado por {other.name}");
        }
    }

    /// <summary>
    /// Activa la animación según el tipo de parámetro configurado
    /// </summary>
    public void ActivarAnimacion()
    {
        if (animatorObjetivo == null)
        {
            Debug.LogError($"[TriggerAnimacion] ? No hay Animator asignado en {gameObject.name}");
            return;
        }
        
        // Reproducir sonido si está configurado
        if (sonidoActivacion != null && audioSource != null)
        {
            audioSource.PlayOneShot(sonidoActivacion, volumen);
        }
        
        // Activar el parámetro del Animator según el tipo
        switch (tipoParametro)
        {
            case TipoParametro.Trigger:
                animatorObjetivo.SetTrigger(nombreTrigger);
                if (mostrarMensajesDebug)
                {
                    Debug.Log($"[TriggerAnimacion] ?? SetTrigger('{nombreTrigger}') ejecutado");
                }
                break;
                
            case TipoParametro.Bool:
                animatorObjetivo.SetBool(nombreTrigger, true);
                if (mostrarMensajesDebug)
                {
                    Debug.Log($"[TriggerAnimacion] ?? SetBool('{nombreTrigger}', true) ejecutado");
                }
                break;
                
            case TipoParametro.Integer:
                animatorObjetivo.SetInteger(nombreTrigger, 1);
                if (mostrarMensajesDebug)
                {
                    Debug.Log($"[TriggerAnimacion] ?? SetInteger('{nombreTrigger}', 1) ejecutado");
                }
                break;
                
            case TipoParametro.Float:
                animatorObjetivo.SetFloat(nombreTrigger, 1f);
                if (mostrarMensajesDebug)
                {
                    Debug.Log($"[TriggerAnimacion] ?? SetFloat('{nombreTrigger}', 1f) ejecutado");
                }
                break;
        }
        
        yaActivado = true;
        
        // Desactivar el trigger si está configurado
        if (desactivarDespuesDeUsar)
        {
            gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Resetea el trigger para poder usarlo nuevamente
    /// </summary>
    public void Resetear()
    {
        yaActivado = false;
        
        if (tipoParametro == TipoParametro.Bool)
        {
            animatorObjetivo.SetBool(nombreTrigger, false);
        }
        else if (tipoParametro == TipoParametro.Integer)
        {
            animatorObjetivo.SetInteger(nombreTrigger, 0);
        }
        else if (tipoParametro == TipoParametro.Float)
        {
            animatorObjetivo.SetFloat(nombreTrigger, 0f);
        }
        
        if (mostrarMensajesDebug)
        {
            Debug.Log($"[TriggerAnimacion] ?? {gameObject.name} reseteado");
        }
    }

    // Visualizar el área del trigger en el editor
    void OnDrawGizmos()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = yaActivado ? new Color(0, 1, 0, 0.3f) : new Color(1, 1, 0, 0.3f);
            Gizmos.DrawCube(transform.position, col.bounds.size);
            
            Gizmos.color = yaActivado ? Color.green : Color.yellow;
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = new Color(1, 0, 0, 0.5f);
            Gizmos.DrawCube(transform.position, col.bounds.size);
        }
    }
}
