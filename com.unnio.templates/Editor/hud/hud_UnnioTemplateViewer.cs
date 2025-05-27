#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Threading;
using System;
using System.IO;
using Unity.Plastic.Newtonsoft.Json.Linq;
using EditorPackages.Unnio;


namespace EditorPackages.UnnioTemp
{
    public class hud_UnnioTemplateViewer : EditorWindow
    {
        #region private vars: storage locations
        private static string m_downloadLocation = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/Unity.Unnio/templateViewer/"; //where to dowload the viewer data //static because it is effient
        private static string m_LastSaveLocation = "<ProjectAssetsFolder>/Editor/AddedBoardTemplates/<TemplateTitle>.unnboa"; //where to add templates to
        #endregion

        #region private vars: search data
        private string m_displaySearchString = "";
        private string m_searchString = "";
        private List<int> m_objectSearchResults = new List<int>();
        #endregion

        #region private vars: download status & shold download happen
        private static bool m_isMountingData = false; //show installing screen
        private static bool m_didUpdateFail = false; //show you lost connection screen
        private static bool m_shouldRepaint = false; //something has updated and rendering also need to update
        private static int m_ViewerCount = 0; //how many viewer are open //important for understanding if we should attempt dowload or event removal
        #endregion

        #region private vars: is loading templates
        private static bool m_isTemplatesUpToDate = false; //if true show done screen and update mounted templates if bellow = false
        private static bool m_hasUpdateBeenLoadedIntoEditor = false;
        #endregion

        #region private vars: template data
        private static Dictionary<string, gui_TemplateLayoutData> m_templates = new Dictionary<string, gui_TemplateLayoutData>();
        private gui_TemplateLayoutData m_currentOpenTemplate = null;
        #endregion

        #region private vars: UI data
        private Vector2 m_templateScrollOffset = Vector2.zero;

        private bool m_usingOverlay = true;
        private bool m_wasCreatedCorrectly = false; //did the window start up correctly

        private float m_elapsedTime = 0;
        #endregion


        #region public vars: current version of unnio this can download templates for
        public static string m_unnioVersion { get; private set; } = "3.3.1";
        #endregion


        #region window startup
        [MenuItem("Window/Unnio/Open template viewer")]
        public static void OpenWindow()
        {
            int width = (int)((Screen.currentResolution.width - 10) / 1.25f);
            int height = (int)((Screen.currentResolution.height - 110) / 1.5f);

            hud_UnnioTemplateViewer window = CreateWindow<hud_UnnioTemplateViewer>("Unnio: template viewer"); //can have more than one
            window.maximized = true;
            window.position = new Rect((Screen.currentResolution.width -width) /2, (Screen.currentResolution.height -height) /2, width, height);
            window.m_wasCreatedCorrectly = true;
        }

        private void Awake()
        {
            if (m_wasCreatedCorrectly == false) //so it dosent get auto started up by unity because it will frzze unity for a couple of seconds and people wont like that especilly if its unexpected 
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

            m_ViewerCount++; //just incase unity bugs out and creates two UIs insted of one

            if (m_isTemplatesUpToDate == false)
            {
                UpdateTemplates(); //update = download lastest version of the layout and template files
            }
        }
        #endregion

        private void Update()
        {
            m_elapsedTime += Time.deltaTime;

            if (m_shouldRepaint == true)
            {
                m_shouldRepaint = false;
                Repaint(); //if you have to call it it is best you call it in update if your doing muiltythreading or unity will not like you
            }

            if ((m_isTemplatesUpToDate == true || m_didUpdateFail == true) && m_hasUpdateBeenLoadedIntoEditor == false) //WHY CANT YOU MAKE TEXTURES OFF THE MAIN THREAD ... FOR THE LOVE OF [InsertDeityHere] UNITY
            {
                m_hasUpdateBeenLoadedIntoEditor = true;
                LoadUpdate();
            }
        }

        private void OnGUI()
        {
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            CreateUITemplateList();

            GUILayout.BeginVertical(); //for the main body area

            if ((m_isTemplatesUpToDate == false || m_hasUpdateBeenLoadedIntoEditor == false) && m_currentOpenTemplate == null) //update in progress
            {
                CreateUIUpdateScreen();
            }
            else if (m_currentOpenTemplate == null) //update complete
            {
                CreateUISelectScreen();
            }
            else //a template has now been selected
            {
                CreateUTemplateScreen();
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private void OnDestroy()
        {
            m_ViewerCount--;

            if (m_ViewerCount <= 0)
            {
                temp_TemplateDownloader.ClearEventsGlobally();
            }
        }

        #region UI
        private void CreateUITemplateList() //list all the currently avalible templates
        {
            Color winBarCol = new Color(0.10f, 0.10f, 0.10f, 1);
            int barWidth = 300;
            int rootTextSpacing = 10;
            int dividerSpacing = 20;

            if (EditorGUIUtility.isProSkin == false)
            {
                winBarCol = new Color(0.7f, 0.7f, 0.7f, 1);
            }

            GUILayout.BeginVertical(gui_UnnioTempFeatures.BoxWithColor(winBarCol), GUILayout.Width(barWidth), GUILayout.Height(position.height)); //for the side bar color
            
            GUILayout.BeginHorizontal();
            GUILayout.Space(4);
            m_displaySearchString = EditorGUILayout.TextField(m_displaySearchString, GUI.skin.FindStyle("ToolbarSearchTextField"));
            GUILayout.EndHorizontal();

            if (m_displaySearchString != m_searchString)
            {
                m_searchString = m_displaySearchString;
                UpdateSearch();
            }

            m_templateScrollOffset = EditorGUILayout.BeginScrollView(m_templateScrollOffset, false, false); //where the list contents will go once thats be sused out

            GUILayout.Space(rootTextSpacing);

            int itemIndex = 0;
            bool doneFirstItem = false;
            foreach (var item in m_templates)
            {
                if (m_searchString.Length <= 0 || m_objectSearchResults.Contains(itemIndex) == true)
                {
                    if (item.Value != null) //if it has content it is a list entry if not it is a divider
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("   • ", GUILayout.ExpandWidth(false));

                        if (EditorGUILayout.LinkButton(item.Key) == true)
                        {
                            m_currentOpenTemplate = item.Value;
                        }
                        GUILayout.EndHorizontal();
                    }
                    else
                    {
                        if (doneFirstItem == true)
                        {
                            GUILayout.Space(dividerSpacing);
                        }
                        GUILayout.Label(item.Key, EditorStyles.boldLabel, GUILayout.ExpandWidth(false));
                    }

                    doneFirstItem = true;
                }

                itemIndex++;
            }            

            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void CreateUIUpdateScreen() //an update is in progress
        {
            int screenSizeX = (int)position.width - 300;
            float progressBarProgress = ((m_elapsedTime * 250) % 1000) / 1000;
            
            if (m_isMountingData == true) //files are being installed
            {
                gui_UnnioTempFeatures.IndeterminateProgressBar(new Color(0.7f, 0.2f, 0, 1), new Color(1, 0.4f, 0, 1), progressBarProgress, screenSizeX, 4);
                GUILayout.Label("almost there: files are now being installed", gui_UnnioTempFeatures.TitleTextStyle());

                m_shouldRepaint = true; //so the progress bar keeps updating
            }
            else if (m_didUpdateFail == true) //connection was lost
            {
                GUILayout.Label("CONNECTION LOST", gui_UnnioTempFeatures.TitleTextStyle());
                GUILayout.Label("restart viewer to retry conncetion. If an old version of the template data exsist it will be loaded insted.");
            }
            else //updates are being dowloaded
            {
                gui_UnnioTempFeatures.IndeterminateProgressBar(new Color(0, 0.5f, 1, 1), new Color(0, 1, 1, 1), progressBarProgress, screenSizeX, 4);
                GUILayout.Label("please wait while updates are downloaded", gui_UnnioTempFeatures.TitleTextStyle());
                GUILayout.Label("I apologize for any inconvenience");

                m_shouldRepaint = true; //so the progress bar keeps updating
            }
        }

        private void CreateUISelectScreen() //select a template you want to use
        {
            GUILayout.Label("all updates are complete", gui_UnnioTempFeatures.TitleTextStyle());
            GUILayout.Label("you are now free to browse the templates");
        }

        private void CreateUTemplateScreen() //select a template you want to use
        {
            int titlePadding = 8;
            int screenSizeX = (int)position.width - 300;
            int screenSizeY = (int)position.height - 10;

            GUIStyle bg = new GUIStyle();
            bg.normal.background = m_currentOpenTemplate.m_background;

            if (m_usingOverlay == true)
            {
                bg.normal.background = m_currentOpenTemplate.m_backgroundTrasparent;
            }

            GUILayout.BeginVertical(bg, GUILayout.Width(screenSizeX), GUILayout.Height(screenSizeY));

            if (m_usingOverlay == true) //renders title and description
            {
                GUILayout.Label(m_currentOpenTemplate.m_title, gui_UnnioTempFeatures.TitleTextStyle());
                GUILayout.Space(titlePadding);
                GUILayout.Label(m_currentOpenTemplate.m_description, gui_UnnioTempFeatures.NormalTextStyle());
            }

            GUILayout.BeginHorizontal();

            if (m_usingOverlay == true) //renders details
            {
                GUILayout.BeginVertical();
                GUILayout.FlexibleSpace();
                GUILayout.Label("creator: " + m_currentOpenTemplate.m_creatorName, gui_UnnioTempFeatures.DetailTextStyle());
                GUILayout.Label("date of last upload: " + m_currentOpenTemplate.m_uploadDate, gui_UnnioTempFeatures.DetailTextStyle());
                GUILayout.Label("version: " + m_currentOpenTemplate.m_templateVersion, gui_UnnioTempFeatures.DetailTextStyle());
                GUILayout.Label("supported unnio versions: " + m_currentOpenTemplate.m_unnioVersions, gui_UnnioTempFeatures.DetailTextStyle());
                GUILayout.EndVertical();
            }

            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("toggle overlay", GUILayout.Width(200)) == true) //toggle button
            {
                m_usingOverlay = !m_usingOverlay;
            }

            if (GUILayout.Button("Add template to project", GUILayout.Width(200), GUILayout.Height(50)) == true)
            {
                if (m_currentOpenTemplate.m_unnioVersions.Contains(m_unnioVersion) == true || EditorUtility.DisplayDialog("unsupported version", "This template is NOT supported on your version of unnio. Do you still wish to add it?", "yes add it", "no") == true)
                {
                    hud_UnnioSaveLocator window = hud_UnnioSaveLocator.OpenWindow("export template to","export","cancel"); //the window for letting you export
                    window.AddToLocalParse("TemplateTitle", m_currentOpenTemplate.m_title);
                    window.AddToLocalParse("Version", m_currentOpenTemplate.m_templateVersion);
                    window.AddToLocalParse("CreatorName", m_currentOpenTemplate.m_creatorName);
                    window.SetSaveLocation(m_LastSaveLocation);

                    window.e_confirm += () => //we are good to export
                    {
                        m_LastSaveLocation = window.GetSaveLocation(); //store its results
                        string saveTo = window.LocalParse(window.GetSaveLocation()); //find export point

                        if (File.Exists(m_currentOpenTemplate.m_templateLocation) == true) //is the template we want to export to the project valid
                        {
                            if (File.Exists(saveTo) == true) //do we have to get rid of a file that already exsits
                            {
                                File.Delete(saveTo); //if so do so
                            }

                            new FileInfo(saveTo).Directory.Create(); //create space for file
                            File.Copy(m_currentOpenTemplate.m_templateLocation, saveTo); //export template to prject
                            AssetDatabase.Refresh(); //stop some possibleasset db issues
                        }
                    };
                }
            }
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
        #endregion

        #region download & update
        private static void UpdateTemplates()
        {
            int myHandle = 8888;
            temp_TemplateDownloader downloader = new temp_TemplateDownloader(myHandle, "https://github.com/bobbobson21/UnnioTemplates/archive/refs/heads/main.zip", m_downloadLocation);

            m_didUpdateFail = false;
            m_isMountingData = false;
            m_isTemplatesUpToDate = false;

            m_templates.Clear();

            temp_TemplateDownloader.e_Failed += (int handle, DateTime date) => //oh no
            {
                if (handle != myHandle) { return; }
                m_didUpdateFail = true;

                m_shouldRepaint = true; //tell it something went wrong
            };

            temp_TemplateDownloader.e_DataIsMounting += (int handle, DateTime date) => //almost there
            {
                if (handle != myHandle) { return; }
                m_isMountingData = true;

                m_shouldRepaint = true;
            };

            temp_TemplateDownloader.e_Completed += (int handle, DateTime date) => //yay
            {
                if (handle != myHandle) { return; }
                m_isTemplatesUpToDate = true; //after this we should then mount the updated template layout data
                

                m_shouldRepaint = true;
            };

            if (m_ViewerCount <= 1 && downloader.IsHandleValid() == true) //if more than 1 a template viewer is most likely already doing this part for us
            {
                Thread downloaderThread = new Thread(new ThreadStart(downloader.DownloadTemplates));
                downloaderThread.Start();
            }
        }

        private static void LoadUpdate()
        {
            string LoadLayoutFrom = m_downloadLocation + "UnnioTemplates-main/LayoutMain.json";
            string PullTemplatesFromRelativeToAssets = m_downloadLocation + "UnnioTemplates-main/";

            if (File.Exists(LoadLayoutFrom) == true) //just making sure its safe . best for offline mode
            {
                string fileDataRaw = File.ReadAllText(LoadLayoutFrom);
                JArray fileData = (JArray)JConstructor.Parse(fileDataRaw);

                for (int i = 0; i < fileData.Count; i++)
                {
                    if (((JObject)fileData[i]).ContainsKey("DIVIDER_TITLE") == true) //is it a divider
                    {
                        AddDivider((string)fileData[i]["DIVIDER_TITLE"]);
                    }
                    else
                    {
                        try
                        {
                            AddTemplate((string)fileData[i]["LINK_TITLE"], new gui_TemplateLayoutData().ConvertFromJson(((JObject)fileData[i]), PullTemplatesFromRelativeToAssets, PullTemplatesFromRelativeToAssets));
                        }
                        catch
                        {
                             Debug.LogError("Unnio Temp: somehow the creation of putting a template link in the side bar failed");
                        }
                    }
                }
            }
        }
        #endregion

        private void UpdateSearch()
        {
            int itemIndex = 0;
            int itemIndexOfLastHeadder = -1;
            bool addeditemIndexOfLastHeadder = false;

            m_objectSearchResults.Clear();

            if (m_searchString == "") //no point in doing search
            {
                return;
            }

            foreach (var item in m_templates)
            {
                if (item.Value == null) //is divider/catogory
                {
                    itemIndexOfLastHeadder = itemIndex; //if so rember it and set this addeditemIndexOfLastHeadder to false so it can be seached again
                    addeditemIndexOfLastHeadder = false;
                }

                if (item.Key.ToLower().Contains(m_searchString) == true || item.Key.ToUpper().Contains(m_searchString) == true)
                {
                    m_objectSearchResults.Add(itemIndex); //turned up in search

                    if (itemIndexOfLastHeadder == itemIndex) //to pervent repeat adds
                    {
                        addeditemIndexOfLastHeadder = true;
                    }

                    if (addeditemIndexOfLastHeadder == false) //if a result eas true for a catogory child and the catagory its in to the result if it isnt already added
                    {
                        m_objectSearchResults.Add(itemIndexOfLastHeadder);
                        addeditemIndexOfLastHeadder = true;
                    }
                }

                itemIndex++;
            }
        }

        #region side bar content formatting
        public static void AddDivider(string title) //do you rember the 21 night of setember ... or where this function may of came from (hint: tutoriallization)
        {
            m_templates.Add(title, null);
        }

        public static void AddTemplate(string title, gui_TemplateLayoutData data)
        {
            if (data == null) { return; }
            m_templates.Add(title, data);
        }

        public static void RemoveDivider(string title)
        {
            if (m_templates.ContainsKey(title) == false || m_templates[title] != null) { return; }
            m_templates.Remove(title);
        }

        public static void RemoveTemplate(string title)
        {
            if (m_templates.ContainsKey(title) == false || m_templates[title] == null) { return; }
            m_templates.Remove(title);
        }
        #endregion
    }
}

#endif