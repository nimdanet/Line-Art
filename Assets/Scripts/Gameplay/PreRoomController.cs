using UnityEngine;
using System.Collections;

public class PreRoomController : Photon.MonoBehaviour 
{
	public GameObject playerPrefab;
	public UIGrid playersGrid;
	public GameObject gameplay;
	public UIButton startGameButton;
	public int firstPlayerPoints;

	private static PreRoomController instance;
	public static PreRoomController Instance
	{
		get { return instance; }
	}

	// Use this for initialization
	void Start () 
	{
		instance = this;

		gameplay.SetActive (false);

		GameController.Instance.firstPlayerPoints = firstPlayerPoints;

		if (PhotonNetwork.playerList.Length > 1)
		{
			startGameButton.gameObject.SetActive(false);
			GameController.Instance.playerDrawing = NetworkController.GetPhotonPlayerFromID((int)PhotonNetwork.room.customProperties["playerDrawingID"]);
		}
		else
		{
			GameController.isHost = true;
			startGameButton.isEnabled = false;

			ExitGames.Client.Photon.Hashtable roomProperties = new ExitGames.Client.Photon.Hashtable();
			roomProperties.Add("playerDrawingID", PhotonNetwork.player.ID);
			roomProperties.Add("time", 0);
			PhotonNetwork.room.SetCustomProperties(roomProperties);

			GameController.Instance.playerDrawing = PhotonNetwork.player;
		}

		//set all player proterties
		ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable();
		playerProperties.Add("guessed", false);
		playerProperties.Add("points", 0);
		playerProperties.Add("drawing", PhotonNetwork.playerList.Length == 1);
		playerProperties.Add ("justEntered", true);
		playerProperties.Add ("title", "No Title Yet");
		//playerProperties.Add ("avatar", "avataor001");
		PhotonNetwork.player.SetCustomProperties(playerProperties);

		ArrangePlayers ();
	}

	public void ManualStart()
	{
		Start ();
	}

	public void OnPhotonPlayerConnected(PhotonPlayer player)
	{
		ArrangePlayers ();

		if (PhotonNetwork.playerList.Length > 1)
		{
			startGameButton.isEnabled = true;

			if(PhotonNetwork.playerList.Length == 2)
			{
				ExitGames.Client.Photon.Hashtable roomProperties = new ExitGames.Client.Photon.Hashtable();
				roomProperties.Add("nextPlayerDrawingID", player.ID);
				PhotonNetwork.room.SetCustomProperties(roomProperties);
			}
		}
	}

	public void OnPhotonPlayerDisconnected(PhotonPlayer player)
	{
		ArrangePlayers ();

		if (PhotonNetwork.playerList.Length == 1)
		{
			GameController.isHost = true;
			startGameButton.gameObject.SetActive(true);
			startGameButton.isEnabled = true;

			ExitGames.Client.Photon.Hashtable roomProperties = new ExitGames.Client.Photon.Hashtable();
			roomProperties.Add("playerDrawingID", PhotonNetwork.player.ID);
			PhotonNetwork.room.SetCustomProperties(roomProperties);
		}
	}

	private void ArrangePlayers()
	{
		//destroy all players created so for (graphic only)
		for( int i = playersGrid.transform.childCount - 1; i >= 0; i--)
			Destroy(playersGrid.transform.GetChild(i).gameObject);
		
		//create all again
		for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
		{
			PhotonPlayer photonPlayer = PhotonNetwork.playerList[i];
			
			Transform player = ((GameObject)Instantiate(playerPrefab)).transform;
			player.parent = playersGrid.transform;
			player.localScale = Vector3.one;
			//UIEventListener.Get(room.gameObject).onClick += JoinRoom;
			//room.GetComponent<UIButton>().onClick += JoinRoom;
			
			player.FindChild("Name").GetComponent<UILabel>().text = photonPlayer.name;
			player.FindChild("Title").GetComponent<UILabel>().text = (string)photonPlayer.customProperties["title"];
			//player.FindChild("Avatar").GetComponent<UISprite>().spritename = "";
		}

		playersGrid.repositionNow = true;
	}

	//called on 'onClick' event
	public void StartGame()
	{
		gameplay.SetActive (true);

		GameController.Instance.charPieces = gameplay.transform.FindChild ("Anchor_BottomLeft").FindChild ("Grid_CharPieces");
		GameController.Instance.StartGame ();

		if(GameController.isHost)
			NetworkController.Instance.StartGame();

		gameObject.SetActive (false);
	}
}
