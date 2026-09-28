using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Coloque no CANVAS (não no painel!). Só precisa de UMA por cena.
public class CaixaDialogo : MonoBehaviour
{
    public static CaixaDialogo Instancia;
    public static bool DialogoAberto;
    public static int FrameFechamento = -1;

    [Header("Referências da UI")]
    public GameObject Painel;               // o painel da caixa de texto (começa escondido)
    public Text TextoNome;
    public Text TextoFala;
    public GameObject IndicadorContinuar;   // opcional: ex. um texto "Aperte qualquer tecla ▼"

    [Header("Configuração")]
    public float LetrasPorSegundo = 45f;    // 0 = texto aparece de uma vez
    public bool PausarJogoDuranteDialogo = true;

    string[] falas;
    int indice;
    bool digitando;
    int frameAbertura;
    int frameUltimoAvanco = -1;
    Coroutine rotinaDigitacao;

    void Awake()
    {
        Instancia = this;
        DialogoAberto = false;
        if (Painel != null) Painel.SetActive(false);
    }

    void OnDestroy()
    {
        if (Instancia == this)
        {
            Instancia = null;
            DialogoAberto = false;
        }
    }

    void Update()
    {
        if (!DialogoAberto) return;
        if (Time.frameCount == frameAbertura) return; // não pula a 1ª fala com a mesma tecla que abriu
        if (Input.anyKeyDown) Avancar();                // qualquer tecla ou clique avança / fecha
    }

    public void Abrir(string nome, string[] linhas)
    {
        if (linhas == null || linhas.Length == 0 || Painel == null) return;

        falas = linhas;
        indice = 0;
        frameAbertura = Time.frameCount;
        DialogoAberto = true;

        Painel.SetActive(true);
        if (TextoNome != null) TextoNome.text = nome;
        if (PausarJogoDuranteDialogo) Time.timeScale = 0f;

        MostrarFalaAtual();
    }

    public void Avancar()
    {
        if (!DialogoAberto) return;
        if (Time.frameCount == frameUltimoAvanco) return;
        frameUltimoAvanco = Time.frameCount;

        // Se ainda está digitando, a 1ª tecla só completa a frase
        if (digitando)
        {
            if (rotinaDigitacao != null) StopCoroutine(rotinaDigitacao);
            if (TextoFala != null) TextoFala.text = falas[indice];
            digitando = false;
            AtualizarIndicador();
            return;
        }

        indice++;
        if (indice >= falas.Length) Fechar();
        else MostrarFalaAtual();
    }

    void MostrarFalaAtual()
    {
        if (rotinaDigitacao != null) StopCoroutine(rotinaDigitacao);
        rotinaDigitacao = StartCoroutine(Digitar(falas[indice]));
    }

    IEnumerator Digitar(string texto)
    {
        digitando = true;
        AtualizarIndicador();
        if (TextoFala != null) TextoFala.text = "";

        float intervalo = LetrasPorSegundo > 0f ? 1f / LetrasPorSegundo : 0f;
        for (int i = 0; i < texto.Length; i++)
        {
            if (TextoFala != null) TextoFala.text += texto[i];
            if (intervalo > 0f) yield return new WaitForSecondsRealtime(intervalo);
        }

        digitando = false;
        AtualizarIndicador();
    }

    void AtualizarIndicador()
    {
        if (IndicadorContinuar != null) IndicadorContinuar.SetActive(!digitando);
    }

    public void Fechar()
    {
        if (rotinaDigitacao != null) StopCoroutine(rotinaDigitacao);
        digitando = false;
        if (Painel != null) Painel.SetActive(false);
        DialogoAberto = false;
        FrameFechamento = Time.frameCount;
        if (PausarJogoDuranteDialogo) Time.timeScale = 1f;
    }
}
