using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TelaFimDeJogo : MonoBehaviour
{
    public Text TextoPontuacao;
    public Text TextoDetalhes;          
    public GameObject PainelRegistro;  
    public InputField CampoNome;
    public Text TextoAviso;            
    public ExibidorLeaderboard Leaderboard;

    public string CenaMenu = "Menu";
    public string CenaJogarNovamente = "Fase1";

    void Start()
    {
        Time.timeScale = 1f;
        GerenciadorPontuacao g = GerenciadorPontuacao.Instancia;

        if (TextoPontuacao != null) TextoPontuacao.text = "Pontuação: " + g.PontosAtuais;
        if (TextoDetalhes != null)
        {
            TextoDetalhes.text = "Inimigos derrotados: " + g.InimigosDerrotados +
                                 "\nItens coletados: " + g.ItensColetados +
                                 "\nDificuldade: " + ConfiguracaoJogo.NomeDificuldade;
        }
        if (TextoAviso != null) TextoAviso.text = "";

        bool podeRegistrar = !g.PartidaRegistrada && g.PontosAtuais > 0;
        if (PainelRegistro != null) PainelRegistro.SetActive(podeRegistrar);

        if (podeRegistrar && CampoNome != null)
        {
            CampoNome.characterLimit = 20;
            CampoNome.Select();
            CampoNome.ActivateInputField();
        }

        if (Leaderboard != null) Leaderboard.Atualizar(g.UltimoRegistro);
    }

    void Update()
    {
        if (PainelRegistro != null && PainelRegistro.activeSelf &&
            (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            Salvar();
        }
    }

    public void Salvar()
    {
        GerenciadorPontuacao g = GerenciadorPontuacao.Instancia;
        if (g.PartidaRegistrada) return;

        string nome = CampoNome != null ? CampoNome.text.Trim() : "";
        if (nome.Length == 0)
        {
            if (TextoAviso != null) TextoAviso.text = "Digite seu nome para entrar no ranking!";
            return;
        }

        g.RegistrarPartida(nome);
        if (PainelRegistro != null) PainelRegistro.SetActive(false);
        if (TextoAviso != null) TextoAviso.text = "Pontuação registrada! Boa sorte, " + nome + "!";
        if (Leaderboard != null) Leaderboard.Atualizar(g.UltimoRegistro);
    }

    public void JogarNovamente()
    {
        GerenciadorPontuacao.Instancia.NovaPartida();
        SceneManager.LoadScene(CenaJogarNovamente);
    }

    public void VoltarAoMenu()
    {
        GerenciadorPontuacao.Instancia.NovaPartida();
        SceneManager.LoadScene(CenaMenu);
    }
}
