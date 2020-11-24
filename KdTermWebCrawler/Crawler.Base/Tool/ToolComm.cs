using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using System.IO;
using System.Windows.Forms;
using Crawler.Base.KdService;

namespace Crawler
{
    public class ToolComm
    {
        private static log4net.ILog _logger;
        /// <summary>
        /// 日志记录对象
        /// </summary>
        protected static log4net.ILog Logger
        {
            get
            {
                if (_logger == null)
                    _logger = log4net.LogManager.GetLogger(typeof(ToolComm));
                return _logger;
            }
        }

        #region 抓取配置的保存和加载
        #region 序列化
        /// <summary>
        /// 序列化对象集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="pXMLFilePath"></param>
        protected static void Serialize<T>(List<T> list, string pXMLFilePath)
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
        protected static List<T> Deserialize<T>(string pXMLFilePath)
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

        private static Dictionary<string, Type> dictTypeCrawllers;
        /// <summary>
        /// 获取程序运行目录中所有程序集包含的抓取程序类型
        /// </summary> 
        protected static Dictionary<string, Type> DictTypeCrawllers
        {
            get
            {
                if (dictTypeCrawllers != null) return dictTypeCrawllers;

                #region 初始化获取抓取程序类型

                dictTypeCrawllers = new Dictionary<string, Type>();
                Type baseCrawllerType = typeof(WebSiteCrawller);
                string[] files = null;

                try
                {
                    files = Directory.GetFiles(System.Environment.CurrentDirectory, "*.*", SearchOption.AllDirectories);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }

                if (files != null && files.Length > 0)
                {
                    foreach (string fileItem in files)
                    {
                        if (string.IsNullOrEmpty(fileItem))
                            continue;

                        try
                        {
                            Assembly ab = Assembly.LoadFile(fileItem);
                            if (ab != null)
                            {
                                Type[] types = ab.GetTypes();
                                foreach (Type tp in types)
                                {
                                    if (tp != null && tp.BaseType != null
                                        && tp.BaseType == baseCrawllerType)
                                    {
                                        try
                                        {
                                            dictTypeCrawllers.Add(tp.FullName, tp);
                                        }
                                        catch (Exception ex)
                                        {
                                            Logger.Error(ex);
                                        }
                                    }
                                }
                            }
                        }
                        catch (FileLoadException ex)
                        {
                            string msg = string.Format("发现一个未能加载的文件: {0}。", fileItem);
                        }
                        catch (BadImageFormatException ex)
                        {
                            string msg = string.Format("无效或无法识别的程序集: {0}。", fileItem);
                        }
                        catch (Exception ex)
                        {
                            string msg = string.Format("发现一个未能加载的文件: {0}。", fileItem);
                            Logger.Error(msg);
                            Logger.Error(ex);
                        }
                    }
                }
                #endregion

                return dictTypeCrawllers;
            }
        }

        private static object objLockCrawlConfigs = new object();
        /// <summary>
        /// 站点配置保存XML路径
        /// </summary>
        protected static string WebSitesXmlPath
        {
            get { return Path.Combine(System.Environment.CurrentDirectory, "WebSites.xml"); }
        }

        /// <summary>
        /// 保存抓取程序类型设定
        /// </summary>
        /// <param name="tree"></param>
        public static void SaveConfigs(TreeView tree)
        {
            lock (objLockCrawlConfigs)
            {
                List<WebSiteCrawlConfig> configs = new List<WebSiteCrawlConfig>();

                foreach (TreeNode node in tree.Nodes)
                {
                    GetNodeConfigs(node, configs);
                }

                configs = configs.OrderBy(x => x.Group).ToList();
                Serialize<WebSiteCrawlConfig>(configs, WebSitesXmlPath);
            }
        }

        /// <summary>
        /// 保存抓取程序类型设定
        /// </summary>
        /// <param name="tree"></param>
        public static void LoadConfigs(TreeView tree)
        {
            lock (objLockCrawlConfigs)
            {
                List<WebSiteCrawlConfig> configs = Deserialize<WebSiteCrawlConfig>(WebSitesXmlPath);
                if (configs != null && configs.Count > 0)
                {
                    Dictionary<string, WebSiteCrawlConfig> dict = new Dictionary<string, WebSiteCrawlConfig>();
                    foreach (WebSiteCrawlConfig item in configs)
                    {
                        dict.Add(item.Key, item);
                    }
                    foreach (TreeNode node in tree.Nodes)
                    {
                        SetNodeConfigs(node, dict);
                    }
                }
            }
        }

        protected static void GetNodeConfigs(TreeNode node, List<WebSiteCrawlConfig> configs)
        {
            WebSiteCrawller crawller = node.Tag as WebSiteCrawller;
            if (crawller != null)
                configs.Add(crawller.Config);
            if (node.Nodes.Count > 0)
            {
                foreach (TreeNode child in node.Nodes)
                {
                    GetNodeConfigs(child, configs);
                }
            }
        }

        protected static void SetNodeConfigs(TreeNode node, Dictionary<string, WebSiteCrawlConfig> configs)
        {
            WebSiteCrawller crawller = node.Tag as WebSiteCrawller;
            if (crawller != null && !string.IsNullOrEmpty(crawller.Key))
            {
                if (configs.ContainsKey(crawller.Key))
                {
                    WebSiteCrawlConfig config = configs[crawller.Key];
                    if (config != null && !string.IsNullOrEmpty(config.Key))
                    {
                        crawller.Config = config;
                        UpdateNodeConfigs(node, false);
                    }
                }
            }
            if (node.Nodes.Count > 0)
            {
                foreach (TreeNode child in node.Nodes)
                {
                    SetNodeConfigs(child, configs);
                }
            }
        }

        public static void UpdateNodeConfigs(TreeNode node, bool updateChilds)
        {
            WebSiteCrawller crawller = node.Tag as WebSiteCrawller;
            if (crawller != null)
            {
                node.Text = crawller.Title;
                node.Name = crawller.Key;
                node.ToolTipText = crawller.Description;
                node.Checked = crawller.Enabled;
                node.ForeColor = crawller.Enabled ? Color.Blue : Color.Gray;
            }
            if (node.Nodes.Count > 0)
            {
                foreach (TreeNode child in node.Nodes)
                {
                    UpdateNodeConfigs(child, updateChilds);
                }
            }
        }

        /// <summary>
        /// 初始化抓取程序类型设定
        /// </summary>
        public static void InitCrawller(TreeView tree)
        {
            if (tree.Nodes.Count > 0)
                SaveConfigs(tree);

            tree.Nodes.Clear();

            Dictionary<string, Type> dictCrawllers = ToolComm.DictTypeCrawllers;

            DateTime dt = DateTime.Now;

            foreach (string typeFullName in dictCrawllers.Keys)
            {
                Type crawllerType = dictCrawllers[typeFullName];
                WebSiteCrawller crawller = Assembly.GetAssembly(crawllerType).CreateInstance(typeFullName) as WebSiteCrawller;

                if (!string.IsNullOrEmpty(crawller.Title))
                {
                    string groupName = string.IsNullOrEmpty(crawller.Group) ? "其他" : crawller.Group;
                    TreeNode[] tn_arr = tree.Nodes.Find(groupName, true);
                    TreeNode group = null;

                    if (tn_arr != null && tn_arr.Length > 0)
                    {
                        group = tn_arr[0];
                    }
                    else
                    {
                        group = new TreeNode();
                        group.Text = groupName;
                        group.Name = groupName;
                        group.Checked = true;

                        tree.Nodes.Add(group);
                    }

                    if (group != null)
                    {
                        TreeNode node = new TreeNode();
                        node.Text = crawller.Title;
                        node.Name = typeFullName;
                        node.ToolTipText = crawller.Description;
                        node.Checked = crawller.Enabled;
                        node.ForeColor = crawller.Enabled ? Color.Blue : Color.Gray;
                        node.Tag = crawller;

                        group.Nodes.Add(node);
                    }
                }
            }

            LoadConfigs(tree);

            tree.ExpandAll();
        }
        #endregion

        /// <summary>
        /// 得到某个类的抓取时间
        /// </summary> 
        public static string GetCrawlerInfo(WebSiteCrawller crawller)
        {
            if (crawller == null) return string.Empty;

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("抓取说明：{0}", crawller.Description);
            sb.AppendLine();
            sb.AppendFormat("抓取程序：{0}", crawller.Key);
            sb.AppendLine();
            sb.AppendFormat("上次抓取：{0:yyyy-MM-dd HH:mm:ss}", crawller.LastCrawlEnd);
            sb.AppendLine();
            sb.AppendFormat("计划时间：{0}", crawller.PlanTime.Replace(",", ", "));
            sb.AppendLine();
            sb.AppendFormat("抓取数量：最近 {0} 条", crawller.MaxCount);
            sb.AppendLine();
            sb.AppendFormat("抓取地址：{0}", crawller.SiteUrl);
            sb.AppendLine();

            return sb.ToString();
        }

        protected static void GetNodeCrawllers(TreeNode node, Dictionary<string, WebSiteCrawller> crawllers)
        {
            if (node.Checked)
            {
                WebSiteCrawller crawller = node.Tag as WebSiteCrawller;
                if (crawller != null)
                    crawllers.Add(crawller.Key, crawller);

                if (node.Nodes.Count > 0)
                {
                    foreach (TreeNode child in node.Nodes)
                    {
                        GetNodeCrawllers(child, crawllers);
                    }
                }
            }
        }

        /// <summary>
        /// 获取程序运行目录中所有程序集包含的抓取程序类型
        /// </summary> 
        public static Dictionary<string, WebSiteCrawller> GetEnabledCrawllers(TreeView tree)
        {
            Dictionary<string, WebSiteCrawller> crawllers = new Dictionary<string, WebSiteCrawller>();
            foreach (TreeNode child in tree.Nodes)
            {
                GetNodeCrawllers(child, crawllers);
            }
            return crawllers;
        }

        /// <summary>
        /// 根据反射，从实体中调用方法，得到返回结果
        /// </summary>
        /// <param name="assemblyPath"></param>
        /// <param name="entityFullName"></param>
        /// <param name="crawlAll">是否抓取所有数据</param>
        /// <returns></returns>
        public static string DealEntity(WebSiteCrawller crawller)
        {
            StringBuilder result = new StringBuilder();
            try
            { 
                //处理信息
                IList infoList = crawller.Crawl(false);
                if (infoList != null && infoList.Count > 0)
                {
                    object[] successList;
                    List<BaseAttach> tattachList = crawller.AttachList;

                    int count = ToolCoreDb.SaveDatas(infoList, crawller.ExistCompareFields, tattachList, out successList, crawller.ExistsUpdate, crawller.ExistsHtlCtx, crawller.ExistsUpdateAttach);
                    result.Append("【").Append(crawller.Title).Append("】信息【").Append(count).Append("/").Append(infoList.Count).Append("】条；");
                    Base.KdService.CrawlerService ser = new Base.KdService.CrawlerService();
                    int resultCount;
                    //处理附件  
                    List<BaseAttach> newAttachList = new List<BaseAttach>();
                    if (crawller.ExistsUpdateAttach)
                    {
                        if (infoList != null && infoList.Count > 0)
                        {
                            List<BaseAttach> attch = null;
                            foreach (var item in infoList)
                            {
                                Type types = item.GetType();
                                string id = types.GetProperty("Id").GetValue(item, null).ToString();
                                attch = tattachList.FindAll(a => a.SourceID == id);
                                for (int i = 0; i < attch.Count; i++)
                                {
                                    newAttachList.Add(attch[i]);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (successList != null && successList.Length > 0)
                        {
                            List<BaseAttach> attch = null;
                            foreach (var item in successList)
                            {
                                Type types = item.GetType();
                                string id = types.GetProperty("Id").GetValue(item, null).ToString();
                                attch = tattachList.FindAll(a => a.SourceID == id);
                                for (int i = 0; i < attch.Count; i++)
                                {
                                    newAttachList.Add(attch[i]);
                                }
                            }
                        }
                    }

                    count = ToolCoreDb.SaveDatas(newAttachList, "SourceID,AttachServerPath");
                    result.Append("附件【").Append(count).Append("/").Append(tattachList.Count).Append("】条；");
                    crawller.AttachList.Clear();
                }
                else
                {
                    result.Append("【").Append(crawller.Title).Append("】信息【").Append("0").Append("/").Append("0").Append("】条；");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                result.Remove(0, result.Length);
                result.Append("抓取【").Append(crawller.Title).Append("】出现异常，详见日志文件！");
            }

            return result.ToString();
        }

        /// <summary>
        /// 拼接字符串，每个值之间用回车换行符分隔
        /// </summary>
        /// <param name="names"></param>
        /// <param name="values"></param>
        /// <param name="splitStr"></param>
        /// <returns></returns>
        public static string SpliceString(string[] names, string[] values, string splitStr)
        {
            StringBuilder sb = new StringBuilder();
            if (string.IsNullOrEmpty(splitStr)) splitStr = "：";
            if (names != null && values != null && names.Length == values.Length)
            {
                for (int i = 0; i < names.Length; i++)
                {
                    sb.Append(names[i]).Append(splitStr).Append(values[i]).Append("\r\n");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// 对字符串进行处理，去掉一些特殊字符
        /// </summary>
        /// <param name="oldString"></param>
        /// <returns></returns>
        public static string DealString(string oldString)
        {
            if (string.IsNullOrEmpty(oldString)) return string.Empty;
            return oldString.Replace("&nbsp;", " ").Replace("<?xml:namespace prefix = o ns = \"urn:schemas-microsoft-com:office:office\" />", string.Empty).Replace("<?xml:namespace prefix = st1 ns = \"urn:schemas-microsoft-com:office:smarttags\" />", string.Empty).Replace("&#8220;", "“").Replace("&#8221;", "”");
        }
    }

    public delegate object AsyncMethodCaller(WebSiteCrawller crawller);
}
