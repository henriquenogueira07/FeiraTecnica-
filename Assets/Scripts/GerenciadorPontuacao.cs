using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class RegistroPontuacao
{
    public string Nome;
    public int Pontos;
    public string Dificuldade;
    public int Inimigos;
    public int Itens;
    public string Data;
    public int Ordem; 
}

[Serializable]
public class ListaDeRegistros
{
    public List<RegistroPontuacao> Registros = new List<RegistroPontuacao>();
}

public class GerenciadorPontuacao : MonoBehaviour
{
    const string CHAVE_SALVAMENTO = "LeaderboardFeiraTecnica";
    const string NOME_ARQUIVO_BACKUP = "leaderboard_feira_backup.csv";

    static GerenciadorPontuacao instancia;

    public static GerenciadorPontuacao Instancia
    {
        get
        {
            if (instancia == null)
            {
                GameObject obj = new GameObject("GerenciadorPontuacao");
                instancia = obj.AddComponent<GerenciadorPontuacao>();
            }
            return instancia;
        }
    }

    public int PontosAtuais { get; private set; }
    public int InimigosDerrotados { get; private set; }
    public int ItensColetados { get; private set; }
    public bool PartidaRegistrada { get; private set; }
    public RegistroPontuacao UltimoRegistro { get; private set; }

    public static string CaminhoBackup
    {
        get { return Path.Combine(Application.persistentDataPath, NOME_ARQUIVO_BACKUP); }
    }

    void Awake()
    {
        if (instancia != null && instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        instancia = this;
        DontDestroyOnLoad(gameObject);
    }

    public void NovaPartida()
    {
        PontosAtuais = 0;
        InimigosDerrotados = 0;
        ItensColetados = 0;
        PartidaRegistrada = false;
        UltimoRegistro = null;
    }

    public void AdicionarPontosItem(int pontos)
    {
        ItensColetados++;
        PontosAtuais += ConfiguracaoJogo.CalcularPontos(pontos);
    }

    public void AdicionarPontosInimigo(int pontos)
    {
        InimigosDerrotados++;
        PontosAtuais += ConfiguracaoJogo.CalcularPontos(pontos);
    }

    public void RegistrarPartida(string nome)
    {
        if (PartidaRegistrada) return;

        ListaDeRegistros lista = Carregar();

        RegistroPontuacao registro = new RegistroPontuacao();
        registro.Nome = LimparNome(nome);
        registro.Pontos = PontosAtuais;
        registro.Dificuldade = ConfiguracaoJogo.NomeDificuldade;
        registro.Inimigos = InimigosDerrotados;
        registro.Itens = ItensColetados;
        registro.Data = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        registro.Ordem = lista.Registros.Count + 1;

        lista.Registros.Add(registro);
        Salvar(lista);
        EscreverNoBackup(registro.Data + ";" + registro.Nome + ";" + registro.Pontos + ";" +
                         registro.Dificuldade + ";" + registro.Inimigos + ";" + registro.Itens);

        UltimoRegistro = registro;
        PartidaRegistrada = true;
    }

    public static List<RegistroPontuacao> ObterTodos()
    {
        List<RegistroPontuacao> lista = Carregar().Registros;
        lista.Sort((a, b) =>
        {
            int comparacao = b.Pontos.CompareTo(a.Pontos);
            return comparacao != 0 ? comparacao : a.Ordem.CompareTo(b.Ordem);
        });
        return lista;
    }

    public static List<RegistroPontuacao> ObterTop(int quantidade)
    {
        List<RegistroPontuacao> lista = ObterTodos();
        return lista.GetRange(0, Mathf.Min(quantidade, lista.Count));
    }

    // Zera o ranking (o arquivo CSV de backup NÃO é apagado, só recebe uma marcação).
    public static void ApagarLeaderboard()
    {
        PlayerPrefs.DeleteKey(CHAVE_SALVAMENTO);
        PlayerPrefs.Save();
        EscreverNoBackup("---- RANKING ZERADO EM " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " ----");
    }

    static ListaDeRegistros Carregar()
    {
        string json = PlayerPrefs.GetString(CHAVE_SALVAMENTO, "");
        if (string.IsNullOrEmpty(json)) return new ListaDeRegistros();

        try
        {
            ListaDeRegistros lista = JsonUtility.FromJson<ListaDeRegistros>(json);
            if (lista == null || lista.Registros == null) return new ListaDeRegistros();
            return lista;
        }
        catch (Exception)
        {
            return new ListaDeRegistros();
        }
    }

    static void Salvar(ListaDeRegistros lista)
    {
        PlayerPrefs.SetString(CHAVE_SALVAMENTO, JsonUtility.ToJson(lista));
        PlayerPrefs.Save();
    }

    static void EscreverNoBackup(string linha)
    {
        try
        {
            bool arquivoNovo = !File.Exists(CaminhoBackup);
            using (StreamWriter escritor = new StreamWriter(CaminhoBackup, true))
            {
                if (arquivoNovo) escritor.WriteLine("Data;Nome;Pontos;Dificuldade;Inimigos;Itens");
                escritor.WriteLine(linha);
            }
            Debug.Log("Backup do ranking salvo em: " + CaminhoBackup);
        }
        catch (Exception erro)
        {
            Debug.LogWarning("Não foi possível salvar o backup do ranking: " + erro.Message);
        }
    }

    static string LimparNome(string nome)
    {
        if (string.IsNullOrEmpty(nome)) return "Jogador";

        nome = nome.Replace(";", " ").Replace("\n", " ").Replace("\r", " ")
                   .Replace("<", "").Replace(">", "").Trim();

        if (nome.Length > 20) nome = nome.Substring(0, 20);
        return nome.Length == 0 ? "Jogador" : nome;
    }
}
