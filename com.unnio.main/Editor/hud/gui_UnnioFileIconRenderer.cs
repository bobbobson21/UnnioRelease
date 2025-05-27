#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System;
using System.IO;

namespace EditorPackages.Unnio
{
    [InitializeOnLoad]
    public class gui_UnnioFileIconRenderer
    {
        #region private vars: icon data
        private static string m_iconPath = "Packages/com.unnio.main/Editor/textures/board.png";
        private static Texture m_textureData = null;
        #endregion


        #region event subscription
        static gui_UnnioFileIconRenderer() //start up
        {
            EditorApplication.projectWindowItemOnGUI += RenderFileIcon;
            EditorApplication.quitting += Exit;
        }

        private static void Exit() //exit
        {
            EditorApplication.projectWindowItemOnGUI -= RenderFileIcon;
            EditorApplication.quitting -= Exit;
        }
        #endregion

        private static void RenderFileIcon(string guid, Rect selectionRect)
        {
            if (m_textureData == null) //first time rendering icon
            {
                try
                {
                    Texture2D tempTexture = (Texture2D)EditorGUIUtility.Load(m_iconPath); //this should be repeated till unity sucessfully loads the icon //because it may take more than one attempt
                    tempTexture.filterMode = FilterMode.Trilinear;
                    m_textureData = (Texture)tempTexture;
                }
                catch (NullReferenceException)
                { 
                    return;
                }
            }


            string path = AssetDatabase.GUIDToAssetPath(guid); //get file

            if (path.Substring(path.LastIndexOf(".") + 1) == "unnboa") //is it a board file if so render icon
            {
                float padding = 10; //sizing and posistioning icon happens bellow
                float iconSize = Math.Max(Math.Min(selectionRect.width, selectionRect.height), 40) - padding;
                Rect fileRect = new Rect(selectionRect.x + (padding /2), selectionRect.y + (padding /2), iconSize, iconSize);

                if (iconSize >= 30 && iconSize <= 40)
                {
                    fileRect.width += 4;
                    fileRect.height += 4;
                }

                if (iconSize < 31)
                {
                    fileRect.x -= 4;
                    fileRect.y -= 4;
                }

                if (selectionRect.width <= 20 || selectionRect.height <= 20)
                {
                    fileRect = new Rect(selectionRect.x + 4, selectionRect.y, 15, 15);
                }

                GUI.DrawTexture(fileRect, m_textureData); //final rendering is done here
            }
        }
    }
}
#endif