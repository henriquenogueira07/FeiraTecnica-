using UnityEngine;

public class InimigoAtirador : MonoBehaviour, IAtordoavel
{
    public GameObject PrefabProjetil;
    public Transform PontoDisparo;
    public float Alcance = 9f;
    public float TempoEntreTiros = 2f;
    public float VelocidadeProjetil = 7f;
    public int Dano = 10;
    public bool MirarNoJogador = true;      
    public string TriggerAnimacaoTiro = "";  

    Transform jogador;
    SpriteRenderer sprite;
    Animator animador;
    float timer;
    bool atordoado;

    void Start()
    {
        GameObject objJogador = GameObject.FindGameObjectWithTag("Player");
        if (objJogador != null) jogador = objJogador.transform;
        sprite = GetComponent<SpriteRenderer>();
        animador = GetComponent<Animator>();
        timer = TempoEntreTiros;
    }

    void Update()
    {
        if (atordoado || jogador == null) return;

        float distancia = Vector2.Distance(transform.position, jogador.position);
        if (distancia > Alcance) return;

        if (sprite != null) sprite.flipX = jogador.position.x < transform.position.x;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = TempoEntreTiros;
            Atirar();
        }
    }

    void Atirar()
    {
        if (PrefabProjetil == null) return;
        if (animador != null && TriggerAnimacaoTiro != "") animador.SetTrigger(TriggerAnimacaoTiro);

        Vector3 origem = PontoDisparo != null ? PontoDisparo.position : transform.position;
        GameObject copia = Instantiate(PrefabProjetil, origem, Quaternion.identity);
        copia.SetActive(true);

        ProjetilBoss scriptProjetil = copia.GetComponent<ProjetilBoss>();
        if (scriptProjetil == null) scriptProjetil = copia.AddComponent<ProjetilBoss>();
        scriptProjetil.Dano = Dano;
        scriptProjetil.Dono = gameObject;

        Rigidbody2D corpo = copia.GetComponent<Rigidbody2D>();
        if (corpo == null)
        {
            corpo = copia.AddComponent<Rigidbody2D>();
            corpo.gravityScale = 0f;
        }

        Vector2 direcao = MirarNoJogador
            ? ((Vector2)(jogador.position - origem)).normalized
            : new Vector2(jogador.position.x < transform.position.x ? -1f : 1f, 0f);
        corpo.velocity = direcao * VelocidadeProjetil;

        SpriteRenderer spriteProjetil = copia.GetComponent<SpriteRenderer>();
        if (spriteProjetil != null) spriteProjetil.flipX = direcao.x < 0f;

        Destroy(copia, 5f);
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
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, Alcance);
    }
}
