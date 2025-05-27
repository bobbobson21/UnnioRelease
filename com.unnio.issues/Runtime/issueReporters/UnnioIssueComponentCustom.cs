using UnityEngine;

namespace EditorPackages.UnnioIssues
{

    [System.Serializable]
    public enum UnnioIssueComponentCustom_Severity
    {
        Resolved,
        Error,
    }

    [ExecuteInEditMode]
    public class UnnioIssueComponentCustom : MonoBehaviour
    {
        #region public vars: settings
        [Tooltip("has it been resolved")]
        public UnnioIssueComponentCustom_Severity m_Severity = UnnioIssueComponentCustom_Severity.Error;

        [Tooltip("think of it as creating a directory but you can only use / do not use \\ or")]
        public string m_issuePath = "";

        [Tooltip("stuff like: who is responceable for fixing this, more details about what went wrong, the name of whats broken")]
        public string m_issueDetails = "";

        [Tooltip("the colour of this issue")]
        public Color m_colour = new Color();

        [Tooltip("the secondary colour of this issue and it dose not get propagated upwards")]
        public Color m_secondaryColour = new Color();

        [Tooltip("Higher value = higher chance that the containters of this issue (everthing before the final slash in the issues path) will inhertit the colour of this issue")]
        public int m_propagation = 0;

        [Tooltip("if true you will be able to select the game object from the issues UI")]
        public bool m_reportGameObject = true;

        [Tooltip("the path (relative to the assets folder) to the cs code file with the issue. leave blank for no file to be added")]
        public string m_pathToIssueFile = "";

        [Tooltip("the path (relative to the assets folder) to the cs code file with the unit test. leave blank for no file to be added")]
        public string m_pathToTestFile = "";
        #endregion
        UnnioIssueComponentCustom() //on creation
        {
#if UNITY_EDITOR
            register_IssueRegister.e_issueRefreshCalled += reportIssue;
#endif
        }

        ~UnnioIssueComponentCustom() //on death
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

                if (m_reportGameObject == true && gameObject != null)
                {
                    reportedGameObject = gameObject;
                }

                if (m_pathToIssueFile.Contains(".") == false || m_pathToIssueFile.Substring(m_pathToIssueFile.LastIndexOf(".")) != ".cs") //if not valid path set it to nothing
                {
                    m_pathToIssueFile = "";
                }

                if (m_pathToTestFile.Contains(".") == false || m_pathToTestFile.Substring(m_pathToTestFile.LastIndexOf(".")) != ".cs") //if not valid path set it to nothing
                {
                    m_pathToTestFile = "";
                }

                register_IssueRegister.ReportCustom(m_issuePath, (issue_IssueData_IssueStatus)m_Severity, m_colour, m_propagation, reportedGameObject, m_issueDetails, m_pathToIssueFile, m_pathToTestFile).m_secondaryColor = m_secondaryColour;
            }
            catch (MissingReferenceException)
            {
                register_IssueRegister.e_issueRefreshCalled -= reportIssue; //just incase our removerer detector fails
            }
#endif
        }
    }
}
