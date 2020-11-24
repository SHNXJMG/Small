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
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.IO;
using System.Data;

namespace Crawler.Instance
{
    public class BidExpertBidProjectSzJyzxZjpb : WebSiteCrawller
    {
        public BidExpertBidProjectSzJyzxZjpb()
            : base()
        {
            this.Group = "评标结果公示";
            this.Title = "广东省深圳市建设工程专家评标工程(2015版)";
            this.Description = "自动抓取广东省深圳市建设工程专家评标工程(2015版)";
            this.PlanTime = "04:02";
            this.ExistCompareFields = "PrjNo,PrjName,Prov,City,Area";
            this.SiteUrl = "https://www.szjsjy.com.cn:8001/jyw/queryPWList.do?page=1&rows=";
            this.MaxCount = 50;
        }
        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<BidProject>();
            string html = string.Empty;
            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + (MaxCount + 20));
            }
            catch { return null; }
            int startIndex = html.IndexOf("{");
            int endIndex = html.LastIndexOf("}");
            html = html.Substring(startIndex, (endIndex + 1) - startIndex);
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, object> smsTypeJson = (Dictionary<string, object>)serializer.DeserializeObject(html);
            foreach (KeyValuePair<string, object> obj in smsTypeJson)
            {
                if (obj.Key == "total") continue;
                object[] array = (object[])obj.Value;
                foreach (object arrValue in array)
                {
                    string bProv = string.Empty, bCity = string.Empty, bArea = string.Empty, bPrjno = string.Empty, bPrjname = string.Empty, bExpertendtime = string.Empty, bBidresultendtime = string.Empty, bBaseprice = string.Empty, bBiddate = string.Empty, bBuildunit = string.Empty, bBidmethod = string.Empty, bRemark = string.Empty, bInfourl = string.Empty;

                    Dictionary<string, object> dic = (Dictionary<string, object>)arrValue;
                    bPrjno = Convert.ToString(dic["bdBH"]);
                    bPrjname = Convert.ToString(dic["bdName"]);
                    bInfourl = "https://www.szjsjy.com.cn:8001/jyw/queryOldDataDetail.do?type=6&id=" + bPrjno;
                    bool IsJson = false;
                    string htmldtl = string.Empty;
                    try
                    {
                        htmldtl = this.ToolWebSite.GetHtmlByUrl(bInfourl).GetJsString().GetReplace("\\t,\\r,\\n,\"");
                    }
                    catch { }
                    if (string.IsNullOrEmpty(htmldtl))
                    {
                        bInfourl = "https://www.szjsjy.com.cn:8001/jyw/queryPWInfoByGuid.do?guid=" + dic["pwBdGuid"];
                        try
                        {
                            htmldtl = this.ToolWebSite.GetHtmlByUrl(bInfourl).GetJsString().GetReplace("\\t,\\r,\\n,\"");
                        }
                        catch { continue; }
                        IsJson = true;
                    }
                    BidProject info = ToolDb.GenExpertProject("广东省", "深圳市", "", bPrjno, bPrjname, bExpertendtime, bBaseprice, bBiddate, bBuildunit, bBidmethod, bRemark, bInfourl);
                    string sql = string.Format("select Id from BidProject where 1=1 and PrjNo='{0}' and PrjName='{1}'", info.PrjNo, info.PrjName);
                    string result = Convert.ToString(ToolDb.ExecuteScalar(sql));
                    if (!string.IsNullOrEmpty(result))
                    {
                        if (!IsJson)
                            SaveExpert(result, bInfourl, htmldtl, true);
                        else
                            SaveExpertJson(htmldtl, result, bInfourl, true);
                    }
                    else
                    {
                        if (ToolDb.SaveEntity(info, this.ExistCompareFields))
                            if (!IsJson)
                                SaveExpert(info.Id, bInfourl, htmldtl, false);
                            else
                                SaveExpertJson(htmldtl, info.Id, bInfourl, false);
                    }
                }
            }
            return list;
        }

        private void SaveExpertJson(string html, string guid, string url, bool isUpdate)
        {
            List<BidProjectExpert> listExpert = new List<BidProjectExpert>();
            try
            {
                int startInx = html.IndexOf("suiJiList");
                int endInx = html.LastIndexOf("qiTaList");
                string htmldtl = "{" + (html.Substring(startInx, endInx - startInx - 1)) + "}";//.GetReplace("suiJiList", "{\"suiJiList\"")+"}";//("{" + html.Substring(startInx, endInx - startInx - 1) + "}").GetReplace("{", "{\"").GetReplace(":", "\":").GetReplace("\":", "\":\"").Replace(",", "\",").Replace("\",", "\",\"").GetReplace("}", "\"}"); 
                                                                                               //List<Expert> list =  Newtonsoft.Json.JsonConvert.<Expert>(htmldtl);

                DataTable dt = JsonToDataTable(htmldtl);
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        string bBidProjectId = string.Empty, bExpertname = string.Empty, bBidtype = string.Empty, bExpertspec = string.Empty,
       bExpertunit = string.Empty,
       bRemark = string.Empty, bInfourl = string.Empty, bCreator = string.Empty, bCreatetime = string.Empty, bLastmodifier = string.Empty,
       bLastmodifytime = string.Empty;
                        bExpertname = Convert.ToString(row["pwName"]);
                        bBidtype = Convert.ToString(row["pbLeiXing"]);
                        bExpertspec = Convert.ToString(row["zhuanYeName"]);
                        bExpertunit = Convert.ToString(row["gongZuoDanWei"]);
                        BidProjectExpert info = ToolDb.GenProjectExpert(guid, bExpertname, bBidtype, bExpertspec, bExpertunit, string.Empty, url);
                        listExpert.Add(info);
                    }
                }

            }
            catch
            {

            }

            if (listExpert.Count > 0)
            {
                if (isUpdate)
                {
                    string delSql = string.Format("delete from BidProjectExpert where BidProjectId='{0}'", guid);
                    string result = Convert.ToString(ToolDb.ExecuteScalar(delSql));
                    if (string.IsNullOrEmpty(result))
                        ToolDb.SaveDatas(listExpert, "");
                }
                else
                    ToolDb.SaveDatas(listExpert, "");
            }
        }


        /// <summary>
        /// 将json转换为DataTable
        /// </summary>
        /// <param name="strJson">得到的json</param>
        /// <returns></returns>
        private DataTable JsonToDataTable(string strJson)
        {
            //转换json格式
            strJson = strJson.Replace(",", "*").Replace(":", "#").ToString();
            //取出表名   
            var rg = new Regex(@"(?<={)[^:]+(?=:\[)", RegexOptions.IgnoreCase);
            string strName = rg.Match(strJson).Value;
            DataTable tb = null;
            //去除表名   
            strJson = strJson.Substring(strJson.IndexOf("[") + 1);
            strJson = strJson.Substring(0, strJson.IndexOf("]"));

            //获取数据   
            rg = new Regex(@"(?<={)[^}]+(?=})");
            MatchCollection mc = rg.Matches(strJson);
            for (int i = 0; i < mc.Count; i++)
            {
                string strRow = mc[i].Value;
                string[] strRows = strRow.Split('*');

                //创建表   
                if (tb == null)
                {
                    tb = new DataTable();
                    tb.TableName = strName + "tableNew";
                    foreach (string str in strRows)
                    {
                        var dc = new DataColumn();
                        string[] strCell = str.Split('#');

                        if (strCell[0].Substring(0, 1) == "\"")
                        {
                            int a = strCell[0].Length;
                            dc.ColumnName = strCell[0].Substring(1, a - 2);
                        }
                        else
                        {
                            dc.ColumnName = strCell[0];
                        }
                        tb.Columns.Add(dc);
                    }
                    tb.AcceptChanges();
                }

                //增加内容   
                DataRow dr = tb.NewRow();
                for (int r = 0; r < strRows.Length; r++)
                {
                    dr[r] = strRows[r].Split('#')[1].Trim().Replace("，", ",").Replace("：", ":").Replace("\"", "");
                }
                tb.Rows.Add(dr);
                tb.AcceptChanges();
            }

            return tb;
        }

        /// <summary> 
        /// Json格式转换成键值对，键值对中的Key需要区分大小写 
        /// </summary> 
        /// <param name="JsonData">需要转换的Json文本数据</param> 
        /// <returns></returns> 
        public static Dictionary<string, object> ToDictionary(string JsonData)
        {
            object Data = null;
            Dictionary<string, object> Dic = new Dictionary<string, object>();
            if (JsonData.StartsWith("["))
            {
                //如果目标直接就为数组类型，则将会直接输出一个Key为List的List<Dictionary<string, object>>集合 
                //使用示例List<Dictionary<string, object>> ListDic = (List<Dictionary<string, object>>)Dic["List"]; 
                List<Dictionary<string, object>> List = new List<Dictionary<string, object>>();
                MatchCollection ListMatch = Regex.Matches(JsonData, @"{[\s\S]+?}");//使用正则表达式匹配出JSON数组 
                foreach (Match ListItem in ListMatch)
                {
                    List.Add(ToDictionary(ListItem.ToString()));//递归调用 
                }
                Data = List;
                Dic.Add("List", Data);
            }
            else
            {
                MatchCollection Match = Regex.Matches(JsonData, @"""(.+?)"": {0,1}(\[[\s\S]+?\]|null|"".+?""|-{0,1}\d*)");//使用正则表达式匹配出JSON数据中的键与值 
                foreach (Match item in Match)
                {
                    try
                    {
                        if (item.Groups[2].ToString().StartsWith("["))
                        {
                            //如果目标是数组，将会输出一个Key为当前Json的List<Dictionary<string, object>>集合 
                            //使用示例List<Dictionary<string, object>> ListDic = (List<Dictionary<string, object>>)Dic["Json中的Key"]; 
                            List<Dictionary<string, object>> List = new List<Dictionary<string, object>>();
                            MatchCollection ListMatch = Regex.Matches(item.Groups[2].ToString(), @"{[\s\S]+?}");//使用正则表达式匹配出JSON数组 
                            foreach (Match ListItem in ListMatch)
                            {
                                List.Add(ToDictionary(ListItem.ToString()));//递归调用 
                            }
                            Data = List;
                        }
                        else if (item.Groups[2].ToString().ToLower() == "null") Data = null;//如果数据为null(字符串类型),直接转换成null 
                        else Data = item.Groups[2].ToString(); //数据为数字、字符串中的一类，直接写入 
                        Dic.Add(item.Groups[1].ToString(), Data);
                    }
                    catch { }
                }
            }
            return Dic;
        }

        private void SaveExpert(string guid, string url, string htmltxt, bool isUpdate)
        {
            List<BidProjectExpert> list = new List<BidProjectExpert>();
            Parser parser = new Parser(new Lexer(htmltxt));
            NodeList pageNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("table"));
            if (pageNode != null && pageNode.Count > 1)
            {
                TableTag table = pageNode[1] as TableTag;
                for (int i = 1; i < table.RowCount; i++)
                {
                    string bBidProjectId = string.Empty, bExpertname = string.Empty, bBidtype = string.Empty, bExpertspec = string.Empty,
                   bExpertunit = string.Empty,
                   bRemark = string.Empty, bInfourl = string.Empty, bCreator = string.Empty, bCreatetime = string.Empty, bLastmodifier = string.Empty,
                   bLastmodifytime = string.Empty;
                    TableRow tr = table.Rows[i];
                    bExpertname = tr.Columns[0].ToNodePlainString();
                    bBidtype = tr.Columns[2].ToPlainTextString();
                    bExpertspec = tr.Columns[3].ToPlainTextString();
                    try
                    {
                        bExpertunit = tr.Columns[4].ToPlainTextString();
                    }
                    catch
                    { }
                    BidProjectExpert info = ToolDb.GenProjectExpert(guid, bExpertname, bBidtype, bExpertspec, bExpertunit, string.Empty, url);
                    list.Add(info);
                }
            }
            if (list.Count > 0)
            {
                if (isUpdate)
                {
                    string delSql = string.Format("delete from BidProjectExpert where BidProjectId='{0}'", guid);
                    string result = Convert.ToString(ToolDb.ExecuteScalar(delSql));
                    if (string.IsNullOrEmpty(result))
                        ToolDb.SaveDatas(list, "");
                }
                else
                    ToolDb.SaveDatas(list, "");
            }
        }
    }

}
