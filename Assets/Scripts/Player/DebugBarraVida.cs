using UnityEngine;

/// <summary>
/// Script temporal de DEBUG para verificar por qué la barra de vida no funciona
/// ELIMINA ESTE SCRIPT cuando encuentres el problema
/// </summary>
public class DebugBarraVida : MonoBehaviour
{
    void Update()
    {
        // Presiona la tecla H para hacer daño manual (testing)
        if (Input.GetKeyDown(KeyCode.H))
        {
            GameObject jugador = GameObject.Find("Bimuri");
            
            if (jugador != null)
            {
                Debug.Log("? Encontré el objeto 'Bimuri'");
                
                PlayerMovement pm = jugador.GetComponent<PlayerMovement>();
                if (pm != null)
                {
                    Debug.Log($"? PlayerMovement encontrado. Vida actual: {pm.vida}/{pm.vidaMaxima}");
                    pm.RecibirDanio(10f);
                    Debug.Log($"? Daño aplicado manualmente. Nueva vida: {pm.vida}");
                }
                else
                {
                    Debug.LogError("? No se encontró el componente PlayerMovement en 'Bimuri'");
                }
            }
            else
            {
                Debug.LogError("? No se encontró ningún objeto llamado 'Bimuri' en la escena");
                Debug.Log("Objetos con tag 'Player' encontrados:");
                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
                foreach (GameObject p in players)
                {
                    Debug.Log($"  - {p.name}");
                }
            }
        }
        
        // Presiona la tecla J para verificar la barra de vida
        if (Input.GetKeyDown(KeyCode.J))
        {
            BarraVida[] barras = FindObjectsOfType<BarraVida>();
            Debug.Log($"Barras de vida encontradas: {barras.Length}");
            
            foreach (BarraVida barra in barras)
            {
                Debug.Log($"  - Barra en objeto: {barra.gameObject.name}");
                Debug.Log($"    Imagen asignada: {(barra.barraVida != null ? "? SÍ" : "? NO")}");
            }
        }
    }
}
