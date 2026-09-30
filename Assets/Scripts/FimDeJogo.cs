using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FimDeJogo : MonoBehaviour {

  public string cenaAtual;
    public string cenaMenu = "MenuPrincipal";

    void Start()
    {
        Time.timeScale = 1f;
        cenaAtual = EstadoJogo.FaseParaReiniciar;
    }

    public void Reiniciar()
    {
        SceneManager.LoadScene(cenaAtual);
    }

    public void VoltarAoMenu()
    {
        SceneManager.LoadScene(cenaMenu);
    }
}

public static class EstadoJogo
{
    public static string FaseParaReiniciar;
}
