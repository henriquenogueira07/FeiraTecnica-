using UnityEngine;

public class BotaoNPC : MonoBehaviour
{
    public NPCDialogo NPC;                   
    public float EscalaAoPassarMouse = 1.15f;
    public float AlturaFlutuar = 0.1f;
    public float VelocidadeFlutuar = 3f;

    Vector3 escalaOriginal;
    Vector3 posicaoOriginal;

    void Awake()
    {
        escalaOriginal = transform.localScale;
        posicaoOriginal = transform.localPosition;
        if (NPC == null) NPC = GetComponentInParent<NPCDialogo>();
    }

    void Update()
    {
        transform.localPosition = posicaoOriginal + Vector3.up * Mathf.Sin(Time.time * VelocidadeFlutuar) * AlturaFlutuar;
    }

    void OnMouseDown()
    {
        if (NPC != null) NPC.Interagir();
    }

    void OnMouseEnter()
    {
        transform.localScale = escalaOriginal * EscalaAoPassarMouse;
    }

    void OnMouseExit()
    {
        transform.localScale = escalaOriginal;
    }

    void OnDisable()
    {
        if (escalaOriginal != Vector3.zero) transform.localScale = escalaOriginal;
    }
}
