#if UNITY_EDITOR

using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.Plastic.Newtonsoft.Json.Linq;

namespace EditorPackages.Unnio
{
    public class node_NodeBase
    {
        #region public vars: child conditions
        public List<node_NodeCondition> m_nodeConditions = new List<node_NodeCondition>();
        #endregion

        #region public vars: node settings
        public string m_title = "... title here";
        public string m_description = "... description here";
        public string m_deadline = "";
        public string m_status = "";
        public string m_user = Environment.UserName;

        public Color m_color = new Color(0,0,0,0);
        #endregion


        #region json
        public JObject ConvertToJson()
        {
            JObject result = new JObject();
            result.Add("T", m_title);
            result.Add("DES", m_description);
            result.Add("DEA", m_deadline);
            result.Add("S", m_status);
            result.Add("U", m_user);

            JArray colArray = new JArray();
            colArray.Add(m_color.r);
            colArray.Add(m_color.g);
            colArray.Add(m_color.b);
            colArray.Add(m_color.a);

            result.Add("COL", colArray);

            JArray conditionsArray = new JArray();

            for (int i = 0; i < m_nodeConditions.Count; i++)
            {
                conditionsArray.Add(m_nodeConditions[i].ConvertToJson());
            }

            result.Add("CONDS", conditionsArray);

            return result;
        }

        public node_NodeBase ConvertFromJson(JObject json)
        {
            try
            {
                m_title = (string)json["T"];
                m_description = (string)json["DES"];
                m_deadline = (string)json["DEA"];
                m_status = (string)json["S"];
                m_user = (string)json["U"];

                if (m_title == null) { m_title = ""; }
                if (m_description == null) { m_description = ""; }
                if (m_deadline == null) { m_deadline = ""; }
                if (m_status == null) { m_status = ""; }
                if (m_user == null) { m_user = ""; }

                m_color.r = (float)((JArray)json["COL"])[0];
                m_color.g = (float)((JArray)json["COL"])[1];
                m_color.b = (float)((JArray)json["COL"])[2];
                m_color.a = (float)((JArray)json["COL"])[3];
            }
            catch
            {
                Debug.LogError("Unnio: failed to load node from file correctly corruption has occoured or file is out of date");
            }

            try
            {
                for (int i = 0; i < ((JArray)json["CONDS"]).Count; i++)
                {
                    try
                    {
                        m_nodeConditions.Add(new node_NodeCondition().ConvertFromJson((JObject)((JArray)json["CONDS"])[i]));
                    }
                    catch 
                    {
                        Debug.LogError("Unnio: node condition failed to load correctly from file corruption has occoured or file is out of date");
                    }
                }
            }
            catch
            { 
                //do noting it could just be the case there were no items in the array and it was removed for compression
            }

            return this;
        }
        #endregion
    }
}

#endif