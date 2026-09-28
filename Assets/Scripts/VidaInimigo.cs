using System.Collections;
using UnityEngine;

// Coloque em QUALQUER inimigo (junto com Inimigo, InimigoPerseguidor, InimigoAtirador ou InimigoSaltador)
// pra ele ter vida, dar pontos, dar dano ao encostar e soltar um item quando morrer.
public class VidaInimigo : MonoBehaviour, IVida
{
    [Header("Vida")]
    public int VidaMaxima = 30;

    [Header("Recompensa")]
    public int Pontos = 50;

    [Header("Dano ao encostar no jogador")]
    public int DanoContato = 15;

    [Header("Drop (opcional)")]
    public GameObject ItemParaDropar;   // prefab OU filho desativado (ex: a poção dentro do inimigo)
    [Range(0f, 1f)] public float ChanceDeDrop = 1f;

    [Header("Morte")]
    public string TriggerAnimacaoMorte = ""; // nome do Trigger no Animator (deixe vazio se não tiver)
    public float TempoParaSumir = 0f;

    int vidaAtual;
    bool morto;
    SpriteRenderer sprite;
    Color corOriginal = Color.white;
    Coroutine rotinaPiscar;

    public bool EstaMorto { get { return morto; } }
    public int VidaAtual { get { return vidaAtual; } }

    void Awake()
    {
        vidaAtual = VidaMaxima;
        sprite = GetComponent<SpriteRenderer>();
        if (sprite != null) corOriginal = sprite.color;
    }

    public void ReceberDano(int dano)
    {
        if (morto) return;
        vidaAtual -= dano;

        if (sprite != null)
        {
            if (rotinaPiscar != null) StopCoroutine(rotinaPiscar);
            rotinaPiscar = StartCoroutine(Piscar());
        }

        if (vidaAtual <= 0) Morrer();
    }

    IEnumerator Piscar()
    {
        sprite.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        if (sprite != null) sprite.color = corOriginal;
    }

    public void Morrer()
    {
        if (morto) return; // garante que só morre (e só dropa) UMA vez
        morto = true;

        GerenciadorPontuacao.Instancia.AdicionarPontosInimigo(Pontos);
        Dropar();

        foreach (Collider2D c in GetComponentsInChildren<Collider2D>()) c.enabled = false;
        foreach (MonoBehaviour script in GetComponents<MonoBehaviour>())
        {
            if (script != this) script.enabled = false;
        }

        Rigidbody2D corpo = GetComponent<Rigidbody2D>();
        if (corpo != null)
        {
            corpo.velocity = Vector2.zero;
            corpo.bodyType = RigidbodyType2D.Kinematic;
        }

        if (!string.IsNullOrEmpty(TriggerAnimacaoMorte))
        {
            Animator animador = GetComponent<Animator>();
            if (animador != null) animador.SetTrigger(TriggerAnimacaoMorte);
        }

        Destroy(gameObject, TempoParaSumir);
    }

    void Dropar()
    {
        if (ItemParaDropar == null) return;
        GameObject modelo = ItemParaDropar;
        ItemParaDropar = null;

        if (Random.value > ChanceDeDrop) return;

        GameObject item;
        bool ehFilho = modelo.scene.IsValid() && modelo.transform.IsChildOf(transform);
        if (ehFilho)
        {
            item = modelo;
            item.transform.parent = null;
        }
        else
        {
            item = Instantiate(modelo, transform.position, Quaternion.identity);
        }
        item.SetActive(true);

        bool temColisorSolido = false;
        foreach (Collider2D c in item.GetComponents<Collider2D>())
        {
            c.enabled = true;
            if (!c.isTrigger) temColisorSolido = true;
        }
        if (!temColisorSolido) item.AddComponent<CircleCollider2D>(); // sem isso o item atravessa o chão

        Rigidbody2D corpoItem = item.GetComponent<Rigidbody2D>();
        if (corpoItem == null) corpoItem = item.AddComponent<Rigidbody2D>();
        corpoItem.bodyType = RigidbodyType2D.Dynamic;
        corpoItem.freezeRotation = true;
        corpoItem.velocity = new Vector2(Random.Range(-1.5f, 1.5f), 6f);
    }
}
