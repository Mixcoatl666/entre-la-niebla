using UnityEngine;

/// <summary>
/// Script OPCIONAL para enemigos que quieras que causen daño personalizado
/// Si no usas este script, el enemigo causará el daño por defecto (danioDeEnemigos en PlayerMovement)
/// </summary>
public class EnemigoPersonalizado : MonoBehaviour
{
    [Header("Configuración de Daño")]
    [Tooltip("Cantidad de daño específico de este enemigo")]
    public float danioEspecifico = 15f;
    
    [Tooltip("¿El enemigo se destruye al tocar al jugador?")]
    public bool destruirAlTocar = false;
    
    [Tooltip("Cooldown entre ataques (segundos)")]
    public float cooldownAtaque = 1f;
    
    private float tiempoUltimoAtaque = -999f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Verificar cooldown
            if (Time.time >= tiempoUltimoAtaque + cooldownAtaque)
            {
                PlayerMovement playerMovement = collision.gameObject.GetComponent<PlayerMovement>();
                
                if (playerMovement != null)
                {
                    playerMovement.RecibirDanio(danioEspecifico);
                    tiempoUltimoAtaque = Time.time;
                    
                    if (destruirAlTocar)
                    {
                        Destroy(gameObject);
                    }
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Verificar cooldown
            if (Time.time >= tiempoUltimoAtaque + cooldownAtaque)
            {
                PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();

                if (playerMovement != null)
                {
                    playerMovement.RecibirDanio(danioEspecifico);
                    tiempoUltimoAtaque = Time.time;

                    if (destruirAlTocar)
                    {
                        Destroy(gameObject);
                    }
                }
            }
        }
    }
}
