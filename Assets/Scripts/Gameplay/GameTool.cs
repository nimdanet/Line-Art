using UnityEngine;
using System.Collections;

public class GameTool : MonoBehaviour 
{
	public enum Functionality
	{
		None,
		Pencil,
		Brush,
		Eraser,
		Tint,
		NewPage,
	}

	public Functionality functionality;

	public void SelectTool()
	{
		if(functionality == Functionality.Pencil)
			SelectPencil();
		else if(functionality == Functionality.Brush)
			SelectBrush();
		else if(functionality == Functionality.Eraser)
			SelectEraser();
		else if(functionality == Functionality.Tint)
			SelectTint();
		else if(functionality == Functionality.NewPage)
			SelectNewPage();
	}

	private void SelectPencil()
	{
		unitycoder_MobilePaint.MobilePaint.Instance.drawMode = unitycoder_MobilePaint.MobilePaint.DrawMode.Draw;
	}

	private void SelectBrush()
	{
		unitycoder_MobilePaint.MobilePaint.Instance.drawMode = unitycoder_MobilePaint.MobilePaint.DrawMode.CustomBrush;
	}

	private void SelectEraser()
	{
		unitycoder_MobilePaint.MobilePaint.Instance.drawMode = unitycoder_MobilePaint.MobilePaint.DrawMode.Eraser;
	}

	private void SelectTint()
	{
		unitycoder_MobilePaint.MobilePaint.Instance.drawMode = unitycoder_MobilePaint.MobilePaint.DrawMode.FloodFill;
	}

	private void SelectNewPage()
	{
		unitycoder_MobilePaint.MobilePaint.Instance.ClearImage ();
	}
}
