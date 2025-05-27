#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.IO;
using System;

using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEditor.PackageManager.UI;
using UnityEngine.TextCore.Text;

namespace EditorPackages.Unnio
{
    public class hud_UnnioBoardEditor : EditorWindow
    {
        #region private vars: drag data
        private List<Rect> m_objectSensorRects = new List<Rect>();
        private List<Vector2> m_objectSensorResults = new List<Vector2>();
        private List<int> m_objectSensorVerticalNodePadding = new List<int>();
        private Vector2 m_objectCurrentlyInDrag = new Vector2(-1, -1);
        #endregion

        #region private vars: scroll offsets & expansion
        private Vector2 m_boardScrollOffset = Vector2.zero;
        private List<Vector2> m_stackScrollOffsets = new List<Vector2>();
        private List<bool> m_stackExpanded = new List<bool>();
        private bool m_dragObjectinRendering = false;
        #endregion

        #region private vars: search data
        private string m_displaySearchString = "";
        private string m_searchString = "";
        private List<Vector2> m_objectSearchResults = new List<Vector2>();
        #endregion

        #region private vars: removal check
        private int m_oldStackCountForRemovalCheck = board_BoardBase.m_childStacks.Count;
        private int m_lastStackToBeEditited = -1;
        #endregion

        #region private vars: tutorial progress
        private int m_tutorialProgress = 0;
        private bool m_tutorialCompleted = false;
        private bool m_skipingPartOfTutorial = false;
        #endregion

        #region private vars: styles
        private GUIStyle m_styleMainBoxTutorial = null;
        private GUIStyle m_styleHalfBoxTutorial = null;
        private GUIStyle m_styleHalfHalfBoxTutorial = null;
        #endregion


        #region on window creation
        [MenuItem("Window/Unnio/Open board")]
        public static void OpenWindow()
        {
            hud_UnnioBoardEditor window = GetWindow<hud_UnnioBoardEditor>("Unnio: Board display"); //cant have more than one
            window.position = new Rect(0, 0, Screen.currentResolution.width - 10, Screen.currentResolution.height - 110);
        }

        private void Awake()
        {
            LoadStackExpandedStatus();
        }
        #endregion

        private void Update()
        {
            if (m_objectCurrentlyInDrag.x >= 0) //if we are draging something this will make sure it dosent look laggy
            {
                m_dragObjectinRendering = true;
            }

            if (m_dragObjectinRendering == true)
            {
                Repaint();
            }

            if (m_objectCurrentlyInDrag.x < 0) //ends draging repaint once draging is complete
            {
                m_dragObjectinRendering = false;
            }
        }

        private void OnGUI()
        {
            //board_BoardBase.RemovalAuthorization(false);

            int bigStackSize = 374;
            int smallStackSize = 80;
            int horzontalScrollBarPaddingFromWindowEnd = 45;
            int stackPaddingFromScrollBar = 20;

            if (Event.current.type == EventType.Repaint) //so we can repaint the drag sensors without issues
            {
                m_objectSensorVerticalNodePadding.Clear();
                m_objectSensorRects.Clear();
                m_objectSensorResults.Clear();
            }

            if (board_BoardBase.m_childStacks.Count >= 1) //should manual save render
            { //yes because we got something woth saving
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("add stack", GUILayout.MaxWidth(((position.width / 3) * 2) - 3)) == true) { board_BoardBase.AddStack(); }
                if (GUILayout.Button("manual board save", GUILayout.MaxWidth((position.width / 3) - 3)) == true) { ManualSave(); }
                GUILayout.EndHorizontal();
            }
            else //no
            {
                if (GUILayout.Button("add stack") == true) { board_BoardBase.AddStack(); }
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(4);
            m_displaySearchString = EditorGUILayout.TextField(m_displaySearchString, GUI.skin.FindStyle("ToolbarSearchTextField"));
            GUILayout.EndHorizontal();

            if (m_displaySearchString != m_searchString)
            {
                m_searchString = m_displaySearchString;
                UpdateSearch();
            }

            m_boardScrollOffset = EditorGUILayout.BeginScrollView(m_boardScrollOffset, false, false, GUILayout.Width(position.width), GUILayout.Height(position.height - horzontalScrollBarPaddingFromWindowEnd)); //container for stancks
            GUILayout.BeginHorizontal();

            int Xlocation = 8;
            for (int i = 0; i < board_BoardBase.m_childStacks.Count; i++) //render all stacks
            {
                if (m_stackExpanded.Count <= i)
                {
                    m_stackExpanded.Add(true);
                }

                if (m_stackScrollOffsets.Count <= i)
                {
                    m_stackScrollOffsets.Add(Vector2.zero);
                }

                if (m_objectSearchResults.Count <= 0 || m_objectSearchResults.Contains(new Vector2(i, -1)) == true) //filtes out the stacks that did not turn up in the search
                {
                    m_stackExpanded[i] = CreateUIStack(i, bigStackSize, smallStackSize, (int)(position.height - (horzontalScrollBarPaddingFromWindowEnd + stackPaddingFromScrollBar)), Xlocation, -1, m_stackExpanded[i]);

                    if (m_stackExpanded[i] == true) //this will help us make sure everthing is layed out right
                    {
                        Xlocation += bigStackSize;
                    }
                    else
                    {
                        Xlocation += smallStackSize;
                    }
                }
            }

            RunObjectSensor(Event.current.mousePosition);
            CreateUIDragObject(380, 60);

            GUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();

            if (board_BoardBase.IsTutorialBoard() == true)
            {
                CreateUITutorial();
            }

            UpdateStackStatusAndScrollbars();

            //board_BoardBase.RemovalAuthorization(true); //appently even if we remove objects at the end of the function or not long after it will still cause issues ... WHY
        }

        private void OnDestroy()
        {
            //board_BoardBase.RemovalAuthorization(true);
            SaveStackExpandedStatus();

            if (m_tutorialCompleted == true) //so the tutorial can come to an end
            {
                board_BoardBase.RemoveTutorial();
                board_BoardBase.SaveBoard();
            }
        }

        #region UI
        private void CreateUIDragObject(int width, int height)//renders the current draging object
        {
            if (m_objectCurrentlyInDrag.x >= 0) //render ghost item
            {
                if (m_objectCurrentlyInDrag.y >= 0) //is node
                {
                    CreateUIDragObjectAsNode(width, height);
                }
                else
                {
                    CreateUIDragObjectAsStack(width, height);
                }
            }
        }

        private void CreateUIDragObjectAsNode(int width, int height)//renders a dragable node
        {
            if (board_BoardBase.m_childStacks[(int)m_objectCurrentlyInDrag.x] == null) { return; } //is stack valid
            if (board_BoardBase.m_childStacks[(int)m_objectCurrentlyInDrag.x].m_childNodes[(int)m_objectCurrentlyInDrag.y] == null) { return; } //is node valid

            int topSpacing = 2; //genral settings
            int crossSpacing = 3;
            int colorIndercatorSizeX = 60;
            int colorIndercatorSizeY = 18;
            int textMaxLength = 16;
            float alphaMuiltyply = 0.75f;

            Color boxColFg = new Color(0.05f, 0.05f, 0.05f, alphaMuiltyply); //color settings
            Color boxColBg = new Color(0.20f, 0.20f, 0.20f, alphaMuiltyply);
            Color objectColor = board_BoardBase.m_childStacks[(int)m_objectCurrentlyInDrag.x].m_childNodes[(int)m_objectCurrentlyInDrag.y].m_color;
            objectColor.a = objectColor.a * alphaMuiltyply;

            if (EditorGUIUtility.isProSkin == false) //darke mode color settings
            {
                boxColFg = new Color(0.95f, 0.95f, 0.95f, alphaMuiltyply);
                boxColBg = new Color(0.80f, 0.80f, 0.80f, alphaMuiltyply);
            }

            string title = board_BoardBase.m_childStacks[(int)m_objectCurrentlyInDrag.x].m_childNodes[(int)m_objectCurrentlyInDrag.y].m_title;
            if (title.Length >= textMaxLength)
            {
                title = title.Substring(0, textMaxLength -3) + "...";
            }

            GUILayout.BeginArea(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y - 43, width, height), gui_UnnioFeatures.BoxWithColor(boxColFg, boxColBg, width, height));

            GUILayout.BeginHorizontal();
            GUILayout.Space(topSpacing); //colour indercator
            GUILayout.BeginVertical();
            GUILayout.Space(crossSpacing);
            GUILayout.Label("", gui_UnnioFeatures.ColorIndicatorStyle(colorIndercatorSizeX, colorIndercatorSizeY, objectColor), GUILayout.Width(colorIndercatorSizeX), GUILayout.Height(colorIndercatorSizeY));
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Label(title, gui_UnnioFeatures.NodeTitleAlphaTextStyle()); //node title

            GUILayout.EndArea();
        }

        private void CreateUIDragObjectAsStack(int width, int height)//renders a dragable stack
        {
            if (board_BoardBase.m_childStacks[(int)m_objectCurrentlyInDrag.x] == null) { return; } //is it a valid stack

            int topSpacing = 2; //generic settings
            int crossSpacing = 3;
            int colorIndercatorSizeX = 160;
            int colorIndercatorSizeY = 18;
            int textMaxLength = 16;
            float alphaMuiltyply = 0.75f;

            Color boxColFg = new Color(0.05f, 0.05f, 0.05f, alphaMuiltyply); //color settings
            Color boxColBg = new Color(0.16f, 0.16f, 0.16f, alphaMuiltyply);
            Color objectColor = board_BoardBase.m_childStacks[(int)m_objectCurrentlyInDrag.x].m_color;
            objectColor.a = objectColor.a * alphaMuiltyply;

            if (EditorGUIUtility.isProSkin == false) //light mode color settings
            {
                boxColFg = new Color(0.95f, 0.95f, 0.95f, alphaMuiltyply);
                boxColBg = new Color(0.74f, 0.74f, 0.74f, alphaMuiltyply);
            }

            string title = board_BoardBase.m_childStacks[(int)m_objectCurrentlyInDrag.x].m_title; //title
            if (title.Length >= textMaxLength) //limits the text size so it cant leve the box
            {
                title = title.Substring(0, textMaxLength - 3) + "...";
            }

            GUILayout.BeginArea(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y - 43, width, height), gui_UnnioFeatures.BoxWithColor(boxColFg, boxColBg, width, height)); //renders the dragable box

            GUILayout.BeginHorizontal();
            GUILayout.Space(topSpacing); //colour indercator
            GUILayout.BeginVertical();
            GUILayout.Space(crossSpacing);
            GUILayout.Label("", gui_UnnioFeatures.ColorIndicatorStyle(colorIndercatorSizeX, colorIndercatorSizeY, objectColor), GUILayout.Width(colorIndercatorSizeX), GUILayout.Height(colorIndercatorSizeY));
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Label(title, gui_UnnioFeatures.StackTitleAlphaTextStyle()); //node title


            GUILayout.EndArea();
        }

        private bool CreateUIStack(int stackIndex, int width, int minWidth, int height, int x, int y, bool expanded) //version which allows the user to switch between the two
        {
            if (expanded == true)
            {
                return CreateStackAsExpanded(stackIndex, width, height, x, y); //will make the stack and its nodes
            }
            else 
            {
                return CreateStackAsUnExpanded(stackIndex, minWidth, height, x, y); //small boi stack
            }
        }

        private bool CreateStackAsExpanded(int stackIndex, int width, int height, int x, int y) //the version which renders nodes
        {
            bool expandedReturn = true;

            if (board_BoardBase.m_childStacks[stackIndex] == null) { return expandedReturn; }

            int nodeSize = 60;
            int nodePaddingFromEnd = 20;
            int topSpacing = 2;
            int crossSpacing = 3;
            int colorIndercatorSizeX = 160;
            int colorIndercatorSizeY = 18;

            GUILayout.BeginVertical(EditorStyles.textField, GUILayout.Width(width), GUILayout.Height(height));

            GUILayout.BeginHorizontal();

            GUILayout.Space(crossSpacing); //colour indercator
            GUILayout.BeginVertical();
            GUILayout.Space(topSpacing);
            GUILayout.Label("", gui_UnnioFeatures.ColorIndicatorStyle(colorIndercatorSizeX, colorIndercatorSizeY, board_BoardBase.m_childStacks[stackIndex].m_color), GUILayout.Width(colorIndercatorSizeX), GUILayout.Height(colorIndercatorSizeY));
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace(); //buttons

            GUILayout.BeginVertical(); //stack id
            GUILayout.Space(topSpacing);
            GUILayout.BeginHorizontal(gui_UnnioFeatures.BoxWithColor(new Color(0, 0.20f, 0.60f), new Color(0, 0.40f, 1.0f), 40, 16), GUILayout.Width(40), GUILayout.Height(16));
            GUILayout.Label(stackIndex.ToString(), GUILayout.Height(15));
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            if (GUILayout.Button("_", GUILayout.Width(20))) //go small
            {
                expandedReturn = false;
            }

            if (GUILayout.Button("...", GUILayout.Width(40))) //editor
            {
                m_lastStackToBeEditited = stackIndex;
                board_BoardBase.EditStack(stackIndex);
            }

            GUILayout.EndHorizontal();
            if (Event.current.type == EventType.Repaint) //so we know what to offset by
            {
                m_objectSensorVerticalNodePadding.Add((int)GUILayoutUtility.GetLastRect().height);
            }

            GUILayout.Label(board_BoardBase.m_childStacks[stackIndex].m_title, gui_UnnioFeatures.StackTitleTextStyle()); //stack title
            if (Event.current.type == EventType.Repaint)
            {
                m_objectSensorVerticalNodePadding[m_objectSensorVerticalNodePadding.Count - 1] += ((int)GUILayoutUtility.GetLastRect().height);
            }

            m_stackScrollOffsets[stackIndex] = EditorGUILayout.BeginScrollView(m_stackScrollOffsets[stackIndex], false, false, GUILayout.Width(width));
            for (int i = 0; i < board_BoardBase.m_childStacks[stackIndex].m_childNodes.Count; i++) //renders the nodes
            {
                int yOffset = 60;

                if (m_objectSensorVerticalNodePadding.Count > 0 && stackIndex < m_objectSensorVerticalNodePadding.Count) //just incase the title takes up more than one line this 
                {
                    yOffset = m_objectSensorVerticalNodePadding[stackIndex];
                }

                CreateUINode(stackIndex, i, width - nodePaddingFromEnd, nodeSize, x, yOffset + 6);
            }
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("add node") == true) //add node
            {
                board_BoardBase.AddNode(stackIndex);
            }

            GUILayout.EndVertical();

            if (Event.current.type == EventType.Repaint) //adds our rect to the lists of dragable objects
            {
                Rect stackRect = new Rect(0, 0, GUILayoutUtility.GetLastRect().width, GUILayoutUtility.GetLastRect().height);
                if (x >= 0 && y >= 0)
                {
                    stackRect.position = new Vector2(x, y);
                }
                else
                {
                    stackRect.position = GUILayoutUtility.GetLastRect().position; //fun fact unity will add the scoll offset to this posistion horziontally but not vertically WHY OH GOD WHY WHY CANT WE GET A SCREEN SPACE RECTS AND WHY DO SOME OF THEM HAVE SCEEN SPACE POSISIONS REGUADLESS GOD OH MIGHT GMIU CONTAINERS WERE NOT MEDE WELL
                }


                m_objectSensorRects.Add(stackRect);
                m_objectSensorResults.Add(new Vector2(stackIndex, -1));
            }

            return expandedReturn;
        }

        private bool CreateStackAsUnExpanded(int stackIndex, int width, int height, int x, int y) //the version which is small
        {
            bool expandedReturn = false;

            if (board_BoardBase.m_childStacks[stackIndex] == null) { return expandedReturn; }

            int topSpacing = 2;
            int crossSpacing = 3;
            int colorIndercatorSizeY = 18;

            GUILayout.BeginVertical(EditorStyles.textField, GUILayout.Width(width), GUILayout.Height(height));

            if (board_BoardBase.m_childStacks[stackIndex].m_color.a > 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(topSpacing); //colour indercator
                GUILayout.BeginVertical();
                GUILayout.Space(crossSpacing);
                GUILayout.Label("", gui_UnnioFeatures.ColorIndicatorStyle(width - (crossSpacing * 3), colorIndercatorSizeY, board_BoardBase.m_childStacks[stackIndex].m_color), GUILayout.Width(width - (crossSpacing * 3)), GUILayout.Height(18));
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }

            if (GUILayout.Button("maximize")) { expandedReturn = true; } //go big button

            m_stackScrollOffsets[stackIndex] = EditorGUILayout.BeginScrollView(m_stackScrollOffsets[stackIndex], false, false, GUILayout.Width(width));
            string newTitle = "";

            for (int i = 0; i < board_BoardBase.m_childStacks[stackIndex].m_title.Length; i++) //title but vertical
            {
                newTitle += board_BoardBase.m_childStacks[stackIndex].m_title[i] + "\n";
            }

            GUILayout.Label(newTitle, gui_UnnioFeatures.StackTitleVerticalTextStyle()); //stack title
            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();
            
            if (Event.current.type == EventType.Repaint) //adds our rect to the lists of dragable objects
            {
                m_objectSensorVerticalNodePadding.Add((int)GUILayoutUtility.GetLastRect().height);

                Rect stackRect = new Rect(0, 0, GUILayoutUtility.GetLastRect().width, GUILayoutUtility.GetLastRect().height);
                if (x >= 0 && y >= 0)
                {
                    stackRect.position = new Vector2(x, y);
                }
                else
                {
                    stackRect.position = GUILayoutUtility.GetLastRect().position; //fun fact unity will add the scoll offset to this posistion horziontally but not vertically WHY OH GOD WHY WHY CANT WE GET A SCREEN SPACE RECTS AND WHY DO SOME OF THEM HAVE SCEEN SPACE POSISIONS REGUADLESS GOD OH MIGHT GMIU CONTAINERS WERE NOT MEDE WELL
                }

                m_objectSensorRects.Add(stackRect);
                m_objectSensorResults.Add(new Vector2(stackIndex, -1));
            }

            return expandedReturn;
        }

        private void CreateUINode(int stackIndex, int nodeIndex, int width, int height, int x, int y)
        {
            if (board_BoardBase.m_childStacks[stackIndex].m_childNodes[nodeIndex] == null) { return; } //hide if invalid
            if (m_objectSearchResults.Count > 0 && m_objectSearchResults.Contains(new Vector2(stackIndex, nodeIndex)) == false) { return; } //hide if it dosent show up in search and there is something being searched for

            int topSpacing = 2;
            int crossSpacing = 3;
            int colorIndercatorSizeX = 60;
            int colorIndercatorSizeY = 18;

            GUIStyle nodeStyle = EditorStyles.helpBox;

            GUILayout.BeginVertical(nodeStyle, GUILayout.Width(width), GUILayout.Height(height));

            GUILayout.BeginHorizontal();

            GUILayout.Space(crossSpacing); //colour indercator
            GUILayout.BeginVertical();
            GUILayout.Space(topSpacing);
            GUILayout.Label("", gui_UnnioFeatures.ColorIndicatorStyle(colorIndercatorSizeX, colorIndercatorSizeY, board_BoardBase.m_childStacks[stackIndex].m_childNodes[nodeIndex].m_color), GUILayout.Width(colorIndercatorSizeX), GUILayout.Height(colorIndercatorSizeY));
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace(); //buttons
            if (GUILayout.Button("...", GUILayout.Width(40))) { board_BoardBase.EditNode(stackIndex, nodeIndex); } //open editor
            GUILayout.EndHorizontal();

            GUILayout.Label(board_BoardBase.m_childStacks[stackIndex].m_childNodes[nodeIndex].m_title, gui_UnnioFeatures.NodeTitleTextStyle()); //node title

            if ((board_BoardBase.m_childStacks[stackIndex].m_childNodes[nodeIndex].m_user != "" && board_BoardBase.m_childStacks[stackIndex].m_childNodes[nodeIndex].m_user != null) || (board_BoardBase.m_childStacks[stackIndex].m_childNodes[nodeIndex].m_deadline != "" && board_BoardBase.m_childStacks[stackIndex].m_childNodes[nodeIndex].m_status != "") || (board_BoardBase.m_childStacks[stackIndex].m_childNodes[nodeIndex].m_deadline != null && board_BoardBase.m_childStacks[stackIndex].m_childNodes[nodeIndex].m_status != null)) //adds a space
            {
                GUILayout.Space(topSpacing);
            }

            if (board_BoardBase.m_childStacks[stackIndex].m_childNodes[nodeIndex].m_user != "" && board_BoardBase.m_childStacks[stackIndex].m_childNodes[nodeIndex].m_user != null) //no point displaying a user if there is no user
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("owner: ", EditorStyles.boldLabel, GUILayout.ExpandWidth(false)); //owner
                GUILayout.Label(board_BoardBase.m_childStacks[stackIndex].m_childNodes[nodeIndex].m_user, EditorStyles.wordWrappedLabel);
                GUILayout.EndHorizontal();
            }

            if (board_BoardBase.m_childStacks[stackIndex].m_childNodes[nodeIndex].m_status != "" && board_BoardBase.m_childStacks[stackIndex].m_childNodes[nodeIndex].m_status != null) //no point displaying a status if there is no status
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("status: ", EditorStyles.boldLabel, GUILayout.ExpandWidth(false)); //status
                GUILayout.Label(ParseStatus(board_BoardBase.m_childStacks[stackIndex].m_childNodes[nodeIndex].m_status), EditorStyles.wordWrappedLabel);
                GUILayout.EndHorizontal();
            }

            if (board_BoardBase.m_childStacks[stackIndex].m_childNodes[nodeIndex].m_deadline != "" && board_BoardBase.m_childStacks[stackIndex].m_childNodes[nodeIndex].m_deadline != null) //no point displaying a deadline if there is no deadline
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("deadline: ", EditorStyles.boldLabel, GUILayout.ExpandWidth(false)); //deadline
                GUILayout.Label(board_BoardBase.m_childStacks[stackIndex].m_childNodes[nodeIndex].m_deadline, EditorStyles.wordWrappedLabel);
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();

            if (Event.current.type == EventType.Repaint) //so it can be draged
            {
                Rect nodeRect = new Rect(0, 0, GUILayoutUtility.GetLastRect().width +1, GUILayoutUtility.GetLastRect().height -1);

                if (x >= 0 && y >= 0) //taking the boxes out of local space
                {
                    nodeRect.position = new Vector2(x, ((y -1) + GUILayoutUtility.GetLastRect().y) - m_stackScrollOffsets[stackIndex].y);

                    if ((GUILayoutUtility.GetLastRect().y + 10) - (m_stackScrollOffsets[stackIndex].y) <= 0) //stop buttons being visible if there out of scroll view
                    {
                        nodeRect.y = -999;
                    }
                }
                else
                {
                    nodeRect.position = GUILayoutUtility.GetLastRect().position;
                }

                m_objectSensorRects.Add(nodeRect);
                m_objectSensorResults.Add(new Vector2(stackIndex, nodeIndex));
            }
        }

        private void CreateUITutorial()
        {
            Rect mainRect = new Rect(position.width - 620.0f, position.height - 320.0f, 600, 300); //the diffrent size boxes that can be used
            Rect halfRect = new Rect(position.width - 620.0f, position.height - 170.0f, 600, 150);
            Rect halfhalfRect = new Rect(position.width - 620.0f, position.height - 95.0f, 600, 75);

            float xSpacing = 20; //spacing
            float alphaMuiltyply = 0.45f;
            Color boxColFg = new Color(0.60f, 0.05f, 0.00f, alphaMuiltyply); //colours
            Color boxColBg = new Color(1.00f, 0.40f, 0.00f, alphaMuiltyply);

            if (m_styleMainBoxTutorial == null || m_styleMainBoxTutorial.normal.background == null) //help make the tutorial more effecient to render since a of these boxes have a lot of area 
            {
                m_styleMainBoxTutorial = gui_UnnioFeatures.BoxWithColor(boxColFg, boxColBg, (int)mainRect.width, (int)mainRect.height); //it should also be noted they have to be created in on gui because all styles can lose there textures every once in awhile ... finding that out was a very annoying process and its also annoying unity have a cheat for this (there styles arnt texture based) that no one else can use
                m_styleHalfBoxTutorial = gui_UnnioFeatures.BoxWithColor(boxColFg, boxColBg, (int)halfRect.width, (int)halfRect.height);
                m_styleHalfHalfBoxTutorial = gui_UnnioFeatures.BoxWithColor(boxColFg, boxColBg, (int)halfhalfRect.width, (int)halfhalfRect.height);
            }

            try
            {
                switch (m_tutorialProgress)
                {
                    case 0: //opening
                        GUILayout.BeginArea(mainRect, m_styleMainBoxTutorial);
                        GUILayout.Label("Hello and welcome to the built in tutorial.", gui_UnnioFeatures.StackTitleTextStyle());
                        GUILayout.Label("Right now you are looking at a blank/empty board with a search bar and add stack button at the top.", gui_UnnioFeatures.NodeTitleTextStyle());
                        GUILayout.Space(xSpacing);
                        GUILayout.Label("To get started click the add stack button at the top.", gui_UnnioFeatures.NodeTitleTextStyle());
                        GUILayout.EndArea();

                        if (board_BoardBase.m_childStacks.Count > 0) //condition to progress tutorial
                        {
                            if (Event.current.type == EventType.Repaint) //we can only do switch on repaint or hot control could crash
                            {
                                Repaint(); //so it updates the display
                                m_tutorialProgress++;
                            }
                        }

                        break;

                    case 1: //covering stacks
                        GUILayout.BeginArea(mainRect, m_styleMainBoxTutorial);

                        GUILayout.Label("Now that you have your stack you will see a blue box with a number, a small button with a '_' in it and a button thee dots like so '...'.", gui_UnnioFeatures.NodeTitleTextStyle());
                        GUILayout.Space(xSpacing);
                        GUILayout.Label("The box with a number is the box that shows you the ID of the stack and this is used for node conditions the box with a '_' in it is used to shink a stack and the three dots button can allow you to edit a stack.", gui_UnnioFeatures.NodeTitleTextStyle());

                        GUILayout.Space(xSpacing * 2);
                        GUILayout.Label("Shrink the fist stack to proceed.", gui_UnnioFeatures.NodeTitleTextStyle());

                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("or click this to skip") == true) //skip
                        {
                            m_skipingPartOfTutorial = true; //we use that so we can make sure its being done on repaint
                        }


                        GUILayout.EndArea();

                        if ((m_stackExpanded.Count > 0 && m_stackExpanded[0] == false)) //condition to progress tutorial
                        {
                            if (Event.current.type == EventType.Repaint)
                            {
                                Repaint();
                                m_skipingPartOfTutorial = false; //needs to be set back to false after use
                                m_tutorialProgress++;
                            }
                        }

                        if (m_skipingPartOfTutorial == true) //section skip
                        {
                            if (Event.current.type == EventType.Repaint)
                            {
                                Repaint();
                                m_skipingPartOfTutorial = false;
                                m_tutorialProgress += 2;
                            }
                        }

                        break;

                    case 2:
                        GUILayout.BeginArea(halfhalfRect, m_styleHalfHalfBoxTutorial);
                        GUILayout.Label("Now click the maximize button to make it big again", gui_UnnioFeatures.StackTitleTextStyle());
                        GUILayout.FlexibleSpace();
                        if (m_stackExpanded.Count > 0 && m_stackExpanded[0] == true) //condition to progress tutorial
                        {
                            if (Event.current.type == EventType.Repaint)
                            {
                                Repaint();
                                m_tutorialProgress++;
                            }
                        }

                        GUILayout.EndArea();

                        break;

                    case 3: //covering nodes
                        GUILayout.BeginArea(halfRect, m_styleHalfBoxTutorial);
                        GUILayout.Label("Now that we have a stack we want to add a node to it.", gui_UnnioFeatures.NodeTitleTextStyle());
                        GUILayout.Space(xSpacing);
                        GUILayout.Label("Click add node which is at the bottom of maximize stack", gui_UnnioFeatures.StackTitleTextStyle());
                        GUILayout.FlexibleSpace();
                        GUILayout.EndArea();

                        for (int i = 0; i < board_BoardBase.m_childStacks.Count; i++) //condition to progress tutorial
                        {
                            if (board_BoardBase.m_childStacks[i].m_childNodes.Count > 0)
                            {
                                if (Event.current.type == EventType.Repaint)
                                {
                                    Repaint();
                                    m_tutorialProgress++;
                                    break;
                                }
                            }
                        }

                        break;

                    case 4: //search bar
                        GUILayout.BeginArea(halfRect, m_styleHalfBoxTutorial);
                        GUILayout.Label("next you shoud try the search bar", gui_UnnioFeatures.StackTitleTextStyle());
                        GUILayout.Space(xSpacing);
                        GUILayout.Label("To so I recommend you make more nodes first.", gui_UnnioFeatures.NodeTitleTextStyle());
                        GUILayout.FlexibleSpace();
                        GUILayout.EndArea();

                        if (m_objectSearchResults.Count > 0) //condition to progress tutorial
                        {
                            if (Event.current.type == EventType.Repaint)
                            {
                                Repaint();
                                m_tutorialProgress++;
                            }
                        }

                        break;

                    case 5: //ending
                        GUILayout.BeginArea(mainRect, m_styleMainBoxTutorial);
                        GUILayout.Label("lastly you most likely want to save your work", gui_UnnioFeatures.StackTitleTextStyle());
                        GUILayout.Space(xSpacing);
                        GUILayout.Label("To do so click the manual save button or close the board and after that this tutorial will be gone", gui_UnnioFeatures.NodeTitleTextStyle());
                        GUILayout.Space(xSpacing);
                        GUILayout.Label("Or click close tutorial at the bottom", gui_UnnioFeatures.NodeTitleTextStyle());


                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("close all tutorials and restore board to normal") == true)
                        {
                            m_skipingPartOfTutorial = true;
                        }

                        GUILayout.EndArea();

                        if (m_skipingPartOfTutorial == true) //end tutorial
                        {
                            if (Event.current.type == EventType.Repaint)
                            {
                                Repaint();
                                m_skipingPartOfTutorial = false;

                                board_BoardBase.RemoveTutorial();
                                m_tutorialProgress++;
                            }
                        }

                        m_tutorialCompleted = true;

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

        #region custom statuses
        private string ParseStatus(string text) //where you can add custom short hand statuses
        {
            text = text.Replace("<STAT=0>", "to be done"); //im sure you know how this works
            text = text.Replace("<STAT=1>", "in progress");
            text = text.Replace("<STAT=2>", "finished");
            text = text.Replace("<STAT=3>", "URGENT");
            text = text.Replace("<STAT=4>", "failed to meet deadline");
            text = text.Replace("<STAT=5>", "NOT to be done");

            text = text.Replace("<STAT=999>", "owo uwu this be an ester egg");

            return text;
        }
        #endregion

        #region drag
        private void RunObjectSensor(Vector2 sensorLocation) //find an object being draged and if there is one put it in the mouses hand
        {
            int sensorIndex = -1;

            for (int i = 0; i < m_objectSensorRects.Count; i++) //loop thougth all dragable objects
            {          
                if (m_objectSensorRects[i].Contains(sensorLocation) == true) //the mouse is inside of one
                {
                    sensorIndex = i;

                    if (m_objectSensorResults[i].y >= 0)
                    {
                        break; //end loop here we do not need to do any more
                    }
                }
            }

            if (sensorIndex >= 0)
            {
                GetObjectUnderSensor(sensorIndex, m_objectSensorResults[sensorIndex]);
            }
            else
            {
                GetObjectUnderSensor(sensorIndex, new Vector2(-1, -1)); //unable to find object under sensor
            }
        }

        private void GetObjectUnderSensor(int index, Vector2 location) //puts the object in the mouses hand
        {
            if (Event.current.type == EventType.MouseDown && m_objectCurrentlyInDrag.x < 0) //the sensor is the mouse
            {
                m_objectCurrentlyInDrag = new Vector2((int)location.x, (int)location.y);
            }

            if (Event.current.GetTypeForControl(GUIUtility.hotControl) == EventType.MouseUp || Event.current.GetTypeForControl(GUIUtility.hotControl) == EventType.Ignore)
            {
                if (location.x >= 0) //is it valid //every valid location will have an index of 0 or above
                {
                    if (m_objectCurrentlyInDrag.x >= 0) //is what we are holding valid
                    {
                        if (m_objectCurrentlyInDrag.y >= 0) //is it a node //stack do not have a y value above 0
                        {
                            if (m_stackExpanded[(int)location.x] == true) //we should only be able drag node to and from expanded stacks
                            {
                                if (location.y >= 0) //we are moing it to a whole new stack
                                {
                                    board_BoardBase.MoveNodeToOtherStackAt((int)m_objectCurrentlyInDrag.x, (int)m_objectCurrentlyInDrag.y, (int)location.x, (int)location.y);
                                    UpdateSearch();
                                }
                                else //we are moving it do a diffrent point in the same stack
                                {
                                    board_BoardBase.MoveNodeToOtherStack((int)m_objectCurrentlyInDrag.x, (int)m_objectCurrentlyInDrag.y, (int)location.x);
                                    UpdateSearch();
                                }
                            }
                        }
                        else //we are moving a stack
                        {
                            MoveStackUIDataTo((int)m_objectCurrentlyInDrag.x, (int)location.x);
                            board_BoardBase.MoveStackToOtherStackAt((int)m_objectCurrentlyInDrag.x, (int)location.x);
                            UpdateSearch();
                        }
                    } 
                }

                 m_objectCurrentlyInDrag = new Vector2(-1, -1);
            }
        }
        #endregion

        #region search
        private void UpdateSearch()
        {
            m_objectSearchResults.Clear(); //clear old search results

            if (m_searchString == "") //because it is faster
            {
                return;
            }

            for (int x = 0; x < board_BoardBase.m_childStacks.Count; x++) //search the stacks
            {
                bool wasStackAdded = false;
                string stackText = board_BoardBase.m_childStacks[x].m_title;

                if (stackText.Contains(m_searchString.ToLower()) == true || stackText.Contains(m_searchString.ToUpper()) || stackText.Contains(m_searchString) == true) //a stack was found with the required text
                {
                    wasStackAdded = true;
                    m_objectSearchResults.Add(new Vector2(x, -1));
                }

                for (int y = 0; y < board_BoardBase.m_childStacks[x].m_childNodes.Count; y++) //searches the nodes
                {
                    string nodeText = board_BoardBase.m_childStacks[x].m_childNodes[y].m_title;

                    if (nodeText.ToLower().Contains(m_searchString) == true || nodeText.ToUpper().Contains(m_searchString) == true || nodeText.Contains(m_searchString) == true) //a node was found with the required text
                    {
                        m_objectSearchResults.Add(new Vector2(x, y));

                        if (wasStackAdded == false) //so the stack its in is added to the search result that way we can see the node results
                        {
                            wasStackAdded = true; //stops us from adding it more than once
                            m_objectSearchResults.Add(new Vector2(x, -1));
                        }
                    }

                }
            }

            if (m_searchString != "")
            {
                m_objectSearchResults.Add(new Vector2(-1, -1));
            }
        }
        #endregion

        private bool MoveStackUIDataTo(int A, int B) //moves the stack expanded and scrollbar data from one stack slot to another
        {
            bool tempAbool = m_stackExpanded[A]; //stores the stack extra UI data we are moving
            Vector2 tempAVector = m_stackScrollOffsets[A]; //stores the stack extra UI data we are moving

            m_stackExpanded.RemoveAt(A); //removes the data from the ui
            m_stackScrollOffsets.RemoveAt(A); //removes the data from the ui

            m_stackExpanded.Add(true); //adds more space for us to move stuff to
            m_stackScrollOffsets.Add(Vector2.zero);

            for (int i = board_BoardBase.m_childStacks.Count - 1; i > B; i--) //begin making space for the new location of the stack
            {
                m_stackExpanded[i] = m_stackExpanded[i - 1];
                m_stackScrollOffsets[i] = m_stackScrollOffsets[i - 1];
            }

            m_stackExpanded[B] = tempAbool; //move our UI data back onto the board but now in a new place
            m_stackScrollOffsets[B] = tempAVector;

            return m_stackExpanded[A]; //should the bord in our old pace be expanded or not
        }

        private void ManualSave()
        {
            SaveStackExpandedStatus();

            if (m_tutorialCompleted == true) //so the tutorial can come to an end
            {
                board_BoardBase.RemoveTutorial();
            }

            board_BoardBase.SaveBoard();
        }

        #region saving and loading local board meta data for UI
        private void UpdateStackStatusAndScrollbars() //mostly gets rid of junk data for stacks envolving the expanded and scroll bar data
        {
            if (Event.current.type == EventType.Repaint && m_oldStackCountForRemovalCheck != board_BoardBase.m_childStacks.Count) //the amout of stacks on the bord changed
            {
                if (board_BoardBase.m_childStacks.Count < m_oldStackCountForRemovalCheck) //something was removed
                {
                    if (m_lastStackToBeEditited >= 0 && m_lastStackToBeEditited < board_BoardBase.m_childStacks.Count) //do we know what was removed
                    {
                        m_stackExpanded.RemoveAt(m_lastStackToBeEditited); //removed the UI data for it
                        m_stackScrollOffsets.RemoveAt(m_lastStackToBeEditited);
                    }

                    m_lastStackToBeEditited = -1;
                }

                if (m_stackExpanded.Count > board_BoardBase.m_childStacks.Count) //as a saft mesure
                {
                    for (int i = m_stackExpanded.Count; i < board_BoardBase.m_childStacks.Count; i++) //this list is bigger than it should be
                    {
                        m_stackExpanded.RemoveAt(i); //remover items from end till size is small enougth
                    }
                }

                if (m_stackScrollOffsets.Count > board_BoardBase.m_childStacks.Count)
                {
                    for (int i = m_stackScrollOffsets.Count; i < board_BoardBase.m_childStacks.Count; i++)
                    {
                        m_stackScrollOffsets.RemoveAt(i);
                    }
                }

                m_oldStackCountForRemovalCheck = board_BoardBase.m_childStacks.Count;
            }
        }

        private void SaveStackExpandedStatus()
        {
            string saveLocation = hud_UnnioSaveEditor.GetMetaSaveLocationLocalToBoard() + "\\stackExpanded.pref"; //the path to a file containing the last saved board
            string saveData = "";

            for (int i = 0; i < board_BoardBase.m_childStacks.Count; i++) //we do not loop thougth m_stackExpanded just encase theres more data there than there shold be ... abd we dont want to save that
            {
                if (m_stackExpanded[i] == true) //a simple but elegent way to save all this
                {
                    saveData += "1"; //is open
                }
                else
                {
                    saveData += "0"; //is closed
                }
            }

            new FileInfo(saveLocation).Directory.Create(); //ensure we can actually save stuff
            File.WriteAllText(saveLocation, saveData); //save it
        }

        private void LoadStackExpandedStatus()
        {
            string saveLocation = hud_UnnioSaveEditor.GetMetaSaveLocationLocalToBoard() + "\\stackExpanded.pref"; //save it to our meta area

            if (File.Exists(saveLocation) == true)
            {
                string saveData = File.ReadAllText(saveLocation);

                for (int i = 0; i < Math.Min(board_BoardBase.m_childStacks.Count, saveData.Length); i++) //loop thougth all stacks avalible
                {
                    if (m_stackExpanded.Count <= i) //so we dont go off the list 
                    {
                        m_stackExpanded.Add(true);
                    }

                    m_stackExpanded[i] = (saveData[i] == '1'); //was it expanded before
                }
            }
        }
        #endregion
    }
}

#endif