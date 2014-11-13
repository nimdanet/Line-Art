// this script takes the initial color from canvas

using UnityEngine;
using System.Collections;

namespace unitycoder_MobilePaint
{

	public class GetStartColor : MonoBehaviour {

		public GameObject canvas;

		void Start () 
		{
			guiTexture.color = canvas.GetComponent<MobilePaint>().paintColor;
		}
		
	}
}