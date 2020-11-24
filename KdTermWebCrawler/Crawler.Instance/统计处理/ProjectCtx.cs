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
    public class ProjectCtx : WebSiteCrawller
    {
        public ProjectCtx()
            : base()
        {
            this.Group = "统计处理";
            this.Title = "处理招标信息纯文本温馨提示等非检索";
            this.Description = "注：该Url为City字段条件，多个用半角逗号隔开。每次抓取条数为需要处理多少天以内的数据。判断重复为需要处理的文本，多个用半角逗号隔开";
            this.PlanTime = "12:40";
            this.ExistCompareFields = "重要提示,温馨提示";
            this.SiteUrl = "深圳市工程";
            this.MaxCount = 30;
        }

        public string BeginDate
        {
            get { return DateTime.Now.AddDays(-MaxCount).ToString("yyyy-MM-dd"); }
        }

        public string EndDate
        {
            get { return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); }
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            Dictionary<string, Regex> dicRegex = new Dictionary<string, Regex>();
            string sqlWhere = string.Empty;
            
            //设置InviteCtx查询条件
            string ctxWhere = string.Empty;
            string[] ctx = this.ExistCompareFields.Split(',');
            for (int i = 0; i < ctx.Length; i++)
            {
                if (!string.IsNullOrEmpty(ctx[i]))
                {
                    dicRegex.Add(ctx[i], new Regex(@"([.\S\s]*)(?=" + ctx[i] + ")"));
                    ctxWhere += "  InviteCtx like '%" + ctx[i] + "%' or";
                }
            }
            if (!string.IsNullOrEmpty(ctxWhere))
            {
                ctxWhere = ctxWhere.Remove(ctxWhere.Length - 2, 2);
                sqlWhere += " and (" + ctxWhere + ")";
            }

            //设置City查询条件
            string[] where = this.SiteUrl.Split(',');
            string cityWhere = string.Empty;
            for (int i = 0; i < where.Length; i++)
            {
                if (!string.IsNullOrEmpty(where[i]))
                {
                    cityWhere += "  City='" + where[i] + "' or"; 
                }
            }
            if (!string.IsNullOrEmpty(cityWhere))
            {
                cityWhere = cityWhere.Remove(cityWhere.Length - 2, 2);
                sqlWhere += " and (" + cityWhere + ")";
            }
            //查询语句
            string sql = "select * from InviteInfo where 1=1 ";
            sql += " and CreateTime <='" + EndDate + "' and CreateTime >='" + BeginDate + "'";
            sql += sqlWhere;

            DataTable dt = ToolDb.GetDbData(sql);
            if (dt == null || dt.Rows.Count < 1) return null;


            //dicRegex.Add("重要提示", new Regex(@"([.\S\s]*)(?=重要提示)"));
            //dicRegex.Add("温馨提示", new Regex(@"([.\S\s]*)(?=温馨提示)"));
            foreach (DataRow row in dt.Rows)
            {
                string inviteCtx = Convert.ToString(row["InviteCtx"]);
                if (string.IsNullOrEmpty(inviteCtx)) continue;

                bool isUpdate = false;
                string id = Convert.ToString(row["Id"]);
                string updateSql = string.Empty, updateWhere = string.Empty;
                foreach (string dicValue in dicRegex.Keys)
                {
                    if (inviteCtx.Contains(dicValue))
                    {
                        inviteCtx = dicRegex[dicValue].Match(inviteCtx).Value;
                        isUpdate = true;
                    }
                    updateWhere = " InviteCtx='" + inviteCtx + "'";
                }
                if (isUpdate)
                {
                    updateSql = "update InviteInfo set " + updateWhere + " where Id='" + id + "'";
                    ToolDb.ExecuteSql(updateSql);
                }
                isUpdate = false;
            }

            return null;
        }

    }
}
