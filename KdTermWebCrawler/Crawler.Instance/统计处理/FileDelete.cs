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
using System.IO;
using System.Web.UI.MobileControls;
using System.Collections.Generic;
using System.Reflection;
using System.Data.SqlClient;
using System.Data;
using System.Web.UI.WebControls;

namespace Crawler.Instance
{
    public class FileDelete : WebSiteCrawller
    {
        public FileDelete()
            : base()
        {
            this.Group = "其它处理";
            this.Title = "附件删除";
            this.Description = "自动抓取处理附件删除";
            this.PlanTime = "1 22:22";
            this.SiteUrl = "InviteAttach"; 
            this.ExistCompareFields = "";
        }

        private string BeginDate
        {
            get { return DateTime.Now.AddDays(-1095).ToString(); }
        }

        private string EndDate
        {
            get { return DateTime.Now.AddDays(1).ToString(); }
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            string selectSql = string.Empty;
            if (this.MaxCount > 0)
                selectSql = "select AttachServerPath from BaseAttach where CreateTime >='" + BeginDate + "' ";
            else
                selectSql = "select AttachServerPath from BaseAttach where 1=1 ";
            string[] paths = new string[] { "Attach", "InviteAttach", "Notify_Attach" };
            foreach (string str in paths)
            {
                string path = ToolCoreDb.DbServerPath + "SiteManage\\Files\\" + str;
                DirectoryInfo dir = new DirectoryInfo(path);
                DirectoryInfo[] subDirs = dir.GetDirectories();
                foreach (DirectoryInfo dirInfo in subDirs)
                {
                    FileInfo[] info = dirInfo.GetFiles();
                    foreach (FileInfo fileInfo in info)
                    {
                        string sql = selectSql + " and AttachServerPath like '%" + fileInfo.Name + "%'";
                        object obj = ToolDb.ExecuteScalar(sql);
                        if (obj == null || string.IsNullOrEmpty(obj.ToString()))
                        {
                            fileInfo.Delete();
                        }
                    }
                }
            }
            return null;
        }
    }
}
