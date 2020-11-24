using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Crawler;
using System.Threading;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Tags;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Windows.Forms;
using System.Web.Script.Serialization;
using System.IO;

namespace Crawler.Instance
{
    public class SzCorpInfoChuHua : WebSiteCrawller
    {
        public SzCorpInfoChuHua()
            : base()
        {
            this.PlanTime = "1-28 2:00,4-28 2:00,7-28 2:00,10-28 2:00";
            this.Group = "处罚信息";
            this.Title = "深圳市建设局处罚信息";
            this.Description = "自动抓取深圳市建设局处罚信息";
            this.ExistCompareFields = "GrantUnit,DocNo,IsShow";
            this.MaxCount = 1000;
            this.SiteUrl = "http://www.szjs.gov.cn/build/build.ashx?_=1353579439242&menu=%E8%A1%8C%E6%94%BF%E5%A4%84%E7%BD%9A&pageSize=10&pageIndex=1&fileOrg=&fileDate=&fileId=&unitName=&timp=";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            string htl = string.Empty;
            int sqlCount = 0;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1;
            string eventValidation = string.Empty;
            try
            {
                htl = ToolWeb.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return list;
            }
            if (htl.Contains("RowCount"))
            {
                try
                {
                    int index = htl.IndexOf("RowCount");
                    string pageStr = htl.Substring(index, htl.Length - index).Replace("RowCount", "").Replace("}", "").Replace(":", "").Replace("\"", "");
                    decimal b = decimal.Parse(pageStr) / 10;
                    if (b.ToString().Contains("."))
                    {
                        pageInt = Convert.ToInt32(b) + 1;
                    }
                    else { pageInt = Convert.ToInt32(b); }
                }
                catch { }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    try
                    {
                        htl = ToolWeb.GetHtmlByUrl("http://www.szjs.gov.cn/build/build.ashx?_=1353579439242&menu=%E8%A1%8C%E6%94%BF%E5%A4%84%E7%BD%9A&pageSize=10&pageIndex=" + i.ToString() + "&fileOrg=&fileDate=&fileId=&unitName=&timp=", Encoding.UTF8);
                    }
                    catch { }
                }
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                Dictionary<string, object> smsTypeJson = (Dictionary<string, object>)serializer.DeserializeObject(htl);
                foreach (KeyValuePair<string, object> obj in smsTypeJson)
                {
                    if (obj.Key != "DataList")
                    {
                        continue;
                    }
                    object[] array = (object[])obj.Value;
                    foreach (object obj2 in array)
                    {
                        Dictionary<string, object> dicSmsType = (Dictionary<string, object>)obj2;
                        string DocNo = string.Empty, PunishType = string.Empty, GrantUnit = string.Empty, DocDate = string.Empty, PunishCtx = string.Empty, GrantName = string.Empty, InfoUrl = string.Empty;
                        try
                        {
                            DocNo = Convert.ToString(dicSmsType["FileId"]);
                            PunishType = Convert.ToString(dicSmsType["PunTypeText"]);
                            GrantUnit = Convert.ToString(dicSmsType["UnitName"]);
                            DocDate = Convert.ToString(dicSmsType["ServiceDate"]);
                            InfoUrl = "http://www.szjs.gov.cn/PUNhtml/" + Convert.ToString(dicSmsType["PunDoc"]);
                            CorpPunish info = ToolDb.GenCorpPunish(string.Empty, DocNo, PunishType, GrantUnit, DocDate, PunishCtx, InfoUrl, GrantName, "1");
                            if (sqlCount <= this.MaxCount)
                            {
                                if (ToolDb.SaveEntity(info, this.ExistCompareFields))
                                {
                                    string file = Convert.ToString(dicSmsType["PunDoc"]);
                                    AddBaseFile(InfoUrl, file, info);
                                }
                                sqlCount++;
                            }
                            else
                                return list;
                        }
                        catch { continue; }
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// 附件下载
        /// </summary>
        /// <param name="infoUrl"></param>
        private void AddBaseFile(string infoUrl, string strFileName,CorpPunish info)
        {
            string strFileUrl = ToolDb.DbServerPath + "SiteManage\\Files\\Corp_Attach\\";
            string strFile = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + "\\"; //新建文件夹地址 
            long lStartPos = 0;          //返回上次下载字节
            long lCurrentPos = 0;        //返回当前下载文件长度
            long lDownLoadFile;          //返回当前下载文件长度 
            System.IO.FileStream fs;
            long length = 0;
            if (System.IO.File.Exists(strFileUrl + strFile))
            {
                fs = System.IO.File.OpenWrite(strFileUrl + strFile);
                lStartPos = fs.Length;
                fs.Seek(lStartPos, System.IO.SeekOrigin.Current);
            }
            else
            {
                Directory.CreateDirectory(strFileUrl + strFile);
                fs = new FileStream(strFileUrl + strFile + strFileName, System.IO.FileMode.OpenOrCreate);
                lStartPos = 0;
            }
            try
            {
                System.Net.HttpWebRequest request = System.Net.HttpWebRequest.Create(infoUrl) as System.Net.HttpWebRequest;
                length = request.GetResponse().ContentLength;
                lDownLoadFile = length;
                if (lStartPos > 0)
                { request.AddRange((int)lStartPos); }
                System.IO.Stream ns = request.GetResponse().GetResponseStream();
                byte[] nbytes = new byte[102]; 
                int nReadSize = 0;
                nReadSize = ns.Read(nbytes, 0, 102);
                while (nReadSize > 0)
                {
                    fs.Write(nbytes, 0, nReadSize);
                    nReadSize = ns.Read(nbytes, 0, 102);
                    lCurrentPos = fs.Length;
                }
                fs.Close();
                ns.Close();
                if (length > 1024)
                {
                    BaseAttach baseInfo = ToolDb.GenBaseAttach(ToolDb.NewGuid, strFileName, info.Id, strFile + strFileName, length.ToString(), "");
                    ToolDb.SaveEntity(baseInfo, string.Empty);
                }
                else
                {
                    File.Delete(strFileUrl + strFile + strFileName);
                }
            }
            catch
            {
                fs.Close();
                File.Delete(strFileUrl + strFile + strFileName);
            } 
        }
    }
}
