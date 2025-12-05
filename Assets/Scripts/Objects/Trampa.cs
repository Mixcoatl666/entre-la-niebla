using UnityEngine;

/// <summary>
/// Script para trampas estáticas como pinchos, sierras, fuego, etc.
/// Objetos que causan daño pero NO se destruyen
/// </summary>
public class Trampa : MonoBehaviour
{
    [Header("Configuración de Daño")]
    [Tooltip("Cantidad de daño que causa la trampa")]
    public float danioTrampa = 20f;
    
    [Tooltip("Tipo de daño: Instantáneo (al tocar) o Continuo (mientras toca)")]
    public TipoDanio tipoDanio = TipoDanio.Instantaneo;
    
    [Tooltip("Intervalo entre daños si es continuo (segundos)")]
    public float intervaloDanio = 0.5f;
    
    [Header("Knockback")]
    [Tooltip("¿La trampa causa knockback?")]
    public bool causaKnockback = true;
    
    private float tiempoUltimoDanio = -999f;

    public enum TipoDanio
    {
        Instantaneo,  // Solo una vez al tocar (como pinchos)
        Continuo      // Daño repetido mientras toca (como lava o fuego)
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (tipoDanio == TipoDanio.Instantaneo)
            {
                // Daño instantáneo con knockback
                PlayerMovement playerMovement = collision.gameObject.GetComponent<PlayerMovement>();
                if (playerMovement != null)
                {
                    if (causaKnockback)
                    {
                        // Usar la posición de la trampa para calcular dirección del knockback
                        playerMovement.RecibirDanio(danioTrampa, transform.position);
                    }
                    else
                    {
                        // Sin knockback
                        playerMovement.RecibirDanio(danioTrampa);
                    }
                    Debug.Log($"Trampa causó {danioTrampa} de daño (instantáneo)");
                }
            }
            else
            {
                // Iniciar daño continuo
                tiempoUltimoDanio = Time.time;
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && tipoDanio == TipoDanio.Continuo)
        {
            // Daño continuo con knockback
            if (Time.time >= tiempoUltimoDanio + intervaloDanio)
            {
                PlayerMovement playerMovement = collision.gameObject.GetComponent<PlayerMovement>();
                if (playerMovement != null)
                {
                    if (causaKnockback)
                    {
                        playerMovement.RecibirDanio(danioTrampa, transform.position);
                    }
                    else
                    {
                        playerMovement.RecibirDanio(danioTrampa);
                    }
                    tiempoUltimoDanio = Time.time;
                    Debug.Log($"Trampa causó {danioTrampa} de daño (continuo)");
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (tipoDanio == TipoDanio.Instantaneo)
            {
                // Daño instantáneo con knockback
                PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
                if (playerMovement != null)
                {
                    if (causaKnockback)
                    {
                        playerMovement.RecibirDanio(danioTrampa, transform.position);
                    }
                    else
                    {
                        playerMovement.RecibirDanio(danioTrampa);
                    }
                    Debug.Log($"Trampa causó {danioTrampa} de daño (instantáneo)");
                }
            }
            else
            {
                // Iniciar daño continuo
                tiempoUltimoDanio = Time.time;
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && tipoDanio == TipoDanio.Continuo)
        {
            // Daño continuo con knockback
            if (Time.time >= tiempoUltimoDanio + intervaloDanio)
            {
                PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
                if (playerMovement != null)
                {
                    if (causaKnockback)
                    {
                        playerMovement.RecibirDanio(danioTrampa, transform.position);
                    }
                    else
                    {
                        playerMovement.RecibirDanio(danioTrampa);
                    }
                    tiempoUltimoDanio = Time.time;
                    Debug.Log($"Trampa causó {danioTrampa} de daño (continuo)");
                }
            }
        }
    }

    // Visualizar el área de la trampa en el editor
    private void OnDrawGizmos()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f); // Rojo transparente
            Gizmos.DrawCube(transform.position, col.bounds.size);
        }
    }
}
