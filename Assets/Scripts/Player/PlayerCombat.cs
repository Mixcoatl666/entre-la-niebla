using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Sistema de combate del jugador
/// - Detecta input de ataque (clic izquierdo o tecla K)
/// - Activa animación de ataque
/// - Detecta enemigos en rango y les causa daño
/// </summary>
public class PlayerCombat : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Referencia al PlayerMovement")]
    public PlayerMovement playerMovement;
    
    [Tooltip("Animator del jugador")]
    public Animator animator;
    
    [Tooltip("Rigidbody2D del jugador")]
    public Rigidbody2D rb;
    
    [Header("Configuración de Ataque")]
    [Tooltip("Daño que causa la espada")]
    public float danioEspada = 30f;
    
    [Tooltip("Punto desde donde se detectan enemigos (frente al jugador)")]
    public Transform puntoAtaque;
    
    [Tooltip("Radio de detección de ataque")]
    public float rangoAtaque = 1.5f;
    
    [Tooltip("Capas que se pueden golpear (Enemy, Boss, etc.)")]
    public LayerMask capasEnemigos;
    
    [Tooltip("¿Detener movimiento durante ataque?")]
    public bool detenerMovimientoDuranteAtaque = true;
    
    [Header("Cooldown")]
    [Tooltip("Tiempo mínimo entre ataques (segundos)")]
    public float cooldownAtaque = 0.5f;
    private float tiempoUltimoAtaque = -999f;
    
    [Header("Audio (Opcional)")]
    public AudioSource audioSource;
    public AudioClip sonidoAtaque;
    [Range(0f, 1f)]
    public float volumen = 0.8f;
    
    [Header("Audio Mixer (Opcional)")]
    [Tooltip("Audio Mixer Group para sonidos de combate")]
    public UnityEngine.Audio.AudioMixerGroup audioMixerGroup;
    
    [Header("Debug")]
    public bool mostrarGizmos = true;
    public bool mostrarMensajesDebug = true;
    
    private bool estaAtacando = false;

    void Start()
    {
        // Buscar componentes si no están asignados
        if (playerMovement == null)
        {
            playerMovement = GetComponent<PlayerMovement>();
        }
        
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
        
        // Configurar AudioSource
        if (audioSource == null && sonidoAtaque != null)
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
                    Debug.Log($"[PlayerCombat] Audio Mixer Group asignado: {audioMixerGroup.name}");
                }
            }
        }
        
        // Crear punto de ataque si no existe
        if (puntoAtaque == null)
        {
            GameObject punto = new GameObject("PuntoAtaque");
            punto.transform.SetParent(transform);
            punto.transform.localPosition = new Vector3(0.5f, 0f, 0f); // Frente al jugador
            puntoAtaque = punto.transform;
            
            if (mostrarMensajesDebug)
            {
                Debug.Log("[PlayerCombat] Punto de ataque creado automáticamente");
            }
        }
    }

    void Update()
    {
        // Solo permitir atacar si tiene la espada
        if (playerMovement == null || !playerMovement.HasSword)
            return;
        
        // No permitir atacar si está muerto
        if (playerMovement.estaMuerto)
            return;
        
        // Detectar input de ataque (teclado/mouse)
        bool inputAtaque = false;
        
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            inputAtaque = true;
        }
        
        if (Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame)
        {
            inputAtaque = true;
        }
        
        // Ejecutar ataque si se presionó el botón y pasó el cooldown
        if (inputAtaque && Time.time >= tiempoUltimoAtaque + cooldownAtaque && !estaAtacando)
        {
            Atacar();
        }
    }
    
    /// <summary>
    /// Método público para ejecutar ataque desde controles móviles
    /// </summary>
    public void MobileAttack()
    {
        // Verificar si puede atacar
        if (playerMovement == null || !playerMovement.HasSword)
            return;
        
        if (playerMovement.estaMuerto)
            return;
        
        if (Time.time >= tiempoUltimoAtaque + cooldownAtaque && !estaAtacando)
        {
            Atacar();
        }
    }

    void Atacar()
    {
        estaAtacando = true;
        tiempoUltimoAtaque = Time.time;
        
        // Notificar al PlayerMovement que está atacando
        if (playerMovement != null)
        {
            playerMovement.estaAtacando = true;
        }
        
        // Detener movimiento durante ataque
        if (detenerMovimientoDuranteAtaque && rb != null)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
        
        // Activar animación de ataque
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        
        // Reproducir sonido de ataque
        if (sonidoAtaque != null && audioSource != null)
        {
            audioSource.PlayOneShot(sonidoAtaque, volumen);
        }
        
        if (mostrarMensajesDebug)
        {
            Debug.Log("[PlayerCombat] ¡Atacando!");
        }
        
        // Llamar a DetectarGolpe después de un pequeño delay (para sincronizar con la animación)
        Invoke("DetectarGolpe", 0.2f);
        
        // Resetear estado de ataque
        Invoke("FinalizarAtaque", 0.4f);
    }

    void DetectarGolpe()
    {
        if (puntoAtaque == null)
        {
            Debug.LogWarning("[PlayerCombat] No hay punto de ataque asignado");
            return;
        }
        
        // Detectar enemigos en rango
        Collider2D[] enemigosGolpeados = Physics2D.OverlapCircleAll(puntoAtaque.position, rangoAtaque, capasEnemigos);
        
        if (mostrarMensajesDebug && enemigosGolpeados.Length > 0)
        {
            Debug.Log($"[PlayerCombat] Detectados {enemigosGolpeados.Length} enemigos en rango");
        }
        
        // Causar daño a cada enemigo detectado
        foreach (Collider2D enemigo in enemigosGolpeados)
        {
            // Intentar dañar como Boss
            BossController boss = enemigo.GetComponent<BossController>();
            if (boss != null)
            {
                boss.RecibirDanio(danioEspada);
                if (mostrarMensajesDebug)
                {
                    Debug.Log($"[PlayerCombat] ¡Golpeaste al jefe! Daño: {danioEspada}");
                }
                continue;
            }
            
            // Aquí puedes agregar otros tipos de enemigos
            // EnemyController enemy = enemigo.GetComponent<EnemyController>();
            // if (enemy != null) enemy.RecibirDanio(danioEspada);
            
            if (mostrarMensajesDebug)
            {
                Debug.Log($"[PlayerCombat] Golpeaste a: {enemigo.name}");
            }
        }
    }

    void FinalizarAtaque()
    {
        estaAtacando = false;
        
        // Notificar al PlayerMovement que terminó el ataque
        if (playerMovement != null)
        {
            playerMovement.estaAtacando = false;
        }
    }
    
    /// <summary>
    /// Método público para verificar si está atacando (útil para PlayerMovement)
    /// </summary>
    public bool EstaAtacando()
    {
        return estaAtacando;
    }

    /// <summary>
    /// Método público para activar/desactivar la espada
    /// </summary>
    public void ActivarEspada(bool activar)
    {
        if (playerMovement != null)
        {
            playerMovement.HasSword = activar;
            
            if (mostrarMensajesDebug)
            {
                Debug.Log($"[PlayerCombat] Espada {(activar ? "ACTIVADA" : "DESACTIVADA")}");
            }
        }
    }

    // Visualizar rango de ataque en el editor
    void OnDrawGizmos()
    {
        if (!mostrarGizmos || puntoAtaque == null) return;
        
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(puntoAtaque.position, rangoAtaque);
    }

    void OnDrawGizmosSelected()
    {
        if (!mostrarGizmos || puntoAtaque == null) return;
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(puntoAtaque.position, rangoAtaque);
    }
}
