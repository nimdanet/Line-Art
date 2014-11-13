using UnityEngine;
using System.Collections;

public class NetworkController : Photon.MonoBehaviour 
{

	private static NetworkController instance;
	public static NetworkController Instance
	{
		get { return instance; }
	}

	// Use this for initialization
	void Start () 
	{
		instance = this;
	}

	#region Send RPC Methods
	public void DrawCircle(float posx, float posy)
	{
		if(PhotonNetwork.connected && !GameController.Instance.justEntered)
			photonView.RPC("ReceivedDrawCircle", PhotonTargets.Others, posx / Screen.width, posy / Screen.height);
	}

	public void DrawWhiteCircle(float posx, float posy)
	{
		if(PhotonNetwork.connected && !GameController.Instance.justEntered)
			photonView.RPC("ReceivedDrawWhiteCircle", PhotonTargets.Others, posx / Screen.width, posy / Screen.height);
	}

	public void DrawCustomBrush(float posx, float posy)
	{
		if(PhotonNetwork.connected && !GameController.Instance.justEntered)
			photonView.RPC("ReceivedDrawCustomBrush", PhotonTargets.Others, posx / Screen.width, posy / Screen.height);
	}

	public void FloodFill(float posx, float posy)
	{
		if(PhotonNetwork.connected && !GameController.Instance.justEntered)
			photonView.RPC("ReceivedFloodFill", PhotonTargets.Others, posx / Screen.width, posy / Screen.height);
	}

	public void DrawLine(Vector2 initialPos, Vector2 finalPos)
	{
		if(PhotonNetwork.connected && !GameController.Instance.justEntered)
			photonView.RPC("ReceivedDrawLine", PhotonTargets.Others, initialPos.x / Screen.width, initialPos.y / Screen.height, finalPos.x / Screen.width, finalPos.y / Screen.height);
	}

	public void DrawWhiteLine(Vector2 initialPos, Vector2 finalPos)
	{
		if(PhotonNetwork.connected && !GameController.Instance.justEntered)
			photonView.RPC("ReceivedDrawWhiteLine", PhotonTargets.Others, initialPos.x / Screen.width, initialPos.y / Screen.height, finalPos.x / Screen.width, finalPos.y / Screen.height);
	}

	public void SetPaintColor(Color color)
	{
		if(PhotonNetwork.connected && !GameController.Instance.justEntered)
			photonView.RPC("ReceivedSetPaintColor", PhotonTargets.Others, color.r, color.g, color.b, color.a);
	}	

	public void SetBrushSize(int size)
	{
		if(PhotonNetwork.connected && !GameController.Instance.justEntered)
			photonView.RPC("ReceivedBrushSize", PhotonTargets.Others, size);
	}

	public void SetCustomBrush(int selGridInt)
	{
		if(PhotonNetwork.connected && !GameController.Instance.justEntered)
			photonView.RPC("ReceivedCustomBrush", PhotonTargets.Others, selGridInt);
	}

	public void ClearImage()
	{
		if(PhotonNetwork.connected && !GameController.Instance.justEntered)
			photonView.RPC("ReceivedClearImage", PhotonTargets.Others);
	}

	public void SendNewNextPlayerToDraw(PhotonPlayer player)
	{
		if(PhotonNetwork.connected)
			photonView.RPC("ReceivedNewNextPlayerToDraw", PhotonTargets.Others, player.ID);
	}

	public void PassTime(int time)
	{
		if(PhotonNetwork.connected)
			photonView.RPC("ReceivedPassTime", PhotonTargets.Others, time);
	}

	public void SendSortedWord(string word)
	{
		if(PhotonNetwork.connected)
			photonView.RPC("ReceivedSortedWord", PhotonTargets.Others, word);
	}

	public void NextRound()
	{
		if(PhotonNetwork.connected)
			photonView.RPC("ReceivedNextRound", PhotonTargets.All);
	}

	public void StartRound()
	{
		if(PhotonNetwork.connected)
			photonView.RPC("ReceivedStartRound", PhotonTargets.Others);
	}

	public void WordGuessed()
	{
		if(PhotonNetwork.connected)
			photonView.RPC("ReceivedWordGuessed", GameController.host);
	}

	public void SendPointsBack(int points, PhotonPlayer player)
	{
		if(PhotonNetwork.connected)
			photonView.RPC("ReceivedCalculatedPoints", player, points);
	}

	public void PointsUpdated()
	{
		if(PhotonNetwork.connected)
			photonView.RPC("PlayerPointsUpdated", PhotonTargets.Others);
	}

	public void StartGame()
	{
		if(PhotonNetwork.connected)
			photonView.RPC("ReceivedStartGame", PhotonTargets.Others);
	}
	#endregion

	#region Receive RPC Methods
	[RPC]
	private void ReceivedDrawCircle(float posx, float posy)
	{
		unitycoder_MobilePaint.MobilePaint.Instance.DrawCircle ((int)(posx * Screen.width), (int)(posy * Screen.height), true);
	}

	[RPC]
	private void ReceivedDrawWhiteCircle(float posx, float posy)
	{
		unitycoder_MobilePaint.MobilePaint.Instance.DrawCircle ((int)(posx * Screen.width), (int)(posy * Screen.height), Color.white, true);
	}
	[RPC]
	private void ReceivedDrawCustomBrush(float posx, float posy)
	{
		unitycoder_MobilePaint.MobilePaint.Instance.DrawCustomBrush ((int)(posx * Screen.width), (int)(posy * Screen.height), true);
	}

	[RPC]
	private void ReceivedFloodFill(float posx, float posy)
	{
		unitycoder_MobilePaint.MobilePaint.Instance.FloodFill ((int)(posx * Screen.width), (int)(posy * Screen.height), true);
	}

	[RPC]
	public void ReceivedDrawLine(float initialPosx, float initialPosy, float finalPosx, float finalPosy)
	{
		unitycoder_MobilePaint.MobilePaint.Instance.DrawLine (new Vector2(initialPosx * Screen.width, initialPosy * Screen.height), new Vector2(finalPosx * Screen.width, finalPosy * Screen.height), true);
	}

	[RPC]
	public void ReceivedDrawWhiteLine(float initialPosx, float initialPosy, float finalPosx, float finalPosy)
	{
		unitycoder_MobilePaint.MobilePaint.Instance.DrawLine (new Vector2(initialPosx * Screen.width, initialPosy * Screen.height), new Vector2(finalPosx * Screen.width, finalPosy * Screen.height), Color.white, true);
	}

	[RPC]
	public void ReceivedSetPaintColor(float r, float g, float b, float a)
	{
		unitycoder_MobilePaint.MobilePaint.Instance.SetPaintColor (new Color (r, g, b, a), false);
	}

	[RPC]
	public void ReceivedBrushSize(int size)
	{
		unitycoder_MobilePaint.MobilePaint.Instance.SetBrushSize (size, false);
	}

	[RPC]
	public void ReceivedCustomBrush(int selGridInt)
	{
		unitycoder_MobilePaint.MobilePaint.Instance.SetCustomBrush (selGridInt, false);
	}
	
	[RPC]
	public void ReceivedClearImage()
	{
		unitycoder_MobilePaint.MobilePaint.Instance.ClearImage (false);
	}

	[RPC]
	public void ReceivedNewNextPlayerToDraw(int player)
	{
		GameController.Instance.nextPlayerDrawing = GetPhotonPlayerFromID(player);

		if(player == PhotonNetwork.player.ID)
			GameController.isHost = true;

		HUDController.Instance.SetNextPlayerToDraw (GameController.Instance.nextPlayerDrawing);
	}

	[RPC]
	public void ReceivedPassTime(int time)
	{
		HUDController.Instance.UpdateTime (time);

		if(time == 0)
			GameController.Instance.EndRound();
	}

	[RPC]
	public void ReceivedSortedWord(string word)
	{
		GameController.Instance.sortedWord = word;

		GameController.Instance.ScrumbbleCharacters ();
	}

	[RPC]
	public void ReceivedNextRound()
	{
		GameController.Instance.StartWaitMode ();
		GameController.isHost = false;

		for(int i = 0; i < PhotonNetwork.playerList.Length; i++)
		{
			PhotonPlayer player = PhotonNetwork.playerList[i];
			if(player == GameController.Instance.playerDrawing)
			{
				GameController.host = PhotonNetwork.playerList[i];
				break;
			}
		}
	}

	[RPC]
	public void ReceivedStartRound()
	{
		HUDController.Instance.HideRanking ();
		GameController.Instance.gameState = GameController.GameState.Guessing;
	}

	[RPC]
	public void ReceivedWordGuessed(PhotonMessageInfo info)
	{
		//apenas o host recebe essa mensagem

		//calcula os pontos do host (desenhista)
		GameController.Instance.playersGuessed++;
		if(GameController.Instance.playersGuessed == 1)
			SendPointsBack(10, PhotonNetwork.player);

		SendPointsBack(1, PhotonNetwork.player);

		//calcular os pontos do jogador que acertou e enviar de volta para o ele
		SendPointsBack (GameController.Instance.firstPlayerPoints - (GameController.Instance.playersGuessed - 1), info.sender);

		//verifica se todos acertaram
		int count = 0;
		for(int i = 0; i < PhotonNetwork.playerList.Length; i++)
		{
			PhotonPlayer player = PhotonNetwork.playerList[i];
			
			if((bool)player.customProperties["guessed"] == true || (bool)player.customProperties["drawing"] == true || (bool)player.customProperties["justEntered"] == true)
				count++;
		}
		
		if(count == PhotonNetwork.playerList.Length)
		{
			GameController.Instance.EndRound();
			NetworkController.Instance.NextRound ();
		}
	}

	[RPC]
	public void ReceivedCalculatedPoints(int points)
	{
		//apenas o player que acertou a palavra recebe esse evento

		//atualiza seus pontos
		GameController.Instance.points += points;

		ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable();
		playerProperties.Add("points", GameController.Instance.points);
		PhotonNetwork.player.SetCustomProperties(playerProperties);

		//manda para todos
		PointsUpdated ();
	}

	[RPC]
	public void PlayerPointsUpdated()
	{
		HUDController.Instance.UpdateRanking ();
	}
	
	[RPC]
	public void ReceivedStartGame()
	{
		GameController.isHost = false;

		PreRoomController.Instance.StartGame ();
	}
	#endregion

	#region static methods
	public static PhotonPlayer GetPhotonPlayerFromID(int id)
	{
		PhotonPlayer player = null;
		
		for(int i = 0; i < PhotonNetwork.playerList.Length; i++)
		{
			player = PhotonNetwork.playerList[i];

			if(player.ID == id)
				break;
		}
		
		return 	player;
	}
	#endregion
}
