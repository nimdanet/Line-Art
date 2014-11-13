// simple palette system: Take palette color from image (using GetPixel), set color in main painter gameobject

using UnityEngine;
using System.Collections;

namespace unitycoder_MobilePaint
{

	public class ColorPicker : MonoBehaviour 
	{

		public GameObject canvas;
		public GameObject paletteButton;
		public GameObject selectedColor;
		public GameObject previewColor;

		public bool closeAfterPick = true;

		private Texture2D tex;

		// for GUIScaling
		public float scaleAdjust = 1.0f;
		private const float BASE_WIDTH = 800;
		private const float BASE_HEIGHT = 480;
		private float ratio=1.0f;
		public int cellSize = 32; // palette color cell height (in the texture)

		// init
		void Awake () 
		{

			// calculate scaling ratio for different screen resolutions
			float _baseHeightInverted = 1.0f/BASE_HEIGHT;
			ratio = (Screen.height * _baseHeightInverted)*scaleAdjust;

			tex = (Texture2D)guiTexture.texture;

			if (canvas==null)
			{
				Debug.LogError("Canvas gameObject not found - Have you assigned it?");
			}

		} // awake


		// guitexture clicked, TODO: nicer if could hold button down..
		void OnMouseDown()
		{
			// get mouse hit position, inside non-scaled guitexture
			Vector3 hitPos = Input.mousePosition;// - corner;
			Rect guiRect= guiTexture.GetScreenRect();
			Color hitColor = tex.GetPixel((int)((hitPos.x-guiRect.x)/ratio),(int)((hitPos.y-guiRect.y)/ratio));
			canvas.GetComponent<MobilePaint>().SetPaintColor(hitColor);
			selectedColor.guiTexture.color = hitColor*0.5f; // half the color, otherwise too bright..unity feature
			previewColor.guiTexture.color = hitColor*0.5f;

			// close palette dialog slowly (to avoid accidental drawing after palette click)
			if (closeAfterPick)
			{
				guiTexture.enabled = false; // hide guitexture just to show something is happening
				Invoke("DelayedToggle",0.0f);
			}

		} // onmousedown

		void DelayedToggle()
		{
			paletteButton.GetComponent<PaletteDialog>().ToggleModalBackground();
			guiTexture.enabled = true;
		}


	} // class
} // namespace
