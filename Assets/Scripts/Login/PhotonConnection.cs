using UnityEngine;
using System.Collections;

public class PhotonConnection : MonoBehaviour 
{
	public bool autoConnect = true;

	private static PhotonConnection instance;
	public static PhotonConnection Instance
	{
		get { return instance; }
	}

	// Use this for initialization
	void Start () 
	{
		instance = this;
		DontDestroyOnLoad (gameObject);
		DontDestroyOnLoad (this);

		PhotonNetwork.autoJoinLobby = true;
		if(autoConnect)
		{
			Connect();
		}
	}

	public void Connect()
	{
		PhotonNetwork.ConnectUsingSettings("2.0");
	}

	public virtual void OnConnectedToMaster()
	{
		if (PhotonNetwork.networkingPeer.AvailableRegions != null) 
		{
			Debug.LogWarning("List of available regions counts " + PhotonNetwork.networkingPeer.AvailableRegions.Count + ". First: " + PhotonNetwork.networkingPeer.AvailableRegions[0] + " \t Current Region: " + PhotonNetwork.networkingPeer.CloudRegion);
		}

		//feedback.text = "OnConnectedToMaster() was called by PUN. Now this client is connected and could join a room. Calling: PhotonNetwork.JoinRandomRoom();";
	}

	public virtual void OnPhotonRandomJoinFailed()
	{
		//feedback.text = "OnPhotonRandomJoinFailed() was called by PUN. No random room available, so we create one. Calling: PhotonNetwork.CreateRoom(null, new RoomOptions() {maxPlayers = 4}, null);";

		PhotonNetwork.CreateRoom(null, new RoomOptions() { maxPlayers = 10 }, null);
	}
	
	// the following methods are implemented to give you some context. re-implement them as needed.
	
	public virtual void OnFailedToConnectToPhoton(DisconnectCause cause)
	{
		//feedback.text = "Cause: " + cause;
	}
	
	public void OnJoinedRoom()
	{
		//feedback.text = "OnJoinedRoom() called by PUN. Now this client is in a room. From here on, your game would be running. For reference, all callbacks are listed in enum: PhotonNetworkingMessage";
	}
	
	public virtual void OnJoinedLobby()
	{
		///feedback.text = "OnJoinedLobby(). Use a GUI to show existing rooms available in PhotonNetwork.GetRoomList().";

		//PhotonNetwork.JoinRandomRoom();
	}
}
