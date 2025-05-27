#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.UI;
using System;

namespace EditorPackages.UnnioTemp
{
	public class gui_UnnioTempFeatures //this way the template dosent need the gui_UnnioFeatures which may be bad if we change something about how one of the feature work and I forget to make thoese changes to how it being used here
    {
        #region boxes
        public static void IndeterminateProgressBar(Color bg, Color fg, float progress, int width, int height)
        {
            GUIStyle returnStyle = new GUIStyle();
            Texture2D textureBody = new Texture2D(width, height); //makes gui box
            int minSize = width /60;
            int maxSize = width /3;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Color col = bg; //if not in bar part this is the colour
                    
                    int size = (int)Mathf.Lerp( minSize, maxSize, 1.0f -(Math.Abs(progress -0.5f) /0.5f)); //makes the bar parot of the progress bar bigger if it is closer to the center
                    int barPos = (int)((float)(width +(minSize * 2)) * progress) - minSize; //the locataion of the bar part

                    if (x >= barPos - size && x <= barPos + size) //if in bar part change color to fg
                    { 
                        col = fg;
                    }

                    textureBody.SetPixel(x, y, col);
                }
            }

            textureBody.Apply();
            returnStyle.normal.background = textureBody;

            GUILayout.Label("", returnStyle, GUILayout.Width(width), GUILayout.Height(height)); //I thougth it be best not to return a style for this one
        }

        public static bool InvalidFieldAlert(bool render, int width, int height = 25, string displayText = "invalid input detected")
        {
            int boarderSize = 2;

            Color bgCol = new Color(1.0f, 0.31f, 0.31f, 1.0f);
            Color fgCol = new Color(0.85f, 0.0f, 0.0f, 1.0f);

            if (render == true)
            {
                GUIStyle buttonStyle = new GUIStyle();
                Texture2D textureBody = new Texture2D(width, height); //makes gui red box

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (x < boarderSize || y < boarderSize || x >= width - boarderSize || y >= height - boarderSize)
                        {
                            textureBody.SetPixel(x, y, fgCol); //border colour
                        }
                        else
                        {
                            textureBody.SetPixel(x, y, bgCol); //background color

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
                if (GUILayout.Button(displayText, buttonStyle, GUILayout.Width(width - height), GUILayout.Height(height)) == true) { render = false; }

                buttonStyle.fontSize = 18;
                buttonStyle.fontStyle = FontStyle.Bold; //renders exit
                buttonStyle.alignment = TextAnchor.MiddleCenter;
                if (GUILayout.Button("X", buttonStyle, GUILayout.Width(height), GUILayout.Height(height)) == true) { render = false; }
                GUILayout.EndHorizontal();
            }

            return render;
        }

        public static GUIStyle BoxWithColor(Color fgCol)
        {
            GUIStyle returnStyle = new GUIStyle();
            Texture2D textureBody = new Texture2D(1, 1); //makes gui box

            textureBody.SetPixel(0, 0, fgCol);
            textureBody.filterMode = FilterMode.Point;
            textureBody.Apply();

            returnStyle.normal.background = textureBody;

            return returnStyle;
        }
        #endregion

        #region fonts
        public static GUIStyle TitleTextStyle()
        {
            GUIStyle textStyle = new GUIStyle();

            textStyle.normal.textColor = new Color(0.95f, 0.95f, 0.95f);
            textStyle.fontSize = 48;
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

        public static GUIStyle NormalTextStyle()
        {
            GUIStyle textStyle = new GUIStyle();

            textStyle.normal.textColor = new Color(0.95f, 0.95f, 0.95f);
            textStyle.fontSize = 24;
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

        public static GUIStyle DetailTextStyle()
        {
            GUIStyle textStyle = new GUIStyle();

            textStyle.normal.textColor = new Color(0.75f, 0.75f, 0.75f);
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
    }
}

#endif