#if UNITY_EDITOR

using UnityEngine;
using Unity.Plastic.Newtonsoft.Json.Linq;

namespace EditorPackages.Unnio
{
    public enum stack_StackActionTypes : int //ALSO UPDATE stack_Oparator:55 hud_UnnioStackEditor:234 hud_UnnioStackEditor:286 if updating
    {
        None,

        SetNodeColor,
        SetNodeColorIfColor,
        SetNodeColorIfColorNot,
        SetNodeColorIfStatusIs,
        SetNodeColorIfStatusIsNot,
        SetNodeColorIfDateMoreThen,
        SetNodeColorIfDateLessThen,

        RemoveAllNodeConditions,
        RemoveAllNodeConditionIfColor,
        RemoveAllNodeConditionIfColorNot,
        RemoveAllNodeConditionsIfStatusIs,
        RemoveAllNodeConditionsIfStatusIsNot,
        RemoveAllNodeConditionsIfDateMoreThen,
        RemoveAllNodeConditionsIfDateLessThen,

        RemoveNode,
        RemoveNodeIfColor,
        RemoveNodeIfColorNot,
        RemoveNodeIfDateMoreThen,
        RemoveNodeIfDateLessThen,
        RemoveNodeIfStatusIs,
        RemoveNodeIfStatusIsNot,

        SetStatus,
        SetStatusIfColor,
        SetStatusIfColorNot,
        SetStatusIfDateMoreThen,
        SetStatusIfDateLessThen,
        SetStatusIfStatusIs,
        SetStatusIfStatusIsNot,

        SetDeadline,
        SetDeadlineIfColor,
        SetDeadlineIfColorNot,
        SetDeadlineIfDateMoreThen,
        SetDeadlineIfDateLessThen,
        SetDeadlineIfStatusIs,
        SetDeadlineIfStatusIsNot,
    }

    public class stack_StackAction
    {
        #region public var: type
        public stack_StackActionTypes m_actionType = stack_StackActionTypes.None;
        #endregion

        #region public vars: if setting
        public Color m_ifColor = new Color(0, 0, 0, 0);
        public string m_ifDate = "... date (DD/MM/YYYY) here";
        public string m_ifStatus = "";
        #endregion

        #region public vars: then setting
        public bool m_thenColorIsTint = false;
        public Color m_thenColor = Color.green; //the color the node gets set to
        public string m_thenStatus = "";
        public string m_thenDeadline = "";
        #endregion


        #region json
        public JObject ConvertToJson()
        {
            JObject action = new JObject();

            action.Add("ACT_AT", (int)m_actionType);

            JArray ifColArray = new JArray();
            ifColArray.Add(m_ifColor.r);
            ifColArray.Add(m_ifColor.g);
            ifColArray.Add(m_ifColor.b);
            ifColArray.Add(m_ifColor.a);

            action.Add("ACT_IF_COL", ifColArray);
            action.Add("ACT_IF_DA", m_ifDate);
            action.Add("ACT_IF_S", m_ifStatus);

            JArray thenColArray = new JArray();
            thenColArray.Add(m_thenColor.r);
            thenColArray.Add(m_thenColor.g);
            thenColArray.Add(m_thenColor.b);
            thenColArray.Add(m_thenColor.a);

            action.Add("ACT_THEN_COL", thenColArray);
            action.Add("ACT_THEN_COL_TINT", m_thenColorIsTint);
            action.Add("ACT_THEN_S", m_thenStatus);
            action.Add("ACT_THEN_DE", m_thenDeadline);

            return action;
        }

        public stack_StackAction ConvertFromJson(JObject json)
        {
            m_actionType = (stack_StackActionTypes)(int)json["ACT_AT"];

            m_ifColor.r = (float)((JArray)json["ACT_IF_COL"])[0];
            m_ifColor.g = (float)((JArray)json["ACT_IF_COL"])[1];
            m_ifColor.b = (float)((JArray)json["ACT_IF_COL"])[2];
            m_ifColor.a = (float)((JArray)json["ACT_IF_COL"])[3];

            m_ifDate = (string)json["ACT_IF_DA"];
            m_ifStatus = (string)json["ACT_IF_S"];

            if (m_ifDate == null) { m_ifDate = ""; }
            if (m_ifStatus == null) { m_ifStatus = ""; }

            m_thenColor.r = (float)((JArray)json["ACT_THEN_COL"])[0];
            m_thenColor.g = (float)((JArray)json["ACT_THEN_COL"])[1];
            m_thenColor.b = (float)((JArray)json["ACT_THEN_COL"])[2];
            m_thenColor.a = (float)((JArray)json["ACT_THEN_COL"])[3];
            
            m_thenColorIsTint = (bool)json["ACT_THEN_COL_TINT"];
            m_thenStatus = (string)json["ACT_THEN_S"];
            m_thenDeadline = (string)json["ACT_THEN_DE"];

            if (m_thenStatus == null) { m_thenStatus = ""; }
            if (m_thenDeadline == null) { m_thenDeadline = ""; }

            return this;
        }
        #endregion
    }
}

#endif