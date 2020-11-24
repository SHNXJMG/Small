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
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Data;

namespace Crawler.Instance
{
    public class SzJzMerit : WebSiteCrawller
    {
        public SzJzMerit()
            : base()
        {
            this.PlanTime = "1 1:00";
            this.Group = "企业信息";
            this.Title = "深圳建筑业网获奖信息";
            this.Description = "自动抓取深圳建筑业网获奖信息";
            this.ExistCompareFields = "MeritType,MeritName,MeritPrjName,Url";
            this.ExistsUpdate = true;
            this.MaxCount = 500000;
            this.SiteUrl = "http://www.jianzhuxh.com/excellence/";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            IList list = new List<CorpMerit>();
            string html = string.Empty;
            string cookiestr = string.Empty;
            string viewState = string.Empty;
            int pageInt = 1;
            string eventValidation = string.Empty;
            try
            {
                html = ToolWeb.GetHtmlByUrl(this.SiteUrl, Encoding.Default);
            }
            catch
            {
                return list;
            }

            #region 优质专业工程
            Parser parser = new Parser(new Lexer(html));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("height", "29")), true), new TagNameFilter("table")));//parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"),new HasAttributeFilter("width","98%")));
            if (nodeList != null && nodeList.Count > 0)
            {
                TableTag table = nodeList[0] as TableTag;
                parser = new Parser(new Lexer(table.ToHtml()));
                NodeList aTagNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                if (aTagNode != null && aTagNode.Count > 0)
                {
                    for (int j = 0; j < aTagNode.Count; j++)
                    {
                        ATag aTag = aTagNode[j].GetATag();
                        string name = "优质专业工程";
                        string typename = aTag.LinkText.Replace("·", "");
                        string url = "http://www.jianzhuxh.com/excellence/" + aTag.Link;
                        string htlList = string.Empty;
                        int page = 1;
                        try
                        {
                            htlList = ToolWeb.GetHtmlByUrl(url, Encoding.Default);
                        }
                        catch { continue; }
                        parser = new Parser(new Lexer(htlList));
                        NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("align", "center")));
                        if (pageNode != null && pageNode.Count > 0)
                        {
                            try
                            {
                                string temp = pageNode.AsString().GetRegexBegEnd("/", "页");
                                page = int.Parse(temp);
                            }
                            catch { }
                        }
                        for (int d = 1; d <= page; d++)
                        {
                            if (d > 1)
                            {
                                try
                                {
                                    htlList = ToolWeb.GetHtmlByUrl(url + "&page=" + d, Encoding.Default);
                                }
                                catch { continue; }
                            }
                            parser = new Parser(new Lexer(htlList));
                            NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "text")));
                            if (dtlNode != null && dtlNode.Count > 0)
                            {
                                parser = new Parser(new Lexer(dtlNode.ToHtml()));
                                NodeList dtlNodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("width", "844")));
                                if (dtlNodeList != null && dtlNodeList.Count > 0)
                                {
                                    TableTag tableTag = dtlNodeList[0] as TableTag;

                                    for (int k = 0; k < tableTag.RowCount; k++)
                                    {
                                        string CorpCode = string.Empty, CorpName = string.Empty, MeritYear = string.Empty, MeritName = string.Empty, MeritDate = string.Empty, MeritLevel = string.Empty, MeritRegion = string.Empty, MeritSector = string.Empty, MeritPrjName = string.Empty, PrjSupporter = string.Empty, Source = string.Empty, Url = string.Empty, Remark = string.Empty, Details = string.Empty, MeritType = string.Empty, PrjMgr = string.Empty, SupMgr = string.Empty, ManCost = string.Empty, ProArea = string.Empty, SupUnit = string.Empty, PileConsUnit = string.Empty, BuildingType = string.Empty;
                                        TableRow tr = tableTag.Rows[k];
                                        MeritName = name;
                                        MeritType = typename;
                                        MeritPrjName = tr.Columns[1].ToNodePlainString();
                                        CorpName = tr.Columns[2].ToNodePlainString();
                                        PrjMgr = tr.Columns[3].ToNodePlainString();
                                        SupUnit = tr.Columns[4].ToNodePlainString();
                                        SupMgr = tr.Columns[5].ToNodePlainString();
                                        ManCost = tr.Columns[6].ToNodePlainString();
                                        if (ManCost.Contains("吨"))
                                            ManCost = string.Empty;
                                        ProArea = tr.Columns[7].ToNodePlainString();
                                        MeritYear = tr.Columns[8].ToNodePlainString();

                                        CorpMerit info = ToolDb.GenCorpMerit("广东省", "深圳市", "", CorpCode, CorpName, MeritYear, MeritName, MeritDate, MeritLevel, MeritRegion, MeritSector, MeritPrjName, PrjSupporter, Source, url, Remark, Details, MeritType, PrjMgr, SupMgr, ManCost, ProArea, SupUnit, PileConsUnit, BuildingType);

                                        list.Add(info);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region 其它工程
            parser = new Parser(new Lexer(html));
            NodeList theNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("height", "32")), true), new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("align", "center"))));
            if (theNode != null && theNode.Count > 2)
            {
                TableTag table = theNode[2] as TableTag;
                parser = new Parser(new Lexer(table.ToHtml()));
                NodeList atagNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                if (atagNode != null && atagNode.Count > 0)
                {
                    for (int j = 0; j < atagNode.Count; j++)
                    {
                        ATag aTag = atagNode[j].GetATag();
                        string typename = aTag.LinkText;
                        string url = "http://www.jianzhuxh.com/excellence/" + aTag.Link;
                        string htmlList = string.Empty;
                        int page = 1;
                        try
                        {
                            htmlList = ToolWeb.GetHtmlByUrl(url, Encoding.Default);
                        }
                        catch
                        {
                            continue;
                        }
                        parser = new Parser(new Lexer(htmlList));
                        NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("align", "center")));
                        if (pageNode != null && pageNode.Count > 0)
                        {
                            try
                            {
                                string temp = pageNode.AsString().GetRegexBegEnd("/", "页");
                                page = int.Parse(temp);
                            }
                            catch { }
                        }
                        for (int k = 1; k <= page; k++)
                        {
                            if (k > 1)
                            {
                                try
                                {
                                    htmlList = ToolWeb.GetHtmlByUrl(url + "&page=" + k.ToString(), Encoding.Default);
                                }
                                catch { }
                            }
                            parser = new Parser(new Lexer(htmlList));
                            NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "text16")));
                            if (dtlNode != null && dtlNode.Count > 0)
                            {
                                TableTag tableTag = dtlNode[0] as TableTag;
                                for (int t = 0; t < tableTag.RowCount; t++)
                                {
                                    TableRow tr = tableTag.Rows[t];
                                    string CorpCode = string.Empty, CorpName = string.Empty, MeritYear = string.Empty, MeritName = string.Empty, MeritDate = string.Empty, MeritLevel = string.Empty, MeritRegion = string.Empty, MeritSector = string.Empty, MeritPrjName = string.Empty, PrjSupporter = string.Empty, Source = string.Empty, Url = string.Empty, Remark = string.Empty, Details = string.Empty, MeritType = string.Empty, PrjMgr = string.Empty, SupMgr = string.Empty, ManCost = string.Empty, ProArea = string.Empty, SupUnit = string.Empty, PileConsUnit = string.Empty, BuildingType = string.Empty;
                                    MeritName = MeritType = typename;
                                    if (typename.Contains("优质工程"))
                                    {
                                        MeritName = MeritType = "深圳市"+typename;
                                        MeritPrjName = tr.Columns[2].ToNodePlainString();
                                        CorpName = tr.Columns[3].ToNodePlainString();
                                        PrjMgr = tr.Columns[4].ToNodePlainString();
                                        SupUnit = tr.Columns[5].ToNodePlainString();
                                        SupMgr = tr.Columns[6].ToNodePlainString();
                                        PrjSupporter = tr.Columns[7].ToNodePlainString();
                                        string temp = tr.Columns[8].ToNodePlainString();
                                        if (temp.Contains("元"))
                                            ManCost = temp;
                                        else
                                            ProArea = temp;
                                        MeritYear = tr.Columns[9].ToNodePlainString();
                                         
                                    }
                                    else if (typename.Contains("优质结构工程"))
                                    {
                                        MeritName = MeritType = "深圳市" + typename;
                                        MeritPrjName = tr.Columns[1].ToNodePlainString();
                                        CorpName = tr.Columns[2].ToNodePlainString();
                                        PrjMgr = tr.Columns[3].ToNodePlainString();
                                        PileConsUnit = tr.Columns[4].ToNodePlainString();
                                        SupUnit = tr.Columns[5].ToNodePlainString();
                                        SupMgr = tr.Columns[6].ToNodePlainString();
                                        string temp = tr.Columns[8].ToNodePlainString();
                                        if (temp.Contains("元"))
                                            ManCost = temp;
                                        else
                                            ProArea = temp;
                                        MeritYear = tr.Columns[10].ToNodePlainString(); 
                                    }
                                    else if (typename.Contains("用户满意工程"))
                                    {
                                        MeritName = MeritType = "深圳市" + typename;
                                        MeritPrjName = tr.Columns[1].ToNodePlainString();
                                        CorpName = tr.Columns[2].ToNodePlainString();
                                        SupUnit = tr.Columns[3].ToNodePlainString();
                                        BuildingType = tr.Columns[4].ToNodePlainString();
                                        ProArea = tr.Columns[5].ToNodePlainString();
                                        MeritYear = tr.Columns[6].ToNodePlainString(); 
                                    }
                                    else if (typename.Contains("绿色施工示范工程"))
                                    {
                                        MeritName = MeritType = "深圳市" + typename;
                                        MeritPrjName = tr.Columns[2].ToNodePlainString();
                                        CorpName = tr.Columns[3].ToNodePlainString(); 
                                        PrjMgr = tr.Columns[4].ToNodePlainString();
                                        SupUnit = tr.Columns[5].ToNodePlainString();
                                        SupMgr = tr.Columns[6].ToNodePlainString();
                                        PrjSupporter = tr.Columns[8].ToNodePlainString();
                                        MeritYear = tr.Columns[10].ToNodePlainString(); 
                                    }
                                    else if (typename.Contains("文明工地") || typename.Contains("双优工地") || typename.Contains("双优样板工地"))
                                    {
                                        MeritPrjName = tr.Columns[1].ToNodePlainString();
                                        CorpName = tr.Columns[2].ToNodePlainString();
                                        PrjMgr = tr.Columns[3].ToNodePlainString();
                                        SupUnit = tr.Columns[4].ToNodePlainString();
                                        SupMgr = tr.Columns[5].ToNodePlainString();
                                        string temp = tr.Columns[6].ToNodePlainString();
                                        if (temp.Contains("元"))
                                            ManCost = temp;
                                        else
                                            ProArea = temp;
                                        MeritYear = tr.Columns[7].ToNodePlainString(); 
                                    }

                                    CorpMerit info = ToolDb.GenCorpMerit("广东省", "深圳市", "", CorpCode, CorpName, MeritYear, MeritName, MeritDate, MeritLevel, MeritRegion, MeritSector, MeritPrjName, PrjSupporter, Source, url, Remark, Details, MeritType, PrjMgr, SupMgr, ManCost, ProArea, SupUnit, PileConsUnit, BuildingType);

                                    list.Add(info);
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region 深圳地区
            parser = new Parser(new Lexer(html));
            NodeList areaNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("height", "29")), true), new TagNameFilter("table")));
            if (areaNode != null && areaNode.Count > 0)
            {
                TableTag table = areaNode[1] as TableTag;
                parser = new Parser(new Lexer(table.ToHtml()));
                NodeList listNode = parser.ExtractAllNodesThatMatch(new TagNameFilter("a"));
                if (listNode != null && listNode.Count > 0)
                {
                    for (int j = 0; j < listNode.Count; j++)
                    {
                        ATag aTag = listNode[j].GetATag();
                        string typename = aTag.LinkText.Replace("·", "");
                        string url = "http://www.jianzhuxh.com/excellence/" + aTag.Link;
                        string htmlList = string.Empty;
                        int page = 1;
                        try
                        {
                            htmlList = ToolWeb.GetHtmlByUrl(url, Encoding.Default);
                        }
                        catch
                        {
                            continue;
                        }
                        parser = new Parser(new Lexer(htmlList));
                        //continue;
                        NodeList pageNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("form"), new HasAttributeFilter("name", "gopage")));
                        if (pageNode != null && pageNode.Count > 0)
                        {
                            try
                            {
                                string temp = pageNode.AsString().GetRegexBegEnd("/", "页");
                                page = int.Parse(temp);
                            }
                            catch { }
                        }
                        for (int k = 1; k <= page; k++)
                        {
                            if (k > 1)
                            {
                                try
                                {
                                    htmlList = ToolWeb.GetHtmlByUrl(url + "?page=" + k.ToString(), Encoding.Default);
                                }
                                catch { continue; }
                            }
                            parser = new Parser(new Lexer(htmlList));
                            NodeList tableNode = null;
                            if (typename.Contains("鲁班奖"))
                            {
                                tableNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "py_tbl")));
                            }
                            else
                            {
                                tableNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("class", "text18")));
                            }
                            if (tableNode != null && tableNode.Count > 0)
                            {
                                TableTag tableTag = tableNode[0] as TableTag;
                                for (int t = 1; t < tableTag.RowCount; t++)
                                {
                                    TableRow tr = tableTag.Rows[t];
                                    string CorpCode = string.Empty, CorpName = string.Empty, MeritYear = string.Empty, MeritName = string.Empty, MeritDate = string.Empty, MeritLevel = string.Empty, MeritRegion = string.Empty, MeritSector = string.Empty, MeritPrjName = string.Empty, PrjSupporter = string.Empty, Source = string.Empty, Url = string.Empty, Remark = string.Empty, Details = string.Empty, MeritType = string.Empty, PrjMgr = string.Empty, SupMgr = string.Empty, ManCost = string.Empty, ProArea = string.Empty, SupUnit = string.Empty, PileConsUnit = string.Empty, BuildingType = string.Empty;
                                    MeritName =  MeritType = typename;
                                    MeritPrjName = tr.Columns[1].ToNodePlainString();
                                    CorpName = tr.Columns[2].ToNodePlainString();
                                    PrjSupporter = tr.Columns[3].ToNodePlainString().Replace("参建单位", "").Replace("：", "").Replace(":", "");
                                    SupUnit = tr.Columns[4].ToNodePlainString();
                                    PrjMgr = tr.Columns[5].ToNodePlainString();
                                    MeritYear = tr.Columns[6].ToNodePlainString();

                                    CorpMerit info = ToolDb.GenCorpMerit("广东省", "深圳市", "", CorpCode, CorpName, MeritYear, MeritName, MeritDate, MeritLevel, MeritRegion, MeritSector, MeritPrjName, PrjSupporter, Source, url, Remark, Details, MeritType, PrjMgr, SupMgr, ManCost, ProArea, SupUnit, PileConsUnit, BuildingType);

                                    list.Add(info);
                                }
                            }
                        }
                    }
                }
            }
            #endregion
            return list;
        }
    }
}
