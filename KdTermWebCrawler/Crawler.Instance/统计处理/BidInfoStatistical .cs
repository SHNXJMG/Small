using System.Text;
using Crawler;
using System.Collections;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Tags;
using System.Collections.Specialized;
using System;
using System.Text.RegularExpressions;
using System.Web.UI.HtmlControls;
using System.Data;
using System.Web.UI.MobileControls;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Crawler.Instance
{
    public class BidInfoStatistical : WebSiteCrawller
    {
        public BidInfoStatistical()
            : base()
        {
            this.Group = "统计处理";
            this.Title = "中标信息统计处理（去重复）";
            this.Description = "自动中标信息统计处理";
            this.PlanTime = "12:35,0:35";
            this.ExistCompareFields = "InfoUrl";
            this.SiteUrl = "见公告内容,见内容公告,见招标公告内容,见招标详细信息,见招标信息,见招标信息内容,见中标详细信息,见中标信息,见中标信息内容,资格后审项目的资格审查情况及确定中标人理由见附件（请下载查看）,采购中心,采购中心线,采购组,没有中标商,成交没有中标商";
            this.MaxCount = 30;
            this.ExistsUpdate = true;
        }

        public string BeginDate
        {
            get { return DateTime.Now.AddDays(-MaxCount).ToString("yyyy-MM-dd"); }
        }
        public string EndDate
        {
            get { return DateTime.Now.ToString("yyyy-MM-dd"); }
        }

        string whereUnit = "'见公告内容', '见内容公告','见招标公告内容', '见招标详细信息', '见招标信息',  '见招标信息内容', '见中标详细信息', '见中标信息','见中标信息内容','资格后审项目的资格审查情况及确定中标人理由见附件（请下载查看）','采购中心','采购中心线','采购组','没有中标商','成交没有中标商'";

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            string sql = "update BidInfo set IsStatistical='1'  where BeginDate >='" + BeginDate + "' and BeginDate <= '" + EndDate + "'  and (BidUnit is null Or BidUnit = '' Or BidUnit in (" + GetBidUnit() + "))"; 
            ToolDb.ExecuteSql(sql);
            UpdateEntity(UpdateBidInfoByCodeOrPrjName(false));
            UpdateEntity(UpdateBidInfoByCodeOrPrjName(true));
            return new ArrayList();
        }

        /// <summary>
        /// 从Sql中获取需要更新的重复记录
        /// </summary>
        /// <param name="isCode"></param>
        /// <returns></returns>
        private List<BidInfo> UpdateBidInfoByCodeOrPrjName(bool isCode)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("select Id,Code,ProjectName,BidUnit,BeginDate,LastModifyTime,BidMoney,IsStatistical from BidInfo  where 1=1 ");
            sql.Append(" and BidUnit <> '' and BidUnit is not null and BidUnit not  in (" + GetBidUnit() + ")");
            if (!isCode)
                sql.Append(" and ProjectName in (select ProjectName from BidInfo group by ProjectName having count(ProjectName) > 1)");
            else
                sql.Append(" and Code in (select Code from BidInfo group by Code having count(Code) > 1)");

            sql.Append(" and BeginDate >='" + BeginDate + "' and BeginDate <= '" + EndDate + "'");
            
            sql.Append(" and IsStatistical <> '1' ");
            List<BidInfo> bidInfo = ToolDb.GetBidInfoList(sql.ToString());
            List<BidInfo> updateBidInfo = new List<BidInfo>();
            if (bidInfo != null && bidInfo.Count > 0)
            {
                foreach (BidInfo info in bidInfo)
                {
                    BidInfo entity = new BidInfo(); 
                    List<BidInfo> bidInfoNew = GetListByNotMsgType(bidInfo, info.Code, info.BidUnit, true);
                    if (bidInfoNew != null && bidInfoNew.Count > 1)
                    {
                        entity = GetList(bidInfoNew);
                        if (entity.Id != info.Id)
                        {
                            info.IsStatistical = "1";
                            updateBidInfo.Add(info);
                        }
                    }
                    else
                    {
                        bidInfoNew = GetListByNotMsgType(bidInfo, info.ProjectName, info.BidUnit, false);
                        if (bidInfoNew != null && bidInfoNew.Count > 1)
                        {
                            entity = GetList(bidInfoNew);
                            if (entity.Id != info.Id)
                            {
                                info.IsStatistical = "1";
                                updateBidInfo.Add(info);
                            }
                        }
                    } 
                }
            }
            return updateBidInfo;
        }


        private string GetBidUnit()
        {
            string sqlWhere = string.Empty;
            try
            {
                string[] bidUnit = new string[] { };
                bidUnit = SiteUrl.Split(',');
                sqlWhere = string.Empty;
                foreach (string strUnit in bidUnit)
                {
                    sqlWhere += "'" + strUnit + "',";
                }
                sqlWhere = sqlWhere.Remove(sqlWhere.Length - 1);
            }
            catch
            {
                sqlWhere = whereUnit;
            }
            return sqlWhere;
        }

        /// <summary>
        /// 通过编号或者名称与中标单位获取重复数据
        /// </summary>
        /// <param name="list"></param>
        /// <param name="codeOrprjName"></param>
        /// <param name="bidunit"></param>
        /// <param name="isCode"></param>
        /// <returns></returns>
        private List<BidInfo> GetListByNotMsgType(List<BidInfo> list, string codeOrprjName, string bidunit, bool isCode)
        {
            if (isCode)
            {
                if (string.IsNullOrEmpty(codeOrprjName)) return null;
                var linqSelect1 = from BidInfoNew in list
                                  where BidInfoNew.Code == codeOrprjName && BidInfoNew.BidUnit == bidunit
                                  select BidInfoNew;
                if (linqSelect1.Count() > 0)
                    return linqSelect1.ToList();
            }
            else
            {
                var linqSelect2 = from BidInfoNew in list
                                  where BidInfoNew.ProjectName == codeOrprjName && BidInfoNew.BidUnit == bidunit
                                  select BidInfoNew;
                if (linqSelect2.Count() > 0)
                    return linqSelect2.ToList();
            }
            return null;
        }

        /// <summary>
        /// 通过重复数据选出作为不重复的数据
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private BidInfo GetList(List<BidInfo> list)
        {
            var linqSelect = from BidInfoNew in list
                             where BidInfoNew.BidMoney > 0
                             orderby BidInfoNew.LastModifyTime descending
                             select BidInfoNew;
            if (linqSelect.Count() > 0)
            {
                return linqSelect.ToList()[0];
            }
            else
            {
                var linqSelect2 = from BidInfoNew in list
                                  orderby BidInfoNew.LastModifyTime descending
                                  select BidInfoNew;
                if (linqSelect2.Count() > 0)
                {
                    return linqSelect2.ToList()[0];
                }
            }
            return list[0];
        }

        /// <summary>
        /// 执行Sql更新
        /// </summary>
        /// <param name="entityList"></param>
        /// <returns></returns>
        private int UpdateEntity(List<BidInfo> entityList)
        {
            int success = 0;
            string sql = string.Empty;
            SqlConnection conn = new SqlConnection(ToolDb.DbConnString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            conn.Open();
            SqlTransaction trans = conn.BeginTransaction();
            try
            {
                if (entityList != null)
                {
                    foreach (BidInfo entity in entityList)
                    {
                        sql = "update BidInfo set IsStatistical='" + ((entity.IsStatistical == null) ? "0" : entity.IsStatistical) + "'  where Id='" + entity.Id + "'";
                        cmd.CommandText = sql;
                        cmd.Transaction = trans;
                        success += cmd.ExecuteNonQuery();
                    }
                    trans.Commit();
                }
            }
            catch (Exception ex)
            {
                trans.Rollback();
                ToolDb.Logger.Error(ex.ToString());
                ToolDb.Logger.Error("错误的Sql：" + sql);
            }
            finally
            {
                conn.Close();
                trans.Dispose();
                conn.Dispose();
            }
            return success;
        }
         
    }
}
