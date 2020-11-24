using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Crawler
{
    /// <summary>
    /// 抓取程序配置项
    /// </summary>
    [Serializable]
    public sealed class WebSiteCrawlConfig
    {
        /// <summary>
        /// 获取或设置站点的唯一标识
        /// </summary> 
        public string Key { get; set; }

        /// <summary>
        /// 获取或设置站点的分组名称
        /// </summary> 
        public string Group { get; set; }

        /// <summary>
        /// 获取或设置站点的标题
        /// </summary> 
        public string Title { get; set; }

        /// <summary>
        /// 获取或设置站点的标题
        /// </summary> 
        public string Description { get; set; }

        /// <summary>
        /// 获取或设置站点 URL 地址
        /// </summary> 
        public string SiteUrl { get; set; }

        /// <summary>
        /// 获取或设置是否禁用当前抓取程序
        /// </summary> 
        public bool Disabled { get; set; }

        /// <summary>
        /// 获取或设置是否抓取站点全部数据记录
        /// </summary> 
        public bool IsCrawlAll { get; set; }

        /// <summary>
        /// 获取或设置每次抓取的数据量，默认为20
        /// </summary>
        public int MaxCount { get; set; }

        /// <summary>
        /// 获取或设置计划抓取时间，每个时间之间用逗号分隔
        /// </summary>
        /// <returns></returns>
        public string PlanTime { get; set; }

        /// <summary>
        /// 获取或设置判断数据是否重复的字段的名称，如果有多个，每个字段名称之间用逗号分隔
        /// </summary>
        /// <returns></returns>
        public string ExistCompareFields { get; set; }

        /// <summary>
        /// 获取或设置当前抓取每次采集网页暂停时间（单位：秒）
        /// </summary>
        public int MaxEndTime { get; set; }

        /// <summary>
        /// 获取或设置重复的数据是否更新
        /// </summary>
        /// <returns></returns>
        public bool ExistsUpdate { get; set; }

        /// <summary>
        /// 获取或设置重复数据是否跟新（只包含（HtmlTxt,Ctx）两个字段更新）
        /// </summary>
        public bool ExistsHtlCtx { get; set; }

        /// <summary>
        /// 获取或设置重复附件是否更新
        /// </summary>
        public bool ExistsUpdateAttach { get; set; }
        
        /// <summary>
        /// 获取或设置最后一次抓取开始时间
        /// </summary>
        /// <returns></returns>
        public DateTime LastCrawlStart { get; set; }

        /// <summary>
        /// 获取或设置最后一次抓取结束时间
        /// </summary>
        /// <returns></returns>
        public DateTime LastCrawlEnd { get; set; }

        protected string encodingName;
        /// <summary>
        /// 获取或设置站点网页采用的默认编码名称
        /// </summary>
        /// <returns></returns>
        public string EncodingName
        {
            get
            {
                if (string.IsNullOrEmpty(encodingName))
                    return Encoding.UTF8.EncodingName;
                return encodingName;
            }
            set
            {
                if (value != encodingName)
                {
                    encodingName = value;
                }
            }
        }
    }
}
