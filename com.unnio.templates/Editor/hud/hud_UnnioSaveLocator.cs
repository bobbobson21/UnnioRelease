#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;
using EditorPackages.UnnioTemp;
using UnityEditor.PackageManager.UI;

namespace EditorPackages.Unnio
{
    public class hud_UnnioSaveLocator : EditorWindow
    {
        #region private vars: save & save location parsing
        private string m_currentSaveLocation = "";
        private Dictionary<string, string> m_addToParse = new Dictionary<string, string>();
        #endregion

        #region private vars: text displayed
        private string m_title = "";
        private string m_confirmText = "yes";
        private string m_cancelText = "no";
        private string m_fileType = "unnboa";
        #endregion

        #region private vars: display alert
        private string m_displayAlertText = "";
        private bool m_displayAlert = false;
        private bool m_wasCreatedCorrectly = false;
        #endregion


        #region public vars: events
        public delegate void EmptyDelegate();
        public event EmptyDelegate e_cancel;
        public event EmptyDelegate e_confirm;
        #endregion


        #region open window
        public static hud_UnnioSaveLocator OpenWindow(string title)
        {
            hud_UnnioSaveLocator window = CreateWindow<hud_UnnioSaveLocator>("Unnio: save editor: " + title);

            int newWindowWidth = 600;
            int newWindowHeight = 110;

            window.position = new Rect((Screen.currentResolution.width - newWindowWidth) / 2, (Screen.currentResolution.height - newWindowHeight) / 2, newWindowWidth, newWindowHeight);
            window.m_title = title;
            window.m_wasCreatedCorrectly = true;

            return window;
        }

        public static hud_UnnioSaveLocator OpenWindow(string title, string yes, string no) //opens the window with custom yes and no options
        {
            hud_UnnioSaveLocator win = OpenWindow(title);

            win.m_confirmText = yes;
            win.m_cancelText = no;

            return win;
        }

        public static hud_UnnioSaveLocator OpenWindow(string fileType, string title, string yes, string no) //opens the window with custom yes, no, title rename options and a file type option
        {
            hud_UnnioSaveLocator win = OpenWindow(title);

            win.m_fileType = fileType; 
            win.m_confirmText = yes;
            win.m_cancelText = no;
            
            return win;
        }
        #endregion

        private void Awake()
        {
            if (m_wasCreatedCorrectly != true) //this means unity is trying to reopen it from your last session
            {
                try
                {
                    Close();
                }
                catch (NullReferenceException)
                {
                    //do nothing it can not be removed
                }
            }
        }

        private void OnGUI()
        {
            m_displayAlert = gui_UnnioTempFeatures.InvalidFieldAlert(m_displayAlert, (int)position.width, 25, m_displayAlertText); //used wong file type

            GUILayout.Label(m_title, EditorStyles.boldLabel);

            GUILayout.BeginHorizontal(); //select what we are testing for
            GUILayout.Label("save to:", GUILayout.Width((position.width / 3) -3));
            m_currentSaveLocation = EditorGUILayout.TextField(m_currentSaveLocation, GUILayout.Width(((position.width / 3) * 2) -9));
            GUILayout.EndHorizontal();

            GUILayout.Space(20);


            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(m_confirmText) == true) //clicked yes
            {
                bool completeFinish = true;
                bool invalidCharDetected = false;
                string parsedLocation = LocalParse(m_currentSaveLocation);

                if (m_currentSaveLocation.Substring(m_currentSaveLocation.LastIndexOf(".") + 1) != m_fileType) //wrong file types
                {
                    completeFinish = false;
                    m_displayAlertText = $"invalid save location (save needs to be a {m_fileType} file)";
                    m_displayAlert = true;
                }


                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) //windows checks
                {
                    int index = parsedLocation.LastIndexOf(":/");
                    if (parsedLocation.LastIndexOf(":\\") != 1 && parsedLocation.LastIndexOf(":/") != 1)
                    {
                        completeFinish = false;
                        m_displayAlertText = "invalid save location (invalid drive name or no drive name detected)";
                        m_displayAlert = true;
                    }
                }

                if (parsedLocation.Contains("*") == true) { invalidCharDetected = true; } //valid files and folders dont have these charters
                if (parsedLocation.Contains('"') == true) { invalidCharDetected = true; }
                if (parsedLocation.Contains("<") == true) { invalidCharDetected = true; }
                if (parsedLocation.Contains(">") == true) { invalidCharDetected = true; }
                if (parsedLocation.Contains("|") == true) { invalidCharDetected = true; }
                if (parsedLocation.Contains("?") == true) { invalidCharDetected = true; }
                if (parsedLocation.Split(":").Length > 2) { invalidCharDetected = true; }

                if (invalidCharDetected == true) //file path not valid
                {
                    completeFinish = false;
                    m_displayAlertText = "invalid save location (invalid characters detected in path)";
                    m_displayAlert = true;
                }

                if (completeFinish == true) //done
                {
                    if (e_confirm != null)
                    {
                        e_confirm();
                    }

                    Close();
                }
            }

            if (GUILayout.Button(m_cancelText) == true) //clicked no
            {
                if (e_cancel != null)
                {
                    e_cancel();
                }

                Close();
            }
            GUILayout.EndHorizontal();

        }

        #region parse & save
        public static string Parse(string path) //replaces the envioment path wit the real path
        {
            path = path.Replace("<ProjectAssetsFolder>", Application.dataPath);
            path = path.Replace("<ProjectName>", System.IO.Directory.GetParent(Application.dataPath).Name);
            path = path.Replace("<AppdataFolder>", $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/Unity.Unnio");
            path = path.Replace("<UserFolder>", System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)));
            path = path.Replace("\\", "/"); //so its vialid for unity stuff

            return path;
        }

        public string LocalParse(string path) //replaces the envioment path wit the real path
        {
            path = Parse(path);

            foreach (var item in m_addToParse) //so custom stuff can be parsed
            {
                path = path.Replace($"<{item.Key}>", item.Value);
            }

            return path;
        }

        public void AddToLocalParse(string key, string value) //adds custome text to the parser than can then be replace by the value text
        {
            if (m_addToParse.ContainsKey(key) == true)
            {
                m_addToParse[key] = value; 
            }

            m_addToParse.Add(key, value);
        }

        public void RemoveFromLocalParse(string key) //removed text from local parser
        {
            m_addToParse.Remove(key);
        }

        public void SetSaveLocation(string loc)
        {
            m_currentSaveLocation = loc;
        }

        public string GetSaveLocation()
        {
            return m_currentSaveLocation;
        }
        #endregion
    }
}

#endif