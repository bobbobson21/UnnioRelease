#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;


//example code for me in case me forgets how it works because im a fool, im a tool, whos dead in a duel

/*Color[] r = new Color[2] { Color.red, new Color(1, 0.4f, 0, 1) }; 
Color[] o = new Color[2] { new Color(1, 0.4f, 0, 1), Color.yellow };
Color[] y = new Color[2] { Color.yellow, Color.green };
Color[] g = new Color[2] { Color.green, Color.blue };
Color[] b = new Color[2] { Color.blue, Color.magenta };
Color[] m = new Color[2] { Color.magenta, Color.red };

Color[][] colArray = new Color[][] { r, r, o, o, y, y, g, g, b, b, m, m };

GUILayout.BeginHorizontal();
GUILayout.Button("", gui_UnnioFeatures.ColorIndicatorStyle(80, 20, colArray), GUILayout.Width(80), GUILayout.Height(20));
GUILayout.Space(3);
GUILayout.Button("", gui_UnnioFeatures.ColorIndicatorStyle(80, 20, new Color(0, 0.75f, 1, 1)), GUILayout.Width(80), GUILayout.Height(20));
GUILayout.EndHorizontal();*/

namespace EditorPackages.Unnio
{
	public class gui_UnnioFeatures
	{
        #region boxes & alerts
        public static bool InvalidFieldAlert(bool render, int width, int height = 25, string displayText = "invalid input detected") //the red invalid box that can appear at the top
		{
			int boarderSize = 2;

			Color bgCol = new Color(1.0f, 0.31f, 0.31f, 1.0f);
            Color fgCol = new Color(0.85f, 0.0f, 0.0f, 1.0f);

            if (render == true) //should render
			{
				GUIStyle buttonStyle = new GUIStyle();
				Texture2D textureBody = new Texture2D(width, height); //makes gui red box

                for (int x = 0; x < width; x++) 
                {
                    for (int y = 0; y < height; y++)
                    {
						if (x < boarderSize || y < boarderSize || x >= width - boarderSize || y >= height - boarderSize)
						{
                            textureBody.SetPixel(x, y, fgCol);
						}
						else
						{
                            textureBody.SetPixel(x, y, bgCol);

                        }
                    }
                }
                textureBody.Apply();

                Texture2D textureInvis = new Texture2D(1, 1);
                textureInvis.SetPixel(0, 0, new Color(0, 0, 0, 0));
                textureInvis.Apply();

                buttonStyle.normal.background = textureBody; //applys background

                GUILayout.BeginHorizontal(buttonStyle); //renders button

                buttonStyle.normal.background = textureInvis; //renders text
				buttonStyle.normal.textColor = Color.white;
				buttonStyle.fontStyle = FontStyle.Bold;
                buttonStyle.alignment = TextAnchor.MiddleLeft;
				buttonStyle.contentOffset = new Vector2(boarderSize + boarderSize, 0);
                if (GUILayout.Button(displayText, buttonStyle, GUILayout.Width(width -height), GUILayout.Height(height)) == true) { render = false; } //should stop the rendering when we are done

                buttonStyle.fontSize = 18;
                buttonStyle.fontStyle = FontStyle.Bold; //renders exit
                buttonStyle.alignment = TextAnchor.MiddleCenter;
                if (GUILayout.Button("X", buttonStyle, GUILayout.Width(height), GUILayout.Height(height)) == true) { render = false; } //should stop the rendering when we are done
                GUILayout.EndHorizontal();
            }

			return render;
		}

        public static GUIStyle BoxWithColor(Color fgCol, Color bgCol, int width, int height) //a simple box that can be give two colors and fg is used as the outline color
        {
            int boarderSize = 1;

            GUIStyle returnStyle = new GUIStyle();
            Texture2D textureBody = new Texture2D(width, height); //makes gui box

            for (int x = 0; x < width; x++) //creates pixles
            {
                for (int y = 0; y < height; y++)
                {
                    if (x < boarderSize || y < boarderSize || x >= width - boarderSize || y >= height - boarderSize) //should it be fg or bg
                    {
                        textureBody.SetPixel(x, y, fgCol);
                    }
                    else
                    {
                        textureBody.SetPixel(x, y, bgCol);
                    }
                }
            }

            Color alphaInvis = new Color(0, 0, 0, 0);

            textureBody.SetPixel(0, 0, alphaInvis); //adds rounded cornos to box
            textureBody.SetPixel(width -1, 0, alphaInvis);
            textureBody.SetPixel(0, height -1, alphaInvis);
            textureBody.SetPixel(width -1, height -1, alphaInvis);

            textureBody.SetPixel(1, 1, fgCol); //adds rounded cornos to box
            textureBody.SetPixel(width -2, 1, fgCol);
            textureBody.SetPixel(1, height -2, fgCol);
            textureBody.SetPixel(width -2, height -2, fgCol);

            //textureBody.filterMode = FilterMode.Point;
            textureBody.Apply();

            returnStyle.normal.background = textureBody;

            return returnStyle;
        }
        #endregion

        #region fonts
        public static GUIStyle StackTitleTextStyle() //stack title font
        {
            GUIStyle textStyle = new GUIStyle();

            textStyle.normal.textColor = Color.white;
            textStyle.fontSize = 24;
            textStyle.fontStyle = FontStyle.Bold;
            textStyle.alignment = TextAnchor.UpperLeft;
            textStyle.contentOffset = new Vector2(2, 0);
            textStyle.wordWrap = true;

            if (EditorGUIUtility.isProSkin == false) //light mode color
            {
                textStyle.normal.textColor = Color.black;
            }

            return textStyle;
        }

        public static GUIStyle StackTitleVerticalTextStyle() //stack title font but for every charter being on a new line
        {
            GUIStyle textStyle = new GUIStyle();

            textStyle.normal.textColor = Color.white;
            textStyle.fontSize = 24;
            textStyle.fontStyle = FontStyle.Bold;
            textStyle.alignment = TextAnchor.UpperCenter;
            textStyle.contentOffset = new Vector2(2, 0);
            textStyle.wordWrap = true;

            if (EditorGUIUtility.isProSkin == false) //light mode color
            {
                textStyle.normal.textColor = Color.black;
            }

            return textStyle;
        }

        public static GUIStyle StackTitleAlphaTextStyle() //stack title font but for dragable UI
        {
            GUIStyle textStyle = new GUIStyle();

            textStyle.normal.textColor = new Color(1, 1, 1, 0.75f);
            textStyle.fontSize = 24;
            textStyle.fontStyle = FontStyle.Bold;
            textStyle.alignment = TextAnchor.UpperLeft;
            textStyle.contentOffset = new Vector2(2, 0);
            textStyle.wordWrap = true;

            if (EditorGUIUtility.isProSkin == false) //light mode color
            {
                textStyle.normal.textColor = Color.black;
            }

            return textStyle;
        }

        public static GUIStyle NodeTitleTextStyle() //node title font
        {
            GUIStyle textStyle = new GUIStyle();

            textStyle.normal.textColor = Color.white;
            textStyle.fontSize = 20;
            textStyle.fontStyle = FontStyle.Normal;
            textStyle.alignment = TextAnchor.UpperLeft;
            textStyle.contentOffset = new Vector2(2, 0);
            textStyle.wordWrap = true;

            if (EditorGUIUtility.isProSkin == false) //light mode color
            {
                textStyle.normal.textColor = Color.black;
            }

            return textStyle;
        }

        public static GUIStyle NodeTitleAlphaTextStyle() //node title font but for dragable
        {
            GUIStyle textStyle = new GUIStyle();

            textStyle.normal.textColor = new Color(1, 1, 1, 0.75f);
            textStyle.fontSize = 20;
            textStyle.fontStyle = FontStyle.Normal;
            textStyle.alignment = TextAnchor.UpperLeft;
            textStyle.contentOffset = new Vector2(2, 0);
            textStyle.wordWrap = true;

            if (EditorGUIUtility.isProSkin == false) //light mode color
            {
                textStyle.normal.textColor = Color.black;
            }

            return textStyle;
        }
        #endregion

        #region colour indercators
        public static GUIStyle ColorIndicatorStyle(int width, int height, Color col, int ShadingExtent = 2, float shadingPower = 0.25f) //the colour indercator used on the stacks and nodes
        {
            GUIStyle returnStyle = new GUIStyle();
            Texture2D textureBody = new Texture2D(width, height);

            for (int x = 0; x < width; x++) //width
            {
                for (int y = 0; y < height; y++) //height
                {
                    Color pixelCol = col;
                    bool shaded = false;

                    if (x <= height)
                    {
                        if (y < (height / 2) - x || y > (height / 2) + x) //hides cornors
                        {
                            pixelCol = new Color(0, 0, 0, 0);
                        }

                        if (y - ShadingExtent <= (height / 2) - x) //applys shadeing
                        {
                            shaded = true;
                            pixelCol = pixelCol * new Color(1.0f - shadingPower, 1.0f - shadingPower, 1.0f - shadingPower, 1.0f);
                        }
                    }

                    if (x >= width -height)
                    {
                        if (y <= (height / 2) - (width -x) || y >= (height / 2) + (width - x)) //hides cornors
                        {
                            pixelCol = new Color(0, 0, 0, 0);
                        }

                        if (y - (ShadingExtent +1) <= (height / 2) - (width - x)) //applys shadeing
                        {
                            shaded = true;
                            pixelCol = pixelCol * new Color(1.0f - shadingPower, 1.0f - shadingPower, 1.0f - shadingPower, 1.0f);
                        }
                    }

                    if (y < ShadingExtent && shaded == false) //applys shadeing
                    {
                        shaded = true;
                        pixelCol = pixelCol * new Color(1.0f - shadingPower, 1.0f - shadingPower, 1.0f - shadingPower, 1.0f);
                    }


                    textureBody.SetPixel(x, y, pixelCol);
                }
            }
            textureBody.Apply();
            
            returnStyle.normal.background = textureBody;

            return returnStyle;
        }

        public static GUIStyle ColorIndicatorStyle(int width, int height, Color[][] col, int ShadingExtent = 2, float shadingPower = 0.25f) //the colour indercator used on the stacks and nodes but it can support more than one colour
        {
            GUIStyle returnStyle = new GUIStyle();
            Texture2D textureBody = new Texture2D(width, height);

            int colX = 0;
            int colY = 0;

            for (int x = 0; x < width; x++)
            {
                colY = 0;
                colX ++;
                if(colX >= col.Length) //loops if ran out of colour values
                {
                    colX = 0;
                }

                for (int y = 0; y < height; y++)
                {
                    colY++;
                    if (colY >= col[colX].Length) //loops if ran out of colour values
                    {
                        colY = 0;
                    }

                    Color pixelCol = col[colX][colY];
                    bool shaded = false;

                    if (x <= height)
                    {
                        if (y < (height / 2) - x || y > (height / 2) + x) //hides cornors
                        {
                            pixelCol = new Color(0, 0, 0, 0);
                        }

                        if (y - ShadingExtent < (height / 2) - x) //applys shadeing
                        {
                            shaded = true;
                            pixelCol = pixelCol * new Color(1.0f - shadingPower, 1.0f - shadingPower, 1.0f - shadingPower, 1.0f);
                        }
                    }

                    if (x >= width - height)
                    {
                        if (y <= (height / 2) - (width - x) || y >= (height / 2) + (width - x)) //hides cornors
                        {
                            pixelCol = new Color(0, 0, 0, 0);
                        }


                        if (y - (ShadingExtent +1) <= (height / 2) - (width - x)) //applys shadeing
                        {
                            shaded = true;
                            pixelCol = pixelCol * new Color(1.0f - shadingPower, 1.0f - shadingPower, 1.0f - shadingPower, 1.0f);
                        }
                    }

                    if (y < ShadingExtent && shaded == false) //applys shadeing
                    {
                        shaded = true;
                        pixelCol = pixelCol * new Color(1.0f - shadingPower, 1.0f - shadingPower, 1.0f - shadingPower, 1.0f);
                    }


                    textureBody.SetPixel(x, y, pixelCol);
                }
            }
            textureBody.Apply();
            returnStyle.normal.background = textureBody;

            return returnStyle;
        }
        #endregion
    }
}

#endif