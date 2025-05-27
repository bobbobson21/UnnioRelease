#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.IO;
using System;

using UnityEditor.Callbacks;

namespace EditorPackages.Unnio
{
    class menu_CreateBoard
    {
        [OnOpenAsset(1)]
        public static bool OpenFileWithEditor(int fileId, int line)
        {
            UnityEngine.Object obj = EditorUtility.InstanceIDToObject(fileId);

            try
            {
                string filePath = AssetDatabase.GetAssetPath(fileId); //do where know where it is
                if (filePath.Substring(filePath.LastIndexOf(".") + 1) == "unnboa") //if it has the value name it is a unnio save even if it say false because if says false it might as well not exsist
                {
                    filePath = filePath.Replace("Assets/", "<ProjectAssetsFolder>/"); //to shorten the path

                    if (File.Exists(hud_UnnioSaveEditor.Parse(filePath)) == true)
                    {
                        hud_UnnioSaveEditor.SwitchSaveLocation(filePath, true); //loads it correctly //I know this is coupling but there not much that can be done about it for a project like this
                        hud_UnnioBoardEditor.OpenWindow(); //open

                        return true;
                    }
                }

            }
            catch (InvalidCastException)
            {
            }
            catch (ArgumentNullException)
            {
            }

            return false;
        }

        public static void CreateBoardWithContents(string contents, string path, string fileName = "NewBoard")
        {
            string fileType = ".unnboa";
            path += "/";

            int fileNameCount = 0;
            string fileNameCountAsString = "";

            while (File.Exists(path + fileName + fileNameCountAsString + fileType) == true) //so we dont accidently replace a file
            {
                fileNameCount++;
                fileNameCountAsString = $" ({fileNameCount})"; //give it a number at the end of its name and make sure its not a number beingused
            }

            string finishedFilePath = path + fileName + fileNameCountAsString + fileType;

            new FileInfo(finishedFilePath).Directory.Create(); //ensure we can actually save stuff
            File.WriteAllText(finishedFilePath, contents);

            AssetDatabase.ImportAsset(finishedFilePath);
        }

        [MenuItem("Assets/Create/Unnio/Create board", priority = 4)]
        public static void CreateBoard()
        {
            CreateBoardWithContents("{\n  " + '"' + "IS_UNNIO_SAVE" + '"' + ": true,\n  " + '"' + "STACKS" + '"' + ": []\n}", AssetDatabase.GetAssetPath(Selection.activeObject));
        }


        [MenuItem("Window/Unnio/Open built in tutorial board")]
        public static void OpenWindowTutorial()
        {
            string creationPath = "Assets/Editor";
            string path = hud_UnnioSaveEditor.Parse("<ProjectAssetsFolder>/Editor");
            string fileName = "TutorialBoard";
            string fileType = ".unnboa";

            if (File.Exists(path + "/" + fileName + fileType) == true)
            {
                File.Delete(path + "/" + fileName + fileType);
            }

            CreateBoardWithContents("{\n  " + '"' + "IS_UNNIO_SAVE" + '"' + ": true,\n  " + '"' + "IS_UNNIO_BUILT_IN_TUTORIAL" + '"' + ": true,\n  " + '"' + "STACKS" + '"' + ": []\n}", creationPath, fileName);
            hud_UnnioSaveEditor.SwitchSaveLocation(path + "/" + fileName + fileType, (board_BoardBase.GetSaveLocation() != path + "/" + fileName + fileType));

            hud_UnnioBoardEditor.OpenWindow();
        }

    }
}

#endif