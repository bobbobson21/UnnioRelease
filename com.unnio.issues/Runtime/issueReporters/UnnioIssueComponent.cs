using UnityEngine;

namespace EditorPackages.UnnioIssues
{
    [System.Serializable]
    public enum UnnioIssueComponent_Severity
    { 
        Resolved,
        Warning,
        Error,
    }

    [ExecuteInEditMode]
    public class UnnioIssueComponent : MonoBehaviour
    {
        #region public vars: settings
        [Tooltip("the level of importance + is it resolved")]
        public UnnioIssueComponent_Severity m_Severity = UnnioIssueComponent_Severity.Error;

        [Tooltip("think of it as creating a directory but you can only use / do not use \\ or")]
        public string m_issuePath = "";

        [Tooltip("stuff like: who is responceable for fixing this, more details about what went wrong, the name of whats broken")]
        public string m_issueDetails = "";

        [Tooltip("the secondary colour of this issue and it dose not get propagated upwards")]
        public Color m_secondaryColour = new Color();

        [Tooltip("if true you will be able to select the game object from the issues UI")]
        public bool m_reportGameObject = true;
        #endregion

        UnnioIssueComponent() //on creation
        {
#if UNITY_EDITOR
            register_IssueRegister.e_issueRefreshCalled += reportIssue; //we cant put these of the main thread bucause it is in a componet and compontes are in unitys code which cant be accessed off main thread
#endif
        }
        ~UnnioIssueComponent() //on death
        {
#if UNITY_EDITOR
            register_IssueRegister.e_issueRefreshCalled -= reportIssue;
#endif
        }

        private void reportIssue() //dose the bulk of the work
        {
#if UNITY_EDITOR
            try
            {
                if (enabled == false) //if this componet is not enable dont bother doing anything
                {
                    return;
                }

                GameObject reportedGameObject = null;

                if (m_reportGameObject == true && gameObject != null) //can report the game object
                {
                    reportedGameObject = gameObject;
                }

                switch (m_Severity) //what type of issue should we report
                {
                    case UnnioIssueComponent_Severity.Resolved:
                        register_IssueRegister.ReportSuccess(register_IssueRegister.m_issueSpaceScene + m_issuePath).m_secondaryColor = m_secondaryColour;
                        break;
                    case UnnioIssueComponent_Severity.Warning:
                        register_IssueRegister.ReportWarningWithGameObject(register_IssueRegister.m_issueSpaceScene + m_issuePath, m_issueDetails, reportedGameObject).m_secondaryColor = m_secondaryColour;
                        break;
                    case UnnioIssueComponent_Severity.Error:
                        register_IssueRegister.ReportFailureWithGameObject(register_IssueRegister.m_issueSpaceScene + m_issuePath, m_issueDetails, reportedGameObject).m_secondaryColor = m_secondaryColour;
                        break;
                    default:
                        break;
                }
            }
            catch (MissingReferenceException) //incase removal fails to exacute that can happen because of unity dumbness ... I know its annoying
            {
                register_IssueRegister.e_issueRefreshCalled -= reportIssue;
            }
#endif
        }
    }
}
