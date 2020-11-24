using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Crawler.Instance
{
    /// <summary>
    /// Http客户端接口
    /// </summary>
    public interface IWebHttpClient : IDisposable
    {
        /// <summary>
        /// Get HTTP请求
        /// </summary>
        /// <typeparam name="TResult">返回结果类型</typeparam>
        /// <param name="url">请求地址</param> 
        /// <param name="paramData">请求参数</param>
        /// <param name="authorizationToken">验证Token</param>
        /// <param name="authorizationMethod">验证方法</param>
        TResult GetSync<TResult>(Uri url, string authorizationToken = null, string authorizationMethod = "Basic") where TResult : class, new();
        /// <summary>
        ///  Get HTTP请求
        /// </summary>
        /// <param name="url">请求地址</param> 
        /// <param name="paramData">请求参数</param>
        /// <param name="authorizationToken">验证Token</param>
        /// <param name="authorizationMethod">验证方法</param>
        /// <returns></returns>
        string GetSync(Uri url, string authorizationToken = null, string authorizationMethod = "Basic");
        /// <summary>
        ///  Post HTTP请求
        /// </summary>
        /// <typeparam name="TResult">返回结果类型</typeparam>
        /// <param name="url">请求地址</param> 
        /// <param name="paramData">请求参数</param>
        /// <param name="authorizationToken">验证Token</param>
        /// <param name="authorizationMethod">验证方法</param>
        /// <returns></returns>
        TResult PostSync<TResult>(Uri url, string paramData = null, string authorizationToken = null, string authorizationMethod = "Basic") where
            TResult : class, new();
        /// <summary>
        ///  Post HTTP请求
        /// </summary>
        /// <param name="url">请求地址</param> 
        /// <param name="paramData">请求参数</param>
        /// <param name="authorizationToken">验证Token</param>
        /// <param name="authorizationMethod">验证方法</param>
        /// <returns></returns>
        string PostSync(Uri url, string paramData = null, string authorizationToken = null, string authorizationMethod = "Basic");
    }

    /// <summary>
    /// HTTP接口调用
    /// </summary>
    public class WebHttpClient : IWebHttpClient
    {
        /// <summary>
        /// 当前HttpClient
        /// </summary>
        public readonly HttpClient HttpClient;
        /// <summary>
        /// 实例化WebHttpClient
        /// </summary>
        public WebHttpClient()
        {
            this.HttpClient = new HttpClient();
        }
        /// <summary>
        /// 获取HttpResponseMessage响应结果
        /// </summary>
        /// <typeparam name="TResult">响应的结果</typeparam>
        /// <param name="responseMessage">Response</param>
        /// <returns></returns>
        private TResult GetResponseResult<TResult>(HttpResponseMessage responseMessage)
             where TResult : class, new()
        {
            if (responseMessage.IsSuccessStatusCode)
            {
                string contentString = responseMessage.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<TResult>(contentString);
            }

            if (responseMessage.StatusCode == HttpStatusCode.BadRequest)
                throw new Exception("数据异常");

            if (responseMessage.StatusCode == HttpStatusCode.InternalServerError)
                throw new Exception("您当前网络已断开，请网络正常后再试");

            if (responseMessage.StatusCode == HttpStatusCode.Unauthorized)
                throw new Exception("用户未授权或者授权已经过期，请重新登录。");

            throw new Exception("您当前网络已断开，请网络正常后再试");
        }
        /// <summary>
        /// 获取HttpResponseMessage响应字符串
        /// </summary>
        /// <param name="responseMessage">Response</param>
        /// <returns></returns>
        private string GetResponseResult(HttpResponseMessage responseMessage)
        {
            if (responseMessage.IsSuccessStatusCode)
                return responseMessage.Content.ReadAsStringAsync().Result;

            if (responseMessage.StatusCode == HttpStatusCode.BadRequest)
                throw new Exception("数据异常");

            if (responseMessage.StatusCode == HttpStatusCode.InternalServerError)
                throw new Exception("您当前网络已断开，请网络正常后再试");

            if (responseMessage.StatusCode == HttpStatusCode.Unauthorized)
                throw new Exception("用户未授权或者授权已经过期，请重新登录。");

            throw new Exception("您当前网络已断开，请网络正常后再试");
        }
        /// <summary>
        /// 获取Header的Content参数
        /// </summary>
        /// <param name="paramData">参数值</param>
        /// <param name="encoding">编码，默认UTF8</param>
        /// <param name="mediaType">参数MediaType，默认application/json</param>
        /// <returns></returns>
        private StringContent GetStringContent(string paramData = null, Encoding encoding = null, string mediaType = "application/json")
        {
            encoding = encoding ?? Encoding.UTF8;
            StringContent content = new StringContent(paramData, encoding, mediaType);
            content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
            return content;
        }
        /// <summary>
        /// 获取Request
        /// </summary> 
        /// <param name="url">请求地址</param>
        /// <param name="method">请求HTTP方法</param>
        /// <param name="paramData">请求参数</param>
        /// <param name="authorizationToken">验证Token</param>
        /// <param name="authorizationMethod">验证方法</param>
        /// <returns></returns>
        private HttpRequestMessage GetHttpRequestMessage(Uri url, HttpMethod method = null, string paramData = null, string authorizationToken = null, string authorizationMethod = "Basic")
        {
            method = method ?? HttpMethod.Get;
            HttpRequestMessage requestMessage = new HttpRequestMessage(method, url);

            if (!string.IsNullOrWhiteSpace(paramData))
                requestMessage.Content = this.GetStringContent(paramData);
            if (authorizationToken != null)
                //requestMessage.Headers.Authorization = new AuthenticationHeaderValue(authorizationMethod, authorizationToken);
                requestMessage.Headers.Add("Authorization", authorizationToken);

            return requestMessage;
        }
        /// <summary>
        /// HTTP请求
        /// </summary>
        /// <typeparam name="TResult">返回的结果类型</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="method">请求HTTP方法</param>
        /// <param name="paramData">请求参数</param>
        /// <param name="authorizationToken">验证Token</param>
        /// <param name="authorizationMethod">验证方法</param>
        /// <returns></returns>
        private TResult HttpSync<TResult>(Uri url, HttpMethod method = null, string paramData = null, string authorizationToken = null, string authorizationMethod = "Basic")
             where TResult : class, new()
        {
            if (url.ToString().ToLower().StartsWith("https"))
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            HttpRequestMessage requestMessage = this.GetHttpRequestMessage(url, method, paramData, authorizationToken, authorizationMethod);

            HttpResponseMessage response = this.HttpClient.SendAsync(requestMessage).Result;
            return this.GetResponseResult<TResult>(response);
        }
        /// <summary>
        /// HTTP请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="method">请求HTTP方法</param>
        /// <param name="paramData">请求参数</param>
        /// <param name="authorizationToken">验证Token</param>
        /// <param name="authorizationMethod">验证方法</param>
        /// <returns></returns>
        private string HttpSync(Uri url, HttpMethod method = null, string paramData = null, string authorizationToken = null, string authorizationMethod = "Basic")
        {
            if (url.ToString().ToLower().StartsWith("https"))
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;

            HttpRequestMessage requestMessage = this.GetHttpRequestMessage(url, method, paramData, authorizationToken, authorizationMethod); 
            HttpResponseMessage response = this.HttpClient.SendAsync(requestMessage).Result;
            return this.GetResponseResult(response);
        }
        /// <summary>
        /// Get HTTP请求
        /// </summary>
        /// <param name="url">请求地址</param> 
        /// <param name="paramData">请求参数</param>
        /// <param name="authorizationToken">验证Token</param>
        /// <param name="authorizationMethod">验证方法</param>
        /// <returns></returns>
        public string GetSync(Uri url, string authorizationToken = null, string authorizationMethod = "Basic")
        {
            return this.HttpSync(url, HttpMethod.Get, null, authorizationToken, authorizationMethod);
        }
        /// <summary>
        /// Get HTTP请求
        /// </summary>
        /// <typeparam name="TResult">返回结果类型</typeparam>
        /// <param name="url">请求地址</param> 
        /// <param name="paramData">请求参数</param>
        /// <param name="authorizationToken">验证Token</param>
        /// <param name="authorizationMethod">验证方法</param>
        /// <returns></returns>
        public TResult GetSync<TResult>(Uri url, string authorizationToken = null, string authorizationMethod = "Basic")
             where TResult : class, new()
        {
            return this.HttpSync<TResult>(url, HttpMethod.Get, null, authorizationToken, authorizationMethod);
        }
        /// <summary>
        /// Post HTTP请求
        /// </summary>
        /// <param name="url">请求地址</param> 
        /// <param name="paramData">请求参数</param>
        /// <param name="authorizationToken">验证Token</param>
        /// <param name="authorizationMethod">验证方法</param>
        /// <returns></returns>
        public string PostSync(Uri url, string paramData = null, string authorizationToken = null, string authorizationMethod = "Basic")
        {
            return this.HttpSync(url, HttpMethod.Post, paramData, authorizationToken, authorizationMethod);
        }
        /// <summary>
        /// Post HTTP请求
        /// </summary>
        /// <typeparam name="TResult">返回结果</typeparam>
        /// <param name="url">请求地址</param> 
        /// <param name="paramData">请求参数</param>
        /// <param name="authorizationToken">验证Token</param>
        /// <param name="authorizationMethod">验证方法</param>
        /// <returns></returns>
        public TResult PostSync<TResult>(Uri url, string paramData = null, string authorizationToken = null, string authorizationMethod = "Basic")
        where TResult : class, new()
        {
            return this.HttpSync<TResult>(url, HttpMethod.Post, paramData, authorizationToken, authorizationMethod);
        }
        /// <summary>
        /// 释放非托管资源
        /// </summary>
        public void Dispose()
        {
            if (this.HttpClient != null)
                this.HttpClient.Dispose();
        }

    }
}
