#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.UI;

using System.Collections.Generic;
using System;

namespace EditorPackages.Unnio
{
    public class hud_UnnioStackEditor : EditorWindow
    {
        #region private vars: editor data
        private static stack_StackBase m_stackInEditor = null;
        private static bool m_wasStackLoadedIntoEditor = false; //if true then we can add a delete node option
        private static bool m_editorOpen = false; //if true m_nodeInEditor can not be accesed outside the class
        #endregion

        #region private vars: drop down sections
        private bool m_displayNormalSetting = true;
        private bool m_displayBuiltInActions = true;
        private bool m_displayEntryActions = true;
        private bool m_displayExitActions = true;
        #endregion

        #region private vars: display alert
        private string m_displayAlertBadAction = "";
        private bool m_displayAlert = false; //on no u messed something up oh dear
        #endregion

        #region private var: was created correctly
        private bool m_wasCreatedCorrectly = false;
        #endregion

        #region private var: scroll offset
        private Vector2 m_StackMenuScrollPos;
        #endregion

        #region private vars: tutorial progress
        private int m_tutorialProgress = 0;
        private static bool m_tutorialCompleted = false;
        private static bool m_skipingPartOfTutorial = false;
        #endregion

        #region private vars: styles
        private GUIStyle m_styleMainBoxTutorial = null;
        private GUIStyle m_styleHalfBoxTutorial = null;
        private GUIStyle m_styleHalfHalfBoxTutorial = null;
        #endregion


        #region public vars: events
        public delegate void OnEditorExchange(stack_StackBase node, bool newStack);
        public static event OnEditorExchange e_onOpened;
        public static event OnEditorExchange e_onClosed;
        #endregion


        #region on window creation
        //[MenuItem("Window/Unnio/StackEditor")] //comment this line when complete
        public static bool OpenWindow()
        {
            if (m_editorOpen == true)
            {
                return false;
            }

            m_wasStackLoadedIntoEditor = false;
            m_editorOpen = true; //corrently sets up editing of a node

            hud_UnnioStackEditor window = GetWindow<hud_UnnioStackEditor>("Unnio: stack editor", true); //cant have more than one
            int newWindowWidth = 700;
            int newWindowHeight = 700;

            m_stackInEditor = new stack_StackBase();

            window.position = new Rect((Screen.currentResolution.width - newWindowWidth) / 2, (Screen.currentResolution.height - newWindowHeight) / 2, newWindowWidth, newWindowHeight);
            window.m_wasCreatedCorrectly = true;

            return true;
        }

        public static bool OpenWindowWithStack(stack_StackBase stack, bool loadedIn = true)
        {
            bool returnValue = OpenWindow();

            m_stackInEditor = stack; //loads the stack into us
            m_wasStackLoadedIntoEditor = loadedIn;

            return returnValue;
        }

        private void Awake()
        {
            if (m_wasCreatedCorrectly == false) //so unity dosent auto start it
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
            else
            {
                if (e_onOpened != null)
                {
                    e_onOpened(m_stackInEditor, m_wasStackLoadedIntoEditor);
                }
            }
        }
        #endregion

        private void OnGUI()
        {
            int crossSpacing = 3;
            int alertBoxHeight = 25;

            if (m_stackInEditor == null) //pervents errors
            {
                return;
            }

            m_displayAlert = gui_UnnioFeatures.InvalidFieldAlert(m_displayAlert, (int)position.width, alertBoxHeight, $"invalid action detected ({m_displayAlertBadAction})"); //you did something wrong
            
            m_displayNormalSetting = EditorGUILayout.Foldout(m_displayNormalSetting, "main setting"); 

            if (m_displayNormalSetting == true) //main settings
            {
                m_stackInEditor.m_title = EditorGUILayout.TextField(m_stackInEditor.m_title);

                GUILayout.BeginHorizontal(); //select type
                GUILayout.Space(crossSpacing);
                GUILayout.Label("stack type color: ", GUILayout.Width((position.width / 2) - crossSpacing));
                m_stackInEditor.m_color = EditorGUILayout.ColorField(m_stackInEditor.m_color, GUILayout.Width((position.width / 2) - (crossSpacing *2)));
                GUILayout.EndHorizontal();

                GUILayout.Space(20);
            }

            GUILayout.BeginVertical(); //action rendering is done bellow this point
            m_StackMenuScrollPos = EditorGUILayout.BeginScrollView(m_StackMenuScrollPos, GUILayout.Width(position.width));

            m_displayBuiltInActions = CreateUIActionList(m_displayBuiltInActions, "node created in actions", m_stackInEditor.m_builtInActions);
            m_displayEntryActions = CreateUIActionList(m_displayEntryActions, "node entry actions", m_stackInEditor.m_entryActions);
            m_displayExitActions = CreateUIActionList(m_displayExitActions, "node exit actions", m_stackInEditor.m_exitActions);

            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();


            GUILayout.FlexibleSpace();

            string DeleteExitButtonText = "delete stack";

            if (m_wasStackLoadedIntoEditor == false)
            {
                DeleteExitButtonText = "cancel";
            }

            if (GUILayout.Button(DeleteExitButtonText) == true)
            {
                if (m_wasStackLoadedIntoEditor == false || EditorUtility.DisplayDialog("delete stack", "click yes to DELETE the stack or pervent it from being created", "yes DELETE stack", "no") == true)
                {
                    m_stackInEditor = null; //delete it
                    Close();
                }

            }

            if (GUILayout.Button("finish") == true)
            {
                bool ProceedToExit = true;

                for (int i = 0; i < m_stackInEditor.m_builtInActions.Count; i++)
                {
                    if (stack_Oparator.IsValidAction(m_stackInEditor.m_builtInActions[i]) == false || m_stackInEditor.m_builtInActions[i].m_actionType == stack_StackActionTypes.None) //are all the buit in actions valid
                    {
                        ProceedToExit = false; //no
                        m_displayAlert = true;
                        m_displayAlertBadAction = $"built in action at: {i}";
                    }
                }

                for (int i = 0; i < m_stackInEditor.m_entryActions.Count; i++)
                {
                    if (stack_Oparator.IsValidAction(m_stackInEditor.m_entryActions[i]) == false || m_stackInEditor.m_entryActions[i].m_actionType == stack_StackActionTypes.None) //are all the entry actions valid
                    {
                        ProceedToExit = false; //no
                        m_displayAlert = true;
                        m_displayAlertBadAction = $"entry action at: {i}";
                    }
                }

                for (int i = 0; i < m_stackInEditor.m_exitActions.Count; i++)
                {
                    if (stack_Oparator.IsValidAction(m_stackInEditor.m_exitActions[i]) == false || m_stackInEditor.m_exitActions[i].m_actionType == stack_StackActionTypes.None) //are all the exit actions valid
                    {
                        ProceedToExit = false; //no
                        m_displayAlert = true;
                        m_displayAlertBadAction = $"exit action at: {i}";
                    }
                }

                if (ProceedToExit == true)
                {
                    Close();
                }
            }

            if (board_BoardBase.IsTutorialBoard() == true && m_tutorialCompleted == false)
            {
                CreateUITutorial();
            }
        }

        private void OnDestroy()
        {
            m_editorOpen = false;

            if (e_onClosed != null)
            {
                e_onClosed(m_stackInEditor, m_wasStackLoadedIntoEditor);
            }
        }

        #region data about editor & stack in editor
        public static bool IsThisStackInEditor(stack_StackBase stack)
        {
            return (m_stackInEditor == stack && m_editorOpen == true);
        }
       
        public static bool ShouldStackBeRemoved()
        {
            return (m_stackInEditor == null);
        }

        public static bool IsEditorOpen()
        {
            return m_editorOpen;
        }
        #endregion

        #region UI
        private bool CreateUIActionList(bool isOpen, string title, List<stack_StackAction> list)
        {
            int crossSpacing = 3;
            isOpen = EditorGUILayout.Foldout(isOpen, title); //shold render action

            if (isOpen == true)
            {
                if (list.Count >= 1) //renders the buttons at the top
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("add action", GUILayout.MaxWidth(((position.width / 3) * 2) - crossSpacing)) == true) { list.Add(new stack_StackAction()); }
                    if (GUILayout.Button("remove all actions", GUILayout.MaxWidth((position.width / 3) - crossSpacing)) == true) { list.Clear(); }
                    GUILayout.EndHorizontal();
                }
                else //only render as one button because there is nothing to remove
                {
                    if (GUILayout.Button("add action") == true) { list.Add(new stack_StackAction()); }
                }

                for (int i = 0; i < list.Count; i++)
                {
                    bool keepAction = CreateUIAction(list[i]);

                    if (keepAction == false)
                    {
                        list.RemoveAt(i);
                        i--;
                        if (i < 0) { i = 0; }
                        if (list.Count <= 0) { break; }
                    }
                }

                GUILayout.Space(10);
            }

            return isOpen;
        }

        private bool CreateUIAction(stack_StackAction action)
        {
            float xPadding = 6;
            float xConditionPadding = 6;
            float yConditionPadding = 3;
            float xConditionSize = position.width - (xPadding + (xConditionPadding * 2));

            string[] actionTypes = new string[] //all actions
            {         
                "None",

                "Set Node Color",
                "Set Node Color If Color",
                "Set Node Color If Color Not",
                "Set Node Color If Status Is",
                "Set Node Color If Status Is Not",
                "Set Node Color If Date More Then",
                "Set Node Color If Date Less Then",

                "Remove All Node Conditions",
                "Remove All Node Condition If Color",
                "Remove All Node Condition If Color Not",
                "Remove All Node Conditions If Status Is",
                "Remove All Node Conditions If Status Is Not",
                "Remove All Node Conditions If Date More Then",
                "Remove All Node Conditions If Date Less Then",

                "Remove Node",
                "Remove Node If Color",
                "Remove Node If Color Not",
                "Remove Node If Date More Then",
                "Remove Node If Date Less Then",
                "Remove Node If Status Is",
                "Remove Node If Status Is Not",

                "Set Status",
                "Set Status If Color",
                "Set Status If Color Not",
                "Set Status If Date More Then",
                "Set Status If Date Less Then",
                "Set Status If Status Is",
                "Set Status If Status Is Not",

                "Set Deadline",
                "Set Deadline If Color",
                "Set Deadline If Color Not",
                "Set Deadline If Date More Then",
                "Set Deadline If Date Less Then",
                "Set Deadline If Status Is",
                "Set Deadline If Status Is Not",
            }; //UPDATE THIS IF ADDING MORE ACTIONS

            GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(1), GUILayout.ExpandWidth(false));

            if (GUILayout.Button("remove action", GUILayout.Width(xConditionSize - (xConditionPadding * 2.25f))) == true)
            {
                GUILayout.EndVertical();
                return false;
            }

            GUILayout.BeginHorizontal(); //select type
            GUILayout.Label("action type:", GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
            action.m_actionType = (stack_StackActionTypes)EditorGUILayout.Popup((int)action.m_actionType, actionTypes, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
            GUILayout.EndHorizontal();


            switch (action.m_actionType)
            {
                case stack_StackActionTypes.None:
                    break;
                case stack_StackActionTypes.SetNodeColor:

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("set color to:",GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_thenColor = EditorGUILayout.ColorField(action.m_thenColor, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("apply color as a tint:", GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_thenColorIsTint = EditorGUILayout.Toggle(action.m_thenColorIsTint);
                    GUILayout.EndHorizontal();

                    break;
                case stack_StackActionTypes.SetNodeColorIfColor:

                    GUILayout.BeginHorizontal(); //if
                    GUILayout.Label("if color:",GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_ifColor = EditorGUILayout.ColorField(action.m_ifColor, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(); //then
                    GUILayout.Label("then set color to:",GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_thenColor = EditorGUILayout.ColorField(action.m_thenColor, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("apply color as a tint:", GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_thenColorIsTint = EditorGUILayout.Toggle(action.m_thenColorIsTint);
                    GUILayout.EndHorizontal();

                    break;
                case stack_StackActionTypes.SetNodeColorIfColorNot:

                    GUILayout.BeginHorizontal(); //if
                    GUILayout.Label("if not color:",GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_ifColor = EditorGUILayout.ColorField(action.m_ifColor, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(); //then
                    GUILayout.Label("then set color to:",GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_thenColor = EditorGUILayout.ColorField(action.m_thenColor, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("apply color as a tint:", GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_thenColorIsTint = EditorGUILayout.Toggle(action.m_thenColorIsTint);
                    GUILayout.EndHorizontal();

                    break;
                case stack_StackActionTypes.SetNodeColorIfStatusIs:

                    GUILayout.BeginHorizontal(); //if
                    GUILayout.Label("if status:",GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_ifStatus = EditorGUILayout.TextField(action.m_ifStatus, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(); //then
                    GUILayout.Label("then set color to:",GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_thenColor = EditorGUILayout.ColorField(action.m_thenColor, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("apply color as a tint:", GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_thenColorIsTint = EditorGUILayout.Toggle(action.m_thenColorIsTint);
                    GUILayout.EndHorizontal();

                    break;
                case stack_StackActionTypes.SetNodeColorIfStatusIsNot:

                    GUILayout.BeginHorizontal(); //if
                    GUILayout.Label("if not status:",GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_ifStatus = EditorGUILayout.TextField(action.m_ifStatus, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(); //then
                    GUILayout.Label("then set color to:",GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_thenColor = EditorGUILayout.ColorField(action.m_thenColor, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("apply color as a tint:", GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_thenColorIsTint = EditorGUILayout.Toggle(action.m_thenColorIsTint);
                    GUILayout.EndHorizontal();

                    break;
                case stack_StackActionTypes.SetNodeColorIfDateMoreThen:

                    GUILayout.BeginHorizontal(); //if
                    GUILayout.Label("if date is more than:",GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_ifDate = EditorGUILayout.TextField(action.m_ifDate, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));  
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(); //then
                    GUILayout.Label("then set color to:",GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_thenColor = EditorGUILayout.ColorField(action.m_thenColor, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("apply color as a tint:", GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_thenColorIsTint = EditorGUILayout.Toggle(action.m_thenColorIsTint);
                    GUILayout.EndHorizontal();

                    break;
                case stack_StackActionTypes.SetNodeColorIfDateLessThen:

                    GUILayout.BeginHorizontal(); //if
                    GUILayout.Label("if date is less than:",GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_ifDate = EditorGUILayout.TextField(action.m_ifDate, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();
                    
                    GUILayout.BeginHorizontal(); //then
                    GUILayout.Label("then set color to:",GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_thenColor = EditorGUILayout.ColorField(action.m_thenColor, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("apply color as a tint:", GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_thenColorIsTint = EditorGUILayout.Toggle(action.m_thenColorIsTint);
                    GUILayout.EndHorizontal();

                    break;

                case stack_StackActionTypes.RemoveAllNodeConditions:
                    break;
                case stack_StackActionTypes.RemoveAllNodeConditionIfColor:

                    GUILayout.BeginHorizontal(); //if
                    GUILayout.Label("if color:",GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_ifColor = EditorGUILayout.ColorField(action.m_ifColor, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();
                    break;
               
                case stack_StackActionTypes.RemoveAllNodeConditionIfColorNot:

                    GUILayout.BeginHorizontal(); //if
                    GUILayout.Label("if color:", GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_ifColor = EditorGUILayout.ColorField(action.m_ifColor, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    break;

                case stack_StackActionTypes.RemoveAllNodeConditionsIfStatusIs:

                    GUILayout.BeginHorizontal(); //if
                    GUILayout.Label("if status:",GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_ifStatus = EditorGUILayout.TextField(action.m_ifStatus, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    break;
                case stack_StackActionTypes.RemoveAllNodeConditionsIfStatusIsNot:

                    GUILayout.BeginHorizontal(); //if
                    GUILayout.Label("if not status:",GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_ifStatus = EditorGUILayout.TextField(action.m_ifStatus, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    break;
                case stack_StackActionTypes.RemoveAllNodeConditionsIfDateMoreThen:

                    GUILayout.BeginHorizontal(); //if
                    GUILayout.Label("if date is more than:",GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_ifDate = EditorGUILayout.TextField(action.m_ifDate, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    break;
                case stack_StackActionTypes.RemoveAllNodeConditionsIfDateLessThen:

                    GUILayout.BeginHorizontal(); //if
                    GUILayout.Label("if date is less than:",GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_ifDate = EditorGUILayout.TextField(action.m_ifDate, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    break;
                case stack_StackActionTypes.RemoveNodeIfColor:

                    GUILayout.BeginHorizontal(); //if
                    GUILayout.Label("if color:",GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_ifColor = EditorGUILayout.ColorField(action.m_ifColor, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    break;
                case stack_StackActionTypes.RemoveNodeIfColorNot:

                    GUILayout.BeginHorizontal(); //if 
                    GUILayout.Label("if not color:",GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_ifColor = EditorGUILayout.ColorField(action.m_ifColor, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    break;
                case stack_StackActionTypes.RemoveNodeIfDateMoreThen:

                    GUILayout.BeginHorizontal(); //if 
                    GUILayout.Label("if date is more than:",GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_ifDate = EditorGUILayout.TextField(action.m_ifDate, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    break;
                case stack_StackActionTypes.RemoveNodeIfDateLessThen:

                    GUILayout.BeginHorizontal(); //if
                    GUILayout.Label("if date is less than:",GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_ifDate = EditorGUILayout.TextField(action.m_ifDate, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    break;
                case stack_StackActionTypes.RemoveNodeIfStatusIs:

                    GUILayout.BeginHorizontal(); //if
                    GUILayout.Label("if status:",GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_ifStatus = EditorGUILayout.TextField(action.m_ifStatus, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    break;
                case stack_StackActionTypes.RemoveNodeIfStatusIsNot:

                    GUILayout.BeginHorizontal(); //if               
                    GUILayout.Label("if not user:",GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_ifStatus = EditorGUILayout.TextField(action.m_ifStatus, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();
                    break;

                case stack_StackActionTypes.SetStatus:

                    GUILayout.BeginHorizontal(); //then
                    GUILayout.Label("then status:", GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_thenStatus = EditorGUILayout.TextField(action.m_thenStatus, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    break;
                case stack_StackActionTypes.SetStatusIfColor:

                    GUILayout.BeginHorizontal(); //if
                    GUILayout.Label("if color:", GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_ifColor = EditorGUILayout.ColorField(action.m_ifColor, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(); //then
                    GUILayout.Label("then status:", GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_thenStatus = EditorGUILayout.TextField(action.m_thenStatus, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    break;
                case stack_StackActionTypes.SetStatusIfColorNot:

                    GUILayout.BeginHorizontal(); //if
                    GUILayout.Label("if not color:", GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_ifColor = EditorGUILayout.ColorField(action.m_ifColor, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(); //then
                    GUILayout.Label("then status:", GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_thenStatus = EditorGUILayout.TextField(action.m_thenStatus, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    break;
                case stack_StackActionTypes.SetStatusIfDateMoreThen:

                    GUILayout.BeginHorizontal(); //if 
                    GUILayout.Label("if date is more than:", GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_ifDate = EditorGUILayout.TextField(action.m_ifDate, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(); //then
                    GUILayout.Label("then status:", GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_thenStatus = EditorGUILayout.TextField(action.m_thenStatus, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    break;
                case stack_StackActionTypes.SetStatusIfDateLessThen:

                    GUILayout.BeginHorizontal(); //if 
                    GUILayout.Label("if date is less than:", GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_ifDate = EditorGUILayout.TextField(action.m_ifDate, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(); //then
                    GUILayout.Label("then status:", GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_thenStatus = EditorGUILayout.TextField(action.m_thenStatus, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    break;
                case stack_StackActionTypes.SetStatusIfStatusIs:

                    GUILayout.BeginHorizontal(); //if
                    GUILayout.Label("if status:", GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_ifStatus = EditorGUILayout.TextField(action.m_ifStatus, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(); //then
                    GUILayout.Label("then status:", GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_thenStatus = EditorGUILayout.TextField(action.m_thenStatus, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    break;
                case stack_StackActionTypes.SetStatusIfStatusIsNot:

                    GUILayout.BeginHorizontal(); //if
                    GUILayout.Label("if not status:", GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_ifStatus = EditorGUILayout.TextField(action.m_ifStatus, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(); //then
                    GUILayout.Label("then status:", GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_thenStatus = EditorGUILayout.TextField(action.m_thenStatus, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    break;
                case stack_StackActionTypes.SetDeadline:

                    GUILayout.BeginHorizontal(); //then
                    GUILayout.Label("then deadline:", GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_thenDeadline = EditorGUILayout.TextField(action.m_thenDeadline, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    break;
                case stack_StackActionTypes.SetDeadlineIfColor:

                    GUILayout.BeginHorizontal(); //if
                    GUILayout.Label("if color:", GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_ifColor = EditorGUILayout.ColorField(action.m_ifColor, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(); //then
                    GUILayout.Label("then deadline:", GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_thenDeadline = EditorGUILayout.TextField(action.m_thenDeadline, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    break;
                case stack_StackActionTypes.SetDeadlineIfColorNot:

                    GUILayout.BeginHorizontal(); //if
                    GUILayout.Label("if not color:", GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_ifColor = EditorGUILayout.ColorField(action.m_ifColor, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(); //then
                    GUILayout.Label("then deadline:", GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_thenDeadline = EditorGUILayout.TextField(action.m_thenDeadline, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    break;
                case stack_StackActionTypes.SetDeadlineIfDateMoreThen:

                    GUILayout.BeginHorizontal(); //if 
                    GUILayout.Label("if date is more than:", GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_ifDate = EditorGUILayout.TextField(action.m_ifDate, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(); //then
                    GUILayout.Label("then deadline:", GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_thenDeadline = EditorGUILayout.TextField(action.m_thenDeadline, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    break;
                case stack_StackActionTypes.SetDeadlineIfDateLessThen:

                    GUILayout.BeginHorizontal(); //if 
                    GUILayout.Label("if date is less than:", GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_ifDate = EditorGUILayout.TextField(action.m_ifDate, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(); //then
                    GUILayout.Label("then deadline:", GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_thenDeadline = EditorGUILayout.TextField(action.m_thenDeadline, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    break;
                case stack_StackActionTypes.SetDeadlineIfStatusIs:

                    GUILayout.BeginHorizontal(); //if
                    GUILayout.Label("if status:", GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_ifStatus = EditorGUILayout.TextField(action.m_ifStatus, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(); //then
                    GUILayout.Label("then deadline:", GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_thenDeadline = EditorGUILayout.TextField(action.m_thenDeadline, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    break;
                case stack_StackActionTypes.SetDeadlineIfStatusIsNot:

                    GUILayout.BeginHorizontal(); //if
                    GUILayout.Label("if status:", GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_ifStatus = EditorGUILayout.TextField(action.m_ifStatus, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(); //then
                    GUILayout.Label("then deadline:", GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    action.m_thenDeadline = EditorGUILayout.TextField(action.m_thenDeadline, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                    GUILayout.EndHorizontal();

                    break;

                default:
                    break;
            }

            GUILayout.EndVertical();
            GUILayout.Space(yConditionPadding*2);
            return true;
        }

        private void CreateUITutorial()
        {
            Rect mainRect = new Rect(position.width - 645.0f, position.height - 345.0f, 600, 300); //the diffrent size boxes that can be used
            Rect halfRect = new Rect(position.width - 645.0f, position.height - 195.0f, 600, 150);
            Rect halfhalfRect = new Rect(position.width - 645.0f, position.height - 120.0f, 600, 75);

            float xSpacing = 20; //spacing
            float alphaMuiltyply = 0.45f;
            Color boxColFg = new Color(0.60f, 0.05f, 0.00f, alphaMuiltyply); //colours
            Color boxColBg = new Color(1.00f, 0.40f, 0.00f, alphaMuiltyply);

            if (m_styleMainBoxTutorial == null || m_styleMainBoxTutorial.normal.background == null) //help make the tutorial more effecient to render since os of these boxes have a lot of area
            {
                m_styleMainBoxTutorial = gui_UnnioFeatures.BoxWithColor(boxColFg, boxColBg, (int)mainRect.width, (int)mainRect.height);
                m_styleHalfBoxTutorial = gui_UnnioFeatures.BoxWithColor(boxColFg, boxColBg, (int)halfRect.width, (int)halfRect.height);
                m_styleHalfHalfBoxTutorial = gui_UnnioFeatures.BoxWithColor(boxColFg, boxColBg, (int)halfhalfRect.width, (int)halfhalfRect.height);
            }

            try
            {
                switch (m_tutorialProgress)
                {
                    case 0: //opening
                        GUILayout.BeginArea(mainRect, m_styleMainBoxTutorial);
                        GUILayout.Label("This is the stack editor", gui_UnnioFeatures.StackTitleTextStyle());
                        GUILayout.Label("It is used to make stacks and also edit them when you click the button in the top right of the stacks that has three dots.", gui_UnnioFeatures.NodeTitleTextStyle());
                        GUILayout.Space(xSpacing);
                        GUILayout.Label("To get started change the title of your stack.", gui_UnnioFeatures.NodeTitleTextStyle());
                        GUILayout.EndArea();

                        if (m_stackInEditor.m_title != "... title here") //condition to progress tutorial
                        {
                            if (Event.current.type == EventType.Repaint)
                            {
                                m_tutorialProgress++;
                            }
                        }

                        break;

                    case 1: //main settings
                        GUILayout.BeginArea(halfhalfRect, m_styleHalfHalfBoxTutorial);
                        GUILayout.Label("Next you want to change the colour", gui_UnnioFeatures.StackTitleTextStyle());
                        
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("or click this to skip") == true)
                        {
                            m_skipingPartOfTutorial = true; //we use this to skip
                        }

                        GUILayout.EndArea();

                        if (m_stackInEditor.m_color != new Color(0,0,0,0)) //condition to progress tutorial
                        {
                            if (Event.current.type == EventType.Repaint)
                            {
                                Repaint(); //we repaint after so it updates the style
                                m_tutorialProgress++;
                            }
                        }

                        if (m_skipingPartOfTutorial == true) //section skip
                        {
                            if (Event.current.type == EventType.Repaint)
                            {
                                Repaint();
                                m_skipingPartOfTutorial = false; //needs to be set back to false after use
                                m_tutorialProgress += 2;
                            }
                        }

                        break;

                    case 2:
                        GUILayout.BeginArea(halfhalfRect, m_styleHalfHalfBoxTutorial);
                        GUILayout.Label("Also dont forget to change the alpha", gui_UnnioFeatures.StackTitleTextStyle());
                        GUILayout.Label("So you can see it.", gui_UnnioFeatures.NodeTitleTextStyle());

                        GUILayout.EndArea();

                        if (m_stackInEditor.m_color.a > 0)
                        {
                            if (Event.current.type == EventType.Repaint)
                            {
                                Repaint();
                                m_tutorialProgress++;
                            }
                        }

                        break;

                    case 3: //covering actions
                        GUILayout.BeginArea(halfRect,m_styleHalfBoxTutorial);
                        GUILayout.Label("Now we want to add an automated action to the board", gui_UnnioFeatures.StackTitleTextStyle());
                        GUILayout.Label("Click add action how ever many times you want under one of the three action type.", gui_UnnioFeatures.NodeTitleTextStyle());

                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("or click this to skip") == true)
                        {
                            m_skipingPartOfTutorial = true;
                        }

                        GUILayout.EndArea();

                        if (m_stackInEditor.m_builtInActions.Count + m_stackInEditor.m_entryActions.Count + m_stackInEditor.m_exitActions.Count > 0) //condition to progress tutorial
                        {
                            if (Event.current.type == EventType.Repaint)
                            {
                                Repaint();
                                m_skipingPartOfTutorial = false;
                                m_tutorialProgress++;
                            }
                        }

                        if (m_skipingPartOfTutorial == true) //section skip
                        {
                            if (Event.current.type == EventType.Repaint)
                            {
                                Repaint();
                                m_skipingPartOfTutorial = false;
                                m_tutorialProgress = 8;
                            }
                        }

                        break;

                    case 4:
                        GUILayout.BeginArea(halfRect, m_styleHalfBoxTutorial);
                        GUILayout.Label("Actions need a type", gui_UnnioFeatures.StackTitleTextStyle());
                        GUILayout.Label("Actions with no type are not valid. You can test this by clicking the finish button.", gui_UnnioFeatures.NodeTitleTextStyle());

                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("or click this to skip") == true)
                        {
                            m_skipingPartOfTutorial = true;
                        }

                        GUILayout.EndArea();

                        if (Event.current.type == EventType.Repaint) //condition to progress tutorial
                        {
                            for (int i = 0; i < m_stackInEditor.m_builtInActions.Count; i++)
                            {
                                if (m_stackInEditor.m_builtInActions[i].m_actionType != stack_StackActionTypes.None)
                                {
                                    Repaint();
                                    m_tutorialProgress++;
                                    return;
                                }
                            }


                            for (int i = 0; i < m_stackInEditor.m_entryActions.Count; i++)
                            {
                                if (m_stackInEditor.m_entryActions[i].m_actionType != stack_StackActionTypes.None)
                                {
                                    Repaint();
                                    m_tutorialProgress++;
                                    return;
                                }
                            }


                            for (int i = 0; i < m_stackInEditor.m_exitActions.Count; i++)
                            {
                                if (m_stackInEditor.m_exitActions[i].m_actionType != stack_StackActionTypes.None)
                                {
                                    Repaint();
                                    m_tutorialProgress++;
                                    return;
                                }
                            }

                            if (m_displayAlert == true || m_skipingPartOfTutorial == true)
                            {
                                Repaint();
                                m_skipingPartOfTutorial = false;
                                m_tutorialProgress++;
                            }
                        }

                        break;

                    case 5:
                        GUILayout.BeginArea(halfRect, m_styleHalfBoxTutorial);
                        GUILayout.Label("Click on the drop down box to change the action type", gui_UnnioFeatures.StackTitleTextStyle());
                        GUILayout.Label("Select a type you want from the box with none on it.", gui_UnnioFeatures.NodeTitleTextStyle());
                        GUILayout.EndArea();

                        if (Event.current.type == EventType.Repaint) //condition to progress tutorial
                        {
                            for (int i = 0; i < m_stackInEditor.m_builtInActions.Count; i++)
                            {
                                if (m_stackInEditor.m_builtInActions[i].m_actionType != stack_StackActionTypes.None)
                                {
                                    Repaint();
                                    m_tutorialProgress++;
                                    return;
                                }
                            }


                            for (int i = 0; i < m_stackInEditor.m_entryActions.Count; i++)
                            {
                                if (m_stackInEditor.m_entryActions[i].m_actionType != stack_StackActionTypes.None)
                                {
                                    Repaint();
                                    m_tutorialProgress++;
                                    return;
                                }
                            }


                            for (int i = 0; i < m_stackInEditor.m_exitActions.Count; i++)
                            {
                                if (m_stackInEditor.m_exitActions[i].m_actionType != stack_StackActionTypes.None)
                                {
                                    Repaint();
                                    m_tutorialProgress++;
                                    return;
                                }
                            }
                        }

                        break;

                    case 6:
                        GUILayout.BeginArea(halfRect, m_styleHalfBoxTutorial);
                        GUILayout.Label("Actions details", gui_UnnioFeatures.StackTitleTextStyle());
                        GUILayout.Label("After doing this options will appear related to the action feel free to fill them out then click the button bellow.", gui_UnnioFeatures.NodeTitleTextStyle());

                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("click to move on") == true)
                        {
                            m_skipingPartOfTutorial = true;
                        }

                        GUILayout.EndArea();

                        if (m_skipingPartOfTutorial == true) //slide skip
                        {
                            if (Event.current.type == EventType.Repaint)
                            {
                                Repaint();
                                m_skipingPartOfTutorial = false;
                                m_tutorialProgress++;
                            }
                        }

                        break;

                    case 7: //action removal
                        GUILayout.BeginArea(mainRect, m_styleMainBoxTutorial); //action removal
                        GUILayout.Label("Action removal", gui_UnnioFeatures.StackTitleTextStyle());
                        GUILayout.Space(xSpacing);
                        GUILayout.Label("You may also notice there are now two remove action buttons. The one inside the action will remove only that action and the other one next to add action will remove all actions of the type.", gui_UnnioFeatures.NodeTitleTextStyle());

                        GUILayout.Space(xSpacing);
                        GUILayout.Label("Remove all actions to see how it works.", gui_UnnioFeatures.NodeTitleTextStyle());

                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("or click this to skip") == true)
                        {
                            m_skipingPartOfTutorial = true;
                        }

                        GUILayout.EndArea();

                        if (m_stackInEditor.m_builtInActions.Count + m_stackInEditor.m_entryActions.Count + m_stackInEditor.m_exitActions.Count == 0 || m_skipingPartOfTutorial == true)
                        {
                            if (Event.current.type == EventType.Repaint)
                            {
                                Repaint();
                                m_skipingPartOfTutorial = false;
                                m_tutorialProgress++;
                            }
                        }

                        break;

                    case 8: //ending
                        GUILayout.BeginArea(mainRect, m_styleMainBoxTutorial);
                        GUILayout.Label("closing remarks", gui_UnnioFeatures.StackTitleTextStyle());
                        GUILayout.Space(xSpacing);
                        GUILayout.Label("The rest of the actions are not difficult to figure out but a lot of them work based on details in the nodes/the little block so I sugest you have a look at that next.", gui_UnnioFeatures.NodeTitleTextStyle());

                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("close stack tutorial") == true)
                        {
                            m_skipingPartOfTutorial = true;
                        }

                        GUILayout.EndArea();

                        if (m_skipingPartOfTutorial == true) //condition to progress tutorial
                        {
                            if (Event.current.type == EventType.Repaint)
                            {
                                Repaint();
                                m_skipingPartOfTutorial = false;
                                m_tutorialCompleted = true;
                                m_tutorialProgress++;
                            }
                        }

                        break;

                        default:
                            break;
                }
            }
            catch (ArgumentException) //deals with hot control crash
            {
                GUILayout.EndArea();
            }
        }
        #endregion
    }
}

#endif