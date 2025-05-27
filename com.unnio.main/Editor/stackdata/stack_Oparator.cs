#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System;

namespace EditorPackages.Unnio
{
    [InitializeOnLoad]
    public class stack_Oparator //makes the conditions in the nodes work and actions in the stacks as well
    {
        #region event subscription
        static stack_Oparator()
        {
            board_BoardBase.e_nodeMovementEvent += InformOparatorOfNodeMovement;
            EditorApplication.quitting += Exit;
        }

        private static void Exit()
        {
            board_BoardBase.e_nodeMovementEvent -= InformOparatorOfNodeMovement;
            EditorApplication.quitting -= Exit;
        }
        #endregion

        public static bool IsValidAction(stack_StackAction act) 
        {
            DateTime dateOutStub;

            switch (act.m_actionType)//all the action stuff should always be valid apart from date
            {
                case stack_StackActionTypes.SetNodeColorIfDateMoreThen:
                case stack_StackActionTypes.SetNodeColorIfDateLessThen:
                case stack_StackActionTypes.RemoveAllNodeConditionsIfDateMoreThen:
                case stack_StackActionTypes.RemoveAllNodeConditionsIfDateLessThen:
                case stack_StackActionTypes.RemoveNodeIfDateMoreThen:
                case stack_StackActionTypes.RemoveNodeIfDateLessThen:
                    return DateTime.TryParse(act.m_ifDate, out dateOutStub);
            }

            return true;
        }

        #region action exacution
        public static void InformOparatorOfNodeMovement(int oldStack, int oldNodeLocation, int newStack, int nodeLocation) //a node was moved
        {
            bool nodeExacutionSupended = false; //no more stack actions should be carried out on the node if this is true as it is a waste of processing

            if (oldStack != newStack)
            {
                if (oldStack == -1)
                {
                    for (int p = 0; p < board_BoardBase.m_childStacks[newStack].m_builtInActions.Count && nodeExacutionSupended == false; p++) //made in
                    {
                        nodeExacutionSupended = ExacuteActionOn(newStack, nodeLocation, board_BoardBase.m_childStacks[newStack].m_builtInActions[p]);
                    }
                }
                else
                {
                    for (int p = 0; p < board_BoardBase.m_childStacks[oldStack].m_exitActions.Count && nodeExacutionSupended == false; p++) //left stack
                    {
                        nodeExacutionSupended = ExacuteActionOn(newStack, nodeLocation, board_BoardBase.m_childStacks[oldStack].m_exitActions[p]);
                    }

                    for (int p = 0; p < board_BoardBase.m_childStacks[newStack].m_entryActions.Count && nodeExacutionSupended == false; p++) //entered stack
                    {
                        nodeExacutionSupended = ExacuteActionOn(newStack, nodeLocation, board_BoardBase.m_childStacks[newStack].m_entryActions[p]);
                    }
                }
            }
        }

        private static bool ExacuteActionOn(int stackIndex, int nodeIndex, stack_StackAction act) //dont forget to update
        {
            if (stackIndex >= board_BoardBase.m_childStacks.Count || nodeIndex >= board_BoardBase.m_childStacks[stackIndex].m_childNodes.Count)
            {
                return false;
            }

            DateTime dateOutValue;
            bool dontExit;

            node_NodeBase node = board_BoardBase.m_childStacks[stackIndex].m_childNodes[nodeIndex];

            switch (act.m_actionType) //oh boy this is going to be fun ... its not //and you will not be needing comments
            {
                case stack_StackActionTypes.None:
                    break;

                case stack_StackActionTypes.SetNodeColor:
                    if (act.m_thenColorIsTint == true) //then
                    {
                        node.m_color = node.m_color * act.m_thenColor;
                    }
                    else
                    {
                        node.m_color = act.m_thenColor;
                    }
                    break;

                case stack_StackActionTypes.SetNodeColorIfColor:
                    if (node.m_color == act.m_ifColor)  //if
                    {
                        if (act.m_thenColorIsTint == true) //then
                        {
                            node.m_color = node.m_color * act.m_thenColor;
                        }
                        else
                        {
                            node.m_color = act.m_thenColor;
                        }
                    }
                    break;

                case stack_StackActionTypes.SetNodeColorIfColorNot:
                    if (node.m_color != act.m_ifColor) //if
                    {
                        if (act.m_thenColorIsTint == true) //then
                        {
                            node.m_color = node.m_color * act.m_thenColor;
                        }
                        else
                        {
                            node.m_color = act.m_thenColor;
                        }
                    }
                    break;

                case stack_StackActionTypes.SetNodeColorIfStatusIs:
                    if (node.m_status == act.m_ifStatus) //if
                    {
                        if (act.m_thenColorIsTint == true) //then
                        {
                            node.m_color = node.m_color * act.m_thenColor;
                        }
                        else
                        {
                            node.m_color = act.m_thenColor;
                        }
                    }
                    break;

                case stack_StackActionTypes.SetNodeColorIfStatusIsNot:
                    if (node.m_status != act.m_ifStatus) //if
                    {
                        if (act.m_thenColorIsTint == true) //then
                        {
                            node.m_color = node.m_color * act.m_thenColor;
                        }
                        else
                        {
                            node.m_color = act.m_thenColor;
                        }
                    }
                    break;

                case stack_StackActionTypes.SetNodeColorIfDateMoreThen:
                    dontExit = DateTime.TryParse(act.m_ifDate, out dateOutValue);
                    if (DateTime.Today > dateOutValue && dontExit == true) //if
                    {
                        if (act.m_thenColorIsTint == true) //then
                        {
                            node.m_color = node.m_color * act.m_thenColor;
                        }
                        else
                        {
                            node.m_color = act.m_thenColor;
                        }
                    }
                    break;

                case stack_StackActionTypes.SetNodeColorIfDateLessThen:
                    dontExit = DateTime.TryParse(act.m_ifDate, out dateOutValue);
                    if (DateTime.Today < dateOutValue && dontExit == true) //if
                    {
                        if (act.m_thenColorIsTint == true) //then
                        {
                            node.m_color = node.m_color * act.m_thenColor;
                        }
                        else
                        {
                            node.m_color = act.m_thenColor;
                        }
                    }
                    break;

                case stack_StackActionTypes.RemoveAllNodeConditions:
                    node.m_nodeConditions.Clear(); //then
                    break;


                case stack_StackActionTypes.RemoveAllNodeConditionIfColor:
                    if (node.m_color == act.m_ifColor) //if
                    {
                        node.m_nodeConditions.Clear(); //then
                    }
                    break;

                case stack_StackActionTypes.RemoveAllNodeConditionIfColorNot:
                    if (node.m_color != act.m_ifColor) //if
                    {
                        node.m_nodeConditions.Clear(); //then
                    }
                    break;

                case stack_StackActionTypes.RemoveAllNodeConditionsIfStatusIs:
                    if (node.m_user == act.m_ifStatus) //if
                    {
                        node.m_nodeConditions.Clear(); //then
                    }
                    break;

                case stack_StackActionTypes.RemoveAllNodeConditionsIfStatusIsNot:
                    if (node.m_user != act.m_ifStatus) //if
                    {
                        node.m_nodeConditions.Clear(); //then
                    }
                    break;

                case stack_StackActionTypes.RemoveAllNodeConditionsIfDateMoreThen:
                    dontExit = DateTime.TryParse(act.m_ifDate, out dateOutValue);
                    if (DateTime.Today > dateOutValue && dontExit == true) //if
                    {
                        node.m_nodeConditions.Clear(); //then
                    }
                    break;

                case stack_StackActionTypes.RemoveAllNodeConditionsIfDateLessThen:
                    dontExit = DateTime.TryParse(act.m_ifDate, out dateOutValue);
                    if (DateTime.Today < dateOutValue && dontExit == true) //if
                    {
                        node.m_nodeConditions.Clear(); //then
                    }
                    break;

                case stack_StackActionTypes.RemoveNode:
                    board_BoardBase.RemoveNode(stackIndex, nodeIndex); //then
                    return true;

                case stack_StackActionTypes.RemoveNodeIfColor:
                    if (node.m_color == act.m_ifColor) //if
                    {
                        board_BoardBase.RemoveNode(stackIndex, nodeIndex); //then
                        return true;
                    }
                    break;

                case stack_StackActionTypes.RemoveNodeIfColorNot:
                    if (node.m_color != act.m_ifColor) //if
                    {
                        board_BoardBase.RemoveNode(stackIndex, nodeIndex); //then
                        return true;
                    }
                    break;

                case stack_StackActionTypes.RemoveNodeIfDateMoreThen:
                    dontExit = DateTime.TryParse(act.m_ifDate, out dateOutValue);
                    if (DateTime.Today > dateOutValue && dontExit == true) //if
                    {
                        board_BoardBase.RemoveNode(stackIndex, nodeIndex); //then
                        return true;
                    }
                    break;

                case stack_StackActionTypes.RemoveNodeIfDateLessThen:
                    dontExit = DateTime.TryParse(act.m_ifDate, out dateOutValue);
                    if (DateTime.Today < dateOutValue && dontExit == true) //if
                    {
                        board_BoardBase.RemoveNode(stackIndex, nodeIndex); //then
                        return true;
                    }
                    break;

                case stack_StackActionTypes.RemoveNodeIfStatusIs:
                    if (node.m_user == act.m_ifStatus) //if
                    {
                        board_BoardBase.RemoveNode(stackIndex, nodeIndex); //then
                        return true;
                    }
                    break;

                case stack_StackActionTypes.RemoveNodeIfStatusIsNot:
                    if (node.m_user != act.m_ifStatus) //if
                    {
                        board_BoardBase.RemoveNode(stackIndex, nodeIndex); //then
                        return true;
                    }
                    break;

                case stack_StackActionTypes.SetStatus:
                    board_BoardBase.m_childStacks[stackIndex].m_childNodes[nodeIndex].m_status = act.m_thenStatus; //then
                    break;

                case stack_StackActionTypes.SetStatusIfColor:
                    if (node.m_color == act.m_ifColor) //if
                    {
                        board_BoardBase.m_childStacks[stackIndex].m_childNodes[nodeIndex].m_status = act.m_thenStatus; //then
                    }
                    break;

                case stack_StackActionTypes.SetStatusIfColorNot:
                    if (node.m_color != act.m_ifColor) //if
                    {
                        board_BoardBase.m_childStacks[stackIndex].m_childNodes[nodeIndex].m_status = act.m_thenStatus; //then
                    }
                    break;

                case stack_StackActionTypes.SetStatusIfDateMoreThen:
                    dontExit = DateTime.TryParse(act.m_ifDate, out dateOutValue);
                    if (DateTime.Today > dateOutValue && dontExit == true) //if
                    {
                        board_BoardBase.m_childStacks[stackIndex].m_childNodes[nodeIndex].m_status = act.m_thenStatus; //then
                    }
                    break;

                case stack_StackActionTypes.SetStatusIfDateLessThen:
                    dontExit = DateTime.TryParse(act.m_ifDate, out dateOutValue);
                    if (DateTime.Today < dateOutValue && dontExit == true) //if
                    {
                        board_BoardBase.m_childStacks[stackIndex].m_childNodes[nodeIndex].m_status = act.m_thenStatus; //then
                    }
                    break;

                case stack_StackActionTypes.SetStatusIfStatusIs:
                    if (node.m_status == act.m_ifStatus) //if
                    {
                        board_BoardBase.m_childStacks[stackIndex].m_childNodes[nodeIndex].m_status = act.m_thenStatus; //then
                    }
                    break;

                case stack_StackActionTypes.SetStatusIfStatusIsNot:
                    if (node.m_status != act.m_ifStatus) //if
                    {
                        board_BoardBase.m_childStacks[stackIndex].m_childNodes[nodeIndex].m_status = act.m_thenStatus; //then
                    }
                    break;

                case stack_StackActionTypes.SetDeadline:
                    board_BoardBase.m_childStacks[stackIndex].m_childNodes[nodeIndex].m_deadline = act.m_thenDeadline; //then
                    break;

                case stack_StackActionTypes.SetDeadlineIfColor:
                    if (node.m_color == act.m_ifColor)  //if
                    {
                        board_BoardBase.m_childStacks[stackIndex].m_childNodes[nodeIndex].m_deadline = act.m_thenDeadline; //then
                    }
                    break;

                case stack_StackActionTypes.SetDeadlineIfColorNot:
                    if (node.m_color != act.m_ifColor)  //if
                    {
                        board_BoardBase.m_childStacks[stackIndex].m_childNodes[nodeIndex].m_deadline = act.m_thenDeadline; //then
                    }
                    break;

                case stack_StackActionTypes.SetDeadlineIfDateMoreThen:
                    dontExit = DateTime.TryParse(act.m_ifDate, out dateOutValue);
                    if (DateTime.Today > dateOutValue && dontExit == true) //if
                    {
                        board_BoardBase.m_childStacks[stackIndex].m_childNodes[nodeIndex].m_deadline = act.m_thenDeadline; //then
                    }
                    break;

                case stack_StackActionTypes.SetDeadlineIfDateLessThen:
                    dontExit = DateTime.TryParse(act.m_ifDate, out dateOutValue);
                    if (DateTime.Today > dateOutValue && dontExit == true) //if
                    {
                        board_BoardBase.m_childStacks[stackIndex].m_childNodes[nodeIndex].m_deadline = act.m_thenDeadline; //then
                    }
                    break;

                case stack_StackActionTypes.SetDeadlineIfStatusIs:
                    if (node.m_status == act.m_ifStatus) //if
                    {
                        board_BoardBase.m_childStacks[stackIndex].m_childNodes[nodeIndex].m_deadline = act.m_thenDeadline; //then
                    }
                    break;

                case stack_StackActionTypes.SetDeadlineIfStatusIsNot:
                    if (node.m_status == act.m_ifStatus) //if
                    {
                        board_BoardBase.m_childStacks[stackIndex].m_childNodes[nodeIndex].m_deadline = act.m_thenDeadline; //then
                    }
                    break;

                default:
                    break;
            }
            return false;
        }
        #endregion
    }
}

#endif