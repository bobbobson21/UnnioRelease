#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json.Linq;
using System.IO;
using static Codice.CM.WorkspaceServer.DataStore.WkTree.WriteWorkspaceTree;
using System.Collections;

namespace EditorPackages.Unnio
{
    [InitializeOnLoad]

    //this is the class that is responcible for containg and managing the whole thing
    public class board_BoardBase //since nodes exsit inside of stacks which exsits inside of the board there is no point decoupling thoese classes in my opioion
    {
        #region private vars: editor interaction details
        private static int m_insertedStackIndex = -1;
        private static int m_insertedNodeIndex = -1;
        #endregion

        #region private vars: current open board
        private static string m_filepathToCurrentBoard = Application.dataPath + "/Editor/MainUnnioBoard.unnboa"; //the default save area
        #endregion

        #region private vars: stack & node removal
        private static List<Vector2> m_discardList = new List<Vector2>();
        private static bool m_discardIsAuthorised = true;
        #endregion


        #region public vars: current version
        public static string m_unnioVersion { private set; get; } = "3.3.1";
        #endregion

        #region public vars: current board data
        public static List<stack_StackBase> m_childStacks = new List<stack_StackBase>();
        private static bool m_isTutorialBoard = false;
        #endregion

        #region  public vars: events
        public delegate void StackMovement(int oldLocation, int newLocation);
        public static event StackMovement e_stackMovementEvent;

        public delegate void NodeMovement(int oldStackLocation, int oldNodeLocation, int newStackLocation, int newNodeLocation);
        public static event NodeMovement e_nodeMovementEvent;

        public delegate void OnBoardLoadOrSave(string filePath);
        public static event OnBoardLoadOrSave e_onBoardLoad;
        public static event OnBoardLoadOrSave e_onBoardSave;
        #endregion


        #region events
        static board_BoardBase()
        {
            EditorApplication.update += Update;
            EditorApplication.quitting += Exit;
            hud_UnnioNodeEditor.e_onClosed += OnNodeEditorClose;
            hud_UnnioStackEditor.e_onClosed += OnStackEditorClosed;
        }


        private static void Exit() //dose the autor save and event unsubscription
        {
            EditorApplication.update -= Update;
            EditorApplication.quitting -= Exit;
            hud_UnnioNodeEditor.e_onClosed -= OnNodeEditorClose;
            hud_UnnioStackEditor.e_onClosed -= OnStackEditorClosed;

            SaveBoard();
        }

        private static void Update()
        {
            if (m_discardIsAuthorised == true) //a safery way of removing nodes and stacks
            {
                for (int i = 0; i < m_discardList.Count; i++)
                {
                    try
                    {
                        if (m_discardList[i].y < 0) //it is a stack
                        {
                            m_childStacks.RemoveAt((int)m_discardList[i].x);
                        }
                        else //it is a node
                        {
                            m_childStacks[(int)m_discardList[i].x].m_childNodes.RemoveAt((int)m_discardList[i].y);
                        }
                    }
                    catch
                    {
                    }
                }

                m_discardList.Clear();
            }
        }

        private static void OnStackEditorClosed(stack_StackBase stack, bool isOld)
        {
            if (stack == null && isOld == true)
            {
                RemoveStack(m_insertedStackIndex);
            }

            if (stack != null && isOld == false)
            {
                m_childStacks.Add(stack);
                if (e_stackMovementEvent != null) { e_stackMovementEvent(-1, m_childStacks.Count - 1); }
            }

            m_insertedStackIndex = -1;
            m_insertedNodeIndex = -1;
        }

        private static void OnNodeEditorClose(node_NodeBase node, bool isOld)
        {
            if (node == null && isOld == true)
            {
                RemoveNode(m_insertedStackIndex, m_insertedNodeIndex);
            }

            if (node != null && isOld == false)
            {
                m_childStacks[m_insertedStackIndex].m_childNodes.Add(node);
                if (e_nodeMovementEvent != null) { e_nodeMovementEvent(-1, -1, m_insertedStackIndex, m_childStacks[m_insertedStackIndex].m_childNodes.Count - 1); }
            }


            m_insertedStackIndex = -1;
            m_insertedNodeIndex = -1;
        }
        #endregion

        #region delete, load & save board
        public static void DeleteBoard()
        {
            string metaString = ".meta";

            m_childStacks.Clear(); //clears out the stacks

            if (File.Exists(m_filepathToCurrentBoard)) //deletes the file
            {
                File.Delete(m_filepathToCurrentBoard);
            }

            if (File.Exists(m_filepathToCurrentBoard + metaString))
            {
                File.Delete(m_filepathToCurrentBoard + metaString);
            }
        }

        public static void DeleteBoard(string path)
        {
            m_filepathToCurrentBoard = path;

            DeleteBoard();
        }

        public static void SaveBoard()
        {
            JObject boardSaveData = new JObject();
            JArray stacksToSave = new JArray();

            for (int i = 0; i < m_childStacks.Count; i++) //loops thougth all stacks
            {
                stacksToSave.Add(m_childStacks[i].ConvertToJson()); //should save eveything in the stacks as well such as nodes colors ect.
            }

            if (m_isTutorialBoard == true)
            {
                boardSaveData.Add("IS_UNNIO_BUILT_IN_TUTORIAL", true);
            }

            boardSaveData.Add("IS_UNNIO_SAVE", true); //so we can open the files dirrectly
            boardSaveData.Add("STACKS", stacksToSave); //stores them in stack array

            string fileData = boardSaveData.ToString();

            try
            {
                new FileInfo(m_filepathToCurrentBoard).Directory.Create(); //ensure we can actually save stuff
                File.WriteAllText(m_filepathToCurrentBoard, fileData);

                if (e_onBoardSave != null)
                {
                    e_onBoardSave(m_filepathToCurrentBoard);
                }

                if (m_filepathToCurrentBoard.Contains(Application.dataPath) == true) //stop unity from throwing a hissy fit if it finds file changes it dosent know about
                {
                    AssetDatabase.ImportAsset(m_filepathToCurrentBoard.Replace(Application.dataPath, "Assets"));
                }
            }
            catch (FileLoadException)
            {
                //do nothing
            }
        }

        public static void SaveBoard(string path)
        {
            m_filepathToCurrentBoard = path;
            
            SaveBoard();
        }

        public static void LoadBoard()
        {
            m_isTutorialBoard = false;

            if (File.Exists(m_filepathToCurrentBoard) == true)
            {
                m_childStacks.Clear();// so it keep the old stacks

                try
                {
                    string fileDataRaw = File.ReadAllText(m_filepathToCurrentBoard);
                    JObject fileData = (JObject)JConstructor.Parse(fileDataRaw);

                    if (e_onBoardLoad != null)
                    {
                        e_onBoardLoad(m_filepathToCurrentBoard);
                    }

                    if (fileDataRaw.Contains("\"IS_UNNIO_BUILT_IN_TUTORIAL\": true") == true) //it has tutorial
                    {
                        m_isTutorialBoard = true;
                    }

                    for (int i = 0; i < ((JArray)fileData["STACKS"]).Count; i++) //loops thougth all stacks
                    {
                        m_childStacks.Add(new stack_StackBase().ConvertFromJson((JObject)((JArray)fileData["STACKS"])[i]));
                    }
                }
                catch (FileLoadException)
                {
                    Debug.LogError("Unnio: failed to load board file, file may be in use or it may have been corrupted by the unity asset database");
                }

            }
        }
        
        public static void LoadBoard(string path)
        {
            m_filepathToCurrentBoard = path;

            LoadBoard();
        }

        public static string GetSaveLocation() //get location of current board file
        {
            return m_filepathToCurrentBoard;
        }
        #endregion

        #region stack & node moving
        public static void SwapStacksWithInBoard(int A, int B)
        {
            stack_StackBase tempA = m_childStacks[A];

            if (e_stackMovementEvent != null) { e_stackMovementEvent(B, A); }
            if (e_stackMovementEvent != null) { e_stackMovementEvent(A, B); }

            m_childStacks[A] = m_childStacks[B];
            m_childStacks[B] = tempA;
        }

        public static void MoveStackToOtherStackAt(int currentStackLocation, int newStackLocation)
        {
            stack_StackBase tempA = m_childStacks[currentStackLocation]; //stores the stack we are moving
            m_childStacks.RemoveAt(currentStackLocation); //removes it from the board

            m_childStacks.Add(null); //expand to make room

            for (int i = m_childStacks.Count - 1; i > newStackLocation; i--) //move everything up
            {
                if (e_stackMovementEvent != null) { e_stackMovementEvent(i - 1, i); } //inform of stack movement

                m_childStacks[i] = m_childStacks[i - 1];
            }

            m_childStacks[newStackLocation] = tempA; //assignes tempA to new spot

            if (e_stackMovementEvent != null) { e_stackMovementEvent(currentStackLocation, newStackLocation); }
        }

        public static void MoveNodeToOtherStackAt(int currentStackLocation, int currentNodeIndex, int newStackLocation, int newNodeLocation)
        {
            node_NodeBase tempA = m_childStacks[currentStackLocation].m_childNodes[currentNodeIndex]; //stores the node we are moving
            m_childStacks[currentStackLocation].m_childNodes.RemoveAt(currentNodeIndex); //removes it from the stack it is in

            m_childStacks[newStackLocation].m_childNodes.Add(null);

            for (int i = m_childStacks[newStackLocation].m_childNodes.Count -1; i > newNodeLocation; i--)
            {
                if (e_nodeMovementEvent != null) { e_nodeMovementEvent(newStackLocation, i - 1, newStackLocation, i); } //inform of node movement

                m_childStacks[newStackLocation].m_childNodes[i] = m_childStacks[newStackLocation].m_childNodes[i -1];
            }

            m_childStacks[newStackLocation].m_childNodes[newNodeLocation] = tempA;

            if (e_nodeMovementEvent != null) { e_nodeMovementEvent(currentStackLocation, currentNodeIndex, newStackLocation, newNodeLocation); }
        }
        
        public static void SwapNodesWithinStack(int stackSelected, int A, int B)
        {
            node_NodeBase tempA = m_childStacks[stackSelected].m_childNodes[A];

            if (e_nodeMovementEvent != null) { e_nodeMovementEvent(stackSelected, B, stackSelected, A); } //inform of node movement
            if (e_nodeMovementEvent != null) { e_nodeMovementEvent(stackSelected, A, stackSelected, B); } //inform of node movement

            m_childStacks[stackSelected].m_childNodes[A] = m_childStacks[stackSelected].m_childNodes[B];
            m_childStacks[stackSelected].m_childNodes[B] = tempA;
        }

        public static void MoveNodeToOtherStack(int currentStackLocation, int currentNodeIndex, int newStackLocation)
        {
            node_NodeBase tempA = m_childStacks[currentStackLocation].m_childNodes[currentNodeIndex]; //stores the node we are moving
            m_childStacks[currentStackLocation].m_childNodes.RemoveAt(currentNodeIndex); //removes it from the stack it is in

            m_childStacks[newStackLocation].m_childNodes.Add(tempA); //adds it to the new stack

            if (e_nodeMovementEvent != null) { e_nodeMovementEvent(currentStackLocation, currentNodeIndex, newStackLocation, m_childStacks[newStackLocation].m_childNodes.Count -1); }
        }

        public static void MoveNodeToLast(int stackSelected, int nodeSelected)
        {
            for (int i = nodeSelected +1; i < m_childStacks[stackSelected].m_childNodes.Count; i++) //moving nodes baby
            {
                if (e_nodeMovementEvent != null) { e_nodeMovementEvent(stackSelected, i, stackSelected, i -1); }
            }

            if (e_nodeMovementEvent != null) { e_nodeMovementEvent(stackSelected, nodeSelected, stackSelected, m_childStacks[nodeSelected].m_childNodes.Count - 1); } //mode node

            m_childStacks[stackSelected].m_childNodes.Add(m_childStacks[stackSelected].m_childNodes[nodeSelected]); //add item to last
            m_childStacks[stackSelected].m_childNodes.RemoveAt(nodeSelected); //delete where it was before
        }

        public static void MoveNodeToFirst(int stackSelected, int nodeSelected)
        {
            m_childStacks[stackSelected].m_childNodes.Add(null); //adds an item to last so we can move everything up
            for (int i = m_childStacks[stackSelected].m_childNodes.Count -2; i >= 0; i++) //moves eveyting up
            {
                if (e_nodeMovementEvent != null) { e_nodeMovementEvent(stackSelected, i, stackSelected, i + 1); } //move node action

                m_childStacks[stackSelected].m_childNodes[i + 1] = m_childStacks[stackSelected].m_childNodes[i];
            }

            if (e_nodeMovementEvent != null) { e_nodeMovementEvent(stackSelected, nodeSelected + 1, stackSelected, 0); }  //move node action

            m_childStacks[stackSelected].m_childNodes[0] = m_childStacks[stackSelected].m_childNodes[nodeSelected + 1]; //set at first
            m_childStacks[stackSelected].m_childNodes.RemoveAt(nodeSelected + 1); //delete where it was before because it now at first
        }
        #endregion

        #region stack & node editing/adding
        public static void EditStack(int stackIndex) //edits stack via stack editor
        {
            if (m_insertedStackIndex >= 0) { return; } //stoped bugs if you have both editors open
            if (hud_UnnioStackEditor.OpenWindowWithStack(m_childStacks[stackIndex]) == false) { return; } //something went wrong

            m_insertedStackIndex = stackIndex;
            m_insertedNodeIndex = -1;
        }

        public static void AddStack() //adds a stack via stack editor
        {
            if (m_insertedStackIndex >= 0) { return; } //stoped bugs if you have both editors open

            m_insertedStackIndex = int.MaxValue;
            m_insertedNodeIndex = - 1;

            if (hud_UnnioStackEditor.OpenWindow() == false)
            {
                m_insertedStackIndex = -1;
                m_insertedNodeIndex = -1;
            }
        }

        public static void EditNode(int stackIndex, int nodeIndex) //edits the node thougth the node editor
        {
            if (m_insertedStackIndex >= 0) { return; } //stoped bugs if you have both editors open
            if (hud_UnnioNodeEditor.OpenWindowWithNode(m_childStacks[stackIndex].m_childNodes[nodeIndex]) == false) { return; } //something went wrong

            m_insertedStackIndex = stackIndex;
            m_insertedNodeIndex = nodeIndex;
        }

        public static void AddNode(int stackIndex) //adds the node thougth the node editor
        {
            if (m_insertedStackIndex >= 0) { return; } //stoped bugs if you have both editors open

            m_insertedStackIndex = stackIndex;
            m_insertedNodeIndex = int.MaxValue;

            if (hud_UnnioNodeEditor.OpenWindow() == false)
            {
                m_insertedStackIndex = -1;
                m_insertedNodeIndex = -1;
            }
        }
        #endregion

        #region stack & node removal
        public static void RemoveStack(int stack) //marks a stack for removal
        {
            m_discardList.Add(new Vector2(stack, -1));
        }

        public static void RemoveNode(int stack, int node) //marks a node for removal
        {
            m_discardList.Add(new Vector2(stack, node));
        }

        public static void RemovalAuthorization(bool isAutorized) //can removal continue
        {
            m_discardIsAuthorised = isAutorized;
        }
        #endregion

        #region tutorial data
        public static void RemoveTutorial() //remove built in tutorial
        {
            m_isTutorialBoard = false;
        }

        public static void AddTutorial() //adds built in tutorial
        {
            m_isTutorialBoard = true;
        }

        public static bool IsTutorialBoard()
        {
            return m_isTutorialBoard;
        }
        #endregion
    }
}

#endif