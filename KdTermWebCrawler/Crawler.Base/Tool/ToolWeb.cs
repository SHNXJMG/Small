using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using System.Collections.Specialized;
using System.Web;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Tags;
using Winista.Text.HtmlParser;
using Winista.Text.HtmlParser.Lex;
using System.Threading;

namespace Crawler
{
    public class ToolWeb
    {
        private static log4net.ILog _logger;
        /// <summary>
        /// 日志记录对象
        /// </summary>
        public static log4net.ILog Logger
        {
            get
            {
                if (_logger == null)
                    _logger = log4net.LogManager.GetLogger(typeof(ToolComm));
                return _logger;
            }
        }

        /// <summary>
        /// 根据url得到网页的html内容
        /// </summary>
        /// <param name="url">网页URL链接地址</param>
        /// <param name="encode">网页采用的编码格式</param>
        /// <returns></returns>
        public static string GetHtmlByUrl(string url, Encoding encode = null)
        {
            url = url.Replace("\r\n", "").Replace("\r", "").Replace("\n", "").Replace("\t", "");
            string html = string.Empty;
            if (encode == null)
                encode = Encoding.UTF8;
            try
            {
                WebClient myWebClient = GetDefaultWebClient(false);
                myWebClient.Encoding = encode;
                html = myWebClient.DownloadString(url);
            }
            catch (Exception ex)
            {
                html = ex.ToString();
                Logger.Error("url==>" + url + "  错误信息：" + ex.ToString());
                throw ex;
            }
            return html;
        }
        /// <summary>
        /// 根据url得到网页的html内容
        /// </summary>
        /// <param name="url"></param>
        /// <param name="encode"></param>
        /// <returns></returns>
        public static string GetHtmlByUrl(string url, Encoding encode, string basic)
        {
            url = url.Replace("\r\n", "").Replace("\r", "").Replace("\n", "").Replace("\t", "");
            string html = string.Empty;
            if (encode == null)
                encode = Encoding.UTF8;
            try
            {
                WebClient myWebClient = GetDefaultWebClient(false, basic);
                myWebClient.Encoding = encode;
                html = myWebClient.DownloadString(url);
            }
            catch (Exception ex)
            {
                html = ex.ToString();
                Logger.Error("url==>" + url + "  错误信息：" + ex.ToString());
                throw ex;
            }
            return html;
        }
        /// <summary>
        /// 根据url得到网页的html内容
        /// </summary>
        /// <param name="url"></param>
        /// <param name="encode"></param>
        /// <returns></returns>
        public static string GetHtmlByUrl(string url, Encoding encode, ref string cookiestr)
        {
            url = url.Replace("\r\n", "").Replace("\r", "").Replace("\n", "").Replace("\t", "");
            string html = string.Empty;
            //cookiestr = string.Empty;
            try
            {
                WebClient myWebClient = GetDefaultWebClient(false);
                myWebClient.Encoding = encode;
                if (!string.IsNullOrEmpty(cookiestr))
                {
                    myWebClient.Headers[HttpRequestHeader.Cookie] = cookiestr;
                }
                html = myWebClient.DownloadString(url);
                if (string.IsNullOrEmpty(cookiestr))
                {
                    cookiestr = myWebClient.ResponseHeaders[HttpResponseHeader.SetCookie];
                }
            }
            catch (Exception ex)
            {
                html = ex.ToString();
                Logger.Error("url==>" + url + "  错误信息：" + ex.ToString());
            }

            return html;
        }

        /// <summary>
        /// 通过post方式，根据url得到页面的html内容
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postValues"></param>
        /// <returns></returns>
        public static string GetHtmlByUrl(string url, NameValueCollection postValues)
        {
            return GetHtmlByUrl(url, postValues, Encoding.UTF8);
        }

        /// <summary>
        /// 通过post方式，根据url得到页面的html内容
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postValues"></param>
        /// <param name="encode"></param>
        /// <returns></returns>
        public static string GetHtmlByUrl(string url, NameValueCollection postValues, Encoding encode)
        {
            return GetHtmlByUrl(url, postValues, encode, false);
        }
        /// <summary>
        /// 通过post方式，根据url得到页面的html内容
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postValues"></param>
        /// <param name="encode"></param>
        /// <param name="onlyGetAjaxCtx"></param>
        /// <returns></returns>
        public static string GetHtmlByUrl(string url, NameValueCollection postValues, Encoding encode, bool onlyGetAjaxCtx)
        {
            string cookies = string.Empty;
            return GetHtmlByUrl(url, postValues, encode, onlyGetAjaxCtx, null, ref cookies);
        }

        /// <summary>
        /// 通过post方式，根据url得到页面的html内容
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postValues"></param>
        /// <param name="encode"></param>
        /// <returns></returns>
        public static string GetHtmlByUrl(string url, NameValueCollection postValues, Encoding encode, ref string cookiestr)
        {
            return GetHtmlByUrl(url, postValues, encode, false, null, ref cookiestr);
        }

        /// <summary>
        /// 通过post方式，根据url得到页面的html内容
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postValues"></param>
        /// <param name="encode"></param>
        /// <param name="onlyGetAjaxCtx"></param>
        /// <param name="cookies"></param>
        /// <returns></returns>
        public static string GetHtmlByUrl(string url, NameValueCollection postValues, Encoding encode, bool onlyGetAjaxCtx, string bais, ref string cookies)
        {
            string html = string.Empty;
            url = url.Replace("\r\n", "").Replace("\r", "").Replace("\n", "").Replace("\t", "");
            WebClient myWebClient = GetDefaultWebClient(onlyGetAjaxCtx, bais);

            if (!string.IsNullOrEmpty(cookies))
            {
                myWebClient.Headers[HttpRequestHeader.Cookie] = cookies;
            }

            try
            {
                byte[] byte1 = myWebClient.UploadValues(url, "POST", postValues);
                if (string.IsNullOrEmpty(cookies))
                {
                    cookies = myWebClient.ResponseHeaders[HttpResponseHeader.SetCookie];
                }
                html = encode.GetString(byte1);
            }
            catch (Exception ex)
            {
                html = ex.ToString();
                Logger.Error("url==>" + url + "  错误信息：" + ex.ToString());
                throw ex;
            }

            return html;
        }

        /// <summary>
        /// 通过post方式，根据url得到页面的html内容
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postValues"></param>
        /// <param name="encode"></param>
        /// <param name="onlyGetAjaxCtx"></param>
        /// <param name="cookies"></param>
        /// <returns></returns>
        public static string GetHtmlByUrlAndIsError(string url, NameValueCollection postValues, Encoding encode, ref bool isError)
        {
            string html = string.Empty;
            url = url.Replace("\r\n", "").Replace("\r", "").Replace("\n", "").Replace("\t", "");
            WebClient myWebClient = GetDefaultWebClient(false);
            string cookies = string.Empty;
            if (!string.IsNullOrEmpty(cookies))
            {
                myWebClient.Headers[HttpRequestHeader.Cookie] = cookies;
            }

            try
            {
                byte[] byte1 = myWebClient.UploadValues(url, "POST", postValues);
                if (string.IsNullOrEmpty(cookies))
                {
                    cookies = myWebClient.ResponseHeaders[HttpResponseHeader.SetCookie];
                }
                html = encode.GetString(byte1);
                isError = false;
            }
            catch (Exception ex)
            {
                html = ex.ToString();
                Logger.Error("url==>" + url + "  错误信息：" + ex.ToString());
                isError = true;
                throw ex;
            }

            return html;
        }
        /// <summary>
        /// 得到默认的WebClient对象，系统自动填充了Headers信息
        /// </summary>
        /// <param name="onlyGetAjaxCtx">如果是ajax方式，是否只得到局部刷新的内容</param>
        /// <returns></returns>
        public static WebClient GetDefaultWebClient(bool onlyGetAjaxCtx, string Basic = null)
        {
            WebClient myWebClient = new WebClient();

            if (!string.IsNullOrEmpty(Basic))
                myWebClient.Headers.Add(HttpRequestHeader.Authorization, "Basic " + Basic);

            myWebClient.Headers.Add("Accept", "*/*");
            myWebClient.Headers.Add("Accept-Language", "zh-cn");

            myWebClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            if (onlyGetAjaxCtx) myWebClient.Headers.Add("x-microsoftajax", "Delta=true");
            myWebClient.Headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0)");
            myWebClient.Credentials = CredentialCache.DefaultCredentials;
            //myWebClient.Headers[HttpRequestHeader.Referer] = "http://localhost:7434/Home/Login";

            return myWebClient;
        }

        /// <summary>
        /// 得到NameValueCollection对象
        /// </summary>
        /// <param name="names"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static NameValueCollection GetNameValueCollection(string[] names, string[] values)
        {
            NameValueCollection postValues = new NameValueCollection();
            if (names != null && values != null && names.Length == values.Length)
            {
                for (int i = 0; i < names.Length; i++)
                {
                    postValues.Add(names[i], values[i]);
                }
            }

            return postValues;
        }

        /// <summary>
        /// 对url进行编码
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string UrlEncode(string url)
        {
            return UrlEncode(url, Encoding.Default);
        }

        /// <summary>
        /// 对url进行编码
        /// </summary>
        /// <param name="url"></param>
        /// <param name="enc"></param>
        /// <returns></returns>
        public static string UrlEncode(string url, Encoding enc)
        {
            string sb = string.Empty;
            if (url == null) url = string.Empty;
            if (url.IndexOf("?") != -1)
            {
                sb = url.Substring(0, url.IndexOf("?") + 1);
                string endUrl = url.Substring(url.IndexOf("?") + 1);
                if (!string.IsNullOrEmpty(endUrl))
                {
                    string[] urlArray = endUrl.Split(new string[] { "&" }, StringSplitOptions.RemoveEmptyEntries);
                    if (urlArray != null && urlArray.Length > 0)
                    {
                        foreach (string urlOne in urlArray)
                        {
                            if (!string.IsNullOrEmpty(urlOne) && urlOne.IndexOf("=") != -1)
                            {
                                if (!sb.EndsWith("?")) sb = sb + "&";
                                sb += urlOne.Substring(0, urlOne.IndexOf("=") + 1) + HttpUtility.UrlEncode(urlOne.Substring(urlOne.IndexOf("=") + 1), enc);
                            }
                        }
                    }
                }
            }
            else
            {
                sb = url;
            }

            return sb;
        }

        /// <summary>
        /// 得到asp.net页面中的viewState值
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string GetAspNetViewState(string html)
        {
            Parser parser = new Parser(new Lexer(html));
            return GetAspNetViewState(parser);
        }

        /// <summary>
        /// 得到asp.net页面中的viewState值
        /// </summary>
        /// <param name="parser"></param>
        /// <returns></returns>
        public static string GetAspNetViewState(Parser parser)
        {
            string viewState = string.Empty;
            parser.Reset();
            NodeList viewNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("input"), new HasAttributeFilter("name", "__VIEWSTATE")));
            if (viewNodes != null && viewNodes.Count > 0)
            {
                InputTag viewTag = (InputTag)viewNodes[0];
                viewState = viewTag.GetAttribute("value");
            }
            return viewState;
        }

        /// <summary>
        /// 得到asp.net页面中的eventValidation值
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string GetAspNetEventValidation(string html)
        {
            Parser parser = new Parser(new Lexer(html));
            return GetAspNetEventValidation(parser);
        }

        /// <summary>
        /// 得到asp.net页面中的eventValidation值
        /// </summary>
        /// <param name="parser"></param>
        /// <returns></returns>
        public static string GetAspNetEventValidation(Parser parser)
        {
            string validataion = string.Empty;
            parser.Reset();
            NodeList viewNodes = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("input"), new HasAttributeFilter("name", "__EVENTVALIDATION")));
            if (viewNodes != null && viewNodes.Count > 0)
            {
                InputTag viewTag = (InputTag)viewNodes[0];
                validataion = viewTag.GetAttribute("value");
            }
            return validataion;
        }

        /// <summary>
        /// 得到asp.net页面中数据翻页提交POST数据所用到的NameValueCollection
        /// </summary>
        /// <param name="viewState"></param>
        /// <param name="eventTarget"></param>
        /// <param name="eventArgument"></param>
        /// <returns></returns>
        public static NameValueCollection GetAspNetNameValueCollection(string viewState, string eventTarget, string eventArgument)
        {
            return GetNameValueCollection(new string[] { "__VIEWSTATE", "__EVENTTARGET", "__EVENTARGUMENT" },
                new string[] { viewState, eventTarget, eventArgument });
        }
    }
}
