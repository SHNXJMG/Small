using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crawler.Instance
{
    public class DateUtility
    {
        private static IDictionary<string, int> ChineseToWorldCharsMap = new Dictionary<string, int>();
        private static IDictionary<int, string> WorldToChineseCharsMap = new Dictionary<int, string>();
        static DateUtility()   
        {
            ChineseToWorldCharsMap.Add("二十", 20);
            ChineseToWorldCharsMap.Add("三十", 30);
            ChineseToWorldCharsMap.Add("一十", 10);
            ChineseToWorldCharsMap.Add("贰拾",20);
            ChineseToWorldCharsMap.Add("叁拾",30);
            ChineseToWorldCharsMap.Add("壹拾", 10);
            ChineseToWorldCharsMap.Add("十", 10);
            ChineseToWorldCharsMap.Add("〇", 0);
            ChineseToWorldCharsMap.Add("一", 1);
            ChineseToWorldCharsMap.Add("二", 2);
            ChineseToWorldCharsMap.Add("三", 3);
            ChineseToWorldCharsMap.Add("四", 4);
            ChineseToWorldCharsMap.Add("五", 5);
            ChineseToWorldCharsMap.Add("六", 6);
            ChineseToWorldCharsMap.Add("七", 7);
            ChineseToWorldCharsMap.Add("八", 8);
            ChineseToWorldCharsMap.Add("九", 9);
            ChineseToWorldCharsMap.Add("零", 0);
            ChineseToWorldCharsMap.Add("壹", 1);
            ChineseToWorldCharsMap.Add("贰", 2);
            ChineseToWorldCharsMap.Add("叁", 3);
            ChineseToWorldCharsMap.Add("肆", 4);
            ChineseToWorldCharsMap.Add("伍", 5);
            ChineseToWorldCharsMap.Add("陆", 6);
            ChineseToWorldCharsMap.Add("柒", 7);
            ChineseToWorldCharsMap.Add("捌", 8);
            ChineseToWorldCharsMap.Add("玖", 9);
            ChineseToWorldCharsMap.Add("拾", 10);
            WorldToChineseCharsMap.Add(0, "〇");
            WorldToChineseCharsMap.Add(1, "一");
            WorldToChineseCharsMap.Add(2, "二");
            WorldToChineseCharsMap.Add(3, "三");
            WorldToChineseCharsMap.Add(4, "四");
            WorldToChineseCharsMap.Add(5, "五");
            WorldToChineseCharsMap.Add(6, "六");
            WorldToChineseCharsMap.Add(7, "七");
            WorldToChineseCharsMap.Add(8, "八");
            WorldToChineseCharsMap.Add(9, "九");
            WorldToChineseCharsMap.Add(10, "十");
            WorldToChineseCharsMap.Add(100, "零");
            WorldToChineseCharsMap.Add(101, "壹");
            WorldToChineseCharsMap.Add(102, "贰");
            WorldToChineseCharsMap.Add(103, "叁");
            WorldToChineseCharsMap.Add(104, "肆");
            WorldToChineseCharsMap.Add(105, "伍");
            WorldToChineseCharsMap.Add(106, "陆");
            WorldToChineseCharsMap.Add(107, "柒");
            WorldToChineseCharsMap.Add(108, "捌");
            WorldToChineseCharsMap.Add(109, "玖");
            WorldToChineseCharsMap.Add(110, "拾");
        }

        /// <summary>
        /// 数字日期转中文日期
        /// </summary>
        /// <param name="datetime"></param>
        /// <param name="big"></param>
        /// <param name="lunar"></param>
        /// <returns></returns>
        public string ToChineseDate(DateTime datetime, bool big = true, bool lunar = false)
        {
            return string.Format("{0}年{1}月{2}日", ToChineseYear(datetime.Year, big, lunar), ToChineseMonth(datetime.Month, big, lunar), ToChineseDay(datetime.Day, big, lunar));
        }

        /// <summary>
        /// 中文日期转数字日期
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public DateTime ToWoldDate(string datetime)
        {
            string[] parts = datetime.Split('年', '月', '日');
            return new DateTime(ToWoldYear(parts[0]), ToWorldMonth(parts[1]), ToWorldDay(parts[2]));
        }
        /// <summary>
        /// 中文日期转数字日期
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public string GetWoldDate(string datetime)
        {
            string[] parts = datetime.Split('年', '月', '日');
            return (new DateTime(ToWoldYear(parts[0]), ToWorldMonth(parts[1]), ToWorldDay(parts[2]))).ToString();
        }

        private string ToChineseYear(int year, bool big = true, bool lunar = false)
        {
            return this.ToChineseNumber(year, big, lunar);
        }

        private int ToWoldYear(string year)
        {
            return this.ToWorldNumber(year);
        }

        private string ToChineseMonth(int month, bool big = true, bool lunar = false)
        {
            return this.ToChineseNumber(month, big, lunar);
        }

        private int ToWorldMonth(string month)
        {
            return this.ToWorldNumberM(month);
        }

        private string ToChineseDay(int day, bool big = true, bool lunar = false)
        {
            string result = this.ToChineseNumber(day, big, lunar);
            if (lunar && day <= 10)
            {
                result = "初" + result;
            }
            return result;
        }

        private int ToWorldDay(string day)
        {
            return ToWorldNumberM(day.Substring(day.IndexOf("初") + 1));
        }

        private string ToChineseNumber(int number, bool big = true, bool lunar = false)
        {
            string result = "";
            while (number > 0)
            {
                result = this.ToChineseNumberChar(number % 10, big, lunar);
                number /= 10;
            }
            return result;
        }

        private int ToWorldNumber(string number)
        {
            int result = 0;
            for (int i = 0; i < number.Length; i++)
            {
                result = result * 10 + ToWorldNumberChar(number[i].ToString());
            }
            return result;
        }

        private int ToWorldNumberM(string number)
        {
            int result = 0;
            if (number.Length > 2)
            {
                result = result + ToWorldNumberChar(number[0].ToString() + number[1].ToString());
                result = result + ToWorldNumberChar(number[2].ToString());
            }
            else if (number.Length > 1)
            {
                result = result + ToWorldNumberChar(number[0].ToString());
                if (ToWorldNumberChar(number[1].ToString()) == 10)
                    result = result * ToWorldNumberChar(number[1].ToString());
                else
                    result = result + ToWorldNumberChar(number[1].ToString());
            }
            else
            {
                result = result + ToWorldNumberChar(number[0].ToString());
            }
            return result;
        }

        private string ToChineseNumberChar(int numberChar, bool big = true, bool lunar = false)
        {
            return WorldToChineseCharsMap[big ? 100 + numberChar : numberChar].ToString();
        }

        private int ToWorldNumberChar(string numberChar)
        {
            return ChineseToWorldCharsMap[numberChar];
        }
    }
}
