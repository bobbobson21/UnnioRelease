#if UNITY_EDITOR

using System;
using UnityEditor;

namespace EditorPackages.Unnio
{
    class menu_NodeMoving
    {
        [MenuItem("Window/Unnio/Move all nodes for me to last stack")] //since this handles automatic moving I thougth it was best to put this here
        public static void CompleteAllForMe()
        {
            for (int i = 0; i < board_BoardBase.m_childStacks.Count - 1; i++) //loop thougth all stacks
            {
                for (int o = 0; o < board_BoardBase.m_childStacks[i].m_childNodes.Count; o++) //loop thougth all nodes in thoese stacks
                {
                    if (board_BoardBase.m_childStacks[i].m_childNodes[o].m_user == Environment.UserName) //is it ours
                    {
                        board_BoardBase.MoveNodeToOtherStack(i, o, board_BoardBase.m_childStacks.Count - 1); //move it to last

                        o--;
                        if (o < -1) { o = -1; }
                    }
                }
            }
        }

        [MenuItem("Window/Unnio/Move all nodes for me to first stack")] //since this handles automatic moving I thougth it was best to put this here
        public static void StartAllForMe()
        {
            for (int i = 1; i < board_BoardBase.m_childStacks.Count; i++) //loop thougth all stacks
            {
                for (int o = 0; o < board_BoardBase.m_childStacks[i].m_childNodes.Count; o++)  //loop thougth all nodes in thoese stacks
                {
                    if (board_BoardBase.m_childStacks[i].m_childNodes[o].m_user == Environment.UserName) //is it ours
                    {
                        board_BoardBase.MoveNodeToOtherStack(i, o, 0); //move it to first

                        o--;
                        if (o < -1) { o = -1; }
                    }
                }
            }
        }
    }
}

#endif