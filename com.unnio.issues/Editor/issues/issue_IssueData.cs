#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;

namespace EditorPackages.UnnioIssues
{
    public enum issue_IssueData_IssueStatus 
    { 
        resolved,
        problematic, ///if set to problmatic buttons will render that can direct a person to the issue
        container, ///if set to container it wll not have any buttons or details and its colour will come from an issue inside of it and it can also have issues inside of it that will render
    }

    public class issue_IssueData
    {
        #region private vars: status
        private issue_IssueData_IssueStatus m_status = issue_IssueData_IssueStatus.resolved;
        #endregion

        #region private vars: parent & children
        private issue_IssueData m_highestPropagator = null;
        private issue_IssueData m_parent = null;
        private List<issue_IssueData> m_childIssues = new List<issue_IssueData>();
        #endregion


        #region public vars: display detials & propagation
        public int m_propagationLevel = 0; ///higher = more likely that the parent will have tis color

        public Color m_color = Color.green;
        public Color m_secondaryColor = new Color(0,0,0,0); ///used as a quick wat to indercate stuff like which quoup gets this
        public string m_title = "";
        public string m_details = "";
        #endregion

        #region public vars: issue resolve options
        public string m_pathToFileWithIssue = ""; ///relative to assets folder ///if left blank it will also not do anything
        public string m_pathToFileWithIssueTest = ""; ///relative to assets folder ///where the automatic test that fired off the issues is

        public GameObject m_gameObjectWithIssue = null; ///if set it will not go to the file but to this game object

        public Func<bool> f_customButtonFunctionForIssue = null; ///what will it do and should it refresh after
        public string f_customButtonFunctionTitleForIssue = ""; ///what is this called
        #endregion

        public issue_IssueData(issue_IssueData_IssueStatus status) //so it cant be changed down the line after it had a bunch of kids
        {
            m_status = status;
        }

        public issue_IssueData_IssueStatus GetStatus()
        {
            return m_status;
        }

        public void ProagateColor()
        {
            issue_IssueData parent = this;

            while (parent != null) //go up the tree untill we cant anymore
            {
                if (parent.m_highestPropagator == null || parent.m_highestPropagator.m_propagationLevel < this.m_propagationLevel) //shoud this parent have our colour insted
                {
                    parent.m_highestPropagator = this; //yes
                    parent.m_color = this.m_color;
                }
                else
                {
                    break;
                }

                parent = parent.m_parent;
            }
        }

        #region children
        public void AddChild(issue_IssueData child)
        {
            if (m_status != issue_IssueData_IssueStatus.container) { return; } //only containers can have children
            m_childIssues.Add(child);
            
            child.m_parent = this;
            child.ProagateColor();
        }

        public List<issue_IssueData> GetChildren()
        {
            return m_childIssues;
        }

        public issue_IssueData GetParent()
        {
            return m_parent;
        }
        #endregion
    }
}

#endif