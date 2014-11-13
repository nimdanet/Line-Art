//  hides color dialog, with small delay to avoid accidental "click through"

using UnityEngine;
using System.Collections;

public class CloseColorDialog : MonoBehaviour {

	public GameObject paletteButton;

	void OnMouseDown()
	{
		Invoke("DelayedToggle",0.4f);
		guiTexture.enabled = false;
	}

	void DelayedToggle()
	{
		paletteButton.GetComponent<PaletteDialog>().ToggleModalBackground();
		guiTexture.enabled = true;
	}


}
