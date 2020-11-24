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
namespace Crawler.Instance
{
    public class Notify : WebSiteCrawller
    {
        public Notify()
            : base()
        {
            this.Group = "其它处理";
            this.Title = "通知公告单个处理";
            this.Description = "通知公告单个处理";
            this.PlanTime = "";
            this.SiteUrl = "http://govinfo.nlc.gov.cn/gdsszfz/xxgk/szsnsqrmzf/201310/t20131018_4032393.shtml?classid=479";
            this.MaxCount = 0;
        }

        protected override IList ExecuteCrawl(bool crawlAll)
        {
            string headName = string.Empty, releaseTime = string.Empty, infoScorce = string.Empty, msgType = string.Empty, infoUrl = string.Empty, ctxHtml = string.Empty, infoCtx = string.Empty, infoType = string.Empty;

            infoType = "通知公告";
            infoUrl = this.SiteUrl;
            string htldtl = string.Empty;
            try
            {
                htldtl = ToolHtml.GetHtmlByUrl(infoUrl, Encoding.UTF8).GetJsString();
            }
            catch { }
            headName = "关于转发深圳市住房和建设局转发《深圳市交通运输委港航和货运交通管理局关于我市泥头车运输企业土石方运输业务投标资质考评和异地泥头车备案托管第二阶段情况的通报》的通知";
            ctxHtml = "<table width='960' background='{root_path}images/xil_jl_05.jpg' border='0' cellspacing='0' cellpadding='0'>  <tbody><tr> <td align='center' background='../../../images/xil_jl_03.jpg' valign='top' style='background-repeat: repeat-x;'><table width='100%' border='0' cellspacing='0' cellpadding='0'>  <tbody><tr> <td width='9%'>&nbsp;</td> <td width='83%' height='25'>&nbsp;</td>  <td width='8%'>&nbsp;</td> </tr> <tr>    <td>&nbsp;</td>   <td valign='top'><table width='100%' border='0' cellspacing='0' cellpadding='0'>      <tbody><tr>  <td width='8%' height='25' class='red12a'>题材分类：</td>  <td width='42%'><a style='text-decoration: underline; cursor: pointer;' onclick='xlsj('catalog1=327')'>通知公告公示</a></td> <td width='8%' class='red12a'>主题分类：</td> <td width='42%'><a style='text-decoration: underline; cursor: pointer;' onclick='xlsj('catalog2=479')'>其他</a></td>      </tr>    <tr>     <td height='25' class='red12a'>发文机构：</td>    <td><span id='fbjgid' style='display: none;'><script>fbjg('深圳市南山区人民政府 ')</script><a style='text-decoration: underline; cursor: pointer;' onclick='xlsj('district=深圳市南山区人民政府')'>深圳市南山区人民政府</a></span></td><script>var wh = ''; wh = wh.replace(/　/ig,''); wh = wh.replace(/ /ig,''); 	if(wh==''||wh==null||'无'==wh){ 	document.getElementById('fbjgid').style.display='none';	}</script>     <td class='red12a'>来源网站发布日期：</td>   <td><a style='text-decoration: underline; cursor: pointer;' onclick='xlsj('urltime=2013.08.12')'>2013-08-12</a></td>   </tr> <tr> <td height='25' class='red12a'>所属地区：</td>    <td><script>ssdq('广东省深圳市 ')</script><a style='text-decoration: underline; cursor: pointer;' onclick='xlsj('vreserved3=广东省深圳市')'>广东省深圳市</a>;</td>      <td class='red12a'>文&nbsp;&nbsp;&nbsp;&nbsp;号：</td>      <td><script type='text/javascript'> 	ycwh(); </script></td>       </tr>   <tr>     <td height='25' class='red12a' valign='top' style='padding-top: 8px;'>关 键 词：</td> <td valign='top' style='line-height: 20px; padding-top: 3px;'><script>gjzsj('深圳市;泥头车;货运交通;交通运输;备案;港航;土石方运输;投标资质;考评;异地')</script><a style='text-decoration: underline; cursor: pointer;' onclick='xlsj('keywords=深圳市')'>深圳市</a>;<a style='text-decoration: underline; cursor: pointer;' onclick='xlsj('keywords=泥头车')'>泥头车</a>;<a style='text-decoration: underline; cursor: pointer;' onclick='xlsj('keywords=货运交通')'>货运交通</a>;<a style='text-decoration: underline; cursor: pointer;' onclick='xlsj('keywords=交通运输')'>交通运输</a>;<a style='text-decoration: underline; cursor: pointer;' onclick='xlsj('keywords=备案')'>备案</a>;<a style='text-decoration: underline; cursor: pointer;' onclick='xlsj('keywords=港航')'>港航</a>;<a style='text-decoration: underline; cursor: pointer;' onclick='xlsj('keywords=土石方运输')'>土石方运输</a>;<a style='text-decoration: underline; cursor: pointer;' onclick='xlsj('keywords=投标资质')'>投标资质</a>;<a style='text-decoration: underline; cursor: pointer;' onclick='xlsj('keywords=考评')'>考评</a>;<a style='text-decoration: underline; cursor: pointer;' onclick='xlsj('keywords=异地')'>异地</a>;</td>        <td class='red12a'>公文发布日期：</td>       <td><a style='text-decoration: underline; cursor: pointer;' onclick='xlsj('urldate=')'></a></td>          </tr>   </tbody></table></td>   <td>&nbsp;</td> </tr>   </tbody></table></td>  </tr>   <tr>    <td bgcolor='#ffffff'><img width='943' height='8' src='../../../images/xil_jl_06.jpg'></td> </tr> </tbody></table>    <table width='960' bgcolor='#ffffff' border='0' cellspacing='0' cellpadding='0'>      <tbody><tr> <td align='center' valign='top'><table width='830' border='0' cellspacing='0' cellpadding='0'>  <tbody><tr>        <td align='center' class='dbiaoti' style='padding: 15px 0px;'>关于转发深圳市住房和建设局转发《深圳市交通运输委港航和货运交通管理局关于我市泥头车运输企业土石方运输业务投标资质考评和异地泥头车备案托管第二阶段情况的通报》的通知</td>    </tr>       </tbody></table>   <table width='830' border='0' cellspacing='0' cellpadding='0'>    <tbody><tr>       <td><table width='100%' background='../../../images/erj_jl_122_28.jpg' border='0' cellspacing='0' cellpadding='0'>        <tbody><tr>    <td width='12'><img width='12' height='34' src='../../../images/erj_jl_121_25.jpg'></td>     <td><table width='100%' height='25' align='center' border='0' cellspacing='0' cellpadding='0'>   <tbody><tr>   <td class='fff12'>来源：<script>lyjs('深圳市南山区人民政府')</script><a style='text-decoration: underline; cursor: pointer;' onclick='xlsj('sitename=深圳市南山区人民政府')'>深圳市南山区人民政府</a>;</td>    <td width='80'><a onclick='checkUrl('http://www.szns.gov.cn/publish/main/1/19/tzgg/20130812110509651949516/index.html','关于转发深圳市住房和建设局转发《深圳市交通运输委港航和货运交通管理局关于我市泥头车运输企业土石方运输业务投标资质考评和异地泥头车备案托管第二阶段情况的通报》的通知','4032393');' href='#'>原文链接 &gt;&gt;</a></td>   <td width='80'><a href='/search/htmlflash4Radar?docid=4032393'>网页快照</a> &gt;&gt; </td>   </tr>            </tbody></table></td>     <td width='8'><img width='8' height='34' src='../../../images/erj_jl_123_30.jpg'></td>     </tr>       </tbody></table></td>    </tr>      </tbody></table>    <table width='830' border='0' cellspacing='0' cellpadding='0'>  <tbody><tr>   <td class='zw_link' valign='top' style='padding: 20px 0px 0px;'>　 		  <br><br>各有关单位：<br>　　现将《深圳市交通运输委港航和货运交通管理局关于我市泥头车运输企业土石方运输业务投标资质考评和异地泥头车备案托管第二阶段情况的通报》（深交港货[2013]164号）转发给你们，请遵照执行。目前，共有46家泥头车运输企业已获取我市土石方运输业务投标资质；共有82家异地企业204辆泥头车，分别与12家土石方运输业务投标资质企业达成了备案托管。<br>　　特此通知。<br>　　联系人：李衍航，电话：83788608。　<br>　　附件：深交港货[2013]164号<br>　　深圳市住房和建设局<br>　　　　2013年8月9日<br>&nbsp;<br><br><br><br>          <script type='text/javascript'> 		qufj();   </script><a href='./P020131018007991034107.pdf'> 附件：深交港货[2013]164号 </a><br>   </td>  </tr>  </tbody></table>     <table width='100%' border='0' cellspacing='0' cellpadding='0'>     <tbody><tr>    <td>&nbsp;</td>   </tr>   </tbody></table></td>    </tr>  </tbody></table>";
            //infoCtx = ctxHtml.GetJsString().Replace("<tr>", "").Replace("</tr>", "").Replace("<br>", "\r\n").ToCtxString().Replace("&gt;", "");
            Parser parser = new Parser(new Lexer(htldtl));
            NodeList nodeList = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("td"), new HasAttributeFilter("background", "../../../images/sd_in_09.jpg")));
            if (nodeList != null && nodeList.Count > 0)
            {
                infoCtx = nodeList.AsHtml().Replace("<br>", "\r\n").ToCtxString().Replace("：\r\n", "：").Replace("&gt;", "");
            }
            msgType = infoScorce = "深圳市住房和建设局";
            releaseTime = "2013-08-09";
            NotifyInfo info = ToolDb.GenNotifyInfo(headName, releaseTime, infoScorce, msgType, infoUrl, ctxHtml, "广东省", "深圳市工程", string.Empty, infoCtx, infoType);
            if (ToolDb.SaveEntity(info, this.ExistCompareFields,this.ExistsUpdate))
            {
                BaseAttach attach = ToolHtml.GetBaseAttach("http://govinfo.nlc.gov.cn/gdsszfz/xxgk/szsnsqrmzf/201310/P020131018007991034107.pdf", "深交港货[2013]164号", info.Id);
                if (attach != null)
                {
                    ToolDb.SaveEntity(attach, string.Empty);
                }
            }
            return null;
        }

    }
}
