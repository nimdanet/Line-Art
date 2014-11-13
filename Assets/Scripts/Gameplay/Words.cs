using UnityEngine;
using System.Collections;
using System.Linq;

public class Words : MonoBehaviour 
{
	public TextAsset csvFile;
	private static ArrayList allWords;
	public static ArrayList inGame;

	public void Start()
	{
		allWords = new ArrayList ();

		string[] test = csvFile.text.Split("\n"[0]);

		for(int i = 0; i < test[i].Split(","[0]).Length; i++)
			allWords.Add(new ArrayList());

		for(int i = 1; i < test.Length; i++)
		{
			string[] row = test[i].Split(","[0]);

			for(int j = 0; j < row.Length; j++)
			{
				if(row[j].Length > 1)
					((ArrayList)allWords[j]).Add(row[j]);
			}
		}
	}

	public static void ArrangeCategory(GameController.Category category)
	{
		if(category == GameController.Category.All)
		{
			inGame = new ArrayList();

			for(int i = 1; i < allWords.Count; i++)
			{
				ArrayList cat = allWords[i] as ArrayList;
				
				for(int j = 0; j < cat.Count; j++)
					inGame.Add(cat[j]);
			}

			//for(int i = 0; i < inGame.Count; i++)
			//	Debug.Log(inGame[i]);
		}
		else
		{
			inGame = allWords [(int)category] as ArrayList;

			//for(int i = 0; i < inGame.Count; i++)
			//	Debug.Log(inGame[i]);
		}
	}
}
