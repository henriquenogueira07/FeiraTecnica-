using UnityEngine;

public class NPCDialogo : MonoBehaviour
{
    public string NomeNPC = "Aldeão";
    [TextArea(2, 4)] public string[] Falas;

    public GameObject BotaoInteragir;          
    public float DistanciaInteracao = 2f;     
    public KeyCode TeclaInteragir = KeyCode.E;
    public bool FalarSoUmaVez = false;
    public bool OlharParaJogador = true;
    public Transform Jogador;                 
    SpriteRenderer sprite;
    bool jaFalou;

    void Start()
    {
        if (Jogador == null)
        {
            GameObject objJogador = GameObject.FindGameObjectWithTag("Player");
            if (objJogador != null) Jogador = objJogador.transform;
        }
        sprite = GetComponent<SpriteRenderer>();
        if (BotaoInteragir != null) BotaoInteragir.SetActive(false);
    }

    void Update()
    {
        bool perto = JogadorPerto();
        bool podeFalar = perto && !CaixaDialogo.DialogoAberto && !(FalarSoUmaVez && jaFalou) &&
                         CaixaDialogo.FrameFechamento != Time.frameCount;

        if (BotaoInteragir != null && BotaoInteragir.activeSelf != podeFalar)
        {
            BotaoInteragir.SetActive(podeFalar);
        }

        if (perto && OlharParaJogador && sprite != null)
        {
            sprite.flipX = Jogador.position.x < transform.position.x;
        }

        if (podeFalar && Input.GetKeyDown(TeclaInteragir)) Interagir();
    }

    bool JogadorPerto()
    {
        return Jogador != null && Vector2.Distance(Jogador.position, transform.position) <= DistanciaInteracao;
    }

    // Público pra poder ser chamado também por um Button da UI (OnClick)
    public void Interagir()
    {
        if (CaixaDialogo.DialogoAberto) return;
        if (FalarSoUmaVez && jaFalou) return;
        if (!JogadorPerto()) return;

        if (CaixaDialogo.Instancia == null)
        {
            Debug.LogWarning("Não existe CaixaDialogo na cena! Coloque o script no Canvas.");
            return;
        }

        CaixaDialogo.Instancia.Abrir(NomeNPC, Falas);
        jaFalou = true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, DistanciaInteracao);
    }
}
