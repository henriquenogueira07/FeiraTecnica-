using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClickAsset : MonoBehaviour
{
    bool apertou;
    string tagObjeto;

    void Start()
    {
        apertou = false;
    }

    void OnMouseDown()
    {
        tagObjeto = gameObject.tag;


        if (tagObjeto == "BotaoJogarFase1")
        {
            SceneManager.LoadScene("Fase01");
        }

        if (tagObjeto == "BotaoJogarFase2")
        {
            SceneManager.LoadScene("Fase2");
        }

        if (tagObjeto == "SairDoJogo")
        {
            Application.Quit();
        }
         if (tagObjeto == "Menu")
        {
            SceneManager.LoadScene("TelaInicial");
        }
    }
}

