#if UNITY_EDITOR

using Unity.Plastic.Newtonsoft.Json.Linq;

namespace EditorPackages.Unnio
{
    public enum node_NodeConditionTypes : int //dont forget to also update 121 of unnio node editor and the node oparator
    {
        None,
        AssetFileCreated,
        FunctionCreated,
        FunctionCreatedInClass,
        VariableCreatedInClass,
        NamespaceCreated,
        SceneCreated,
        SceneCountMoreThan,
        DateMoreThan,
    }

    public class node_NodeCondition
    {
        #region public vars: condition data
        public node_NodeConditionTypes m_conditionType = node_NodeConditionTypes.None;
        public string m_testAgainstString = "... number,date (DD/MM/YYYY), or class/scene name here";
        public int m_onTestPassMoveToStack = 0; //-1 means remove
        #endregion


        #region json
        public JObject ConvertToJson()
        {
            JObject condition = new JObject();
            condition.Add("COND_CT", (int)m_conditionType);
            condition.Add("COND_TAS", m_testAgainstString);
            condition.Add("COND_OTPMTS", m_onTestPassMoveToStack);

            return condition;
        }

        public node_NodeCondition ConvertFromJson(JObject json)
        {
            m_conditionType = (node_NodeConditionTypes)(int)json["COND_CT"];
            m_testAgainstString = (string)json["COND_TAS"];
            m_onTestPassMoveToStack = (int)json["COND_OTPMTS"];

            if (m_testAgainstString == null) { m_testAgainstString = ""; } 

            return this;
        }
        #endregion
    }
}

#endif