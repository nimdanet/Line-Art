using UnityEngine;
using System.Collections;

public class RoomController : Photon.MonoBehaviour 
{
	public GameObject roomPrefab;
	public UILabel roomFeedback;
	public UIGrid roomsGrid;

	// Use this for initialization
	void Start () 
	{
		LoadPlayerStats ();

		UpdateRooms ();
	}

	private void LoadPlayerStats()
	{

		Player.id = PhotonNetwork.player.ID;
		Player.name = PhotonNetwork.player.name;
		Player.title = "No title yet";
		Player.avatar = "001";
		Player.currency = 0;

		#region temp until database is implemented
		//Player.titles = new string[1];
		//Player.avatars = new string[1];
		Player.colors = new System.Collections.Generic.List<Color> ();
		Player.colors.Add(Color.black);
		Player.colors.Add(Color.red);
		Player.colors.Add(Color.yellow);
		Player.colors.Add(Color.blue);
		#endregion

		Player.gamesPlayed = 0;
		Player.gamesWon = 0;
		Player.firstHit = 0;

		Player.totalPoints = 0;
		Player.avaragePoints = 0;

		Player.playTime = 0;
	}

	public void CreateNewRoom()
	{
		RoomOptions roomOptions = new RoomOptions ();
		roomOptions.maxPlayers = 10;
		roomOptions.isVisible = true;
		roomOptions.isOpen = true;

		ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable ();
		customRoomProperties.Add("description", "generic description");
		roomOptions.customRoomProperties = customRoomProperties;

		PhotonNetwork.CreateRoom(null, roomOptions, null);

		LoginController.Instance.state = LoginController.LoginState.CreatingRoom;
		LoginController.Instance.ShowPopup ();
	}

	public void OnCreatedRoom()
	{
		//Application.LoadLevel ("Gameplay");
	}

	public void OnJoinedRoom()
	{
		Application.LoadLevel ("Gameplay");
	}

	void OnReceivedRoomList()
	{
		UpdateRooms ();
	}

	void OnReceivedRoomListUpdate()
	{
		UpdateRooms ();
	}

	private void UpdateRooms()
	{
		//destroy all rooms created so for (graphic only)
		for( int i = roomsGrid.transform.childCount - 1; i >= 0; i--)
		{
			Destroy(roomsGrid.transform.GetChild(i).gameObject);
		}
		
		//create all again
		roomFeedback.text = "Rooms created: " + PhotonNetwork.GetRoomList ().Length;
		
		for (int i = 0; i < PhotonNetwork.GetRoomList().Length; i++)
		{
			RoomInfo roomInfo = PhotonNetwork.GetRoomList()[i];
			
			Transform room = ((GameObject)Instantiate(roomPrefab)).transform;
			room.parent = roomsGrid.transform;
			room.localScale = Vector3.one;
			//UIEventListener.Get(room.gameObject).onClick += JoinRoom;
			//room.GetComponent<UIButton>().onClick += JoinRoom;

			room.FindChild("Name").GetComponent<UILabel>().text = roomInfo.name;
			room.FindChild("Description").GetComponent<UILabel>().text = (roomInfo.customProperties.ContainsKey("description")) ? roomInfo.customProperties["description"].ToString() : "no description";
			room.FindChild("Players").GetComponent<UILabel>().text = roomInfo.playerCount + "/" + roomInfo.maxPlayers;
			
			UILabel state = room.FindChild("State").GetComponent<UILabel>();
			
			if(roomInfo.playerCount == 1)
			{
				state.text = "Waiting";
				state.color = Color.green;
			}
			else if(roomInfo.playerCount == roomInfo.maxPlayers)//10 playes - full room
			{
				state.text = "Full";
				state.color = Color.red;
			}
			else
			{
				state.text = "Playing";
				state.color = Color.yellow;
			}
		}
		
		roomsGrid.repositionNow = true;
	}

	public void JoinRoom()
	{
		string roomSelected = UIButton.current.gameObject.transform.FindChild("Name").GetComponent<UILabel>().text;

		Debug.Log (roomSelected);

		PhotonNetwork.JoinRoom (roomSelected);

		LoginController.Instance.state = LoginController.LoginState.JoiningRoom;
		LoginController.Instance.ShowPopup ();
	}
}
