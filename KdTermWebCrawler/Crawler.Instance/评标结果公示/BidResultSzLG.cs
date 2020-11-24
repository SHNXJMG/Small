using System.Text;
using Crawler;
using System.Collections;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Tags;
using System.Collections.Specialized;
using System;
using System.Text.RegularExpressions;
using System.Data;
using System.IO;
using System.Windows.Forms;

namespace Crawler.Instance
{
    public class BidResultSzLG : WebSiteCrawller
    {
        public BidResultSzLG()
            : base()
        {
            this.Group = "评标结果公示";
            this.Title = "广东省深圳市建设工程龙岗分中心评标结果";
            this.Description = "自动抓取广东省深圳市建设工程龙岗分中心评标结果";
            this.PlanTime = "04:15";
            this.SiteUrl = "http://jyzx.cb.gov.cn/LGjyzxWeb/SiteManage/ZsjyjgsList.aspx?type=11&MenuName=PublicInformation&ModeId=6&ItemId=zsjyjgs&ItemName=%e8%af%84%e6%a0%87%e7%bb%93%e6%9e%9c%e5%85%ac%e7%a4%ba++&clearpaging=true";
            this.MaxCount = 50;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new ArrayList();
            string htl = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1;
            string eventValidation = string.Empty;
            try
            {
                htl = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8, ref cookiestr);
            }
            catch { return null; }
            Parser parser = new Parser(new Lexer(htl));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("id", "ctl00_cph_context_ZsjyjgsList2_GridViewPaging1_PagingDescTd")));
            if (nodeList != null && nodeList.Count > 0)
            {
                try
                {
                    string pagestr = nodeList[0].ToPlainTextString().Trim();
                    string[] page = pagestr.Split('，');
                    pageInt = int.Parse(page[page.Length - 1].Replace("共", "").Replace("页", ""));
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(htl);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(htl);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] { "ctl00$ScriptManager1", 
                        "__EVENTTARGET", "__EVENTARGUMENT", "__VIEWSTATE",
                        "ctl00$cph_context$ZsjyjgsList2$ddlSearch", "ctl00$cph_context$ZsjyjgsList2$txtTitle", 
                        "ctl00$cph_context$ZsjyjgsList2$txtStartTime", "ctl00$cph_context$ZsjyjgsList2$txtEndTime", 
                        "ctl00$cph_context$ZsjyjgsList2$GridViewPaging1$txtGridViewPagingForwardTo", 
                        "__VIEWSTATEENCRYPTED", "__EVENTVALIDATION","ctl00$cph_context$ZsjyjgsList2$GridViewPaging1$btnForwardToPage" },
                        new string[] { "ctl00$cph_context$ZsjyjgsList2$UpdatePanel2|ctl00$cph_context$ZsjyjgsList2$GridViewPaging1$btnForwardToPage", "", "", viewState, "xxbt", "", "", "", i.ToString(), "", eventValidation, "GO" });
                    try
                    {
                        htl = this.ToolWebSite.GetHtmlByUrl(SiteUrl, nvc, Encoding.UTF8, ref cookiestr);
                    }
                    catch { }
                }
                parser = new Parser(new Lexer(htl));
                NodeList dtList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "ctl00_cph_context_ZsjyjgsList2_GridView1")));
                if (dtList != null && dtList.Count > 0)
                {
                    TableTag table = dtList[0] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        string bProv = string.Empty, bCity = string.Empty, bArea = string.Empty, bPrjno = string.Empty,
                                bPrjname = string.Empty, bBidresultendtime = string.Empty,
                                bBaseprice = string.Empty, bBiddate = string.Empty, bBuildunit = string.Empty, bBidmethod = string.Empty,
                                 bRemark = string.Empty, bInfourl = string.Empty;
                        TableRow tr = table.Rows[j];
                        bPrjname = tr.Columns[2].ToPlainTextString().Trim();
                        bBuildunit = tr.Columns[3].ToPlainTextString().Trim();
                        bBiddate = tr.Columns[4].ToPlainTextString().Trim();
                        ATag aTag = tr.Columns[2].SearchFor(typeof(ATag), true)[0] as ATag;
                        bInfourl = "http://jyzx.cb.gov.cn/LGjyzxWeb/SiteManage/" + aTag.Link;
                        BidProject info = ToolDb.GenResultProject("广东省", "深圳市", "龙岗区", bPrjno, bPrjname, bBidresultendtime, bBaseprice, bBiddate, bBuildunit, bBidmethod, bRemark, bInfourl);
                        string sql = string.Format("select Id from BidProject where 1=1 and PrjNo='{0}' and PrjName='{1}'", info.PrjNo, info.PrjName);
                        string result = Convert.ToString(ToolDb.ExecuteScalar(sql));
                        if (!string.IsNullOrEmpty(result))
                        {
                            if (ToolDb.SaveEntity(info, this.ExistCompareFields))
                            {
                                string htltxt = string.Empty;
                                try
                                {
                                    htltxt = this.ToolWebSite.GetHtmlByUrl(bInfourl, Encoding.UTF8);
                                }
                                catch { }
                                Parser par = new Parser(new Lexer(htltxt));
                                NodeList fileList = par.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "ctl00_cph_context_AccessoriesControl1_GridView1")));
                                if (fileList != null && fileList.Count > 0)
                                {
                                    string sqlDelete = string.Format("delete from BaseAttach where SourceId='{0}'", result);
                                    ToolDb.ExecuteSql(sqlDelete);
                                    TableTag tab = fileList[0] as TableTag;
                                    for (int k = 1; k < tab.RowCount; k++)
                                    {
                                        TableRow dr = tab.Rows[k];
                                        ATag aLink = dr.Columns[1].SearchFor(typeof(ATag), true)[0] as ATag;
                                        string data = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + "\\";
                                        string annexName = ToolDb.NewGuid;
                                        FilesClass file = new FilesClass();
                                        file.strUrl = "http://jyzx.cb.gov.cn/LGjyzxWeb/" + aLink.Link.Replace("../", "");
                                        int index = aLink.LinkText.IndexOf(".");
                                        string fileName = annexName + aLink.LinkText.Substring(index, aLink.LinkText.Length - index);
                                        file.strFileName = fileName;
                                        file.strFile = data;
                                        long size = file.DownLoadFile();
                                        if (size > 1024)
                                        {
                                            BaseAttach baseInfo = ToolDb.GenBaseAttach(annexName, aLink.LinkText, info.Id, data + fileName, size.ToString(), "");
                                            ToolDb.SaveEntity(baseInfo, "");
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            //开始下载附件
                            if (ToolDb.SaveEntity(info, this.ExistCompareFields))
                            {
                                string htltxt = string.Empty;
                                try
                                {
                                    htltxt = this.ToolWebSite.GetHtmlByUrl(bInfourl, Encoding.UTF8);
                                }
                                catch { }
                                Parser par = new Parser(new Lexer(htltxt));
                                NodeList fileList = par.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "ctl00_cph_context_AccessoriesControl1_GridView1")));
                                if (fileList != null && fileList.Count > 0)
                                {
                                    TableTag tab = fileList[0] as TableTag;
                                    for (int k = 1; k < tab.RowCount; k++)
                                    {
                                        TableRow dr = tab.Rows[k];
                                        ATag aLink = dr.Columns[1].SearchFor(typeof(ATag), true)[0] as ATag;
                                        string data = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + "\\";
                                        string annexName = ToolDb.NewGuid;
                                        FilesClass file = new FilesClass();
                                        file.strUrl = "http://jyzx.cb.gov.cn/LGjyzxWeb/" + aLink.Link.Replace("../", "");
                                        int index = aLink.LinkText.IndexOf(".");
                                        string fileName = annexName + aLink.LinkText.Substring(index, aLink.LinkText.Length - index);
                                        file.strFileName = fileName;
                                        file.strFile = data;
                                        long size = file.DownLoadFile();
                                        if (size > 1024)
                                        {
                                            BaseAttach baseInfo = ToolDb.GenBaseAttach(annexName, aLink.LinkText, info.Id, data + fileName, size.ToString(), "");
                                            ToolDb.SaveEntity(baseInfo, "");
                                        }
                                    }
                                }
                            }
                        }

                    }
                }
            }
            return list;
        }
    }



    public class FilesClass
    {
        public string strFileUrl = ToolDb.DbServerPath + "SiteManage\\Files\\Attach\\";
        public string strUrl;               //文件下载地址
        public string strFileName;          //下载文件保存名称
        public string strFile;              //新建文件夹地址
        public string strError;             //返回结果
        public long lStartPos = 0;          //返回上次下载字节
        public long lCurrentPos = 0;        //返回当前下载文件长度
        public long lDownLoadFile;          //返回当前下载文件长度
        public string state;

        public long DownLoadFile()
        {
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
                System.Net.HttpWebRequest request = System.Net.HttpWebRequest.Create(strUrl) as System.Net.HttpWebRequest;
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
                if (length < 1024)
                {
                    File.Delete(strFileUrl + strFile + strFileName);
                }
                strError = "下载完成";
            }
            catch
            {
                fs.Close();
                File.Delete(strFileUrl + strFile + strFileName);
                strError = "下载过程中出现错误";
            }
            return length;
        }
    }
}
