using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Crawler.Instance
{
    public class ToolSocket
    {
        /// <summary>
        /// Url结构
        /// </summary>
        struct UrlInfo
        {
            public string Host;
            public int Port;
            public string File;
            public string Body;
        }

        /// <summary>
        /// 解析URL
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static UrlInfo ParseURL(string url)
        {
            UrlInfo urlInfo = new UrlInfo();
            string[] strTemp = null;
            urlInfo.Host = "";
            urlInfo.Port = 80;
            urlInfo.File = "/";
            urlInfo.Body = "";
            int intIndex = url.ToLower().IndexOf("http://");
            if (intIndex != -1)
            {
                url = url.Substring(7);
                intIndex = url.IndexOf("/");
                if (intIndex == -1)
                {
                    urlInfo.Host = url;
                }
                else
                {
                    urlInfo.Host = url.Substring(0, intIndex);
                    url = url.Substring(intIndex);
                    intIndex = urlInfo.Host.IndexOf(":");
                    if (intIndex != -1)
                    {
                        strTemp = urlInfo.Host.Split(':');
                        urlInfo.Host = strTemp[0];
                        int.TryParse(strTemp[1], out urlInfo.Port);
                    }
                    intIndex = url.IndexOf("?");
                    if (intIndex == -1)
                    {
                        urlInfo.File = url;
                    }
                    else
                    {
                        strTemp = url.Split('?');
                        urlInfo.File = strTemp[0];
                        urlInfo.Body = strTemp[1];
                    }
                }
            }
            return urlInfo;
        }

        /// <summary>
        /// 发出请求并获取响应
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="body"></param>
        /// <param name="encode"></param>
        /// <returns></returns>
        private static string GetResponse(string host, int port, string body, Encoding encode)
        {
            string strResult = string.Empty;
            byte[] bteSend = Encoding.ASCII.GetBytes(body);
            byte[] bteReceive = new byte[1024];
            int intLen = 0;

            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                try
                {
                    socket.Connect(host, port);
                    if (socket.Connected)
                    {
                        socket.Send(bteSend, bteSend.Length, 0);
                        while ((intLen = socket.Receive(bteReceive, bteReceive.Length, 0)) > 0)
                        {
                            strResult += encode.GetString(bteReceive, 0, intLen);
                        }
                    }
                    socket.Close();
                }
                catch (Exception ex){
                    ToolDb.Logger.Error(ex);
                }
            }

            return strResult;
        }
        /// <summary>
        /// GET请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="encode"></param>
        /// <returns></returns>
        public static string Get(string url, Encoding encode)
        {
            UrlInfo urlInfo = ParseURL(url);
            string strRequest = string.Format("GET {0}?{1} HTTP/1.1\r\nHost:{2}:{3}\r\nConnection:Close\r\n\r\n", urlInfo.File, urlInfo.Body, urlInfo.Host, urlInfo.Port.ToString());
            return GetResponse(urlInfo.Host, urlInfo.Port, strRequest, encode);
        }

        /// <summary>
        /// POST请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="encode"></param>
        /// <returns></returns>
        public static string Post(string url, Encoding encode)
        {
            UrlInfo urlInfo = ParseURL(url);
            string strRequest = string.Format("POST {0} HTTP/1.1\r\nHost:{1}:{2}\r\nContent-Length:{3}\r\nContent-Type:application/x-www-form-urlencoded\r\nConnection:Close\r\n\r\n{4}", urlInfo.File, urlInfo.Host, urlInfo.Port.ToString(), urlInfo.Body.Length, urlInfo.Body);
            return GetResponse(urlInfo.Host, urlInfo.Port, strRequest, encode);
        }

    }
}
