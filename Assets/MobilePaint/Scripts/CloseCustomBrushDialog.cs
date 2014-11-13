//  hides color dialog, with small delay to avoid accidental "click through"

using UnityEngine;
using System.Collections;

namespace unitycoder_MobilePaint
{
	public class CloseCustomBrushDialog : MonoBehaviour {

		public GameObject functionButton;

		void OnMouseDown()
		{
			Invoke("DelayedToggle",0.4f);
			guiTexture.enabled = false;
		}

		void DelayedToggle()
		{
			functionButton.GetComponent<CustomBrushDialog>().ToggleModalBackground();
			guiTexture.enabled = true;
		}


	}
}