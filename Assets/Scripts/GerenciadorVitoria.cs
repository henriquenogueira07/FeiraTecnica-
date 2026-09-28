using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GerenciadorVitoria : MonoBehaviour
{
    public string cenaCreditos = "Creditos";
    public string cenaMenu = "MenuPrincipal";

    public void AbrirCreditos()
    {
        SceneManager.LoadScene(cenaCreditos);
    }

    public void VoltarAoMenu()
    {
        SceneManager.LoadScene(cenaMenu);
    }
}
