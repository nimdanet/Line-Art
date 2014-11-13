using UnityEngine;
using System.Collections;
using System;

public class HUDController : MonoBehaviour 
{

	public UILabel feedback;
	public UILabel players;
	public UILabel playerID;
	public UILabel playerDrawing;
	public UILabel nextPlayerDraw;
	public UILabel time;
	public UILabel ranking;

	public GameObject placeChar;
	public Transform lettersGrid;
	public Transform placeGrid;
	public Transform piecesGrid;

	public GameObject color;
	public Transform colorGrid;

	private static HUDController instance;
	public static HUDController Instance
	{
		get { return instance; }
	}

	// Use this for initialization
	void Start () 
	{
		instance = this;
		ranking.enabled = false;

		ArrangeColors ();

		HideLetters ();
		HideColors ();
		HideRanking ();
	}
	
	void Update()
	{
		if(GameController.Instance != null)
		{
			if(GameController.Instance.gameState == GameController.GameState.Drawing)
			{
				feedback.text = "DRAW: " + GameController.Instance.sortedWord;
			}
			else if (GameController.Instance.gameState == GameController.GameState.Guessing)
			{
				//feedback.text = "TRY TO GUESS USER " + GameController.host.name + " DRAWING";
			}
			else if(GameController.Instance.gameState == GameController.GameState.WaitingForNextRound)
			{
				//feedback.text = "WAITING FOR GAME TO START";
			}
		}
	}

	public void SetPlayerID()
	{
		playerID.text = PhotonNetwork.player.name;
	}

	public void SetPlayerDrawing(PhotonPlayer pDrawing)
	{
		playerDrawing.text = (pDrawing.ID == Player.id) ? "YOU" : pDrawing.name;
	}

	public void SetNextPlayerToDraw(PhotonPlayer playerToDraw)
	{
		if(playerToDraw == null)
			nextPlayerDraw.text = "--";
		else
			nextPlayerDraw.text = (playerToDraw.ID == Player.id) ? "YOU" : playerToDraw.name;
	}

	public void	UpdateTime(int time)
	{
		TimeSpan timeSpan = TimeSpan.FromSeconds (time);

		if(time >= 0)
			this.time.text = String.Format ("{0:0}:{1:00}", timeSpan.Minutes, timeSpan.Seconds);
		else
			this.time.text = "-" + String.Format ("{0:0}:{1:00}", timeSpan.Minutes, Mathf.Abs(timeSpan.Seconds));

		if(time < 0)
			this.time.color = Color.yellow;
		else if(time < 10 && time > 0)
			this.time.color = Color.red;
		else
			this.time.color = Color.white;
	}

	public void ArrangePlaces()
	{
		for(int i = 0; i < GameController.Instance.sortedWord.Length; i++)
		{
			Transform obj = ((GameObject)Instantiate(placeChar)).transform;
			obj.parent = placeGrid;
			obj.localScale = Vector3.one;
		}

		placeGrid.GetComponent<UIGrid> ().repositionNow = true;
	}

	public void HideLetters()
	{
		for(int i = piecesGrid.childCount - 1; i >= 0; i--)
		{
			piecesGrid.GetChild(i).GetComponent<UISprite>().color = Color.white;
			piecesGrid.GetChild(i).GetComponent<UIButton>().defaultColor = Color.white;
			piecesGrid.GetChild(i).parent = lettersGrid;
		}

		for(int i = placeGrid.childCount - 1; i >= 0; i--)
			Destroy(placeGrid.GetChild(i).gameObject);

		lettersGrid.gameObject.SetActive (false);
		placeGrid.gameObject.SetActive (false);

		lettersGrid.GetComponent<UIGrid> ().repositionNow = true;
		placeGrid.GetComponent<UIGrid> ().repositionNow = true;
	}

	public void ShowLetters()
	{
		for(int i = 0; i < GameController.sortedLetters.Length; i++)
			lettersGrid.GetChild(i).GetChild(0).GetComponent<UILabel>().text = GameController.sortedLetters[i].ToString();
		
		ArrangePlaces ();

		lettersGrid.gameObject.SetActive (true);
		placeGrid.gameObject.SetActive (true);
	}

	public void OnClickLetter()
	{
		if(GameController.Instance.guessed) return;

		Transform letter = UIButton.current.transform;

		if(letter.parent == lettersGrid)
		{
			//user is trying to make the word
			if(piecesGrid.childCount >= GameController.Instance.sortedWord.Length) return;
			
			letter.parent = piecesGrid;

			//verify if its the correct word
			if(piecesGrid.childCount == GameController.Instance.sortedWord.Length)
			{
				string word = "";
				for(int i = 0; i < piecesGrid.childCount; i++)
					word += piecesGrid.GetChild(i).GetChild(0).GetComponent<UILabel>().text;

				Debug.Log(word + " == " + GameController.Instance.sortedWord);
				if(word == GameController.Instance.sortedWord)
				{
					//paint it in green
					for(int i = 0; i < piecesGrid.childCount; i++)
					{
						piecesGrid.GetChild(i).GetComponent<UISprite>().color = Color.green;
						piecesGrid.GetChild(i).GetComponent<UIButton>().defaultColor = Color.green;
					}

					GameController.Instance.DrawGuessed();
				}
				else
				{
					//paint it in red
					if(piecesGrid.childCount == GameController.Instance.sortedWord.Length)
					{
						for(int i = 0; i < piecesGrid.childCount; i++)
						{
							piecesGrid.GetChild(i).GetComponent<UISprite>().color = Color.red;
							piecesGrid.GetChild(i).GetComponent<UIButton>().defaultColor = Color.red;
						}
					}
				}
			}
		}
		else
		{
			//user is sending back a word

			//but first, back to normal color
			if(piecesGrid.childCount == GameController.Instance.sortedWord.Length)
			{
				for(int i = 0; i < piecesGrid.childCount; i++)
				{
					piecesGrid.GetChild(i).GetComponent<UISprite>().color = Color.white;
					piecesGrid.GetChild(i).GetComponent<UIButton>().defaultColor = Color.white;
				}
			}

			letter.parent = lettersGrid;
		}

		piecesGrid.GetComponent<UIGrid> ().repositionNow = true;
		lettersGrid.GetComponent<UIGrid> ().repositionNow = true;
	}

	public void UpdateRanking()
	{
		PhotonPlayer[] playersSorted = SortByScore ();

		string output = "";
		for(int i = 0; i < playersSorted.Length; i++)
		{
			output += "Player : " + playersSorted[i].name + ", points: " + playersSorted[i].customProperties["points"];

			if(i < playersSorted.Length - 1)
				output += "\n";
		}

		ranking.text = output;
	}

	public PhotonPlayer[] SortByScore()
	{
		PhotonPlayer[] players = PhotonNetwork.playerList;
		PhotonPlayer holder;
		
		for(int i = 1; i < players.Length; i++)
		{
			int j = i;
			while(j > 1)
			{
				if((int)players[j-1].customProperties["points"] < (int)players[j].customProperties["points"])
				{
					holder = players[j-1];
					players[j-1] = players[j];
					players[j] = holder;
					j--;
				}
				else
					break;
			}
		}

		return players;
	}

	public void ShowRanking()
	{
		UpdateRanking ();
		ranking.enabled = true;
	}

	public void HideRanking()
	{
		ranking.enabled = false;
	}

	public void ShowColors()
	{
		colorGrid.parent.gameObject.SetActive (true);
	}

	public void HideColors()
	{
		colorGrid.parent.gameObject.SetActive (false);
	}

	public void ArrangeColors()
	{
		for(int i = 0; i < Player.colors.Count; i++)
		{
			GameObject go = Instantiate(color) as GameObject;
			go.transform.parent = colorGrid;
			go.transform.localScale = Vector3.one;

			go.GetComponent<UIButton>().defaultColor = Player.colors[i];
			go.GetComponent<UIButton>().hover = Player.colors[i];
			go.GetComponent<UIButton>().pressed = Player.colors[i];
			go.GetComponent<UISprite>().color = Player.colors[i];
		}

		colorGrid.GetComponent<UIGrid> ().repositionNow = true;
	}
}
