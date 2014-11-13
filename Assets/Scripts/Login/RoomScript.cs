using UnityEngine;
using System.Collections;

public class RoomScript : MonoBehaviour 
{

	public void JoinRoom()
	{
		string roomSelected = UIButton.current.gameObject.transform.FindChild("Name").GetComponent<UILabel>().text;
		
		PhotonNetwork.JoinRoom (roomSelected);
		
		LoginController.Instance.state = LoginController.LoginState.JoiningRoom;
		LoginController.Instance.ShowPopup ();
	}
}
