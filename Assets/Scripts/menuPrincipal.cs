using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class menuPrincipal : MonoBehaviour {

	[SerializeField] private string nomeFase;
	[SerializeField] private GameObject painelOpcoes;
	[SerializeField] private GameObject painelMenuPrincipal;
	[SerializeField] private GameObject painelControles;
	public void Jogar()
	{
		SceneManager.LoadScene (nomeFase);
	}

	public void AbrirOpcoes()
	{
		painelMenuPrincipal.SetActive (false);
		painelOpcoes.SetActive (true);
	}

	public void FecharOpcoes()
	{
		painelOpcoes.SetActive (false);
		painelMenuPrincipal.SetActive (true);
	}

	public void AbrirControles()
	{
		painelControles.SetActive (true);
		painelOpcoes.SetActive (false);
	}

	public void FecharControles()
	{
		painelControles.SetActive (false);
		painelOpcoes.SetActive (true);
	}

	public void SairJogo()
	{
		Debug.Log ("Sair do Jogo");
		Application.Quit();
	}
}
