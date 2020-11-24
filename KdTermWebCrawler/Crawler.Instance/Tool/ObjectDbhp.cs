using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Crawler.Instance;
using Winista.Text.HtmlParser;using Crawler.Base.KdService;using Crawler.Base.KdService;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Tags;
using Winista.Text.HtmlParser.Util;
using System.Windows.Forms;


namespace System
{
    public static class ObjectDbhp
    {
        /// <summary>
        /// 删除项目名称的项目编号
        /// </summary>
        /// <param name="prjName"></param>
        /// <returns></returns>
        public static string GetPrjNameDelCode(this string prjName)
        {
            if (prjName.Contains("(项目编号"))
                prjName = prjName.Remove(prjName.IndexOf("(项目编号"));
            if (prjName.Contains("（项目编号"))
                prjName = prjName.Remove(prjName.IndexOf("（项目编号"));
            if (prjName.Contains("【项目编号"))
                prjName = prjName.Remove(prjName.IndexOf("【项目编号"));
            if (prjName.Contains("[项目编号"))
                prjName = prjName.Remove(prjName.IndexOf("[项目编号"));
            if (prjName.Contains("（采购编号"))
                prjName = prjName.Remove(prjName.IndexOf("（采购编号"));
            if (prjName.Contains("(采购编号"))
                prjName = prjName.Remove(prjName.IndexOf("(采购编号"));
            if (prjName.Contains("【采购编号"))
                prjName = prjName.Remove(prjName.IndexOf("【采购编号"));
            if (prjName.Contains("[采购编号"))
                prjName = prjName.Remove(prjName.IndexOf("[采购编号"));
            if (prjName.Contains("（工程编号"))
                prjName = prjName.Remove(prjName.IndexOf("（工程编号"));
            if (prjName.Contains("(工程编号"))
                prjName = prjName.Remove(prjName.IndexOf("(工程编号"));
            if (prjName.Contains("【工程编号"))
                prjName = prjName.Remove(prjName.IndexOf("【工程编号"));
            if (prjName.Contains("[工程编号"))
                prjName = prjName.Remove(prjName.IndexOf("[工程编号"));
            if (prjName.Contains("（招标编号"))
                prjName = prjName.Remove(prjName.IndexOf("（招标编号"));
            if (prjName.Contains("(招标编号"))
                prjName = prjName.Remove(prjName.IndexOf("(招标编号"));
            if (prjName.Contains("【招标编号"))
                prjName = prjName.Remove(prjName.IndexOf("【招标编号"));
            if (prjName.Contains("[招标编号"))
                prjName = prjName.Remove(prjName.IndexOf("[招标编号"));
            if (prjName.Contains("（采购项目"))
                prjName = prjName.Remove(prjName.IndexOf("（采购项目"));
            if (prjName.Contains("(采购项目"))
                prjName = prjName.Remove(prjName.IndexOf("(采购项目"));
            if (prjName.Contains("【采购项目"))
                prjName = prjName.Remove(prjName.IndexOf("【采购项目"));
            if (prjName.Contains("[采购项目"))
                prjName = prjName.Remove(prjName.IndexOf("[采购项目"));
            if (prjName.Contains("（谈判编号"))
                prjName = prjName.Remove(prjName.IndexOf("（谈判编号"));
            if (prjName.Contains("(谈判编号"))
                prjName = prjName.Remove(prjName.IndexOf("(谈判编号"));
            if (prjName.Contains("【谈判编号"))
                prjName = prjName.Remove(prjName.IndexOf("【谈判编号"));
            if (prjName.Contains("[谈判编号"))
                prjName = prjName.Remove(prjName.IndexOf("[谈判编号"));
            return prjName;
        }
        /// <summary>
        /// 删除编号无用的括号
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string GetCodeDel(this string code)
        {
            if (code.Contains("）") && !code.Contains("（"))
                code = code.Remove(code.IndexOf("）"));
            if (code.Contains("]") && !code.Contains("["))
                code = code.Remove(code.IndexOf("]"));
            if (code.Contains(")") && !code.Contains("("))
                code = code.Remove(code.IndexOf(")"));
            if (code.Contains("】") && !code.Contains("【"))
                code = code.Remove(code.IndexOf("】"));

            return code;
        }
        /// <summary>
        /// 去除中标单位中的括号等
        /// </summary>
        /// <param name="bidUnit"></param>
        /// <returns></returns>
        public static string GetBidUnitDel(this string bidUnit)
        {
            if (!string.IsNullOrEmpty(bidUnit))
                return bidUnit.Replace("（联合体）", "").Replace("(联合体)", "").Replace("（", "").Replace("）", "").Replace("(", "").Replace(")", "").Replace("成交", "").Replace("中标", "");
            if (bidUnit.Contains("金额"))
                bidUnit = bidUnit.Remove(bidUnit.IndexOf("金额"));
            return bidUnit;
        }
        /// <summary>
        /// 获取页面input值
        /// </summary>
        /// <param name="htl"></param>
        /// <param name="inputId"></param>
        /// <returns></returns>
        public static string GetInputValue(this string htl, string inputId)
        {
            return ToolHtml.GetHtmlInputValue(htl, inputId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="strValues"></param>
        /// <returns></returns>
        public static string GetReplace(this string value, string[] strValues, string replaceValue = "")
        {
            foreach (string val in strValues)
            {
                if (string.IsNullOrEmpty(replaceValue))
                    value = value.Replace(val, "");
                else
                    value = value.Replace(val, replaceValue);
            }
            return value;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="strValues"></param>
        /// <returns></returns>
        public static string GetReplace(this string value, string strValues, string replaceValue = "")
        {
            return value.GetReplace(strValues.Split(','), replaceValue);
        }
        /// <summary>
        /// 获取Atag
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ATag GetATag(this string value, int i = 0)
        {
            return ToolHtml.GetHtmlAtag(value, null, i);
        }
        /// <summary>
        /// 获取Atag
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ATag GetATag(this CompositeTag value, int i = 0)
        {
            return ToolHtml.GetHtmlAtag(value.ToHtml(), null, i);
        }
        /// <summary>
        /// 获取Atag
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ATag GetATag(this INode value, int i = 0)
        {
            return ToolHtml.GetHtmlAtag(value.ToHtml(), null, i);
        }
        /// <summary>
        /// 获取Atag
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ATag GetATag(this NodeList value, int i = 0)
        {
            return ToolHtml.GetHtmlAtag(value.ToHtml(), null, i);
        }

        public static Span GetSpan(this string value, int i = 0)
        {
            return ToolHtml.GetHtmlSpan(value, null, i);
        }
        public static Span GetSpan(this CompositeTag value, int i = 0)
        {
            return ToolHtml.GetHtmlSpan(value.ToHtml(), null, i);
        }
        public static Span GetSpan(this INode value, int i = 0)
        {
            return ToolHtml.GetHtmlSpan(value.ToHtml(), null, i);
        }
        public static Span GetSpan(this NodeList value, int i = 0)
        {
            return ToolHtml.GetHtmlSpan(value.ToHtml(), null, i);
        }

        public static TableTag GetTableTag(this string value, int i = 0)
        {
            return ToolHtml.GetHtmlTableTag(value, null, i);
        }
        public static TableTag GetTableTag(this CompositeTag value, int i = 0)
        {
            return ToolHtml.GetHtmlTableTag(value.ToHtml(), null, i);
        }
        public static TableTag GetTableTag(this INode value, int i = 0)
        {
            return ToolHtml.GetHtmlTableTag(value.ToHtml(), null, i);
        }
        public static TableTag GetTableTag(this NodeList value, int i = 0)
        {
            return ToolHtml.GetHtmlTableTag(value.ToHtml(), null, i);
        }
        /// <summary>
        /// 获取Html里A标签href
        /// </summary>
        /// <param name="value"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        public static string GetATagHref(this string value, int i = 0)
        {
            return ToolHtml.GetHtmlAtag(value, null, i).Link;
        }
        /// <summary>
        /// 获取Html里A标签href
        /// </summary>
        /// <param name="value"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        public static string GetATagHref(this CompositeTag value, int i = 0)
        {
            return ToolHtml.GetHtmlAtag(value.ToHtml(), null, i).Link;
        }
        /// <summary>
        /// 将<br>标签替换为\r\n
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetCtxBr(this string value, string reValue = "\r\n")
        {
            return value.Replace("<br />", reValue).Replace("<BR />", reValue).Replace("<br/>", reValue).Replace("<BR/>", reValue).Replace("<br>", reValue).Replace("<BR>", reValue);
        }
        /// <summary>
        /// 获取Html里A标签href
        /// </summary>
        /// <param name="value"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        public static string GetATagHref(this INode value, int i = 0)
        {
            return ToolHtml.GetHtmlAtag(value.ToHtml(), null, i).Link;
        }
        /// <summary>
        /// 获取Html里A标签href
        /// </summary>
        /// <param name="value"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        public static string GetATagHref(this NodeList value, int i = 0)
        {
            return ToolHtml.GetHtmlAtag(value.ToHtml(), null, i).Link;
        }
        /// <summary>
        /// 获取ATag属性值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="strName">ATag属性</param>
        /// <param name="i"></param>
        /// <returns></returns>
        public static string GetATagValue(this string value, string strName = "href", int i = 0)
        {
            return ToolHtml.GetHtmlAtagValue(strName, value, null, i);
        }
        /// <summary>
        /// 获取ATag属性值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="strName">ATag属性</param>
        /// <param name="i"></param>
        /// <returns></returns>
        public static string GetATagValue(this CompositeTag value, string strName = "href", int i = 0)
        {
            return ToolHtml.GetHtmlAtagValue(strName, value.ToHtml(), null, i);
        }
        /// <summary>
        /// 获取ATag属性值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="strName">ATag属性</param>
        /// <param name="i"></param>
        /// <returns></returns>
        public static string GetATagValue(this INode value, string strName = "href", int i = 0)
        {
            return ToolHtml.GetHtmlAtagValue(strName, value.ToHtml(), null, i);
        }
        /// <summary>
        /// 获取ATag属性值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="strName">ATag属性</param>
        /// <param name="i"></param>
        /// <returns></returns>
        public static string GetATagValue(this NodeList value, string strName = "href", int i = 0)
        {
            return ToolHtml.GetHtmlAtagValue(strName, value.ToHtml(), null, i);
        }
        /// <summary>
        /// 获取两个字符之间的字符串
        /// </summary>
        /// <param name="value"></param>
        /// <param name="strBegin"></param>
        /// <param name="strEnd"></param>
        /// <returns></returns>
        public static string GetRegexBegEnd(this string value, string strBegin, string strEnd, int len = 150)
        {
            string returnStr = ToolHtml.GetRegexString(value, strBegin, strEnd);
            if (Encoding.Default.GetByteCount(returnStr) > len)
                return string.Empty;
            return returnStr;
        }
        /// <summary>
        /// 日期匹配
        /// </summary>
        /// <param name="value"></param>
        /// <param name="floats"></param>
        /// <returns></returns>
        public static string GetDateRegex(this string value, string floats = "yyyy-MM-dd")
        {
            return ToolHtml.GetRegexDateTime(value, floats);
        }
        /// <summary>
        /// 匹配招标单位
        /// </summary>
        /// <param name="value">匹配字符串</param>
        /// <param name="isMon">匹配模式，是否带上冒号</param>
        /// <param name="len">匹配结果字符长度</param>
        /// <returns></returns>
        public static string GetBuildRegex(this string value, string[] build = null, bool isMon = true, int len = 150)
        {
            string str = string.Empty;
            if (build == null)
                str = ToolHtml.GetRegexString(value, ToolHtml.BuildRegex, isMon);
            else
                str = ToolHtml.GetRegexString(value, build, isMon);
            return Encoding.Default.GetByteCount(str) > len ? string.Empty : str;
        }
        /// <summary>
        /// CompositeTag转换字符串
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToNodePlainString(this CompositeTag value)
        {
            return value.ToPlainTextString().ToNodeString().Replace(" ", "");
        }
        /// <summary>
        /// INode转换字符串
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToNodePlainString(this INode value)
        {
            return value.ToPlainTextString().ToNodeString();
        }
        /// <summary>
        /// 匹配中标单位
        /// </summary>
        /// <param name="value">匹配字符串</param>
        /// <param name="isMon">匹配模式，是否带上冒号</param>
        /// <param name="len">匹配结果字符长度</param>
        /// <returns></returns>
        public static string GetBidRegex(this string value, string[] bid = null, bool isMon = true, int len = 150)
        {
            string str = string.Empty;
            if (bid == null)
                str = ToolHtml.GetRegexString(value, ToolHtml.BidRegex, isMon);
            else
                str = ToolHtml.GetRegexString(value, bid, isMon);
            return Encoding.Default.GetByteCount(str) > len ? string.Empty : str;
        }
        /// <summary>
        /// 正则匹配
        /// </summary>
        /// <param name="value"></param>
        /// <param name="str"></param>
        /// <param name="isMon"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static string GetRegex(this string value, string[] str, bool isMon = true, int len = 150)
        {
            string strValue = ToolHtml.GetRegexStrings(value, str, isMon);
            return Encoding.Default.GetByteCount(strValue) > len ? string.Empty : strValue;
        }
        /// <summary>
        /// 正则匹配
        /// </summary>
        /// <param name="value"></param>
        /// <param name="str"></param>
        /// <param name="isMon"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static string GetRegex(this string value, string str, bool isMon = true, int len = 150)
        {
            return value.GetRegex(str.Split(','), isMon, len);
        }
        /// <summary>
        /// 匹配地址
        /// </summary>
        /// <param name="value">匹配字符串</param>
        /// <param name="isMon">匹配模式，是否带上冒号</param>
        /// <param name="len">匹配结果字符长度</param>
        /// <returns></returns>
        public static string GetAddressRegex(this string value, string[] adr = null, bool isMon = true, int len = 50)
        {
            string str = string.Empty;
            if (adr == null)
                str = ToolHtml.GetRegexString(value, ToolHtml.AddressRegex, isMon);
            else
                str = ToolHtml.GetRegexString(value, adr, isMon);
            return Encoding.Default.GetByteCount(str) > len ? string.Empty : str;
        }
        /// <summary>
        /// 匹配项目经理
        /// </summary>
        /// <param name="value">匹配字符串</param>
        /// <param name="isMon">匹配模式，是否带上冒号</param>
        /// <param name="len">匹配结果字符长度</param>
        /// <returns></returns>
        public static string GetMgrRegex(this string value, string[] mgr = null, bool isMon = true, int len = 50)
        {
            string str = string.Empty;
            if (mgr == null)
                str = ToolHtml.GetRegexString(value, ToolHtml.MgrRegex, isMon);
            else
                str = ToolHtml.GetRegexString(value, mgr, isMon);
            return Encoding.Default.GetByteCount(str) > len ? string.Empty : str;
        }
        /// <summary>
        /// 匹配金额
        /// </summary>
        /// <param name="value">匹配字符串</param>
        /// <param name="isMon">匹配模式，是否带上冒号</param>
        /// <param name="len">匹配结果字符长度</param>
        /// <returns></returns>
        public static string GetMoneyRegex(this string value, string[] money = null, bool isMon = false, string mon = "万", int len = 100, string lastStr = "\r\n")
        {
            string str = string.Empty;
            if (money == null)
                str = ToolHtml.GetRegexString(value, ToolHtml.MoneyRegex, isMon, lastStr);
            else
                str = ToolHtml.GetRegexString(value, money, isMon, lastStr);
            string moneys = ToolHtml.GetRegexMoney(str, mon);
            return Encoding.Default.GetByteCount(moneys) > len ? string.Empty : moneys;
        }
        /// <summary>
        /// 是否包含数字
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNumber(this string value)
        {
            Regex reg = new Regex(@"[0-9]");
            string strValue = reg.Match(value).Value;
            return !string.IsNullOrEmpty(strValue);
        }

        /// <summary>
        /// 判断是否包含英文和数字
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNatural_Number(this string str)
        {
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(@"^[A-Za-z0-9]+$");
            return reg.IsMatch(str);
        }
        /// <summary>
        /// 判断是否包含英文
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNatural(this string str)
        {
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(@"^[A-Za-z]+$");
            return reg.IsMatch(str);
        }

        /// <summary>
        /// 判断是否[]{}
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsJsonKh(this string str)
        {
            return str.Contains("[") ||
               str.Contains("]") ||
               str.Contains("{") ||
           str.Contains("}");

        }
        /// <summary>
        /// 判断是否[]{}
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsJsonKh(this char str)
        {
            return str.Equals("[") ||
               str.Equals("]") ||
               str.Equals("{") ||
           str.Equals("}");

        }

        /// <summary>
        /// 获取金额字符串
        /// </summary>
        /// <param name="value"></param>
        /// <param name="money"></param>
        /// <param name="isMon"></param>
        /// <param name="mon"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static string GetMoneyString(this string value, string[] money = null, bool isMon = false, string mon = "万", int len = 100)
        {
            string str = string.Empty;
            if (money == null)
                str = ToolHtml.GetRegexString(value, ToolHtml.MoneyRegex, isMon);
            else
                str = ToolHtml.GetRegexString(value, money, isMon);
            return Encoding.Default.GetByteCount(str) > len ? string.Empty : str;

        }
        /// <summary>
        /// 金额处理
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetMoney(this string value, string mon = "万")
        {
            return ToolHtml.GetRegexMoney(value, mon);
        }
        /// <summary>
        ///  匹配日期
        /// </summary>
        /// <param name="value"></param>
        /// <param name="date"></param>
        /// <param name="isMon"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static string GetTimeRegex(this string value, string[] date = null, bool isMon = true, int len = 100)
        {
            string str = string.Empty;
            if (date == null)
                str = ToolHtml.GetRegexString(value, ToolHtml.DateRegex, isMon);
            else
                str = ToolHtml.GetRegexString(value, date, isMon);
            return Encoding.Default.GetByteCount(str) > len ? string.Empty : str;
        }
        /// <summary>
        /// 匹配编号
        /// </summary>
        /// <param name="value">匹配字符串</param>
        /// <param name="isMon">匹配模式，是否带上冒号</param>
        /// <param name="len">匹配结果字符长度</param>
        /// <returns></returns>
        public static string GetCodeRegex(this string value, string[] code = null, bool isMon = true, int len = 50)
        {
            string str = string.Empty;
            if (code == null)
                str = ToolHtml.GetRegexString(value, ToolHtml.CodeRegex, isMon);
            else
                str = ToolHtml.GetRegexString(value, code, isMon);
            return Encoding.Default.GetByteCount(str) > len ? string.Empty : str;
        }

        public static string GetNoticePrjCode(this string strValue, string code = null, bool isMon = false, int len = 50)
        {
            Regex reg = null;
            string values = string.Empty, names = string.Empty;
            string[] strName = null;
            if (!string.IsNullOrEmpty(code)) code.Split(',');
            else strName = ToolHtml.CodeRegex;
            for (int i = 0; i < strName.Length; i++)
            {
                if (i == strName.Length - 1)
                    names += strName[i];
                else
                    names += strName[i] + "|";
            }
            if (isMon)
            {
                reg = new Regex(@"(" + names + ")(:|：)[^\n]+\n");
            }
            else
            {
                reg = new Regex(@"(" + names + ")(:|：|)[^\n]+\n");
            }
            values = reg.Match(strValue).Value.Replace("：", "").Replace(":", "").Replace("\r", "").Replace("\t", "").Replace("\n", "").Replace("，", "").Replace(",", "").Replace("；", "").Replace(";", "");
            for (int j = 0; j < strName.Length; j++)
            {
                values = values.Replace(strName[j], "");
            }
            return values.Replace("。", "").Replace("，", "");
        }

        /// <summary>
        /// 去除换行符
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToNodeString(this object value)
        {
            return Convert.ToString(value).Replace("&nbsp;", "").Replace("&nbsp", "").Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace("&middot;", "");
        }
        /// <summary>
        /// 去除Html字符串中所有标签
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToCtxString(this string value)
        {
            return Regex.Replace(Convert.ToString(value), "<[^>]*>", "").Replace("&nbsp;", "").Replace(" ", "").Replace("\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\t\t", "").Replace("\r\r", "\r").Replace("\n\n", "\n");
        }
        /// <summary>
        /// 去除汉子
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetChina(this string value)
        {
            return Regex.Replace(value, @"[\u4E00-\u9FA5]", "");
        }
        /// <summary>
        /// 获取中文
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetNotChina(this string value)
        {
            return Regex.Replace(value, @"[^\u4E00-\u9FA5]", "");
        }
        /// <summary>
        /// 是否包含汉字
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsChina(this string value)
        {
            string temp = Regex.Replace(value, @"[^\u4E00-\u9FA5]", "");
            return !string.IsNullOrEmpty(temp);
        }
        /// <summary>
        /// 去除字符串空格、空格符、换行符、冒号等
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToRegString(this object value)
        {
            return Convert.ToString(value).ToLower().Replace(" ", "").Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace("&nbsp;", "").Replace("：", "").Replace(":", "");
        }
        /// <summary>
        /// 去除Html里的JS、Css
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetJsString(this string value)
        {
            return System.Text.RegularExpressions.Regex.Replace(value, "(<script)[\\s\\S]*?(</script>)|(<style)[\\s\\S]*?(</style>)|(<SCRIPT)[\\s\\S]*?(</SCRIPT>)|(<STYLE)[\\s\\S]*?(</STYLE>)", "");
        }
        /// <summary>
        /// 获取招中标类型
        /// </summary>
        /// <param name="strValue"></param>
        /// <returns></returns>
        public static string GetInviteBidType(this string strValue)
        {
            string strName = string.Empty;
            if (strValue.Contains("施工"))
            {
                strName = "施工";
            }
            if (strValue.Contains("监理"))
            {
                strName = "监理";
            }
            if (strValue.Contains("设计"))
            {
                strName = "设计";
            }
            if (strValue.Contains("勘察"))
            {
                strName = "勘察";
            }
            if (strValue.Contains("服务"))
            {
                strName = "服务";
            }
            if (strValue.Contains("劳务分包"))
            {
                strName = "劳务分包";
            }
            if (strValue.Contains("专业分包"))
            {
                strName = "专业分包";
            }
            if (strValue.Contains("小型施工"))
            {
                strName = "小型工程";
            }
            if (strValue.Contains("设备材料"))
            {
                strName = "设备材料";
            }
            return strName == string.Empty ? "" : strName;
        }
        /// <summary>
        ///  判断该A标签是否为附件
        /// </summary>
        /// <param name="aTagLink">A标签Html</param>
        /// <param name="fie">附件类型</param>
        /// <param name="fies">多个附件类型</param>
        /// <returns></returns>
        public static bool IsAtagAttach(this string aTagLink, string fie = null, string[] fies = null)
        {
            string strValue = string.Empty;
            try
            {
                strValue = System.IO.Path.GetExtension(aTagLink.ToLower().Replace(".aspx", "").Replace(".asp", "").Replace(".html", "").Replace(".htm", "").Replace(".jsp", "").Replace(".php", "").Replace(".ashx", ""));
            }
            catch { return false; }
            if (!string.IsNullOrEmpty(fie))
            {
                if (strValue == fie)
                    return true;
            }
            else if (fies != null)
            {
                for (int i = 0; i < fies.Length; i++)
                {
                    if (strValue == fies[i])
                        return true;
                }
            }
            else
            {
                for (int i = 0; i < ToolHtml.AttachName.Length; i++)
                {
                    if (strValue == ToolHtml.AttachName[i])
                        return true;
                }
            }
            try
            {
                if (strValue.ToLower().Contains(".pdf") || strValue.ToLower().Contains(".zip") || strValue.ToLower().Contains(".rar") ||
                    strValue.ToLower().Contains(".doc") || strValue.ToLower().Contains(".docx") || strValue.ToLower().Contains(".xls") ||
                    strValue.ToLower().Contains(".xlsx") || strValue.ToLower().Contains(".txt") || strValue.ToLower().Contains(".pdf") ||
                    strValue.ToLower().Contains(".mpp") || strValue.ToLower().Contains(".ppt") || strValue.ToLower().Contains("xml") ||
                    strValue.ToLower().Contains(".jpg") || strValue.ToLower().Contains(".jpeg") || strValue.ToLower().Contains("png"))
                    return true;
            }
            catch { }
            return false;
        }
        /// <summary>
        ///  判断该A标签是否为附件
        /// </summary>
        /// <param name="aTag">A标签</param>
        /// <param name="fie">附件类型</param>
        /// <param name="fies">多个附件类型</param>
        /// <returns></returns>
        public static bool IsAtagAttach(this ATag aTag, string fie = null, string[] fies = null)
        {
            if (!aTag.LinkText.IsAtagAttach(fie, fies))
            {
                return aTag.Link.IsAtagAttach(fie, fies);
            }
            return true;
        }
        /// <summary>
        /// 去除字符串两边括号
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string GetPrjNameByName(this string str)
        {
            return ToolHtml.GetPrjNameByName(str);
        }
        /// <summary>
        /// 判断是否是图片后缀名
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsImage(this string str, string[] strLen = null)
        {
            string strImg = System.IO.Path.GetExtension(str).ToLower();
            if (strLen != null)
            {
                for (int i = 0; i < strLen.Length; i++)
                {
                    if (strImg == strLen[i])
                        return true;
                }
            }
            else
            {
                for (int i = 0; i < ToolHtml.AttachImg.Length; i++)
                {
                    if (strImg == ToolHtml.AttachImg[i])
                        return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 判断是否是图片后缀名
        /// </summary>
        /// <param name="aTag"></param>
        /// <param name="strLen"></param>
        /// <returns></returns>
        public static bool IsImage(this ATag aTag, string[] strLen = null)
        {
            if (aTag.Link.IsImage(strLen))
                return true;
            else
                return aTag.LinkText.IsImage(strLen);
        }
        /// <summary>
        /// 首尾字符串对换
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string GetBegEnd(this string str)
        {
            string value = string.Empty;
            for (int i = str.Length - 1; i >= 0; i--)
            {
                value += str[i];
            }
            return value;
        }
        /// <summary>
        /// 匹配中文日期
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string GetChinaTime(this string time)
        {
            Regex reg = new Regex(@"[^u4e00-u9fa5]+\d{4}年[^u4e00-u9fa5]{1,3}月[^u4e00-u9fa5]{1,3}日");
            DateUtility dateUtility = new DateUtility();
            try
            {
                return dateUtility.GetWoldDate(reg.Match(time).Value);
            }
            catch { return string.Empty; }
        }
        /// <summary>
        /// 中文日期转换为数字日期
        /// </summary>
        /// <param name="st"></param>
        /// <returns></returns>
        public static string GetChinaNum(this string st)
        {
            if (st.Contains("十月"))
            {
                st = st.Replace("十月", "10月").ToString();
            }
            if (st.Contains("十日"))
            {
                st = st.Replace("十日", "10").ToString();
            }
            if (st.Contains("二十"))
            {
                st = st.Replace("二十", "2").ToString();
            }
            if (st.Contains("三十"))
            {
                st = st.Replace("三十", "3").ToString();
            }
            if (st.Contains("十"))
            {
                st = st.Replace("十", "1").ToString();
            }
            if (st.Contains("一"))
            {
                st = st.Replace("一", "1").ToString();
            }
            if (st.Contains("二"))
            {
                st = st.Replace("二", "2").ToString();
            }
            if (st.Contains("三"))
            {
                st = st.Replace("三", "3").ToString();
            }
            if (st.Contains("四"))
            {
                st = st.Replace("四", "4").ToString();
            }
            if (st.Contains("五"))
            {
                st = st.Replace("五", "5").ToString();
            }
            if (st.Contains("六"))
            {
                st = st.Replace("六", "6").ToString();
            }
            if (st.Contains("七"))
            {
                st = st.Replace("七", "7").ToString();
            }
            if (st.Contains("八"))
            {
                st = st.Replace("八", "8").ToString();
            }
            if (st.Contains("九"))
            {
                st = st.Replace("九", "9").ToString();
            }
            if (st.Contains("〇") || st.Contains("零"))
            {
                st = st.Replace("〇", "0").ToString();
            }
            return st;
        }
        /// <summary>
        /// 清除中标公告、中标公示等
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string GetBidPrjName(this string str)
        {
            return ToolDb.GetPrjName(str);
        }
        /// <summary>
        /// 获取等级
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public static string GetLevel(this string level)
        {
            string qualNum = string.Empty;
            if (string.IsNullOrEmpty(level)) return "0";
            if (level.Contains("特级"))
            {
                qualNum = "1";
            }
            else if (level.Contains("一级") || level.Contains("甲级") || level.Contains("壹级") || level.ToUpper() == "A")
            {
                qualNum = "2";
            }
            else if (level.Contains("二级") || level.Contains("乙级") || level.Contains("贰级") || level.ToUpper() == "B")
            {
                qualNum = "3";
            }
            else if (level.Contains("三级") || level.Contains("丙级") || level.Contains("叁级") || level.ToUpper() == "C")
            {
                qualNum = "4";
            }
            else if (level.Contains("四级") || level.Contains("丁级") || level.Contains("肆级") || level == "D")
            {
                qualNum = "5";
            }
            else if (level.Contains("五级"))
            {
                qualNum = "6";
            }
            else if (level.Contains("六级"))
            {
                qualNum = "7";
            }
            else if (level.Contains("不分级"))
            {
                qualNum = "10";
            }
            else
            {
                qualNum = "0";
            }
            return qualNum;
        }
    }
}
