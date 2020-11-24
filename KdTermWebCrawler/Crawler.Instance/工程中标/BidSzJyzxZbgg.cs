using System.Text;
using Crawler;
using System.Collections;
using Winista.Text.HtmlParser;
using Crawler.Base.KdService;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Filters;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Tags;
using System.Collections.Specialized;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using Org.BouncyCastle.X509;
using Newtonsoft.Json;
using KdCore;
using System.Linq;
using Crawler.Instance.Entity;
using System.Configuration;
using KdCore.Web;
using System.Net;

namespace Crawler.Instance
{
    public class Face
    {
        /// <summary>
        /// 证件类型
        /// </summary> 
        public int? CertType { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        public string PersonName { get; set; }
    }
    public class BidSzJyzxZbgg : WebSiteCrawller
    {
        public BidSzJyzxZbgg()
        {
            this.Group = "中标信息";
            this.Title = "广东省深圳市区中标信息(2015版)";
            this.Description = "自动抓取广东省深圳市区中标信息(2015版)";
            this.PlanTime = "1:00,9:00,09:05,09:25,09:50,10:20,10:50,11:30,14:05,14:25,14:50,15:25,16:00,16:50,19:00";
            this.MaxCount = 50;
            this.SiteUrl = "https://www.szjsjy.com.cn:8001/jyw/queryZBJieGuoList.do?page=1&isHistoryGS=true&rows=";
            this.ExistsHtlCtx = true;
        }
        string jsonDatas = "";
        protected void SetTemp()
        {
            string keyEncrypt = "KdNszj.Bdimp.WebApi.AzdgKEY";
            KdAzdgHelper azdg = new KdAzdgHelper(keyEncrypt);
            string cookies = string.Empty;
            IWebHttpClient httpClient = new WebHttpClient();
            Uri url = new Uri("http://localhost:7434/Home/Login");
            string json = ToolWeb.GetHtmlByUrl(url.ToString(), Encoding.UTF8, ref cookies);
            Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            string mrsa = dict["mrsa"];
            string ersa = dict["ersa"];
            string auths_Token = dict["auths_Token"];
            string userName = azdg.Encrypt("adminer");

            string userPwd2 = DESEncrypt.GenerateMD5("1");
            string userPwd = azdg.Encrypt(userPwd2);
            NameValueCollection nvc = ToolWeb.GetNameValueCollection(new string[] {
            "userName",
            "userPwd",
            "mrsa",
            "ersa",
            "auths_Token"},
            new string[] {
            userName,
            userPwd,
            mrsa,
            ersa,
            auths_Token
            });
            string result = ToolWeb.GetHtmlByUrl(url.ToString(), nvc, Encoding.UTF8, ref cookies);
        }

        protected void HtppZsjApi()
        {
            string client_id = "9e1f36ba32f24cf0938af7d39e5a3086";
            string client_secret = "f7d39e5a3086";
            string grant_type = "client_credentials";

            string post = string.Format("client_id={0}&client_secret={1}&grant_type={2}", client_id, client_secret, grant_type);
            string url = "https://10.200.66.211/authen";

            IWebHttpClient httpClient = new WebHttpClient();
            string json = httpClient.PostSync(new Uri(url), post);
        }

        protected void HttpApi()
        {
            //HtppZsjApi();
            //return;
            string appKey = "AGIGZwdjA2BTZAY3V2dXYAM0W0gBNQM1XmEAMwY9BzoBZFJGB2cAZQJPDD5RawQ2DDlUQlBuURQLMF1rBzsMNQARBmAHYwNmU2sGN1dgVxMDRFs/AUoDMV5gADcGPwc/";
            string appSecret = "ABEGZAdpAxZTHgZEV2BXZwNEWzsBPQNDXmUANQZMBzIBaFJHBxUAYgJJDEpRYgQ7DEpUP1BkURMLQV1tBzUMQw==";

            //appKey = "AGIGZwdjA2BTHQY3V2RXYQM+Wz8BNAMyXmEAMwY9BzoBZ1JGBxUAagI+DD1RZQQzDE1UNlBpURQLMF1nBzwMMABpBmIHaQNkU20GMVdlVxcDNls7ATgDR15mADMGTgc4";
            //appSecret = "AGIGZwdjA2BTaAY0V2xXYQNFWzABOwMwXmEAMwY9BzsBF1I6B2IAYgI6DD1RZQQ7DD1UNVBlUW4LMF0aB00MMAARBhIHZQMUU24GMldmV2ADMFtLAU8DNF5gADYGSQc4";

            appKey = "AGIGZwdjA2BTHgY3V2NXYQNAWzABPgMxXmEAMwY9BzoBYFI1BxcAawI5DElRFwQ0DD5UQ1AfURELMF1nBzkMMQARBhIHFwNiUxoGNFcVVxcDQls4AUoDQV5iADMGNQdK";
            appSecret = "AGIGZwdjA2BTaAY0V2xXYQNFWzABOwMwXmEAMwY9BzkBYVI6B2AAYAJPDDxRZARHDExUMVBvUWMLMF0aBzgMQwASBhEHZgMRU24GMVcQVxMDQltLAUkDPF5nADYGTwdI";

            //string loginUrl = string.Format("https://10.200.67.197/ApiServe/GetToken");//?appKey={0}&appSecret={1}", appKey, appSecret);
            string loginUrl = string.Format("http://localhost:7434/ApiServe/GetToken");

            //string dataUrl = "http://localhost:7434/AffordDevice/GetRooms?pageSize=5&pageIndex=1";
            string dataUrl = "http://localhost:7434/AffordDevice/GetRooms";

            NameValueCollection nvc = ToolWeb.GetNameValueCollection(new string[] { "appKey", "appSecret" }, new string[] { appKey, appSecret });

            IWebHttpClient httpClient = new WebHttpClient();
            string postData = nvc.ToQueryString();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            string html = httpClient.PostSync(new Uri(loginUrl), postData);

            string token = ToolWeb.GetHtmlByUrl(loginUrl, nvc);
            Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(token);
          
            
            //string json = httpClient.GetSync(new Uri(dataUrl), dict["token"]);// ToolWeb.GetHtmlByUrl(dataUrl);//

            //string dataUrl1 = "https://nszj2.szns.gov.cn/AffordDevice/GetPersons?pageSize=5&pageIndex=1";
            //string personData = httpClient.GetSync(new Uri(dataUrl1), dict["token"]);

            //string dataUrl2 = "http://localhost:7434/AffordDevice/SetDoorAnomalyOpen";
            string dataUrl2 = "http://localhost:7434/AffordDevice/SetDeviceAltitude";
            //List<Face> faces = new List<Face>();

            //Face face = new Face();
            //face.CertType = 10;
            //face.PersonName = "张三";
            //faces.Add(face);

            //Face face1 = new Face();
            //face1.CertType = 10;
            //face1.PersonName = "李四";
            //faces.Add(face1);

            //string postDatas = faces.ToDataJson();

            //Dictionary<string, object> dicData = new Dictionary<string, object>();
            //dicData.Add("data", postDatas);

            //string temp = "bsf\"sd\"hf";
            //string jsonStr = "data:" + "zheshi";
            string jsons = "[{\"IsFace\":true,\"PersonName\":\"郭滢\",\"RoomName\":\"3209\",\"EndTime\":\"2020-01-15 00:00:00.0\",\"LocateFloor\":\"32\",\"BuildingName\":\"1栋\",\"Sex\":\"女\",\"StartTime\":\"2019-01-16 00:00:00.0\",\"PhotoPath3\":\"\",\"CertType\":10,\"Mobile\":\"13148859655\",\"OpenPowers\":100,\"PhotoPath1\":\"https://timgsa.baidu.com/timg?image&quality=80&size=b9999_10000&sec=1599197027901&di=1e063142cad59d9b24f49bdb11443fe5&imgtype=0&src=http%3A%2F%2Fimgsrc.baidu.com%2Fforum%2Fw%3D580%2Fsign%3D4534064588d4b31cf03c94b3b7d7276f%2F62ada5ec08fa513d294e71d83e6d55fbb3fbd9f6.jpg\",\"PhotoPath2\":\"\",\"RegisterTime\":\"2020-09-08 09:05:45.0\",\"DeviceType\":10,\"Desc\":\"\",\"RelationTypeOther\":\"申请人\",\"BiotopeName\":\"测试小区\",\"RegisterTypeOther\":\"租户\",\"RelationType\":1,\"IdCard\":\"120101198906260569\",\"RegisterType\":10,\"IsSffectivity\":true}]";

            //jsons = "[{\"AnomalyOpens\":[{\"OpenType\":800,\"OpenCount\":2}],\"DeviceType\":20,\"Desc\":\"长期通过非刷脸开门\",\"BiotopeName\":\"测试小区\",\"RoomName\":\"3209\",\"EndTime\":\"2020-09-10\",\"AnomalyTime\":\"2020-09-10\",\"LocateFloor\":\"32\",\"OpenCount\":2,\"BuildingName\":\"1栋\",\"AnomalyNote\":\"长期通过非刷脸开门\",\"StartTime\":\"2020-08-11\"}]";
            jsons = "[ {\"PersonName\": \"张三\",\"CertType\": 1,\"IdCard\": \" 42118119840********\",\"Mobile\": \"13255632245\",\"Sex\": \"男\",\"RelationType\": 1,\"BiotopeName\": \"蛇口人才公寓\",\"BuildingName\": \"B栋\",\"RoomName\": \"B1301\",\"LocateFloor\": \"16\",\"AltitudeDate\": \"2020-06-02\",\"ParabolicTime\": \"2023-06-01 14:32:14\",\"FloorTime\": \"2023-06-01 14:33:32\",\"FloorName\": \"椅子\",\"DeviceCodes\": \"SX001,SX002\", \"IsCasualties\":true,\"PersonNum\":4,\"FireNote\":\"2人重伤，2人轻伤，目前均以送往医院，无生命危险\",\"ImgPath1\": \"https://nszj2.szns.gov.cn/img/zs1.jpg\",\"ImgPath2\": \"https://nszj2.szns.gov.cn/img/zs2.jpg\",\"ImgPath3\": \"https://nszj2.szns.gov.cn/img/zs3.jpg\",\"ImgPath4\": \"https://nszj2.szns.gov.cn/img/zs4.jpg\",\"VideoPath1\": \"https://nszj2.szns.gov.cn/video/pw1.mp4\",\"VideoPath2\": \"https://nszj2.szns.gov.cn/video/pw2.mp4\",\"VideoPath3\": \"https://nszj2.szns.gov.cn/video/pw3.mp4\",\"VideoPath4\": \"https://nszj2.szns.gov.cn/video/pw4.mp4\",\"Explain\": \"该房号于2023-06-01 14:32:14从16楼抛下一把椅子，椅子落地坠毁，造成2人重伤，2人轻伤，目前均以送往医院，无生命危险\"}]";

            string result = httpClient.PostSync(new Uri(dataUrl2), jsons, dict["token"]);

            string cookie = string.Empty;
            result = ToolHtml.GetHtmlByUrlPost(dataUrl2, jsons, dict["token"], Encoding.UTF8, ref cookie);
        }
        protected override IList ExecuteCrawl(bool crawlAll)
        {
            this.HttpApi();
            //SetTemp();
            DateTime time = ToolHtml.GetDateTimeByLong(1543593600000);

            string html = string.Empty;
            //string url = "http://web.zjj.sz.gov.cn/HouseOutService/queryFwRentms/getGrQyInfo.json";
            //string url = "http://web.zjj.sz.gov.cn/HouseOutService/queryFwRentms/getApplyers.json";
            string url = "http://web.zjj.sz.gov.cn/HouseOutService/queryFwRentms/getUnitQyInfos.json";
            //string url = "http://web.zjj.sz.gov.cn/HouseOutService/queryFwRentms/getUnitFlatInfos.json";
            //string url = "http://web.zjj.sz.gov.cn/zfxx_jscjn/external/project/info/get?pageIndex=1&pageSize=100";
            string cookies = string.Empty;
            string publicKey = "bnNfZGF0YTpLSUlmMndLVWJ1RmVyVEhRZWh5WTFyNzNlVEM4VmVTb3p2eFBDanN2VVJRWnExR20xdVduVk1FQnlyK0ZrMEdhcVRGRzFVUUw1dTBDNEpxRWNRSVRra3NOYWgxcFVldnJCbnpTcDJaUnU3THpyNTZsUmhzd09NdHNiZHYxVCtJbGdHdzBEcUZXczJIVVYzZkw0NWFnbldqemt3MHJpVlJ2cEs5MFFiOHBMb1E9";

            string key = string.Format("{0}{1}{2}{3}", "e02d02ec17a14446a861bbad068c40ef", "440305", "", "1990-01-01");
            string keys = DESEncrypt.GenerateMD5(key);
            //"983f9b3b897c77f27c9bd27837d82f5f"
            //"983f9b3b897c77f27c9bd21837d02f5f"
            keys = keys.Replace("o", "p");
            keys = keys.Replace("i", "t");
            keys = keys.Replace("l", "n");
            keys = keys.Replace("1", "7");
            keys = keys.Replace("0", "8");
            Dictionary<string, string> dic1 = new Dictionary<string, string>();
            dic1.Add("key", keys);
            dic1.Add("belongto", "440305");
            dic1.Add("quart", "");
            dic1.Add("page", "60");
            dic1.Add("timestamp", "1990-01-01");
            string jsonStr = JsonConvert.SerializeObject(dic1);

            string jsonData = string.Empty;

            try
            {
                using (IWebHttpClient httpClient = new WebHttpClient())
                {
                    jsonData = httpClient.PostSync(new Uri(url), jsonStr, publicKey);
                }
            }
            catch (Exception ex)
            {

            }

            Dictionary<string, object> dics = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonData);

            object oobj = dics["date"];

            Dictionary<string, object> contents = JsonConvert.DeserializeObject<Dictionary<string, object>>(oobj.ToString());

            string str1 = contents["content"].ToString();

            List<Dictionary<string, object>> jsonLists = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(str1);

            foreach (Dictionary<string, object> keys1 in jsonLists)
            {
                WebPactCorpData model = JsonConvert.DeserializeObject<WebPactCorpData>(JsonConvert.SerializeObject(keys1));
                string str2 = keys1.ToString();
            }

            string jsonsss = oobj.ToString();

            KeyValuePair<string, object> keyValues = (KeyValuePair<string, object>)oobj;

            Dictionary<string, object> content = (Dictionary<string, object>)dics["date"];

            object[] objs = (object[])content["content"];

            foreach (object obj in objs)
            {
                WebPactCorpData corp = obj as WebPactCorpData;
            }

            WebPactCorp entity = JsonConvert.DeserializeObject<WebPactCorp>(jsonData);

            //using (IWebHttpClient httpClient = new WebHttpClient())
            //{
            //    jsonData = httpClient.GetSync(new Uri(url), publicKey);
            //}

            //html = ToolWeb.GetHtmlByUrl(url, nvc, Encoding.UTF8, true, publicKey, ref cookies);

            IList list = new List<BidInfo>();
            int sqlCount = 0;

            try
            {
                html = this.ToolWebSite.GetHtmlByUrl(this.SiteUrl + (this.MaxCount + 20));
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
                    string prjName = string.Empty, buildUnit = string.Empty, bidUnit = string.Empty, bidMoney = string.Empty, code = string.Empty, bidDate = string.Empty, beginDate = string.Empty, endDate = string.Empty, bidType = string.Empty, specType = string.Empty, InfoUrl = string.Empty, msgType = string.Empty, bidCtx = string.Empty, prjAddress = string.Empty, remark = string.Empty, prjMgr = string.Empty, HtmlTxt = string.Empty;

                    Dictionary<string, object> dic = (Dictionary<string, object>)arrValue;
                    code = Convert.ToString(dic["bdBH"]);
                    prjName = Convert.ToString(dic["bdName"]);
                    bidType = Convert.ToString(dic["gcLeiXing2"]);
                    beginDate = Convert.ToString(dic["fabuTime2"]);
                    try
                    {
                        bidMoney = Convert.ToString(dic["zhongBiaoJE"]).GetMoney();
                    }
                    catch
                    {

                    }
                    string addUrl = Convert.ToString(dic["detailUrl"]);
                    //https://www.szjsjy.com.cn:8001/jyw/queryOldDataDetail.do?type=4&id=158df5f1-73a1-440c-a59b-e4ca1464b4e9
                    InfoUrl = "https://www.szjsjy.com.cn:8001/jyw/queryOldDataDetail.do?type=4&id=" + dic["dbZhongBiaoJieGuoGuid"];

                    try
                    {
                        HtmlTxt = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString().GetReplace("\\t,\\r,\\n,\"");
                    }
                    catch { }

                    List<Dictionary<string, string>> dicFile = new List<Dictionary<string, string>>();
                    if (string.IsNullOrEmpty(HtmlTxt))
                    {
                        string strHtml = string.Empty;
                        string newUrl = "https://www.szjsjy.com.cn:8001/jyw/queryZbgs.do?guid=" + dic["dbZhongBiaoJieGuoGuid"] + "&ggGuid=bdGuid=";
                        InfoUrl = Convert.ToString(dic["detailUrl"]);
                        try
                        {
                            HtmlTxt = this.ToolWebSite.GetHtmlByUrl(InfoUrl).GetJsString().GetReplace("\\t,\\r,\\n,\"");
                            strHtml = this.ToolWebSite.GetHtmlByUrl(newUrl).GetJsString();
                        }
                        catch { }

                        if (!string.IsNullOrEmpty(strHtml))
                        {
                            string gcBH = string.Empty, gcName = string.Empty, xmBH = string.Empty, xmName = string.Empty, zbgsStartTime = string.Empty, zbgsEndTime = string.Empty, zbRName = string.Empty, zbdlJG = string.Empty, zbFangShi = string.Empty, bdName = string.Empty, tbrName = string.Empty, zhongBiaoJE = string.Empty, zhongBiaoGQ = string.Empty, xiangMuJiLi = string.Empty, ziGeDengJi = string.Empty, ziGeZhengShu = string.Empty, isZanDingJinE = string.Empty, gcLeiXing = string.Empty, isPLZB = string.Empty, ztbFileGroupGuid = string.Empty;
                            try
                            {
                                Dictionary<string, string> zbfs = new Dictionary<string, string>();
                                zbfs.Add("2", "邀请招标");
                                zbfs.Add("1", "公开招标");
                                zbfs.Add("YuXuanZhaoBiaoZGC", "预选招标子工程");
                                zbfs.Add("GongKaiZhaoBiao", "公开招标");
                                zbfs.Add("5", "预选招标子工程");
                                zbfs.Add("4", "单一来源");
                                zbfs.Add("DanYiLaiYuan", "单一来源");
                                zbfs.Add("YaoQingZhaoBiao", "邀请招标");
                                JavaScriptSerializer newSerializer = new JavaScriptSerializer();
                                Dictionary<string, object> newTypeJson = (Dictionary<string, object>)newSerializer.DeserializeObject(strHtml);
                                Dictionary<string, object> bd = newTypeJson["bd"] as Dictionary<string, object>;
                                Dictionary<string, object> gc = bd["gc"] as Dictionary<string, object>;
                                ztbFileGroupGuid = Convert.ToString(newTypeJson["ztbFileGroupGuid"]);
                                gcBH = Convert.ToString(gc["gcBH"]);
                                gcName = Convert.ToString(gc["gcName"]);
                                Dictionary<string, object> xm = bd["xm"] as Dictionary<string, object>;

                                if (xm != null)
                                {
                                    xmBH = Convert.ToString(xm["xm_BH"]);
                                    xmName = Convert.ToString(xm["xm_Name"]);
                                }
                                object startTime = newTypeJson["zbgsStartTime"];
                                if (startTime != null)
                                    zbgsStartTime = ToolHtml.GetDateTimeByLong(Convert.ToInt64(startTime)).ToString("yyyy-MM-dd HH:mm");

                                object endTime = newTypeJson["zbgsEndTime"];
                                if (endTime != null)
                                    endDate = zbgsEndTime = ToolHtml.GetDateTimeByLong(Convert.ToInt64(endTime)).ToString("yyyy-MM-dd HH:mm");

                                buildUnit = zbRName = Convert.ToString(gc["zbRName"]);

                                zbdlJG = Convert.ToString(newTypeJson["zbdlJG"]);

                                zbFangShi = Convert.ToString(gc["zbFangShi"]);
                                if (!string.IsNullOrEmpty(zbFangShi))
                                    zbFangShi = zbfs[zbFangShi];
                                bdName = Convert.ToString(bd["bdName"]);
                                bidUnit = tbrName = Convert.ToString(newTypeJson["tbrName"]);

                                zhongBiaoJE = Convert.ToString(newTypeJson["zhongBiaoJE"]);
                                if (!string.IsNullOrEmpty(zhongBiaoJE))
                                {
                                    try
                                    {
                                        bidMoney = zhongBiaoJE = (decimal.Parse(zhongBiaoJE) / 1000000).ToString();
                                    }
                                    catch { }
                                }
                                else
                                {
                                    try
                                    {
                                        zhongBiaoJE = Convert.ToString(newTypeJson["tongYongZhongBiaoJia"]);
                                        bidMoney = (zhongBiaoJE + "\r\n").GetMoneyRegex(new string[] { "人民币" });
                                    }
                                    catch { }
                                }
                                zhongBiaoGQ = Convert.ToString(newTypeJson["zhongBiaoGQ"]);
                                prjMgr = xiangMuJiLi = Convert.ToString(newTypeJson["xiangMuJiLi"]);

                                ziGeDengJi = Convert.ToString(newTypeJson["ziGeDengJi"]);
                                ziGeZhengShu = Convert.ToString(newTypeJson["ziGeZhengShu"]);
                                isZanDingJinE = Convert.ToString(newTypeJson["isZanDingJinE"]);
                                gcLeiXing = Convert.ToString(bd["gcLeiXing"]);
                                isPLZB = Convert.ToString(gc["isPLZB"]);

                            }
                            catch (Exception ex)
                            {
                                Logger.Error(ex);
                            }

                            Parser parser = new Parser(new Lexer(HtmlTxt));
                            NodeList dtlNode = parser.ExtractAllNodesThatMatch(new AndFilter(new TagNameFilter("div"), new HasAttributeFilter("class", "detail_contect")));
                            if (dtlNode != null && dtlNode.Count > 0)
                            {
                                HtmlTxt = dtlNode.AsHtml();
                                HtmlTxt = HtmlTxt.GetReplace("<span id=gcBH></span>", "<span id=gcBH>" + gcBH + "</span>");
                                HtmlTxt = HtmlTxt.GetReplace("<span id=gcName></span>", "<span id=gcName>" + gcName + "</span>");
                                HtmlTxt = HtmlTxt.GetReplace("<span id=xmBH></span>", "<span id=xmBH>" + xmBH + "</span>");
                                HtmlTxt = HtmlTxt.GetReplace("<span id=xmName></span>", "<span id=xmName>" + xmName + "</span>");
                                HtmlTxt = HtmlTxt.GetReplace("<span id=zbgsStartTime></span>", "<span id=zbgsStartTime>" + zbgsStartTime + "</span>");
                                HtmlTxt = HtmlTxt.GetReplace("<span id=zbgsEndTime></span>", "<span id=zbgsEndTime>" + zbgsEndTime + "</span>");
                                HtmlTxt = HtmlTxt.GetReplace("<span id=zbRName></span>", "<span id=zbRName>" + zbRName + "</span>");
                                HtmlTxt = HtmlTxt.GetReplace("<span id=zbdlJG></span>", "<span id=zbdlJG>" + zbdlJG + "</span>");
                                HtmlTxt = HtmlTxt.GetReplace("<span id=zbFangShi></span>", "<span id=zbFangShi>" + zbFangShi + "</span>");
                                HtmlTxt = HtmlTxt.GetReplace("<span id=bdName></span>", "<span id=bdName>" + bdName + "</span>");
                                HtmlTxt = HtmlTxt.GetReplace("<span id=tbrName></span>", "<span id=tbrName>" + tbrName + "</span>");
                                HtmlTxt = HtmlTxt.GetReplace("<span id=zhongBiaoJE></span>", "<span id=zhongBiaoJE>" + zhongBiaoJE + "</span>");
                                HtmlTxt = HtmlTxt.GetReplace("<span id=zhongBiaoGQ></span>", "<span id=zhongBiaoGQ>" + zhongBiaoGQ + "</span>");
                                HtmlTxt = HtmlTxt.GetReplace("<span id=xiangMuJiLi></span>", "<span id=xiangMuJiLi>" + xiangMuJiLi + "</span>");
                                HtmlTxt = HtmlTxt.GetReplace("<span id=ziGeDengJi></span>", "<span id=ziGeDengJi>" + ziGeDengJi + "</span>");
                                HtmlTxt = HtmlTxt.GetReplace("<span id=ziGeZhengShu></span>", "<span id=ziGeZhengShu>" + ziGeZhengShu + "</span>");
                                HtmlTxt = HtmlTxt.GetReplace("<span id=isZanDingJinE></span>", "<span id=isZanDingJinE>" + isZanDingJinE.ToLower() == "true" ? "是" : "否" + "</span>");
                            }
                            string fileUrl = "https://www.szjsjy.com.cn:8001/jyw/filegroup/queryByGroupGuidZS.do?groupGuid=" + ztbFileGroupGuid;
                            string fileHtml = string.Empty;
                            try
                            {
                                fileHtml = this.ToolWebSite.GetHtmlByUrl(fileUrl);
                                JavaScriptSerializer fileSerializer = new JavaScriptSerializer();
                                Dictionary<string, object> fileTypeJson = (Dictionary<string, object>)fileSerializer.DeserializeObject(fileHtml);
                                foreach (KeyValuePair<string, object> fileObj in fileTypeJson)
                                {
                                    object[] fileArray = (object[])fileObj.Value;
                                    foreach (object fileValue in fileArray)
                                    {
                                        Dictionary<string, object> tempDic = (Dictionary<string, object>)fileValue;
                                        Dictionary<string, string> file = new Dictionary<string, string>();
                                        file.Add("Name", Convert.ToString(tempDic["attachName"]));
                                        file.Add("Url", Convert.ToString("https://www.szjsjy.com.cn:8001/file/downloadFile?fileId=" + tempDic["attachGuid"]));
                                        dicFile.Add(file);
                                    }
                                }
                            }
                            catch { }
                        }

                    }
                    bidCtx = HtmlTxt.GetReplace("<br />,<br/>,</ br>,</br>", "\r\n").ToCtxString() + "\r\n";
                    if (string.IsNullOrEmpty(buildUnit) && string.IsNullOrEmpty(bidUnit))
                    {
                        bidUnit = bidCtx.GetBidRegex();
                        if (string.IsNullOrEmpty(bidUnit))
                            bidUnit = bidCtx.Replace(" ", "").GetBidRegex();
                        if (string.IsNullOrEmpty(bidUnit))
                            bidUnit = bidCtx.GetRegex("中 标 人");
                        string money = bidCtx.GetMoneyRegex();
                        if (!string.IsNullOrEmpty(money))
                            bidMoney = money;
                        if (string.IsNullOrEmpty(bidMoney) || bidMoney == "0")
                            bidMoney = bidCtx.GetMoneyRegex();
                        if (string.IsNullOrEmpty(bidMoney) || bidMoney == "0")
                            bidMoney = bidCtx.Replace(" ", "").GetMoneyRegex();
                        if (string.IsNullOrEmpty(bidMoney) || bidMoney == "0")
                            bidMoney = bidCtx.GetRegex("中 标 价");

                        prjMgr = bidCtx.GetMgrRegex();
                        if (string.IsNullOrEmpty(prjMgr))
                            prjMgr = bidCtx.Replace(" ", "").GetMgrRegex();
                        if (string.IsNullOrEmpty(prjMgr))
                            prjMgr = bidCtx.GetRegex("项 目 总 监");

                        buildUnit = bidCtx.GetBuildRegex();
                        if (string.IsNullOrEmpty(buildUnit))
                            buildUnit = bidCtx.Replace(" ", "").GetBuildRegex();
                        if (string.IsNullOrEmpty(buildUnit))
                            buildUnit = bidCtx.GetRegex("建 设 单 位");
                        prjAddress = bidCtx.GetAddressRegex();
                        if (string.IsNullOrEmpty(prjAddress))
                            prjAddress = bidCtx.Replace(" ", "").GetAddressRegex();
                        if (string.IsNullOrEmpty(prjAddress))
                            prjAddress = bidCtx.GetRegex("工 程 地 址");
                    }
                    specType = "建设工程";
                    msgType = "深圳市建设工程交易中心";
                    if (Encoding.Default.GetByteCount(prjMgr) > 50)
                        prjMgr = "";
                    if (Encoding.Default.GetByteCount(bidUnit) > 150)
                    {
                        string[] bidUnits = bidUnit.Split(';');
                        bidUnit = bidUnits[0];
                    }
                    if (Encoding.Default.GetByteCount(bidUnit) > 150)
                        bidUnit = "";
                    if (prjMgr.Contains("----"))
                        prjMgr = "";
                    BidInfo info = ToolDb.GenBidInfo("广东省", "深圳市工程", "", string.Empty, code, prjName, buildUnit, beginDate, bidUnit, beginDate, endDate, bidCtx, string.Empty, msgType, bidType, specType, string.Empty, bidMoney, addUrl, prjMgr, HtmlTxt);
                    sqlCount++;
                    if (ToolDb.SaveEntity(info, this.ExistCompareFields, this.ExistsUpdate))
                    {
                        if (dicFile.Count > 0)
                        {
                            foreach (Dictionary<string, string> file in dicFile)
                            {
                                try
                                {
                                    BaseAttach item = ToolHtml.GetBaseAttach(file["Url"], file["Name"], info.Id, "SiteManage\\Files\\InviteAttach\\");
                                    if (item != null)
                                        ToolDb.SaveEntity(item, "SourceID,AttachServerPath");
                                }
                                catch { }
                            }
                        }
                    }
                    if (!crawlAll && sqlCount >= this.MaxCount) return null;
                }
            }
            return list;
        }
    }
}
