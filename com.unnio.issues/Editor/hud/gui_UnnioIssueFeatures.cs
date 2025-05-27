#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace EditorPackages.UnnioIssues
{
	public class gui_UnnioIssueFeatures //this way the template dosent need the gui_UnnioFeatures which may be bad if we change something about how one of the feature work and I forget to make thoese changes to how it being used here
    {
        #region colour indercator parts
        public static GUIStyle ColorIndicatorStyleTop(int width, int height, Color col) //the color indercatorwas orignally splited into three part so the middle par can be easily streched to fill a space but this was not used in the end as it didnt look very good
        {
            GUIStyle returnStyle = new GUIStyle();
            Texture2D textureBody = new Texture2D(width, height);

            for (int x = 0; x < width; x++) //width should be as same as helight for this
            {
                for (int y = 0; y < height; y++)
                {
                    Color pixelCol = col;

                    if (x - 1 < (width / 2) - (height - y) || x + 1 > (width / 2) + (height - y)) //top left and righ cornors
                    {
                        pixelCol = new Color(0, 0, 0, 0);
                    }

                    textureBody.SetPixel(x, y, pixelCol);
                }
            }
            textureBody.Apply();

            returnStyle.normal.background = textureBody;

            return returnStyle;
        }

        public static GUIStyle ColorIndicatorStyleMiddle(Color col) //the strechable part
        {
            GUIStyle returnStyle = new GUIStyle();
            Texture2D textureBody = new Texture2D(1, 1);

            textureBody.SetPixel(0, 0, col);
            textureBody.Apply();

            returnStyle.normal.background = textureBody;

            return returnStyle;
        }

        public static GUIStyle ColorIndicatorStyleBottom(int width, int height, Color col, int ShadingExtent = 2, float shadingPower = 0.25f)
        {
            GUIStyle returnStyle = new GUIStyle();
            Texture2D textureBody = new Texture2D(width, height);


            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Color pixelCol = col;

                    if (y <= width)
                    {
                        if (x - 1 < (width / 2) - y || x + 1 > (width / 2) + y) //hides bottom left and right cornors
                        {
                            pixelCol = new Color(0, 0, 0, 0);
                        }

                        if (x - (ShadingExtent + 1) <= (width / 2) - y || x + (ShadingExtent + 1) >= (width / 2) + y) //applys shadeing
                        {
                            pixelCol = pixelCol * new Color(1.0f - shadingPower, 1.0f - shadingPower, 1.0f - shadingPower, 1.0f);
                        }
                    }

                    textureBody.SetPixel(x, y, pixelCol);
                }
            }
            textureBody.Apply();

            returnStyle.normal.background = textureBody;

            return returnStyle;
        }
        #endregion

        #region fonts
        public static GUIStyle TitleTextStyle() //font used in all cotainers and issues bellow the first level
        {
            GUIStyle textStyle = new GUIStyle();

            textStyle.normal.textColor = new Color(0.95f, 0.95f, 0.95f);
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

        public static GUIStyle TitleTextStyleBig() //font used in the first level containers
        {
            GUIStyle textStyle = new GUIStyle();

            textStyle.normal.textColor = new Color(0.95f, 0.95f, 0.95f);
            textStyle.fontSize = 32;
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

        public static GUIStyle NormalTextStyle() //issue details font
        {
            GUIStyle textStyle = new GUIStyle();

            textStyle.normal.textColor = new Color(0.95f, 0.95f, 0.95f);
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