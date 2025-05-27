#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;
using MacFsWatcher;

namespace EditorPackages.Unnio
{
    [InitializeOnLoad]

    public class node_Oparator //makes the conditions in the nodes work
    {
        #region timer data
        private static float m_maxTimeBeforeNextCodeFileChecks = 6;
        private static float m_timeBeforeNextCodeFileChecks = 0;
        #endregion

        #region event subscription
        static node_Oparator()
        {
            EditorApplication.update += Update;
            EditorApplication.quitting += Exit;
        }
        private static void Exit()
        {
            EditorApplication.update -= Update;
            EditorApplication.quitting -= Exit;
        }
        #endregion

        public static bool IsValidCondition(node_NodeCondition cond) //if you think this is long just wait till we get to stack actions
        {
            int intOutStub;
            DateTime dateOutStub;

            switch (cond.m_conditionType)
            {
                case node_NodeConditionTypes.None:
                    return true;

                case node_NodeConditionTypes.AssetFileCreated:
                case node_NodeConditionTypes.SceneCreated: //technically it would be valid with these charters it would just do nothing tho
                    if (cond.m_testAgainstString.Contains("*") == true) { return false; } //valid files dont have these charters
                    if (cond.m_testAgainstString.Contains('"') == true) { return false; }
                    if (cond.m_testAgainstString.Contains("/") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("\\") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("<") == true) { return false; }
                    if (cond.m_testAgainstString.Contains(">") == true) { return false; }
                    if (cond.m_testAgainstString.Contains(":") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("|") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("?") == true) { return false; }
                    if (cond.m_testAgainstString == "") { return false; }
                    return true;

                case node_NodeConditionTypes.FunctionCreated: 
                    if (cond.m_testAgainstString.Contains("!") == true) { return false; } //valid data dont have these charters
                    if (cond.m_testAgainstString.Contains('"') == true) { return false; }
                    if (cond.m_testAgainstString.Contains("£") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("$") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("%") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("^") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("&") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("*") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("-") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("+") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("`") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("¬") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("[") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("{") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("]") == true) { return false; } 
                    if (cond.m_testAgainstString.Contains("}") == true) { return false; }
                    if (cond.m_testAgainstString.Contains(",") == true) { return false; }
                    if (cond.m_testAgainstString.Contains(".") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("<") == true) { return false; }
                    if (cond.m_testAgainstString.Contains(">") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("/") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("?") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("\\") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("|") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("#") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("~") == true) { return false; }
                    if (cond.m_testAgainstString == "") { return false; }

                    if (cond.m_testAgainstString.Contains("(") == false) { return false; }
                    if (cond.m_testAgainstString.Substring(cond.m_testAgainstString.Length-1) != ")") { return false; }
                    return true;

                case node_NodeConditionTypes.FunctionCreatedInClass:
                case node_NodeConditionTypes.VariableCreatedInClass:
                    if (cond.m_testAgainstString.Contains("!") == true) { return false; } //valid data dont have these charters
                    if (cond.m_testAgainstString.Contains('"') == true) { return false; }
                    if (cond.m_testAgainstString.Contains("£") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("$") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("%") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("^") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("&") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("*") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("+") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("`") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("¬") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("[") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("{") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("]") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("}") == true) { return false; }
                    if (cond.m_testAgainstString.Contains(",") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("<") == true) { return false; }
                    if (cond.m_testAgainstString.Contains(">") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("/") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("?") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("\\") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("|") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("#") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("~") == true) { return false; }
                    if (cond.m_testAgainstString == "") { return false; }

                    if (cond.m_conditionType == node_NodeConditionTypes.FunctionCreatedInClass)
                    {
                        if (cond.m_testAgainstString.Contains("(") == false) { return false; }
                        if (cond.m_testAgainstString.Substring(cond.m_testAgainstString.Length - 1) != ")") { return false; }
                    }

                    return true;

                case node_NodeConditionTypes.NamespaceCreated:
                    if (cond.m_testAgainstString.Contains("!") == true) { return false; } //valid data dont have these charters
                    if (cond.m_testAgainstString.Contains('"') == true) { return false; }
                    if (cond.m_testAgainstString.Contains("£") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("$") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("%") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("^") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("&") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("*") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("-") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("+") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("`") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("¬") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("[") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("{") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("]") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("}") == true) { return false; }
                    if (cond.m_testAgainstString.Contains(",") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("<") == true) { return false; }
                    if (cond.m_testAgainstString.Contains(">") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("/") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("?") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("\\") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("|") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("#") == true) { return false; }
                    if (cond.m_testAgainstString.Contains("~") == true) { return false; }
                    return true;


                case node_NodeConditionTypes.SceneCountMoreThan:
                    return int.TryParse(cond.m_testAgainstString, out intOutStub);

                case node_NodeConditionTypes.DateMoreThan:
                    return DateTime.TryParse(cond.m_testAgainstString, out dateOutStub);
            }

            return false;
        }

        #region condition exacution
        private static void Update()
        {
            m_timeBeforeNextCodeFileChecks -= Time.deltaTime;

            for (int i = 0; i < board_BoardBase.m_childStacks.Count; i++) //loop thougth stacks
            {
                int removalSubtractOffset = 0;

                for (int o = 0; o < board_BoardBase.m_childStacks[i].m_childNodes.Count; o++) //loop thougth all node
                {
                    if (hud_UnnioNodeEditor.IsThisNodeInEditor(board_BoardBase.m_childStacks[i].m_childNodes[o]) == false && board_BoardBase.m_childStacks[i].m_childNodes[o] != null) //stops issues
                    {
                        List<node_NodeCondition> nodeConditions = board_BoardBase.m_childStacks[i].m_childNodes[o].m_nodeConditions;

                        if (nodeConditions != null)
                        {
                            for (int p = 0; p < nodeConditions.Count; p++) //loop thougth all conditions
                            {
                                if (CheckCondition(nodeConditions[p]) == true && nodeConditions[p] != null)
                                {
                                    removalSubtractOffset = PassCondition(i, o, p, removalSubtractOffset);
                                    p = Math.Max(p-1, 0);

                                    if (board_BoardBase.m_childStacks[i].m_childNodes.Count <= 0 || nodeConditions.Count <= 0) //we dont really care if returning like this skips over a few things they will be delt with on the next update
                                    {
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (m_timeBeforeNextCodeFileChecks <= 0)
            {
                m_timeBeforeNextCodeFileChecks = m_maxTimeBeforeNextCodeFileChecks;
            }
        }

        private static bool CheckCondition(node_NodeCondition cond)
        {
            int intOutValue;
            DateTime dateOutValue;
            bool dontExit = false;

            string[] codeFileArray = GetCodeFiles();

            switch (cond.m_conditionType) //check the condition
            {
                case node_NodeConditionTypes.None:
                    break;
                case node_NodeConditionTypes.AssetFileCreated:
                    if (DoseFileExist(cond.m_testAgainstString) == true)
                    {
                        return true;
                    }
                    break;

                case node_NodeConditionTypes.SceneCreated:
                    if (DoesSceneExist(cond.m_testAgainstString) == true)
                    {
                        return true;
                    }
                    break;

                case node_NodeConditionTypes.FunctionCreated:
                    if (IsValidCondition(cond) == false) //make sure it valid before hand
                    {
                        return false;
                    }

                    if (m_timeBeforeNextCodeFileChecks <= 0) //we have the dlay so the files arent in concent use thus becoing hard to delete
                    {
                        for (int i = 0; i < codeFileArray.Length; i++) //loop thougth all code files
                        {
                            try
                            {
                                if (File.ReadAllText(" " + codeFileArray[i]).Contains(cond.m_testAgainstString) == true && cond.m_testAgainstString.Contains("(") == true && cond.m_testAgainstString.Contains(")") == true) // dose the function exsit in this file
                                {
                                    return true; //yes
                                }
                            }
                            catch
                            {
                                //file not accessible right now
                            }
                        }
                    }
                    break;

                case node_NodeConditionTypes.FunctionCreatedInClass:
                    if (IsValidCondition(cond) == false)
                    {
                        return false;
                    }

                    if (m_timeBeforeNextCodeFileChecks <= 0)
                    {
                        for (int i = 0; i < codeFileArray.Length; i++)
                        {
                            try
                            {
                                string fileText = File.ReadAllText(codeFileArray[i]);

                                if (fileText.Contains(" " + cond.m_testAgainstString.Substring(cond.m_testAgainstString.LastIndexOf(".") + 1)) == true && cond.m_testAgainstString.Contains("(") == true && cond.m_testAgainstString.Contains(")") == true)
                                {
                                    if (fileText.Contains("class " + cond.m_testAgainstString.Substring(0, cond.m_testAgainstString.LastIndexOf(".") - 1)) == true) //is it the right class
                                    {
                                        return true;
                                    }
                                }
                            }
                            catch
                            {
                                //file not accessible right now
                            }
                        }
                    }
                    break;

                case node_NodeConditionTypes.VariableCreatedInClass:
                    if (IsValidCondition(cond) == false)
                    {
                        return false;
                    }

                    if (m_timeBeforeNextCodeFileChecks <= 0)
                    {
                        for (int i = 0; i < codeFileArray.Length; i++)
                        {
                            try
                            {
                                string fileText = File.ReadAllText(codeFileArray[i]);

                                if (fileText.Contains(" " + cond.m_testAgainstString.Substring(cond.m_testAgainstString.LastIndexOf(".") + 1)) == true)
                                {
                                    if (fileText.Contains("class " + cond.m_testAgainstString.Substring(0, cond.m_testAgainstString.LastIndexOf(".") - 1)) == true) //is it the right class
                                    {
                                        return true;
                                    }
                                }
                            }
                            catch
                            {
                                //file not accessible right now
                            }
                        }
                    }
                    break;

                case node_NodeConditionTypes.NamespaceCreated:
                    if (IsValidCondition(cond) == false)
                    {
                        return false;
                    }

                    if (m_timeBeforeNextCodeFileChecks <= 0)
                    {
                        for (int i = 0; i < codeFileArray.Length; i++)
                        {
                            try
                            {
                                if (File.ReadAllText(codeFileArray[i]).Contains("namespace " + cond.m_testAgainstString) == true)
                                {
                                    return true;
                                }
                            }
                            catch
                            {
                                //file not accessible right now
                            }
                        }
                    }
                    break;

                case node_NodeConditionTypes.SceneCountMoreThan:
                    dontExit = int.TryParse(cond.m_testAgainstString, out intOutValue);
                    int sceneCount = Directory.GetFiles(Application.dataPath, "*.unity", SearchOption.AllDirectories).Length;
                    if (sceneCount > intOutValue && dontExit == true)
                    {
                        return true;
                    }
                    break;

                case node_NodeConditionTypes.DateMoreThan:
                    dontExit = DateTime.TryParse(cond.m_testAgainstString, out dateOutValue);
                    if (DateTime.Today > dateOutValue && dontExit == true)
                    {
                        return true;
                    }
                    break;
                default:
                    break;
            }

            return false;
        }

        private static int PassCondition(int stackLocation, int nodeLocation, int conditionLocation, int removalOffsetIndex)
        {
            int newStackPos = board_BoardBase.m_childStacks[stackLocation].m_childNodes[nodeLocation].m_nodeConditions[conditionLocation].m_onTestPassMoveToStack;
            newStackPos = Math.Min(newStackPos, board_BoardBase.m_childStacks.Count -1); //so it cant go beyond the stack no matter how high of a number

            if (newStackPos >= 0) //if move than 0 move it if less than remove it
            {
                board_BoardBase.m_childStacks[stackLocation].m_childNodes[nodeLocation].m_nodeConditions.RemoveAt(conditionLocation);
                board_BoardBase.MoveNodeToOtherStack(stackLocation, nodeLocation, newStackPos);
            }
            else 
            {
                board_BoardBase.RemoveNode(stackLocation, Math.Max(nodeLocation + removalOffsetIndex,0));
                return removalOffsetIndex -1;
            }

            return 0;
        }
        #endregion

        #region asset checking
        private static bool DoesSceneExist(string scenePath)
        {
            string[] allFiles = Directory.GetFiles(Application.dataPath, "*.unity", SearchOption.AllDirectories);

            for (int i = 0; i < allFiles.Length; i++)
            {
                if (allFiles[i].Contains(scenePath + ".unity") == true)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool DoseFileExist(string fileName)
        {
            string[] allFiles = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories);
            for (int i = 0; i < allFiles.Length; i++) //loops thougth all files in project
            {
                string otherFileName = allFiles[i].Substring(allFiles[i].LastIndexOf("\\") +1); //gets the real file name and not the path
                if (otherFileName == fileName) //is it what we are looking for
                {
                    return true;
                }
            }

            return false;
        }

        private static string[] GetCodeFiles()
        {
            string[] allFiles = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);

            return allFiles;
        }
        #endregion
    }
}

#endif