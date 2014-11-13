// Optimized Mobile Painter - Unitycoder.com

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace unitycoder_MobilePaint
{

	public class MobilePaint : MonoBehaviour 
	{
		public enum DrawMode
		{
			Draw,
			CustomBrush,
			FloodFill,
			Eraser,
		}

		private static MobilePaint instance;

		public static MobilePaint Instance
		{
			get { return instance; }
		}

	//	*** Default settings ***
		public Color32 paintColor = new Color32(255,0,0,255);
		public float resolutionScaler = 1.0f; // 1 means screen resolution, 0.5f means half the screen resolution
		public int brushSize = 24; // default brush size
		public int brushSizeMin = 1; // default min brush size
		public int brushSizeMax = 64; // default max brush size

		public DrawMode drawMode = DrawMode.Draw; // drawing modes: 0 = draw (default), 1 = custom brush, 2 = floodfill
		//public bool drawAfterFill = true; // TODO: return to drawing mode after first fill?

		public Vector2 screenSizeAdjust = new Vector2(-32,0); // this means, "ScreenResolution.xy+screenSizeAdjust.xy" (use only minus values, to add un-drawable border on right or bottom)
		public FilterMode filterMode = FilterMode.Point;

		// canvas clear color
		public Color32 clearColor = new Color32(255,255,255,255);
		
		// for using texture on canvas
		public bool useMaskImage=false;
		public Texture2D maskImage;
		
		// for using custom brushes
		public bool useCustomBrushes=true;
		public Texture2D[] customBrushes;
		public int selectedBrush = 0; // currently selected brush index
		private Color[] customBrushPixels;
		private byte[] customBrushBytes;
		private int customBrushWidth;
		private int customBrushHeight;
		private int customBrushWidthHalf;
//		private int customBrushHeightHalf;
		private int texWidthMinusCustomBrushWidth;
		private int texHeightMinusCustomBrushHeight;

		// for GUIScaling
		private float scaleAdjust = 1.0f;
		private const float BASE_WIDTH = 800;
		private const float BASE_HEIGHT = 480;


	//	*** private variables, no need to touch ***
		private byte[] pixels; // byte array for texture painting
		private byte[] maskPixels; // byte array for mask texture
		private Texture2D tex; // texture that we paint into
		private Texture2D maskTex; // texture used as a overlay mask
		private int texWidth;
		private int texHeight;
		private Touch touch; // touch reference
		private Camera cam;
		private RaycastHit hit;

		private Vector2 pixelUV; // with mouse
		private Vector2 pixelUVOld; // with mouse

		private Vector2[] pixelUVs; // mobiles
		private Vector2[] pixelUVOlds; // mobiles

		private bool needsUpdate = false; // if we have modified texture


	//	do initial setups
		void Awake () 
		{

			// calculate scaling ratio for different screen resolutions
			float _baseHeightInverted = 1.0f/BASE_HEIGHT;
			float ratio = (Screen.height * _baseHeightInverted)*scaleAdjust;
			screenSizeAdjust*=ratio;


			// WARNING: fixed maximum amount of touches, set to 20 here. Not sure if somehow can get maximum amount checked by code?
			pixelUVs = new Vector2[20];
			pixelUVOlds = new Vector2[20];

			// adjust cam size
			Camera.main.orthographicSize = Screen.height / 2;
			
			//Get camera component
			cam = Camera.main;
			
			// create mesh plane, fits in camera view (with screensize adjust taken into consideration)
			Mesh go_Mesh = GetComponent<MeshFilter>().mesh;
			go_Mesh.Clear();
			go_Mesh.vertices = new [] {
								cam.ScreenToWorldPoint(new Vector3(0, screenSizeAdjust.y, cam.nearClipPlane + 0.1f)),
								cam.ScreenToWorldPoint(new Vector3(0, cam.pixelHeight+screenSizeAdjust.y, cam.nearClipPlane + 0.1f)),
								cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth+screenSizeAdjust.x, cam.pixelHeight+screenSizeAdjust.y, cam.nearClipPlane + 0.1f)),
								cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth+screenSizeAdjust.x, 0, cam.nearClipPlane + 0.1f))
			                    };
			go_Mesh.uv = new [] {new Vector2(0, 0), new Vector2(0, 1),new Vector2(1, 1), new Vector2(1, 0)};
			go_Mesh.triangles = new  [] {0, 1, 2, 0, 2, 3};

			// add mesh collider
			gameObject.AddComponent<MeshCollider>();
			

			// create texture
			if (useMaskImage)
			{
				maskTex = maskImage;
				// calculate texture size
				texWidth = maskTex.width;
				texHeight = maskTex.height;
				renderer.material.SetTexture("_MaskTex", maskTex);

			}
			else
			{ 

				// calculate texture size, instead of taking mask size
				texWidth = (int)(Screen.width*resolutionScaler+screenSizeAdjust.x);
				texHeight = (int)(Screen.height*resolutionScaler+screenSizeAdjust.y);


			}

			// create new texture
			tex = new Texture2D(texWidth, texHeight, TextureFormat.RGBA32, false);
			tex.filterMode = filterMode;

			renderer.material.SetTexture("_MainTex", tex);

			// do this for new texture or existing texture
			tex.wrapMode = TextureWrapMode.Clamp;


			// init pixels array
			pixels = new byte[texWidth * texHeight * 4];
			maskPixels = new byte[texWidth * texHeight * 4];

			// initialize our texture

			if (useMaskImage)
			{
				ReadMaskImage();
			}

			ClearImage(false);



		} // awake
		
		void Start()
		{
			instance = this;
		}

		public void SetPaintColor(Color newColor)
		{
			SetPaintColor (newColor, true);
		}

		public void SetPaintColor(Color newColor, bool sendRPC)
		{
			paintColor = newColor;

			if(sendRPC)
				NetworkController.Instance.SetPaintColor(paintColor);
		}

		public void SetBrushSize(int size)
		{
			SetBrushSize (size, true);
		}

		public void SetBrushSize(int size, bool sendRPC)
		{
			brushSize = size;
			
			if(sendRPC)
				NetworkController.Instance.SetBrushSize(brushSize);
		}

		public void SetCustomBrush(int selGridInt)
		{
			SetCustomBrush (selGridInt, true);
		}

		public void SetCustomBrush(int selGridInt, bool sendRPC)
		{
			selectedBrush = selGridInt;
			GetCurrentBrush();
			
			if(sendRPC)
				NetworkController.Instance.SetCustomBrush(selectedBrush);
		}

		// *** mainloop ***
		void Update () 
		{
			if(!GameController.canDraw) return;

			#if UNITY_EDITOR || UNITY_WEBPLAYER

			// *** Mouse paint - START
			if (Input.GetMouseButton (0))
			{
				// Only if we hit something, then we continue
				if (!Physics.Raycast (Camera.main.ScreenPointToRay(Input.mousePosition), out hit)) return;

				// take previous value, so can compare them
				pixelUVOld = pixelUV;

				pixelUV = hit.textureCoord;
				pixelUV.x *= texWidth;
				pixelUV.y *= texHeight;

				// lets paint where we hit
				switch ((int)drawMode)
				{
					case 0: // drawing
						DrawCircle((int)pixelUV.x, (int)pixelUV.y);
						NetworkController.Instance.DrawCircle(pixelUV.x, pixelUV.y);

					break;

					case 1: // custom brush
						DrawCustomBrush((int)pixelUV.x, (int)pixelUV.y);
						NetworkController.Instance.DrawCustomBrush(pixelUV.x, pixelUV.y);
					break;

					case 2: // floodfill
						FloodFill((int)pixelUV.x, (int)pixelUV.y);
						NetworkController.Instance.FloodFill(pixelUV.x, pixelUV.y);
					break;

					case 3: // eraser
						DrawCircle((int)pixelUV.x, (int)pixelUV.y, Color.white);
						NetworkController.Instance.DrawWhiteCircle(pixelUV.x, pixelUV.y);
					break;

					default: // unknown mode
					break;
				}

				// set flag that texture needs to be applied
				needsUpdate = true;
			}

			// mouse pressed, take this position as start position
			if (Input.GetMouseButtonDown (0))
			{
				if (!Physics.Raycast (Camera.main.ScreenPointToRay(Input.mousePosition), out hit)) return;
				pixelUVOld = pixelUV;
			}


			// check distance from previous drawing point
			if (Vector2.Distance(pixelUV,pixelUVOld)>brushSize)
			{

				switch ((int)drawMode)
				{
					case 0: // drawing
						DrawLine(pixelUVOld, pixelUV);
						NetworkController.Instance.DrawLine(pixelUVOld, pixelUV);
						break;
					case 3: // eraser
						DrawLine(pixelUVOld, pixelUV, Color.white);
						NetworkController.Instance.DrawWhiteLine(pixelUVOld, pixelUV);
					break;

					default: // unknown mode
						break;
				}


				//DrawLineWidth(pixelUVOld, pixelUV,16);
				pixelUVOld = pixelUV;
				// set flag that texture needs to be applied
				needsUpdate = true;
			}
			// Mouse paint END ***
		



			#elif UNITY_IPHONE || UNITY_ANDROID

			// Touch paint - START

			int i = 0;

			// loop until all touches are processed
        	while (i < Input.touchCount) 
        	{
				// get current touch [#]
				touch = Input.GetTouch(i);

				// check state
				if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Began)
				{
					// do raycast on touch position
					if (Physics.Raycast (Camera.main.ScreenPointToRay(touch.position), out hit)) 
					{
						// take previous value, so can compare them
						pixelUVOlds[touch.fingerId] = pixelUVs[touch.fingerId];

						// get hit texture coordinate
						pixelUVs[touch.fingerId] = hit.textureCoord;
						pixelUVs[touch.fingerId].x *= texWidth;
						pixelUVs[touch.fingerId].y *= texHeight;
						
						// paint where we hit
						switch (drawMode)
						{
						case 0: // drawing
							DrawCircle((int)pixelUVs[touch.fingerId].x, (int)pixelUVs[touch.fingerId].y);
							break;
							
						case 1: // custom brush
							DrawCustomBrush((int)pixelUVs[touch.fingerId].x, (int)pixelUVs[touch.fingerId].y);
							break;
							
						case 2: // floodfill
							FloodFill((int)pixelUVs[touch.fingerId].x, (int)pixelUVs[touch.fingerId].y);
							break;
							
						default: // unknown mode
							break;
						}

						// set flag that texture needs to be applied
						needsUpdate = true;
					}
				}

				// if we just touched screen, set this finger id texture paint start position to that place
				if (touch.phase == TouchPhase.Began)
				{
					pixelUVOlds[touch.fingerId] = pixelUVs[touch.fingerId];
				}


				// check distance from previous drawing point
				if (Vector2.Distance(pixelUVs[touch.fingerId],pixelUVOlds[touch.fingerId])>brushSize)
				{

					switch (drawMode)
					{
					case 0: // drawing
						DrawLine(pixelUVOlds[touch.fingerId], pixelUVs[touch.fingerId]);
						break;
						
					default: // unknown mode, set back to 0
						break;
					}

					// set flag that texture needs to be applied
					needsUpdate = true;
				}

				// loop all touches
				i++;
			}
			// touch paint - end



			#endif

			
			
			// check flag if texture needs to be applied
			if (needsUpdate)
			{
				needsUpdate = false;
				tex.LoadRawTextureData(pixels);
				tex.Apply(false);
			}
		} // Update()




		// TEST: simple GUI scrollbar to adjust brush size
//		void OnGUI() 
//		{
//			brushSize = (int)GUI.VerticalSlider(new Rect(Screen.width-24, Screen.height-200, 100, 128), brushSize, 64.0F, 1.0F);
//		}


		public void UpdateTextures()
		{
			tex.LoadRawTextureData(pixels);
			tex.Apply(false);
		}

		// init/clear image, this can be called outside this script also
		public void ClearImage()
		{
			ClearImage (true);
			
		} // clear image

		public void ClearImage(bool sendRPC)
		{
			int pixel = 0;
			for (int y = 0; y < texHeight; y++) 
			{
				for (int x = 0; x < texWidth; x++) 
				{
					pixels[pixel] = clearColor.r;
					pixels[pixel+1] = clearColor.g;
					pixels[pixel+2] = clearColor.b;
					pixels[pixel+3] = 0;
					pixel += 4;
				}
			}
			tex.LoadRawTextureData(pixels);
			tex.Apply(false);

			if(sendRPC)
				NetworkController.Instance.ClearImage();

		} // clear image



		// init/read existing image, this can be called outside this script also
		public void ReadMaskImage()
		{

			int pixel = 0;
			for (int y = 0; y < texHeight; y++) 
			{
				for (int x = 0; x < texWidth; x++) 
				{

					Color c = maskTex.GetPixel(x,y);

					maskPixels[pixel] = (byte)(c.r*255);
					maskPixels[pixel+1] = (byte)(c.g*255);
					maskPixels[pixel+2] = (byte)(c.b*255);
					maskPixels[pixel+3] = 0; //(int)(c.r*255); // TODO: alpha
					pixel += 4;
				}
			}
			//tex.LoadRawTextureData(pixels);
			//tex.Apply(false);
		} // read image


		// main painting function, http://stackoverflow.com/a/24453110
		public void DrawCircle(int x,int y)
		{
			DrawCircle (x, y, false);
		}

		public void DrawCircle(int x,int y, bool update)
		{
			DrawCircle (x, y, paintColor, update);
		}

		public void DrawCircle (int x, int y, Color32 color)
		{
			DrawCircle (x, y, color, false);
		}

		public void DrawCircle(int x,int y, Color32 color, bool update)
		{
			Debug.Log (color);

			// clamp brush inside texture
			x = ClampBrushInt(x,texWidth-brushSize);
			y = ClampBrushInt(y,texHeight-brushSize);
			
			int pixel = 0;
			
			// draw fast circle: 
			int r2 = brushSize * brushSize;
			int area = r2 << 2;
			int rr = brushSize << 1;
			for (int i = 0; i < area; i++)
			{
				int tx = (i % rr) - brushSize;
				int ty = (i / rr) - brushSize;
				if (tx * tx + ty * ty < r2)
				{
					pixel = (texWidth*(y+ty)+x+tx)*4;
					pixels[pixel] = color.r;
					pixels[pixel+1] = color.g;
					pixels[pixel+2] = color.b;
					pixels[pixel+3] = 255;
				}
			}

			if(update)
				UpdateTextures();
		}


		
		// actual custom brush painting function
		public void DrawCustomBrush(int px,int py)
		{
			DrawCustomBrush (px, py, false);

		} // DrawCustomBrush


		public void DrawCustomBrush(int px, int py, bool update)
		{
			
			// TODO: this function needs comments/info..
			
			// get position where we paint
			int startX=(int)(px-customBrushWidthHalf);
			int startY=(int)(py-customBrushWidthHalf);
			
			if (startX<0) 
			{
				startX = 0;
			}else{
				if (startX+customBrushWidth>=texWidth) startX = texWidthMinusCustomBrushWidth;
			}
			
			if (startY<1)  // TODO: temporary fix, 1 instead of 0
			{
				startY = 1;
			}else{
				if (startY+customBrushHeight>=texHeight) startY = texHeightMinusCustomBrushHeight;
			}
			
			int pixel = 0;
			
			// could use this for speed? (but then its box shape..)
			//System.Array.Copy(splatPixByte,0,data,4*(startY*startX),splatPixByte.Length);
			
			
			pixel = (texWidth*(startY==0?1:startY+1)+startX+1)*4;
			float newColor;
			
			//
			for (int y = 0; y < customBrushHeight; y++) 
			{
				for (int x = 0; x < customBrushWidth; x++) 
				{
					newColor = (customBrushPixels[x*customBrushWidth+y].a);
					
					if (newColor>0)
					{
						// TODO: opacity / additive support..
						
						pixels[pixel] = paintColor.r;
						pixels[pixel+1] = paintColor.g;
						pixels[pixel+2] = paintColor.b;
						pixels[pixel+3] = 255; //(byte)Mathf.Clamp((int)data[pixel+3]+newCol,0,255);
					} // if got color
					
					pixel+= 4;
					
				} // for x
				
				pixel = (texWidth*(startY==0?1:startY+y)+startX+1)*4;
			} // for y

			if(update)
				UpdateTextures();
		}


		//
		public void DrawPoint(int x,int y)
		{
			DrawPoint (x, y, false);
		}

		//
		public void DrawPoint(int pixel)
		{
			DrawPoint (pixel, false);
		}

		public void DrawPoint(int x, int y, bool update)
		{
			int pixel = (texWidth*y+x)*4;
			
			pixels[pixel] = paintColor.r;
			pixels[pixel+1] = paintColor.g;
			pixels[pixel+2] = paintColor.b;
			pixels[pixel+3] = 255;

			if(update)
				UpdateTextures();
		}

		public void DrawPoint(int pixel, bool update)
		{
			pixels[pixel] = paintColor.r;
			pixels[pixel+1] = paintColor.g;
			pixels[pixel+2] = paintColor.b;
			pixels[pixel+3] = 255;

			if(update)
				UpdateTextures();
		}

		// draw line between 2 points (if moved too far/fast)
		// http://en.wikipedia.org/wiki/Bresenham%27s_line_algorithm
		public void DrawLine(Vector2 start, Vector2 end)
		{
			DrawLine (start, end, false);
		} // drawline

		public void DrawLine(Vector2 start, Vector2 end, bool update)
		{
			DrawLine (start, end, paintColor, update);
		} // drawline

		public void DrawLine(Vector2 start, Vector2 end, Color32 color)
		{
			DrawLine (start, end, color, false);
		} // drawline

		public void DrawLine(Vector2 start, Vector2 end, Color32 color, bool update)
		{
			int x0=(int)start.x;
			int y0=(int)start.y;
			int x1=(int)end.x;
			int y1=(int)end.y;
			int dx= Mathf.Abs(x1-x0);
			int dy= Mathf.Abs(y1-y0);
			int sx,sy;
			if (x0 < x1) {sx=1;}else{sx=-1;}
			if (y0 < y1) {sy=1;}else{sy=-1;}
			int err=dx-dy;
			bool loop=true;
			//			int minDistance=brushSize-1;
			int minDistance=(int)(brushSize*0.5f);
			int pixelCount=0;
			int e2;
			while (loop) 
			{
				pixelCount++;
				if (pixelCount>minDistance)
				{
					pixelCount=0;
					DrawCircle(x0,y0, color);
				}
				if ((x0 == x1) && (y0 == y1)) loop=false;
				e2 = 2*err;
				if (e2 > -dy)
				{
					err = err - dy;
					x0 = x0 + sx;
				}
				if (e2 <  dx)
				{
					err = err + dx;
					y0 = y0 + sy;
				}
			}

			if(update)
				UpdateTextures();
		} // drawline


		// http://members.chello.at/~easyfilter/bresenham.html
		public void DrawLineWidth(Vector2 start, Vector2 end, float wd)
		//		void DrawLineWidth(int x0, int y0, int x1, int y1, float wd)
		{ 
			int x0=(int)start.x;
			int y0=(int)start.y;
			int x1=(int)end.x;
			int y1=(int)end.y;

			int dx = Mathf.Abs(x1-x0), sx = x0 < x1 ? 1 : -1; 
			int dy = Mathf.Abs(y1-y0), sy = y0 < y1 ? 1 : -1; 
			int err = dx-dy, e2, x2, y2;                          /* error value e_xy */
			float ed = dx+dy == 0 ? 1 : Mathf.Sqrt((float)dx*dx+(float)dy*dy);
			
			for (wd = (wd+1)/2;;) 
			{                                   /* pixel loop */
//				setPixelColor(x0,y0,max(0,255*(abs(err-dx+dy)/ed-wd+1));
				DrawPoint(x0, y0);
				e2 = err;
				x2 = x0;
				if (2*e2 >= -dx) 
				{                                           /* x step */
					for (e2 += dy, y2 = y0; e2 < ed*wd && (y1 != y2 || dx > dy); e2 += dx)
					{
//						setPixelColor(x0, y2 += sy, max(0,255*(abs(e2)/ed-wd+1));
						DrawPoint(x0, y2 += sy);
					}
						if (x0 == x1) break;
						e2 = err; err -= dy; x0 += sx; 
				} 

				if (2*e2 <= dy) 
				{                                            /* y step */
					for (e2 = dx-e2; e2 < ed*wd && (x1 != x2 || dx < dy); e2 += dy)
					{
//						setPixelColor(x2 += sx, y0, max(0,255*(abs(e2)/ed-wd+1));
						DrawPoint(x2 += sx, y0);
					}
					if (y0 == y1) break;
					err += dx; y0 += sy; 
				}
			}
		} // DrawLineWidth



		// clamp int value, http://stackoverflow.com/a/3040551
		int ClampBrushInt(int value, int max)
		{
			return (value < brushSize) ? brushSize : (value > max) ? max : value;
		}



		// basic floodfill
		public void FloodFill(int x,int y)
		{
			FloodFill (x, y, false);
		} // floodfill

		public void FloodFill(int x,int y, bool update)
		{
			//			System.Diagnostics.Stopwatch stopwatch  = new System.Diagnostics.Stopwatch();
			//			stopwatch.Start();
			
			// get canvas hit color (alpha)
			byte hitColorR = pixels[ ((texWidth*(y)+x)*4) +0 ];
			byte hitColorG = pixels[ ((texWidth*(y)+x)*4) +1 ];
			byte hitColorB = pixels[ ((texWidth*(y)+x)*4) +2 ];
			byte hitColorA = pixels[ ((texWidth*(y)+x)*4) +3 ];
			
			// early exit if its same color already
			if (paintColor.r == hitColorR && paintColor.g == hitColorG && paintColor.b == hitColorB && paintColor.a == hitColorA)
			{
				return;
			}
			
			Queue<int> qptsx = new Queue<int>();
			Queue<int> qptsy = new Queue<int>();
			qptsx.Enqueue(x);
			qptsy.Enqueue(y);
			
			int ptsx,ptsy;
			int pixel = 0;
			
			while (qptsx.Count > 0)
			{
				
				ptsx = qptsx.Dequeue();
				ptsy = qptsy.Dequeue();
				
				if (ptsy-1>-1)
				{
					pixel = (texWidth*(ptsy-1)+ptsx)*4; // down
					if (pixels[pixel+0]==hitColorR && pixels[pixel+1]==hitColorG && pixels[pixel+2]==hitColorB)
					{
						qptsx.Enqueue(ptsx);
						qptsy.Enqueue(ptsy-1);
						DrawPoint(pixel);
					}
				}
				
				if (ptsx+1<texWidth)
				{
					pixel = (texWidth*ptsy+ptsx+1)*4; // right
					if (pixels[pixel+0]==hitColorR && pixels[pixel+1]==hitColorG && pixels[pixel+2]==hitColorB)
					{
						qptsx.Enqueue(ptsx+1);
						qptsy.Enqueue(ptsy);
						DrawPoint(pixel);
					}
				}
				
				if (ptsx-1>-1)
				{
					pixel = (texWidth*ptsy+ptsx-1)*4; // left
					if (pixels[pixel+0]==hitColorR && pixels[pixel+1]==hitColorG && pixels[pixel+2]==hitColorB)
					{
						qptsx.Enqueue(ptsx-1);
						qptsy.Enqueue(ptsy);
						DrawPoint(pixel);
					}
				}
				
				if (ptsy+1<texHeight)
				{
					pixel = (texWidth*(ptsy+1)+ptsx)*4; // up
					if (pixels[pixel+0]==hitColorR && pixels[pixel+1]==hitColorG && pixels[pixel+2]==hitColorB)
					{
						qptsx.Enqueue(ptsx);
						qptsy.Enqueue(ptsy+1);
						DrawPoint(pixel);
					}
				}
			}

			if(update)
				UpdateTextures();
			
			//			stopwatch.Stop();
			//			Debug.Log("Timer: " + stopwatch.ElapsedMilliseconds);
			//			stopwatch.Reset();
		} // floodfill


		// get custom brush into array, TODO: need to optimize
		public void GetCurrentBrush()
		{
			customBrushPixels=customBrushes[selectedBrush].GetPixels();
			customBrushWidth=customBrushes[selectedBrush].width;
			customBrushHeight=customBrushes[selectedBrush].height;
			
			customBrushBytes = new byte[customBrushWidth * customBrushHeight * 4];
			
			int pixel = 0;
			for (int y = 0; y < customBrushHeight; y++) 
			{
				for (int x = 0; x < customBrushWidth; x++) 
				{

					// TODO: take from array
					Color sc = customBrushes[selectedBrush].GetPixel(x,y);

					customBrushBytes[pixel] = (byte)(sc.r*255);
					customBrushBytes[pixel+1] = (byte)(sc.g*255);
					customBrushBytes[pixel+2] = (byte)(sc.b*255);
					customBrushBytes[pixel+3] = (byte)(sc.a*255);
					pixel += 4;

				}
			}
			
			customBrushWidthHalf = (int)(customBrushWidth*0.5f);
//			customBrushHeightHalf = (int)(customBrushHeight*0.5f);
			
			texWidthMinusCustomBrushWidth = texWidth-customBrushWidth;
			texHeightMinusCustomBrushHeight = texHeight-customBrushHeight;
			
			//UpdateBrushPreview();
			
		} // GetBrush



	} // class
} // namespace
