using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crawler.Instance
{
    [Serializable]
    public class PrjResult
    {
        public string Bh
        {
            set;
            get;
        }
        public int Xh
        {
            set;
            get;
        }
        public string Name
        { set; get; }
        public string Mc
        { set; get; }
        public string Date
        { set; get; }
        public string IsBid
        { set; get; }

        public int lunCiXuHao
        { set; get; }
    }


    public class LPrjResult
    {
        public static List<PrjResult> GetPrjZlResult(object[] obj)
        {
            List<PrjResult> list = new List<PrjResult>();
            for (int i = 0; i < obj.Length; i++)
            {
                Dictionary<string, object> jsonDtl = (Dictionary<string, object>)obj[i];
                PrjResult result = new PrjResult();
                result.Bh = Convert.ToString(jsonDtl["tbrTouPiaoBH"]);
                try
                {
                    result.Xh = Convert.ToInt32(jsonDtl["tbrSequence"]);
                }
                catch { }
                try
                {
                    result.lunCiXuHao = Convert.ToInt32(jsonDtl["lunCiXuHao"]);
                }
                catch { }
                result.Name = Convert.ToString(jsonDtl["tbrName"]);
                result.Mc = Convert.ToString(jsonDtl["dePiaoShu"]);
                list.Add(result);
            }
            return list.OrderBy(x => x.lunCiXuHao).ToList();
        }
        public static List<PrjResult> GetPrjResult(object[] obj)
        {
            List<PrjResult> list = new List<PrjResult>();
            for (int i = 0; i < obj.Length; i++)
            {
                Dictionary<string, object> jsonDtl = (Dictionary<string, object>)obj[i];
                PrjResult result = new PrjResult();
                result.Bh = Convert.ToString(jsonDtl["tbrTouPiaoBH"]);
                try
                {
                    result.Xh = Convert.ToInt32(jsonDtl["tbrSequence"]);
                }
                catch { }
                result.Name = Convert.ToString(jsonDtl["tbrName"]);
                result.Mc = Convert.ToString(jsonDtl["dePiaoShu"]);
                list.Add(result);
            }
            return list.OrderBy(x => x.Bh).ToList();
        }
        public static List<PrjResult> GetPrjResultBid(object[] obj)
        {
            List<PrjResult> list = new List<PrjResult>();
            for (int i = 0; i < obj.Length; i++)
            {
                Dictionary<string, object> jsonDtl = (Dictionary<string, object>)obj[i];
                PrjResult result = new PrjResult();
                try
                {
                    result.Xh = Convert.ToInt32(jsonDtl["tbrSequence"]);
                }
                catch { return list; }
                result.Name = Convert.ToString(jsonDtl["tbrName"]);
                try
                {
                    result.Date = Convert.ToString(jsonDtl["tbTime"]);
                }
                catch { }
                if (!string.IsNullOrEmpty(result.Date))
                {
                    result.Date = ToolHtml.GetDateTimeByLong(long.Parse(result.Date)).ToString();
                }
                result.IsBid = Convert.ToString(jsonDtl["zhongBiaoZhuangTai"]);
                if (result.IsBid == "0" || string.IsNullOrEmpty(result.IsBid))
                    result.IsBid = "<input type='checkbox' disabled=true/>";
                else
                    result.IsBid = "<input type='checkbox'  checked=true disabled=true/>";
                list.Add(result);
            }
            return list.OrderBy(x => x.Xh).ToList();
        }
    }
}
