using UnityEngine;

public class Inimigo : MonoBehaviour, IAtordoavel
{
    SpriteRenderer PersonagemSpriteRenderer;
    Vector2 PosicaoInicial;
    float TempoDecorrido;

    public float Distancia;
    public float VelocidadeMovimento;
    public Vector2 Direcao;

    Rigidbody2D CorpoRigido;
    bool EstaAtordoado;

    void Start()
    {
        PersonagemSpriteRenderer = GetComponent<SpriteRenderer>();
        CorpoRigido = GetComponent<Rigidbody2D>();
        PosicaoInicial = transform.position;
        TempoDecorrido = 0f;
        EstaAtordoado = false;

        if (CorpoRigido != null)
        {
            CorpoRigido.bodyType = RigidbodyType2D.Kinematic;
            CorpoRigido.freezeRotation = true;
        }
    }

    void Update()
    {
        if (EstaAtordoado) return;
        MovimentoPingPong();
    }

    void MovimentoPingPong()
    {
        float tempoAnterior = TempoDecorrido;
        TempoDecorrido += Time.deltaTime * VelocidadeMovimento;

        float movimento = Mathf.PingPong(TempoDecorrido, Distancia);

        if (Mathf.PingPong(tempoAnterior, Distancia) > Mathf.PingPong(TempoDecorrido, Distancia))
        {
            PersonagemSpriteRenderer.flipX = true;
        }
        else
        {
            PersonagemSpriteRenderer.flipX = false;
        }

        transform.position = PosicaoInicial + Direcao.normalized * movimento;
    }

    public void Atordoar(float duracao)
    {
        EstaAtordoado = true;

        if (CorpoRigido != null)
        {
            CorpoRigido.bodyType = RigidbodyType2D.Dynamic;
            CorpoRigido.gravityScale = 3f;
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
}