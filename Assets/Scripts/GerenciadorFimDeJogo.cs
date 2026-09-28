using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GerenciadorFimDeJogo : MonoBehaviour
{
    public string cenaAtual;
    public string cenaMenu = "MenuPrincipal";

    void Start()
    {
        Time.timeScale = 1f;
        cenaAtual = EstadoJogo.FaseParaReiniciar;
    }

    public void Reiniciar()
    {
        if (string.IsNullOrEmpty(cenaAtual))
        {
            cenaAtual = "Fase1"; // valor de segurança, caso teste essa cena sozinha
        }
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
