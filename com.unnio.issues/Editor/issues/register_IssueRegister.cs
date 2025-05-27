#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace EditorPackages.UnnioIssues
{
    public class register_IssueRegister //devenv, code to open files
    {
        #region private var: locking object
        private static System.Object m_lockingObject  = new System.Object();
        #endregion


        #region public vars: events
        public delegate void Empty(); //dont need anything
        public static event Empty e_issueRefreshCalled;
        public static event Empty e_issueRefreshCalledOffMainThread;
        #endregion

        #region public vars: master child list
        public static List<issue_IssueData> m_childIssues { get; private set; } = new List<issue_IssueData>();
        #endregion

        #region public vars: default issue colours
        public static Color m_issueErrorColor { get; private set; } = new Color(0.9f, 0, 0, 1);
        public static Color m_issueWarningColor { get; private set; } = new Color(1, 1, 0, 1);
        public static Color m_issueGoodColor { get; private set; } = new Color(0, 0.9f, 0, 1);
        #endregion

        #region public vars: issueSpaces
        public static string m_issueSpaceCode { get; private set; } = "in code/"; //basicly there the first containers
        public static string m_issueSpaceScene { get; private set; } = "in current scene/";
        #endregion

        public static void Refresh()
        {
            m_childIssues.Clear();

            if (e_issueRefreshCalled != null)
            {
                e_issueRefreshCalled();
            }

            Thread downloaderThread = new Thread(new ThreadStart(() =>
            {
                if (e_issueRefreshCalledOffMainThread != null)
                {
                    e_issueRefreshCalledOffMainThread();
                }
            }));
            downloaderThread.Start();
        }

        public static issue_IssueData ReportCustom(string path, issue_IssueData_IssueStatus status, Color col, int propagation, GameObject obj, string details, string pathToIssueLocation, string pathToTestLocation)
        {
            lock (m_lockingObject) //to stop thread clashing
            {
                string[] pathArray = path.Split("/");
                List<issue_IssueData> currentLevel = m_childIssues; //for this one instance we can thnak god lists are classes
                issue_IssueData parentContainer = null;
                issue_IssueData lastIssueCreated = null;

                for (int i = 0; i < pathArray.Length; i++)
                {
                    if (i != pathArray.Length - 1) //we are not at the end of the path yet
                    {
                        bool addContainer = true;

                        for (int o = 0; o < currentLevel.Count; o++) //search for if this container already exists
                        {
                            if (currentLevel[o].m_title == pathArray[i] && currentLevel[o].GetStatus() == issue_IssueData_IssueStatus.container) //it dose
                            {
                                parentContainer = currentLevel[o];
                                currentLevel = currentLevel[o].GetChildren(); //we search for the next container in that
                                addContainer = false;
                                break; //exit o loop
                            }
                        }

                        if (addContainer == true) //the container dose not exist so make it
                        {
                            issue_IssueData newContainer = new issue_IssueData(issue_IssueData_IssueStatus.container);
                            newContainer.m_title = pathArray[i];
                            currentLevel = newContainer.GetChildren();

                            if (parentContainer == null)
                            {
                                m_childIssues.Add(newContainer); //it is top level
                            }
                            else
                            {
                                parentContainer.AddChild(newContainer); //it is not top level
                            }

                            lastIssueCreated = newContainer;
                            parentContainer = newContainer;
                        }
                    }
                    else
                    {
                        issue_IssueData newIssue = new issue_IssueData(status); //the file part of the path is the issue its self
                        newIssue.m_title = pathArray[i];
                        newIssue.m_color = col;
                        newIssue.m_propagationLevel = propagation;
                        newIssue.m_gameObjectWithIssue = obj;
                        newIssue.m_details = details;
                        newIssue.m_pathToFileWithIssue = pathToIssueLocation;
                        newIssue.m_pathToFileWithIssueTest = pathToTestLocation;

                        parentContainer.AddChild(newIssue);
                        lastIssueCreated = newIssue;
                    }
                }

                return lastIssueCreated;
            }
        }

        #region normal issue reporting functions
        public static issue_IssueData ReportFailure(string path, string details)
        {
            return ReportCustom(path, issue_IssueData_IssueStatus.problematic, m_issueErrorColor, 2, null, details, "", "");
        }

        public static issue_IssueData ReportFailureWithGameObject(string path, string details, GameObject obj)
        {
            return ReportCustom(path, issue_IssueData_IssueStatus.problematic, m_issueErrorColor, 2, obj, details, "", "");
        }

        public static issue_IssueData ReportFailureWithCustomOparation(string path, string details, string funcName, Func<bool> func)
        {
            issue_IssueData issue = ReportCustom(path, issue_IssueData_IssueStatus.problematic, m_issueErrorColor, 2, null, details, "", "");
            issue.f_customButtonFunctionForIssue = func;
            issue.f_customButtonFunctionTitleForIssue = funcName;

            return issue;
        }

        public static issue_IssueData ReportFailureWithLocation(string path, string details, string pathToIssueLocation)
        {
            return ReportCustom(path, issue_IssueData_IssueStatus.problematic, m_issueErrorColor, 2, null, details, pathToIssueLocation, "");
        }

        public static issue_IssueData ReportFailureWithTest(string path, string details, string pathToTestLocation)
        {
            return ReportCustom(path, issue_IssueData_IssueStatus.problematic, m_issueErrorColor, 2, null, details, "", pathToTestLocation);
        }

        public static issue_IssueData ReportFailureWithLocationAndTest(string path, string details, string pathToIssueLocation, string pathToTestLocation)
        {
            return ReportCustom(path, issue_IssueData_IssueStatus.problematic, m_issueErrorColor, 2, null, details, pathToIssueLocation, pathToIssueLocation);
        }

        public static issue_IssueData ReportWarning(string path, string details)
        {
            return ReportCustom(path, issue_IssueData_IssueStatus.problematic, m_issueWarningColor, 1, null, details, "", "");
        }

        public static issue_IssueData ReportWarningWithGameObject(string path, string details, GameObject obj)
        {
            return ReportCustom(path, issue_IssueData_IssueStatus.problematic, m_issueWarningColor, 1, obj, details, "", "");
        }

        public static issue_IssueData ReportWarningWithCustomOparation(string path, string details, string funcName, Func<bool> func)
        {
            issue_IssueData issue = ReportCustom(path, issue_IssueData_IssueStatus.problematic, m_issueWarningColor, 1, null, details, "", "");
            issue.f_customButtonFunctionForIssue = func;
            issue.f_customButtonFunctionTitleForIssue = funcName;

            return issue;
        }

        public static issue_IssueData ReportWarningWithLocation(string path, string details, string pathToIssueLocation)
        {
            return ReportCustom(path, issue_IssueData_IssueStatus.problematic, m_issueWarningColor, 1, null, details, pathToIssueLocation, "");
        }

        public static issue_IssueData ReportWarningWithTest(string path, string details, string pathToTestLocation)
        {
            return ReportCustom(path, issue_IssueData_IssueStatus.problematic, m_issueWarningColor, 1, null, details, "", pathToTestLocation);
        }

        public static issue_IssueData ReportWarningWithLocationAndTest(string path, string details, string pathToIssueLocation, string pathToTestLocation)
        {
            return ReportCustom(path, issue_IssueData_IssueStatus.problematic, m_issueWarningColor, 1, null, details, pathToIssueLocation, pathToTestLocation);
        }

        public static issue_IssueData ReportSuccess(string path)
        {
            return ReportCustom(path, issue_IssueData_IssueStatus.resolved, m_issueGoodColor, 0, null, "", "", "");
        }
        #endregion
    }
}

#endif