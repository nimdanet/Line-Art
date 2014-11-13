using UnityEngine;
using System.Collections;

public class SetColor : MonoBehaviour 
{
	public void Set()
	{
		GameController.Instance.SetNewColor (GetComponent<UISprite> ().color);
	}
}
