using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Crawler.Instance;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;using Crawler.Base.KdService;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Tags;
using Winista.Text.HtmlParser.Util;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.Web;

namespace Crawler.Instance
{
    public class WebAutoLogin
    {
        /// <SUMMARY>   
        /// 构建一个httt请求以获取目标链接的cookies,需要传入目标的登录地址和相关的post信息,返回完成登录的cookies,以及返回的            html内容   
        /// </SUMMARY>   
        /// <PARAM name="url">登录页面的地址</PARAM>   
        /// <PARAM name="post">post信息</PARAM>   
        /// <PARAM name="strHtml">输出的html代码</PARAM>   
        /// <PARAM name="rppt">请求的标头所需要的相关属性设置</PARAM>   
        /// <RETURNS>请求完成后的cookies</RETURNS>   
        public static CookieCollection funGetCookie(string url, string postParam, out string strHtml)
        {
            byte[] post = GetPostParam(postParam);
            CookieCollection ckclReturn = new CookieCollection();
            CookieContainer cc = new CookieContainer();
            HttpWebRequest hwRequest;
            HttpWebResponse hwResponse;
            //请求cookies的格式   
            //hwRequest = (HttpWebRequest)HttpWebRequest.Create(new Uri(url));   
            //hwResponse = (HttpWebResponse)hwRequest.GetResponse();   
            //string cookie = hwResponse.Headers.Get("Set-Cookie");   
            //cookie = cookie.Split(';')[0];   
            //hwRequest = null;   
            //hwResponse = null;   
            //构建即将发送的包头   
            //cc.SetCookies(new Uri(server), cookie);              
            hwRequest = (HttpWebRequest)HttpWebRequest.Create(new Uri(url));
            hwRequest.CookieContainer = cc;
            hwRequest.Accept = "text/html, application/xhtml+xml, */*";
            //hwRequest.Headers.Add(HttpRequestHeader.AcceptLanguage, "zh-CN");
            //hwRequest.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
            //hwRequest.Headers.Add(HttpRequestHeader.CacheControl, "no-cache");
            
            //hwRequest.Referer = "https://wwsso.szjs.gov.cn:8084/wwsso/login";
            //hwRequest.Host = "wwsso.szjs.gov.cn:8084";
            //hwRequest.Connection = "Keep-Alive";
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
            hwRequest.ContentType = "application/x-www-form-urlencoded";
            hwRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0; QQBrowser/7.7.28658.400) like Gecko";
            hwRequest.Method = "POST";
            hwRequest.ContentLength = post.Length;
            //写入标头   
            Stream stream;
            stream = hwRequest.GetRequestStream();
            stream.Write(post, 0, post.Length);
            stream.Close();
            //发送请求获取响应内容   
            try
            {
                hwResponse = (HttpWebResponse)hwRequest.GetResponse();
            }
            catch
            {
                strHtml = "";
                return ckclReturn;
            }
            stream = hwResponse.GetResponseStream();
            StreamReader sReader = new StreamReader(stream, Encoding.UTF8);
            strHtml = sReader.ReadToEnd();
            sReader.Close();
            stream.Close();
            //获取缓存内容   
            ckclReturn = hwResponse.Cookies;
            return ckclReturn;
        }

        /// 根据已经获取的有效cookies来获取目标链接的内容   
        /// </SUMMARY>   
        /// <PARAM name="strUri">目标链接的url</PARAM>   
        /// <PARAM name="ccl">已经获取到的有效cookies</PARAM>   
        /// <PARAM name="rppt">头属性的相关设置</PARAM>   
        /// <RETURNS>目标连接的纯文本:"txt/html"</RETURNS>   
        public static string funGetHtmlByCookies(string strUri, CookieCollection ccl)
        {
            CookieContainer cc = new CookieContainer();
            HttpWebRequest hwRequest;
            HttpWebResponse hwResponse;

            //构建即将发送的包头          
            hwRequest = (HttpWebRequest)HttpWebRequest.Create(new Uri(strUri));
            cc.Add(ccl);
            hwRequest.CookieContainer = cc;
            
            hwRequest.Accept = "text/html, application/xhtml+xml, */*";
            ////hwRequest.Headers.Add(HttpRequestHeader.AcceptLanguage, "zh-CN");
            ////hwRequest.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
            ////hwRequest.Headers.Add(HttpRequestHeader.CacheControl, "no-cache"); 
            ////hwRequest.Host = "wwsso.szjs.gov.cn:8084";
            ////hwRequest.Connection = "Keep-Alive";
            hwRequest.UnsafeAuthenticatedConnectionSharing = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
            hwRequest.ContentType = "application/x-www-form-urlencoded";
            hwRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0; QQBrowser/7.7.28658.400) like Gecko";
            hwRequest.Method = "GET";
            hwRequest.ContentLength = 0;

            //发送请求获取响应内容   
            try
            {
                hwResponse = (HttpWebResponse)hwRequest.GetResponse();
            }
            catch
            {
                return "";
            }

            Stream stream;
            stream = hwResponse.GetResponseStream();
            StreamReader sReader = new StreamReader(stream, Encoding.UTF8);
            string strHtml = sReader.ReadToEnd();
            sReader.Close();
            stream.Close();

            //返回值             
            return strHtml;
        }
        /// <SUMMARY>   
        /// 根据已经获取的有效cookies来获取目标链接的内容   
        /// </SUMMARY>   
        /// <PARAM name="strUri">目标链接的url</PARAM>   
        ///<PARAM name="post">post的byte信息</PARAM>   
        /// <PARAM name="ccl">已经获取到的有效cookies</PARAM>   
        /// <PARAM name="rppt">头属性的相关设置</PARAM>   
        /// <RETURNS>目标连接的纯文本:"txt/html"</RETURNS>   
        public static string funGetHtmlByCookies(string strUri, string postParam, CookieCollection ccl)
        {
            byte[] post = GetPostParam(postParam);
            CookieContainer cc = new CookieContainer();
            HttpWebRequest hwRequest;
            HttpWebResponse hwResponse;

            //构建即将发送的包头          
            hwRequest = (HttpWebRequest)HttpWebRequest.Create(new Uri(strUri));
            cc.Add(ccl);
            hwRequest.CookieContainer = cc;
            hwRequest.Accept = "text/html, application/xhtml+xml, */*";
            hwRequest.ContentType = "application/x-www-form-urlencoded";
            hwRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0; QQBrowser/7.7.28658.400) like Gecko";
            hwRequest.Method = "POST /wwsso/login HTTP/1.1";
            hwRequest.ContentLength = post.Length;
            //写入post信息   
            Stream stream;
            stream = hwRequest.GetRequestStream();
            stream.Write(post, 0, post.Length);
            stream.Close();
            //发送请求获取响应内容   
            try
            {
                hwResponse = (HttpWebResponse)hwRequest.GetResponse();
            }
            catch
            {
                return "";
            }

            stream = hwResponse.GetResponseStream();
            StreamReader sReader = new StreamReader(stream, Encoding.Default);
            string strHtml = sReader.ReadToEnd();
            sReader.Close();
            stream.Close();

            //返回值             
            return strHtml;
        }

        private static byte[] GetPostParam(string paramList)
        {
            StringBuilder UrlEncoded = new StringBuilder();
            //对参数进行encode     
            Char[] reserved = { '?', '=', '&' };
            byte[] SomeBytes = null;
            if (paramList != null)
            {
                int i = 0, j;
                while (i < paramList.Length)
                {
                    j = paramList.IndexOfAny(reserved, i);
                    if (j == -1)
                    {
                        UrlEncoded.Append(HttpUtility.UrlEncode(paramList.Substring(i, paramList.Length - i)));
                        break;
                    }
                    UrlEncoded.Append(HttpUtility.UrlEncode(paramList.Substring(i, j - i)));
                    UrlEncoded.Append(paramList.Substring(j, 1));
                    i = j + 1;
                }
                SomeBytes = Encoding.UTF8.GetBytes(UrlEncoded.ToString());
            }
            return SomeBytes;
        }
    }
}
