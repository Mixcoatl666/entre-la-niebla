using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class BarraVida : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Arrastra aquí la imagen de relleno de la barra")]
    public Image barraVida;
    
    [Tooltip("Arrastra aquí el jugador (opcional, si no lo haces busca por tag)")]
    public PlayerMovement playerMovement;
    
    private float vidaMaxima;

    void Start()
    {
        // Si no se asignó manualmente, buscar al jugador
        if (playerMovement == null)
        {
            // Primero intentar buscar por tag "Player"
            GameObject jugador = GameObject.FindGameObjectWithTag("Player");
            
            if (jugador != null)
            {
                playerMovement = jugador.GetComponent<PlayerMovement>();
                Debug.Log($"✅ Jugador encontrado por tag: {jugador.name}");
            }
            else
            {
                // Si no lo encuentra por tag, intentar por nombre "Bimori"
                jugador = GameObject.Find("Bimori");
                if (jugador != null)
                {
                    playerMovement = jugador.GetComponent<PlayerMovement>();
                    Debug.Log($"✅ Jugador encontrado por nombre: {jugador.name}");
                }
            }
        }
        
        // Verificar si se encontró el PlayerMovement
        if (playerMovement != null)
        {
            vidaMaxima = playerMovement.vidaMaxima;
            Debug.Log($"✅ BarraVida inicializada. Vida máxima: {vidaMaxima}");
        }
        else
        {
            Debug.LogError("❌ NO SE ENCONTRÓ EL JUGADOR. Verifica:\n" +
                          "1. Que tu jugador tenga el Tag 'Player'\n" +
                          "2. O que se llame 'Bimori'\n" +
                          "3. O arrastra manualmente el jugador al campo 'Player Movement' en el Inspector");
        }

        // Verificar la imagen
        if (barraVida == null)
        {
            Debug.LogError("❌ NO SE ASIGNÓ LA IMAGEN. Arrastra la imagen de relleno al campo 'Barra Vida' en el Inspector");
        }
        else
        {
            Debug.Log($"✅ Imagen de barra de vida asignada: {barraVida.name}");
            
            // Configurar automáticamente como barra horizontal
            if (barraVida.type == Image.Type.Filled)
            {
                barraVida.fillMethod = Image.FillMethod.Horizontal;
                barraVida.fillOrigin = (int)Image.OriginHorizontal.Left;
                Debug.Log("✅ Barra configurada como HORIZONTAL (se vacía de derecha a izquierda)");
            }
            else
            {
                Debug.LogWarning("⚠️ La imagen debería tener Image Type = 'Filled' para funcionar correctamente");
            }
        }
    }

    private void Update()
    {
        if (playerMovement != null && barraVida != null)
        {
            // Actualizar el fillAmount de la barra de vida
            float porcentajeVida = playerMovement.vida / vidaMaxima;
            barraVida.fillAmount = porcentajeVida;
            
            // Debug para ver el cambio
            // Debug.Log($"Vida: {playerMovement.vida}/{vidaMaxima} = {porcentajeVida * 100}%");
        }
    }
}
