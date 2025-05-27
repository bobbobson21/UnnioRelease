#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace EditorPackages.UnnioTut
{
	public class gui_UnnioTutFeatures //this way the tutorial dosent need the gui_UnnioFeatures which may be bad if we change something about how one of the feature work and I forget to make thoese changes to how it being used here
    {
        public static GUIStyle BoxWithColor(Color fgCol, Color bgCol, int width, int height = 60) //a box that can be collored with fg being the border color
        {
            int boarderSize = 1;

            GUIStyle returnStyle = new GUIStyle();
            Texture2D textureBody = new Texture2D(width, height); //makes gui box

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

        public static GUIStyle TitleTextStyle() //the title style used on the main page
        {
            GUIStyle textStyle = new GUIStyle();

            textStyle.normal.textColor = new Color(0.85f, 0.85f, 0.85f);
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
    }
}

#endif