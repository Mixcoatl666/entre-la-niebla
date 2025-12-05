using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    [Header("Referencias del UI")]
    [Tooltip("Panel de Game Over (debe tener CanvasGroup)")]
    public GameObject panelGameOver;
    
    [Tooltip("Panel de Pausa (se ocultará durante Game Over)")]
    public GameObject panelPausa;
    
    [Tooltip("Barra de vida u otros elementos del HUD (se ocultarán durante Game Over)")]
    public GameObject[] elementosHUDOcultar;
    
    [Header("Sonido de Game Over")]
    [Tooltip("AudioSource para reproducir el sonido de Game Over")]
    public AudioSource gameOverAudioSource;
    
    [Tooltip("Sonido que se reproduce al mostrar el Game Over")]
    public AudioClip gameOverSound;
    
    [Range(0f, 1f)]
    public float gameOverSoundVolume = 1f;
    
    [Header("Configuración Automática")]
    [Tooltip("Buscar automáticamente el panel 'GameOver' si no está asignado")]
    public bool buscarPanelAutomaticamente = true;
    
    private CanvasGroup canvasGroup;
    private PausarJuego scriptPausa;
    private static GameOverManager instance;
    
    // Singleton para acceso desde PlayerMovement
    public static GameOverManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameOverManager>();
            }
            return instance;
        }
    }

    void Awake()
    {
        // Configurar singleton
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Buscar panel automáticamente si no está asignado
        if (panelGameOver == null && buscarPanelAutomaticamente)
        {
            panelGameOver = GameObject.Find("GameOver");
            
            if (panelGameOver != null)
            {
                Debug.Log("Panel GameOver encontrado automáticamente: " + panelGameOver.name);
            }
            else
            {
                Debug.LogError("No se pudo encontrar el Panel 'GameOver'. Asígnalo manualmente en el Inspector.");
                return;
            }
        }
        
        // Obtener o agregar CanvasGroup
        if (panelGameOver != null)
        {
            canvasGroup = panelGameOver.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = panelGameOver.AddComponent<CanvasGroup>();
                Debug.Log("CanvasGroup agregado al panel GameOver");
            }
            
            // Inicializar oculto
            OcultarGameOver();
        }
        
        // Buscar script de pausa si no está asignado
        if (panelPausa != null)
        {
            scriptPausa = FindObjectOfType<PausarJuego>();
        }
        
        // Configurar AudioSource para Game Over si no existe
        if (gameOverAudioSource == null)
        {
            // Buscar un AudioSource existente o crear uno nuevo
            gameOverAudioSource = GetComponent<AudioSource>();
            if (gameOverAudioSource == null)
            {
                gameOverAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Configurar el AudioSource
        gameOverAudioSource.playOnAwake = false;
        gameOverAudioSource.volume = gameOverSoundVolume;
        
        Debug.Log("GameOverManager inicializado correctamente");
    }

    /// <summary>
    /// Muestra el menú de Game Over (llamado desde PlayerMovement)
    /// </summary>
    public void MostrarGameOver()
    {
        Debug.Log("=== MOSTRANDO GAME OVER ===");
        
        // Reproducir sonido de Game Over
        if (gameOverSound != null && gameOverAudioSource != null)
        {
            gameOverAudioSource.PlayOneShot(gameOverSound, gameOverSoundVolume);
        }
        
        // Mostrar panel de Game Over
        if (panelGameOver != null)
        {
            panelGameOver.SetActive(true);
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
        }
        
        // Ocultar panel de pausa
        if (panelPausa != null)
        {
            panelPausa.SetActive(false);
        }
        
        // Deshabilitar el script de pausa para evitar que se abra con ESC
        if (scriptPausa != null)
        {
            scriptPausa.enabled = false;
        }
        
        // Ocultar elementos del HUD (barra de vida, etc.)
        foreach (GameObject elemento in elementosHUDOcultar)
        {
            if (elemento != null)
            {
                elemento.SetActive(false);
            }
        }
        
        // Asegurar que el cursor esté visible
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Pausar el juego
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Oculta el menú de Game Over
    /// </summary>
    public void OcultarGameOver()
    {
        if (panelGameOver != null)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            
            panelGameOver.SetActive(false);
        }
        
        // Reactivar panel de pausa
        if (panelPausa != null)
        {
            panelPausa.SetActive(true);
        }
        
        // Reactivar script de pausa
        if (scriptPausa != null)
        {
            scriptPausa.enabled = true;
        }
        
        // Mostrar elementos del HUD
        foreach (GameObject elemento in elementosHUDOcultar)
        {
            if (elemento != null)
            {
                elemento.SetActive(true);
            }
        }
        
        // Reanudar el juego
        Time.timeScale = 1f;
    }

    /// <summary>
    /// Botón: Reiniciar nivel desde el principio
    /// </summary>
    public void BotonReaparecer()
    {
        string nombreEscenaActual = SceneManager.GetActiveScene().name;
        Debug.Log("=== BOTÓN REAPARECER PRESIONADO ===");
        Debug.Log("Escena actual: " + nombreEscenaActual);
        Debug.Log("Reiniciando escena: " + nombreEscenaActual);
        
        // Reanudar el tiempo antes de cargar la escena
        Time.timeScale = 1f;
        
        // Recargar la escena actual (reinicia todo desde el principio)
        SceneManager.LoadScene(nombreEscenaActual);
    }

    /// <summary>
    /// Botón: Salir al menú inicial
    /// </summary>
    public void BotonSalirAlMenu()
    {
        Debug.Log("=== BOTÓN SALIR PRESIONADO ===");
        Debug.Log("Cargando escena: MenuInicial");
        
        // Reanudar el tiempo antes de cambiar de escena
        Time.timeScale = 1f;
        
        // Asegurar cursor visible
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Cargar menú inicial
        SceneManager.LoadScene("MenuInicial");
    }

    /// <summary>
    /// Botón opcional: Salir del juego
    /// </summary>
    public void BotonSalirJuego()
    {
        Debug.Log("=== SALIENDO DEL JUEGO ===");
        
        Time.timeScale = 1f;
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
