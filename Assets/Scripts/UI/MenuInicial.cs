using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuInicial : MonoBehaviour
{
    public void IniciarJuego()
    {
        SceneManager.LoadScene("IntroCutscene");
    }

    public void SalirJuego()
    {
        Application.Quit();
        Debug.Log("Saliendo del Game...");
    }
}
