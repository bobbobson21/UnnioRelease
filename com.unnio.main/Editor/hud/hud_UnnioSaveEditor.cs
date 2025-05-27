#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Runtime.InteropServices;

namespace EditorPackages.Unnio
{
    [InitializeOnLoad]
    public class hud_UnnioSaveEditor : EditorWindow
    {
        #region private vars: save locations
        private static string m_currentSaveLocation = "<ProjectAssetsFolder>/Editor/MainUnnioBoard.unnboa";
        private static string m_currentMetaSaveLocation = "<AppdataFolder>/meta/<ProjectName>_meta";
        private static string m_currentMetaSaveLocationLocalToBoard = m_currentMetaSaveLocation + "/LocalTo_<BoardName>_<BoardUID>"; //board uid will only parse in GetMetaSaveLocationLocalToBoard so it makes no sence to use it anywhere but here

        private string m_displaySaveLocation = "<ProjectAssetsFolder>/Editor/MainUnnioBoard.unnboa";
        #endregion

        #region private vars: display alert
        private string m_displayAlertText = "";
        private bool m_displayAlert = false;
        #endregion

        #region private var: scroll offset
        private Vector2 m_scrollOffset = Vector2.zero;
        #endregion


        #region public vars: save location
        public static string m_defaultSaveLocation { get { return "<ProjectAssetsFolder>/Editor/MainUnnioBoard.unnboa"; } }
        #endregion


        #region on window creation
        static hud_UnnioSaveEditor()
        {
            m_currentSaveLocation = GetBoardSaveLocationUnparsed(); //just encase
            board_BoardBase.LoadBoard(hud_UnnioSaveEditor.GetBoardSaveLocation()); //loads the default board into it
        }

        [MenuItem("Window/Unnio/Open board save editor")]
        public static void OpenWindow()
        {
            hud_UnnioSaveEditor window = CreateWindow<hud_UnnioSaveEditor>("Unnio: save editor"); //there is no harm in having more than one of these

            int newWindowWidth = 600;
            int newWindowHeight = 540;

            window.position = new Rect((Screen.currentResolution.width - newWindowWidth) / 2, (Screen.currentResolution.height - newWindowHeight) / 2, newWindowWidth, newWindowHeight);
        }

        private void Awake()
        {
            m_currentSaveLocation = GetBoardSaveLocationUnparsed(); //load current save location
            m_displaySaveLocation = m_currentSaveLocation;
        }
        #endregion

        private void OnGUI()
        {
            int crossSpacing = 3;
            int alertBoxHeight = 25;

            m_scrollOffset = EditorGUILayout.BeginScrollView(m_scrollOffset, false, false);
            m_displayAlert = gui_UnnioFeatures.InvalidFieldAlert(m_displayAlert, (int)position.width, alertBoxHeight, m_displayAlertText);

            GUILayout.BeginHorizontal(); //select what we are testing for
            GUILayout.Label("save to:", GUILayout.Width((position.width / 3) - crossSpacing));
            m_displaySaveLocation = EditorGUILayout.TextField(m_displaySaveLocation, GUILayout.Width(((position.width / 3) * 2) -(crossSpacing * 3)));
            GUILayout.EndHorizontal();

            GUILayout.Space(20);

            GUILayout.BeginVertical();
            GUILayout.Label("list of valid unnio path varibles:", EditorStyles.boldLabel);
            GUILayout.Label("   • <ProjectAssetsFolder>: Is the assets folder in your current project.");
            GUILayout.Label("   • <AppdataFolder>: The app data folder used to store your Unnio prefrences.");
            GUILayout.Label("   • <UserFolder>: The folder which contains content like the users desktop.");
            GUILayout.Label("   • <ProjectName>: the name of the current project.");
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("delete save data") == true) //deletes current board
            {
                if (EditorUtility.DisplayDialog("delete save", "are you sure you wish to DELETE you board save", "yes DELETE save", "no") == true)
                {
                    if(Directory.Exists(GetMetaSaveLocationLocalToBoard())) //dose current board have meta data
                    {
                        Directory.Delete(GetMetaSaveLocationLocalToBoard(),true); //if so delete that
                    }

                    board_BoardBase.DeleteBoard(GetBoardSaveLocation());
                }
            }

            if (GUILayout.Button("delete board preference data") == true)
            {
                if (EditorUtility.DisplayDialog("delete preference data", "click yes DELETE preference data for the current board", "yes DELETE preference data", "no") == true)
                {
                    if (Directory.Exists(GetMetaSaveLocationLocalToBoard())) //deletes only the meta data
                    {
                        Directory.Delete(GetMetaSaveLocationLocalToBoard(), true);
                    }
                }
            }

            if (GUILayout.Button("delete all preference data") == true)
            {
                if (EditorUtility.DisplayDialog("delete preference data", "click yes DELETE preference data if you will not be using unnio ever again", "yes DELETE preference data", "no") == true)
                {
                    string metaFolder = Parse("<AppdataFolder>/meta");

                    if (Directory.Exists(metaFolder) == true) //deletes all meta data
                    {
                        m_displaySaveLocation = m_defaultSaveLocation; //reset save location to default encase of errors
                        m_currentSaveLocation = m_defaultSaveLocation;

                        board_BoardBase.LoadBoard(Parse(m_defaultSaveLocation));
                        Directory.Delete(metaFolder,true);
                    }
                }
            }

            if (GUILayout.Button("reload current save") == true)
            {
                board_BoardBase.LoadBoard(); //loading a board with no file path should just reload it
            }

            if (GUILayout.Button("finish") == true)
            {
                bool completeFinish = true;
                bool invalidCharDetected = false;
                string parsedDisplayLocation = Parse(m_displaySaveLocation);

                if (m_displaySaveLocation.Substring(m_displaySaveLocation.LastIndexOf(".") + 1) != "unnboa") //is it of the right file type
                {
                    completeFinish = false;
                    m_displayAlertText = "invalid save location (save needs to be a unnboa file)";
                    m_displayAlert = true;
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) //is it in a valid location for windows
                {
                    int index = parsedDisplayLocation.LastIndexOf(":/");
                    if (parsedDisplayLocation.LastIndexOf(":\\") != 1 && parsedDisplayLocation.LastIndexOf(":/") != 1)
                    {
                        completeFinish = false;
                        m_displayAlertText = "invalid save location (invalid drive name or no drive name detected)";
                        m_displayAlert = true;
                    }
                }

                if (parsedDisplayLocation.Contains("*") == true) { invalidCharDetected = true; } //valid files and folders dont have these charters
                if (parsedDisplayLocation.Contains('"') == true) { invalidCharDetected = true; }
                if (parsedDisplayLocation.Contains("<") == true) { invalidCharDetected = true; }
                if (parsedDisplayLocation.Contains(">") == true) { invalidCharDetected = true; }
                if (parsedDisplayLocation.Contains("|") == true) { invalidCharDetected = true; }
                if (parsedDisplayLocation.Contains("?") == true) { invalidCharDetected = true; }
                if (parsedDisplayLocation.Split(":").Length > 2) { invalidCharDetected = true; }

                if (invalidCharDetected == true)
                {
                    completeFinish = false;
                    m_displayAlertText = "invalid save location (invalid characters detected in path)";
                    m_displayAlert = true;
                }

                if (parsedDisplayLocation.Contains("<BoardName>") == true) //are you using somthing you shouldnt be
                {
                    completeFinish = false;
                    m_displayAlertText = "invalid save location (<BoardName> IS NOT MENT TO BE USED BY MORTAL MEN)";
                    m_displayAlert = true;
                }

                if (parsedDisplayLocation.Contains("<BoardUID>") == true) //are you using somthing you shouldnt be
                {
                    completeFinish = false;
                    m_displayAlertText = "invalid save location (<BoardUID> your not allowed to touch this FOOL)";
                    m_displayAlert = true;
                }

                if (completeFinish == true) //if all is good then we close
                {
                    SwitchSaveLocation(m_displaySaveLocation);
                    Close();
                }
            }

            EditorGUILayout.EndScrollView();
        }

        #region parsing & save locations
        public static string Parse(string path) //replaces the envioment path wit the real path
        {
            path = path.Replace("<ProjectAssetsFolder>", Application.dataPath);
            path = path.Replace("<ProjectName>", System.IO.Directory.GetParent(Application.dataPath).Name);
            path = path.Replace("<AppdataFolder>", $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/Unity.Unnio");
            path = path.Replace("<UserFolder>", System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)));
            path = path.Replace("\\", "/"); //so its vialid for unity stuff

            int fileTypeLength = m_currentSaveLocation.Length -(m_currentSaveLocation.LastIndexOf(".") -1); //note if you try to use this in the save to file path it will not work
            path = path.Replace("<BoardName>", m_currentSaveLocation.Substring(m_currentSaveLocation.LastIndexOf("/") +1, m_currentSaveLocation.Length - (m_currentSaveLocation.LastIndexOf("/") + fileTypeLength)));

            return path;
        }

        public static string GetMetaSaveLocation() //where meta data relative to a unity project should be saved to (by meta data I mean data like what save was open last)
        {
            return Parse(m_currentMetaSaveLocation);
        }

        public static string GetMetaSaveLocationLocalToBoard() //where meta data relative to a given board in a unity project should be saved to
        {
            string path = m_currentMetaSaveLocationLocalToBoard;
            path = path.Replace("<BoardUID>", Parse(m_currentSaveLocation).GetHashCode().ToString()); //putting this in parse will cause a stack overflow so put it here
            return Parse(path);
        }

        public static string GetBoardSaveLocation()
        {
            string boardSaveLocationFile = "/SearchForSaveAt.pref";

            if (File.Exists(GetMetaSaveLocation() + boardSaveLocationFile) == true) //use the stuff at this location if it exsists
            {
                return Parse(File.ReadAllText(GetMetaSaveLocation() + boardSaveLocationFile));
            }

            return Parse(m_currentSaveLocation);
        }

        public static string GetBoardSaveLocationUnparsed()
        {
            string boardSaveLocationFile = "/SearchForSaveAt.pref";

            if (File.Exists(GetMetaSaveLocation() + boardSaveLocationFile) == true) //use the stuff at this location if it exsists
            {
                 return File.ReadAllText(GetMetaSaveLocation() + boardSaveLocationFile);
            }

            return m_currentSaveLocation;
        }

    public static void SwitchSaveLocation(string saveLocationNow, bool saveBeforSwitch = true)
        {
            string boardSaveLocationFile = "/SearchForSaveAt.pref";

            new FileInfo(GetMetaSaveLocation() + boardSaveLocationFile).Directory.Create(); //ensure we can actually save stuff
            File.WriteAllText( GetMetaSaveLocation() + boardSaveLocationFile, saveLocationNow ); //save it

            if (saveBeforSwitch == true && File.Exists(Parse(m_currentSaveLocation)) == true)
            {
                board_BoardBase.SaveBoard(); //saves stuff at the old save location just encase
            }

            m_currentSaveLocation = saveLocationNow;
            board_BoardBase.LoadBoard(Parse(m_currentSaveLocation)); //loads everything at the new save loaction
        }
    }
    #endregion
}

#endif