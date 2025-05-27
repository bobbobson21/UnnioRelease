#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.UI;
using System;

namespace EditorPackages.Unnio
{
    public class hud_UnnioNodeEditor : EditorWindow
    {
        #region private vars: editor data
        private static node_NodeBase m_nodeInEditor = null;
        private static bool m_wasNodeLoadedIntoEditor = false; //if true then we can add a delete node option
        private static bool m_editorOpen = false; //if true m_nodeInEditor can not be accesed outside the class
        #endregion

        #region private vars: drop down sections
        private bool m_displayNormalSetting = true;
        private bool m_displayConditions = true;
        #endregion

        #region private vars: display alert
        private int m_displayAlertBadConditionsCount = 0;
        private string m_displayAlertBadConditions = "";
        private bool m_displayAlert = false; //on no u messed something up oh dear
        #endregion

        #region private var: created correctly
        private bool m_wasCreatedCorrectly = false;
        #endregion

        #region private var: scroll pos
        private Vector2 m_conditionsScrollPos;
        #endregion

        #region private vars: tutorial data
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
        public delegate void OnEditorExchange(node_NodeBase node, bool newNode);
        public static event OnEditorExchange e_onOpened;
        public static event OnEditorExchange e_onClosed;
        #endregion


        #region on window creation
        //[MenuItem("Window/Unnio/NodeEditor")] //comment this line when complete
        public static bool OpenWindow()
        {
            if (m_editorOpen == true)
            {
                return false;
            }

            m_editorOpen = true; //corrently sets up editing of a node
            m_wasNodeLoadedIntoEditor = false;

            m_nodeInEditor = new node_NodeBase(); //this line is inportant because it stops us from accidently editing old nodes
            hud_UnnioNodeEditor window = GetWindow<hud_UnnioNodeEditor>("Unnio: node editor", true); //cant have more than one
            int newWindowWidth = 700;
            int newWindowHeight = 700;

            window.position = new Rect((Screen.currentResolution.width - newWindowWidth) / 2, (Screen.currentResolution.height - newWindowHeight) / 2, newWindowWidth, newWindowHeight);
            window.m_wasCreatedCorrectly = true;

            return true;
        }

        public static bool OpenWindowWithNode(node_NodeBase node, bool loadedIn = true)
        {
            bool returnValue = OpenWindow();

            m_nodeInEditor = node; //loads the node into us
            m_wasNodeLoadedIntoEditor = loadedIn; //so we can do things like delete it

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
                    e_onOpened(m_nodeInEditor, m_wasNodeLoadedIntoEditor);
                }
            }
        }
        #endregion

        private void OnGUI()
        {
            float textHeight = 17;
            int alertBoxHeight = 25;

            m_displayAlert = gui_UnnioFeatures.InvalidFieldAlert(m_displayAlert, (int)position.width, alertBoxHeight, $"invaild conditions detected at indexes ({m_displayAlertBadConditions})");

            m_displayNormalSetting = EditorGUILayout.Foldout(m_displayNormalSetting, "main setting");

            if (m_displayNormalSetting == true)
            {
                m_nodeInEditor.m_title = EditorGUILayout.TextField(m_nodeInEditor.m_title);
                m_nodeInEditor.m_description = EditorGUILayout.TextArea(m_nodeInEditor.m_description, GUILayout.Height(textHeight * 6)); //17 is text height //FOR THE LOVE OF GOD WHY DOSENT GUI LAYOUT USE THE NON BROKEN FUNCTION IN EditorGUILayout 

                GUILayout.BeginHorizontal(); //select type
                GUILayout.Space(3);
                GUILayout.Label("node type color: ", GUILayout.Width((position.width / 2) - 3));
                m_nodeInEditor.m_color = EditorGUILayout.ColorField(m_nodeInEditor.m_color, GUILayout.Width((position.width / 2) - 6));
                GUILayout.EndHorizontal();


                GUILayout.BeginHorizontal();
                GUILayout.Space(3);
                GUILayout.Label("node owner/user:", GUILayout.Width((position.width /2) -3));
                m_nodeInEditor.m_user = EditorGUILayout.TextField(m_nodeInEditor.m_user, GUILayout.Width((position.width /2) -6));
                GUILayout.EndHorizontal();
                
                GUILayout.BeginHorizontal();
                GUILayout.Space(3);
                GUILayout.Label("node deadline (dose not have to be a date):", GUILayout.Width((position.width / 2) - 3));
                m_nodeInEditor.m_deadline = EditorGUILayout.TextField(m_nodeInEditor.m_deadline, GUILayout.Width((position.width / 2) - 6));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(3);
                GUILayout.Label("node status (example <STAT=0> - <STAT=5>):", GUILayout.Width((position.width / 2) - 3));
                m_nodeInEditor.m_status = EditorGUILayout.TextField(m_nodeInEditor.m_status, GUILayout.Width((position.width / 2) - 6));
                GUILayout.EndHorizontal();

                GUILayout.Space(10);
            }

            m_displayConditions = EditorGUILayout.Foldout(m_displayConditions, "condition settings");

            if (m_displayConditions == true)
            {
                
                if (m_nodeInEditor.m_nodeConditions.Count >= 1)
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("add condition", GUILayout.MaxWidth(((position.width / 3) * 2) - 3)) == true) { AddConditionToNodeInEditor(); }
                    if (GUILayout.Button("remove all condition", GUILayout.MaxWidth((position.width / 3) - 3)) == true) { RemoveAllConditionsFromNodeInEditor(); }
                    GUILayout.EndHorizontal();
                }
                else
                {
                    if (GUILayout.Button("add condition") == true) { AddConditionToNodeInEditor(); }
                }

                CreateUIConditions();
            }

            GUILayout.FlexibleSpace();


            string DeleteExitButtonText = "delete node";

            if (m_wasNodeLoadedIntoEditor == false)
            {
                DeleteExitButtonText = "cancel";
            }

            if (GUILayout.Button(DeleteExitButtonText) == true) //the delete / cancel button
            {
                if (m_wasNodeLoadedIntoEditor == false || EditorUtility.DisplayDialog("delete node", "click yes to DELETE the node or pervent it from being created", "yes DELETE node", "no") == true)
                {
                    m_nodeInEditor = null; //delete it
                    Close();
                }
            }

            if (GUILayout.Button("finish") == true)
            {
                m_displayAlertBadConditionsCount =  0;
                m_displayAlertBadConditions = "";
                int maxErroredConditionsToDisplay = 16; //how may bad conditions can we inform the display alert about

                bool ProceedToExit = true;

                for (int i = 0; i < m_nodeInEditor.m_nodeConditions.Count; i++)
                {
                    if (node_Oparator.IsValidCondition(m_nodeInEditor.m_nodeConditions[i]) == false || m_nodeInEditor.m_nodeConditions[i].m_conditionType == node_NodeConditionTypes.None) //dose it work
                    {
                        if (m_displayAlertBadConditionsCount < maxErroredConditionsToDisplay) //inform alert display of errors if it isnt over welmed with errrors to display
                        {
                            m_displayAlertBadConditionsCount++;
                            m_displayAlertBadConditions += i.ToString() + ",";
                        }

                        m_displayAlert = true; //display alert

                        ProceedToExit = false; //no it dose not work
                        //m_nodeInEditor.m_nodeConditions[i].m_testAgainstString = "INVALID INPUT, FIX HERE";  //now that we got m_displayAlertBadConditions we dont need this

                        if (m_nodeInEditor.m_nodeConditions[i].m_conditionType == node_NodeConditionTypes.None) //none types are not fixable so you might as well remove them
                        {
                            m_nodeInEditor.m_nodeConditions[i].m_testAgainstString = "POINTLESS CONDITION, PLEASE REMOVE";
                        }
                    }
                }

                if (m_displayAlertBadConditionsCount > 0)
                {
                    m_displayAlertBadConditions = m_displayAlertBadConditions.Substring(0, m_displayAlertBadConditions.Length - 1); //gets rid of trailing comma
                    if (m_displayAlertBadConditionsCount >= maxErroredConditionsToDisplay) //to indercate that there is more than can be displayed
                    {
                        m_displayAlertBadConditions += ",...";
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
                e_onClosed(m_nodeInEditor, m_wasNodeLoadedIntoEditor);
            }
        }

        #region data about editor & node in editor
        public static bool IsThisNodeInEditor(node_NodeBase node)
        {
            return (m_nodeInEditor == node && m_editorOpen == true);
        }

        public static bool ShouldNodeBeRemoved()
        {
            return (m_nodeInEditor == null);
        }

        public static bool IsEditorOpen()
        {
            return m_editorOpen;
        }

        private static void AddConditionToNodeInEditor()
        {
            m_nodeInEditor.m_nodeConditions.Add(new node_NodeCondition());
        }

        private static void RemoveAllConditionsFromNodeInEditor()
        {
            m_nodeInEditor.m_nodeConditions.Clear();
        }
        #endregion

        #region UI
        private void CreateUIConditions()
        {
            float xPadding = 6;

            float xConditionPadding = 6;
            float yConditionPadding = 3;
            float xConditionSize = position.width - (xPadding + (xConditionPadding *3));

            string[] conditionTypes = new string[] 
            { 
                "None",
                "Asset File Created",
                "Function Created",
                "Function Created In Class",
                "Variable Created In Class",
                "Namespace Created",
                "Scene Created", 
                "Scene Count Is More Than", 
                "Date Is More Than",
            }; //UPDATE THIS IF ADDING MROE CONDITIONS

            m_conditionsScrollPos = EditorGUILayout.BeginScrollView(m_conditionsScrollPos, GUILayout.Width(position.width - xPadding));


            for (int i = 0; i < m_nodeInEditor.m_nodeConditions.Count; i++)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(1));

                if (GUILayout.Button("remove condition", GUILayout.Width(xConditionSize - (xConditionPadding *2.4f)) ) == true) 
                { 
                    m_nodeInEditor.m_nodeConditions.Remove(m_nodeInEditor.m_nodeConditions[i]);

                    i--; //for safety
                    if (i < 0) { i = 0; }

                    if (m_nodeInEditor.m_nodeConditions.Count <= 0) //no nodes means end rendering or bad things will happen
                    {
                        EditorGUILayout.EndScrollView();
                        GUILayout.EndVertical();
                        return;
                    }
                }

                GUILayout.BeginHorizontal(); //select type
                GUILayout.Label("condition type:", GUILayout.Width((xConditionSize / 2) - (xConditionPadding *1.5f)));
                m_nodeInEditor.m_nodeConditions[i].m_conditionType = (node_NodeConditionTypes)EditorGUILayout.Popup((int)m_nodeInEditor.m_nodeConditions[i].m_conditionType, conditionTypes, GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal(); //select what we are testing for
                GUILayout.Label("testing for:", GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f)));
                m_nodeInEditor.m_nodeConditions[i].m_testAgainstString = EditorGUILayout.TextField(m_nodeInEditor.m_nodeConditions[i].m_testAgainstString, GUILayout.Width(((xConditionSize / 2) - (xConditionPadding * 1.5f))));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal(); //select what we are testing for
                GUILayout.Label("stack to move to if tests pases:", GUILayout.Width((xConditionSize /2)  - (xConditionPadding * 1.5f)));
                int.TryParse(EditorGUILayout.TextField(m_nodeInEditor.m_nodeConditions[i].m_onTestPassMoveToStack.ToString(), GUILayout.Width((xConditionSize / 2) - (xConditionPadding * 1.5f))), out m_nodeInEditor.m_nodeConditions[i].m_onTestPassMoveToStack);
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();

                GUILayout.Space(yConditionPadding * 2);

            }

            EditorGUILayout.EndScrollView();
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
                    case 0: //intro
                        GUILayout.BeginArea(mainRect, m_styleMainBoxTutorial);
                        GUILayout.Label("This is the node editor", gui_UnnioFeatures.StackTitleTextStyle());
                        GUILayout.Label("It is used to make nodes and also edit them when you click the button in the top right of the nodes that has three dots.", gui_UnnioFeatures.NodeTitleTextStyle());
                        GUILayout.Space(xSpacing);
                        GUILayout.Label("To get started change the title of your node.", gui_UnnioFeatures.NodeTitleTextStyle());
                        GUILayout.EndArea();

                        if (m_nodeInEditor.m_title != "... title here") //condition to progress tutorial
                        {
                            if (Event.current.type == EventType.Repaint) //do stop things from moving when in layout stage
                            {
                                Repaint();
                                m_tutorialProgress++;
                            }
                        }

                        break;

                    case 1:
                        GUILayout.BeginArea(halfRect, m_styleHalfBoxTutorial);
                        GUILayout.Label("Now change the description", gui_UnnioFeatures.StackTitleTextStyle());

                        GUILayout.Label("Descriptions can only be seen in the node editor but they can provide useful context on a task.", gui_UnnioFeatures.NodeTitleTextStyle());
                        GUILayout.EndArea();

                        if (m_nodeInEditor.m_description != "... description here") //condition to progress tutorial
                        {
                            if (Event.current.type == EventType.Repaint)
                            {
                                Repaint();
                                m_tutorialProgress++;
                            }
                        }

                        break;

                    case 2: //main settings
                        GUILayout.BeginArea(halfhalfRect, m_styleHalfHalfBoxTutorial); 
                        GUILayout.Label("Next you want to change the colour", gui_UnnioFeatures.StackTitleTextStyle());

                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("or click this to skip") == true) //push to skip
                        {
                            m_skipingPartOfTutorial = true; //this is used because we can only skip when in repaint stange without causing issues
                        }

                        GUILayout.EndArea();

                        if (m_nodeInEditor.m_color != new Color(0, 0, 0, 0)) //condition to progress tutorial
                        {
                            if (Event.current.type == EventType.Repaint)
                            {
                                Repaint();
                                m_tutorialProgress++;
                            }
                        }

                        if (m_skipingPartOfTutorial == true) //skip over section
                        {
                            if (Event.current.type == EventType.Repaint)
                            {
                                Repaint();
                                m_skipingPartOfTutorial = false; //if skipping this needs to be set to false when skip complete
                                m_tutorialProgress += 2;
                            }
                        }

                        break;

                    case 3:
                        GUILayout.BeginArea(halfhalfRect, m_styleHalfHalfBoxTutorial);
                        GUILayout.Label("also dont forget to change the alpha", gui_UnnioFeatures.StackTitleTextStyle());
                        GUILayout.Label("So you can see it.", gui_UnnioFeatures.NodeTitleTextStyle());

                        GUILayout.EndArea();

                        if (m_nodeInEditor.m_color.a > 0) //condition to progress tutorial
                        {
                            if (Event.current.type == EventType.Repaint)
                            {
                                Repaint();
                                m_tutorialProgress++;
                            }
                        }

                        break;

                    case 4:
                        GUILayout.BeginArea(halfRect, m_styleHalfBoxTutorial);
                        GUILayout.Label("Now change the deadline to anything you want", gui_UnnioFeatures.StackTitleTextStyle());
                        GUILayout.Label("You can also change the node user this way if you want this will then display a field in the node with the text 'deadline: <yourTextHere>'.", gui_UnnioFeatures.NodeTitleTextStyle());

                        GUILayout.EndArea();

                        if (m_nodeInEditor.m_deadline != "") //condition to progress tutorial
                        {
                            if (Event.current.type == EventType.Repaint)
                            {
                                Repaint();
                                m_tutorialProgress++;
                            }
                        }

                        break;

                    case 5:
                        GUILayout.BeginArea(mainRect, m_styleMainBoxTutorial);
                        GUILayout.Label("Now change the status to <STAT=0>", gui_UnnioFeatures.StackTitleTextStyle());
                        GUILayout.Label("The status can be anything you want but if you set it to <STAT=?> with the question mark being a number 0 to 5 it will render as some diffrent text in a status field insted of <STAT=0>.", gui_UnnioFeatures.NodeTitleTextStyle());
                        GUILayout.Space(xSpacing);
                        GUILayout.Label("<STAT=0> will make the status field render the text 'status: to be done'", gui_UnnioFeatures.NodeTitleTextStyle());
                        GUILayout.EndArea();

                        if (m_nodeInEditor.m_status == "<STAT=0>") //condition to progress tutorial
                        {
                            if (Event.current.type == EventType.Repaint)
                            {
                                Repaint();
                                m_tutorialProgress++;
                            }
                        }

                        break;

                    case 6: //node conditions
                        GUILayout.BeginArea(mainRect, m_styleMainBoxTutorial);
                        GUILayout.Label("Now lets add conditions", gui_UnnioFeatures.StackTitleTextStyle());
                        GUILayout.Label("Conditions are used to make nodes move around automaticlly base on if the condition returns true tested with the test for string.", gui_UnnioFeatures.NodeTitleTextStyle());
                        GUILayout.Space(xSpacing);
                        GUILayout.Label("To make a condition click add condition.", gui_UnnioFeatures.NodeTitleTextStyle());

                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("or click this to skip") == true)
                        {
                            m_skipingPartOfTutorial = true;
                        }
                        GUILayout.EndArea();

                        if (m_nodeInEditor.m_nodeConditions.Count > 0) //condition to progress tutorial
                        {
                            if (Event.current.type == EventType.Repaint)
                            {
                                Repaint();
                                m_tutorialProgress++;
                            }
                        }

                        if (m_skipingPartOfTutorial == true) //section skip
                        {
                            if (Event.current.type == EventType.Repaint)
                            {
                                Repaint();
                                m_tutorialProgress += 3;
                                m_skipingPartOfTutorial = false;
                            }
                        }

                        break;

                    case 7:
                        GUILayout.BeginArea(mainRect, m_styleMainBoxTutorial);
                        GUILayout.Label("Set number and change type.", gui_UnnioFeatures.StackTitleTextStyle());
                        GUILayout.Label("In the condition you will see a condition type, a testing for field and and number field and one the tests passes the node will be moved to the stack that shares this number or removed if the number is bellow 0.", gui_UnnioFeatures.NodeTitleTextStyle());
                        GUILayout.Space(xSpacing);
                        GUILayout.Label("set the number field in the first condition to negative one and change the type to 'Asset File Created'.", gui_UnnioFeatures.NodeTitleTextStyle());
                        GUILayout.EndArea();

                        if ((m_nodeInEditor.m_nodeConditions[0].m_onTestPassMoveToStack < 0 && m_nodeInEditor.m_nodeConditions[0].m_conditionType == node_NodeConditionTypes.AssetFileCreated)) //condition to progress tutorial
                        {
                            if (Event.current.type == EventType.Repaint)
                            {
                                Repaint();
                                m_tutorialProgress++;
                            }
                        }

                        break;

                    case 8:
                        GUILayout.BeginArea(mainRect, m_styleMainBoxTutorial);
                        GUILayout.Label("Setting testing for", gui_UnnioFeatures.StackTitleTextStyle());
                        GUILayout.Label("The testing for input field is where you would set the right part of the condition.", gui_UnnioFeatures.NodeTitleTextStyle());
                        GUILayout.Space(xSpacing);
                        GUILayout.Label("It must be compatible with the left part (that part thats being mached aginst the testing for input) or you will not be able to click finish.", gui_UnnioFeatures.NodeTitleTextStyle());
                        GUILayout.Space(xSpacing);
                        GUILayout.Label("For this example come up with a file name and extension like so ExampleClass.cs.", gui_UnnioFeatures.NodeTitleTextStyle());
                        GUILayout.EndArea();

                        if (m_nodeInEditor.m_nodeConditions.Count > 0 && node_Oparator.IsValidCondition(m_nodeInEditor.m_nodeConditions[0]) == true) //condition to progress tutorial
                        {
                            if (Event.current.type == EventType.Repaint)
                            {
                                Repaint();
                                m_tutorialProgress++;
                            }
                        }

                        break;

                    case 9: //node condition removal
                        GUILayout.BeginArea(mainRect, m_styleMainBoxTutorial);
                        GUILayout.Label("Node condition removal", gui_UnnioFeatures.StackTitleTextStyle());
                        GUILayout.Label("like before there will now be two remove buttons. The one next to the add condition button will remove all conditions and the ones in the conditions them selfs will just remove that condition.", gui_UnnioFeatures.NodeTitleTextStyle());
                        GUILayout.Space(xSpacing);
                        GUILayout.Label("now remove all your condition", gui_UnnioFeatures.NodeTitleTextStyle());
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("or click this to skip") == true)
                        {
                            m_skipingPartOfTutorial = true;
                        }

                        GUILayout.EndArea();

                        if (m_nodeInEditor.m_nodeConditions.Count <= 0 || m_skipingPartOfTutorial == true) //condition to progress tutorial and section skip
                        {
                            if (Event.current.type == EventType.Repaint)
                            {
                                Repaint();
                                m_tutorialProgress++;
                                m_skipingPartOfTutorial = false;
                            }
                        }

                        break;

                    case 10: //ending
                        GUILayout.BeginArea(mainRect, m_styleMainBoxTutorial);
                        GUILayout.Label("closing remarks", gui_UnnioFeatures.StackTitleTextStyle());
                        GUILayout.Label("A lot of the other conditions are not that hard to figure out but I do recommend you read the tutorial index/menu for the code conditions. Likewise as an extra challange figgure out what all the other statuses are.", gui_UnnioFeatures.NodeTitleTextStyle());
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("close node tutorial") == true)
                        {
                            m_skipingPartOfTutorial = true;
                        }

                        GUILayout.EndArea();

                        if (m_skipingPartOfTutorial == true) //finalize end
                        {
                            if (Event.current.type == EventType.Repaint)
                            {
                                Repaint();
                                m_tutorialProgress++;
                                m_skipingPartOfTutorial = false;
                                m_tutorialCompleted = true; //when this is true the node part of the tutorial will not play again
                            }
                        }

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