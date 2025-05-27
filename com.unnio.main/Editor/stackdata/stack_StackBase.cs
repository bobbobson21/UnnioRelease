#if UNITY_EDITOR

using UnityEngine;
using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json.Linq;

namespace EditorPackages.Unnio
{
    public class stack_StackBase
    {
        #region public vars: child node & action data
        public List<node_NodeBase> m_childNodes = new List<node_NodeBase>();

        public List<stack_StackAction> m_builtInActions = new List<stack_StackAction>();
        public List<stack_StackAction> m_entryActions = new List<stack_StackAction>();
        public List<stack_StackAction> m_exitActions = new List<stack_StackAction>();
        #endregion

        #region public vars: setting
        public string m_title = "... title here";
        public Color m_color = new Color(0, 0, 0, 0);
        #endregion


        #region json
        public JObject ConvertToJson()
        {
            JObject result = new JObject();
            result.Add("T", m_title); //saves our title

            JArray colArray = new JArray(); //saves our color
            colArray.Add(m_color.r);
            colArray.Add(m_color.g);
            colArray.Add(m_color.b);
            colArray.Add(m_color.a);

            result.Add("COL", colArray);

            JArray nodeArray = new JArray();

            for (int i = 0; i < m_childNodes.Count; i++) //saves all nodes
            {
                nodeArray.Add(m_childNodes[i].ConvertToJson());
            }

            result.Add("NODES", nodeArray);

            JObject actions = new JObject();

            JArray builtInActions = new JArray();
            JArray entryActions = new JArray();
            JArray exitActions = new JArray();

            for (int i = 0; i < m_builtInActions.Count; i++) //saves all the built in actions
            {
                builtInActions.Add(m_builtInActions[i].ConvertToJson());
            }

            for (int i = 0; i < m_entryActions.Count; i++) //saves all the entry actions
            {
                entryActions.Add(m_entryActions[i].ConvertToJson());
            }

            for (int i = 0; i < m_exitActions.Count; i++) //saves all the exit actions
            {
                exitActions.Add(m_exitActions[i].ConvertToJson());
            }

            actions.Add("ACT_BIA", builtInActions);
            actions.Add("ACT_ENA", entryActions);
            actions.Add("ACT_EXA", exitActions);

            result.Add("ACTIONS",actions);

            return result;
        }

        public stack_StackBase ConvertFromJson(JObject json)
        {
            try
            {
                m_title = (string)json["T"]; //load title

                if (m_title == null) { m_title = ""; }

                m_color.r = (float)((JArray)json["COL"])[0]; //load color
                m_color.g = (float)((JArray)json["COL"])[1];
                m_color.b = (float)((JArray)json["COL"])[2];
                m_color.a = (float)((JArray)json["COL"])[3];
            }
            catch
            {
                Debug.LogError("Unnio: failed to load stack from file correctly corruption has occoured or file is out of date");
            }

            try //just encase
            {
                for (int i = 0; i < ((JArray)json["ACTIONS"]["ACT_BIA"]).Count; i++) //load built in actions
                {
                    m_builtInActions.Add(new stack_StackAction().ConvertFromJson((JObject)((JArray)json["ACTIONS"]["ACT_BIA"])[i]));
                }

                for (int i = 0; i < ((JArray)json["ACTIONS"]["ACT_ENA"]).Count; i++) //load entry actions
                {
                    m_entryActions.Add(new stack_StackAction().ConvertFromJson((JObject)((JArray)json["ACTIONS"]["ACT_ENA"])[i]));
                }

                for (int i = 0; i < ((JArray)json["ACTIONS"]["ACT_EXA"]).Count; i++) //load exit actions
                {
                    m_exitActions.Add(new stack_StackAction().ConvertFromJson((JObject)((JArray)json["ACTIONS"]["ACT_EXA"])[i]));
                }
            }
            catch
            {
                Debug.LogError("Unnio: stack action(s) failed to load correctly from file corruption has occoured or file is out of date");
            }

            for (int i = 0; i < ((JArray)json["NODES"]).Count; i++) //load the stacks nodes
            {
                try
                {
                    node_NodeBase newNode = new node_NodeBase().ConvertFromJson((JObject)((JArray)json["NODES"])[i]);
                    m_childNodes.Add(newNode);
                }
                catch
                { 
                    //do noting something else will take care of it
                }
            }

            return this;
        }
        #endregion
    }
}

#endif