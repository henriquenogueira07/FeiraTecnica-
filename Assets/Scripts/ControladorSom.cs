using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControladorSom : MonoBehaviour {

	private bool tasaindosom = true;
	[SerializeField] private AudioSource musica;
	[SerializeField] private Sprite iconeLigado;
	[SerializeField] private Sprite iconeDesligado;
	[SerializeField] private Image botao;
	public void LigarDesligarSom()
	{
		tasaindosom = !tasaindosom;
		musica.enabled = tasaindosom;
		if (tasaindosom) {
			botao.sprite = iconeLigado;
		} else {
			botao.sprite = iconeDesligado;
		}
	}
	public void VolumeMusical (float value){
		musica.volume = value;
	}
}