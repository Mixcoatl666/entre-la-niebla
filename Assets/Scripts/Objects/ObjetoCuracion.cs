using UnityEngine;

/// <summary>
/// Script para objetos que curan al jugador
/// Coloca este script en objetos como pociones, frutas, botiquines, etc.
/// </summary>
public class ObjetoCuracion : MonoBehaviour
{
    [Header("Configuración de Curación")]
    [Tooltip("Cantidad de vida que restaura")]
    public float cantidadCuracion = 25f;
    
    [Tooltip("¿Se destruye el objeto después de usarlo?")]
    public bool destruirDespuesDeUsar = true;
    
    [Tooltip("¿Se puede recoger automáticamente o requiere interacción?")]
    public bool recogerAutomatico = true;
    
    [Header("Efectos Visuales Opcionales")]
    [Tooltip("Partículas que aparecen al recoger el objeto")]
    public GameObject efectoParticulas;
    
    [Tooltip("Velocidad de rotación del objeto (0 = sin rotación)")]
    public float velocidadRotacion = 50f;
    
    [Tooltip("Amplitud de flotación del objeto (0 = sin flotación)")]
    public float amplitudFlotacion = 0.3f;
    
    [Tooltip("Velocidad de flotación")]
    public float velocidadFlotacion = 2f;
    
    private Vector3 posicionInicial;
    private bool yaUsado = false;

    void Start()
    {
        posicionInicial = transform.position;
    }

    void Update()
    {
        // Efecto de rotación
        if (velocidadRotacion != 0f)
        {
            transform.Rotate(Vector3.forward * velocidadRotacion * Time.deltaTime);
        }
        
        // Efecto de flotación
        if (amplitudFlotacion != 0f)
        {
            float nuevaY = posicionInicial.y + Mathf.Sin(Time.time * velocidadFlotacion) * amplitudFlotacion;
            transform.position = new Vector3(transform.position.x, nuevaY, transform.position.z);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (yaUsado) return;

        // Verificar si es el jugador
        if (other.CompareTag("Player"))
        {
            if (recogerAutomatico)
            {
                UsarObjeto(other.gameObject);
            }
        }
    }

    void UsarObjeto(GameObject jugador)
    {
        if (yaUsado) return;

        PlayerMovement player = jugador.GetComponent<PlayerMovement>();
        
        if (player != null)
        {
            // Solo curar si el jugador no tiene vida completa
            if (player.vida < player.vidaMaxima)
            {
                // Curar al jugador
                player.Curar(cantidadCuracion);
                yaUsado = true;
                
                // Reproducir efecto de partículas
                if (efectoParticulas != null)
                {
                    Instantiate(efectoParticulas, transform.position, Quaternion.identity);
                }
                
                Debug.Log("¡Objeto de curación usado! Restauró " + cantidadCuracion + " de vida");
                
                // Destruir el objeto
                if (destruirDespuesDeUsar)
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                Debug.Log("La vida del jugador ya está completa");
            }
        }
    }

    // Método para usar manualmente el objeto (si no es automático)
    public void UsarManualmente(GameObject jugador)
    {
        if (!recogerAutomatico)
        {
            UsarObjeto(jugador);
        }
    }

    // Método opcional para cambiar la cantidad de curación
    public void CambiarCuracion(float nuevaCantidad)
    {
        cantidadCuracion = nuevaCantidad;
    }
}
