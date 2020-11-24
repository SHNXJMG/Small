using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Data;
using Crawler.Base.KdService;

namespace Crawler.Instance
{
    public class ToolFile
    {
        /// <summary>
        /// 删除附件
        /// </summary>
        /// <param name="list"></param>
        public static void Delete(List<BaseAttach> list,string path="Attach")
        {
            string filePath =ToolCoreDb.DbServerPath+ "SiteManage\\Files\\" + path + "\\";
            foreach (BaseAttach entity in list)
            {
                FileInfo file = new FileInfo(filePath+entity.AttachServerPath);
                if (file.Exists)
                    file.Delete();
            }
        }

        public static void Delete(string sourceId, string path = "Attach")
        {
            DataTable dt = ToolDb.GetDbData(string.Format("select AttachServerPath from BaseAttach where SourceID='{0}'", sourceId));
            if (dt != null && dt.Rows.Count > 0)
            {
                string filePath = ToolCoreDb.DbServerPath + "SiteManage\\Files\\" + path + "\\";
                foreach (DataRow row in dt.Rows)
                {
                    if (row["AttachServerPath"] != null && row["AttachServerPath"].ToString() != "")
                    {
                        FileInfo file = new FileInfo(filePath + row["AttachServerPath"].ToString());
                        if (file.Exists)
                            file.Delete();
                    }
                }
            }
        }

        public static string WebQualPath
        {
            get { return Path.Combine(System.Environment.CurrentDirectory, "QualPat.xml"); }
        }

        public static string WebCityPath
        {
            get { return Path.Combine(System.Environment.CurrentDirectory, "CityPath.xml"); }
        }

        #region 序列化
        /// <summary>
        /// 序列化对象集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="pXMLFilePath"></param>
        public static void Serialize<T>(List<T> list, string pXMLFilePath)
        {
            System.Xml.Serialization.XmlSerializer seriliaser = new System.Xml.Serialization.XmlSerializer(typeof(List<T>));
            try
            {
                using (System.IO.TextWriter txtWriter = new System.IO.StreamWriter(pXMLFilePath))
                {
                    seriliaser.Serialize(txtWriter, list);
                    txtWriter.Close();
                }
            }
            catch (Exception e)
            {
            }
        }

        /// <summary>
        /// 反序列化集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pXMLFilePath"></param>
        /// <returns></returns>
        public static List<T> Deserialize<T>(string pXMLFilePath)
        {
            List<T> list = new List<T>();
            try
            {
                System.Xml.Serialization.XmlSerializer seriliaser = new System.Xml.Serialization.XmlSerializer(typeof(List<T>));
                if (File.Exists(pXMLFilePath))
                {
                    using (System.IO.TextReader txtReader = new System.IO.StreamReader(pXMLFilePath))
                    {
                        list = (List<T>)seriliaser.Deserialize(txtReader);
                        txtReader.Close();
                    }
                }
            }
            catch (Exception e)
            {
            }
            return list;
        }
        #endregion
    }
}
