using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Crawler.Instance;
using Winista.Text.HtmlParser;
using Crawler.Base.KdService;
using Crawler.Base.KdService;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Tags;
using Winista.Text.HtmlParser.Util;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.Web;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Net.Cache;
using System.IO.Compression;

namespace Crawler.Instance
{
    public class ToolHtml
    {

        #region 常量
        /// <summary>
        /// 匹配中标人
        /// </summary>
        public static readonly string[] BidRegex = new string[] { "中标单位名称", "中标单位", "中标企业", "成交单位", "中标人名称", "中标人", "中标方", "中标商", "成交供应商名称", "成交供应商", "成交商", "中标供应商", "服务商名称", "投标报名人", "供应商名称", "投标人名称", "中标单位为", "第一中标后选人单位", "第一候选人", "第一中标候选人", "中标供应商为", "中标供应商名称", "中标（成交）供应商名称", "中标候选单位", "中标供应商名称（包一）", "中标（成交）候选人", "成交单位名称", "成交供应商为", "中标（成交）人名称", "中标候选人", "投标供应商", "拟定中标人候选人" };
        /// <summary>
        /// 匹配招标单位
        /// </summary>
        public static readonly string[] BuildRegex = { "建设单位", "采购人名称", "集中采购机构", "招标人名称", "招标人", "承包人", "招标单位", "招标方", "联系单位", "采购人" };
        /// <summary>
        /// 匹配项目编号
        /// </summary>
        public static readonly string[] CodeRegex = { "工程编号", "项目编号", "招标编号", "中标编号", "编号" };
        /// <summary>
        /// 匹配中标金额
        /// </summary>
        public static readonly string[] MoneyRegex = { "中标（成交）金额人民币", "中标金额为", "预中标金额（元）", "成交金额", "中标总金额", "中标（成交）金额", "中标价", "中标金额", "中标金额（元）", "成交总金额（人民币）", "中标金额（包一）", "投标价", "其中标价为", "投标报价", "预中标人", "总投资", "发包价", "投标报价", "中标标价", "承包标价", "价格", "金额", "总价", "报价" };
        /// <summary>
        /// 匹配项目经理
        /// </summary>
        public static readonly string[] MgrRegex = { "项目经理姓名", "项目经理（或建造师）", "项目经理", "项目负责人", "项目总监", "建造师", "总工程师", "监理师", "建造师（总监）" };
        /// <summary>
        /// 匹配工程地点
        /// </summary>
        public static readonly string[] AddressRegex = { "工程位置", "工程地点", "工程地址", "详细地址", "工程所在地", "地点", "地址" };
        /// <summary>
        /// 匹配日期
        /// </summary>
        public static readonly string[] DateRegex = { "报名开始日期", "报名开始时间", "报名起止日期", "报名起止时间", "开标日期", "开标时间", "发标日期", "发标时间", "发布日期", "发布时间", "获取招标文件日期", "获取招标文件时间", "公示开始时间", "公示结束时间" };
        /// <summary>
        /// 附件后缀名
        /// </summary>
        public static readonly string[] AttachName = { ".doc", ".xls", ".docx", ".xlsx", ".pdf", ".txt", ".rar", ".zip", ".mpp", ".ppt" };

        /// <summary>
        /// 图片后缀名
        /// </summary>
        public static readonly string[] AttachImg = { ".jpg", ".jepg", ".png", ".gif" };

        #endregion
        public static DateTime GetDateTimeByLong(long timeLong)
        {
            System.DateTime times = System.DateTime.MinValue;
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return startTime.AddMilliseconds(timeLong);
        }
        public static long GetDateTimeLong(DateTime dateTime)
        {
            System.DateTime startTimes = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            return (dateTime.Ticks - startTimes.Ticks) / 10000;
        }
        #region html获取
        /// <summary>
        /// post获取网页
        /// </summary>
        /// <param name="url"></param>
        /// <param name="post"></param>
        /// <param name="enc"></param>
        /// <returns></returns>
        public static string GetHtmlByUrlPost(string url, string post, string header, Encoding enc, ref string cookie)
        {
            byte[] byteRequest = Encoding.UTF8.GetBytes(post);

            HttpWebRequest httpWebRequest;
            HttpWebResponse webResponse;
            Stream getStream;

            httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);

            httpWebRequest.UnsafeAuthenticatedConnectionSharing = true;
            httpWebRequest.KeepAlive = false;
            httpWebRequest.AllowAutoRedirect = true;
            // ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            httpWebRequest.ContentType = "application/json";
            //httpWebRequest.Accept = "text/html, application/xhtml+xml, */*";
            //httpWebRequest.Referer = url;
            httpWebRequest.Headers[HttpRequestHeader.Authorization] = header;
            //httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko";
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentLength = byteRequest.Length;
            Stream stream;
            stream = httpWebRequest.GetRequestStream();
            stream.Write(byteRequest, 0, byteRequest.Length);
            stream.Close();
            webResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            getStream = webResponse.GetResponseStream();
            StreamReader streamReader = new StreamReader(getStream, enc);
            string s = streamReader.ReadToEnd();

            streamReader.Close();
            getStream.Close();
            return s;
        }

        /// <summary>
        /// post获取网页
        /// </summary>
        /// <param name="url"></param>
        /// <param name="post"></param>
        /// <param name="enc"></param>
        /// <returns></returns>
        public static string GetHtmlByUrlPost(string url, string post, Encoding enc, ref string cookie)
        {
            byte[] byteRequest = Encoding.UTF8.GetBytes(post);

            HttpWebRequest httpWebRequest;
            HttpWebResponse webResponse;
            Stream getStream;

            httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            CookieContainer co = new CookieContainer();
            co.SetCookies(new Uri(url), cookie);
            httpWebRequest.UnsafeAuthenticatedConnectionSharing = true;
            httpWebRequest.CookieContainer = co;
            httpWebRequest.KeepAlive = false;
            httpWebRequest.AllowAutoRedirect = true;
            ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;//验证服务器证
            // ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            httpWebRequest.Accept = "text/html, application/xhtml+xml, */*";
            httpWebRequest.Referer = url;
            httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko";
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentLength = byteRequest.Length;
            Stream stream;
            stream = httpWebRequest.GetRequestStream();
            stream.Write(byteRequest, 0, byteRequest.Length);
            stream.Close();
            webResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            getStream = webResponse.GetResponseStream();
            StreamReader streamReader = new StreamReader(getStream, enc);
            string s = streamReader.ReadToEnd();

            streamReader.Close();
            getStream.Close();
            return s;
        }

        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public static string GetHtmlGJByUrlPost(string url, string post, Encoding enc, string cookie)
        {
            byte[] byteRequest = Encoding.UTF8.GetBytes(post);

            HttpWebRequest httpWebRequest;
            HttpWebResponse webResponse;
            Stream getStream;

            httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            CookieContainer co = new CookieContainer();
            co.SetCookies(new Uri(url), cookie);
            httpWebRequest.UnsafeAuthenticatedConnectionSharing = true;
            httpWebRequest.ServicePoint.Expect100Continue = false;
            httpWebRequest.CookieContainer = co;
            httpWebRequest.KeepAlive = false;
            httpWebRequest.AllowAutoRedirect = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            httpWebRequest.ContentType = "text/xml";//"application/x-www-form-urlencoded";
            httpWebRequest.Accept =
                "text/html, application/xhtml+xml, */*";
            //httpWebRequest.Referer = url;
            httpWebRequest.UserAgent =
                "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko";
            httpWebRequest.Method = "Post";
            httpWebRequest.ContentLength = byteRequest.Length;
            Stream stream;
            stream = httpWebRequest.GetRequestStream();
            stream.Write(byteRequest, 0, byteRequest.Length);
            stream.Close();
            webResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            //header = webResponse.Headers.ToString();
            getStream = webResponse.GetResponseStream();
            StreamReader streamReader = new StreamReader(getStream, enc);
            string s = streamReader.ReadToEnd();
            //streamReader = new StreamReader(getStream, Encoding.UTF8);
            //getString = streamReader.ReadToEnd();

            streamReader.Close();
            getStream.Close();
            //header = webResponse.Headers.ToString();
            //getStream = webResponse.GetResponseStream();
            //contentLength = webResponse.ContentLength;

            //byte[] outBytes = new byte[2];
            //outBytes = ReadFully(getStream);
            //getStream.Close();
            return s;
        }

        /// <summary>
        /// 湖北黄冈
        /// </summary>
        /// <param name="url"></param>
        /// <param name="post"></param>
        /// <param name="enc"></param>
        /// <param name="cookie"></param>
        /// <returns></returns>
        public static string GetHtmlGJByUrlPost(string url, string post, Encoding enc, ref string cookie)
        {
            byte[] byteRequest = Encoding.Default.GetBytes(post);

            HttpWebRequest httpWebRequest;
            HttpWebResponse webResponse;
            Stream getStream;

            httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            CookieContainer co = new CookieContainer();
            co.SetCookies(new Uri(url), cookie);
            httpWebRequest.UnsafeAuthenticatedConnectionSharing = true;
            httpWebRequest.CookieContainer = co;
            //httpWebRequest.Connection = "Keep-alive";
            httpWebRequest.KeepAlive = true;
            httpWebRequest.AllowAutoRedirect = true;
            //var policy = new HttpRequestCachePolicy(HttpCacheAgeControl.MaxAge,
            //                             TimeSpan.FromMinutes(1));
            //httpWebRequest.CachePolicy = policy;
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            httpWebRequest.ContentType = "text/html; charset=gb2312";
            httpWebRequest.Accept = "*/*";
            httpWebRequest.Host = "www.hgggzy.com";
            httpWebRequest.Headers["Accept-Language"] = "zh-CN,zh;q=0.8,en-US;q=0.5,en;q=0.3";
            httpWebRequest.Headers["Set-Cookies"] = cookie;
            httpWebRequest.Referer = url;
            httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:46.0) Gecko/20100101 Firefox/46.0";

            httpWebRequest.Method = "POST";
            httpWebRequest.ContentLength = byteRequest.Length;
            Stream stream;
            stream = httpWebRequest.GetRequestStream();
            stream.Write(byteRequest, 0, byteRequest.Length);
            stream.Close();
            webResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            webResponse.Cookies = co.GetCookies(httpWebRequest.RequestUri);
            getStream = webResponse.GetResponseStream();

            StreamReader streamReader = new StreamReader(getStream, enc);
            string strHtml = streamReader.ReadToEnd();

            streamReader.Close();
            getStream.Close();
            return strHtml;
        }



        public static string GetHtmlByUrlCookie(string url, Encoding enc, ref string cookie)
        {
            WebRequest wrt;
            wrt = WebRequest.Create(url);
            wrt.Credentials = CredentialCache.DefaultCredentials;
            WebResponse wrp;
            wrp = wrt.GetResponse();
            string html = new StreamReader(wrp.GetResponseStream(), enc).ReadToEnd();
            cookie = wrp.Headers.Get("Set-Cookie");
            return html;
        }
        /// <summary>
        /// 抓取防盗链地址
        /// </summary>
        /// <param name="listHtl">上级地址</param>
        /// <param name="dtlHtl">要打开的地址</param>
        /// <returns></returns>
        public static string GetHtmlByUrl(string listHtl, string dtlHtl)
        {
            string cookie = string.Empty;
            StringBuilder content = new System.Text.StringBuilder();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(dtlHtl);
            request.AllowAutoRedirect = true;
            //建立存储Cookie的对象
            CookieContainer co = new CookieContainer();
            co.SetCookies(new Uri(listHtl), cookie);
            request.CookieContainer = co;
            request.Method = "GET";
            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; Maxthon; .NET CLR 1.1.4322)";
            request.Referer = listHtl;
            //request.Timeout=
            Stream stream = null;
            HttpWebResponse webresp = null;
            try
            {
                webresp = (HttpWebResponse)request.GetResponse();
                stream = webresp.GetResponseStream();
            }
            catch (Exception ex) { ex.ToString(); }
            webresp.Cookies = co.GetCookies(request.RequestUri);
            StreamReader sReader = new StreamReader(stream, Encoding.GetEncoding("utf-8"));
            // 开始读取数据
            Char[] sReaderBuffer = new Char[256];
            int count = sReader.Read(sReaderBuffer, 0, 256);
            while (count > 0)
            {
                String tempStr = new String(sReaderBuffer, 0, count);
                content.Append(tempStr);
                count = sReader.Read(sReaderBuffer, 0, 256);
            }
            // 读取结束
            sReader.Close();
            return content.ToString();
        }

        public static string GetHtmlByUrlEncode(string url, Encoding encode)
        {
            string cookie = string.Empty;
            StringBuilder content = new System.Text.StringBuilder();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AllowAutoRedirect = true;
            //建立存储Cookie的对象
            CookieContainer co = new CookieContainer();
            co.SetCookies(new Uri(url), cookie);
            request.CookieContainer = co;
            request.Method = "GET";
            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; Maxthon; .NET CLR 1.1.4322)";
            request.Timeout = 50000;
            request.KeepAlive = true;
            request.AllowAutoRedirect = false;
            request.Accept = "*/*";
            request.Headers.Add("Accept-Language", "zh-cn");
            request.Headers.Add("Accept-Encoding", "gzip, deflate");

            Stream stream = null;
            HttpWebResponse webresp = null;
            try
            {
                webresp = (HttpWebResponse)request.GetResponse();
                stream = webresp.GetResponseStream();
            }
            catch (Exception ex) { ToolDb.Logger.Error(ex); }
            webresp.Cookies = co.GetCookies(request.RequestUri);
            StreamReader sReader = new StreamReader(stream, encode);
            // 开始读取数据
            Char[] sReaderBuffer = new Char[256];
            int count = sReader.Read(sReaderBuffer, 0, 256);
            while (count > 0)
            {
                String tempStr = new String(sReaderBuffer, 0, count);
                content.Append(tempStr);
                count = sReader.Read(sReaderBuffer, 0, 256);
            }
            // 读取结束
            sReader.Close();
            return content.ToString();
        }




        /// <summary>
        /// 抓取防盗链地址
        /// </summary>
        /// <param name="listUrl">上级地址</param>
        /// <param name="dtlUrl">要打开的地址</param>
        /// <param name="encode">字符编码</param>
        /// <returns></returns>
        public static string GetHtmlByUrl(string listUrl, string dtlUrl, Encoding encode)
        {
            string cookie = string.Empty;
            StringBuilder content = new System.Text.StringBuilder();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(dtlUrl);
            request.AllowAutoRedirect = true;
            //建立存储Cookie的对象
            CookieContainer co = new CookieContainer();
            co.SetCookies(new Uri(listUrl), cookie);
            request.CookieContainer = co;
            request.Method = "GET";
            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; Maxthon; .NET CLR 1.1.4322)";
            request.Referer = listUrl;
            Stream stream = null;
            HttpWebResponse webresp = null;
            try
            {
                webresp = (HttpWebResponse)request.GetResponse();
                stream = webresp.GetResponseStream();
            }
            catch (Exception ex) { ex.ToString(); }
            webresp.Cookies = co.GetCookies(request.RequestUri);
            StreamReader sReader = new StreamReader(stream, encode);
            // 开始读取数据
            Char[] sReaderBuffer = new Char[256];
            int count = sReader.Read(sReaderBuffer, 0, 256);
            while (count > 0)
            {
                String tempStr = new String(sReaderBuffer, 0, count);
                content.Append(tempStr);
                count = sReader.Read(sReaderBuffer, 0, 256);
            }
            // 读取结束
            sReader.Close();
            return content.ToString();
        }

        /// <summary>
        /// 抓取网页，返回htl字符串
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetHtmlByUrl(string url, Encoding encode)
        {
            string strHtml = string.Empty;
            WebClient webClient = new WebClient();
            byte[] reqHTML = webClient.DownloadData(url);
            strHtml = encode.GetString(reqHTML);
            return strHtml;
        }




        /// <summary>
        /// 获取A标签属性值
        /// </summary>
        /// <param name="aTagValue">A标签属性</param>
        /// <param name="strHtml">A标签所在的html</param>
        /// <param name="list">A标签所在的NodeList</param>
        /// <returns></returns>
        public static string GetHtmlAtagValue(string aTagValue, string strHtml = null, NodeList list = null, int i = 0)
        {
            string strReturn = string.Empty;
            if (list != null && list.Count > 0)
            {
                Parser parser = new Parser(new Lexer(list.AsHtml()));
                NodeList sNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                if (sNode != null && sNode.Count > 0)
                {
                    ATag htag = sNode.SearchFor(typeof(ATag), true)[i] as ATag;
                    strReturn = htag.GetAttribute(aTagValue);
                }
            }
            else if (!string.IsNullOrEmpty(strHtml))
            {
                Parser parser = new Parser(new Lexer(strHtml));
                NodeList sNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                if (sNode != null && sNode.Count > 0)
                {
                    ATag htag = sNode.SearchFor(typeof(ATag), true)[i] as ATag;
                    strReturn = htag.GetAttribute(aTagValue);
                }
            }
            return strReturn;
        }
        //重写证书验证方法，总是返回TRUE，解决未能为SSL/TLS安全通道建立信任关系的问题 
        public static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            //总是返回TRUE 
            return true;
        }


        public static string GetHtmlCookieByCer(string url, string post, Encoding encode, string cerFileName, ref string cookie)
        {
            byte[] byteRequest = Encoding.UTF8.GetBytes(post);

            HttpWebRequest httpWebRequest;
            HttpWebResponse webResponse;
            Stream getStream;

            httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            X509Certificate cer = new X509Certificate("D:\\抓取工作\\市交易中心.cer", "11111");
            httpWebRequest.ClientCertificates.Add(cer);
            CookieContainer co = new CookieContainer();
            co.SetCookies(new Uri(url), cookie);
            httpWebRequest.UnsafeAuthenticatedConnectionSharing = true;
            httpWebRequest.CookieContainer = co;
            httpWebRequest.KeepAlive = false;
            httpWebRequest.AllowAutoRedirect = true;
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            httpWebRequest.Accept =
                "text/html, application/xhtml+xml, */*";
            httpWebRequest.Referer = "http://www.bajsjy.com/wsbm/default.aspx";
            httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Trident/7.0; rv:11.0) like Gecko";
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentLength = byteRequest.Length;
            Stream stream;
            stream = httpWebRequest.GetRequestStream();
            stream.Write(byteRequest, 0, byteRequest.Length);
            stream.Close();
            webResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            //header = webResponse.Headers.ToString();
            getStream = webResponse.GetResponseStream();
            StreamReader streamReader = new StreamReader(getStream, encode);
            string s = streamReader.ReadToEnd();
            //streamReader = new StreamReader(getStream, Encoding.UTF8);
            //getString = streamReader.ReadToEnd();

            streamReader.Close();
            getStream.Close();
            //header = webResponse.Headers.ToString();
            //getStream = webResponse.GetResponseStream();
            //contentLength = webResponse.ContentLength;

            //byte[] outBytes = new byte[2];
            //outBytes = ReadFully(getStream);
            //getStream.Close();
            return s;
        }

        public static Span GetHtmlSpan(string strHtml = null, NodeList list = null, int i = 0)
        {
            Span span = null;
            if (list != null && list.Count > 0)
            {
                Parser parser = new Parser(new Lexer(list.AsHtml()));
                NodeList sNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("span"));
                if (sNode != null && sNode.Count > 0)
                {
                    span = sNode.SearchFor(typeof(Span), true)[i] as Span;
                }
            }
            else if (!string.IsNullOrEmpty(strHtml))
            {
                Parser parser = new Parser(new Lexer(strHtml));
                NodeList sNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("span"));
                if (sNode != null && sNode.Count > 0)
                {
                    span = sNode.SearchFor(typeof(Span), true)[i] as Span;
                }
            }
            return span;
        }

        public static TableTag GetHtmlTableTag(string strHtml = null, NodeList list = null, int i = 0)
        {
            TableTag tableTag = null;
            if (list != null && list.Count > 0)
            {
                Parser parser = new Parser(new Lexer(list.AsHtml()));
                NodeList sNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                if (sNode != null && sNode.Count > 0)
                {
                    tableTag = sNode.SearchFor(typeof(TableTag), true)[i] as TableTag;
                }
            }
            else if (!string.IsNullOrEmpty(strHtml))
            {
                Parser parser = new Parser(new Lexer(strHtml));
                NodeList sNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
                if (sNode != null && sNode.Count > 0)
                {
                    tableTag = sNode.SearchFor(typeof(TableTag), true)[i] as TableTag;
                }
            }
            return tableTag;
        }

        /// <summary>
        /// 获取A标签
        /// </summary>
        /// <param name="strHtml">A标签所在HTML</param>
        /// <param name="list">A标签所在NodeList</param>
        /// <returns></returns>
        public static ATag GetHtmlAtag(string strHtml = null, NodeList list = null, int i = 0)
        {
            ATag aTag = null;
            if (list != null && list.Count > 0)
            {
                Parser parser = new Parser(new Lexer(list.AsHtml()));
                NodeList sNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                if (sNode != null && sNode.Count > 0)
                {
                    aTag = sNode.SearchFor(typeof(ATag), true)[i] as ATag;
                }
            }
            else if (!string.IsNullOrEmpty(strHtml))
            {
                Parser parser = new Parser(new Lexer(strHtml));
                NodeList sNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                if (sNode != null && sNode.Count > 0)
                {
                    aTag = sNode.SearchFor(typeof(ATag), true)[i] as ATag;
                }
            }
            return aTag;
        }
        #endregion

        #region 附件下载
        /// <summary>
        /// 附件下载
        /// </summary>
        /// <param name="url">附件下载地址</param>
        /// <param name="fileName">附件原名称</param>
        /// <param name="id">关联主表的Id</param>
        /// <param name="strFileUrl">附件下载到本地的路径</param>
        public static void AddFileAttach(string url, string fileName, string id, string strFileUrl = "SiteManage\\Files\\Attach\\")
        {
            strFileUrl = ToolDb.DbServerPath + strFileUrl;
            string strFile = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + "\\"; //新建文件夹地址 
            long lStartPos = 0;             //返回上次下载字节
            long lCurrentPos = 0;         //返回当前下载文件长度
            long lDownLoadFile;           //返回当前下载文件长度 
            int len = url.IndexOf("=");
            string junName = System.IO.Path.GetExtension(url.Substring(len, url.Length - len).ToLower());
            string attachName = ToolDb.NewGuid + junName;
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
                fs = new FileStream(strFileUrl + strFile + attachName, System.IO.FileMode.OpenOrCreate);
                lStartPos = 0;
            }
            try
            {
                System.Net.HttpWebRequest request = System.Net.HttpWebRequest.Create(url) as System.Net.HttpWebRequest;
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
                    BaseAttach baseInfo = ToolDb.GenBaseAttach(ToolDb.NewGuid, fileName, id, strFile + attachName, length.ToString(), "");
                    ToolDb.SaveEntity(baseInfo, string.Empty);
                }
                else
                {
                    File.Delete(strFileUrl + strFile + fileName);
                }
            }
            catch
            {
                fs.Close();
                File.Delete(strFileUrl + strFile + fileName);
            }
        }

        /// <summary>
        /// 附件下载，返回附件实体
        /// </summary>
        /// <param name="url">附件下载地址</param>
        /// <param name="fileName">附件原名称</param>
        /// <param name="id">关联主表的Id</param>
        /// <param name="strFileUrl">附件下载到本地的路径</param>
        /// <returns></returns>
        public static BaseAttach GetBaseAttach(string url, string fileName, string id, string strFileUrl = "SiteManage\\Files\\Notify_Attach\\")
        {
            strFileUrl = ToolDb.DbServerPath + strFileUrl;
            string strFile = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + "\\"; //新建文件夹地址 
            long lStartPos = 0;
            long lCurrentPos = 0;
            fileName = fileName.ToNodeString();
            string junName = System.IO.Path.GetExtension(url.ToLower().Replace(".aspx", "").Replace(".asp", "").Replace(".html", "").Replace(".htm", "").Replace(".jsp", "").Replace(".php", "").Replace(".ashx", ""));
            if (string.IsNullOrEmpty(junName))
            {
                junName = System.IO.Path.GetExtension(fileName.ToLower().Replace(".aspx", "").Replace(".asp", "").Replace(".html", "").Replace(".htm", "").Replace(".jsp", "").Replace(".php", "").Replace(".ashx", ""));
            }
            if (junName.Contains("&"))
                junName = junName.Remove(junName.IndexOf("&"));
            string attachName = ToolDb.NewGuid + junName;
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
                fs = new FileStream(strFileUrl + strFile + attachName, System.IO.FileMode.OpenOrCreate);
                lStartPos = 0;
            }
            try
            {
                System.Net.HttpWebRequest request = System.Net.HttpWebRequest.Create(url) as System.Net.HttpWebRequest;
                length = request.GetResponse().ContentLength;
                if (lStartPos > 0)
                {
                    request.AddRange((int)lStartPos);
                }
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
                if (length > 1024 || length == -1)
                {
                    BaseAttach baseInfo = ToolDb.GenBaseAttach(ToolDb.NewGuid, fileName, id, strFile + attachName, length.ToString(), string.Empty);
                    return baseInfo;
                }
                else
                {
                    File.Delete(strFileUrl + strFile + fileName);
                }
            }
            catch (Exception ex)
            {
                fs.Close();
                File.Delete(strFileUrl + strFile + attachName);
            }
            return null;
        }

        public static BaseAttach GetBaseAttachByUrl(string url, string fileName, string id, string path = "SiteManage\\Files\\Notify_Attach\\")
        {
            path = ToolDb.DbServerPath + path;
            fileName = fileName.ToNodeString();
            string junName = Path.GetExtension(fileName.ToLower().Replace(".aspx", "").Replace(".asp", "").Replace(".html", "").Replace(".htm", "").Replace(".jsp", "").Replace(".php", "").Replace(".com", "").Replace(".cn", ""));

            if (string.IsNullOrEmpty(junName))
                junName = Path.GetExtension(url.ToLower().Replace(".aspx", "").Replace(".asp", "").Replace(".html", "").Replace(".htm", "").Replace(".jsp", "").Replace(".php", "").Replace(".do?", "").Replace(".com", "").Replace(".cn", ""));

            if (junName.Contains("&"))
                junName = junName.Remove(junName.IndexOf("&"));

            string sqlPath = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + "\\" + Guid.NewGuid() + junName;
            string pathNew = path + sqlPath;
            try
            {
                string u = ToolWeb.UrlEncode(url);
                HttpWebRequest Myrq = (HttpWebRequest)HttpWebRequest.Create(u);
                HttpWebResponse myrp = (HttpWebResponse)Myrq.GetResponse();
                long totalBytes = myrp.ContentLength;
                Stream st = myrp.GetResponseStream();
                FileStream so = new FileStream(pathNew, FileMode.OpenOrCreate);
                long totalDownloadedByte = 0;
                byte[] by = new byte[1024];
                int osize = st.Read(by, 0, (int)by.Length);
                while (osize > 0)
                {
                    totalDownloadedByte = osize + totalDownloadedByte;
                    so.Write(by, 0, osize);
                    osize = st.Read(by, 0, (int)by.Length);
                }
                so.Close();
                st.Close();

                if (totalBytes > 1024 || totalBytes == -1)
                {
                    if (string.IsNullOrEmpty(junName))
                    {
                        try
                        {
                            junName = FileTypeDetector.FileDetector(pathNew);
                            if (!string.IsNullOrEmpty(junName))
                            {
                                fileName += "." + junName;
                            }
                        }
                        catch { }
                    }
                    BaseAttach baseInfo = ToolDb.GenBaseAttach(ToolDb.NewGuid, fileName, id, sqlPath, totalBytes.ToString(), string.Empty);
                    return baseInfo;
                }
                else
                {
                    File.Delete(pathNew);
                }
            }
            catch (Exception ex)
            {
            }
            return null;
        }
        #endregion

        #region 字符串处理

        /// <summary>
        /// 将json转换为DataTable
        /// </summary>
        /// <param name="strJson">得到的json</param>
        /// <returns></returns>
        public static DataTable JsonToDataTable(string strJson)
        {
            //转换json格式
            strJson = strJson.Replace(",", "*").Replace(":", "#").ToString();
            //取出表名   
            var rg = new Regex(@"(?<={)[^:]+(?=:\[)", RegexOptions.IgnoreCase);
            string strName = rg.Match(strJson).Value;
            DataTable tb = null;
            //去除表名   
            strJson = strJson.Substring(strJson.IndexOf("[") + 1);
            strJson = strJson.Substring(0, strJson.IndexOf("]"));

            //获取数据   
            rg = new Regex(@"(?<={)[^}]+(?=})");
            MatchCollection mc = rg.Matches(strJson);
            for (int i = 0; i < mc.Count; i++)
            {
                string strRow = mc[i].Value;
                string[] strRows = strRow.Split('*');
                List<string[]> list = new List<string[]>();

                //创建表   
                if (tb == null)
                {
                    tb = new DataTable();
                    tb.TableName = strName + "tableNew";
                    foreach (string str in strRows)
                    {
                        var dc = new DataColumn();
                        string[] strCell = str.Split('#');

                        if (strCell[0].ToLower().Contains("attachguid") || strCell[0].ToLower().Contains("attachname"))
                        {

                            if (strCell[0].Substring(0, 1) == "\"")
                            {
                                int a = strCell[0].Length;
                                dc.ColumnName = strCell[0].Substring(1, a - 2);
                            }
                            else
                            {
                                dc.ColumnName = strCell[0];
                            }
                            if (tb.Columns.Contains(dc.ColumnName))
                                continue;
                            tb.Columns.Add(dc);
                            list.Add(strCell);
                        }
                        else continue;
                    }
                    tb.AcceptChanges();
                }
                else
                {
                    foreach (string str in strRows)
                    {
                        string[] strCell = str.Split('#');
                        if (strCell[0].ToLower().Contains("attachguid") || strCell[0].ToLower().Contains("attachname"))
                        {
                            list.Add(strCell);
                        }
                    }
                }

                //增加内容   
                DataRow dr = tb.NewRow();
                for (int l = 0; l < list.Count; l++)
                {
                    string[] listRow = list[l];
                    string tempName = listRow[0];
                    string tempValue = listRow[1];
                    dr[tempName] = tempValue;
                }
                //for (int r = 0; r < strRows.Length; r++)
                //{
                //    try
                //    {
                //        string temp = strRows[r].Split('#')[1].Trim();
                //        dr[r] = temp.Replace("，", ",").Replace("：", ":").Replace("\"", "");
                //    }
                //    catch
                //    {

                //    }
                //}
                tb.Rows.Add(dr);
                tb.AcceptChanges();
            }

            return tb;
        }

        public static Dictionary<string, string> JsonToDictionary(string strJson)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            string[] dics = strJson.Split(',');
            foreach (string str in dics)
            {
                string[] jsons = str.Split(':');
                if (jsons.Length >= 2)
                {
                    if (dic.ContainsKey(jsons[0]))
                        continue;
                    dic.Add(jsons[0], jsons[1]);
                }
            }
            return dic;
        }
        public static Dictionary<string, object> JsonToDictionaryObject(string strJson)
        {

            char[] charJson = strJson.ToCharArray();
            List<int> startNum = new List<int>();
            List<int> endNum = new List<int>();
            List<int> endList = new List<int>();
            for (int i = 0; i < charJson.Length; i++)
            {
                if (charJson[i].Equals('{'))
                {
                    startNum.Add(i);
                }
                if (charJson[i].Equals('}'))
                {
                    endNum.Add(i);
                }

            }
            for (int j = endNum.Count - 1; j >= 0; j--)
            {
                endList.Add(endNum[j]);
            }

            Dictionary<string, object> dic = new Dictionary<string, object>();
            string tempJson = strJson;
            int strStartIndex = tempJson.IndexOf("{");
            int strEndIndex = tempJson.LastIndexOf("}");
            int index = 0;
            while (true)
            {
                tempJson = tempJson.Substring(strStartIndex + 1, strEndIndex - (strStartIndex + 1));
                int tempIndex = tempJson.IndexOf("{");
                string tempStr = string.Empty;
                if (tempIndex >= 0)
                {
                    tempStr = tempJson.Substring(0, tempIndex);
                }
                if (index == 0)
                {
                    string[] dics = tempStr.Split(',');
                    foreach (string str in dics)
                    {
                        string[] jsons = str.Split(':');
                        if (jsons.Length == 2)
                        {
                            if (dic.ContainsKey(jsons[0]))
                                continue;
                            dic.Add(jsons[0], jsons[1]);
                        }
                    }
                }
                index++;
            }
            if (startNum.Count > 1)
            {
                for (int i = 1; i < startNum.Count; i++)
                {
                    int endIndex = endList[i];
                    int startIndex = startNum[i];
                    int startIndexTwo = 0;
                    if (startNum.Count > i + 1)
                        startIndexTwo = startNum[i + 1];

                    string strValue = string.Empty;
                    if (startIndexTwo > 0)
                        strValue = strJson.Substring(startIndex, startIndexTwo - startIndex);


                }

            }
            else
            {
                string[] dics = strJson.Split(',');
                foreach (string str in dics)
                {
                    string[] jsons = str.Split(':');
                    if (jsons.Length == 2)
                    {
                        if (dic.ContainsKey(jsons[0]))
                            continue;
                        dic.Add(jsons[0], jsons[1]);
                    }
                }
            }
            return dic;
        }


        /// <summary>
        /// 去除字符串两边括号
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string GetPrjNameByName(string str)
        {
            //if (!string.IsNullOrEmpty(str) && str.Length > 1)
            //{
            //    string strValue = str.Replace("&nbsp;", "").Replace("&nbsp", "").Replace(" ", "");
            //    int beg = strValue.LastIndexOf("(");
            //    int end = strValue.LastIndexOf(")");
            //    if (beg > end)
            //    {
            //        strValue = strValue.Remove(beg, 1);
            //    }
            //    int begQj = strValue.LastIndexOf("（");
            //    int endQj = strValue.LastIndexOf("）");
            //    if (begQj > endQj)
            //    {
            //        strValue = strValue.Remove(begQj, 1);
            //    }
            //    int begZ = strValue.LastIndexOf("[");
            //    int endZ = strValue.LastIndexOf("]");
            //    if (begZ > endZ)
            //    {
            //        strValue = strValue.Remove(begZ, 1);
            //    }
            //    int begZd = strValue.LastIndexOf("【");
            //    int endZx = strValue.LastIndexOf("】");
            //    if (begZd > endZx)
            //    {
            //        strValue = strValue.Remove(begZ, 1);
            //    }
            //    return strValue;
            //}
            return str;
        }

        public static string GetMeetPrjName(string str)
        {
            if (!string.IsNullOrEmpty(str) && str.Length > 1)
            {
                string strValue = str.Replace("&nbsp;", "").Replace("&nbsp", "").Replace("\0", "").Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace(" ", "");
                int beg = strValue.LastIndexOf("(");
                int end = strValue.LastIndexOf(")");
                if (beg > end)
                {
                    strValue += ")";
                }
                int begQj = strValue.LastIndexOf("（");
                int endQj = strValue.LastIndexOf("）");
                if (begQj > endQj)
                {
                    strValue += "）";
                }
                int begZ = strValue.LastIndexOf("[");
                int endZ = strValue.LastIndexOf("]");
                if (begZ > endZ)
                {
                    strValue += "]";
                }
                int begZd = strValue.LastIndexOf("【");
                int endZx = strValue.LastIndexOf("】");
                if (begZd > endZx)
                {
                    strValue += "】";
                }
                return strValue;
            }
            return str;
        }

        /// <summary>
        /// 获取input值
        /// </summary>
        /// <param name="htl"></param>
        /// <param name="inputId"></param>
        /// <returns></returns>
        public static string GetHtmlInputValue(string htl, string name)
        {
            Parser parser = new Parser(new Lexer(htl));
            NodeList value = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("input"), new HasAttributeFilter("name", name)));
            if (value != null && value.Count > 0)
            {
                InputTag inputTag = value.SearchFor(typeof(InputTag), true)[0] as InputTag;
                string str = inputTag.GetAttribute("value");
                return str == null ? string.Empty : str;
            }
            return string.Empty;
        }

        /// <summary>
        /// 获取input值
        /// </summary>
        /// <param name="htl"></param>
        /// <param name="inputId"></param>
        /// <returns></returns>
        public static string GetHtmlInputValueById(string htl, string inputId)
        {
            Parser parser = new Parser(new Lexer(htl));
            NodeList value = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("input"), new HasAttributeFilter("id", inputId)));
            if (value != null && value.Count > 0)
            {
                InputTag inputTag = value.SearchFor(typeof(InputTag), true)[0] as InputTag;
                string str = inputTag.GetAttribute("value");
                return str == null ? string.Empty : str;
            }
            return string.Empty;
        }

        /// <summary>
        /// 字符串长度判断
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="leng">长度</param>
        /// <returns></returns>
        public static string GetSubString(string str, int leng)
        {
            if (!string.IsNullOrEmpty(str))
            {
                if (Encoding.Default.GetByteCount(str) > leng)
                {
                    return string.Empty;
                }
            }
            return str;
        }
        /// <summary>
        /// 替换字符串（例：1包替换为空）
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string GetStringTemp(string str)
        {
            string[] bidStr = new string[] { "第一包","第二包","第三包","第四包","第五包","第六包","第七包","第八包","第九包","第一","第二",
            "第三","第四","第五","第六","第七","第八","第九","第1","第2","盖章","名称","(",")","（","）",
            "第3","第4","第5","第6","第7","第8","第9","1包","2包","3包","4包","5包","6包","7包","8包","9包"};
            for (int i = 0; i < bidStr.Length; i++)
            {
                if (str.Contains(bidStr[i]))
                {
                    str = str.Replace(bidStr[i], "");
                }
            }
            return str;
        }

        /// <summary>
        /// 工程类型，如果为空，则返回“”
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetInviteType(string type)
        {
            if (string.IsNullOrEmpty(type))
            {
                return "";
            }
            return type;
        }

        /// <summary>
        /// 招标、中标类型处理
        /// </summary>
        /// <param name="strValue"></param>
        /// <returns></returns>
        public static string GetInviteTypes(string strValue)
        {
            string strName = string.Empty;
            if (strValue.Contains("施工"))
            {
                strName = "施工";
            }
            if (strValue.Contains("监理"))
            {
                strName = "监理";
            }
            if (strValue.Contains("设计"))
            {
                strName = "设计";
            }
            if (strValue.Contains("勘察"))
            {
                strName = "勘察";
            }
            if (strValue.Contains("服务"))
            {
                strName = "服务";
            }
            if (strValue.Contains("劳务分包"))
            {
                strName = "劳务分包";
            }
            if (strValue.Contains("专业分包"))
            {
                strName = "专业分包";
            }
            if (strValue.Contains("小型施工"))
            {
                strName = "小型工程";
            }
            if (strValue.Contains("设备材料"))
            {
                strName = "设备材料";
            }
            return strName == string.Empty ? "" : strName;
        }

        /// <summary>
        /// 招标类型地址处理
        /// </summary>
        /// <param name="strValue"></param>
        /// <returns></returns>
        public static string GetAddress(string strValue)
        {
            if (Encoding.Default.GetByteCount(strValue) > 150)
            {
                return "见招标信息";
            }
            else if (string.IsNullOrEmpty(strValue))
            {
                return "见招标信息";
            }
            return strValue;
        }

        /// <summary>
        /// 清空文本的换行符和空格
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetString(string value)
        {
            return value.ToLower().Replace(" ", "").Replace("\t", "").Replace("\n", "").Replace("\r", "").Replace("&nbsp;", "").Replace("&nbsp", "");
        }

        #endregion

        #region 正则匹配处理

        /// <summary>
        /// 正则匹配
        /// </summary>
        /// <param name="strValue">匹配的字符串</param>
        /// <param name="strName">要匹配的开始字符</param>
        /// <param name="isMon">是否需要冒号匹配</param>
        /// <returns></returns>
        public static string GetRegexString(string strValue, string[] strName, bool isMon = true, string lastStr = "\r\n")
        {
            Regex reg = null;
            string values = string.Empty, names = string.Empty;
            for (int i = 0; i < strName.Length; i++)
            {
                if (i == strName.Length - 1)
                    names += strName[i];
                else
                    names += strName[i] + "|";
            }
            if (isMon)
            {
                reg = new Regex(@"(" + names + ")(:|：)[^" + lastStr + "]+" + lastStr);
            }
            else
            {
                reg = new Regex(@"(" + names + ")(:|：|)[^" + lastStr + "]+" + lastStr);
            }
            values = reg.Match(strValue).Value.Replace("：", "").Replace(":", "").Replace("\r", "").Replace("\t", "").Replace("\n", "").Replace("，", "").Replace(",", "").Replace("；", "").Replace(";", "");
            for (int j = 0; j < strName.Length; j++)
            {
                values = values.Replace(strName[j], "");
            }
            return values.Replace("。", "").Replace("，", "");
        }

        /// <summary>
        /// 正则匹配
        /// </summary>
        /// <param name="strValue">匹配的字符串</param>
        /// <param name="strName">要匹配的开始字符</param>
        /// <param name="isMon">是否需要冒号匹配</param>
        /// <returns></returns>
        public static string GetRegexStrings(string strValue, string[] strName, bool isMon = true)
        {
            Regex reg = null;
            string values = string.Empty, names = string.Empty;
            for (int i = 0; i < strName.Length; i++)
            {
                if (i == strName.Length - 1)
                    names += strName[i];
                else
                    names += strName[i] + "|";
            }
            if (isMon)
            {
                reg = new Regex(@"(" + names + ")(:|：)[^\r\n]+\r\n");
            }
            else
            {
                reg = new Regex(@"(" + names + ")(:|：|)[^\r\n]+\r\n");
            }
            values = reg.Match(strValue).Value;
            for (int j = 0; j < strName.Length; j++)
            {
                values = values.Replace(strName[j], "");
            }

            return values.Replace("。", "").Replace("，", "").Replace("：", "").Replace(":", "").Replace("\r", "").Replace("\t", "").Replace("\n", "").Replace("（", "").Replace("）", "").Replace("(", "").Replace(")", "").Replace("；", "").Replace(";", "");
        }

        /// <summary>
        /// 正则匹配
        /// </summary>
        /// <param name="strValue">匹配的字符串</param>
        /// <param name="strName">要匹配的开始字符</param>
        /// <param name="isMon">是否需要冒号匹配</param>
        /// <returns></returns>
        public static string GetRegexStringNot(string strValue, string[] strName, bool isMon = true)
        {
            Regex reg = null;
            string values = string.Empty, names = string.Empty;
            for (int i = 0; i < strName.Length; i++)
            {
                if (i == strName.Length - 1)
                    names += strName[i];
                else
                    names += strName[i] + "|";
            }
            if (isMon)
            {
                reg = new Regex(@"(" + names + ")(:|：)[^\r\n]+\r\n");
            }
            else
            {
                reg = new Regex(@"(" + names + ")(:|：|)[^\r\n]+\r\n");
            }
            values = reg.Match(strValue).Value;
            for (int j = 0; j < strName.Length; j++)
            {
                values = values.Replace(strName[j], "");
            }

            return values.Replace("。", "").Replace("，", "").Replace("：", "").Replace(":", "").Replace("\r", "").Replace("\t", "").Replace("\n", "").Replace("；", "").Replace(";", "");
        }
        /// <summary>
        /// 正则匹配
        /// </summary>
        /// <param name="strValue">匹配的字符串</param>
        /// <param name="strName">要匹配的开始字符</param>
        /// <param name="isMon">是否需要冒号匹配</param>
        /// <param name="isMon">字符串长度</param>
        /// <returns></returns>
        public static string GetRegexString(string strValue, string[] strName, bool isMon, int leng)
        {
            Regex reg = null;
            string values = string.Empty, names = string.Empty;
            for (int i = 0; i < strName.Length; i++)
            {
                if (i == strName.Length - 1)
                    names += strName[i];
                else
                    names += strName[i] + "|";
            }
            if (isMon)
            {
                reg = new Regex(@"(" + names + ")(:|：)[^\r\n]+\r\n");
            }
            else
            {
                reg = new Regex(@"(" + names + ")(:|：|)[^\r\n]+\r\n");
            }
            values = reg.Match(strValue.Replace(" ", "")).Value.Replace("：", "").Replace(":", "").Replace("\r", "").Replace("\t", "").Replace("\n", "").Replace("（", "").Replace("）", "").Replace("(", "").Replace(")", "").Replace("，", "").Replace(",", "").Replace("；", "").Replace(";", "");
            for (int j = 0; j < strName.Length; j++)
            {
                values = values.Replace(strName[j], "");
            }
            if (Encoding.Default.GetByteCount(values) > leng)
            {
                values = string.Empty;
            }
            return values;
        }

        /// <summary>
        /// 使用正则匹配两个字符之间的字符串
        /// </summary>
        /// <param name="strValue">匹配的字符串</param>
        /// <param name="strBegin">开始的匹配字符</param>
        /// <param name="strEnd">结束的匹配字符</param>
        /// <returns></returns>
        public static string GetRegexString(string strValue, string strBegin, string strEnd)
        {
            Regex reg = new Regex("(?<=(" + strBegin + "))[.\\s\\S]*?(?=(" + strEnd + "))", RegexOptions.Multiline | RegexOptions.Singleline);

            return reg.Match(strValue.Replace(" ", "")).Value.Replace(strBegin, "").Replace(strEnd, "");
        }

        /// <summary>
        /// 使用正则匹配两个字符之间的字符串
        /// </summary>
        /// <param name="strValue">匹配的字符串</param>
        /// <param name="strBegin">开始的匹配字符</param>
        /// <param name="strEnd">结束的匹配字符</param>
        /// <param name="leng">匹配的字符串长度</param>
        /// <returns></returns>
        public static string GetRegexString(string strValue, string strBegin, string strEnd, int leng)
        {
            Regex reg = new Regex("(?<=(" + strBegin + "))[.\\s\\S]*?(?=(" + strEnd + "))", RegexOptions.Multiline | RegexOptions.Singleline);
            string value = reg.Match(strValue.Replace(" ", "")).Value.Replace(strBegin, "").Replace(strEnd, "");
            if (Encoding.Default.GetByteCount(value) > leng)
            {
                value = string.Empty;
            }
            return value;
        }

        /// <summary>
        /// 金额处理
        /// </summary>
        /// <param name="strMoney">金额</param>
        /// <returns></returns>
        public static string GetRegexMoney(string strMoney, string mon = "万")
        {
            bool isMon = true;
            string strValue = string.Empty;
            string[] str = mon.Split(',');
            Regex regBidMoney = new Regex(@"[0-9]+[.]{0,1}[0-9]+");
            for (int i = 0; i < str.Length; i++)
            {
                if (strMoney.Contains(str[i]))
                {
                    //bidMoney = bidMoney.Remove(bidMoney.IndexOf("万")).Trim();
                    strValue = regBidMoney.Match(strMoney).Value;
                    isMon = false;
                    break;
                }
            }
            if (isMon)
            {
                try
                {
                    strValue = (decimal.Parse(regBidMoney.Match(strMoney).Value) / 10000).ToString();
                    if (decimal.Parse(strValue) < decimal.Parse("0.1"))
                    {
                        strValue = "0";
                    }
                }
                catch
                {
                    strValue = "0";
                }
            }
            return strValue;
        }

        /// <summary>
        /// 日期匹配
        /// </summary>
        /// <param name="strValue"></param>
        /// <param name="isType"></param>
        /// <returns></returns>
        public static string GetRegexDateTime(string strValue, string isType = "yyyy-MM-dd")
        {
            string value = string.Empty;
            Regex regDate = null;
            switch (isType)
            {
                case "yyyy-MM-dd":
                    regDate = new Regex(@"\d{4}-\d{1,2}-\d{1,2}");
                    value = regDate.Match(strValue).Value;
                    break;
                case "yyyy年MM月dd日":
                    regDate = new Regex(@"\d{4}年\d{1,2}月\d{1,2}日");
                    value = regDate.Match(strValue).Value;
                    break;
                case "yyyyMMdd":
                    regDate = new Regex(@"\d{4} \d{1,2} \d{1,2}");
                    value = regDate.Match(strValue).Value;
                    break;
                case "yy-MM-dd":
                    regDate = new Regex(@"\d{2}-\d{1,2}-\d{1,2}");
                    value = regDate.Match(strValue).Value;
                    break;
                case "MM-dd":
                    regDate = new Regex(@"\d{1,2}-\d{1,2}");
                    value = regDate.Match(strValue).Value;
                    break;
                case "yyyy-MM":
                    regDate = new Regex(@"\d{4}-\d{1,2}");
                    value = regDate.Match(strValue).Value;
                    break;
                case "yyyy/MM/dd":
                    regDate = new Regex(@"\d{4}/\d{1,2}/\d{1,2}");
                    value = regDate.Match(strValue).Value;
                    break;
                default:
                    break;
            }
            return value;
        }

        /// <summary>
        /// 去掉字符串中的script和style和xml
        /// </summary>
        /// <param name="strValue"></param>
        /// <returns></returns>
        public static string GetRegexHtlTxt(string strValue)
        {
            Regex reg = new Regex(@"<script[^<]*</script>|<\?xml[^/]*/>|<style[^<]*</style>|<xml[^<]*</xml>");
            return reg.Replace(strValue.ToLower(), "");
        }
        #endregion

        /// <summary>
        /// 将查询字符串解析转换为名值集合.
        /// </summary>
        /// <param name="queryString"></param>
        /// <returns></returns>
        public static NameValueCollection GetQueryString(string queryString)
        {
            return GetQueryString(queryString, null, true);
        }

        /// <summary>
        /// 将查询字符串解析转换为名值集合.
        /// </summary>
        /// <param name="queryString"></param>
        /// <param name="encoding"></param>
        /// <param name="isEncoded"></param>
        /// <returns></returns>
        public static NameValueCollection GetQueryString(string queryString, Encoding encoding, bool isEncoded)
        {
            queryString = queryString.Replace("?", "");
            NameValueCollection result = new NameValueCollection(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrEmpty(queryString))
            {
                int count = queryString.Length;
                for (int i = 0; i < count; i++)
                {
                    int startIndex = i;
                    int index = -1;
                    while (i < count)
                    {
                        char item = queryString[i];
                        if (item == '=')
                        {
                            if (index < 0)
                            {
                                index = i;
                            }
                        }
                        else if (item == '&')
                        {
                            break;
                        }
                        i++;
                    }
                    string key = null;
                    string value = null;
                    if (index >= 0)
                    {
                        key = queryString.Substring(startIndex, index - startIndex);
                        value = queryString.Substring(index + 1, (i - index) - 1);
                    }
                    else
                    {
                        key = queryString.Substring(startIndex, i - startIndex);
                    }
                    if (isEncoded)
                    {
                        result[MyUrlDeCode(key, encoding)] = MyUrlDeCode(value, encoding);
                    }
                    else
                    {
                        result[key] = value;
                    }
                    if ((i == (count - 1)) && (queryString[i] == '&'))
                    {
                        result[key] = string.Empty;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 解码URL.
        /// </summary>
        /// <param name="encoding">null为自动选择编码</param>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string MyUrlDeCode(string str, Encoding encoding)
        {
            if (encoding == null)
            {
                Encoding utf8 = Encoding.UTF8;
                //首先用utf-8进行解码                     
                string code = HttpUtility.UrlDecode(str.ToUpper(), utf8);
                //将已经解码的字符再次进行编码.
                string encode = HttpUtility.UrlEncode(code, utf8).ToUpper();
                if (str == encode)
                    encoding = Encoding.UTF8;
                else
                    encoding = Encoding.GetEncoding("gb2312");
            }
            return HttpUtility.UrlDecode(str, encoding);
        }
    }
}
