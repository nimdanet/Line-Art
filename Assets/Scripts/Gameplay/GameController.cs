using UnityEngine;
using System.Collections;

public class GameController : Photon.MonoBehaviour 
{
	public enum Category
	{
		Comidas,
		Animais,
		All,
	}

	public enum GameState
	{
		WaitingForStartGameFromHost,
		WaitingForPlayers,
		WaitingForNextRound,
		Drawing,
		Guessing,
	}

	public GameState gameState = GameState.WaitingForStartGameFromHost;

	public static bool canDraw = false;

	public PhotonPlayer playerDrawing;
	public PhotonPlayer nextPlayerDrawing;
	public int roundTime = 60;
	public int betweenRoundsTime = 5;
	[HideInInspector]
	public int currentTime;
	public Category category;
	public string sortedWord;
	public static string[] sortedLetters;
	private string characters = "abcdefghijklmnopqrstuvwxyz";
	public static bool isHost = false;
	public int firstPlayerPoints;
	public int points;
	public bool guessed = false;
	public bool justEntered = true;
	public int playersGuessed;

	public static PhotonPlayer host;

	public Transform charPieces;

	private static GameController instance;
	public static GameController Instance
	{
		get 
		{ 
			if(instance == null)
			{
				GameObject go = new GameObject();
				go.AddComponent<GameController>();
				go.name = "@GameController (real-time created)";
				go.transform.parent = GameObject.Find("Controllers").transform;

				instance = go.GetComponent<GameController>();
			}

			instance.gameObject.SetActive(true);

			return instance; 
		}
	}

	public void StartGame()
	{
		instance = this;
		currentTime = 0;
		playersGuessed = 0;
		
		Words.ArrangeCategory (category);

		ArrangeGame ();
		
		StartWaitMode();
	}

	public void ArrangeGame()
	{
		//as soon as we connected to game, set the first parameters or get all of them if the game has already started
		gameState = GameState.WaitingForNextRound;

		//player drawing
		playerDrawing = NetworkController.GetPhotonPlayerFromID((int)PhotonNetwork.room.customProperties["playerDrawingID"]);
		//time
		HUDController.Instance.UpdateTime ((int)PhotonNetwork.room.customProperties["time"]);
		
		//next player drawing
		nextPlayerDrawing = NetworkController.GetPhotonPlayerFromID((int)PhotonNetwork.room.customProperties["nextPlayerDrawingID"]);
		HUDController.Instance.SetNextPlayerToDraw (nextPlayerDrawing);

		//host
		for(int i = 0; i < PhotonNetwork.playerList.Length; i++)
		{
			PhotonPlayer player = PhotonNetwork.playerList[i];

			if(player.ID == playerDrawing.ID)
			{
				host = player;
				break;
			}
		}

		HUDController.Instance.SetPlayerID ();
		HUDController.Instance.SetPlayerDrawing (playerDrawing);

		ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable();
		playerProperties.Add("guessed", false);
		playerProperties.Add("points", 0);
		playerProperties.Add("drawing", GameController.isHost);
		playerProperties.Add ("justEntered", justEntered);
		PhotonNetwork.player.SetCustomProperties(playerProperties);
	}

	public void OnPhotonPlayerConnected(PhotonPlayer player)
	{
		if(isHost)
		{
			PhotonPlayer newNextPlayerDrawing = GetNextPlayerForDrawing();

			if(newNextPlayerDrawing != nextPlayerDrawing)
			{
				nextPlayerDrawing = newNextPlayerDrawing;
				ExitGames.Client.Photon.Hashtable roomProperties = new ExitGames.Client.Photon.Hashtable();
				roomProperties.Add("nextPlayerDrawingID", nextPlayerDrawing.ID);
				PhotonNetwork.room.SetCustomProperties(roomProperties);

				NetworkController.Instance.SendNewNextPlayerToDraw(nextPlayerDrawing);
				HUDController.Instance.SetNextPlayerToDraw(nextPlayerDrawing);
			}
		}
	}

	public void OnPhotonPlayerDisconnected(PhotonPlayer player)
	{
		if(isHost && PhotonNetwork.room.playerCount > 1)
		{
			PhotonPlayer newNextPlayerDrawing = GetNextPlayerForDrawing();
			
			if(newNextPlayerDrawing != nextPlayerDrawing)
			{
				nextPlayerDrawing = newNextPlayerDrawing;
				ExitGames.Client.Photon.Hashtable roomProperties = new ExitGames.Client.Photon.Hashtable();
				roomProperties.Add("nextPlayerDrawingID", nextPlayerDrawing.ID);
				PhotonNetwork.room.SetCustomProperties(roomProperties);
				
				NetworkController.Instance.SendNewNextPlayerToDraw(nextPlayerDrawing);
			}
			
			HUDController.Instance.SetNextPlayerToDraw(nextPlayerDrawing);
		}

		if(PhotonNetwork.room.playerCount == 1)
		{
			StopGame();
		}
	}

	public void StartDrawing()
	{
		SetNewColor (Color.black);

		isHost = true;

		gameState = GameState.Drawing;
		SortWord ();

		unitycoder_MobilePaint.MobilePaint.Instance.ClearImage ();
		canDraw = true;

		currentTime = 0;
		playersGuessed = 0;

		ExitGames.Client.Photon.Hashtable roomProperties = new ExitGames.Client.Photon.Hashtable();
		roomProperties.Add("time", roundTime - currentTime);
		PhotonNetwork.room.SetCustomProperties(roomProperties);

		HUDController.Instance.UpdateTime (roundTime - currentTime);
		HUDController.Instance.HideRanking ();
		HUDController.Instance.ShowColors ();

		NetworkController.Instance.PassTime (roundTime - currentTime);
		NetworkController.Instance.StartRound ();

		StartCoroutine (Pass1Sec (1f));
	}

	public void SetNewColor(Color color)
	{
		unitycoder_MobilePaint.MobilePaint.Instance.SetPaintColor(color);
	}

	private void SortWord()
	{
		int rnd;
		do
		{
			rnd = Random.Range (0, Words.inGame.Count);
		}
		while((string)Words.inGame[rnd] == sortedWord);

		sortedWord = (string)Words.inGame[rnd];

		ExitGames.Client.Photon.Hashtable roomProperties = new ExitGames.Client.Photon.Hashtable();
		roomProperties.Add("sortedWord", sortedWord);
		PhotonNetwork.room.SetCustomProperties(roomProperties);

		NetworkController.Instance.SendSortedWord (sortedWord);
	}

	public void ScrumbbleCharacters()
	{
		sortedLetters = new string[12];

		for(int i = 0; i < 12; i++)
		{
			if(i < sortedWord.Length)
				sortedLetters.SetValue(sortedWord[i].ToString(), i);
			else
			{
				int rnd = Random.Range(0, characters.Length);
				sortedLetters.SetValue(characters[rnd].ToString(), i);
			}
		}

		//scrumbble
		string[] scrumbbledLetters = new string[12];
		for(int i = 0; i < 12; i++)
		{
			int rnd;
			do
			{
				rnd = Random.Range(0, scrumbbledLetters.Length);
			}
			while(!string.IsNullOrEmpty(scrumbbledLetters[rnd]));

			scrumbbledLetters[rnd] = sortedLetters[i];
		}

		sortedLetters = scrumbbledLetters;
		//end scrumbble

		HUDController.Instance.ShowLetters ();
	}

	public void DrawGuessed()
	{
		guessed = true;
		gameState = GameState.WaitingForNextRound;
		HUDController.Instance.HideLetters ();

		ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable();
		playerProperties.Add("guessed", true);
		PhotonNetwork.player.SetCustomProperties(playerProperties);

		NetworkController.Instance.WordGuessed ();
	}

	public void EndRound()
	{
		gameState = GameState.WaitingForNextRound;

		StopAllCoroutines ();
		canDraw = false;
		isHost = false;

		playerDrawing = nextPlayerDrawing;
		nextPlayerDrawing = GetNextPlayerForDrawing (playerDrawing);

		ExitGames.Client.Photon.Hashtable roomProperties = new ExitGames.Client.Photon.Hashtable();
		roomProperties.Add("playerDrawingID", playerDrawing.ID);
		roomProperties.Add("nextPlayerDrawingID", nextPlayerDrawing.ID);
		PhotonNetwork.room.SetCustomProperties(roomProperties);

		NetworkController.Instance.SendNewNextPlayerToDraw (playerDrawing);

		GameController.Instance.playerDrawing = PhotonNetwork.player;

		HUDController.Instance.HideLetters ();
		HUDController.Instance.HideColors ();
	}

	public void StartWaitMode()
	{
		HUDController.Instance.ShowRanking ();

		gameState = GameState.WaitingForNextRound;
		
		currentTime = 0;
		
		playerDrawing = NetworkController.GetPhotonPlayerFromID ((int)PhotonNetwork.room.customProperties ["playerDrawingID"]);
		nextPlayerDrawing = NetworkController.GetPhotonPlayerFromID ((int)PhotonNetwork.room.customProperties ["nextPlayerDrawingID"]);

		justEntered = false;
		guessed = false;

		ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable();
		playerProperties.Add("guessed", false);
		playerProperties.Add("drawing", playerDrawing.ID == PhotonNetwork.player.ID);
		playerProperties.Add ("justEntered", false);
		PhotonNetwork.player.SetCustomProperties(playerProperties);
		
		HUDController.Instance.SetPlayerDrawing (playerDrawing);
		HUDController.Instance.SetNextPlayerToDraw (nextPlayerDrawing);

		if(isHost)
			StartCoroutine (Pass1Sec (1f));
	}

	public void NextRound()
	{
		guessed = false;

		playerDrawing = NetworkController.GetPhotonPlayerFromID ((int)PhotonNetwork.room.customProperties ["playerDrawingID"]);
		nextPlayerDrawing = NetworkController.GetPhotonPlayerFromID ((int)PhotonNetwork.room.customProperties ["nextPlayerDrawingID"]);

		if(playerDrawing.ID == PhotonNetwork.player.ID)
		{
			StartWaitMode();
			HUDController.Instance.HideLetters();
		}
		else
			HUDController.Instance.ShowLetters();
	}

	private IEnumerator Pass1Sec(float waitTime)
	{
		yield return new WaitForSeconds (waitTime);
		
		currentTime++;
		int time = (gameState == GameState.Drawing) ? roundTime - currentTime : currentTime - betweenRoundsTime;
		
		ExitGames.Client.Photon.Hashtable roomProperties = new ExitGames.Client.Photon.Hashtable ();
		roomProperties.Add("time", time);
		PhotonNetwork.room.SetCustomProperties(roomProperties);
		
		HUDController.Instance.UpdateTime (time);
		NetworkController.Instance.PassTime (time);
		
		if(gameState == GameState.Drawing)
		{
			if(currentTime >= roundTime)
			{
				EndRound();
				NetworkController.Instance.NextRound ();
			}
			else
				StartCoroutine (Pass1Sec (1f));
		}
		else if(gameState == GameState.WaitingForNextRound)
		{
			if(currentTime >= betweenRoundsTime)
				StartDrawing();
			else
				StartCoroutine (Pass1Sec (1f));
		}
	}

	public PhotonPlayer GetNextPlayerForDrawing()
	{
		return GetNextPlayerForDrawing (playerDrawing);
	}

	public PhotonPlayer GetNextPlayerForDrawing(PhotonPlayer pDrawing)
	{
		PhotonPlayer player = null;
		
		//get next player drawing
		for(byte i = 0; i < PhotonNetwork.playerList.Length; i++)
		{
			if(PhotonNetwork.playerList[i] == pDrawing)
			{
				//get next or player 0 (if we reached the last player)
				player = PhotonNetwork.playerList[(i < PhotonNetwork.playerList.Length - 1) ? i + 1 : 0];
				break;
			}
		}
		
		return player;
	}

	public void StopGame()
	{
		canDraw = false;

		HUDController.Instance.HideLetters ();
		unitycoder_MobilePaint.MobilePaint.Instance.ClearImage (true);

		StopAllCoroutines ();

		playerDrawing = PhotonNetwork.player;
		nextPlayerDrawing = null;

		ExitGames.Client.Photon.Hashtable roomProperties = new ExitGames.Client.Photon.Hashtable();
		roomProperties.Add("playerDrawingID", playerDrawing);
		roomProperties.Add("time", currentTime - betweenRoundsTime);
		PhotonNetwork.room.SetCustomProperties(roomProperties);
		
		HUDController.Instance.UpdateTime (currentTime - betweenRoundsTime);

		sortedWord = "";

		HUDController.Instance.SetPlayerDrawing (playerDrawing);
		HUDController.Instance.SetNextPlayerToDraw (nextPlayerDrawing);
		
		isHost = true;

		gameState = GameState.WaitingForPlayers;

		PreRoomController.Instance.gameObject.SetActive (true);
		PreRoomController.Instance.ManualStart ();

		gameObject.SetActive (false);
	}
}
