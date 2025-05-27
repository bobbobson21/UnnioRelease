#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace EditorPackages.UnnioIssues
{
    public class hud_issuesUI : EditorWindow
    {
        #region private vars: scroll point
        private Vector2 m_scrollPos = Vector2.zero;
        #endregion

        #region private vars: search results
        private string m_displaySearchString = "";
        private string m_searchString = "";
        private List<issue_IssueData> m_searchResults = new List<issue_IssueData>();
        #endregion


        #region Window creation
        [MenuItem("Window/Unnio/Open issues display")]
        public static void OpenWindow()
        {
            hud_issuesUI window = CreateWindow<hud_issuesUI>("Unnio: issues display");

            int width = (int)((Screen.currentResolution.width - 10) / 1.25f);
            int height = (int)((Screen.currentResolution.height - 110) / 1.5f);

            window.maximized = true;
            window.position = new Rect((Screen.currentResolution.width - width) / 2, (Screen.currentResolution.height - height) / 2, width, height);
        }

        private void Awake()
        {
            register_IssueRegister.Refresh();
        }
        #endregion

        void OnGUI()
        {
            int issueDomainSpacing = 6;

            if (GUILayout.Button("refresh issues list") == true) //should refresh this issue list to see all new issues
            {
                RefreshAll();
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(4);
            m_displaySearchString = GUILayout.TextField(m_displaySearchString, GUI.skin.FindStyle("ToolbarSearchTextField"));
            GUILayout.EndHorizontal();

            if (m_displaySearchString != m_searchString) //should search be updated
            {
                m_searchString = m_displaySearchString;
                UpdateSearch();
            }

            m_scrollPos = GUILayout.BeginScrollView(m_scrollPos);
            GUILayout.BeginVertical();

            for (int i = 0; i < register_IssueRegister.m_childIssues.Count; i++)
            {
                if (register_IssueRegister.m_childIssues[i].GetStatus() == issue_IssueData_IssueStatus.container) //just incase the first thing is an issue and not a container
                {
                    CreateUICodeSpace(register_IssueRegister.m_childIssues[i]); //creates a top level container
                }
                else
                {
                    CreateUIIssue(register_IssueRegister.m_childIssues[i]); //makes top level container issue
                }

                if (i != register_IssueRegister.m_childIssues.Count - 1) //spacing of issue spaces
                {
                    GUILayout.Space(issueDomainSpacing); //spaces out all the issue domain areas
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        private void RefreshAll()
        {
            register_IssueRegister.Refresh();

            m_searchString = ""; //because its now rebuilding everything the seach results are invalid so get rid of them
            m_displaySearchString = "";
            m_searchResults.Clear();

            Repaint(); //renders out the new results
        }

        #region search
        private void UpdateSearch()
        {
            m_searchResults.Clear();

            for (int i = 0; i < register_IssueRegister.m_childIssues.Count; i++)
            {
                UpdateSearchRecursive(register_IssueRegister.m_childIssues[i]);
            }
        }

        private void UpdateSearchRecursive(issue_IssueData issue)
        {
            if (issue.m_title.ToLower().Contains(m_searchString.ToLower()) == true) //did it pass the search
            {
                issue_IssueData parent = issue.GetParent();
                while (parent != null) //if they did add all parents up to the root to the search results
                {
                    if (m_searchResults.Contains(parent) == false) //so we dont repeat add
                    {
                        m_searchResults.Add(parent);
                    }

                    parent = parent.GetParent(); //make us go up till we hit root
                }

                m_searchResults.Add(issue); //then add this
            }

            if (issue.GetStatus() == issue_IssueData_IssueStatus.container)
            {
                List<issue_IssueData> children = issue.GetChildren();
                for (int i = 0; i < children.Count; i++)
                {
                    UpdateSearchRecursive(children[i]);
                }
            }
        }
        #endregion

        #region UI
        private void CreateUICodeSpace(issue_IssueData data)
        {
            int topSpacing = 3;
            int bottomSpacing = 4;

            if (m_searchResults.Contains(data) == false && m_searchString.Length > 0) //should it be rendered
            {
                return;
            }

            GUILayout.BeginVertical(EditorStyles.textArea); //style

            GUILayout.BeginHorizontal();
            
            GUILayout.BeginVertical(GUILayout.Width(17));
            GUILayout.Space(topSpacing);
            GUILayout.Box("", gui_UnnioIssueFeatures.ColorIndicatorStyleTop(17, 17, data.m_color), GUILayout.Width(17), GUILayout.Height(17)); //this should only have one colour
            GUILayout.Box("", gui_UnnioIssueFeatures.ColorIndicatorStyleMiddle(data.m_color), GUILayout.Width(17), GUILayout.Height(18));
            GUILayout.Box("", gui_UnnioIssueFeatures.ColorIndicatorStyleBottom(17, 17, data.m_color), GUILayout.Width(17), GUILayout.Height(17));
            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            GUILayout.Label(data.m_title, gui_UnnioIssueFeatures.TitleTextStyleBig()); //title extra big

            if (data.GetStatus() == issue_IssueData_IssueStatus.container) //render children
            {
                List<issue_IssueData> childData = data.GetChildren();
                for (int i = 0; i < childData.Count; i++)
                {
                    CreateUIIssue(childData[i]);
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Space(bottomSpacing);

            GUILayout.EndVertical();
        }

        private void CreateUIIssue(issue_IssueData data)
        {
            int topSpacing = 3;
            int crossSpacing = 3;

            if (m_searchResults.Contains(data) == false && m_searchString.Length > 0)
            {
                return;
            }

            if (data.GetStatus() == issue_IssueData_IssueStatus.container) //help make the distinction between container and issue more apperent
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
            }
            else
            {
                GUILayout.BeginVertical(EditorStyles.textArea);
            }

             GUILayout.BeginHorizontal(); //first color/propagational color
            GUILayout.BeginVertical(GUILayout.Width(17));
            GUILayout.Space(topSpacing);
            GUILayout.Box("", gui_UnnioIssueFeatures.ColorIndicatorStyleTop(17, 17, data.m_color), GUILayout.Width(17), GUILayout.Height(17));
            GUILayout.Box("", gui_UnnioIssueFeatures.ColorIndicatorStyleMiddle(data.m_color), GUILayout.Width(17), GUILayout.Height(18));
            GUILayout.Box("", gui_UnnioIssueFeatures.ColorIndicatorStyleBottom(17, 17, data.m_color), GUILayout.Width(17), GUILayout.Height(17));
            GUILayout.EndVertical();


            if (data.m_secondaryColor.a > 0) //other colour
            {
                GUILayout.Space(crossSpacing);
                GUILayout.BeginVertical(GUILayout.Width(17));
                GUILayout.Space(topSpacing);
                GUILayout.Box("", gui_UnnioIssueFeatures.ColorIndicatorStyleTop(17, 17, data.m_secondaryColor), GUILayout.Width(17), GUILayout.Height(17));
                GUILayout.Box("", gui_UnnioIssueFeatures.ColorIndicatorStyleMiddle(data.m_secondaryColor), GUILayout.Width(17), GUILayout.Height(18));
                GUILayout.Box("", gui_UnnioIssueFeatures.ColorIndicatorStyleBottom(17, 17, data.m_secondaryColor), GUILayout.Width(17), GUILayout.Height(17));
                GUILayout.EndVertical();
            }

            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));


            GUILayout.Label(data.m_title, gui_UnnioIssueFeatures.TitleTextStyle()); //title

            if (data.GetStatus() == issue_IssueData_IssueStatus.problematic)
            {
                GUILayout.Label(data.m_details, gui_UnnioIssueFeatures.NormalTextStyle()); //details of issue
            }

            if (data.GetStatus() == issue_IssueData_IssueStatus.container) //render child containers and issues if container
            {
                List<issue_IssueData> childData = data.GetChildren();
                for (int i = 0; i < childData.Count; i++)
                {
                    CreateUIIssue(childData[i]);
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            if (data.GetStatus() == issue_IssueData_IssueStatus.problematic)
            {
                GUILayout.BeginHorizontal();

                if (data.f_customButtonFunctionForIssue != null) //dose it have a game object we can select
                {
                    if (GUILayout.Button(data.f_customButtonFunctionTitleForIssue) == true)
                    {
                        if (data.f_customButtonFunctionForIssue() == true)
                        {
                            RefreshAll();
                        }
                    }
                }

                if (data.m_gameObjectWithIssue != null) //dose it have a game object we can select
                {
                    if (GUILayout.Button("select game object") == true)
                    {
                        Selection.activeGameObject = data.m_gameObjectWithIssue; //this was supriseingly easy which is suprising
                    }
                }

                if (data.m_pathToFileWithIssue != "") //dose it have a file
                {
                    if (GUILayout.Button("open file causing issue in vs code") == true)
                    {
                        string path = Application.dataPath + "/" + data.m_pathToFileWithIssue;

                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) //make sure we use the right slash per os to open file
                        {
                            path = path.Replace("/", "\\");
                        }
                        else
                        {
                            path = path.Replace("\\", "/");
                        }

                        ProcessStartInfo startInfo = new ProcessStartInfo("code"); //if you dont have vs code this will do nothing fyi
                        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        startInfo.Arguments = path;

                        Process.Start(startInfo);
                    }
                }

                if (data.m_pathToFileWithIssueTest != "") //dose it have a file
                {
                    if (GUILayout.Button("open file with unit test vs code") == true)
                    {
                        string path = Application.dataPath + "/" + data.m_pathToFileWithIssueTest;

                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            path = path.Replace("/", "\\");
                        }
                        else
                        {
                            path = path.Replace("\\", "/");
                        }

                        ProcessStartInfo startInfo = new ProcessStartInfo("code");
                        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        startInfo.Arguments = path;

                        Process.Start(startInfo);
                    }
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }
        #endregion
    }
}

#endif
