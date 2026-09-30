using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Personagem : MonoBehaviour
{
    [Header("HUD (arraste os Texts da UI)")]
    public Text QTD_TEXT_Moedas;
    public Text QTD_TEXT_Runa;
    public Text QTD_TEXT_Vida;
    public Text QTD_TEXT_Pontos;
    [Header("Vida")]
    public int VidaMaxima = 100;
    public int Vida;
    public float TempoInvencivel = 0.8f;          
    public int DanoContatoInimigoPadrao = 15;     
    public int DanoArmadilha = 35;               
    public int CuraPocao = 30;                    
    public float ForcaEmpurraoDano = 7f;
    [Header("Movimento")]
    public float VelocidadeNormal = 5.5f;
    public float VelocidadeCorrida = 9f;         
    public float VelocidadeBonus = 11f;          
    public float Aceleracao = 70f;
    public float Desaceleracao = 90f;
    [Range(0f, 1f)] public float ControleNoAr = 0.85f;
    [Header("Pulo")]
    public float PuloNormal = 13f;
    public float PuloBonus = 18f;
    public float GravidadeBase = 3.5f;
    public float MultiplicadorQueda = 1.5f;       
    public float VelocidadeQuedaMaxima = 22f;
    public float CoyoteTime = 0.12f;             
    public float JumpBufferTime = 0.15f;         
    public bool TemPuloDuplo;
    [Header("Checagem de chão")]
    public Transform CheckPe;
    public float raioCheck = 0.2f;
    public LayerMask CamadaDoChao;
    [Header("Dash (Q)")]
    public float VelocidadeDash = 20f;
    public float TempoEmDash = 0.15f;
    public float DashCooldown = 0.5f;
    [Header("Ataque corpo a corpo (F)")]
    public int DanoAtaqueJogador = 25;
    public float AlcanceAtaque = 1f;              
    public float RaioAtaque = 0.9f;              
    public float CooldownAtaque = 0.35f;
    public float ForcaRecuoAtaque = 6f;
    public float TempoRecuo = 0.12f;
    public float ForcaKnockbackInimigo = 8f;
    public int PontosInimigoSemVida = 50;         
    [Header("Projéteis (Z = rápido / Ctrl = forte)")]
    public GameObject PrefabProjetil1;            
    public GameObject PrefabProjetil2;           
    public bool PodeUsarProjetil = true;          
    public int DanoProjetil1 = 15;
    public int DanoProjetil2 = 30;
    public float VelocidadeProjetil = 10f;
    public float CooldownProjetil1 = 0.35f;
    public float CooldownProjetil2 = 2f;
    [Header("Pontuação")]
    public int PontosMoeda = 10;
    public int PontosRuna = 25;
    public int PontosBonus = 15;
    public int PontosChave = 50;
    [Header("Cenas")]
    public string ProximaCena = "Final";          
    public string CenaGameOver = "GameOver";
    [Header("Outros")]
    public Animator PlayerAnimator;
    public GameObject PuloDuplo;                  
    Rigidbody2D CorpoRigido;
    SpriteRenderer SpritePersonagem;
    Collider2D ColisorPersonagem;
    int QTD_Moedas;
    int QTD_Runa;
    bool PodePassarDeFase;
    string TagObjetoTocado;
    bool morto;
    float VelocidadePulo;
    bool BonusPulo;
    float TempoBonusPulo;
    bool BonusVelocidade;
    float TempoBonusVelocidade;
    const float DURACAO_BONUS = 5f;
    bool EstaNoChao;
    int ContadorDePulos;
    float coyoteCounter;
    float jumpBufferCounter;
    bool EstaEmDash;
    float DashTimer;
    float CooldownTimer;
    float DirecaoDoDash = 1f;
    bool PodeDashNoAr = true;
    bool EstaEmRecuo;
    float timerRecuo;
    float invencivelTimer;
    float timerAtaque;
    bool emHitStop;
    float timerProjetil1;
    float timerProjetil2;
    float timerPortal;
    Transform TransformObjetoCarregado;
    Rigidbody2D CorpoCarregado;
    RigidbodyType2D TipoOriginalCarregado;
    readonly HashSet<int> processados = new HashSet<int>();  
    readonly HashSet<int> itensSoltos = new HashSet<int>();  
    int ultimosPontosMostrados = -1;

    float Direcao
    {
        get { return (SpritePersonagem != null && SpritePersonagem.flipX) ? -1f : 1f; }
    }

    public bool EstaEmDashAtivo
    {
        get { return EstaEmDash; }
    }

    void Start()
    {
        Time.timeScale = 1f;

        CorpoRigido = GetComponent<Rigidbody2D>();
        CorpoRigido.gravityScale = GravidadeBase;
        CorpoRigido.freezeRotation = true;

        SpritePersonagem = GetComponent<SpriteRenderer>();
        ColisorPersonagem = GetComponent<Collider2D>();
        if (PlayerAnimator == null) PlayerAnimator = GetComponent<Animator>();

        Vida = VidaMaxima;
        VelocidadePulo = PuloNormal;
        QTD_Moedas = 0;
        QTD_Runa = 0;
        PodePassarDeFase = false;

        if (QTD_TEXT_Moedas != null) QTD_TEXT_Moedas.text = "0";
        if (QTD_TEXT_Runa != null) QTD_TEXT_Runa.text = "0";
        AtualizarHUDVida();

        DesativarAreasDeAtaqueAntigas();
        PrepararModeloProjetil(ref PrefabProjetil1, "projetil1");
        PrepararModeloProjetil(ref PrefabProjetil2, "projetil2");
    }

    void Update()
    {
        if (morto) return;

        if (CaixaDialogo.DialogoAberto || CaixaDialogo.FrameFechamento == Time.frameCount)
        {
            CorpoRigido.velocity = new Vector2(0f, CorpoRigido.velocity.y);
            return;
        }

        AtualizarTimers();
        LerChao();
        SoltarObjeto();
        Dash();
        MovimentoHorizontal();
        MovimentoPulo();
        AplicarGravidade();
        VerificarBonus();
        Atacar();
        LancarProjetil1();
        LancarProjetil2();
        AtualizarInvencibilidade();
        AtualizarHUDPontos();
    }

    void AtualizarTimers()
    {
        if (timerRecuo > 0f)
        {
            timerRecuo -= Time.deltaTime;
            if (timerRecuo <= 0f) EstaEmRecuo = false;
        }
        if (timerPortal > 0f) timerPortal -= Time.deltaTime;
    }

    void LerChao()
    {
        bool tocando = CheckPe != null &&
                       Physics2D.OverlapCircle(CheckPe.position, raioCheck, CamadaDoChao) != null;

    
        EstaNoChao = tocando && CorpoRigido.velocity.y <= 0.1f;

        if (EstaNoChao)
        {
            coyoteCounter = CoyoteTime;
            ContadorDePulos = 0;
            PodeDashNoAr = true;
        }
        else
        {
            coyoteCounter -= Time.deltaTime;
        }

        if (PlayerAnimator != null)
        {
            PlayerAnimator.SetBool("estaNoChao", EstaNoChao);
            PlayerAnimator.SetBool("estaCaindo", CorpoRigido.velocity.y < 0f && !EstaNoChao);
        }
    }

    void MovimentoHorizontal()
    {
        if (EstaEmDash || EstaEmRecuo) return;

        float entrada = Input.GetAxisRaw("Horizontal");
        bool segurandoCorrer = Input.GetKey(KeyCode.LeftShift);

        float velocidadeMax = segurandoCorrer ? VelocidadeCorrida : VelocidadeNormal;
        if (BonusVelocidade) velocidadeMax = Mathf.Max(velocidadeMax, VelocidadeBonus);

        float alvo = entrada * velocidadeMax;
        float taxa = Mathf.Abs(entrada) > 0.01f ? Aceleracao : Desaceleracao;
        if (!EstaNoChao) taxa *= ControleNoAr;

        float novoX = Mathf.MoveTowards(CorpoRigido.velocity.x, alvo, taxa * Time.deltaTime);
        CorpoRigido.velocity = new Vector2(novoX, CorpoRigido.velocity.y);

        if (entrada != 0f && SpritePersonagem != null) SpritePersonagem.flipX = entrada < 0f;

        if (PlayerAnimator != null)
        {
            bool estaSeMovendo = entrada != 0f;
            PlayerAnimator.SetBool("estaAndando", estaSeMovendo && !segurandoCorrer);
            PlayerAnimator.SetBool("estaCorrendo", estaSeMovendo && segurandoCorrer);
        }
    }

    void MovimentoPulo()
    {
        if (Input.GetKeyDown(KeyCode.Space)) jumpBufferCounter = JumpBufferTime;
        else jumpBufferCounter -= Time.deltaTime;

        if (EstaEmDash) return;

        if (jumpBufferCounter > 0f)
        {
            if (coyoteCounter > 0f && ContadorDePulos == 0)
            {
                Pular(VelocidadePulo);
                ContadorDePulos = 1;
            }
            else if (TemPuloDuplo && ContadorDePulos < 2 && !EstaNoChao)
            {
                Pular(VelocidadePulo * 0.85f);
                ContadorDePulos = 2;
            }
        }

        if (Input.GetKeyUp(KeyCode.Space) && CorpoRigido.velocity.y > 0f)
        {
            CorpoRigido.velocity = new Vector2(CorpoRigido.velocity.x, CorpoRigido.velocity.y * 0.5f);
        }
    }

    void Pular(float forca)
    {
        CorpoRigido.velocity = new Vector2(CorpoRigido.velocity.x, forca);
        jumpBufferCounter = 0f;
        coyoteCounter = 0f;
        EstaNoChao = false;
        if (PlayerAnimator != null) PlayerAnimator.SetTrigger("pular");
    }

    void AplicarGravidade()
    {
        if (EstaEmDash)
        {
            CorpoRigido.gravityScale = 0f;
            return;
        }

        CorpoRigido.gravityScale = CorpoRigido.velocity.y < -0.01f
            ? GravidadeBase * MultiplicadorQueda
            : GravidadeBase;

        if (CorpoRigido.velocity.y < -VelocidadeQuedaMaxima)
        {
            CorpoRigido.velocity = new Vector2(CorpoRigido.velocity.x, -VelocidadeQuedaMaxima);
        }
    }

    void Dash()
    {
        if (CooldownTimer > 0f) CooldownTimer -= Time.deltaTime;

        if (!EstaEmDash && Input.GetKeyDown(KeyCode.Q) && CooldownTimer <= 0f && (EstaNoChao || PodeDashNoAr))
        {
            EstaEmDash = true;
            DashTimer = TempoEmDash;
            CooldownTimer = DashCooldown;
            DirecaoDoDash = Direcao;
            if (!EstaNoChao) PodeDashNoAr = false;
            EstaEmRecuo = false;
            timerRecuo = 0f;
        }

        if (EstaEmDash)
        {
            DashTimer -= Time.deltaTime;
            CorpoRigido.velocity = new Vector2(DirecaoDoDash * VelocidadeDash, 0f);

            if (DashTimer <= 0f)
            {
                EstaEmDash = false;
                CorpoRigido.velocity = new Vector2(DirecaoDoDash * VelocidadeNormal, 0f);
            }
        }
    }

    public void AplicarEmpurrao(Vector2 velocidade, float duracao)
    {
        if (morto) return;
        EstaEmDash = false;
        CorpoRigido.velocity = velocidade;
        EstaEmRecuo = true;
        timerRecuo = Mathf.Max(timerRecuo, duracao);
    }

    void VerificarBonus()
    {
        if (BonusPulo)
        {
            TempoBonusPulo -= Time.deltaTime;
            if (TempoBonusPulo <= 0f)
            {
                BonusPulo = false;
                VelocidadePulo = PuloNormal;
            }
        }

        if (BonusVelocidade)
        {
            TempoBonusVelocidade -= Time.deltaTime;
            if (TempoBonusVelocidade <= 0f) BonusVelocidade = false;
        }
    }

    public void ReceberDano(int dano)
    {
        ReceberDano(dano, (Vector2)transform.position + new Vector2(Direcao, 0f));
    }

    public void ReceberDano(int dano, Vector2 origemDoDano)
    {
        if (morto || Vida <= 0) return;
        if (invencivelTimer > 0f) return;

        int danoFinal = ConfiguracaoJogo.CalcularDanoRecebido(dano);
        Vida -= danoFinal;
        Debug.Log("Jogador levou " + danoFinal + " de dano. Vida: " + Vida);
        AtualizarHUDVida();

        if (PlayerAnimator != null) PlayerAnimator.SetTrigger("levouDano");

        if (Vida <= 0)
        {
            Morrer();
            return;
        }

        invencivelTimer = TempoInvencivel;

        float lado = Mathf.Sign(transform.position.x - origemDoDano.x);
        if (Mathf.Approximately(transform.position.x, origemDoDano.x)) lado = -Direcao;
        AplicarEmpurrao(new Vector2(lado * ForcaEmpurraoDano, ForcaEmpurraoDano * 0.6f), 0.2f);
    }

    public void Curar(int quantidade)
    {
        Vida = Mathf.Min(VidaMaxima, Vida + quantidade);
        AtualizarHUDVida();
    }

    void Morrer()
    {
        if (morto) return;
        morto = true;
        Vida = 0;
        AtualizarHUDVida();
        Time.timeScale = 1f;
        EstadoJogo.FaseParaReiniciar = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(CenaGameOver);
    }

    void AtualizarInvencibilidade()
    {
        if (SpritePersonagem == null) return;
        Color cor = SpritePersonagem.color;

        if (invencivelTimer > 0f)
        {
            invencivelTimer -= Time.deltaTime;
            cor.a = (Mathf.FloorToInt(Time.time * 15f) % 2 == 0) ? 0.35f : 1f;
            if (invencivelTimer <= 0f) cor.a = 1f;
            SpritePersonagem.color = cor;
        }
        else if (cor.a != 1f)
        {
            cor.a = 1f;
            SpritePersonagem.color = cor;
        }
    }

    void AtualizarHUDVida()
    {
        if (QTD_TEXT_Vida != null) QTD_TEXT_Vida.text = Vida.ToString();
    }

    void AtualizarHUDPontos()
    {
        if (QTD_TEXT_Pontos == null) return;
        int pontos = GerenciadorPontuacao.Instancia.PontosAtuais;
        if (pontos != ultimosPontosMostrados)
        {
            ultimosPontosMostrados = pontos;
            QTD_TEXT_Pontos.text = pontos.ToString();
        }
    }

    void OnCollisionEnter2D(Collision2D colisao)
    {
        if (morto) return;
        GameObject obj = colisao.collider.gameObject;
        TagObjetoTocado = obj.tag;

        if (TentarColetar(obj)) return;
        if (VerificarPisaoNaCabeca(obj, colisao.collider)) return;
        VerificarDestrutivel(obj);
        VerificarBau(obj);

        if (TagObjetoTocado == "Morte")
        {
            if (!EstaEmDash) ReceberDano(DanoArmadilha, obj.transform.position);
            return;
        }

        if (EhInimigoDeContato(obj, true)) LevarDanoDeContato(obj);
    }

    void OnCollisionStay2D(Collision2D colisao)
    {
        if (morto) return;
        GameObject obj = colisao.collider.gameObject;

        TentarCarregar(obj);

        if (invencivelTimer > 0f || EstaEmDash) return;
        if (obj.tag == "Morte") ReceberDano(DanoArmadilha, obj.transform.position);
        else if (EhInimigoDeContato(obj, true)) LevarDanoDeContato(obj);
    }

    void OnTriggerEnter2D(Collider2D outro)
    {
        if (morto) return;
        GameObject obj = outro.gameObject;
        TagObjetoTocado = obj.tag;

        if (TentarColetar(obj)) return;

        if (TagObjetoTocado == "Armadilha")
        {
            GameObject bola = null;
            try { bola = GameObject.FindGameObjectWithTag("Morte"); } catch (UnityException) { }
            if (bola != null)
            {
                Rigidbody2D corpoBola = bola.GetComponent<Rigidbody2D>();
                if (corpoBola != null) corpoBola.gravityScale = 1.5f;
            }
        }
        else if (TagObjetoTocado == "Final")
        {
            if (PodePassarDeFase) SceneManager.LoadScene(ProximaCena);
        }
        else if (TagObjetoTocado == "GameOver")
        {
            Morrer();
        }
        else if (TagObjetoTocado == "Portal")
        {
            Teleportar("Portal2");
        }
        else if (TagObjetoTocado == "Portal2")
        {
            Teleportar("Portal");
        }
        else if (!EstaEmDash && EhInimigoDeContato(obj, false))
        {
            LevarDanoDeContato(obj);
        }
    }

    void OnTriggerStay2D(Collider2D outro)
    {
        if (morto || invencivelTimer > 0f || EstaEmDash) return;
        if (EhInimigoDeContato(outro.gameObject, false)) LevarDanoDeContato(outro.gameObject);
    }

    bool JaProcessado(GameObject obj)
    {
        return !processados.Add(obj.GetInstanceID());
    }

    bool TentarColetar(GameObject obj)
    {
        string t = obj.tag;
        bool coletavel = t == "Moeda" || t == "Runa" || t == "BonusPulo" || t == "BonusVelocidade" ||
                         t == "BonusPuloDuplo" || t == "Bonus1" || t == "PocaoVida" ||
                         t == "ChavePassaFase" || t == "BonusProjetil";
        if (!coletavel) return false;
        if (JaProcessado(obj)) return true;

        int pontos = PontosBonus;

        switch (t)
        {
            case "Moeda":
                QTD_Moedas++;
                if (QTD_TEXT_Moedas != null) QTD_TEXT_Moedas.text = QTD_Moedas.ToString();
                pontos = PontosMoeda;
                break;
            case "Runa":
                QTD_Runa++;
                if (QTD_TEXT_Runa != null) QTD_TEXT_Runa.text = QTD_Runa.ToString();
                pontos = PontosRuna;
                break;
            case "BonusPulo":
                BonusPulo = true;
                TempoBonusPulo = DURACAO_BONUS;
                VelocidadePulo = PuloBonus;
                break;
            case "BonusVelocidade":
                BonusVelocidade = true;
                TempoBonusVelocidade = DURACAO_BONUS;
                break;
            case "BonusPuloDuplo":
                TemPuloDuplo = true;
                break;
            case "BonusProjetil":
                PodeUsarProjetil = true;
                break;
            case "Bonus1":
            case "PocaoVida":
                Curar(CuraPocao);
                break;
            case "ChavePassaFase":
                PodePassarDeFase = true;
                pontos = PontosChave;
                break;
        }

        GerenciadorPontuacao.Instancia.AdicionarPontosItem(pontos);
        RemoverObjeto(obj);
        return true;
    }

    void RemoverObjeto(GameObject obj)
    {
        foreach (Collider2D c in obj.GetComponentsInChildren<Collider2D>()) c.enabled = false;
        Destroy(obj);
    }

    bool EhCabecaDeInimigo(string t)
    {
        return t == "DestruirInimigos" || t.Contains("destruirInimigo");
    }

    bool EhInimigoDeContato(GameObject obj, bool aceitarPeloComponente)
    {
        string t = obj.tag;
        if (EhCabecaDeInimigo(t)) return false;
        if (obj.GetComponentInParent<BossMedusa>() != null) return false; 

        if (t == "Inimigo" || t == "inimigo" || t == "inimigo01" || t == "inimigo02") return true;

        if (!aceitarPeloComponente) return false;
        VidaInimigo vida = obj.GetComponentInParent<VidaInimigo>();
        return vida != null && !vida.EstaMorto && vida.DanoContato > 0;
    }

    void LevarDanoDeContato(GameObject inimigo)
    {
        if (EstaEmDash) return;
        VidaInimigo vida = inimigo.GetComponentInParent<VidaInimigo>();
        if (vida != null && vida.EstaMorto) return;

        int dano = vida != null ? vida.DanoContato : DanoContatoInimigoPadrao;
        ReceberDano(dano, inimigo.transform.position);
    }

    bool VerificarPisaoNaCabeca(GameObject cabeca, Collider2D colisorCabeca)
    {
        string t = cabeca.tag;
        if (!EhCabecaDeInimigo(t)) return false;

        if (ColisorPersonagem != null &&
            ColisorPersonagem.bounds.min.y < colisorCabeca.bounds.center.y - 0.15f)
        {
            return true;
        }

        if (JaProcessado(cabeca)) return true;
        colisorCabeca.enabled = false;

        Transform pai = cabeca.transform.parent;
        GameObject inimigo = pai != null ? pai.gameObject : cabeca;

        VidaInimigo vida = inimigo.GetComponentInParent<VidaInimigo>();
        if (vida != null)
        {
            vida.Morrer(); 
        }
        else
        {
            if (t.Contains("destruirInimigo02") && pai != null && pai.childCount > 1)
            {
                GameObject item = pai.GetChild(1).gameObject;
                if (item != cabeca) SoltarItem(item, new Vector2(2f, 5f), "Bonus1");
            }
            GerenciadorPontuacao.Instancia.AdicionarPontosInimigo(PontosInimigoSemVida);
            Destroy(inimigo);
        }

        CorpoRigido.velocity = new Vector2(CorpoRigido.velocity.x, PuloNormal * 0.8f);
        ContadorDePulos = 1;
        coyoteCounter = 0f;
        return true;
    }

    void VerificarDestrutivel(GameObject obj)
    {
        string t = obj.tag;

        if (t.Contains("destrutivel2"))
        {
            if (JaProcessado(obj)) return;
            if (obj.transform.childCount >= 2)
            {
                GameObject quadradoBranco = obj.transform.GetChild(0).gameObject;
                GameObject circuloDestrutivel = obj.transform.GetChild(1).gameObject;
                quadradoBranco.transform.parent = null;
                SoltarItem(circuloDestrutivel, new Vector2(0f, 5f), null);
                Destroy(quadradoBranco);
            }
            Destroy(obj);
        }
        else if (t.Contains("destrutivel1"))
        {
            if (JaProcessado(obj)) return;
            if (obj.transform.childCount >= 1)
            {
                SoltarItem(obj.transform.GetChild(0).gameObject, new Vector2(0f, 5f), null);
            }
            Destroy(obj);
        }
    }

    void VerificarBau(GameObject obj)
    {
        if (obj.tag != "Bau1") return;
        if (JaProcessado(obj)) return;

        GameObject pocao = PuloDuplo;
        if (pocao == null)
        {
            try { pocao = GameObject.FindGameObjectWithTag("BonusPuloDuplo"); } catch (UnityException) { }
        }
        if (pocao != null) SoltarItem(pocao, new Vector2(-2f, 6f), null);
    }

    void SoltarItem(GameObject item, Vector2 velocidade, string novaTag)
    {
        if (item == null) return;
        if (!itensSoltos.Add(item.GetInstanceID())) return; 

        item.transform.parent = null;
        item.SetActive(true);

        bool temColisorSolido = false;
        foreach (Collider2D c in item.GetComponents<Collider2D>())
        {
            c.enabled = true;
            if (!c.isTrigger) temColisorSolido = true;
        }
        if (!temColisorSolido) item.AddComponent<BoxCollider2D>(); 

        Rigidbody2D corpo = item.GetComponent<Rigidbody2D>();
        if (corpo == null) corpo = item.AddComponent<Rigidbody2D>();
        corpo.bodyType = RigidbodyType2D.Dynamic;
        corpo.freezeRotation = true;
        corpo.velocity = velocidade;

        if (!string.IsNullOrEmpty(novaTag)) item.tag = novaTag;
    }

    void Teleportar(string tagDestino)
    {
        if (timerPortal > 0f) return; 

        GameObject destino = null;
        try { destino = GameObject.FindGameObjectWithTag(tagDestino); } catch (UnityException) { }
        if (destino == null) return;

        Vector3 posicao = destino.transform.position;
        transform.position = new Vector2(posicao.x + 2f, posicao.y);
        timerPortal = 0.5f;
    }

    void TentarCarregar(GameObject obj)
    {
        if (TransformObjetoCarregado != null) return;
        if (!obj.tag.Contains("Carregavel")) return;
        if (!Input.GetKey(KeyCode.E)) return;

        TransformObjetoCarregado = obj.transform;
        obj.transform.parent = transform;

        CorpoCarregado = obj.GetComponent<Rigidbody2D>();
        if (CorpoCarregado != null)
        {
            TipoOriginalCarregado = CorpoCarregado.bodyType;
            CorpoCarregado.bodyType = RigidbodyType2D.Kinematic;
            CorpoCarregado.velocity = Vector2.zero;
            CorpoCarregado.angularVelocity = 0f;
        }
        DefinirColisaoComCarregado(true);
    }

    void SoltarObjeto()
    {
        if (TransformObjetoCarregado == null) return;
        if (Input.GetKey(KeyCode.E)) return;

        DefinirColisaoComCarregado(false);
        TransformObjetoCarregado.parent = null;

        if (CorpoCarregado != null)
        {
            CorpoCarregado.bodyType = TipoOriginalCarregado;
            CorpoCarregado.velocity = CorpoRigido.velocity;
        }

        TransformObjetoCarregado = null;
        CorpoCarregado = null;
    }

    void DefinirColisaoComCarregado(bool ignorar)
    {
        if (TransformObjetoCarregado == null) return;
        foreach (Collider2D colisorObjeto in TransformObjetoCarregado.GetComponentsInChildren<Collider2D>())
        {
            foreach (Collider2D meu in GetComponents<Collider2D>())
            {
                if (colisorObjeto.enabled && meu.enabled) Physics2D.IgnoreCollision(colisorObjeto, meu, ignorar);
            }
        }
    }

    void Atacar()
    {
        if (timerAtaque > 0f) timerAtaque -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.F) && timerAtaque <= 0f)
        {
            timerAtaque = CooldownAtaque;
            if (PlayerAnimator != null) PlayerAnimator.SetTrigger("atacar");
            ExecutarAtaque();
        }
    }

    void ExecutarAtaque()
    {
        Vector2 centro = (Vector2)transform.position + new Vector2(Direcao * AlcanceAtaque, 0f);
        Collider2D[] acertos = Physics2D.OverlapCircleAll(centro, RaioAtaque);

        HashSet<GameObject> jaAcertados = new HashSet<GameObject>();
        bool acertouAlgo = false;

        foreach (Collider2D colisor in acertos)
        {
            GameObject alvo = ObterAlvoDoAtaque(colisor);
            if (alvo == null || jaAcertados.Contains(alvo)) continue;
            jaAcertados.Add(alvo);
            AplicarGolpe(alvo);
            acertouAlgo = true;
        }

        if (acertouAlgo)
        {
            AplicarEmpurrao(new Vector2(-Direcao * ForcaRecuoAtaque, CorpoRigido.velocity.y), TempoRecuo);
            StartCoroutine(HitStop(0.05f));
        }
    }

    GameObject ObterAlvoDoAtaque(Collider2D colisor)
    {
        if (colisor == null || !colisor.enabled) return null;
        if (colisor.GetComponentInParent<Personagem>() != null) return null;

        IVida vida = colisor.GetComponentInParent<IVida>();
        if (vida != null)
        {
            VidaInimigo vidaInimigo = vida as VidaInimigo;
            if (vidaInimigo != null && vidaInimigo.EstaMorto) return null;
            Component componente = vida as Component;
            return componente != null ? componente.gameObject : null;
        }

        string t = colisor.tag;
        if (t == "inimigo" || t == "Inimigo" || t == "inimigo01" || t == "inimigo02" || t == "Boss1")
        {
            return colisor.gameObject;
        }
        return null;
    }

    void AplicarGolpe(GameObject alvo)
    {
        int dano = ConfiguracaoJogo.CalcularDanoJogador(DanoAtaqueJogador);
        IVida vida = alvo.GetComponent<IVida>();
        bool ehBoss = alvo.GetComponent<BossMedusa>() != null;
        bool temVidaInimigo = alvo.GetComponent<VidaInimigo>() != null;

        if (!ehBoss && !temVidaInimigo)
        {
            SpriteRenderer sprite = alvo.GetComponent<SpriteRenderer>();
            if (sprite != null) StartCoroutine(PiscarDano(sprite));
        }

        IAtordoavel atordoavel = alvo.GetComponent<IAtordoavel>();
        if (atordoavel != null) atordoavel.Atordoar(0.3f);

        if (!ehBoss)
        {
            Rigidbody2D corpoInimigo = alvo.GetComponent<Rigidbody2D>();
            if (corpoInimigo != null && corpoInimigo.bodyType == RigidbodyType2D.Dynamic)
            {
                corpoInimigo.velocity = new Vector2(Direcao * ForcaKnockbackInimigo, ForcaKnockbackInimigo * 0.5f);
            }
        }

        if (vida != null)
        {
            vida.ReceberDano(dano);
        }
        else
        {
            Collider2D colisorInimigo = alvo.GetComponent<Collider2D>();
            if (colisorInimigo != null) colisorInimigo.enabled = false;
            GerenciadorPontuacao.Instancia.AdicionarPontosInimigo(PontosInimigoSemVida);
            Destroy(alvo, 0.25f);
        }
    }

    IEnumerator PiscarDano(SpriteRenderer sprite)
    {
        if (sprite.color == Color.red) yield break;
        Color corOriginal = sprite.color;
        sprite.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        if (sprite != null) sprite.color = corOriginal;
    }

    IEnumerator HitStop(float duracao)
    {
        if (emHitStop || Time.timeScale == 0f) yield break;
        emHitStop = true;
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duracao);
        if (!CaixaDialogo.DialogoAberto) Time.timeScale = 1f;
        emHitStop = false;
    }

    void DesativarAreasDeAtaqueAntigas()
    {
        DesativarColisoresPorTag("areaAtaqueEsquerda");
        DesativarColisoresPorTag("areaAtaqueDireita");
    }

    void DesativarColisoresPorTag(string tag)
    {
        try
        {
            GameObject area = GameObject.FindGameObjectWithTag(tag);
            if (area != null)
            {
                foreach (Collider2D c in area.GetComponents<Collider2D>()) c.enabled = false;
            }
        }
        catch (UnityException) { }
    }


    void PrepararModeloProjetil(ref GameObject modelo, string tag)
    {
        if (modelo == null)
        {
            try { modelo = GameObject.FindGameObjectWithTag(tag); } catch (UnityException) { modelo = null; }
        }
        if (modelo != null && modelo.scene.IsValid()) modelo.SetActive(false);
    }

    void LancarProjetil1()
    {
        if (timerProjetil1 > 0f) timerProjetil1 -= Time.deltaTime;
        if (!PodeUsarProjetil) return;

        if (Input.GetKeyDown(KeyCode.Z) && timerProjetil1 <= 0f)
        {
            timerProjetil1 = CooldownProjetil1;
            Disparar(PrefabProjetil1, DanoProjetil1);
        }
    }

    void LancarProjetil2()
    {
        if (timerProjetil2 > 0f) timerProjetil2 -= Time.deltaTime;
        if (!PodeUsarProjetil) return;

        if (Input.GetKey(KeyCode.LeftControl) && timerProjetil2 <= 0f)
        {
            timerProjetil2 = CooldownProjetil2;
            Disparar(PrefabProjetil2, DanoProjetil2);
        }
    }

    void Disparar(GameObject modelo, int danoBase)
    {
        if (modelo == null) return;

        Vector2 posicao = (Vector2)transform.position + new Vector2(Direcao * 1f, 0f);
        GameObject copia = Instantiate(modelo, posicao, modelo.transform.rotation);
        copia.SetActive(true);
        foreach (Collider2D colisorProjetil in copia.GetComponentsInChildren<Collider2D>())
        {
            foreach (Collider2D meu in GetComponentsInChildren<Collider2D>())
            {
                if (colisorProjetil.enabled && meu.enabled) Physics2D.IgnoreCollision(colisorProjetil, meu, true);
            }
        }

        Projetil scriptProjetil = copia.GetComponent<Projetil>();
        if (scriptProjetil == null) scriptProjetil = copia.AddComponent<Projetil>();
        scriptProjetil.Dano = ConfiguracaoJogo.CalcularDanoJogador(danoBase);
        scriptProjetil.PontosInimigoSemVida = PontosInimigoSemVida;
        // O próprio Projetil agora se movimenta sozinho com essa velocidade
        scriptProjetil.Velocidade = new Vector2(VelocidadeProjetil * Direcao, 0f);

        SpriteRenderer spriteProjetil = copia.GetComponent<SpriteRenderer>();
        if (spriteProjetil != null) spriteProjetil.flipX = Direcao < 0f;

        Destroy(copia, 3.5f);
    }

    void OnDrawGizmos()
    {
        if (CheckPe != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(CheckPe.position, raioCheck);
        }
    }

    void OnDrawGizmosSelected()
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        float direcao = (sprite != null && sprite.flipX) ? -1f : 1f;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + new Vector3(direcao * AlcanceAtaque, 0f, 0f), RaioAtaque);
    }
}