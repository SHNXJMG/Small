using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Crawler.Base.KdService;

namespace Crawler
{
    [Serializable]
    public abstract class WebSiteCrawller
    {
        private log4net.ILog _logger;
        /// <summary>
        /// 日志记录对象
        /// </summary>
        public log4net.ILog Logger
        {
            get
            {
                if (_logger == null)
                    _logger = log4net.LogManager.GetLogger(this.GetType());
                return _logger;
            }
        }

        private WebSiteCollection _WebCollection;
        /// <summary>
        /// Web网页抓取工具
        /// </summary>
        public WebSiteCollection ToolWebSite
        {
            get
            {
                if (_WebCollection == null)
                    _WebCollection = new WebSiteCollection(this);
                return _WebCollection;
            }
        }

        /// <summary>
        /// 默认构造方法，初始化部分默认值
        /// </summary>
        public WebSiteCrawller()
        {
            this.MaxCount = 20;
            this.MaxEndTime = 5;
            this.PlanTime = "1:00,9:00,09:05,09:25,09:50,11:30,14:05,14:25,14:50,16:50,19:00";
            this.ExistCompareFields = "InfoUrl";
        }

        /// <summary>
        /// 默认构造方法，初始化部分默认值
        /// </summary>
        /// <param name="config">抓取程序配置项</param>
        public WebSiteCrawller(WebSiteCrawlConfig config)
            : this()
        {
            this.Config = config;
        }
        /// <summary>
        /// 获取或设置当前抓取每次采集网页暂停时间（单位：秒）
        /// </summary>
        public virtual int MaxEndTime
        {
            get { return Config.MaxEndTime; }
            set { Config.MaxEndTime = value; }
        }

        /// <summary>
        /// 默认构造方法，初始化部分默认值
        /// </summary>
        /// <param name="disabled">是否禁用</param>
        public WebSiteCrawller(bool disabled)
            : this()
        {
            this.Disabled = disabled;
        }

        /// <summary>
        /// 全参数构造方法
        /// </summary>
        /// <param name="folder">所属分组</param>
        /// <param name="title">站点名称</param>
        /// <param name="siteUrl">站点URL</param>
        /// <param name="description">描述说明</param>
        /// <param name="maxCount">最大抓取数据条数</param>
        /// <param name="planTime">计划执行抓取时间</param>
        /// <param name="existCompareFields">判断已存在数据比较字段</param>
        /// <param name="disabled">是否启用</param>
        /// <param name="isCrawlAll">是否抓取全部数据</param>
        /// <param name="existsUpdate">已存在数据是否更新</param>
        public WebSiteCrawller(string folder, string title,
            string siteUrl, string description, int maxCount = 20,
            string planTime = "1:00,9:00,09:05,09:25,09:50,11:30,14:05,14:25,14:50,16:50,19:00",
            string existCompareFields = "InfoUrl",
            bool disabled = false, bool isCrawlAll = false, bool existsUpdate = false, bool existsHtlCtx = false)
            : this()
        {
            this.Group = folder;
            this.Title = title;
            this.Description = description;
            this.SiteUrl = siteUrl;
            this.MaxCount = maxCount;
            this.ExistCompareFields = existCompareFields;
            this.PlanTime = planTime;
            this.Disabled = disabled;
            this.IsCrawlAll = isCrawlAll;
            this.ExistsUpdate = existsUpdate;
            this.ExistsHtlCtx = existsHtlCtx;
        }


        private WebSiteCrawlConfig _config;
        /// <summary>
        /// 当前抓取配置
        /// </summary>
        public WebSiteCrawlConfig Config
        {
            get
            {
                if (_config == null)
                {
                    _config = new WebSiteCrawlConfig();
                    _config.Key = this.Key;
                }
                return _config;
            }
            set
            {
                _config = value;
                _config.Key = this.Key;
            }
        }

        /// <summary>
        /// 获取或设置站点的唯一标识
        /// </summary> 
        public virtual string Key
        {
            get
            {
                return this.GetType().FullName;
            }
        }

        /// <summary>
        /// 获取或设置站点的分组名称
        /// </summary> 
        public virtual string Group
        {
            get { return Config.Group; }
            set { Config.Group = value; }
        }

        /// <summary>
        /// 获取或设置站点的标题
        /// </summary> 
        public virtual string Title
        {
            get { return Config.Title; }
            set { Config.Title = value; }
        }

        /// <summary>
        /// 获取或设置站点的标题
        /// </summary> 
        public virtual string Description
        {
            get { return Config.Description; }
            set { Config.Description = value; }
        }

        /// <summary>
        /// 获取或设置站点 URL 地址
        /// </summary> 
        public virtual string SiteUrl
        {
            get { return Config.SiteUrl; }
            set { Config.SiteUrl = value; }
        }

        /// <summary>
        /// 获取或设置是否禁用当前抓取程序
        /// </summary> 
        public virtual bool Disabled
        {
            get { return Config.Disabled; }
            set { Config.Disabled = value; }
        }

        /// <summary>
        /// 获取或设置是否抓取站点全部数据记录
        /// </summary> 
        public virtual bool IsCrawlAll
        {
            get { return Config.IsCrawlAll; }
            set { Config.IsCrawlAll = value; }
        }

        /// <summary>
        /// 获取或设置每次抓取的数据量，默认为20
        /// </summary>
        public virtual int MaxCount
        {
            get { return Config.MaxCount; }
            set { Config.MaxCount = value; }
        }

        /// <summary>
        /// 获取或设置计划抓取时间，每个时间之间用逗号分隔
        /// </summary>
        /// <returns></returns>
        public virtual string PlanTime
        {
            get { return Config.PlanTime; }
            set { Config.PlanTime = value; }
        }

        /// <summary>
        /// 获取或设置判断数据是否重复的字段的名称，如果有多个，每个字段名称之间用逗号分隔
        /// </summary>
        /// <returns></returns>
        public virtual string ExistCompareFields
        {
            get { return Config.ExistCompareFields; }
            set { Config.ExistCompareFields = value; }
        }

        /// <summary>
        /// 获取或设置重复的数据是否更新
        /// </summary>
        /// <returns></returns>
        public virtual bool ExistsUpdate
        {
            get { return Config.ExistsUpdate; }
            set { Config.ExistsUpdate = value; }
        }

        /// <summary>
        /// 获取或设置重复数据是否跟新（只包含（CtxHtml,Ctx）两个字段更新）
        /// </summary>
        public virtual bool ExistsHtlCtx
        {
            get { return Config.ExistsHtlCtx; }
            set { Config.ExistsHtlCtx = value; }
        }

        /// <summary>
        /// 获取或设置重复附件是否更新
        /// </summary>
        public virtual bool ExistsUpdateAttach
        {
            get { return Config.ExistsUpdateAttach; }
            set { Config.ExistsUpdateAttach = value; }
        }

        /// <summary>
        /// 获取或设置站点网页采用的默认编码名称
        /// </summary>
        /// <returns></returns>
        public virtual string EncodingName
        {
            get
            {
                return Config.EncodingName;
            }
            set
            {
                if (value != Config.EncodingName)
                {
                    Config.EncodingName = value;
                    encoding = Encoding.GetEncoding(Config.EncodingName);
                }
            }
        }

        /// <summary>
        /// 获取或设置最后一次抓取开始时间
        /// </summary>
        /// <returns></returns>
        public virtual DateTime LastCrawlStart
        {
            get { return Config.LastCrawlStart; }
            set { Config.LastCrawlStart = value; }
        }

        /// <summary>
        /// 获取或设置最后一次抓取结束时间
        /// </summary>
        /// <returns></returns>
        public virtual DateTime LastCrawlEnd
        {
            get { return Config.LastCrawlEnd; }
            set { Config.LastCrawlEnd = value; }
        }

        /// <summary>
        /// 获取或设置是否启用当前抓取程序
        /// </summary>  
        public bool Enabled
        {
            get { return !Disabled; }
            set { Disabled = !value; }
        }

        /// <summary>
        /// 获取或设置下次抓取时间
        /// </summary>
        /// <returns></returns>
        public virtual DateTime NextCrawl { get; set; }

        private Encoding encoding;
        /// <summary>
        /// 获取站点网页采用的默认编码名称
        /// </summary>
        /// <returns></returns> 
        public virtual Encoding Encoding
        {
            get
            {
                if (encoding == null)
                    encoding = Encoding.GetEncoding(EncodingName);
                return encoding;
            }
        }

        private List<BaseAttach> _attachList;

        /// <summary>
        /// 获取或设置本次抓取到的附件列表
        /// </summary> 
        public virtual List<BaseAttach> AttachList
        {
            get
            {
                if (_attachList == null)
                    _attachList = new List<BaseAttach>();
                return _attachList;
            }
            protected set { _attachList = value; }
        }

        /// <summary>
        /// 执行抓取操作
        /// </summary>
        /// <param name="crawlAll">是否抓取所有数据</param> 
        public IList Crawl(bool crawlAll)
        {
            try
            {
                this.Config.LastCrawlStart = DateTime.Now;

                IList list = this.ExecuteCrawl(crawlAll);

                this.Config.LastCrawlEnd = DateTime.Now;
                return list;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return null;
        }

        /// <summary>
        /// 子类需要实现的具体抓取操作
        /// </summary>
        /// <param name="crawlAll">是否抓取所有数据</param> 
        protected abstract IList ExecuteCrawl(bool crawlAll);

       
    }
}
