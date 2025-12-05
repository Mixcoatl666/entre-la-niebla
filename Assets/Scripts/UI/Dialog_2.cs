using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem;

public class Dialog_2 : MonoBehaviour
{
    [Header("Dialogue Content")]
    [TextArea(3, 10)]
    public string[] lines; // Las l�neas de di�logo de este NPC/cartel
    public string npcName = "NPC"; // Nombre del NPC (opcional)

    [Header("UI References (Asignar en el primer NPC)")]
    public GameObject dialogPanelReference; // Panel compartido - asignar solo en el primer NPC
    public TextMeshProUGUI dialogTextReference; // Texto compartido - asignar solo en el primer NPC
    public TextMeshProUGUI nameTextReference; // Texto del nombre (opcional) - asignar solo en el primer NPC
    
    [Header("Local References (Cada NPC tiene el suyo)")]
    [SerializeField] private GameObject dialogMark; // El indicador "E" sobre este NPC
    
    [Header("Dialogue Settings")]
    public float textSpeed = 0.05f;
    public AudioClip npcVoice; // Sonido espec�fico de este NPC
    public int soundEveryNLetters = 2;
    public bool closeDialogueOnFinish = true; // Cerrar autom�ticamente al terminar
    public bool freezePlayer = true; // Congelar al jugador durante el di�logo

    // Static variables - compartidas entre todos los NPCs
    private static GameObject dialogPanel;
    private static TextMeshProUGUI dialogText;
    private static TextMeshProUGUI nameText;

    // Private variables
    private bool isPlayerInRange = false;
    private bool dialogueActive = false;
    private int currentLineIndex = 0;
    private bool isLineComplete = false;
    private AudioSource audioSource;
    private static Dialog_2 activeDialogue; // El di�logo activo actualmente
    
    // Referencias al jugador para congelarlo
    private PlayerMovement playerMovement;
    private Rigidbody2D playerRb;

    void Start()
    {
        // Si este NPC tiene referencias asignadas, compartirlas con todos los dem�s
        if (dialogPanelReference != null)
        {
            dialogPanel = dialogPanelReference;
        }
        if (dialogTextReference != null)
        {
            dialogText = dialogTextReference;
        }
        if (nameTextReference != null)
        {
            nameText = nameTextReference;
        }

        // Configurar AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && npcVoice != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Asegurarse de que el panel est� oculto al inicio
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(false);
        }
    }

    void Update()
    {
        // Solo procesar input si el jugador está en rango y este diálogo está activo (o ninguno está activo)
        if (isPlayerInRange && !dialogueActive && activeDialogue == null)
        {
            // Presionar E para iniciar diálogo (teclado)
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                StartDialogue();
            }
        }
        else if (dialogueActive && activeDialogue == this)
        {
            // Click o E para avanzar/completar línea
            bool advanceInput = false;
            
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                advanceInput = true;
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                advanceInput = true;
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                advanceInput = true;

            if (advanceInput)
            {
                if (isLineComplete)
                {
                    NextLine();
                }
                else
                {
                    // Completar línea actual instantáneamente
                    StopAllCoroutines();
                    dialogText.text = lines[currentLineIndex];
                    isLineComplete = true;
                    if (audioSource != null && audioSource.isPlaying)
                    {
                        audioSource.Stop();
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Método público para interactuar desde controles móviles
    /// Simula presionar E
    /// </summary>
    public void OnMobileInteract()
    {
        // Si el jugador está en rango y no hay diálogo activo, iniciar
        if (isPlayerInRange && !dialogueActive && activeDialogue == null)
        {
            StartDialogue();
        }
        // Si este diálogo está activo, avanzar
        else if (dialogueActive && activeDialogue == this)
        {
            if (isLineComplete)
            {
                NextLine();
            }
            else
            {
                // Completar línea actual instantáneamente
                StopAllCoroutines();
                if (dialogText != null && currentLineIndex < lines.Length)
                {
                    dialogText.text = lines[currentLineIndex];
                }
                isLineComplete = true;
                if (audioSource != null && audioSource.isPlaying)
                {
                    audioSource.Stop();
                }
            }
        }
    }

    void StartDialogue()
    {
        if (lines == null || lines.Length == 0)
        {
            Debug.LogWarning($"No hay l�neas de di�logo configuradas en {gameObject.name}");
            return;
        }

        if (dialogPanel == null || dialogText == null)
        {
            Debug.LogError("No se han configurado las referencias del panel de di�logo. Asigna 'Dialog Panel Reference' y 'Dialog Text Reference' en el primer NPC.");
            return;
        }

        dialogueActive = true;
        activeDialogue = this;
        currentLineIndex = 0;

        // Congelar al jugador
        if (freezePlayer)
        {
            FreezePlayer();
        }

        // Activar panel
        dialogPanel.SetActive(true);

        // Ocultar marcador
        if (dialogMark != null)
        {
            dialogMark.SetActive(false);
        }

        // Mostrar nombre del NPC si hay un campo para ello
        if (nameText != null)
        {
            nameText.text = npcName;
        }

        // Empezar a escribir la primera l�nea
        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        if (currentLineIndex >= lines.Length)
        {
            EndDialogue();
            yield break;
        }

        isLineComplete = false;
        dialogText.text = string.Empty;
        int letterCount = 0;

        foreach (char c in lines[currentLineIndex].ToCharArray())
        {
            dialogText.text += c;
            letterCount++;

            // Reproducir sonido cada N letras
            if (npcVoice != null && audioSource != null && letterCount % soundEveryNLetters == 0)
            {
                audioSource.PlayOneShot(npcVoice);
            }

            yield return new WaitForSeconds(textSpeed);
        }

        isLineComplete = true;
    }

    void NextLine()
    {
        currentLineIndex++;

        if (currentLineIndex < lines.Length)
        {
            StopAllCoroutines();
            StartCoroutine(TypeLine());
        }
        else
        {
            // Se acabaron las l�neas
            if (closeDialogueOnFinish)
            {
                EndDialogue();
            }
        }
    }

    void EndDialogue()
    {
        dialogueActive = false;
        activeDialogue = null;

        // Detener cualquier sonido
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        // Descongelar al jugador
        if (freezePlayer)
        {
            UnfreezePlayer();
        }

        // Ocultar panel
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(false);
        }

        // Mostrar marcador si el jugador sigue en rango
        if (isPlayerInRange && dialogMark != null)
        {
            dialogMark.SetActive(true);
        }

        // Limpiar texto
        if (dialogText != null)
        {
            dialogText.text = string.Empty;
        }
    }

    void FreezePlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Guardar referencias
            playerMovement = player.GetComponent<PlayerMovement>();
            playerRb = player.GetComponent<Rigidbody2D>();

            // Desactivar el script de movimiento
            if (playerMovement != null)
            {
                playerMovement.enabled = false;
            }

            // Detener completamente el movimiento
            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector2.zero;
                playerRb.angularVelocity = 0f;
            }
        }
    }

    void UnfreezePlayer()
    {
        // Reactivar el script de movimiento
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        // Limpiar referencias
        playerMovement = null;
        playerRb = null;
    }

    // Detectar cuando el jugador entra en el trigger
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
            
            // Mostrar marcador solo si no hay otro di�logo activo
            if (dialogMark != null && activeDialogue == null)
            {
                dialogMark.SetActive(true);
            }
        }
    }

    // Detectar cuando el jugador sale del trigger
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;

            // Ocultar marcador
            if (dialogMark != null)
            {
                dialogMark.SetActive(false);
            }

            // Si este di�logo est� activo, cerrarlo
            if (dialogueActive && activeDialogue == this)
            {
                EndDialogue();
            }
        }
    }
}
