using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class ExibidorLeaderboard : MonoBehaviour
{
    public Text TextoLista;
    public int Quantidade = 10;
    public bool PermitirZerar = true;

    float tempoSegurando;
    RegistroPontuacao destaqueAtual;

    void OnEnable()
    {
        Atualizar(destaqueAtual);
    }

    public void Atualizar()
    {
        Atualizar(destaqueAtual);
    }

    public void Atualizar(RegistroPontuacao destaque)
    {
        destaqueAtual = destaque;
        if (TextoLista == null) return;

        List<RegistroPontuacao> top = GerenciadorPontuacao.ObterTop(Quantidade);
        if (top.Count == 0)
        {
            TextoLista.text = "Ninguém jogou ainda.\nSeja o primeiro!";
            return;
        }

        StringBuilder texto = new StringBuilder();
        for (int i = 0; i < top.Count; i++)
        {
            RegistroPontuacao r = top[i];
            string linha = (i + 1) + "º  " + r.Nome + "  -  " + r.Pontos + " pts  (" + r.Dificuldade + ")";
            if (destaque != null && r.Ordem == destaque.Ordem) linha = "<b>" + linha + "  <-- VOCÊ</b>";
            texto.AppendLine(linha);
        }
        TextoLista.text = texto.ToString();
    }

    void Update()
    {
        if (!PermitirZerar) return;

        bool segurando = (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) &&
                         (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) &&
                         Input.GetKey(KeyCode.Delete);

        if (segurando)
        {
            tempoSegurando += Time.unscaledDeltaTime;
            if (tempoSegurando >= 3f)
            {
                tempoSegurando = 0f;
                GerenciadorPontuacao.ApagarLeaderboard();
                destaqueAtual = null;
                Atualizar();
                Debug.Log("Ranking zerado!");
            }
        }
        else
        {
            tempoSegurando = 0f;
        }
    }
}
