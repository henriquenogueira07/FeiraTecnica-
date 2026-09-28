using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BossMedusa : MonoBehaviour, IVida, IAtordoavel
{
    [Header("Vida")]
    public int VidaMaxima = 250;
    public int Vida;
    public Slider BarraDeVida; 
    [Header("Detecção")]
    public float RangeDeteccao = 8f;
    public Transform Jogador;
    [Header("Ataque de corrida")]
    public float VelocidadeCorrida = 10f;
    public float TempoDeCorrida = 0.6f;
    public int DanoCorrida = 15;
    [Header("Ataque de projétil (idle 2)")]
    public GameObject PrefabProjetilBoss;
    public Transform PontoDisparo;
    public float VelocidadeProjetilBoss = 8f;
    public int DanoProjetil = 10;
    [Header("Ataque Special (puxão)")]
    public float TempoDeAvisoSpecial = 0.5f;
    public float RaioDoPuxao = 3.5f;
    public float ForcaDoPuxao = 16f;
    public float RaioDeContatoSpecial = 1.3f;
    public int DanoSpecial = 20;
    [Header("Geral")]
    public float TempoEntreAtaques = 3f;
    [Header("Fase 2 (fúria) - ativa com pouca vida")]
    [Range(0f, 1f)] public float PorcentagemVidaFase2 = 0.5f;
    public float MultiplicadorVelocidadeFase2 = 1.3f;
    public float MultiplicadorTempoEntreAtaquesFase2 = 0.65f;
    public Color CorFase2 = new Color(1f, 0.65f, 0.65f);
    [Header("Derrota")]
    public int PontosAoDerrotar = 500;
    public string CenaAposDerrota = "Final"; 
    public float EsperaAntesDaCena = 0.5f;   
    [Header("Efeito de morte (tremida + tela branca)")]
    public Sprite SpriteMedusaMorta;         
    public Image TelaBranca;               
    public Camera CameraDoJogo;              
    public float DuracaoTremida = 0.4f;
    public float IntensidadeTremida = 0.2f;
    public float DuracaoFadeParaBranco = 1f;
    public float DuracaoTelaTotalmenteBranca = 3f;
    public float DuracaoFadeDeVolta = 1f;
    public Vector2 DeslocamentoAoMorrer = new Vector2(0f, -0.5f); 
    Animator BossAnimator;
    Rigidbody2D CorpoRigido;
    SpriteRenderer SpriteBoss;
    Color corBase = Color.white;
    bool estaAtacando;
    bool estaAtordoada;
    bool morta;
    bool emFase2;
    float timerProximoAtaque;
    string ataqueAtual = "";
    int ultimoAtaque = -1;
    Coroutine rotinaAtordoar;
    Coroutine rotinaPiscar;
    public bool EstaMorta
    {
        get { return morta; }
    }

    void Start()
    {
        Vida = VidaMaxima;
        BossAnimator = GetComponent<Animator>();
        CorpoRigido = GetComponent<Rigidbody2D>();
        SpriteBoss = GetComponent<SpriteRenderer>();
        if (SpriteBoss != null) corBase = SpriteBoss.color;

        if (Jogador == null)
        {
            GameObject objJogador = GameObject.FindGameObjectWithTag("Player");
            if (objJogador != null) Jogador = objJogador.transform;
        }

        timerProximoAtaque = TempoEntreAtaques;
        AtualizarBarra();
    }

    void Update()
    {
        if (morta || Jogador == null) return;
        if (estaAtacando) return;

        float distancia = Vector2.Distance(transform.position, Jogador.position);
        bool jogadorPerto = distancia <= RangeDeteccao;

        if (jogadorPerto) timerProximoAtaque -= Time.deltaTime;

        if (estaAtordoada) return;

        VirarParaJogador();

        if (jogadorPerto && timerProximoAtaque <= 0f)
        {
            EscolherEIniciarAtaque();
            timerProximoAtaque = TempoAtualEntreAtaques();
        }
    }

    float TempoAtualEntreAtaques()
    {
        return emFase2 ? TempoEntreAtaques * MultiplicadorTempoEntreAtaquesFase2 : TempoEntreAtaques;
    }

    float MultiplicadorVelocidade()
    {
        return emFase2 ? MultiplicadorVelocidadeFase2 : 1f;
    }

    void VirarParaJogador()
    {
        if (SpriteBoss == null) return;
        SpriteBoss.flipX = Jogador.position.x < transform.position.x;
    }

    void EscolherEIniciarAtaque()
    {
        int escolha = Random.Range(0, 3);
        if (escolha == ultimoAtaque) escolha = Random.Range(0, 3);
        ultimoAtaque = escolha;

        if (escolha == 0) StartCoroutine(AtaqueCorrida());
        else if (escolha == 1) StartCoroutine(AtaqueProjetil());
        else StartCoroutine(AtaqueSpecial());
    }

    IEnumerator AtaqueCorrida()
    {
        estaAtacando = true;
        ataqueAtual = "corrida";
        if (BossAnimator != null) BossAnimator.SetBool("AtaqueCorrida", true);

        yield return new WaitForSeconds(0.2f);

        Vector2 direcao = (Jogador.position - transform.position).normalized;
        float velocidade = VelocidadeCorrida * MultiplicadorVelocidade();
        float tempo = 0f;

        while (tempo < TempoDeCorrida && !morta)
        {
            CorpoRigido.velocity = new Vector2(direcao.x * velocidade, CorpoRigido.velocity.y);
            tempo += Time.deltaTime;
            yield return null;
        }
        CorpoRigido.velocity = new Vector2(0f, CorpoRigido.velocity.y);

        yield return new WaitForSeconds(0.3f);
        ataqueAtual = "";
        estaAtacando = false;
        if (BossAnimator != null) BossAnimator.SetBool("AtaqueCorrida", false);
    }

    IEnumerator AtaqueProjetil()
    {
        estaAtacando = true;
        if (BossAnimator != null) BossAnimator.SetBool("AtaqueProjetil", true);

        yield return new WaitForSeconds(0.6f);

        int quantidadeTiros = emFase2 ? 2 : 1; // na fase 2 ela atira 2 vezes
        for (int i = 0; i < quantidadeTiros && !morta; i++)
        {
            DispararProjetil();
            if (i < quantidadeTiros - 1) yield return new WaitForSeconds(0.35f);
        }

        yield return new WaitForSeconds(0.4f);
        if (BossAnimator != null) BossAnimator.SetBool("AtaqueProjetil", false);
        estaAtacando = false;
    }

    void DispararProjetil()
    {
        if (PrefabProjetilBoss == null || Jogador == null) return;

        Vector3 origem = PontoDisparo != null ? PontoDisparo.position : transform.position;
        GameObject copiaProjetil = Instantiate(PrefabProjetilBoss, origem, Quaternion.identity);
        copiaProjetil.SetActive(true);

        ProjetilBoss scriptProjetil = copiaProjetil.GetComponent<ProjetilBoss>();
        if (scriptProjetil == null) scriptProjetil = copiaProjetil.AddComponent<ProjetilBoss>();
        scriptProjetil.Dano = DanoProjetil;
        scriptProjetil.Dono = gameObject;

        Rigidbody2D corpoProjetil = copiaProjetil.GetComponent<Rigidbody2D>();
        if (corpoProjetil == null)
        {
            corpoProjetil = copiaProjetil.AddComponent<Rigidbody2D>();
            corpoProjetil.gravityScale = 0f;
        }

        Vector2 direcao = ((Vector2)(Jogador.position - origem)).normalized;
        corpoProjetil.velocity = direcao * VelocidadeProjetilBoss * MultiplicadorVelocidade();

        Destroy(copiaProjetil, 4f);
    }

    IEnumerator AtaqueSpecial()
    {
        estaAtacando = true;
        ataqueAtual = "special";
        if (BossAnimator != null) BossAnimator.SetBool("AtaquePuxao", true);

        yield return new WaitForSeconds(TempoDeAvisoSpecial);

        if (!morta && Jogador != null && Vector2.Distance(transform.position, Jogador.position) <= RaioDoPuxao)
        {
            Personagem scriptJogador = Jogador.GetComponent<Personagem>();
            bool jogadorEmDash = scriptJogador != null && scriptJogador.EstaEmDashAtivo;

            if (scriptJogador != null && !jogadorEmDash)
            {
                Vector2 direcaoPuxao = (transform.position - Jogador.position).normalized;
                scriptJogador.AplicarEmpurrao(direcaoPuxao * ForcaDoPuxao, 0.25f);
            }

            yield return new WaitForSeconds(0.25f);

            if (!morta && !jogadorEmDash && scriptJogador != null &&
                Vector2.Distance(transform.position, Jogador.position) <= RaioDeContatoSpecial)
            {
                scriptJogador.ReceberDano(DanoSpecial, transform.position);
            }
        }

        yield return new WaitForSeconds(0.5f);
        ataqueAtual = "";
        estaAtacando = false;
        if (BossAnimator != null) BossAnimator.SetBool("AtaquePuxao", false);
    }

    void OnCollisionEnter2D(Collision2D colisao) { VerificarContatoComJogador(colisao.collider); }
    void OnCollisionStay2D(Collision2D colisao) { VerificarContatoComJogador(colisao.collider); }
    void OnTriggerEnter2D(Collider2D objetoTocado) { VerificarContatoComJogador(objetoTocado); }
    void OnTriggerStay2D(Collider2D objetoTocado) { VerificarContatoComJogador(objetoTocado); }

    void VerificarContatoComJogador(Collider2D objetoTocado)
    {
        if (morta || ataqueAtual != "corrida" || objetoTocado == null) return;

        Personagem scriptJogador = objetoTocado.GetComponentInParent<Personagem>();
        if (scriptJogador == null || scriptJogador.EstaEmDashAtivo) return;

        scriptJogador.ReceberDano(DanoCorrida, transform.position);
        ataqueAtual = ""; // só 1 hit por corrida
    }

    public void ReceberDano(int dano)
    {
        if (morta) return;

        Vida -= dano;
        Debug.Log("Boss levou " + dano + " de dano. Vida atual: " + Vida);
        if (BossAnimator != null) BossAnimator.SetTrigger("TomouDano");

        if (!emFase2 && Vida > 0 && Vida <= VidaMaxima * PorcentagemVidaFase2)
        {
            emFase2 = true;
            corBase = CorFase2;
            Debug.Log("Boss entrou na FASE 2!");
        }

        if (SpriteBoss != null)
        {
            if (rotinaPiscar != null) StopCoroutine(rotinaPiscar);
            rotinaPiscar = StartCoroutine(Piscar());
        }

        AtualizarBarra();

        if (Vida <= 0)
        {
            Vida = 0;
            AtualizarBarra();
            Morrer();
        }
    }

    IEnumerator Piscar()
    {
        SpriteBoss.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        if (SpriteBoss != null) SpriteBoss.color = corBase;
    }

    void AtualizarBarra()
    {
        if (BarraDeVida == null) return;
        BarraDeVida.minValue = 0;
        BarraDeVida.maxValue = VidaMaxima;
        BarraDeVida.value = Vida;
    }

    void Morrer()
    {
        morta = true;
        StopAllCoroutines(); 

        if (SpriteBoss != null) SpriteBoss.color = corBase;
        if (CorpoRigido != null)
        {
            CorpoRigido.velocity = Vector2.zero;
            CorpoRigido.gravityScale = 0f;
            CorpoRigido.bodyType = RigidbodyType2D.Kinematic; 
        }

        if (BossAnimator != null) BossAnimator.enabled = false;

        GerenciadorPontuacao.Instancia.AdicionarPontosInimigo(PontosAoDerrotar);

        StartCoroutine(EfeitoMorteBoss()); 
    }

    IEnumerator EfeitoMorteBoss()
    {
        yield return StartCoroutine(TremerCamera());

        if (TelaBranca != null)
        {
            yield return StartCoroutine(FadeTela(0f, 1f, DuracaoFadeParaBranco));
        }

        if (SpriteBoss != null && SpriteMedusaMorta != null)
        {
            SpriteBoss.sprite = SpriteMedusaMorta;
            transform.position += (Vector3)DeslocamentoAoMorrer;
        }

        yield return new WaitForSecondsRealtime(DuracaoTelaTotalmenteBranca);

        if (TelaBranca != null)
        {
            yield return StartCoroutine(FadeTela(1f, 0f, DuracaoFadeDeVolta));
        }

        if (!string.IsNullOrEmpty(CenaAposDerrota))
        {
            yield return new WaitForSecondsRealtime(EsperaAntesDaCena);
            SceneManager.LoadScene(CenaAposDerrota);
        }
    }

    IEnumerator TremerCamera()
    {
        Camera camera = CameraDoJogo != null ? CameraDoJogo : Camera.main;
        if (camera == null) yield break;

        Vector3 posicaoOriginal = camera.transform.localPosition;
        float tempo = 0f;

        while (tempo < DuracaoTremida)
        {
            float x = Random.Range(-1f, 1f) * IntensidadeTremida;
            float y = Random.Range(-1f, 1f) * IntensidadeTremida;
            camera.transform.localPosition = posicaoOriginal + new Vector3(x, y, 0f);
            tempo += Time.unscaledDeltaTime;
            yield return null;
        }

        camera.transform.localPosition = posicaoOriginal;
    }

    IEnumerator FadeTela(float alphaDe, float alphaPara, float duracao)
    {
        if (TelaBranca == null) yield break;

        Color cor = TelaBranca.color;
        float tempo = 0f;

        while (tempo < duracao)
        {
            cor.a = Mathf.Lerp(alphaDe, alphaPara, tempo / duracao);
            TelaBranca.color = cor;
            tempo += Time.unscaledDeltaTime;
            yield return null;
        }

        cor.a = alphaPara;
        TelaBranca.color = cor;
    }

    public void Atordoar(float duracao)
    {
        if (morta) return;
        duracao = Mathf.Min(duracao, 0.1f);
        if (rotinaAtordoar != null) StopCoroutine(rotinaAtordoar);
        rotinaAtordoar = StartCoroutine(RotinaAtordoar(duracao));
    }

    IEnumerator RotinaAtordoar(float duracao)
    {
        estaAtordoada = true;
        if (CorpoRigido != null && !estaAtacando) CorpoRigido.velocity = new Vector2(0f, CorpoRigido.velocity.y);
        yield return new WaitForSeconds(duracao);
        estaAtordoada = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, RangeDeteccao);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, RaioDoPuxao);
    }
}