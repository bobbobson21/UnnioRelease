#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEngine;


namespace EditorPackages.UnnioIssues
{
    [InitializeOnLoad]
    class reporter_ReportAttributeFunctionality
    {
        static reporter_ReportAttributeFunctionality()
        {
            //File.Exists(appl"")

            register_IssueRegister.e_issueRefreshCalled += DoRefresh;
        }

        private static void DoRefresh() //on issue refresh
        {
            //search these locations
            string expectedLocation = Application.dataPath + "/UnnioIssuesIntergration/reporter_AttributeExacuteorRuntime.cs";
            string expectedEditorLocation = Application.dataPath + "/Editor/UnnioEditorIssuesIntergration/reporter_AttributeExacuteorEditor.cs";

            if (File.Exists(expectedLocation) == false) //if the location dose not exist we may not be able to use the UnnioIssueTestAttribute so report this as an issue
            {
                register_IssueRegister.ReportFailureWithCustomOparation("Unnio issues services/Attribute exacuteor", "the attribute exacutor was not found at it's expected location\n\nthe exacutor is needed in order for Unnio issues unit testing to work", "click to genarate new exacutor", AddRuntimeFile);
            }

            if (File.Exists(expectedEditorLocation) == false)
            {
                register_IssueRegister.ReportFailureWithCustomOparation("Unnio issues services/Attribute editor exacuteor", "the attribute exacutor for the editor was not found at it's expected location\n\nthe exacutor is needed in order for Unnio issues unit testing to work", "click to genarate new exacutor", AddEditorFile);
            }
        }

        private static bool AddRuntimeFile() //a soloution for the issue
        {
            string newLocation = Application.dataPath + "/UnnioIssuesIntergration/reporter_AttributeExacuteorRuntime.cs";
            string oldLocation = Path.GetFullPath("Packages/com.unnio.issues/ExportToProject/reporter_AttributeExacuteorRuntime.cs.dummy");

            new FileInfo(newLocation).Directory.Create();
            File.Copy(oldLocation, newLocation);
            AssetDatabase.Refresh();

            return true;
        }

        private static bool AddEditorFile()
        {
            string newLocation = Application.dataPath + "/Editor/UnnioEditorIssuesIntergration/reporter_AttributeExacuteorEditor.cs";
            string oldLocation = Path.GetFullPath("Packages/com.unnio.issues/ExportToProject/reporter_AttributeExacuteorEditor.cs.dummy");

            new FileInfo(newLocation).Directory.Create();
            File.Copy(oldLocation, newLocation);
            AssetDatabase.Refresh();

            return true;
        }
    }
}

#endif
