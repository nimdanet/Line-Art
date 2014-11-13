// simple button: call ClearImage on paint plane

using UnityEngine;
using System.Collections;

namespace unitycoder_MobilePaint
{
	public class ClearButton : MonoBehaviour {

		public GameObject painter;

		void OnMouseDown()
		{
			// send message to clear image
			painter.GetComponent<MobilePaint>().ClearImage();
		}

	}
}