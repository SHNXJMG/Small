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
using System.IO;
using System.Web.Script.Serialization;
using System.Collections.Generic;

namespace Crawler.Instance
{
    public class NotifyInfoSzJsGcJy : WebSiteCrawller
    {
        public NotifyInfoSzJsGcJy()
            : base()
        {
            this.Group = "通知公告";
            this.Title = "广东省深圳市建设工程通知公告";
            this.Description = "自动抓取广东省深圳市建设工程通知公告";
            this.PlanTime = "21:40";
            this.SiteUrl = "http://www.szjsjy.com.cn/Notify/InformBrows.aspx?xxlb=31";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        { 
            //取得页码
            int pageInt = 1,sqlCount=0;
            string html = string.Empty;
            string viewState = string.Empty;
            string eventValidation = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(SiteUrl, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return null;
            }
            Parser parser = new Parser(new Lexer(html));
            NodeList pageList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "GridView1")), true), new TagNameFilter("table")));
            if (pageList != null && pageList.Count > 0)
            {
                try
                {
                    TableTag table = pageList[0] as TableTag;
                    int pageAtag = table.Rows[0].ColumnCount;
                    pageInt = int.Parse((table.Rows[0].SearchFor(typeof(ATag), true)[pageAtag - 2] as ATag).LinkText);
                }
                catch { pageInt = 1; }
            }
            for (int i = 1; i <= pageInt; i++)
            {
                if (i > 1)
                {
                    viewState = this.ToolWebSite.GetAspNetViewState(html);
                    eventValidation = this.ToolWebSite.GetAspNetEventValidation(html);
                    NameValueCollection nvc = this.ToolWebSite.GetNameValueCollection(new string[] { 
                        "__EVENTTARGET",
                    "__EVENTARGUMENT",
                    "__VIEWSTATE",
                    "__VIEWSTATEENCRYPTED",
                    "__EVENTVALIDATION",
                    "sel",
                    "beginDate",
                    "endDate",
                    "infotitle"},
                    new string[] {
                    "GridView1","Page$"+i.ToString(),viewState,"",eventValidation,"1","","",""});
                    try
                    {
                        html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), nvc, Encoding.UTF8);
                    }
                    catch { continue; }
                }
                parser = new Parser(new Lexer(html));
                NodeList dtList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("id", "GridView1")));
                if (dtList != null && dtList.Count > 0)
                {
                    TableTag table = dtList[0] as TableTag;
                    for (int j = 1; j < table.RowCount - 1; j++)
                    {
                        string headName = string.Empty, releaseTime = string.Empty, infoScorce = string.Empty, msgType = string.Empty,
                            infoUrl = string.Empty, ctxHtml = string.Empty, infoCtx = string.Empty, infoType = string.Empty;
                        TableRow tr = table.Rows[j];
                        headName = tr.Columns[1].ToPlainTextString().Trim();
                        infoScorce = tr.Columns[2].ToPlainTextString().Trim();
                        releaseTime = tr.Columns[3].ToPlainTextString().Trim();
                        infoType = "通知公告";
                        ATag aTag = tr.Columns[1].SearchFor(typeof(ATag), true)[0] as ATag;
                        infoUrl = "http://www.szjsjy.com.cn/Notify/" + aTag.Link;
                        string htmldetailtxt = string.Empty;
                        try
                        {
                            htmldetailtxt = this.ToolWebSite.GetHtmlByUrl(infoUrl, Encoding.UTF8);
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htmldetailtxt));
                        NodeList noList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("background", "../img/A-3_17.gif")));
                        if (noList != null && noList.Count > 0)
                        {
                            ctxHtml = noList.AsHtml().Replace("<br/>", "\r\n").Replace("<BR/>", "");
                            infoCtx = noList.AsString().Replace(" ", "").Replace("&nbsp;", "").Replace("\t\t", "\t").Replace("\t\t", "\t");
                            infoCtx = Regex.Replace(infoCtx, @"<(.[^>]*)>", "", RegexOptions.IgnoreCase).Replace(" ", "").Replace("\t", "");
                            msgType = "深圳市建设工程交易中心";
                            infoScorce = infoScorce.Replace("&nbsp;", "");
                            NotifyInfo info = ToolDb.GenNotifyInfo(headName, releaseTime, infoScorce, msgType, infoUrl, ctxHtml, "广东省", "深圳市工程", string.Empty, infoCtx, infoType);
                            if (!crawlAll && sqlCount >= this.MaxCount)
                            {
                                return null;
                            }
                            else
                            {
                                sqlCount++;
                                if (ToolDb.SaveEntity(info, this.ExistCompareFields))
                                {
                                    parser = new Parser(new Lexer(ctxHtml));
                                    NodeFilter aLink = new TagNameFilter("a");
                                    NodeList aList = parser.ExtractAllNodesThatMatch(aLink);
                                    if (aList != null && aList.Count > 0)
                                    {
                                        for (int k = 0; k < aList.Count; k++)
                                        {
                                            ATag a = aList.SearchFor(typeof(ATag), true)[k] as ATag;
                                            if (a != null)
                                            {
                                                if (!a.LinkText.Contains("返回"))
                                                {
                                                    AddBaseFile("http://www.szjsjy.com.cn/" + a.Link.Replace("../", ""), a.LinkText, info);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 附件下载
        /// </summary>
        /// <param name="infoUrl"></param>
        private void AddBaseFile(string infoUrl, string strFileName, NotifyInfo info)
        {
            string pathName = System.IO.Path.GetExtension(strFileName.ToLower());
            string path = ToolDb.NewGuid + pathName;
            string strFileUrl = ToolDb.DbServerPath +  "SiteManage\\Files\\Notify_Attach\\";
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
                fs = new FileStream(strFileUrl + strFile + path, System.IO.FileMode.OpenOrCreate);
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
                    BaseAttach baseInfo = ToolDb.GenBaseAttach(ToolDb.NewGuid, strFileName, info.Id, strFile + path, length.ToString(), "");
                    ToolDb.SaveEntity(baseInfo, string.Empty);
                }
                else
                {
                    File.Delete(strFileUrl + strFile + path);
                }
            }
            catch
            {
                fs.Close();
                File.Delete(strFileUrl + strFile + path);
            }
        }
    }
}
