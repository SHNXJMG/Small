using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Crawler.Instance
{
    public class FileTypeDetector
    {
        static Dictionary<string, string> FileTypes = new Dictionary<string, string>();


        static FileTypeDetector()
        {
            FileTypes.Add("255044", "pdf"); 
            FileTypes.Add("FFFE3C","xsl");
            FileTypes.Add("526563", "eml");
            FileTypes.Add("89504E", "png");
            FileTypes.Add("474946", "gif");
            FileTypes.Add("49492A", "tif");
            FileTypes.Add("424D3E", "bmp");
            FileTypes.Add("414331", "dwg");
            FileTypes.Add("384250", "psd");
            FileTypes.Add("7B5C72", "rtf");
            FileTypes.Add("3C3F78", "xml");
            FileTypes.Add("68746D", "html"); 
            FileTypes.Add("CFAD12FEC5FD746F ", "dbx");
            FileTypes.Add("214244", "pst");
            FileTypes.Add("D0CF11","ppt");
            //FileTypes.Add("D0CF11E0", "xls/doc");
            FileTypes.Add("5374616E64617264204A", "mdb");
            FileTypes.Add("FF575043", "wpd");
            //FileTypes.Add("252150532D41646F6265", "eps/ps"); 
            FileTypes.Add("E38285", "pwl");
            FileTypes.Add("504B03", "zip");
            FileTypes.Add("526172", "rar");
            FileTypes.Add("574156", "wav");
            FileTypes.Add("415649", "avi");
            FileTypes.Add("2E7261", "ram");
            FileTypes.Add("2E524D", "rm");
            FileTypes.Add("000001", "mpg"); 
            FileTypes.Add("6D6F6F", "mov");
            FileTypes.Add("3026B2", "asf");
            FileTypes.Add("4D5468", "mid");
            FileTypes.Add("2F2AE5", "txt");
            FileTypes.Add("4D5A90","exe");
        }

        private static string BytesToHexString(string fileName)
        {
            StringBuilder stringBuilder = new StringBuilder();
            using (FileStream fis = new FileStream(fileName, FileMode.OpenOrCreate))
            {
                try
                {
                    byte[] b = new byte[3];
                    fis.Read(b, 0, b.Length); 
                    for (int i = 0; i < b.Length; i++)
                    {
                        int v = b[i] & 0xFF;
                        String hv = Convert.ToString(v, 16); //转换为16进制
                        if (hv.Length < 2)
                        {
                            stringBuilder.Append(0);
                        }
                        stringBuilder.Append(hv);
                    }
                   
                }
                catch
                {
                    fis.Close();
                    fis.Dispose();
                }
                if (fis != null)
                {
                    fis.Close();  
                }
            }
            return stringBuilder.ToString().ToUpper();
        }


        /// <summary>
        /// 获取文件类型（默认为空）
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string FileDetector(string fileName)
        {
            string head = BytesToHexString(fileName);
            if (FileTypes.Keys.Contains(head))
                return FileTypes[head].ToLower();
            return string.Empty;
        }
    }
}
