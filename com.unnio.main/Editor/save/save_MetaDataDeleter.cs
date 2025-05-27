#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Runtime.InteropServices;
using UnityEditor.PackageManager.UI;

namespace EditorPackages.Unnio
{
    class save_MetaDataDeleter : AssetModificationProcessor //so meta data gets deleted as well if you foolishly delete it thougth the assets browser ... PS this use to be a paid for feature thank got for unity 6
    {
        #region on delete
        private static void DeleteBoardMetaDataFor(string pathToBoard)
        {
            int fileTypeLength = pathToBoard.Length - (pathToBoard.LastIndexOf(".") - 1); //add two to it to account for the dot //note if you try to use this in the save to file path it will not work
            string boardName = pathToBoard.Substring(pathToBoard.LastIndexOf("/") + 1, pathToBoard.Length - (pathToBoard.LastIndexOf("/") + fileTypeLength)); //gets us the board name
            string uID = pathToBoard.GetHashCode().ToString();
            string localTo = "/LocalTo_";

            if (Directory.Exists(hud_UnnioSaveEditor.GetMetaSaveLocation() + localTo + boardName + "_" + uID) == true) //dose it exsit
            {
                Directory.Delete(hud_UnnioSaveEditor.GetMetaSaveLocation() + localTo + boardName + "_" + uID, true); //burn it
            }
        }

        private static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions opt)
        {
            string currentSavePoint = hud_UnnioSaveEditor.GetBoardSaveLocation();
            string fullPath = path.Replace("Assets/", Application.dataPath + "/"); //we need the full path for the boardUID check to work and for directory.exsist

            if (File.Exists(fullPath) && fullPath.Substring(fullPath.LastIndexOf(".") + 1) == "unnboa") //for files
            {
                DeleteBoardMetaDataFor(fullPath);
            }

            if (fullPath == currentSavePoint)
            {
                hud_UnnioSaveEditor.SwitchSaveLocation(hud_UnnioSaveEditor.m_defaultSaveLocation);
            }

            if (Directory.Exists(fullPath) == true) //for folders
            {
                string[] allFiles = Directory.GetFiles(fullPath, "*.*", SearchOption.AllDirectories);
                for (int i = 0; i < allFiles.Length; i++) //loops thougth all files in project
                {
                    if (File.Exists(allFiles[i]) && allFiles[i].Substring(allFiles[i].LastIndexOf(".") + 1) == "unnboa")
                    {
                        DeleteBoardMetaDataFor(allFiles[i]);
                    }

                    if (allFiles[i] == currentSavePoint)
                    {
                        hud_UnnioSaveEditor.SwitchSaveLocation(hud_UnnioSaveEditor.m_defaultSaveLocation);
                    }
                }
            }


            return AssetDeleteResult.DidNotDelete;
        }
        #endregion

        #region on move
        private static AssetMoveResult OnWillMoveAsset(string from, string to) // so that if you rename a board the location the metadata is stored at gets renamed as wekk
        {
            string fullPathFrom = from.Replace("Assets/", Application.dataPath + "/"); //so we have the full path
            string fullPathTo = to.Replace("Assets/", Application.dataPath + "/");

            if (File.Exists(fullPathFrom) && fullPathFrom.Substring(fullPathFrom.LastIndexOf(".") + 1) == "unnboa" && fullPathTo.Substring(fullPathTo.LastIndexOf(".") + 1) == "unnboa") //to make it for our board files only
            {
                int fileTypeLength = fullPathFrom.Length - (fullPathFrom.LastIndexOf(".") - 1); //the length of the file extension
                string fromName = fullPathFrom.Substring(fullPathFrom.LastIndexOf("/") + 1, fullPathFrom.Length - (fullPathFrom.LastIndexOf("/") + fileTypeLength)); //name of file
                string toName = fullPathTo.Substring(fullPathTo.LastIndexOf("/") + 1, fullPathTo.Length - (fullPathTo.LastIndexOf("/") + fileTypeLength)); //name of file
                string fromUID = fullPathFrom.GetHashCode().ToString(); //path hash
                string toUID = fullPathTo.GetHashCode().ToString(); //new path hash
                string meta = hud_UnnioSaveEditor.GetMetaSaveLocation(); //where the meta data is
                string localTo = "/LocalTo_";

                if (fromName != toName && Directory.Exists(meta + localTo + fromName + "_" + fromUID) == true)
                {
                    Directory.Move(meta + localTo + fromName + "_" + fromUID, meta + localTo + toName + "_" + toUID);
                }
            }

            return AssetMoveResult.DidNotMove;
        }
        #endregion
    }
}

#endif