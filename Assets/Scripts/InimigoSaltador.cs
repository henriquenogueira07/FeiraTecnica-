using UnityEngine;

public class InimigoSaltador : MonoBehaviour, IAtordoavel
{
    public float Alcance = 7f;
    public float TempoEntrePulos = 1.6f;
    public float ForcaPuloHorizontal = 4f;
    public float ForcaPuloVertical = 9f;

    [Header("Checagem de chão (opcional)")]
    public Transform CheckPe;
    public float RaioCheck = 0.15f;
    public LayerMask CamadaDoChao;

    Rigidbody2D corpo;
    SpriteRenderer sprite;
    Transform jogador;
    float timer;
    bool atordoado;

    void Start()
    {
        corpo = GetComponent<Rigidbody2D>();
        if (corpo != null)
        {
            corpo.bodyType = RigidbodyType2D.Dynamic;
            corpo.freezeRotation = true;
        }
        sprite = GetComponent<SpriteRenderer>();
        GameObject objJogador = GameObject.FindGameObjectWithTag("Player");
        if (objJogador != null) jogador = objJogador.transform;
        timer = TempoEntrePulos;
    }

    bool EstaNoChao()
    {
        if (CheckPe != null) return Physics2D.OverlapCircle(CheckPe.position, RaioCheck, CamadaDoChao) != null;
        return Mathf.Abs(corpo.velocity.y) < 0.05f;
    }

    void Update()
    {
        if (atordoado || jogador == null || corpo == null) return;
        if (!EstaNoChao()) return;

        // Freia ao encostar no chão
        corpo.velocity = new Vector2(Mathf.MoveTowards(corpo.velocity.x, 0f, 25f * Time.deltaTime), corpo.velocity.y);

        float distancia = Vector2.Distance(transform.position, jogador.position);
        if (distancia > Alcance)
        {
            timer = TempoEntrePulos;
            return;
        }

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = TempoEntrePulos;
            float lado = jogador.position.x < transform.position.x ? -1f : 1f;
            if (sprite != null) sprite.flipX = lado < 0f;
            corpo.velocity = new Vector2(lado * ForcaPuloHorizontal, ForcaPuloVertical);
        }
    }

    public void Atordoar(float duracao)
    {
        atordoado = true;
        CancelInvoke("Recuperar");
        Invoke("Recuperar", duracao);
    }

    void Recuperar()
    {
        atordoado = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, Alcance);
    }
}
