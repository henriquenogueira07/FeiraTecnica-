using UnityEngine;

public class ProjetilBoss : MonoBehaviour
{
    public int Dano = 10;
    public GameObject Dono; 
    public bool DestruirAoBaterNoCenario = true;

    bool usado;

    void OnTriggerEnter2D(Collider2D outro) { Verificar(outro); }
    void OnCollisionEnter2D(Collision2D colisao) { Verificar(colisao.collider); }

    void Verificar(Collider2D outro)
    {
        if (usado || outro == null) return;
        if (Dono != null && outro.transform.IsChildOf(Dono.transform)) return;

        Personagem jogador = outro.GetComponentInParent<Personagem>();
        if (jogador != null)
        {
            if (jogador.EstaEmDashAtivo) return;
            usado = true;
            jogador.ReceberDano(Dano, transform.position);
            Destroy(gameObject);
            return;
        }

        if (outro.isTrigger) return;
        if (outro.GetComponentInParent<BossMedusa>() != null) return;
        if (outro.GetComponentInParent<IVida>() != null) return;
        if (outro.GetComponent<ProjetilBoss>() != null) return;

        if (DestruirAoBaterNoCenario)
        {
            usado = true;
            Destroy(gameObject);
        }
    }
}
