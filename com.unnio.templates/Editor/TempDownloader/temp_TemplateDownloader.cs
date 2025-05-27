#if UNITY_EDITOR

using System.IO;
using System.IO.Compression;
using System.Net;
using System;
using System.Collections.Generic;


namespace EditorPackages.UnnioTemp
{
    public class temp_TemplateDownloader
    {
        #region private vars: storage locations
        private string m_pointOfOffSiteStorage = "";
        private string m_pointOfOnSiteStorage = "";
        private string m_pointOfOnSiteZipStorage = "";
        #endregion

        #region private vars: web data
        private WebClient m_webClient = null;
        #endregion

        #region private vars: handle
        private bool m_isValid = true;
        private static List<int> m_allHandlesInUse = new List<int>();
        private int m_handle = -1;
        #endregion


        #region public vars: events
        public delegate void TimeStamp(int handle, DateTime date); //stamp info is a time stamp and a who made that time stamp
        public static event TimeStamp e_Failed;
        public static event TimeStamp e_DataIsMounting;
        public static event TimeStamp e_Completed;
        #endregion

        public temp_TemplateDownloader(int handle, string pullFrom, string pushTo)
        {
            m_handle = handle;

            if (m_allHandlesInUse.Contains(handle) == false)
            {
                m_allHandlesInUse.Add(handle);
            }
            else
            {
                m_isValid = false;
            }

            m_pointOfOffSiteStorage = pullFrom;
            m_pointOfOnSiteStorage = pushTo;
            m_pointOfOnSiteZipStorage = pushTo + $"/extract_{handle}.zip";
        }

        public bool IsHandleValid()
        {
            return m_isValid;
        }

        public void DownloadTemplates()
        {
            if (m_isValid == false) { return; } //so we cant use the same handle twice

            m_webClient = new WebClient();

            try
            {
                m_webClient.OpenRead("https://github.com/github/"); //should throw an exception if internet is off or github is unreachable

                try
                {
                    Directory.Delete(m_pointOfOnSiteStorage, true); //and if the exception is thrown before this you should be able to use it off line yay
                }
                catch
                { 
                    //do nothing the next step will sort it out
                }

                try
                {
                    new FileInfo(m_pointOfOnSiteZipStorage).Directory.Create(); //these two lines of code empty out the folder incase you wanted to kow that
                }
                catch
                {
                    if (e_Failed != null)
                    {
                        e_Failed(m_handle, DateTime.Now); //so we know the point of when something hapened
                        ClearDisposeables();
                        return;
                    }
                }

                m_webClient.DownloadFile(m_pointOfOffSiteStorage, m_pointOfOnSiteZipStorage); //will stop here untill dowload is done
                if (e_DataIsMounting != null) //we are now mounting data
                {
                    e_DataIsMounting(m_handle,DateTime.Now);
                }

                ZipFile.ExtractToDirectory(m_pointOfOnSiteZipStorage, m_pointOfOnSiteStorage); //mount data
                if (File.Exists(m_pointOfOnSiteZipStorage) == true) //get rid of zip
                {
                    File.Delete(m_pointOfOnSiteZipStorage);
                }

                if (e_Completed != null) //when the exacution pointer gets past the line above we are done
                {
                    e_Completed(m_handle, DateTime.Now);
                    ClearDisposeables();
                    return;
                }
            }
            catch (WebException) //we were unable to connect to the internet
            {
                if (e_Failed != null)
                {
                    e_Failed(m_handle, DateTime.Now); //so we know the point of when something hapened
                    ClearDisposeables();
                    return;
                }
            }
        }

        public static void ClearEventsGlobally() //running this could be bad unless you know what your doing
        {
            e_Failed = null;
            e_DataIsMounting = null;
            e_Completed = null;
        }

        private void ClearDisposeables()
        {
            m_allHandlesInUse.Remove(m_handle);
            m_webClient.Dispose();
            m_webClient = null;

            //if theres anything else we would ever need to dispose of put it here
        }

    }
}

#endif