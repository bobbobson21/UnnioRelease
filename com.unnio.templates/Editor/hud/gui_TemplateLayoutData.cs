#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using Unity.Plastic.Newtonsoft.Json.Linq;
using System.IO;

namespace EditorPackages.UnnioTemp
{
    public class gui_TemplateLayoutData
    {
        private static float m_backgroundTrasparentOpacity = 0.20f;

        #region private vars: paths
        private string m_backgroundPath; //the relative path
        private string m_templatePath = ""; //the relative path
        #endregion


        #region public vars: uploader details
        public string m_title { get; private set; } = "" ; //uploader details
        public string m_description { get; private set; } = ""; //uploader details

        public string m_creatorName { get; private set; } = ""; //uploader details
        public string m_uploadDate { get; private set; } = ""; //uploader details
        public string m_templateVersion { get; private set; } = ""; //uploader details
        public string m_unnioVersions { get; private set; } = ""; //uploader details
        #endregion

        #region public vars: texture & template data
        public Texture2D m_background { get; private set; } = null; //image with normal transparency
        public Texture2D m_backgroundTrasparent { get; private set; } = null; //image with transparency
        public string m_templateLocation { get; private set; } = ""; //full path to where we got the image from
        #endregion


        #region json
        public JObject ConvertToJson() //I dont believe you would be needing this but it is still nice to have just incase
        {
            JObject result = new JObject();
            result.Add("TITLE", m_title);
            result.Add("DESCRIPTION", m_description);

            result.Add("CREATOR_NAME", m_creatorName);
            result.Add("UPLOAD_DATE", m_uploadDate);
            result.Add("TEMPLATE_VERSION", m_templateVersion);
            result.Add("UNNIO_VERSIONS", m_unnioVersions);

            result.Add("BG", m_backgroundPath);
            result.Add("TEMPLATE", m_templatePath);

            return result;
        }

        public gui_TemplateLayoutData ConvertFromJson(JObject json, string startingTexturePath, string startingTemplatePath)
        {
            try //load title and description
            {
                m_title = (string)json["TITLE"];
                m_description = (string)json["DESCRIPTION"];

                if (m_title == null) { m_title = ""; }
                if (m_description == null) { m_description = ""; }
            }
            catch
            {
                Debug.LogError("Unnio Temp: failed to load basic template details into template viewer from file correctly, corruption has occoured or file is out of date");
            }

            try //load details
            {
                m_creatorName = (string)json["CREATOR_NAME"];
                m_uploadDate = (string)json["UPLOAD_DATE"];
                m_templateVersion = (string)json["TEMPLATE_VERSION"];
                m_unnioVersions = (string)json["UNNIO_VERSIONS"];

                if (m_creatorName == null) { m_creatorName = ""; }
                if (m_uploadDate == null) { m_uploadDate = ""; }
                if (m_templateVersion == null) { m_templateVersion = ""; }
                if (m_unnioVersions == null) { m_unnioVersions = ""; }
            }
            catch
            {
                Debug.LogError("Unnio Temp: failed to load template details into template viewer from file correctly, corruption has occoured or file is out of date");
            }

            try
            {
                m_backgroundPath = (string)json["BG"]; //the path to where the background is
                if (m_backgroundPath == null) { m_backgroundPath = ""; }

                m_templatePath = (string)json["TEMPLATE"]; //no if null check for these because if this is null something was done wrong and an error should be thrown
                m_templateLocation = startingTemplatePath + m_templatePath; //full path 

                string imagePath = startingTexturePath + m_backgroundPath; //full path of image
                Texture2D tex = new Texture2D(1, 1); //texture objects
                Texture2D texTrans = new Texture2D(1, 1);

                if (File.Exists(imagePath) == true) //is texture valid
                {
                    tex.LoadImage(File.ReadAllBytes(imagePath)); //load texture
                    tex.Apply();

                    texTrans.LoadImage(File.ReadAllBytes(imagePath));
                    texTrans.Apply();

                    for (int x = 0; x < texTrans.width; x++) //apply transparency to the transparent version
                    {
                        for (int y = 0; y < texTrans.width; y++)
                        {
                            Color col = texTrans.GetPixel(x, y);
                            col.a = m_backgroundTrasparentOpacity;

                            texTrans.SetPixel(x, y, col);
                        }
                    }

                    texTrans.Apply();
                }
                else //if texture is not create empty backgrounds
                {
                    tex.SetPixel(0, 0, new Color(0,0,0,0));
                    texTrans.SetPixel(0, 0, new Color(0, 0, 0, 0));
                    
                    tex.Apply();
                    texTrans.Apply();
                }

                m_backgroundTrasparent = texTrans;
                m_background = tex;
            }
            catch
            {
                Debug.LogError("Unnio Temp: failed to load template path data into template viewer from file correctly, corruption has occoured or file is out of date");
            }

            return this;
        }
        #endregion
    }
}

#endif