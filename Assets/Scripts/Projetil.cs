using UnityEngine;

public class Projetil : MonoBehaviour
{
    public int Dano = 15;
    public int PontosInimigoSemVida = 50;
    public bool DestruirAoBaterNoCenario = true;
    public bool UsarGravidade = false;   

    [HideInInspector] public Vector2 Velocidade; 

    Rigidbody2D corpo;
    bool usado;

    void Start()
    {
        corpo = GetComponent<Rigidbody2D>();
        if (corpo == null) corpo = gameObject.AddComponent<Rigidbody2D>();

       
        corpo.bodyType = RigidbodyType2D.Dynamic;        
        corpo.simulated = true;
        corpo.constraints = RigidbodyConstraints2D.FreezeRotation; 
        corpo.gravityScale = UsarGravidade ? 1f : 0f;
        corpo.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        corpo.velocity = Velocidade;

        if (Velocidade == Vector2.zero)
        {
            Debug.LogWarning("Projetil criado com velocidade zero! Confira 'Velocidade Projetil' no Personagem.");
        }
    }

    void FixedUpdate()
    {
        if (usado || corpo == null) return;

        if (UsarGravidade)
        {
            corpo.velocity = new Vector2(Velocidade.x, corpo.velocity.y);
        }
        else
        {
            corpo.velocity = Velocidade;
        }
    }

    void OnCollisionEnter2D(Collision2D colisao) { Acertar(colisao.collider); }
    void OnTriggerEnter2D(Collider2D outro) { Acertar(outro); }

    void Acertar(Collider2D outro)
    {
        if (usado || outro == null) return;
        if (outro.GetComponentInParent<Personagem>() != null) return;

        IVida vida = outro.GetComponentInParent<IVida>();
        if (vida != null)
        {
            VidaInimigo vidaInimigo = vida as VidaInimigo;
            if (vidaInimigo != null && vidaInimigo.EstaMorto) return;

            usado = true;
            vida.ReceberDano(Dano);
            Destroy(gameObject);
            return;
        }

        string tag = outro.tag;
        if (tag == "inimigo01" || tag == "inimigo02" || tag == "inimigo" || tag == "Inimigo")
        {
            usado = true;
            outro.enabled = false;
            GerenciadorPontuacao.Instancia.AdicionarPontosInimigo(PontosInimigoSemVida);
            Destroy(outro.gameObject);
            Destroy(gameObject);
            return;
        }

        if (outro.isTrigger) return;
        if (outro.GetComponent<Projetil>() != null) return;

        if (DestruirAoBaterNoCenario)
        {
            usado = true;
            Destroy(gameObject);
        }
    }
}