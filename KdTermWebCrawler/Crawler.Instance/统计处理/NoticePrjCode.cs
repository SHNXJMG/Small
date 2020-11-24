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
    public class NoticePrjCode : WebSiteCrawller
    {
        public NoticePrjCode()
            : base()
        {
            this.Group = "统计处理";
            this.Title = "通知公示（工程编号处理）";
            this.Description = "自动通知公示（工程编号处理）";
            this.PlanTime = "12:35";
            this.ExistCompareFields = "";
            this.SiteUrl = "";
            this.MaxCount = 30;
            this.ExistsUpdate = true;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<NoticeInfo>();
            string sql = "select Id,PrjCode,InfoCtx,InfoUrl from NoticeInfo where (PrjCode='' or PrjCode is null) ";
            sql += " and convert(varchar(max), InfoCtx) <> '见附件' and convert(varchar(max), InfoCtx)<>'详见附件' ";
            sql += " and datalength (InfoCtx)<>0 and datalength (InfoCtx) is not null";
            DataTable dt = ToolCoreDb.GetDbData(sql);
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    string ctx = Convert.ToString(row["InfoCtx"]);
                    string prjCode = ctx.GetNoticePrjCode(); 
                    if (string.IsNullOrEmpty(prjCode))
                    {
                        prjCode = ctx.GetRegexBegEnd("工程编号", "工程名称").Replace("：","").Replace(":","").Replace("\r","").Replace("\n","").Replace(" ","").Replace("\t","");
                    }
                    string update = "update NoticeInfo set PrjCode='" + prjCode + "' where Id='" + row["Id"].ToString() + "'";
                    int result =ToolCoreDb.ExecuteSql(update);
                }
            } 
            return list;
        }
    }
}
