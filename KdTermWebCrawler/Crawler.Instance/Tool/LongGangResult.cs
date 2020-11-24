using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crawler.Instance
{
    public class LongGangResult
    {
        public int Xh { get; set; }
        public string Code { get; set; }
        /// <summary>
        /// 单位名称
        /// </summary>
        public string UnitName { get; set; }
        /// <summary>
        /// 投标金额
        /// </summary>
        public string BidMoney { get; set; }
        /// <summary>
        /// 投标时间
        /// </summary>
        public string TbDate { get; set; }
        /// <summary>
        /// 是否淘汰
        /// </summary>
        public string IsNo { get; set; }
        /// <summary>
        /// 取胜次数
        /// </summary>
        public string Win { get; set; }
        /// <summary>
        /// 得票数
        /// </summary>
        public string Piao { get; set; }
        /// <summary>
        /// 名次
        /// </summary>
        public string Ming { get; set; }
        /// <summary>
        /// 是否确定中标人
        /// </summary>
        public string IsBid { get; set; }
        /// <summary>
        /// 中标状态
        /// </summary>
        public string BidStatus { get; set; }
    }
}
