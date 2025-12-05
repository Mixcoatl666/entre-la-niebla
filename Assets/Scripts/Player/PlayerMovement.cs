using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 7f;
    public float acceleration = 50f;
    public float deceleration = 50f;
    public float velocityPower = 0.9f;

    [Header("Jump")]
    public float jumpForce = 15f;
    public float jumpCutMultiplier = 0.5f;
    public float fallGravityMultiplier = 2.5f;
    public float maxFallSpeed = 25f;

    [Header("Ground Check")]
    public float groundCheckDistance = 0.2f;
    public float groundCheckWidth = 0.8f;
    public LayerMask groundLayer;

    [Header("Coyote Time & Jump Buffer")]
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.15f;

    [Header("Animation")]
    public Animator animator;

    [Header("Combat System")]
    [Tooltip("¿El jugador tiene la espada?")]
    public bool HasSword = false;
    
    [HideInInspector]
    public bool estaAtacando = false; // Para que PlayerCombat pueda controlarlo
    
    [Header("Mobile Input (Auto-configurado)")]
    [Tooltip("Input horizontal desde controles móviles")]
    [HideInInspector]
    public float mobileHorizontalInput = 0f;
    
    [Tooltip("¿Está usando controles móviles?")]
    [HideInInspector]
    public bool usingMobileControls = false;
    
    [Tooltip("Salto solicitado desde controles móviles")]
    [HideInInspector]
    public bool mobileJumpRequested = false;

    [Header("Health System")]
    public float vidaMaxima = 100f;
    public float vida = 100f;
    public float danioDeEnemigos = 10f; // Daño que causan los enemigos
    public float tiempoInvencibilidad = 1.5f; // Segundos de invencibilidad después de recibir daño
    public bool estaMuerto = false;

    [Header("Knockback")]
    public float knockbackFuerzaX = 8f; // Fuerza horizontal del retroceso
    public float knockbackFuerzaY = 6f; // Fuerza vertical del retroceso
    public float knockbackDuracion = 0.3f; // Duración del knockback (sin control del jugador)
    
    [Header("Health Sounds")]
    public AudioSource healthAudioSource; // AudioSource para sonidos de salud
    public AudioClip damageSound; // Sonido al recibir daño
    public AudioClip healSound; // Sonido al curarse
    public AudioClip deathSound; // Sonido al morir
    [Range(0f, 1f)]
    public float healthSoundVolume = 0.8f;

    [Header("Footstep Sounds")]
    public AudioSource footstepAudioSource; // AudioSource dedicado para pasos (conecta al Audio Mixer)
    public AudioClip grassFootstep; // Sonido de pasto
    public AudioClip dirtFootstep; // Sonido de tierra
    public AudioClip stoneFootstep; // Sonido de piedra
    public AudioClip woodFootstep; // Sonido de madera
    public AudioClip defaultFootstep; // Sonido por defecto
    [Range(0.1f, 2f)]
    public float footstepInterval = 0.4f; // Intervalo entre pasos (en segundos)
    [Range(0f, 1f)]
    public float footstepVolume = 0.7f; // Volumen de los pasos

    private Rigidbody2D rb;
    private Collider2D col;
    private bool isGrounded;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float horizontalInput;
    private bool jumpPressed;
    private bool jumpReleased;
    private float footstepTimer = 0f;
    private string currentGroundTag = ""; // Tag del suelo actual
    private bool esInvencible = false;
    private float tiempoInvencibleRestante = 0f;
    private bool enKnockback = false;
    private float knockbackTiempoRestante = 0f;
    private SpriteRenderer spriteRenderer;
    private Vector3 posicionInicial;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Guardar posición inicial
        posicionInicial = transform.position;
        
        // Inicializar vida
        vida = vidaMaxima;
        estaMuerto = false;
        
        // Configurar AudioSource para pasos
        if (footstepAudioSource == null)
        {
            // Si no se asignó uno, buscar o crear uno
            footstepAudioSource = GetComponent<AudioSource>();
            if (footstepAudioSource == null)
            {
                footstepAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Configurar AudioSource para salud
        if (healthAudioSource == null)
        {
            // Crear un segundo AudioSource para sonidos de salud
            healthAudioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Configurar el AudioSource de pasos
        footstepAudioSource.volume = footstepVolume;
        footstepAudioSource.playOnAwake = false;
        
        // Configurar el AudioSource de salud
        healthAudioSource.volume = healthSoundVolume;
        healthAudioSource.playOnAwake = false;
        
        // Configurar interpolación del Rigidbody2D para movimiento más suave
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        
        // IMPORTANTE: Configurar Collision Detection a Continuous para evitar atascamiento
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        
        // Congelar rotación para evitar que el jugador se voltee
        rb.freezeRotation = true;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        // Crear y aplicar Physics Material 2D sin fricción
        PhysicsMaterial2D sinFriccion = new PhysicsMaterial2D("PlayerMaterial");
        sinFriccion.friction = 0f;
        sinFriccion.bounciness = 0f;
        
        // Aplicar material al collider del jugador
        if (col != null)
        {
            col.sharedMaterial = sinFriccion;
            Debug.Log("✅ Physics Material sin fricción aplicado al jugador");
        }
        
        // Obtener el Animator si no está asignado
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    void Update()
    {
        // No permitir movimiento si está muerto
        if (estaMuerto)
        {
            horizontalInput = 0f;
            mobileHorizontalInput = 0f;
            return;
        }

        // Actualizar knockback
        if (enKnockback)
        {
            knockbackTiempoRestante -= Time.deltaTime;
            if (knockbackTiempoRestante <= 0f)
            {
                enKnockback = false;
            }
            // Durante knockback, no permitir input del jugador
            horizontalInput = 0f;
            mobileHorizontalInput = 0f;
        }
        else
        {
            // PRIORIDAD: Si hay input móvil, usarlo; si no, usar teclado
            if (usingMobileControls && Mathf.Abs(mobileHorizontalInput) > 0.01f)
            {
                // Usar input móvil (si no está atacando)
                if (!estaAtacando)
                {
                    horizontalInput = mobileHorizontalInput;
                }
                else
                {
                    horizontalInput = 0f;
                }
            }
            else
            {
                // Input del nuevo Input System (solo si no está en knockback NI atacando)
                if (Keyboard.current != null && !estaAtacando)
                {
                    horizontalInput = 0f;
                    if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                    {
                        horizontalInput = -1f;
                    }
                    else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                    {
                        horizontalInput = 1f;
                    }

                    jumpPressed = Keyboard.current.spaceKey.wasPressedThisFrame;
                    jumpReleased = Keyboard.current.spaceKey.wasReleasedThisFrame;
                }
                else if (estaAtacando)
                {
                    // Durante ataque, no permitir input
                    horizontalInput = 0f;
                }
            }
            
            // Verificar salto móvil
            if (mobileJumpRequested)
            {
                jumpPressed = true;
                mobileJumpRequested = false; // Resetear después de leer
            }
        }

        // Actualizar invencibilidad
        if (esInvencible)
        {
            tiempoInvencibleRestante -= Time.deltaTime;
            if (tiempoInvencibleRestante <= 0f)
            {
                esInvencible = false;
                // Restaurar color normal
                if (spriteRenderer != null)
                {
                    Color color = spriteRenderer.color;
                    color.a = 1f;
                    spriteRenderer.color = color;
                }
            }
            else
            {
                // Efecto de parpadeo durante invencibilidad
                if (spriteRenderer != null)
                {
                    float alpha = Mathf.PingPong(Time.time * 10f, 1f);
                    Color color = spriteRenderer.color;
                    color.a = alpha;
                    spriteRenderer.color = color;
                }
            }
        }

        // Ground check mejorado
        CheckGround();

        // Coyote time
        if (isGrounded)
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.deltaTime;

        // Jump buffer (no permitir saltar durante knockback)
        if (jumpPressed && !enKnockback)
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        // Jump
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f && !enKnockback)
        {
            Jump();
            jumpBufferCounter = 0f;
        }

        // Jump cut (soltar el botón de salto para caer más rápido)
        if (jumpReleased && rb.linearVelocity.y > 0f && !enKnockback)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
            coyoteTimeCounter = 0f;
        }

        // Flip del sprite (no cambiar durante knockback para mantener dirección del golpe)
        if (!enKnockback)
        {
            if (horizontalInput > 0f)
                transform.localScale = new Vector3(0.3f, 0.3f, 1f);
            else if (horizontalInput < 0f)
                transform.localScale = new Vector3(-0.3f, 0.3f, 1f);
        }

        // Actualizar animaciones
        UpdateAnimations();

        // Reproducir sonidos de pasos
        HandleFootsteps();
    }

    void FixedUpdate()
    {
        if (estaMuerto) return;

        // No aplicar movimiento horizontal si está en knockback O atacando
        if (!enKnockback && !estaAtacando)
        {
            // Movimiento horizontal suave
            float targetSpeed = horizontalInput * moveSpeed;
            float speedDif = targetSpeed - rb.linearVelocity.x;
            float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
            float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velocityPower) * Mathf.Sign(speedDif);
            
            rb.AddForce(movement * Vector2.right);
        }

        // Gravedad aumentada al caer
        if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = fallGravityMultiplier;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -maxFallSpeed));
        }
        else
        {
            rb.gravityScale = 1f;
        }
    }

    // Método simplificado para recibir daño
    public void RecibirDanio(float cantidad)
    {
        // No recibir daño si está muerto o es invencible
        if (estaMuerto || esInvencible) return;

        vida -= cantidad;
        vida = Mathf.Clamp(vida, 0f, vidaMaxima);

        // Reproducir sonido de daño
        if (damageSound != null && healthAudioSource != null)
        {
            healthAudioSource.PlayOneShot(damageSound, healthSoundVolume);
        }

        // Activar invencibilidad temporal
        esInvencible = true;
        tiempoInvencibleRestante = tiempoInvencibilidad;

        Debug.Log("Jugador recibió " + cantidad + " de daño. Vida restante: " + vida);

        // Verificar muerte
        if (vida <= 0f)
        {
            Morir();
        }
    }

    // Método sobrecargado para recibir daño con knockback desde una posición
    public void RecibirDanio(float cantidad, Vector2 posicionEnemigo)
    {
        // No recibir daño si está muerto o es invencible
        if (estaMuerto || esInvencible) return;

        vida -= cantidad;
        vida = Mathf.Clamp(vida, 0f, vidaMaxima);

        // Reproducir sonido de daño
        if (damageSound != null && healthAudioSource != null)
        {
            healthAudioSource.PlayOneShot(damageSound, healthSoundVolume);
        }

        // Aplicar knockback
        AplicarKnockback(posicionEnemigo);

        // Activar invencibilidad temporal
        esInvencible = true;
        tiempoInvencibleRestante = tiempoInvencibilidad;

        Debug.Log("Jugador recibió " + cantidad + " de daño. Vida restante: " + vida);

        // Verificar muerte
        if (vida <= 0f)
        {
            Morir();
        }
    }

    void AplicarKnockback(Vector2 posicionOrigen)
    {
        // Activar estado de knockback
        enKnockback = true;
        knockbackTiempoRestante = knockbackDuracion;

        // Calcular dirección del knockback (alejarse del origen del daño)
        float direccion = Mathf.Sign(transform.position.x - posicionOrigen.x);
        
        // Si la dirección es 0 (están en la misma posición X), usar la escala actual
        if (direccion == 0f)
        {
            direccion = Mathf.Sign(transform.localScale.x);
        }

        // Aplicar fuerza de knockback
        Vector2 fuerzaKnockback = new Vector2(direccion * knockbackFuerzaX, knockbackFuerzaY);
        
        // Resetear velocidad actual y aplicar knockback
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(fuerzaKnockback, ForceMode2D.Impulse);

        Debug.Log("Knockback aplicado con dirección: " + direccion);
    }

    // Método para curar
    public void Curar(float cantidad)
    {
        if (estaMuerto) return;

        float vidaAntes = vida;
        vida += cantidad;
        vida = Mathf.Clamp(vida, 0f, vidaMaxima);

        // Solo reproducir sonido si realmente se curó
        if (vida > vidaAntes)
        {
            if (healSound != null && healthAudioSource != null)
            {
                healthAudioSource.PlayOneShot(healSound, healthSoundVolume);
            }

            Debug.Log("Jugador curado " + cantidad + " de vida. Vida actual: " + vida);
        }
    }

    // Método simplificado para morir
    void Morir()
    {
        if (estaMuerto) return;

        estaMuerto = true;
        vida = 0f;
        
        // IMPORTANTE: Cancelar knockback activo antes de morir
        enKnockback = false;
        knockbackTiempoRestante = 0f;

        // Reproducir sonido de muerte
        if (deathSound != null && healthAudioSource != null)
        {
            healthAudioSource.PlayOneShot(deathSound, healthSoundVolume);
        }

        Debug.Log("Jugador ha muerto!");
        
        // Resetear animaciones antes de desaparecer
        if (animator != null)
        {
            animator.SetBool("Run", false);
            animator.SetBool("IsHurt", false);
        }

        // Desaparecer el jugador
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        // Desactivar collider
        if (col != null)
        {
            col.enabled = false;
        }

        // Detener movimiento
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // Mostrar menú de Game Over
        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.MostrarGameOver();
        }
        else
        {
            // Si no hay GameOverManager, usar el sistema antiguo (reaparecer automático)
            Debug.LogWarning("No se encontró GameOverManager. Usando reaparición automática.");
            Invoke("Reaparecer", 2f);
        }
    }

    void Reaparecer()
    {
        // Restaurar vida
        vida = vidaMaxima;
        estaMuerto = false;
        
        // IMPORTANTE: Resetear todos los estados de knockback
        enKnockback = false;
        knockbackTiempoRestante = 0f;
        
        // Resetear estados de invencibilidad también
        esInvencible = false;
        tiempoInvencibleRestante = 0f;

        // Reaparecer el jugador
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }

        // Reactivar collider
        if (col != null)
        {
            col.enabled = true;
        }

        // Mover a posición inicial
        transform.position = posicionInicial;
        
        // Resetear velocidad del Rigidbody
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        
        // IMPORTANTE: Resetear COMPLETAMENTE el Animator
        if (animator != null)
        {
            // Desactivar y reactivar el Animator para forzar un reset completo
            animator.enabled = false;
            animator.enabled = true;
            
            // Resetear todos los parámetros
            animator.SetBool("Run", false);
            animator.SetBool("IsGrounded", true);
            animator.SetFloat("VelocityY", 0f);
            animator.SetBool("IsHurt", false);
            animator.SetBool("HasSword", HasSword);
            
            // Forzar al estado Idle
            animator.Play("Idle", 0, 0f); // Asegúrate de que "Idle" sea el nombre de tu animación idle
            
            // Forzar actualización
            animator.Update(0f);
            
            Debug.Log("✅ Animator reseteado completamente");
        }

        // Activar invencibilidad temporal DESPUÉS de resetear animator
        esInvencible = true;
        tiempoInvencibleRestante = tiempoInvencibilidad;

        Debug.Log("Jugador ha reaparecido!");
    }

    // Detectar colisión con enemigos
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Obtener posición del enemigo para el knockback
            Vector2 posicionEnemigo = collision.transform.position;
            RecibirDanio(danioDeEnemigos, posicionEnemigo);
        }
    }

    // Detectar colisión con enemigos (para triggers)
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Obtener posición del enemigo para el knockback
            Vector2 posicionEnemigo = other.transform.position;
            RecibirDanio(danioDeEnemigos, posicionEnemigo);
        }
    }

    void UpdateAnimations()
    {
        if (animator == null) return;

        // Run (no mostrar animación de correr durante knockback o ataque)
        bool isRunning = Mathf.Abs(horizontalInput) > 0.01f && !estaMuerto && !enKnockback && !estaAtacando;
        animator.SetBool("Run", isRunning);

        // Grounded & Vertical velocity
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetFloat("VelocityY", rb.linearVelocity.y);
        
        // Knockback/Hurt animation
        animator.SetBool("IsHurt", enKnockback);
        
        // HasSword para controlar animaciones con espada
        animator.SetBool("HasSword", HasSword);
    }

    void HandleFootsteps()
    {
        // Solo reproducir pasos si está en el suelo Y moviéndose Y no está muerto Y no está en knockback
        bool isMoving = Mathf.Abs(horizontalInput) > 0.01f && isGrounded && !estaMuerto && !enKnockback;

        if (isMoving)
        {
            footstepTimer -= Time.deltaTime;

            if (footstepTimer <= 0f)
            {
                PlayFootstepSound();
                footstepTimer = footstepInterval;
            }
        }
        else
        {
            footstepTimer = 0f;
            
            if (footstepAudioSource != null && footstepAudioSource.isPlaying)
            {
                footstepAudioSource.Stop();
            }
        }
    }

    void PlayFootstepSound()
    {
        AudioClip soundToPlay = GetFootstepSound();

        if (soundToPlay != null && footstepAudioSource != null)
        {
            footstepAudioSource.PlayOneShot(soundToPlay, footstepVolume);
        }
    }

    AudioClip GetFootstepSound()
    {
        switch (currentGroundTag)
        {
            case "Grass":
                return grassFootstep;
            case "Dirt":
                return dirtFootstep;
            case "Stone":
                return stoneFootstep;
            case "Wood":
                return woodFootstep;
            default:
                return defaultFootstep;
        }
    }

    void CheckGround()
    {
        if (col == null) return;

        Vector2 origin = new Vector2(col.bounds.center.x, col.bounds.min.y);
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, Vector2.down, groundCheckDistance, groundLayer);
        
        isGrounded = false;
        currentGroundTag = "";

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != col)
            {
                isGrounded = true;
                currentGroundTag = hit.collider.tag;
                break;
            }
        }

        if (!isGrounded)
        {
            float checkWidth = col.bounds.size.x * groundCheckWidth;
            Vector2 leftOrigin = new Vector2(col.bounds.center.x - checkWidth / 2, col.bounds.min.y);
            Vector2 rightOrigin = new Vector2(col.bounds.center.x + checkWidth / 2, col.bounds.min.y);
            
            RaycastHit2D leftHit = Physics2D.Raycast(leftOrigin, Vector2.down, groundCheckDistance, groundLayer);
            RaycastHit2D rightHit = Physics2D.Raycast(rightOrigin, Vector2.down, groundCheckDistance, groundLayer);
            
            if (leftHit.collider != null && leftHit.collider != col)
            {
                isGrounded = true;
                currentGroundTag = leftHit.collider.tag;
            }
            else if (rightHit.collider != null && rightHit.collider != col)
            {
                isGrounded = true;
                currentGroundTag = rightHit.collider.tag;
            }
        }
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        coyoteTimeCounter = 0f;
    }

    void OnDrawGizmosSelected()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            Vector2 origin = new Vector2(collider.bounds.center.x, collider.bounds.min.y);
            float checkWidth = collider.bounds.size.x * groundCheckWidth;
            Vector2 leftOrigin = new Vector2(collider.bounds.center.x - checkWidth / 2, collider.bounds.min.y);
            Vector2 rightOrigin = new Vector2(collider.bounds.center.x + checkWidth / 2, collider.bounds.min.y);
            
            Gizmos.color = Color.red;
            Gizmos.DrawLine(origin, origin + Vector2.down * groundCheckDistance);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(leftOrigin, leftOrigin + Vector2.down * groundCheckDistance);
            Gizmos.DrawLine(rightOrigin, rightOrigin + Vector2.down * groundCheckDistance);
        }
    }
    
    #region Métodos para Controles Móviles
    
    /// <summary>
    /// Método público para que PlayerMovementJoystick inyecte input horizontal
    /// </summary>
    public void SetMobileInput(float horizontal)
    {
        if (estaMuerto || enKnockback) return;
        
        mobileHorizontalInput = horizontal;
        usingMobileControls = true;
    }
    
    /// <summary>
    /// Método público para solicitar salto desde controles móviles
    /// </summary>
    public void MobileJump()
    {
        if (estaMuerto || enKnockback) return;
        
        mobileJumpRequested = true;
    }
    
    /// <summary>
    /// Resetear input móvil (llamar cuando el joystick vuelva a 0)
    /// </summary>
    public void ResetMobileInput()
    {
        mobileHorizontalInput = 0f;
        usingMobileControls = false;
    }
    
    #endregion
}
