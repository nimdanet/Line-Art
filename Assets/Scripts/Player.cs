using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour 
{
	/// <summary>
	/// Locla ID of current player (one different each login)
	/// </summary>
	public static int id;
	/// <summary>
	/// Name of current player
	/// </summary>
	public static string name;
	/// <summary>
	/// Current selected title of current player
	/// </summary>
	public static string title;
	/// <summary>
	/// Current selected avatar of current player
	/// </summary>
	public static string avatar;
	/// <summary>
	/// Coins that user have right now to spend
	/// </summary>
	public static long currency;

	/// <summary>
	/// All titles owned by player
	/// </summary>
	public static List<string> titles;
	/// <summary>
	/// All avatars owned by player
	/// </summary>
	public static List<string> avatars;
	/// <summary>
	/// All colors owned by player
	/// </summary>
	public static List<Color> colors;



	/// <summary>
	/// Total of complete games played (from first / second round to final round)
	/// </summary>
	public static long gamesPlayed;
	/// <summary>
	/// Total of complete games won (from first / second round to final round)
	/// </summary>
	public static long gamesWon;
	/// <summary>
	/// How many times the user guessed first
	/// </summary>
	public static long firstHit;

	/// <summary>
	/// Total points made by player from all time
	/// </summary>
	public static long totalPoints;
	/// <summary>
	/// An avarage points per game made by player
	/// </summary>
	public static long avaragePoints;

	/// <summary>
	/// Total time inside a game room (lobby doesn't count)
	/// </summary>
	public static float playTime;
}
