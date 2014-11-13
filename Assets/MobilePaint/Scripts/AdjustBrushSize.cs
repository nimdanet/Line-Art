// simple GUITexture dragbar for brush size

using UnityEngine;
using System.Collections;

namespace unitycoder_MobilePaint
{

	public class AdjustBrushSize : MonoBehaviour {

		public GameObject painter; // our main paint plane reference

		public GUITexture indicator; // current size indicator
		private int minSize = 1; // min brush radius
		private int maxSize = 64; // max brush radius
		private float sizeScaler = 1; // temporary variable to calculate scale


		// init
		void Awake () 
		{
			if (painter==null)
			{
				Debug.LogError("Painter gameObject not found - Have you assigned it?");
			}

			// calculate current indicator position
			minSize = painter.GetComponent<unitycoder_MobilePaint.MobilePaint>().brushSizeMin;
			maxSize = painter.GetComponent<unitycoder_MobilePaint.MobilePaint>().brushSizeMax;
			sizeScaler = maxSize/guiTexture.pixelInset.height;
//			float borderOffsetY = (painter.GetComponent<unitycoder_MobilePaint.MobilePaint>().brushSize-1)/sizeScaler+guiTexture.pixelInset.y;
			float borderOffsetY = (painter.GetComponent<unitycoder_MobilePaint.MobilePaint>().brushSize-1-minSize)/sizeScaler+guiTexture.pixelInset.y;
			indicator.pixelInset = new Rect(indicator.pixelInset.x,borderOffsetY,indicator.pixelInset.width,indicator.pixelInset.height);
		}


		// guitexture is dragged, update indicator position & brush size variable in painter gameobject
		void OnMouseDrag()
		{
			float borderOffsetY = Mathf.Clamp((int)(Input.mousePosition.y),guiTexture.pixelInset.y,guiTexture.pixelInset.y+guiTexture.pixelInset.height);
			painter.GetComponent<unitycoder_MobilePaint.MobilePaint>().SetBrushSize((int)Mathf.Clamp( ((borderOffsetY-guiTexture.pixelInset.y)*sizeScaler), minSize, maxSize));
			indicator.pixelInset = new Rect(indicator.pixelInset.x,borderOffsetY,indicator.pixelInset.width,indicator.pixelInset.height);
		}
	}
}