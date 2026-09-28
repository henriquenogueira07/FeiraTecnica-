using UnityEngine;

public class InimigoPerseguidor : MonoBehaviour, IAtordoavel
{
    SpriteRenderer PersonagemSpriteRenderer;
    Vector2 PosicaoInicial;
    float TempoDecorrido;

    public float Distancia;
    public float VelocidadeMovimento;
    public Vector2 Direcao;
    public GameObject Player;              
    public float MaximoPerto;
    public float VelocidadePerseguicao = 3f;
    public bool PerseguirNoEixoY = false;   

    Rigidbody2D CorpoRigido;
    bool EstaAtordoado;
    bool EstavaPerseguindo;

    void Start()
    {
        PersonagemSpriteRenderer = GetComponent<SpriteRenderer>();
        CorpoRigido = GetComponent<Rigidbody2D>();
        PosicaoInicial = transform.position;
        TempoDecorrido = 0f;
        EstaAtordoado = false;

        if (Player == null) Player = GameObject.FindGameObjectWithTag("Player");

        if (CorpoRigido != null)
        {
            CorpoRigido.bodyType = RigidbodyType2D.Kinematic;
            CorpoRigido.freezeRotation = true;
        }
    }

    void Update()
    {
        if (EstaAtordoado) return;

        if (Player == null)
        {
            MovimentoPingPong();
            return;
        }

        float distancia = Vector2.Distance(Player.transform.position, transform.position);

        if (distancia <= MaximoPerto)
        {
            SeguirPlayer();
            EstavaPerseguindo = true;
        }
        else
        {
            if (EstavaPerseguindo)
            {
                PosicaoInicial = transform.position;
                TempoDecorrido = 0f;
                EstavaPerseguindo = false;
            }
            MovimentoPingPong();
        }
    }

    void SeguirPlayer()
    {
        Vector3 alvo = Player.transform.position;
        if (!PerseguirNoEixoY) alvo.y = transform.position.y;

        Vector3 direcao = (alvo - transform.position).normalized;
        transform.position = Vector3.MoveTowards(transform.position, alvo, VelocidadePerseguicao * Time.deltaTime);

        if (PersonagemSpriteRenderer != null && Mathf.Abs(direcao.x) > 0.01f)
        {
            PersonagemSpriteRenderer.flipX = direcao.x < 0f;
        }
    }

    void MovimentoPingPong()
    {
        float tempoAnterior = TempoDecorrido;
        TempoDecorrido += Time.deltaTime * VelocidadeMovimento;

        float movimento = Mathf.PingPong(TempoDecorrido, Distancia);

        if (PersonagemSpriteRenderer != null)
        {
            PersonagemSpriteRenderer.flipX =
                Mathf.PingPong(tempoAnterior, Distancia) > Mathf.PingPong(TempoDecorrido, Distancia);
        }

        transform.position = PosicaoInicial + Direcao.normalized * movimento;
    }

    public void Atordoar(float duracao)
    {
        EstaAtordoado = true;

        if (CorpoRigido != null)
        {
            CorpoRigido.bodyType = RigidbodyType2D.Dynamic;
            CorpoRigido.gravityScale = PerseguirNoEixoY ? 0f : 3f;
        }

        CancelInvoke("VoltarAndar");
        Invoke("VoltarAndar", duracao);
    }

    void VoltarAndar()
    {
        EstaAtordoado = false;
        PosicaoInicial = transform.position;
        TempoDecorrido = 0f;

        if (CorpoRigido != null)
        {
            CorpoRigido.velocity = Vector2.zero;
            CorpoRigido.gravityScale = 0f;
            CorpoRigido.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, MaximoPerto);
    }
}
