#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;
using Unity.Plastic.Newtonsoft.Json.Linq;


namespace EditorPackages.UnnioTut
{
    public class hud_TutorialIndex : EditorWindow
    {
        #region private vars: current tutorial data
        private static Dictionary<string, List<Texture2D>> m_tutorials = new Dictionary<string, List<Texture2D>>();
        private string m_currentOpenTutorial = "";
        private int m_currentOpenSlide = 0;
        #endregion

        #region private vars: tutorial list data
        private Vector2 m_scrollOffset = Vector2.zero;
        #endregion


        #region public vars: defaults
        public static string m_defaultTitle { get; private set; } = "Unnio tut: tutorial index";
        public static string m_tutorialIndexFilePath { get; private set; } = "Packages/com.unnio.tutorials/Editor/tutorialLayout/TutorialLayout.json";
        #endregion

        [MenuItem("Window/Unnio/Open tutorial menu")]
        public static void OpenWindow()
        {
            hud_TutorialIndex window = GetWindow<hud_TutorialIndex>(m_defaultTitle);

            int newWindowWidth = 700;
            int newWindowHeight = 700;

            window.position = new Rect((Screen.currentResolution.width - newWindowWidth) / 2, (Screen.currentResolution.height - newWindowHeight) / 2, newWindowWidth, newWindowHeight);
        }

        private void Awake()
        {
            if (m_tutorials.Count <= 0) //so we only load it when we have to
            {
                LoadListLayoutFromFile(m_tutorialIndexFilePath); //load in all the tutorials
            }
        }

        private void OnGUI()
        {
            if (m_tutorials.ContainsKey(m_currentOpenTutorial) == false) //have we not got a tutorial to view
            {
                CreateUITutorialList(); //view the tutorial list then
            }
            else
            {
                CreateUITutorialSlideViewer(); //if we have got a tutorial to view view that
            }
        }

        #region UI
        private void CreateUITutorialList()
        {
            int rootTextSpacing = 30;
            int dividerSpacing = 20;
            int scrollBarPadding = 0;

            m_scrollOffset = EditorGUILayout.BeginScrollView(m_scrollOffset, false, false, GUILayout.Width(position.width - scrollBarPadding), GUILayout.Height(position.height));
            GUILayout.Label("Tutorial Index", gui_UnnioTutFeatures.TitleTextStyle());
            GUILayout.Label("A list of all the tutorials(built in) so far on using unnio:", EditorStyles.wordWrappedLabel);

            GUILayout.Space(rootTextSpacing); //the first spacing of the list

            bool doneFirstItem = false;
            foreach (var item in m_tutorials)
            {
                if (item.Value != null) //if it has content it is a tutorial if not it is a divider
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("   • ", GUILayout.ExpandWidth(false)); //the bullet point

                    if (EditorGUILayout.LinkButton(item.Key) == true) //the link
                    {
                        titleContent.text = "Unnio tut: " + item.Key; //update title
                        m_currentOpenTutorial = item.Key; //sets the loaded tutorial
                    }
                    GUILayout.EndHorizontal();
                }
                else //if not it is a divider
                {
                    if (doneFirstItem == true) //add spacing if spacing not already added
                    {
                        GUILayout.Space(dividerSpacing);
                    }
                    GUILayout.Label(item.Key, EditorStyles.boldLabel, GUILayout.ExpandWidth(false)); //divider
                }

                doneFirstItem = true;
            }

            EditorGUILayout.EndScrollView();
        }

        private void CreateUITutorialSlideViewer()
        {
            int topSpacing = 3;
            int slideCountSizeX = 60;
            int slideCountSizeY = 16;
            int buttonRowHightPadding = 10;
            int imageBoxHightPadding = 27;

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("<", GUILayout.Width(20)))
            {
                m_currentOpenSlide = Math.Max(m_currentOpenSlide -1, 0);
            }

            if (GUILayout.Button(">", GUILayout.Width(20)))
            {
                m_currentOpenSlide = Math.Min(m_currentOpenSlide + 1, m_tutorials[m_currentOpenTutorial].Count -1);
            }

            GUILayout.BeginVertical(GUILayout.Width(slideCountSizeX), GUILayout.Height(slideCountSizeY));
            GUILayout.Space(topSpacing - 1);
            GUILayout.BeginHorizontal(gui_UnnioTutFeatures.BoxWithColor(new Color(0, 0.20f, 0.60f), new Color(0, 0.40f, 1.0f), 60, 16), GUILayout.Width(slideCountSizeX), GUILayout.Height(slideCountSizeY));
            GUILayout.Label($"{m_currentOpenSlide +1} / {m_tutorials[m_currentOpenTutorial].Count}", GUILayout.Width(slideCountSizeX -1), GUILayout.Height(slideCountSizeY -1));
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            if (GUILayout.Button("go back to index"))
            {
                titleContent.text = m_defaultTitle;
                m_currentOpenTutorial = "";
                m_currentOpenSlide = 0;

                GUILayout.EndHorizontal();

                return;
            }

            GUILayout.EndHorizontal();

            int size = (int)Math.Min(position.width, position.height - imageBoxHightPadding);

            GUILayout.Space(buttonRowHightPadding);
            GUILayout.BeginHorizontal();
            GUILayout.Space((position.width /2) - (size/2));
            GUILayout.Box(m_tutorials[m_currentOpenTutorial][m_currentOpenSlide], new GUIStyle(), GUILayout.Width(size), GUILayout.Height(size));
            GUILayout.EndHorizontal();
        }

        #endregion

        #region tutorial adding, loading and removal
        public void AddTutorial(string titleText, List<Texture2D> slides)
        {
            if (slides == null) //tutorials need slides you fool
            {
                Debug.LogError("UnnioTut: tutorials need slides!!!");
                return;
            }

            m_tutorials[titleText] = slides;
        }

        public void AddDivider(string titleText)
        {
            m_tutorials[titleText] = null; //this is basically a tutorial without slides but thats why we force you to add them in AddTutorial
        }

        public void RemoveTutorial(string titleText)
        {
            if (m_tutorials.ContainsKey(titleText) == false || m_tutorials[titleText] == null) { return; } //restricts input to only tutorials
            m_tutorials.Remove(titleText);
        }

        public void RemoveDivider(string titleText)
        {
            if (m_tutorials.ContainsKey(titleText) == false || m_tutorials[titleText] != null) { return; } //restricts input to only dividers
            m_tutorials.Remove(titleText); //yes I
        }

        public void RemoveAll()
        {
            m_tutorials.Clear();
        }

        public void LoadListLayoutFromFile(string loadFrom)
        {
            try
            {
                if ((TextAsset)EditorGUIUtility.Load(loadFrom) != null) //is the tutorial file valid
                {
                    RemoveAll(); //out with the old version

                    string fileDataRaw = ((TextAsset)EditorGUIUtility.Load(loadFrom)).text; //get the text
                    JObject fileData = (JObject)JConstructor.Parse(fileDataRaw);

                    for (int i = 0; i < ((JArray)fileData["LAYOUT"]).Count; i++)
                    {
                        string divider = (string)((JArray)fileData["LAYOUT"])[i]["DIVIDER"]; //adds the dividers
                        if (divider == null) { divider = ""; }

                        AddDivider(divider); //adds a divider

                        for (int o = 0; o < ((JArray)fileData["LAYOUT"][i]["CONTENT"]).Count; o++) //adds the slide links
                        {
                            string title = (string)((JArray)fileData["LAYOUT"][i]["CONTENT"])[o]["TITLE"]; //slideshow title
                            if (title == null) { title = ""; }

                            JArray slides = (JArray)((JArray)fileData["LAYOUT"][i]["CONTENT"])[o]["SLIDES"]; //slide images
                            List<Texture2D> processedSlides = new List<Texture2D>();

                            for (int p = 0; p < slides.Count; p++) //add slides contents
                            {
                                try
                                {
                                    string slidePath = "Packages/com.unnio.tutorials/Editor/textures/" + (string)slides[p]; //if slides[p] is null we want an error
                                    slidePath = slidePath.Replace("\\", "/");

                                    Texture2D newSlide = (Texture2D)AssetDatabase.LoadAssetAtPath(slidePath, typeof(Texture2D));
                                    //newSlide.LoadImage(File.ReadAllBytes(slidePath));

                                    processedSlides.Add(newSlide); //converts image paths to real images
                                }
                                catch
                                {
                                    Debug.LogError($"Unnio Tut: failed to load tutorial slide '{slides[p]}'");
                                }
                            }

                            AddTutorial(title, processedSlides); //add the tutorial
                        }
                    }
                }
                else
                {
                    Debug.LogError($"Unnio Tut: failed to load tutorial because file '{loadFrom}' dose not exsit or the asset database is corupted");
                }
            }
            catch
            {
                Debug.LogError($"Unnio Tut: failed to load tutorial layout correctly corruption has occoured in file or asset database or file '{loadFrom}' is out of date");
            }
        }
        #endregion
    }
}

#endif