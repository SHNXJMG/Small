using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Crawler.Instance
{
    /// <summary>
    /// 模拟登陆类
    /// </summary>
    public class HttpLogin
    {
        /// <summary>
        /// 获取html源码
        /// </summary>
        /// <param name="URL">url地址</param>
        /// <param name="encoding">编码格式</param>
        /// <returns></returns>
        public static string GetHtmlByUrl(string URL,Encoding encoding)
        {
            WebRequest wrt;
            wrt = WebRequest.Create(URL);
            wrt.Credentials = CredentialCache.DefaultCredentials;
            WebResponse wrp;
            wrp = wrt.GetResponse();
            return new StreamReader(wrp.GetResponseStream(), encoding).ReadToEnd();
        }
        /// <summary>
        /// 获取html源码，并读取Cookie
        /// </summary>
        /// <param name="URL">url地址</param>
        /// <param name="encoding">编码格式</param>
        /// <param name="cookie">cookie</param>
        /// <returns></returns>
        public static string GetHtmlByUrl(string URL, Encoding encoding,out string cookie)
        {
            WebRequest wrt;
            wrt = WebRequest.Create(URL);
            wrt.Credentials = CredentialCache.DefaultCredentials;
            WebResponse wrp; 
            wrp = wrt.GetResponse(); 
            string html = new StreamReader(wrp.GetResponseStream(), encoding).ReadToEnd();
            cookie = wrp.Headers.Get("Set-Cookie");
            return html;
        }
        /// <summary>
        /// post获取html源码
        /// </summary>
        /// <param name="URL">需获取的地址</param>
        /// <param name="serverUrl">服务器域名</param>
        /// <param name="postData">post参数</param>
        /// <param name="cookie">cookie</param>
        /// <param name="header">header</param>
        /// <returns></returns>
        public static string GetHtmlByUrl(string URL,string serverUrl,string refere, string postData,Encoding encod, string cookie, out string header)
        {
            byte[] byteRequest = Encoding.UTF8.GetBytes(postData);
            return GetHtmlByUrl(serverUrl, URL,refere, byteRequest, encod,  cookie, out header);
        } 
        /// <summary>
        /// post获取html源码
        /// </summary>
        /// <param name="server">服务器域名</param>
        /// <param name="URL">需获取的地址</param>
        /// <param name="byteRequest">byteRequest参数</param>
        /// <param name="cookie">cookie</param>
        /// <param name="header">header</param>
        /// <returns></returns>
        public static string GetHtmlByUrl(string server, string URL, string refere,byte[] byteRequest, Encoding encod, string cookie, out string header)
        {
            return GetHtmlByBytes(server, URL, refere,encod, byteRequest, cookie, out header); 
        }
        public static string GetHtmlByBytes(string server, string URL,string refere, Encoding enc,byte[] byteRequest, string cookie,out string header)
        {
            HttpWebRequest httpWebRequest;
            HttpWebResponse webResponse;
            Stream getStream;

            httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(URL);
            CookieContainer co = new CookieContainer();
            co.SetCookies(new Uri(server), cookie);
            httpWebRequest.UnsafeAuthenticatedConnectionSharing = true;
            httpWebRequest.CookieContainer = co;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            httpWebRequest.Accept =
                "text/html, application/xhtml+xml, */*";
            httpWebRequest.Referer = refere;
            httpWebRequest.UserAgent =
                "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0; QQBrowser/7.7.28658.400) like Gecko";
            httpWebRequest.Method = "Post";
            httpWebRequest.ContentLength = byteRequest.Length;
            Stream stream;
            stream = httpWebRequest.GetRequestStream();
            stream.Write(byteRequest, 0, byteRequest.Length);
            stream.Close();
            webResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            header = webResponse.Headers.ToString();
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
        public static byte[] ReadFully(Stream stream)
        {
            byte[] buffer = new byte[128];
            using (MemoryStream ms = new MemoryStream())
            {
                while (true)
                {
                    int read = stream.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                        return ms.ToArray();
                    ms.Write(buffer, 0, read);
                }
            }
        }
        public static string GetHtml(string URL,string server,string Referer, Encoding enc, string cookie, out string header)
        {
            return GetHtml(URL, cookie, Referer,enc, out header, server);
        }
        public static string GetHtml(string URL, string cookie,string Referer, Encoding enc, out string header, string server)
        {
            HttpWebRequest httpWebRequest;
            HttpWebResponse webResponse;
            Stream getStream;
            StreamReader streamReader;
            string getString = "";
            httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(URL);
 
            httpWebRequest.Referer = Referer;
            CookieContainer co = new CookieContainer();
            co.SetCookies(new Uri(server), cookie);
            httpWebRequest.Headers.Add("Cookie:" + cookie);

            httpWebRequest.CookieContainer = co;

            httpWebRequest.UnsafeAuthenticatedConnectionSharing = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            httpWebRequest.Accept =
                "text/html, application/xhtml+xml, */*"; 
            httpWebRequest.UserAgent =
                "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0; QQBrowser/7.7.28658.400) like Gecko";
             
            httpWebRequest.Method = "GET";
            webResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            header = webResponse.Headers.ToString();
            getStream = webResponse.GetResponseStream();
            streamReader = new StreamReader(getStream, enc);
            getString = streamReader.ReadToEnd();
            
            streamReader.Close();
            getStream.Close();
            return getString;
        }
    }
}
