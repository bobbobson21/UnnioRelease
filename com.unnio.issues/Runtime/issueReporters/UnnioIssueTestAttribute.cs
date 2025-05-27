using System;
using System.Reflection;

namespace EditorPackages.UnnioIssues
{
    [AttributeUsage(AttributeTargets.Method)]
    public class UnnioIssueTestAttribute : Attribute
    {
        public bool m_OffmainThread = false;

        public UnnioIssueTestAttribute()
        { 
            //stuff
        }

        /// <param name="OffMainThread">if true unity will not be slowed down by the test but some unity things may not work anymore</param>
        public UnnioIssueTestAttribute(bool OffMainThread)
        {
            m_OffmainThread = OffMainThread;
        }

    }
}
