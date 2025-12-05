using UnityEngine;
using UnityEngine.SceneManagement; // NUEVO: Para cargar escenas

/// <summary>
/// Controlador para el jefe final
/// - Sigue al jugador cuando está cerca
/// - Ataca al jugador en rango
/// - Muere después de 3 minutos o al perder toda la vida
/// </summary>
public class BossController : MonoBehaviour
{
    [Header("Estadísticas")]
    [Tooltip("Vida máxima del jefe")]
    public float vidaMaxima = 1000f;
    private float vidaActual;
    
    [Tooltip("Tiempo de vida antes de morir automáticamente (segundos)")]
    public float tiempoDeVida = 180f; // 3 minutos
    private float tiempoVivo = 0f;
    
    [Header("Movimiento")]
    [Tooltip("Velocidad de movimiento del jefe")]
    public float velocidadMovimiento = 3f;
    
    [Tooltip("Distancia a la que empieza a seguir al jugador")]
    public float rangoDeteccion = 10f;
    
    [Tooltip("Distancia mínima al jugador (para no pegarse demasiado)")]
    public float distanciaMinima = 1.5f;
    
    [Header("Combate")]
    [Tooltip("Distancia a la que puede atacar")]
    public float rangoAtaque = 2f;
    
    [Tooltip("Tiempo entre ataques (segundos)")]
    public float cooldownAtaque = 2f;
    private float tiempoUltimoAtaque = -999f;
    
    [Tooltip("Daño que causa al jugador (por el momento en 0)")]
    public float danio = 0f;
    
    [Header("Referencias")]
    [Tooltip("Animator del jefe")]
    public Animator animator;
    
    [Tooltip("Rigidbody2D del jefe")]
    public Rigidbody2D rb;
    
    [Tooltip("SpriteRenderer para voltear el sprite")]
    public SpriteRenderer spriteRenderer;
    
    [Header("Configuración de Dirección")]
    [Tooltip("¿Hacia dónde mira el sprite por defecto? (true = derecha, false = izquierda)")]
    public bool spriteMiraDerechaPorDefecto = true;
    
    [Header("Audio (Opcional)")]
    public AudioSource audioSource;
    public AudioClip sonidoAtaque;
    public AudioClip sonidoGolpe;
    public AudioClip sonidoMuerte;
    [Range(0f, 1f)]
    public float volumen = 0.7f;
    
    [Header("Audio Mixer (Opcional)")]
    [Tooltip("Audio Mixer Group para el jefe")]
    public UnityEngine.Audio.AudioMixerGroup audioMixerGroup;
    
    [Header("Victory Dialog")]
    [Tooltip("Diálogo que aparece al derrotar al jefe")]
    public Dialog_2 victoryDialog;
    
    [Tooltip("Tiempo de espera antes de mostrar el diálogo de victoria (segundos)")]
    public float tiempoAntesDeDialogo = 5f;
    
    [Tooltip("Nombre de la escena del menú principal")]
    public string nombreEscenaMenu = "MenuInicial";
    
    [Header("Debug")]
    public bool mostrarGizmos = true;
    public bool mostrarMensajesDebug = true;
    
    // Estados
    private enum EstadoJefe
    {
        Idle,
        Persiguiendo,
        Atacando,
        Recibiendo_Golpe,
        Muerto
    }
    
    private EstadoJefe estadoActual = EstadoJefe.Idle;
    private Transform jugador;
    private bool estaMuerto = false;
    private bool mirandoDerecha = true;
    private bool estaAtacando = false; // NUEVO: Flag para bloquear el estado durante ataque
    private bool ataqueEjecutado = false; // NUEVO: Flag para evitar ejecutar Atacar() múltiples veces
    
    // Parámetros del Animator
    private const string PARAM_IS_WALKING = "isWalking";
    private const string PARAM_ATTACK = "attack";
    private const string PARAM_HIT = "hit";
    private const string PARAM_IS_DEAD = "isDead";

    void Start()
    {
        // Inicializar vida
        vidaActual = vidaMaxima;
        
        // Buscar componentes si no están asignados
        if (animator == null)
            animator = GetComponent<Animator>();
        
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
        
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Configurar Rigidbody2D
        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.gravityScale = 1f;
            
            // IMPORTANTE: Configurar constraints para evitar rotación en todos los ejes
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        
        // Crear y aplicar Physics Material 2D sin fricción
        PhysicsMaterial2D sinFriccion = new PhysicsMaterial2D("BossMaterial");
        sinFriccion.friction = 0f;
        sinFriccion.bounciness = 0f;
        
        // Aplicar material a todos los colliders del jefe
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.sharedMaterial = sinFriccion;
        }
        
        if (mostrarMensajesDebug)
        {
            Debug.Log($"[BossController] Physics Material sin fricción aplicado a {colliders.Length} collider(s)");
        }
        
        // Configurar AudioSource
        if (audioSource == null && (sonidoAtaque != null || sonidoGolpe != null || sonidoMuerte != null))
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
                    Debug.Log($"[BossController] Audio Mixer Group asignado: {audioMixerGroup.name}");
                }
            }
        }
        
        // Buscar al jugador
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            jugador = playerObj.transform;
        }
        else
        {
            Debug.LogError("[BossController] No se encontró al jugador con el tag 'Player'");
        }
        
        // Configurar dirección inicial del sprite basándose en la posición del jugador
        if (jugador != null)
        {
            // Determinar hacia dónde debe mirar basándose en la posición del jugador
            bool jugadorALaDerecha = jugador.position.x > transform.position.x;
            mirandoDerecha = jugadorALaDerecha;
            
            if (mostrarMensajesDebug)
            {
                Debug.Log($"[BossController] Jugador detectado a la {(jugadorALaDerecha ? "DERECHA" : "IZQUIERDA")}. Mirando derecha: {mirandoDerecha}");
            }
        }
        else
        {
            // Si no hay jugador, usar la configuración por defecto
            mirandoDerecha = spriteMiraDerechaPorDefecto;
            
            if (mostrarMensajesDebug)
            {
                Debug.LogWarning("[BossController] No se encontró jugador. Usando dirección por defecto.");
            }
        }
        
        // Aplicar la dirección al sprite
        if (spriteRenderer != null)
        {
            if (spriteMiraDerechaPorDefecto)
            {
                spriteRenderer.flipX = !mirandoDerecha;
            }
            else
            {
                spriteRenderer.flipX = mirandoDerecha;
            }
        }
        
        if (mostrarMensajesDebug)
        {
            Debug.Log($"[BossController] Jefe iniciado. Vida: {vidaActual}, Tiempo de vida: {tiempoDeVida}s");
        }
    }

    void Update()
    {
        if (estaMuerto) return;
        
        // Actualizar tiempo de vida
        tiempoVivo += Time.deltaTime;
        
        // Morir automáticamente después de 3 minutos
        if (tiempoVivo >= tiempoDeVida)
        {
            if (mostrarMensajesDebug)
            {
                Debug.Log("[BossController] Tiempo de vida agotado. Muriendo...");
            }
            Morir();
            return;
        }
        
        // Si no hay jugador, no hacer nada
        if (jugador == null) return;
        
        // Actualizar estado según distancia
        ActualizarEstado();
        
        // Ejecutar comportamiento según el estado
        switch (estadoActual)
        {
            case EstadoJefe.Idle:
                ComportamientoIdle();
                break;
            case EstadoJefe.Persiguiendo:
                ComportamientoPersiguiendo();
                break;
            case EstadoJefe.Atacando:
                ComportamientoAtacando();
                break;
        }
        
        // Actualizar animaciones
        ActualizarAnimaciones();
    }

    void ActualizarEstado()
    {
        // No cambiar estado si está muerto, recibiendo golpe, o ATACANDO
        if (estaMuerto || estadoActual == EstadoJefe.Recibiendo_Golpe || estaAtacando)
            return;
        
        float distanciaAlJugador = Vector2.Distance(transform.position, jugador.position);
        
        // Verificar si puede atacar
        if (distanciaAlJugador <= rangoAtaque && Time.time >= tiempoUltimoAtaque + cooldownAtaque)
        {
            estadoActual = EstadoJefe.Atacando;
        }
        // Verificar si debe perseguir
        else if (distanciaAlJugador <= rangoDeteccion && distanciaAlJugador > distanciaMinima)
        {
            estadoActual = EstadoJefe.Persiguiendo;
        }
        // Si está muy cerca pero no puede atacar, quedarse quieto
        else if (distanciaAlJugador <= distanciaMinima)
        {
            estadoActual = EstadoJefe.Idle;
        }
        // Si está lejos, idle
        else
        {
            estadoActual = EstadoJefe.Idle;
        }
    }

    void ComportamientoIdle()
    {
        // Detener movimiento
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
        
        // Mirar al jugador
        if (jugador != null)
        {
            ActualizarDireccion();
        }
    }

    void ComportamientoPersiguiendo()
    {
        if (jugador == null || rb == null) return;
        
        float distanciaActual = Vector2.Distance(transform.position, jugador.position);
        
        // Si está muy cerca, dejar de moverse para no atascarse
        if (distanciaActual <= distanciaMinima)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }
        
        // Calcular dirección hacia el jugador
        Vector2 direccion = (jugador.position - transform.position).normalized;
        
        // Usar velocidad directa en lugar de AddForce para control más preciso
        rb.linearVelocity = new Vector2(direccion.x * velocidadMovimiento, rb.linearVelocity.y);
        
        // Actualizar dirección del sprite
        ActualizarDireccion();
    }

    void ComportamientoAtacando()
    {
        // Solo ejecutar el ataque UNA VEZ por ciclo de ataque
        if (!ataqueEjecutado)
        {
            // Activar flag de ataque para bloquear cambios de estado
            estaAtacando = true;
            ataqueEjecutado = true;
            
            // Detener movimiento durante ataque
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
            
            // Mirar al jugador
            ActualizarDireccion();
            
            // Ejecutar ataque (SOLO UNA VEZ)
            Atacar();
            
            // Registrar tiempo del ataque
            tiempoUltimoAtaque = Time.time;
            
            // Iniciar corrutina para desbloquear después de la animación
            StartCoroutine(FinalizarAtaque());
        }
        // IMPORTANTE: Mantener el estado bloqueado durante el ataque
        // NO resetear ataqueEjecutado aquí, eso solo debe ocurrir en FinalizarAtaque()
    }
    
    /// <summary>
    /// Corrutina para esperar a que termine la animación de ataque
    /// </summary>
    System.Collections.IEnumerator FinalizarAtaque()
    {
        // Esperar la duración de la animación de ataque
        // Ajusta este valor según la duración de tu animación
        float duracionAnimacionAtaque = 0.6f; // Cambia esto según tu animación
        
        yield return new WaitForSeconds(duracionAnimacionAtaque);
        
        // Desbloquear estado y volver a Idle
        estaAtacando = false;
        ataqueEjecutado = false; // Resetear para permitir el próximo ataque
        estadoActual = EstadoJefe.Idle;
        
        if (mostrarMensajesDebug)
        {
            Debug.Log("[BossController] Ataque finalizado, volviendo a Idle");
        }
    }

    void Atacar()
    {
        if (animator != null)
        {
            animator.SetTrigger(PARAM_ATTACK);
        }
        
        // Reproducir sonido de ataque (PlayOneShot siempre reproduce el sonido completo)
        if (sonidoAtaque != null && audioSource != null)
        {
            audioSource.PlayOneShot(sonidoAtaque, volumen);
        }
        
        if (mostrarMensajesDebug)
        {
            Debug.Log("[BossController] Atacando!");
        }
        
        // Causar daño al jugador si está en rango
        if (jugador != null && danio > 0f)
        {
            float distanciaAlJugador = Vector2.Distance(transform.position, jugador.position);
            
            if (distanciaAlJugador <= rangoAtaque)
            {
                PlayerMovement player = jugador.GetComponent<PlayerMovement>();
                if (player != null && !player.estaMuerto)
                {
                    player.RecibirDanio(danio, transform.position);
                    
                    if (mostrarMensajesDebug)
                    {
                        Debug.Log($"[BossController] ¡Golpeaste al jugador! Daño: {danio}");
                    }
                }
            }
        }
    }

    void ActualizarDireccion()
    {
        if (jugador == null || spriteRenderer == null) return;
        
        // Determinar si el jugador está a la derecha
        bool jugadorALaDerecha = jugador.position.x > transform.position.x;
        
        // Determinar si debe mirar a la derecha
        bool debeVerDerecha = jugadorALaDerecha;
        
        // Solo actualizar si cambió la dirección
        if (debeVerDerecha != mirandoDerecha)
        {
            mirandoDerecha = debeVerDerecha;
            Voltear();
        }
    }
    
    void Voltear()
    {
        if (spriteRenderer == null) return;
        
        // Si el sprite mira a la derecha por defecto:
        // - flipX = false cuando mira derecha
        // - flipX = true cuando mira izquierda
        // Si el sprite mira a la izquierda por defecto, invertir
        if (spriteMiraDerechaPorDefecto)
        {
            spriteRenderer.flipX = !mirandoDerecha;
        }
        else
        {
            spriteRenderer.flipX = mirandoDerecha;
        }
        
        if (mostrarMensajesDebug)
        {
            Debug.Log($"[BossController] Volteando. Mirando derecha: {mirandoDerecha}, FlipX: {spriteRenderer.flipX}");
        }
    }

    void ActualizarAnimaciones()
    {
        if (animator == null) return;
        
        // isWalking: true SOLO si está persiguiendo Y NO está atacando
        // Esto asegura que la animación de caminar se detenga en otros estados
        bool estaMoviendose = estadoActual == EstadoJefe.Persiguiendo 
                            && !estaAtacando 
                            && rb != null 
                            && Mathf.Abs(rb.linearVelocity.x) > 0.1f;
        animator.SetBool(PARAM_IS_WALKING, estaMoviendose);
    }

    /// <summary>
    /// Método público para que el jefe reciba daño
    /// </summary>
    public void RecibirDanio(float cantidad)
    {
        if (estaMuerto) return;
        
        vidaActual -= cantidad;
        vidaActual = Mathf.Clamp(vidaActual, 0f, vidaMaxima);
        
        // Reproducir sonido de golpe (PlayOneShot permite múltiples reproducciones simultáneas si es necesario)
        if (sonidoGolpe != null && audioSource != null)
        {
            audioSource.PlayOneShot(sonidoGolpe, volumen);
        }
        
        // Trigger de animación de golpe
        if (animator != null)
        {
            animator.SetTrigger(PARAM_HIT);
        }
        
        if (mostrarMensajesDebug)
        {
            Debug.Log($"[BossController] Recibió {cantidad} de daño. Vida restante: {vidaActual}/{vidaMaxima}");
        }
        
        // Verificar muerte
        if (vidaActual <= 0f)
        {
            Morir();
        }
    }

    void Morir()
    {
        if (estaMuerto) return;
        
        estaMuerto = true;
        estadoActual = EstadoJefe.Muerto;
        
        // Detener movimiento
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
        }
        
        // IMPORTANTE: Desactivar Canvas ANTES de desactivar colliders para evitar errores de UI
        Canvas[] canvases = GetComponentsInChildren<Canvas>(true); // true para incluir inactivos
        foreach (Canvas canvas in canvases)
        {
            canvas.gameObject.SetActive(false); // Desactivar el GameObject completo, no solo el componente
        }
        
        // Desactivar todos los hijos EXCEPTO el propio jefe para evitar cualquier script activo
        foreach (Transform hijo in transform)
        {
            hijo.gameObject.SetActive(false);
        }
        
        // Desactivar colliders para que no estorben
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }
        
        // Reproducir sonido de muerte
        if (sonidoMuerte != null && audioSource != null)
        {
            audioSource.PlayOneShot(sonidoMuerte, volumen);
        }
        
        // Activar animación de muerte
        if (animator != null)
        {
            animator.SetBool(PARAM_IS_DEAD, true);
        }
        
        if (mostrarMensajesDebug)
        {
            Debug.Log($"[BossController] ¡Jefe derrotado! Tiempo vivo: {tiempoVivo:F1}s, Vida restante: {vidaActual}");
        }
        
        // Iniciar efecto de "derretirse" (bajar mientras dura la animación)
        StartCoroutine(EfectoMuerte());
    }
    
    /// <summary>
    /// Efecto de derretirse hacia abajo durante la animación de muerte
    /// </summary>
    System.Collections.IEnumerator EfectoMuerte()
    {
        float duracionAnimacion = 1f; // Duración de la animación de muerte
        float velocidadCaida = 2f; // Velocidad a la que "se derrite" hacia abajo
        float tiempoTranscurrido = 0f;
        
        Vector3 posicionInicial = transform.position;
        
        // Bajar gradualmente durante la animación
        while (tiempoTranscurrido < duracionAnimacion)
        {
            tiempoTranscurrido += Time.deltaTime;
            
            // Mover hacia abajo
            transform.position += Vector3.down * velocidadCaida * Time.deltaTime;
            
            yield return null;
        }
        
        // Opcional: Desvanecer el sprite
        if (spriteRenderer != null)
        {
            float fadeTime = 0.5f;
            float fadeElapsed = 0f;
            Color colorInicial = spriteRenderer.color;
            
            while (fadeElapsed < fadeTime)
            {
                fadeElapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, fadeElapsed / fadeTime);
                spriteRenderer.color = new Color(colorInicial.r, colorInicial.g, colorInicial.b, alpha);
                yield return null;
            }
        }
        
        if (mostrarMensajesDebug)
        {
            Debug.Log("[BossController] Animación de muerte completada");
        }
        
        // NUEVO: Esperar antes de mostrar el diálogo de victoria
        yield return new WaitForSeconds(tiempoAntesDeDialogo);
        
        // Mostrar diálogo de victoria si está configurado
        if (victoryDialog != null)
        {
            if (mostrarMensajesDebug)
            {
                Debug.Log("[BossController] Mostrando diálogo de victoria");
            }
            
            // Activar el diálogo de victoria
            MostrarDialogoVictoria();
        }
        else
        {
            // Si no hay diálogo configurado, ir directamente al menú
            if (mostrarMensajesDebug)
            {
                Debug.LogWarning("[BossController] No hay diálogo de victoria configurado. Yendo al menú...");
            }
            
            yield return new WaitForSeconds(2f);
            IrAlMenu();
        }
        
        // No destruir el objeto aún, esperar a que termine el diálogo
        // Destroy(gameObject, 0.5f); // REMOVIDO: Ya no destruimos automáticamente
    }
    
    /// <summary>
    /// Muestra el diálogo de victoria y configura la escena
    /// </summary>
    void MostrarDialogoVictoria()
    {
        if (victoryDialog == null) return;
        
        // Buscar al jugador para posicionarlo cerca del diálogo si es necesario
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player != null)
        {
            // Descongelar al jugador para que pueda interactuar con el diálogo
            PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.enabled = true;
            }
            
            // Detener cualquier movimiento residual
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector2.zero;
            }
        }
        
        // ARREGLO: Activar el GameObject del diálogo de victoria
        victoryDialog.gameObject.SetActive(true);
        
        // ARREGLO: Llamar al método que inicia el diálogo
        VictoryDialog victoryDialogScript = victoryDialog.GetComponent<VictoryDialog>();
        if (victoryDialogScript != null)
        {
            victoryDialogScript.ShowVictoryDialog();
            
            if (mostrarMensajesDebug)
            {
                Debug.Log("[BossController] ShowVictoryDialog() llamado correctamente");
            }
        }
        else
        {
            // Si es un Dialog_2 normal, activarlo de forma alternativa
            if (mostrarMensajesDebug)
            {
                Debug.LogWarning("[BossController] No es un VictoryDialog, intentando activar panel manualmente");
            }
            
            // Activar el panel manualmente si existe
            if (victoryDialog.dialogPanelReference != null)
            {
                victoryDialog.dialogPanelReference.SetActive(true);
            }
        }
        
        // Suscribirse al evento de finalización del diálogo
        StartCoroutine(EsperarFinDelDialogo());
    }
    
    /// <summary>
    /// Espera a que el diálogo termine y luego va al menú
    /// </summary>
    System.Collections.IEnumerator EsperarFinDelDialogo()
    {
        if (victoryDialog == null) yield break;
        
        // Esperar un frame para asegurarse de que el diálogo esté inicializado
        yield return null;
        
        // Esperar hasta que el diálogo termine
        // Verificamos si el panel del diálogo está activo
        while (victoryDialog.dialogPanelReference != null && 
               victoryDialog.dialogPanelReference.activeSelf)
        {
            yield return new WaitForSeconds(0.5f); // Verificar cada medio segundo
        }
        
        if (mostrarMensajesDebug)
        {
            Debug.Log("[BossController] Diálogo de victoria terminado. Yendo al menú...");
        }
        
        // Esperar un momento antes de cambiar de escena
        yield return new WaitForSeconds(2f);
        
        // Ir al menú principal
        IrAlMenu();
    }
    
    /// <summary>
    /// Carga la escena del menú principal
    /// </summary>
    void IrAlMenu()
    {
        if (mostrarMensajesDebug)
        {
            Debug.Log($"[BossController] Cargando escena: {nombreEscenaMenu}");
        }
        
        // Cargar la escena del menú
        SceneManager.LoadScene(nombreEscenaMenu);
    }

    /// <summary>
    /// Método opcional para resetear el jefe
    /// </summary>
    public void Resetear()
    {
        vidaActual = vidaMaxima;
        tiempoVivo = 0f;
        estaMuerto = false;
        estaAtacando = false; // Resetear flag de ataque
        ataqueEjecutado = false; // Resetear flag de ejecución de ataque
        estadoActual = EstadoJefe.Idle;
        
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = Vector2.zero;
        }
        
        if (animator != null)
        {
            animator.SetBool(PARAM_IS_DEAD, false);
            animator.SetBool(PARAM_IS_WALKING, false);
        }
        
        if (mostrarMensajesDebug)
        {
            Debug.Log("[BossController] Jefe reseteado");
        }
    }

    // Visualización en el editor
    void OnDrawGizmos()
    {
        if (!mostrarGizmos) return;
        
        // Rango de detección (amarillo)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);
        
        // Rango de ataque (rojo)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoAtaque);
        
        // Distancia mínima (verde)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, distanciaMinima);
    }

    void OnDrawGizmosSelected()
    {
        if (!mostrarGizmos) return;
        
        // Línea hacia el jugador
        if (jugador != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, jugador.position);
        }
    }
}
