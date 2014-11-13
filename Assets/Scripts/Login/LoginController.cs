using UnityEngine;
using System.Collections;

public class LoginController : Photon.MonoBehaviour 
{
	public enum LoginState
	{
		WaitingForConnect,
		Connecting,
		OnLobby,
		CreatingRoom,
		JoiningRoom,
	}

	public LoginState state = LoginState.WaitingForConnect;

	public GameObject loginScreen;
	public GameObject popupScreen;
	public GameObject roomScreen;
	public UILabel popupLabel;

	private bool isGuest;

	private static LoginController instance;
	public static LoginController Instance
	{
		get { return instance; }
	}

	void Start()
	{
		isGuest = false;
		instance = this;

		loginScreen.SetActive (true);
		popupScreen.SetActive (false);
		roomScreen.SetActive (false);

		loginScreen.transform.FindChild ("Facebook").GetComponent<UIButton> ().isEnabled = false;
	}

	public void ConnectAsGuest()
	{
		isGuest = true;
		Connect ();
	}

	public void Connect()
	{
		PhotonConnection.Instance.Connect ();

		loginScreen.SetActive (false);
		ShowPopup ();

		state = LoginState.Connecting;
	}

	void Update()
	{
		popupLabel.text = PhotonNetwork.connectionStateDetailed.ToString ();
	}

	void OnJoinedLobby()
	{
		PhotonNetwork.player.name = (isGuest) ? "Guest#" + Random.Range(1, 9999) : "ERROR";
		
		HidePopup();
		roomScreen.SetActive(true);
		state = LoginState.OnLobby;
	}

	public void ShowPopup()
	{
		popupScreen.SetActive (true);
	}

	public void HidePopup()
	{
		popupScreen.SetActive (false);
	}
}
