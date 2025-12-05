using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class PausarJuego : MonoBehaviour
{
    public GameObject menuPausa;
    public bool juegoPausado = false;
    
    [Header("Configuración Automática")]
    [Tooltip("Si está marcado, buscará automáticamente el Panel si menuPausa no está asignado")]
    public bool buscarPanelAutomaticamente = true;
    public string nombreDelPanel = "Panel"; // Nombre del Panel a buscar
    
    [Header("Botón de Pausa Móvil (Opcional)")]
    [Tooltip("Botón visible en pantalla para pausar (útil para móviles)")]
    public Button botonPausaMovil;
    
    [Tooltip("¿Ocultar el botón de pausa en PC?")]
    public bool ocultarBotonEnPC = true;
    
    private CanvasGroup canvasGroup;

    private void Start()
    {
        // Si menuPausa no está asignado, intentar encontrarlo automáticamente
        if (menuPausa == null && buscarPanelAutomaticamente)
        {
            // Buscar en hijos si este script está en el Canvas
            Transform panelTransform = transform.Find(nombreDelPanel);
            if (panelTransform != null)
            {
                menuPausa = panelTransform.gameObject;
                Debug.Log("Panel encontrado automáticamente: " + menuPausa.name);
            }
            else
            {
                Debug.LogError("No se pudo encontrar el Panel automáticamente. Asegúrate de asignarlo manualmente o que se llame '" + nombreDelPanel + "'");
                return;
            }
        }
        
        // Verificar que existe un EventSystem con el módulo correcto para el nuevo Input System
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem == null)
        {
            Debug.LogWarning("No hay EventSystem en la escena. Creando uno compatible con Input System...");
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystem = eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<InputSystemUIInputModule>();
        }
        else
        {
            // Si existe pero tiene StandaloneInputModule, cambiarlo
            StandaloneInputModule oldModule = eventSystem.GetComponent<StandaloneInputModule>();
            if (oldModule != null)
            {
                Debug.LogWarning("EventSystem tiene StandaloneInputModule. Reemplazándolo con InputSystemUIInputModule...");
                DestroyImmediate(oldModule);
                eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            }
            
            // Verificar que tenga InputSystemUIInputModule
            if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
            {
                Debug.LogWarning("Agregando InputSystemUIInputModule al EventSystem...");
                eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            }
        }
        
        // Obtener o agregar CanvasGroup al panel del menú de pausa
        if (menuPausa != null)
        {
            canvasGroup = menuPausa.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                Debug.LogWarning("CanvasGroup no encontrado. Agregando uno al panel...");
                canvasGroup = menuPausa.AddComponent<CanvasGroup>();
            }
            
            // Inicializar el menú como oculto
            OcultarMenu();
        }
        
        // Configurar botón de pausa móvil
        ConfigurarBotonPausaMovil();
        
        Debug.Log("PausarJuego inicializado correctamente");
    }
    
    /// <summary>
    /// Configura el botón de pausa para móviles
    /// </summary>
    void ConfigurarBotonPausaMovil()
    {
        if (botonPausaMovil == null) return;
        
        // Asignar evento onClick si no está configurado
        botonPausaMovil.onClick.RemoveAllListeners(); // Limpiar listeners previos
        botonPausaMovil.onClick.AddListener(TogglePausa);
        
        // Ocultar en PC si está configurado
        if (ocultarBotonEnPC && !Application.isMobilePlatform)
        {
            botonPausaMovil.gameObject.SetActive(false);
            Debug.Log("[PausarJuego] Botón de pausa oculto en PC");
        }
        else
        {
            botonPausaMovil.gameObject.SetActive(true);
            Debug.Log("[PausarJuego] Botón de pausa visible");
        }
    }
    
    /// <summary>
    /// Alterna entre pausar y reanudar (útil para el botón)
    /// </summary>
    public void TogglePausa()
    {
        if (juegoPausado)
        {
            Reanudar();
        }
        else
        {
            Pausar();
        }
    }

    private void Update()
    {
        // Pausar/Reanudar con ESC (solo en PC donde existe teclado)
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePausa();
        }
    }

    public void Reanudar()
    {
        Debug.Log("=== REANUDAR LLAMADO ===");
        OcultarMenu();
        Time.timeScale = 1f;
        juegoPausado = false;
        
        // Si el botón de pausa existe, asegurarse de que esté visible
        if (botonPausaMovil != null && !ocultarBotonEnPC)
        {
            botonPausaMovil.gameObject.SetActive(true);
        }
    }

    public void Pausar()
    {
        Debug.Log("=== PAUSAR LLAMADO ===");
        MostrarMenu();
        Time.timeScale = 0f;
        juegoPausado = true;
        
        // Asegurar que el cursor esté visible y desbloqueado
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void SalirAlMenuInicial()
    {
        Debug.Log("=== SALIR AL MENU INICIAL LLAMADO ===");
        Time.timeScale = 1f;
        juegoPausado = false;
        
        // Asegurar que el cursor esté visible en el menú
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        SceneManager.LoadScene("MenuInicial");
    }

    private void MostrarMenu()
    {
        if (menuPausa != null)
        {
            menuPausa.SetActive(true);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
        }
    }

    private void OcultarMenu()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
