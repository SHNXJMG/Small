using System;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.X509;

namespace Crawler.Instance
{
    /// <summary>
    /// RAS 加解密辅助类
    /// </summary>
    public static class RasHelper
    {
        /// <summary>
        /// 将 8 位无符号整数数组的值转换为其用 Base64 数字编码的等效字符串表示形式。
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private static string ToBase64(byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }
        /// <summary>
        /// 将指定的字符串（它将二进制数据编码为 Base64 数字）转换为等效的 8 位无符号整数数组。
        /// </summary>
        /// <param name="base64String"></param>
        /// <returns></returns>
        public static byte[] FromBase64(string base64String)
        {
            X509DefaultEntryConverter asd = new X509DefaultEntryConverter();
             
            return Convert.FromBase64String(base64String);
        }

        #region 密钥转换
        /// <summary>    
        /// 从 Java 生成的 RSA 密钥转换为 .Net RSA 算法参数   
        /// </summary>    
        /// <param name="javaRsaKey">Java 生成的密钥，FromBase64自动判断私钥公钥</param>    
        /// <returns></returns>    
        public static RSAParameters FromJavaKey(string javaRsaKey, bool isPrivate)
        {
            byte[] bytes = FromBase64(javaRsaKey);
            AsymmetricKeyParameter param;
            if (isPrivate)
            {
                param = PrivateKeyFactory.CreateKey(bytes);
                return DotNetUtilities.ToRSAParameters(param as RsaPrivateCrtKeyParameters);
            }

            param = PublicKeyFactory.CreateKey(bytes);
            return DotNetUtilities.ToRSAParameters(param as RsaKeyParameters);

        }

        /// <summary>    
        /// 从 Java 生成的 RSA 密钥创建 RSA 加密算法实例
        /// </summary>    
        /// <param name="javaRsaKey">Java 生成的密钥，私钥公钥自动匹配</param>    
        /// <returns></returns>    
        public static RSA CreateRSA(string javaRsaKey, bool isPrivate)
        {
            RSAParameters parms = FromJavaKey(javaRsaKey, isPrivate);
            RSACng rsa = new RSACng();
            rsa.ImportParameters(parms);
            return rsa;
        }

        /// <summary>    
        /// 从 Java 生成的 RSA 私钥转换为 .Net RSA 算法的 XML 格式私钥   
        /// </summary>    
        /// <param name="privateKey">Java 生成的 RSA 私钥</param>    
        /// <returns></returns>   
        public static string FromJavaPrivateKeyToXmlString(string privateKey)
        {
            RsaPrivateCrtKeyParameters param = (RsaPrivateCrtKeyParameters)PrivateKeyFactory.CreateKey(FromBase64(privateKey));
            return string.Format(
                "<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent><P>{2}</P><Q>{3}</Q><DP>{4}</DP><DQ>{5}</DQ><InverseQ>{6}</InverseQ><D>{7}</D></RSAKeyValue>",
                ToBase64(param.Modulus.ToByteArrayUnsigned()),
                ToBase64(param.PublicExponent.ToByteArrayUnsigned()),
                ToBase64(param.P.ToByteArrayUnsigned()),
                ToBase64(param.Q.ToByteArrayUnsigned()),
                ToBase64(param.DP.ToByteArrayUnsigned()),
                ToBase64(param.DQ.ToByteArrayUnsigned()),
                ToBase64(param.QInv.ToByteArrayUnsigned()),
                ToBase64(param.Exponent.ToByteArrayUnsigned()));
        }
        /// <summary>
        /// RSA公钥格式转换，java->.net
        /// </summary>
        /// <param name="keyInfoData">java生成的公钥</param>
        /// <returns></returns>
        public static string RSAPublicKeyJava2DotNet(byte[] keyInfoData)
        {
            RsaKeyParameters publicKeyParam = (RsaKeyParameters)PublicKeyFactory.CreateKey(keyInfoData);
            return string.Format("<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent></RSAKeyValue>",
                Convert.ToBase64String(publicKeyParam.Modulus.ToByteArrayUnsigned()),
                Convert.ToBase64String(publicKeyParam.Exponent.ToByteArrayUnsigned()));
        }
        /// <summary>    
        /// 从 Java 生成的 RSA 公钥转换为 .Net RSA 算法的 XML 格式  
        /// </summary>    
        /// <param name="publicKey">Java 生成的 RSA 公钥</param>    
        /// <returns></returns>    
        public static string FromJavaPublicKeyToXmlString(string publicKey)
        {
            byte[] bytes = FromBase64(publicKey);
            RsaKeyParameters param = (RsaKeyParameters)PrivateKeyFactory.CreateKey(bytes);
            return string.Format("<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent></RSAKeyValue>",
                ToBase64(param.Modulus.ToByteArrayUnsigned()),
                ToBase64(param.Exponent.ToByteArrayUnsigned()));
        }

        /// <summary>    
        /// 从 .Net RSA 算法的 XML 格式的私钥转换为 Java 格式的 RSA 私钥
        /// </summary>    
        /// <param name="xmlPrivateKey">.Net RSA 算法的 XML 格式的私钥</param>    
        /// <returns></returns>   
        public static string ToJavaPrivateKey(string xmlPrivateKey)
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.FromXmlString(xmlPrivateKey);
                return rsa.ExportParameters(true).ToJavaPrivateKey();
            }
        }
        /// <summary>    
        /// 从 .Net RSA 算法的 XML 格式的公钥转换为 Java 格式的 RSA 公钥 
        /// </summary>    
        /// <param name="xmlPublicKey">.Net RSA 算法的 XML 格式的公钥</param>    
        /// <returns></returns>   
        public static string ToJavaPublicKey(string xmlPublicKey)
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.FromXmlString(xmlPublicKey);
                return rsa.ExportParameters(false).ToJavaPublicKey();
            }
        }

        /// <summary>    
        /// 从 .Net RSA 算法私钥参数转换为 Java 格式的 RSA 私钥
        /// </summary>
        /// <param name="privateKey">.Net生成的私钥参数</param>    
        /// <returns></returns>   
        public static string ToJavaPrivateKey(this RSAParameters privateKey)
        {
            AsymmetricCipherKeyPair keyPair = DotNetUtilities.GetRsaKeyPair(privateKey);
            PrivateKeyInfo info = PrivateKeyInfoFactory.CreatePrivateKeyInfo(keyPair.Private);
            byte[] bytes = info.ToAsn1Object().GetEncoded();
            return ToBase64(bytes);
        }
        /// <summary>    
        /// 从 .Net RSA 算法公钥参数转换为 Java 格式的 RSA 公钥  
        /// </summary>
        /// <param name="publicKey">.Net生成的公钥参数</param>    
        /// <returns></returns>   
        public static string ToJavaPublicKey(this RSAParameters publicKey)
        {
            RsaKeyParameters para = DotNetUtilities.GetRsaPublicKey(publicKey);
            SubjectPublicKeyInfo info = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(para);
            byte[] bytes = info.ToAsn1Object().GetDerEncoded();
            return ToBase64(bytes);
        }
        /// <summary>    
        /// 从 .Net RSA 实例中导出 Java 格式的 RSA 私钥 
        /// </summary>    
        /// <param name="rsa">.Net RSA 实例</param>    
        /// <returns></returns>   
        public static string ToJavaPrivateKey(this RSA rsa)
        {
            return rsa.ExportParameters(true).ToJavaPrivateKey();
        }
        /// <summary>    
        /// 从 .Net RSA 实例中导出 Java 格式的 RSA 公钥 
        /// </summary>
        /// <param name="publicKey">.Net生成的公钥</param>    
        /// <returns></returns>   
        public static string ToJavaPublicKey(this RSA rsa)
        {
            return rsa.ExportParameters(false).ToJavaPublicKey();
        }
        /// <summary>    
        /// 从 .Net RSA 实例中导出 Java 格式的 RSA 密钥 
        /// </summary>    
        /// <param name="rsa">.Net RSA 实例</param>  
        /// <param name="includePrivateParameters">若要包含私有参数，则为 true；否则为 false。 </param>    
        /// <returns></returns>   
        public static string ToJavaKey(this RSA rsa, bool includePrivateParameters)
        {
            return includePrivateParameters ? rsa.ToJavaPrivateKey() : rsa.ToJavaPublicKey();
        }
        #endregion

        #region 私钥加密
        /// <summary>
        /// 使用 BouncyCastle 方式的 RSA 私钥加密
        /// </summary>
        /// <param name="javaPrivateKey"></param>
        /// <param name="plaintext"></param>
        /// <returns></returns>
        public static string EncryptUsePrivateKey(string javaPrivateKey, string plaintext, string encoding = "UTF-8")
        {
            //转换密钥 
            RSAParameters para = FromJavaKey(javaPrivateKey, true);
            AsymmetricCipherKeyPair keyPair = DotNetUtilities.GetRsaKeyPair(para);
            //使用 RSA/ECB/PKCS1Padding 格式，Java 默认格式
            IBufferedCipher cipher = CipherUtilities.GetCipher("RSA/ECB/PKCS1Padding");
            cipher.Init(true, keyPair.Private);
            byte[] bytes = Encoding.GetEncoding(encoding).GetBytes(plaintext);
            byte[] result = cipher.DoFinal(bytes);
            return Convert.ToBase64String(result);
        }
        #endregion

        #region 公钥解密
        /// <summary>
        /// 使用 BouncyCastle 方式的 RSA 公钥解密
        /// </summary>
        /// <param name="javaPublicKey"></param>
        /// <param name="encryptedtext"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string DecryptUsePublicKey(string javaPublicKey, string ciphertext, string encoding = "UTF-8")
        {
            //转换密钥
            RSAParameters para = FromJavaKey(javaPublicKey, false);
            AsymmetricKeyParameter keyPara = DotNetUtilities.GetRsaPublicKey(para);
            //使用 RSA/ECB/PKCS1Padding 格式，Java 默认格式
            //IBufferedCipher cipher = CipherUtilities.GetCipher("RSA/ECB/PKCS1Padding");

            IBufferedCipher cipher = CipherUtilities.GetCipher("RSA");
            cipher.Init(false, keyPara);
            byte[] bytes = Convert.FromBase64String(ciphertext);
            byte[] result = cipher.DoFinal(bytes);
            return Convert.ToBase64String(result);
        }
        #endregion

        #region 加签    
        /// <summary>
        /// 使用 BouncyCastle 方式的 RSA 签名
        /// </summary>
        /// <param name="data">源数据</param>
        /// <param name="javaPrivateKey">Java 格式的私钥</param>
        /// <param name="hashAlgorithm">采用的哈希算法名称</param>
        /// <param name="encoding">字符串编码格式</param>
        /// <returns>Base64 格式编码的签名结果</returns>
        public static string SignDataByBouncyCastle(string data, string javaPrivateKey, string hashAlgorithm = "SHA256WithRSA", string encoding = "UTF-8")
        {
            RsaKeyParameters privateKeyParam = (RsaKeyParameters)PrivateKeyFactory.CreateKey(Convert.FromBase64String(javaPrivateKey));
            ISigner signer = SignerUtilities.GetSigner(hashAlgorithm);
            signer.Init(true, privateKeyParam);
            byte[] dataByte = Encoding.GetEncoding(encoding).GetBytes(data);
            signer.BlockUpdate(dataByte, 0, dataByte.Length);
            return Convert.ToBase64String(signer.GenerateSignature());
        }
        #endregion

        #region 验签
        /// <summary>
        /// 使用 BouncyCastle 方式的 RSA 签名验证
        /// </summary>
        /// <param name="data">源数据</param>
        /// <param name="javaPublicKey">Java 格式的公钥</param>
        /// <param name="signature">Base64 格式编码的签名结果</param>
        /// <param name="hashAlgorithm">采用的哈希算法名称</param>
        /// <param name="encoding">字符串编码格式</param>
        /// <returns></returns>
        public static bool VerifyDataByBouncyCastle(string data, string javaPublicKey, string signature, string hashAlgorithm = "MD5withRSA", string encoding = "UTF-8")
        {
            RsaKeyParameters publicKeyParam = (RsaKeyParameters)PublicKeyFactory.CreateKey(Convert.FromBase64String(javaPublicKey));
            ISigner signer = SignerUtilities.GetSigner(hashAlgorithm);
            signer.Init(false, publicKeyParam);
            byte[] dataByte = Encoding.GetEncoding(encoding).GetBytes(data);
            signer.BlockUpdate(dataByte, 0, dataByte.Length);
            byte[] signatureByte = Convert.FromBase64String(signature);
            return signer.VerifySignature(signatureByte);
        }
        #endregion

        /// <summary>
        /// 将字符串以指定的Encoding方式转换
        /// </summary>
        /// <param name="encodingText"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string ToBase64String(string encodingText, string encoding = "UTF-8")
        {
            byte[] bytes = Encoding.UTF8.GetBytes(encodingText);//Encoding.GetEncoding(encoding).GetBytes(encodingText);
            return Base64.ToBase64String(bytes);
        }
         
    }
}
