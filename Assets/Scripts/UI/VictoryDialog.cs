using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Diálogo de victoria que se muestra al derrotar al jefe
/// Hereda de Dialog_2 pero agrega funcionalidad para ir al menú al terminar
/// </summary>
public class VictoryDialog : Dialog_2
{
    [Header("Victory Settings")]
    [Tooltip("Nombre de la escena del menú principal")]
    public string menuSceneName = "MainMenu";
    
    [Tooltip("Tiempo de espera después de terminar el diálogo antes de ir al menú")]
    public float delayBeforeMenu = 2f;
    
    [Tooltip("¿Desactivar el movimiento del jugador durante el diálogo?")]
    public bool disablePlayerMovement = true;
    
    [Header("Audio Settings")]
    [Tooltip("Audio Mixer Group para el diálogo de victoria")]
    public UnityEngine.Audio.AudioMixerGroup audioMixerGroup;
    
    [Tooltip("Volumen del sonido de voz (0 a 1)")]
    [Range(0f, 1f)]
    public float voiceVolume = 0.3f; // Más bajo que el default de 0.7
    
    private bool victoryDialogFinished = false;
    private AudioSource victoryAudioSource;

    void Awake()
    {
        // Configurar AudioSource específico para el diálogo de victoria
        ConfigurarAudioSource();
    }

    /// <summary>
    /// Configura el AudioSource con el Audio Mixer y volumen
    /// </summary>
    void ConfigurarAudioSource()
    {
        // Obtener o crear AudioSource
        victoryAudioSource = GetComponent<AudioSource>();
        
        if (victoryAudioSource == null && npcVoice != null)
        {
            victoryAudioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("[VictoryDialog] AudioSource creado automáticamente");
        }
        
        // Configurar propiedades del AudioSource
        if (victoryAudioSource != null)
        {
            victoryAudioSource.playOnAwake = false;
            victoryAudioSource.volume = voiceVolume;
            
            // Asignar Audio Mixer Group si está configurado
            if (audioMixerGroup != null)
            {
                victoryAudioSource.outputAudioMixerGroup = audioMixerGroup;
                Debug.Log($"[VictoryDialog] Audio Mixer Group asignado: {audioMixerGroup.name}");
            }
            else
            {
                Debug.LogWarning("[VictoryDialog] No se asignó Audio Mixer Group. El sonido usará el output por defecto.");
            }
            
            Debug.Log($"[VictoryDialog] AudioSource configurado con volumen: {voiceVolume}");
        }
    }

    /// <summary>
    /// Inicia el diálogo de victoria automáticamente
    /// </summary>
    public void ShowVictoryDialog()
    {
        Debug.Log("[VictoryDialog] ShowVictoryDialog() llamado");
        
        if (lines == null || lines.Length == 0)
        {
            Debug.LogWarning("[VictoryDialog] No hay líneas de diálogo configuradas");
            GoToMenu();
            return;
        }

        Debug.Log($"[VictoryDialog] Iniciando con {lines.Length} líneas");
        
        // Simular que el jugador está en rango para activar el diálogo
        StartCoroutine(ActivateVictoryDialogCoroutine());
    }

    /// <summary>
    /// Corrutina para activar el diálogo de victoria automáticamente
    /// </summary>
    private IEnumerator ActivateVictoryDialogCoroutine()
    {
        Debug.Log("[VictoryDialog] ActivateVictoryDialogCoroutine iniciado");
        
        // Esperar un frame para asegurarse de que todo esté inicializado
        yield return null;
        
        // Activar el diálogo usando el método privado de la clase base
        // Como no podemos acceder directamente, usamos reflexión o llamamos al método público
        
        // Encontrar al jugador
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && disablePlayerMovement)
        {
            Debug.Log("[VictoryDialog] Congelando jugador");
            
            PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.enabled = false;
            }
            
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector2.zero;
            }
        }
        
        // Forzar inicio del diálogo
        // Como Dialog_2 usa un sistema de triggers, lo activamos manualmente
        if (dialogPanelReference != null)
        {
            Debug.Log("[VictoryDialog] Activando dialogPanelReference");
            dialogPanelReference.SetActive(true);
        }
        else
        {
            Debug.LogError("[VictoryDialog] dialogPanelReference es NULL!");
        }
        
        // Simular que estamos en el método StartDialogue de Dialog_2
        ForceStartDialogue();
        
        // Monitorear cuando termine el diálogo
        StartCoroutine(WaitForDialogueEnd());
    }

    /// <summary>
    /// Forzar el inicio del diálogo sin necesidad de trigger
    /// </summary>
    private void ForceStartDialogue()
    {
        Debug.Log("[VictoryDialog] ForceStartDialogue llamado");
        
        if (lines == null || lines.Length == 0)
        {
            Debug.LogWarning("[VictoryDialog] No hay líneas de diálogo");
            return;
        }

        // Acceder a las variables estáticas de Dialog_2 mediante reflexión
        // o usar los campos públicos si están disponibles
        
        if (dialogPanelReference != null)
        {
            Debug.Log("[VictoryDialog] Activando panel en ForceStartDialogue");
            dialogPanelReference.SetActive(true);
        }
        else
        {
            Debug.LogError("[VictoryDialog] dialogPanelReference es NULL en ForceStartDialogue!");
        }

        if (nameTextReference != null)
        {
            Debug.Log($"[VictoryDialog] Estableciendo nombre: {npcName}");
            nameTextReference.text = npcName;
        }

        if (dialogTextReference != null)
        {
            Debug.Log("[VictoryDialog] dialogTextReference encontrado, iniciando TypeLineVictory");
        }
        else
        {
            Debug.LogError("[VictoryDialog] dialogTextReference es NULL!");
        }

        // Iniciar la primera línea
        StartCoroutine(TypeLineVictory(0));
    }

    /// <summary>
    /// Versión modificada de TypeLine para el diálogo de victoria
    /// </summary>
    private IEnumerator TypeLineVictory(int lineIndex)
    {
        if (lineIndex >= lines.Length)
        {
            victoryDialogFinished = true;
            yield break;
        }

        if (dialogTextReference != null)
        {
            dialogTextReference.text = string.Empty;
            int letterCount = 0;

            foreach (char c in lines[lineIndex].ToCharArray())
            {
                dialogTextReference.text += c;
                letterCount++;

                // Reproducir sonido cada N letras usando el AudioSource configurado
                if (npcVoice != null && victoryAudioSource != null && letterCount % soundEveryNLetters == 0)
                {
                    victoryAudioSource.PlayOneShot(npcVoice, voiceVolume);
                }

                yield return new WaitForSeconds(textSpeed);
            }
        }

        // Esperar input para avanzar
        yield return StartCoroutine(WaitForInputToAdvance(lineIndex));
    }

    /// <summary>
    /// Espera input del jugador para avanzar a la siguiente línea
    /// </summary>
    private IEnumerator WaitForInputToAdvance(int currentLineIndex)
    {
        bool waitingForInput = true;

        while (waitingForInput)
        {
            // Verificar inputs
            bool advanceInput = false;

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                advanceInput = true;
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                advanceInput = true;
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                advanceInput = true;

            if (advanceInput)
            {
                waitingForInput = false;
                
                // Avanzar a la siguiente línea
                int nextLineIndex = currentLineIndex + 1;
                
                if (nextLineIndex < lines.Length)
                {
                    StartCoroutine(TypeLineVictory(nextLineIndex));
                }
                else
                {
                    // Terminó el diálogo
                    victoryDialogFinished = true;
                }
            }

            yield return null;
        }
    }

    /// <summary>
    /// Monitorea cuando el diálogo termina
    /// </summary>
    private IEnumerator WaitForDialogueEnd()
    {
        // Esperar hasta que el diálogo termine
        while (!victoryDialogFinished)
        {
            yield return new WaitForSeconds(0.1f);
        }

        Debug.Log("[VictoryDialog] Diálogo de victoria terminado");

        // Ocultar panel
        if (dialogPanelReference != null)
        {
            dialogPanelReference.SetActive(false);
        }

        // Esperar antes de ir al menú
        yield return new WaitForSeconds(delayBeforeMenu);

        // Ir al menú
        GoToMenu();
    }

    /// <summary>
    /// Carga la escena del menú principal
    /// </summary>
    private void GoToMenu()
    {
        Debug.Log($"[VictoryDialog] Cargando escena: {menuSceneName}");
        SceneManager.LoadScene(menuSceneName);
    }
}
