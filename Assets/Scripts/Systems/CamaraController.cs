using UnityEngine;

public class CamaraController : MonoBehaviour
{
    [Header("Target")]
    public Transform objetivo;

    [Header("Follow Settings")]
    public float velocidadCamaraX = 8f;
    public float velocidadCamaraY = 5f;

    [Header("Offset & Dead Zone")]
    public Vector2 offsetPosicion = new Vector2(0f, 2f); // Offset en X y Y (personaje más abajo)
    public Vector2 deadZone = new Vector2(3f, 2f); // Zona muerta donde no se mueve la cámara
    
    [Header("Look Ahead")]
    public float lookAheadDistance = 2f; // Cuánto "mira adelante" en la dirección del movimiento
    public float lookAheadSpeed = 2f;

    private Vector3 velocity = Vector3.zero;
    private float currentLookAhead = 0f;
    private float lastPlayerX = 0f;

    void Start()
    {
        if (objetivo != null)
        {
            lastPlayerX = objetivo.position.x;
        }
    }

    void LateUpdate()
    {
        if (objetivo == null) return;

        // Calcular la dirección del movimiento del jugador
        float playerDelta = objetivo.position.x - lastPlayerX;
        float playerDirection = playerDelta != 0 ? Mathf.Sign(playerDelta) : 0;
        lastPlayerX = objetivo.position.x;

        // Look ahead suave (solo si el jugador se está moviendo)
        float targetLookAhead = playerDirection * lookAheadDistance;
        currentLookAhead = Mathf.Lerp(currentLookAhead, targetLookAhead, lookAheadSpeed * Time.deltaTime);

        // Posición objetivo base
        Vector3 targetPos = objetivo.position;
        targetPos.x += offsetPosicion.x + currentLookAhead;
        targetPos.y += offsetPosicion.y;
        targetPos.z = transform.position.z; // Mantener la Z de la cámara

        // Aplicar Dead Zone
        Vector3 currentPos = transform.position;
        
        // Dead Zone en X
        float deltaX = targetPos.x - currentPos.x;
        if (Mathf.Abs(deltaX) > deadZone.x)
        {
            targetPos.x = currentPos.x + Mathf.Sign(deltaX) * (Mathf.Abs(deltaX) - deadZone.x);
        }
        else
        {
            targetPos.x = currentPos.x;
        }

        // Dead Zone en Y
        float deltaY = targetPos.y - currentPos.y;
        if (Mathf.Abs(deltaY) > deadZone.y)
        {
            targetPos.y = currentPos.y + Mathf.Sign(deltaY) * (Mathf.Abs(deltaY) - deadZone.y);
        }
        else
        {
            targetPos.y = currentPos.y;
        }

        // Movimiento suave con velocidades diferentes para X e Y
        Vector3 newPos = currentPos;
        newPos.x = Mathf.Lerp(currentPos.x, targetPos.x, velocidadCamaraX * Time.deltaTime);
        newPos.y = Mathf.Lerp(currentPos.y, targetPos.y, velocidadCamaraY * Time.deltaTime);
        newPos.z = currentPos.z; // Asegurar que Z no cambie

        transform.position = newPos;
    }

    // Visualizar la dead zone en el editor
    void OnDrawGizmosSelected()
    {
        if (objetivo == null) return;

        // Dibujar la zona muerta
        Gizmos.color = Color.yellow;
        Vector3 center = transform.position;
        Vector3 size = new Vector3(deadZone.x * 2, deadZone.y * 2, 0);
        Gizmos.DrawWireCube(center, size);

        // Dibujar la posición objetivo del personaje (con offset)
        Gizmos.color = Color.green;
        Vector3 targetPoint = new Vector3(
            objetivo.position.x + offsetPosicion.x,
            objetivo.position.y + offsetPosicion.y,
            transform.position.z
        );
        Gizmos.DrawWireSphere(targetPoint, 0.5f);
    }
}
