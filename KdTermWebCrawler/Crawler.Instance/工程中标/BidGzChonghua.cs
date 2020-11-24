using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;
using Crawler;
using System.Threading;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Tags;
using System.Windows.Forms;

namespace Crawler.Instance
{
    /// <summary>
    /// 广东广州从化
    /// </summary>
    public class BidGzChonghua : WebSiteCrawller
    {
        public BidGzChonghua()
            : base(true)
        {
            this.Group = "中标信息";
            this.Title = "广东省从化市";
            this.PlanTime = "01:08,9:08,10:08,11:38,14:08,15:08,17:38";
            this.Description = "自动抓取广东省从化市中标信息";
            this.ExistCompareFields = "Prov,City,Area,Road,Code,ProjectName";
            this.SiteUrl = "http://www.chjssz.gov.cn/NewsClassztbgl2.asp?bigclass=招投标管理&SmallClass=中标公示";
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {

            IList list = new ArrayList();

            //取得页码
            int pageInt = 1; 
            string html = string.Empty;
            string HtmlTxt = string.Empty;
            try
            {
               html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl), Encoding.Default);
            }
            catch (Exception ex)
            {
                  
                return list;
            }
           
            Parser parser = new Parser(new Lexer(html));
            NodeList sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new HasParentFilter(new TagNameFilter("form")), new TagNameFilter("a")));
            if (sNode != null && sNode.Count > 0)
            {
                for (int i = 0; i < sNode.Count; i++)
                {

                    ATag pageA = sNode[i] as ATag;
                    if (pageA.ToPlainTextString().Contains("尾页"))
                    {
                        try
                        {
                            pageInt = int.Parse(pageA.Link.Remove(0, pageA.Link.LastIndexOf("=") + 1));
                        }
                        catch (Exception)
                        {


                        }
                    }

                }
            }

            parser.Reset();

            for (int i = 1; i <= pageInt; i++)
            {
                try
                {
                    html = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(SiteUrl + "&Page=" + i.ToString()), Encoding.Default);
                }
                catch (Exception ex)
                {
                      
                    continue;
                }
             
                parser = new Parser(new Lexer(html));
                sNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("table"), new HasAttributeFilter("bordercolor", "#CCCCCC")));
                if (sNode != null && sNode.Count > 0)
                {
                    HtmlTxt = sNode.AsHtml();
                    string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty,
                        bidDate = string.Empty, beginDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty;
                    StringBuilder ctx = new StringBuilder();
                    decimal decMoney = 0;
                    TableTag table = sNode[1] as TableTag;
                    for (int j = 1; j < table.RowCount; j++)
                    {
                        TableRow tr = table.Rows[j];
                        //招标类型
                        bidType = tr.Columns[0].ToPlainTextString();

                        string invType = "施工,设计,勘察,服务,劳务分包,专业分包,小型施工,监理,设备材料,其他";
                        if (invType.Contains(bidType))
                        {
                            specType = "建设工程";
                        } 
                        else 
                        {
                            specType = "其他";
                        }

                        //项目名称
                        prjName = tr.Columns[1].ToPlainTextString().Replace("&nbsp;", "");
                       

                        //中标单位
                        bidUnit = tr.Columns[2].ToPlainTextString().Replace("&nbsp;", "");
                        
                        
                        //发布时间
                        bidDate = tr.Columns[3].ToPlainTextString().TrimStart('[').TrimEnd(']');

                        NodeList cNode = new NodeList();
                        //进行搜索子节点A标签
                        tr.Columns[1].CollectInto(cNode, new TagNameFilter("a"));


                        InfoUrl = "http://www.chjssz.gov.cn/" + (cNode[0] as ATag).Link;
                        prjName = ToolDb.GetPrjName(prjName);
                        bidType = ToolHtml.GetInviteTypes(bidType);
                        BidInfo info = ToolDb.GenBidInfo("广东省", "广州市区", "从化市", string.Empty, code, prjName, string.Empty, bidDate, bidUnit, bidDate, string.Empty, "见附件", string.Empty, "广州建设工程交易中心", bidType, specType, string.Empty, string.Empty, InfoUrl,string.Empty, HtmlTxt);
                        list.Add(info);


                        //采集内容页
                        string dlHtml = string.Empty;
                        try
                        {
                            dlHtml = this.ToolWebSite.GetHtmlByUrl(this.ToolWebSite.UrlEncode(InfoUrl), Encoding.Default);
                        }
                        catch (Exception ex)
                        {
                              
                            continue;
                        }
                     
                        Parser dlParser = new Parser(new Lexer(dlHtml));
                        NodeList dlNodes = dlParser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("background", "pic/abouts_16.jpg")));
                        if (dlNodes != null && dlNodes.Count > 0)
                        {

                            NodeList ddNode = dlNodes.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("A"), new HasAttributeFilter("target", "_blank")), true);
                            if (ddNode != null && ddNode.Count > 0)
                            {
                                for (int k = 0; k < ddNode.Count; k++)
                                {
                                    ATag ddATag = ddNode[k] as ATag;
                                    if (ddATag.Link.Contains("UploadFiles"))
                                    {
                                        BaseAttach attach = ToolDb.GenBaseAttach(ddATag.StringText, info.Id, "http://www.chjssz.gov.cn/" + ddATag.Link);
                                        base.AttachList.Add(attach);
                                    }

                                }
                                dlParser.Reset();
                            }
                        }
                        if (!crawlAll && list.Count >= this.MaxCount) return list;
                    }
                }


            }

            return list;
        }


    }

}
