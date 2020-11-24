using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Crawler.Instance.Entity
{
    /// <summary>
    /// 市局接口房源数据转换实体
    /// </summary>
    public class WebPactCorp
    {
        /// <summary>
        /// 详细企业合同数据
        /// </summary>
        [JsonProperty(PropertyName = "date")]
        public WebCorpContent WebCorpContent { get; set; }
        [JsonProperty(PropertyName = "retmsg")]
        public string Message { get; set; }
        [JsonProperty(PropertyName = "retcode")]
        public string RetCode { get; set; }
    }
    /// <summary>
    /// 市局接口企业合同数据内容转换实体
    /// </summary>
    public class WebCorpContent
    {
        /// <summary>
        /// 当前页（需+1）
        /// </summary>
        [JsonProperty(PropertyName = "number")]
        public int? Number { get; set; }
        /// <summary>
        /// 每页条数
        /// </summary>
        [JsonProperty(PropertyName = "size")]
        public int? PageSize { get; set; }
        /// <summary>
        /// 总条数
        /// </summary>
        [JsonProperty(PropertyName = "totalElements")]
        public int? TotalElements { get; set; }
        /// <summary>
        /// 总页数
        /// </summary>
        [JsonProperty(PropertyName = "totalPages")]
        public int? TotalPages { get; set; }
        /// <summary>
        /// 详细房源数据
        /// </summary>
        [JsonProperty(PropertyName = "content")]
        public List<WebPactCorpData> WebPactCorpDatas { get; set; }
    }
    /// <summary>
    /// 市局接口合同信息表，与市局接口字段保持一致
    /// </summary>
    public class WebPactCorpData
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [JsonProperty(PropertyName = "pkid")]
        public int? PkId { get; set; }
        /// <summary>
        /// 签约ID
        /// </summary>
        [JsonProperty(PropertyName = "signid")]
        public int? SignId { get; set; }
        /// <summary>
        /// 签约状态
        /// </summary>
        [JsonProperty(PropertyName = "signstate")]
        public string SignState { get; set; }
        /// <summary>
        /// 分配类型
        /// </summary>
        [JsonProperty(PropertyName = "allotype")]
        public string AlloType { get; set; }
        /// <summary>
        /// 合同编号
        /// </summary>
        [JsonProperty(PropertyName = "contnum")]
        public string ContNum { get; set; }
        /// <summary>
        /// 合同类型
        /// </summary>
        [JsonProperty(PropertyName = "conttype")]
        public string ContType { get; set; }
        /// <summary>
        /// 企业名称
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        /// <summary>
        /// 签约ID
        /// </summary>
        [JsonProperty(PropertyName = "identitycard")]
        public string IdentityCard { get; set; }
        /// <summary>
        /// 押金交款时间
        /// </summary>
        [JsonProperty(PropertyName = "paydepotime")]
        public DateTime? PayDepoTime { get; set; }
        /// <summary>
        /// 租金标准
        /// </summary>
        [JsonProperty(PropertyName = "rentstandar")]
        public string RentStandar { get; set; }
        /// <summary>
        /// 租金金额
        /// 
        /// </summary>
        [JsonProperty(PropertyName = "rentprice")]
        public decimal? RentPrice { get; set; }
        /// <summary>
        /// 合同总面积
        /// </summary>
        [JsonProperty(PropertyName = "area")]
        public decimal? Area { get; set; }
        /// <summary>
        /// 租金金额
        /// </summary>
        [JsonProperty(PropertyName = "amount")]
        public decimal? Amount { get; set; }
        /// <summary>
        /// 首次签约时间
        /// </summary>
        [JsonProperty(PropertyName = "firstsigntime")]
        public DateTime? FirstSignTime { get; set; }
        /// <summary>
        /// 首次租赁日期
        /// 
        /// </summary>
        [JsonProperty(PropertyName = "firstconttime")]
        public DateTime? FirstContTime { get; set; }
        /// <summary>
        /// 交租起始年月
        /// </summary>
        [JsonProperty(PropertyName = "payrent")]
        public string PayRent { get; set; }
        /// <summary>
        /// 已收押金
        /// </summary>
        [JsonProperty(PropertyName = "isdeposit")]
        public string IsDeposit { get; set; }
        /// <summary>
        /// 押金金额
        /// </summary>
        [JsonProperty(PropertyName = "deposit")]
        public decimal? Deposit { get; set; }
        /// <summary>
        /// 押金交款方式
        /// </summary>
        [JsonProperty(PropertyName = "paydepometh")]
        public string PayDepoMeth { get; set; }
        /// <summary>
        /// 托收银行
        /// </summary>
        [JsonProperty(PropertyName = "bank")]
        public string Bank { get; set; }
        /// <summary>
        /// 托收账户
        /// </summary>
        [JsonProperty(PropertyName = "account")]
        public string Account { get; set; }
        /// <summary>
        /// 经办人
        /// </summary>
        [JsonProperty(PropertyName = "handler")]
        public string Handler { get; set; }
        /// <summary>
        /// 经办时间
        /// </summary>
        [JsonProperty(PropertyName = "handtime")]
        public DateTime? HandTime { get; set; }
        /// <summary>
        /// 钥匙领取数量
        /// </summary>
        [JsonProperty(PropertyName = "keysquantity")]
        public int? Keysquantity { get; set; }
        /// <summary>
        /// 钥匙领取时间
        /// </summary>
        [JsonProperty(PropertyName = "recikeytime")]
        public object RecikeyTime { get; set; }
        /// <summary>
        /// 领取钥匙人
        /// </summary>
        [JsonProperty(PropertyName = "receiver")]
        public string Receiver { get; set; }
        /// <summary>
        /// 租赁起始日期
        /// </summary>
        [JsonProperty(PropertyName = "contstardate")]
        public DateTime? ContStarDate { get; set; }
        /// <summary>
        /// 租赁结束日期
        /// </summary>
        [JsonProperty(PropertyName = "contenddate")]
        public DateTime? ContendDate { get; set; }
        /// <summary>
        /// 签约信息备注
        /// </summary>
        [JsonProperty(PropertyName = "remark")]
        public string Remark { get; set; }
        /// <summary>
        /// 单位名称
        /// </summary>
        [JsonProperty(PropertyName = "corp")]
        public string Corp { get; set; }
        /// <summary>
        /// 单位性质
        /// </summary>
        [JsonProperty(PropertyName = "nature")]
        public string Nature { get; set; }
        /// <summary>
        /// 重点单位类型
        /// </summary>
        [JsonProperty(PropertyName = "unittype")]
        public string UnitType { get; set; }
        /// <summary>
        /// 单位证件类型
        /// </summary>
        [JsonProperty(PropertyName = "unitcerttype")]
        public string UnitcertType { get; set; }
        /// <summary>
        /// 租赁状态
        /// </summary>
        [JsonProperty(PropertyName = "rentstatus")]
        public string RentStatus { get; set; }
        /// <summary>
        /// 单位证件号码
        /// </summary>
        [JsonProperty(PropertyName = "unitcode")]
        public string UnitCode { get; set; }
        /// <summary>
        /// 详细地址
        /// </summary>
        [JsonProperty(PropertyName = "addr")]
        public string Addr { get; set; }
        /// <summary>
        /// 法人代表姓名
        /// </summary>
        [JsonProperty(PropertyName = "lpname")]
        public string Lpname { get; set; }
        /// <summary>
        /// 证件类型
        /// </summary>
        [JsonProperty(PropertyName = "lpcerttype")]
        public string Lpcerttype { get; set; }
        /// <summary>
        /// 证件号码
        /// </summary>
        [JsonProperty(PropertyName = "lpcode")]
        public string Lpcode { get; set; }
        /// <summary>
        /// 移动电话
        /// </summary>
        [JsonProperty(PropertyName = "lpmobile")]
        public string Lpmobile { get; set; }
        /// <summary>
        /// 住房专员姓名
        /// </summary>
        [JsonProperty(PropertyName = "comname")]
        public string Comname { get; set; }
        /// <summary>
        /// 备案审核要点
        /// </summary>
        [JsonProperty(PropertyName = "filingpoint")]
        public string Filingpoint { get; set; }
        /// <summary>
        /// 证件类型
        /// </summary>
        [JsonProperty(PropertyName = "comcerttype")]
        public string ComcertType { get; set; }
        /// <summary>
        /// 证件号码
        /// </summary>
        [JsonProperty(PropertyName = "comcode")]
        public string ComCode { get; set; }
        /// <summary>
        /// 移动电话
        /// </summary>
        [JsonProperty(PropertyName = "commobile")]
        public string ComMobile { get; set; }
        /// <summary>
        /// 固定电话
        /// </summary>
        [JsonProperty(PropertyName = "comtel")]
        public string ComTel { get; set; }
        /// <summary>
        /// 住房专员姓名2
        /// </summary>
        [JsonProperty(PropertyName = "comname2")]
        public string ComName2 { get; set; }
        /// <summary>
        /// 证件类型2
        /// </summary>
        [JsonProperty(PropertyName = "comcerttype2")]
        public string ComcertType2 { get; set; }
        /// <summary>
        /// 证件号码2
        /// </summary>
        [JsonProperty(PropertyName = "comcode2")]
        public string ComCode2 { get; set; }
        /// <summary>
        /// 移动电话2
        /// </summary>
        [JsonProperty(PropertyName = "commobile2")]
        public string ComMobile2 { get; set; }
        /// <summary>
        /// 固定电话2
        /// </summary>
        [JsonProperty(PropertyName = "comtel2")]
        public string ComTel2 { get; set; }
        /// <summary>
        /// 签约ID
        /// 
        /// </summary>
        [JsonProperty(PropertyName = "rownum_")]
        public string Rownum_ { get; set; }

    }
}
