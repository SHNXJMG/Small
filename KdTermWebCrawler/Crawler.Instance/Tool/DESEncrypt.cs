using System;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;

namespace Crawler.Instance
{
    /// <summary>
    /// 加密解密类
    /// </summary>
    public class DESEncrypt
    {
        #region ========加密========
        /// <summary>
        /// MD5字符串加密
        /// </summary>
        /// <param name="txt"></param>
        /// <returns>加密后字符串</returns>
        public static string GenerateMD5(string txt)
        {
            using (MD5 mi = MD5.Create())
            {
                byte[] buffer = Encoding.UTF8.GetBytes(txt);
                //开始加密
                byte[] newBuffer = mi.ComputeHash(buffer);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < newBuffer.Length; i++)
                {
                    sb.Append(newBuffer[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="Text"></param>
        /// <returns></returns>
        public static string Encrypt(string Text)
        {
            return Encrypt(Text, "printuniusre");
        }
        /// <summary> 
        /// 加密数据 
        /// </summary> 
        /// <param name="Text"></param> 
        /// <param name="sKey"></param> 
        /// <returns></returns> 
        public static string Encrypt(string Text, string sKey)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] inputByteArray;
            inputByteArray = Encoding.Default.GetBytes(Text);
            des.Key = ASCIIEncoding.ASCII.GetBytes(System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(sKey, "md5").Substring(0, 8));
            des.IV = ASCIIEncoding.ASCII.GetBytes(System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(sKey, "md5").Substring(0, 8));
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            StringBuilder ret = new StringBuilder();
            foreach (byte b in ms.ToArray())
            {
                ret.AppendFormat("{0:X2}", b);
            }
            return ret.ToString();
        }

        public static string EncryptJava(string text, string key)
        {
            string s = text == null ? "" : text;
            char[] hexDigits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
                'a', 'b', 'c', 'd', 'e', 'f' };
            string strValue = "";
            try
            {
                byte[] strTemp = Convert.FromBase64String(s);
                IBufferedCipher cipher = CipherUtilities.GetCipher("MD5");
                //MessageDigest mdTemp = MessageDigest.getInstance("MD5");
                //mdTemp.update(strTemp);
                //byte[] md = mdTemp.digest();
                //int j = md.length;
                //char[] str = new char[j * 2];
                //int k = 0;
                //for (int i = 0; i < j; i++)
                //{
                //    byte byte0 = md[i];
                //    str[k++] = hexDigits[byte0 >> 4 & 0xf];
                //    str[k++] = hexDigits[byte0 & 0xf];
                //}
                //strValue = str.ToString().ToLower();//全部转换成小写
                //strValue = strValue.Replace("o", "p");
                //strValue = strValue.Replace("i", "t");
                //strValue = strValue.Replace("l", "n");
                //strValue = strValue.Replace("1", "7");
                //strValue = strValue.Replace("0", "8");
                return strValue;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        #endregion

        #region ========解密========


        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="Text"></param>
        /// <returns></returns>
        public static string Decrypt(string Text)
        {
            return Decrypt(Text, "litianping");
        }
        /// <summary> 
        /// 解密数据 
        /// </summary> 
        /// <param name="Text"></param> 
        /// <param name="sKey"></param> 
        /// <returns></returns> 
        public static string Decrypt(string Text, string sKey)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            int len;
            len = Text.Length / 2;
            byte[] inputByteArray = new byte[len];
            int x, i;
            for (x = 0; x < len; x++)
            {
                i = Convert.ToInt32(Text.Substring(x * 2, 2), 16);
                inputByteArray[x] = (byte)i;
            }
            des.Key = ASCIIEncoding.ASCII.GetBytes(System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(sKey, "md5").Substring(0, 8));
            des.IV = ASCIIEncoding.ASCII.GetBytes(System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(sKey, "md5").Substring(0, 8));
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            return Encoding.Default.GetString(ms.ToArray());
        }

        #endregion
        /// <summary>
        /// 实现SHA1加密
        /// </summary>
        /// <param name="str_sha1_in"></param>
        /// <returns></returns>
        public static string SHA1Encrypt(string str_sha1_in)
        {
            SHA1 sha1 = new SHA1CryptoServiceProvider();

            byte[] bytes_sha1_in = UTF8Encoding.Default.GetBytes(str_sha1_in);

            byte[] bytes_sha1_out = sha1.ComputeHash(bytes_sha1_in);

            string str_sha1_out = BitConverter.ToString(bytes_sha1_out);

            //str_sha1_out = str_sha1_out.Replace("-", "");

            return str_sha1_out;
        }
    }
}
