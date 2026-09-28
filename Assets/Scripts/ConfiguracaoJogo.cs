using UnityEngine;

public enum Dificuldade { Normal = 0, PaiDeFamilia = 1 }

public static class ConfiguracaoJogo
{
    public const float DanoJogador_Normal = 1f;
    public const float DanoJogador_PaiDeFamilia = 1.75f;
    public const float DanoRecebido_Normal = 1f;
    public const float DanoRecebido_PaiDeFamilia = 0.5f;
    public const float Pontos_Normal = 1f;
    public const float Pontos_PaiDeFamilia = 0.75f;
    const string CHAVE = "DificuldadeJogo";
    static bool carregado;
    static Dificuldade atual;

    public static Dificuldade DificuldadeAtual
    {
        get
        {
            if (!carregado)
            {
                atual = (Dificuldade)PlayerPrefs.GetInt(CHAVE, 0);
                carregado = true;
            }
            return atual;
        }
        set
        {
            atual = value;
            carregado = true;
            PlayerPrefs.SetInt(CHAVE, (int)value);
            PlayerPrefs.Save();
        }
    }

    public static bool ModoPaiDeFamilia
    {
        get { return DificuldadeAtual == Dificuldade.PaiDeFamilia; }
    }

    public static string NomeDificuldade
    {
        get { return ModoPaiDeFamilia ? "Pai de Família" : "Normal"; }
    }

    public static float MultiplicadorDanoJogador
    {
        get { return ModoPaiDeFamilia ? DanoJogador_PaiDeFamilia : DanoJogador_Normal; }
    }

    public static float MultiplicadorDanoRecebido
    {
        get { return ModoPaiDeFamilia ? DanoRecebido_PaiDeFamilia : DanoRecebido_Normal; }
    }

    public static float MultiplicadorPontos
    {
        get { return ModoPaiDeFamilia ? Pontos_PaiDeFamilia : Pontos_Normal; }
    }

    public static int CalcularDanoJogador(int danoBase)
    {
        return Mathf.Max(1, Mathf.RoundToInt(danoBase * MultiplicadorDanoJogador));
    }

    public static int CalcularDanoRecebido(int danoBase)
    {
        return Mathf.Max(1, Mathf.RoundToInt(danoBase * MultiplicadorDanoRecebido));
    }

    public static int CalcularPontos(int pontosBase)
    {
        return Mathf.Max(0, Mathf.RoundToInt(pontosBase * MultiplicadorPontos));
    }
}
