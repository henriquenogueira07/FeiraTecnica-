using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlataformasAndarilhas : MonoBehaviour {
    Transform TransformPlataforma;
	Vector2 PosicaoInicial; 
	float TempoDecorrido; 
	public float Distancia; 
	public float VelocidadeMovimento; 
	public Vector2 Direcao;
	void Start () {
	TransformPlataforma = GetComponent<Transform>();
	PosicaoInicial = TransformPlataforma.position;
	TempoDecorrido = 0f;	
	}
	
	void Update () {
		mover();
	}
	void mover()
{
	TempoDecorrido += Time.deltaTime * VelocidadeMovimento;
	float movimento = Mathf.PingPong(TempoDecorrido, Distancia);
	transform.position = PosicaoInicial + Direcao.normalized * movimento;
}
void OnCollisionExit2D(Collision2D objetoParouTocar)
{
string tag = objetoParouTocar.gameObject.tag;
	if (tag.Contains("Player") == true)
	{
	objetoParouTocar.transform.parent = null;
	}
  }
}
