using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Sistema de controles móviles para Android
/// Maneja el joystick y botones de UI (Jump, Interact, Attack)
/// Se integra con PlayerMovement y PlayerCombat existentes
/// </summary>
public class PlayerMovementJoystick : MonoBehaviour
{
    [Header("Referencias del Jugador")]
    [Tooltip("Referencia al script PlayerMovement")]
    public PlayerMovement playerMovement;
    
    [Tooltip("Referencia al script PlayerCombat (opcional, para ataque)")]
    public PlayerCombat playerCombat;
    
    [Header("Controles de UI")]
    [Tooltip("Joystick para movimiento (Fixed, Dynamic o Variable)")]
    public Joystick movementJoystick;
    
    [Tooltip("Botón de salto")]
    public Button jumpButton;
    
    [Tooltip("Botón de interacción (E/W)")]
    public Button interactButton;
    
    [Tooltip("Botón de ataque")]
    public Button attackButton;
    
    [Header("Configuración")]
    [Tooltip("Zona muerta del joystick (ignorar movimientos pequeños)")]
    [Range(0f, 0.5f)]
    public float joystickDeadzone = 0.1f;
    
    [Tooltip("¿Desactivar controles móviles en PC?")]
    public bool desactivarEnPC = true;
    
    [Header("Debug")]
    public bool mostrarDebug = false;
    
    // Estados privados
    private bool jumpButtonPressed = false;
    private bool jumpButtonReleased = false;
    private bool attackButtonPressed = false;
    private bool interactButtonPressed = false;
    
    // Flags para evitar múltiples llamadas
    private bool jumpWasPressed = false;
    private bool attackWasPressed = false;
    private bool interactWasPressed = false;

    void Start()
    {
        // Verificar si estamos en PC y desactivar si es necesario
        if (desactivarEnPC && !Application.isMobilePlatform)
        {
            DesactivarControlesMobiles();
            this.enabled = false;
            return;
        }
        
        // Buscar PlayerMovement si no está asignado
        if (playerMovement == null)
        {
            playerMovement = FindFirstObjectByType<PlayerMovement>();
            if (playerMovement == null)
            {
                Debug.LogError("[PlayerMovementJoystick] No se encontró PlayerMovement en la escena!");
            }
        }
        
        // Buscar PlayerCombat si no está asignado
        if (playerCombat == null)
        {
            playerCombat = FindFirstObjectByType<PlayerCombat>();
        }
        
        // Configurar eventos de botones
        ConfigurarBotones();
        
        if (mostrarDebug)
        {
            Debug.Log("[PlayerMovementJoystick] Sistema de controles móviles inicializado");
        }
    }

    void Update()
    {
        // Actualizar input del joystick
        ActualizarMovimiento();
        
        // Actualizar estados de botones
        ActualizarBotones();
    }
    
    /// <summary>
    /// Lee el input del joystick y lo aplica al PlayerMovement
    /// </summary>
    void ActualizarMovimiento()
    {
        if (movementJoystick == null || playerMovement == null) return;
        
        // Obtener input horizontal del joystick
        float horizontal = movementJoystick.Horizontal;
        
        // Aplicar zona muerta para evitar movimientos no deseados
        if (Mathf.Abs(horizontal) < joystickDeadzone)
        {
            horizontal = 0f;
        }
        
        // Inyectar el input al PlayerMovement
        // Nota: Necesitaremos modificar PlayerMovement para aceptar input externo
        InyectarInputMovimiento(horizontal);
        
        if (mostrarDebug && horizontal != 0f)
        {
            Debug.Log($"[Joystick] Horizontal: {horizontal:F2}");
        }
    }
    
    /// <summary>
    /// Actualiza los estados de los botones
    /// </summary>
    void ActualizarBotones()
    {
        // Salto
        if (jumpButtonPressed && !jumpWasPressed)
        {
            EjecutarSalto();
            jumpWasPressed = true;
        }
        else if (!jumpButtonPressed)
        {
            jumpWasPressed = false;
        }
        
        // Detección de soltar botón de salto (para Jump Cut)
        if (jumpButtonReleased)
        {
            jumpButtonReleased = false;
        }
        
        // Ataque
        if (attackButtonPressed && !attackWasPressed)
        {
            EjecutarAtaque();
            attackWasPressed = true;
        }
        else if (!attackButtonPressed)
        {
            attackWasPressed = false;
        }
        
        // Interacción
        if (interactButtonPressed && !interactWasPressed)
        {
            EjecutarInteraccion();
            interactWasPressed = true;
        }
        else if (!interactButtonPressed)
        {
            interactWasPressed = false;
        }
    }
    
    /// <summary>
    /// Configura los eventos de los botones de UI
    /// </summary>
    void ConfigurarBotones()
    {
        // Jump Button
        if (jumpButton != null)
        {
            // Configurar evento PointerDown y PointerUp para detectar press y release
            EventTrigger jumpTrigger = jumpButton.gameObject.GetComponent<EventTrigger>();
            if (jumpTrigger == null)
            {
                jumpTrigger = jumpButton.gameObject.AddComponent<EventTrigger>();
            }
            
            // PointerDown (cuando presiona)
            EventTrigger.Entry pointerDown = new EventTrigger.Entry();
            pointerDown.eventID = EventTriggerType.PointerDown;
            pointerDown.callback.AddListener((data) => { OnJumpButtonDown(); });
            jumpTrigger.triggers.Add(pointerDown);
            
            // PointerUp (cuando suelta)
            EventTrigger.Entry pointerUp = new EventTrigger.Entry();
            pointerUp.eventID = EventTriggerType.PointerUp;
            pointerUp.callback.AddListener((data) => { OnJumpButtonUp(); });
            jumpTrigger.triggers.Add(pointerUp);
            
            if (mostrarDebug)
            {
                Debug.Log("[PlayerMovementJoystick] Jump Button configurado");
            }
        }
        
        // Attack Button
        if (attackButton != null)
        {
            EventTrigger attackTrigger = attackButton.gameObject.GetComponent<EventTrigger>();
            if (attackTrigger == null)
            {
                attackTrigger = attackButton.gameObject.AddComponent<EventTrigger>();
            }
            
            EventTrigger.Entry attackDown = new EventTrigger.Entry();
            attackDown.eventID = EventTriggerType.PointerDown;
            attackDown.callback.AddListener((data) => { OnAttackButtonDown(); });
            attackTrigger.triggers.Add(attackDown);
            
            EventTrigger.Entry attackUp = new EventTrigger.Entry();
            attackUp.eventID = EventTriggerType.PointerUp;
            attackUp.callback.AddListener((data) => { OnAttackButtonUp(); });
            attackTrigger.triggers.Add(attackUp);
            
            if (mostrarDebug)
            {
                Debug.Log("[PlayerMovementJoystick] Attack Button configurado");
            }
        }
        
        // Interact Button
        if (interactButton != null)
        {
            EventTrigger interactTrigger = interactButton.gameObject.GetComponent<EventTrigger>();
            if (interactTrigger == null)
            {
                interactTrigger = interactButton.gameObject.AddComponent<EventTrigger>();
            }
            
            EventTrigger.Entry interactDown = new EventTrigger.Entry();
            interactDown.eventID = EventTriggerType.PointerDown;
            interactDown.callback.AddListener((data) => { OnInteractButtonDown(); });
            interactTrigger.triggers.Add(interactDown);
            
            EventTrigger.Entry interactUp = new EventTrigger.Entry();
            interactUp.eventID = EventTriggerType.PointerUp;
            interactUp.callback.AddListener((data) => { OnInteractButtonUp(); });
            interactTrigger.triggers.Add(interactUp);
            
            if (mostrarDebug)
            {
                Debug.Log("[PlayerMovementJoystick] Interact Button configurado");
            }
        }
    }
    
    #region Callbacks de Botones
    
    void OnJumpButtonDown()
    {
        jumpButtonPressed = true;
        if (mostrarDebug) Debug.Log("[PlayerMovementJoystick] Jump Button Pressed");
    }
    
    void OnJumpButtonUp()
    {
        jumpButtonPressed = false;
        jumpButtonReleased = true;
        if (mostrarDebug) Debug.Log("[PlayerMovementJoystick] Jump Button Released");
    }
    
    void OnAttackButtonDown()
    {
        attackButtonPressed = true;
        if (mostrarDebug) Debug.Log("[PlayerMovementJoystick] Attack Button Pressed");
    }
    
    void OnAttackButtonUp()
    {
        attackButtonPressed = false;
        if (mostrarDebug) Debug.Log("[PlayerMovementJoystick] Attack Button Released");
    }
    
    void OnInteractButtonDown()
    {
        interactButtonPressed = true;
        if (mostrarDebug) Debug.Log("[PlayerMovementJoystick] Interact Button Pressed");
    }
    
    void OnInteractButtonUp()
    {
        interactButtonPressed = false;
        if (mostrarDebug) Debug.Log("[PlayerMovementJoystick] Interact Button Released");
    }
    
    #endregion
    
    #region Acciones del Jugador
    
    /// <summary>
    /// Inyecta input de movimiento al PlayerMovement
    /// </summary>
    void InyectarInputMovimiento(float horizontal)
    {
        if (playerMovement == null) return;
        
        // Llamar directamente al método público SetMobileInput
        playerMovement.SetMobileInput(horizontal);
        
        if (mostrarDebug && horizontal != 0f)
        {
            Debug.Log($"[PlayerMovementJoystick] Input inyectado: {horizontal:F2}");
        }
    }
    
    /// <summary>
    /// Ejecuta el salto del jugador
    /// </summary>
    void EjecutarSalto()
    {
        if (playerMovement == null) return;
        
        // Enviar mensaje de salto
        playerMovement.SendMessage("MobileJump", SendMessageOptions.DontRequireReceiver);
        
        if (mostrarDebug)
        {
            Debug.Log("[PlayerMovementJoystick] Salto ejecutado");
        }
    }
    
    /// <summary>
    /// Ejecuta el ataque del jugador
    /// </summary>
    void EjecutarAtaque()
    {
        if (playerCombat == null) return;
        
        // Enviar mensaje de ataque
        playerCombat.SendMessage("MobileAttack", SendMessageOptions.DontRequireReceiver);
        
        if (mostrarDebug)
        {
            Debug.Log("[PlayerMovementJoystick] Ataque ejecutado");
        }
    }
    
    /// <summary>
    /// Ejecuta la interacción (E/W)
    /// Funciona con Dialog_2, ChangeScene y cualquier otro sistema de interacción
    /// </summary>
    void EjecutarInteraccion()
    {
        if (playerMovement == null) return;
        
        // MÉTODO 1: Buscar y activar Dialog_2 cercanos (para NPCs y diálogos)
        BuscarYActivarDialogos();
        
        // MÉTODO 2: Buscar y activar ChangeScene cercanos (para entrar a lugares)
        BuscarYActivarCambioEscena();
        
        // MÉTODO 3: Buscar objetos con tag "Interactable" (método genérico)
        BuscarObjetosInteractuables();
        
        if (mostrarDebug)
        {
            Debug.Log("[PlayerMovementJoystick] Interacción ejecutada");
        }
    }
    
    /// <summary>
    /// Busca y activa Dialog_2 cercanos (simula presionar E)
    /// </summary>
    void BuscarYActivarDialogos()
    {
        // Buscar todos los Dialog_2 en la escena
        Dialog_2[] dialogos = FindObjectsByType<Dialog_2>(FindObjectsSortMode.None);
        
        foreach (Dialog_2 dialogo in dialogos)
        {
            if (dialogo == null) continue;
            
            // Calcular distancia al diálogo
            float distancia = Vector2.Distance(playerMovement.transform.position, dialogo.transform.position);
            
            // Si está en rango de interacción (2 unidades por defecto)
            if (distancia < 2f)
            {
                // Usar reflexión para acceder a las variables privadas o métodos
                // O simplemente enviar el mensaje que Dialog_2 espera
                dialogo.SendMessage("OnMobileInteract", SendMessageOptions.DontRequireReceiver);
                
                if (mostrarDebug)
                {
                    Debug.Log($"[PlayerMovementJoystick] Interactuando con Dialog_2: {dialogo.gameObject.name}");
                }
                
                // Solo interactuar con el más cercano
                return;
            }
        }
    }
    
    /// <summary>
    /// Busca y activa ChangeScene cercanos (simula presionar W)
    /// </summary>
    void BuscarYActivarCambioEscena()
    {
        // Buscar todos los ChangeScene en la escena
        ChangeScene[] cambios = FindObjectsByType<ChangeScene>(FindObjectsSortMode.None);
        
        foreach (ChangeScene cambio in cambios)
        {
            if (cambio == null) continue;
            
            // Calcular distancia al trigger de cambio de escena
            float distancia = Vector2.Distance(playerMovement.transform.position, cambio.transform.position);
            
            // Si está en rango de interacción
            if (distancia < 2f)
            {
                // Enviar mensaje para activar cambio de escena
                cambio.SendMessage("OnMobileInteract", SendMessageOptions.DontRequireReceiver);
                
                if (mostrarDebug)
                {
                    Debug.Log($"[PlayerMovementJoystick] Activando ChangeScene: {cambio.gameObject.name}");
                }
                
                // Solo activar el más cercano
                return;
            }
        }
    }
    
    /// <summary>
    /// Método genérico para buscar objetos con tag "Interactable"
    /// </summary>
    void BuscarObjetosInteractuables()
    {
        try
        {
            GameObject[] interactuables = GameObject.FindGameObjectsWithTag("Interactable");
            foreach (GameObject obj in interactuables)
            {
                float distancia = Vector2.Distance(playerMovement.transform.position, obj.transform.position);
                if (distancia < 2f) // Rango de interacción
                {
                    obj.SendMessage("OnInteract", SendMessageOptions.DontRequireReceiver);
                    obj.SendMessage("OnMobileInteract", SendMessageOptions.DontRequireReceiver);
                    
                    if (mostrarDebug)
                    {
                        Debug.Log($"[PlayerMovementJoystick] Interactuando con objeto: {obj.name}");
                    }
                }
            }
        }
        catch (UnityException)
        {
            // El tag "Interactable" no existe, no hay problema
            // Los otros métodos de interacción ya funcionan
        }
    }
    
    #endregion
    
    /// <summary>
    /// Desactiva todos los controles móviles (útil para PC)
    /// </summary>
    void DesactivarControlesMobiles()
    {
        if (movementJoystick != null)
        {
            movementJoystick.gameObject.SetActive(false);
        }
        
        if (jumpButton != null)
        {
            jumpButton.gameObject.SetActive(false);
        }
        
        if (attackButton != null)
        {
            attackButton.gameObject.SetActive(false);
        }
        
        if (interactButton != null)
        {
            interactButton.gameObject.SetActive(false);
        }
        
        Debug.Log("[PlayerMovementJoystick] Controles móviles desactivados (plataforma PC)");
    }
    
    /// <summary>
    /// Método público para verificar si el joystick está siendo usado
    /// </summary>
    public bool IsUsingJoystick()
    {
        if (movementJoystick == null) return false;
        return Mathf.Abs(movementJoystick.Horizontal) > joystickDeadzone;
    }
    
    /// <summary>
    /// Obtener el input horizontal del joystick
    /// </summary>
    public float GetHorizontalInput()
    {
        if (movementJoystick == null) return 0f;
        
        float horizontal = movementJoystick.Horizontal;
        
        // Aplicar zona muerta
        if (Mathf.Abs(horizontal) < joystickDeadzone)
        {
            return 0f;
        }
        
        return horizontal;
    }
}
