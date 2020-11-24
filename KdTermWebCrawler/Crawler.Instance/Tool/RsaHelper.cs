using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Crawler.Instance
{
    public static class RSAUtils
    {

        private static StreamReader GetStreamReader(string content)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            var memory = new MemoryStream(bytes);
            var reader = new StreamReader(memory);

            return reader;
        }

        private static RSAParameters GenerateRSAParameterWithXmlPrivateKey(string xmlPrivateKey)
        {
            var rsa = RSA.Create();
            rsa.FromXmlString(xmlPrivateKey);

            if (!(DotNetUtilities.GetRsaKeyPair(rsa) is AsymmetricCipherKeyPair asymmetricCipherKeyPair))
            {
                throw new Exception("Private key format is incorrect");
            }
            RsaPrivateCrtKeyParameters rsaPrivateCrtKeyParameters =
                (RsaPrivateCrtKeyParameters)PrivateKeyFactory.CreateKey(
                    PrivateKeyInfoFactory.CreatePrivateKeyInfo(asymmetricCipherKeyPair.Private));
            var rsap = new RSAParameters();
            rsap.Modulus = rsaPrivateCrtKeyParameters.Modulus.ToByteArrayUnsigned();
            rsap.Exponent = rsaPrivateCrtKeyParameters.PublicExponent.ToByteArrayUnsigned();
            rsap.P = rsaPrivateCrtKeyParameters.P.ToByteArrayUnsigned();
            rsap.Q = rsaPrivateCrtKeyParameters.Q.ToByteArrayUnsigned();
            rsap.DP = rsaPrivateCrtKeyParameters.DP.ToByteArrayUnsigned();
            rsap.DQ = rsaPrivateCrtKeyParameters.DQ.ToByteArrayUnsigned();
            rsap.InverseQ = rsaPrivateCrtKeyParameters.QInv.ToByteArrayUnsigned();
            rsap.D = rsaPrivateCrtKeyParameters.Exponent.ToByteArrayUnsigned();

            return rsap;
        }

        private static RSAParameters GenerateRSAParameterWithPkcs1PrivateKey(string privateKeyStr)
        {
            using (var txtreader = GetStreamReader(privateKeyStr))
            {
                PemReader pr = new PemReader(txtreader);
                if (!(pr.ReadObject() is AsymmetricCipherKeyPair asymmetricCipherKeyPair))
                {
                    throw new Exception("Private key format is incorrect");
                }
                RsaPrivateCrtKeyParameters rsaPrivateCrtKeyParameters =
                    (RsaPrivateCrtKeyParameters)PrivateKeyFactory.CreateKey(
                        PrivateKeyInfoFactory.CreatePrivateKeyInfo(asymmetricCipherKeyPair.Private));
                var rsap = new RSAParameters();
                rsap.Modulus = rsaPrivateCrtKeyParameters.Modulus.ToByteArrayUnsigned();
                rsap.Exponent = rsaPrivateCrtKeyParameters.PublicExponent.ToByteArrayUnsigned();
                rsap.P = rsaPrivateCrtKeyParameters.P.ToByteArrayUnsigned();
                rsap.Q = rsaPrivateCrtKeyParameters.Q.ToByteArrayUnsigned();
                rsap.DP = rsaPrivateCrtKeyParameters.DP.ToByteArrayUnsigned();
                rsap.DQ = rsaPrivateCrtKeyParameters.DQ.ToByteArrayUnsigned();
                rsap.InverseQ = rsaPrivateCrtKeyParameters.QInv.ToByteArrayUnsigned();
                rsap.D = rsaPrivateCrtKeyParameters.Exponent.ToByteArrayUnsigned();

                return rsap;
            }
        }

        /// <summary>
        /// 读取pfx证书，并将密钥存储为PKCS#8格式
        /// </summary>
        /// <param name="pfxFileName"></param>
        /// <param name="password"></param>
        public static void ConvertPfxToPkcs8(string pfxFileName, string password)
        {
            var certificate = ReadX509Certificate(pfxFileName, password);
            var rsa = RSA.Create();
            rsa.FromXmlString(certificate.PrivateKey.ToXmlString(true));

            var bcKeyPair = DotNetUtilities.GetRsaKeyPair(rsa);
            var pkcs8Gen = new Pkcs8Generator(bcKeyPair.Private);
            var pemObj = pkcs8Gen.Generate();
            var pkcs8Out = new StreamWriter(@"e:\privkey.pk8", false);
            var pemWriter = new PemWriter(pkcs8Out);
            pemWriter.WriteObject(pemObj);
            pkcs8Out.Close();
        }

        /// <summary>
        /// 读取密钥证书
        /// </summary>
        /// <param name="pfxFileName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private static X509Certificate2 ReadX509Certificate(string pfxFileName, string password)
        {
            return new X509Certificate2(pfxFileName, password, X509KeyStorageFlags.Exportable);
        }

        public static X509Certificate2 ReadX509Certificate(string content)
        {
            var bytes = Encoding.UTF8.GetBytes(content);

            return new X509Certificate2(bytes);
        }

        /// <summary>
        /// 使用PFX证书，对数据进行PKCS#12签名
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="pfxFileName">密钥证书路径</param>
        /// <param name="password">密钥证书读取密码</param>
        /// <returns></returns>
        public static string SignWithPfx(string data, string pfxFileName, string password)
        {
            var certificate = ReadX509Certificate(pfxFileName, password);
            var rsa = RSA.Create();
            rsa.KeySize = certificate.PrivateKey.KeySize;
            var rsaPara = GenerateRSAParameterWithXmlPrivateKey(certificate.PrivateKey.ToXmlString(true));
            rsa.ImportParameters(rsaPara);

            var bytes = Encoding.UTF8.GetBytes(data);
            var signBytes = rsa.SignData(bytes, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
            return Convert.ToBase64String(signBytes);
        }

        /// <summary>
        /// 使用PFX证书，对数据进行签名验证
        /// </summary>
        /// <param name="data"></param>
        /// <param name="pfxFileName"></param>
        /// <param name="password"></param>
        /// <param name="signature"></param>
        /// <returns></returns>
        public static bool VerifyWithPfx(string data, string pfxFileName, string password, string signature)
        {
            var cert = ReadX509Certificate(pfxFileName, password);
            return VerifyWithPfx(data, signature, cert);
        }

        private static bool VerifyWithPfx(string data, string signature, X509Certificate2 cert)
        {
            var rsa = RSA.Create();
            rsa.FromXmlString(cert.PublicKey.Key.ToXmlString(false));

            var bcKeyPair = DotNetUtilities.GetRsaPublicKey(rsa);

            ISigner signer = SignerUtilities.GetSigner("SHA1withRSA");
            signer.Init(false, bcKeyPair);

            var expectedSig = Convert.FromBase64String(signature);
            var msgBytes = Encoding.UTF8.GetBytes(data);
            signer.BlockUpdate(msgBytes, 0, msgBytes.Length);
            return signer.VerifySignature(expectedSig);
        }

        /// <summary>
        /// 使用PKCS#12密钥，对数据进行加密
        /// </summary>
        /// <param name="data"></param>
        /// <param name="privateKeyStr">-----BEGIN RSA PRIVATE KEY-----</param>
        /// <returns></returns>
        public static string SignWithPKC12(string data, string privateKeyStr)
        {
            using (var txtreader = GetStreamReader(privateKeyStr))
            {
                var keyPair = (AsymmetricCipherKeyPair)new PemReader(txtreader).ReadObject();

                ISigner signer = SignerUtilities.GetSigner("SHA1withRSA");
                signer.Init(true, keyPair.Private);

                var bytes = Encoding.UTF8.GetBytes(data);
                signer.BlockUpdate(bytes, 0, bytes.Length);
                byte[] signature = signer.GenerateSignature();

                return Convert.ToBase64String(signature);
            }
        }

        /// <summary>
        /// 使用公钥证书(-----BEGIN CERTIFICATE-----)，对签名进行验证
        /// </summary>
        /// <param name="data"></param>
        /// <param name="signature"></param>
        /// <param name="publicKey">-----BEGIN CERTIFICATE-----</param>
        /// <returns></returns>
        public static bool VerifyWithCert(string data, string signature, string publicKey)
        {
            var cert = ReadX509Certificate(publicKey);
            return VerifyWithPfx(data, signature, cert);
        }

        /// <summary>
        /// 使用PKCS#12公钥(-----BEGIN PUBLIC KEY-----)，对签名进行验证
        /// </summary>
        /// <param name="data"></param>
        /// <param name="signature"></param>
        /// <param name="publicKeyStr">-----BEGIN PUBLIC KEY-----</param>
        /// <returns></returns>
        public static bool VerifyWithPKCS12(string data, string signature, string publicKeyStr)
        {
            using (var sr = GetStreamReader(publicKeyStr))
            {
                var keyParameter = (AsymmetricKeyParameter)new PemReader(sr).ReadObject();

                ISigner signer = SignerUtilities.GetSigner("SHA1withRSA");
                signer.Init(false, keyParameter);

                var expectedSig = Convert.FromBase64String(signature);
                var msgBytes = Encoding.UTF8.GetBytes(data);
                signer.BlockUpdate(msgBytes, 0, msgBytes.Length);

                return signer.VerifySignature(expectedSig);
            }
        }

        public static string AESEncrypt(string content, string privateKeyStr)
        {
            var privateKeyBytes = Encoding.UTF8.GetBytes(privateKeyStr);
            var encryptBytes = Encoding.UTF8.GetBytes(content);

            RijndaelManaged managed = new RijndaelManaged();
            managed.Key = privateKeyBytes;
            managed.Mode = CipherMode.ECB;
            managed.Padding = PaddingMode.PKCS7;

            ICryptoTransform transform = managed.CreateEncryptor();
            byte[] resultArray = transform.TransformFinalBlock(encryptBytes, 0, encryptBytes.Length);

            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        public static string AESDecrypt(string encryptStr, string privateKeyStr)
        {
            var publicKeyBytes = Encoding.UTF8.GetBytes(privateKeyStr);
            var encryptBytes = Convert.FromBase64String(encryptStr);

            RijndaelManaged managed = new RijndaelManaged();
            managed.Key = publicKeyBytes;
            managed.Mode = CipherMode.ECB;
            managed.Padding = PaddingMode.PKCS7;

            ICryptoTransform transform = managed.CreateDecryptor();
            byte[] resultArray = transform.TransformFinalBlock(encryptBytes, 0, encryptBytes.Length);

            return Encoding.UTF8.GetString(resultArray);
        }

        public static string RSADecrypt(string encryptStr, string publicKeyStr, int secretLength)
        {
            var bytesToDecrypt = Convert.FromBase64String(encryptStr);
            var decryptEngine = new Pkcs1Encoding(new RsaEngine(), secretLength);

            using (var sr = GetStreamReader(publicKeyStr))
            {
                var keyParameter = (AsymmetricKeyParameter)new PemReader(sr).ReadObject();
                decryptEngine.Init(false, keyParameter);
            }

            var processBlock = decryptEngine.ProcessBlock(bytesToDecrypt, 0, bytesToDecrypt.Length);
            var decryptStr = Encoding.UTF8.GetString(processBlock);
            return decryptStr;
        }

        public static string RSAEncrypt(string content, string privateKeyStr)
        {
            //content.NotNull(nameof(content));

            var bytesToEncrypt = Encoding.UTF8.GetBytes(content);

            var encryptEngine = new Pkcs1Encoding(new RsaEngine());

            using (var txtreader = GetStreamReader(privateKeyStr))
            {
                var keyPair = (AsymmetricCipherKeyPair)new PemReader(txtreader).ReadObject();

                encryptEngine.Init(true, keyPair.Private);
            }

            var encrypted = Convert.ToBase64String(encryptEngine.ProcessBlock(bytesToEncrypt, 0, bytesToEncrypt.Length));
            return encrypted;
        }

        /// <summary>
        /// RSA加密+base64
        /// </summary>
        /// <param name="publickey">公钥</param>
        /// <param name="content">原文</param>
        /// <returns>加密后的密文字符串</returns>
        public static string RSAEncrypt1(string publickey, string content)
        {
            //最大文件加密块
            int MAX_ENCRYPT_BLOCK = 245;

            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            byte[] cipherbytes;
            rsa.FromXmlString(publickey);
            byte[] contentByte = Encoding.UTF8.GetBytes(content);
            int inputLen = contentByte.Length;

            int offSet = 0;
            byte[] cache;
            int i = 0;
            System.IO.MemoryStream aMS = new System.IO.MemoryStream();
            // 对数据分段加密
            while (inputLen - offSet > 0)
            {
                byte[] temp = new byte[MAX_ENCRYPT_BLOCK];
                if (inputLen - offSet > MAX_ENCRYPT_BLOCK)
                {
                    Array.Copy(contentByte, offSet, temp, 0, MAX_ENCRYPT_BLOCK);
                    cache = rsa.Encrypt(Encoding.UTF8.GetBytes(content), false);
                }
                else
                {
                    Array.Copy(contentByte, offSet, temp, 0, inputLen - offSet);
                    cache = rsa.Encrypt(Encoding.UTF8.GetBytes(content), false);
                }
                aMS.Write(cache, 0, cache.Length);
                i++;
                offSet = i * MAX_ENCRYPT_BLOCK;
            }

            cipherbytes = aMS.ToArray();
            return Convert.ToBase64String(cipherbytes);
        }

        /// <summary>
        /// RSA解密
        /// </summary>
        /// <param name="privatekey">私钥</param>
        /// <param name="content">密文（RSA+base64）</param>
        /// <returns>解密后的字符串</returns>
        public static string RSADecrypt(string privatekey, string content)
        {
            //最大文件解密块
            int MAX_DECRYPT_BLOCK = 256;

            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            byte[] cipherbytes;
            rsa.FromXmlString(privatekey);
            byte[] contentByte = Convert.FromBase64String(content);
            int inputLen = contentByte.Length;

            // 对数据分段解密
            int offSet = 0;
            int i = 0;
            byte[] cache;
            System.IO.MemoryStream aMS = new System.IO.MemoryStream();
            while (inputLen - offSet > 0)
            {
                byte[] temp = new byte[MAX_DECRYPT_BLOCK];
                if (inputLen - offSet > MAX_DECRYPT_BLOCK)
                {
                    Array.Copy(contentByte, offSet, temp, 0, MAX_DECRYPT_BLOCK);
                    cache = rsa.Decrypt(temp, false);
                }
                else
                {
                    Array.Copy(contentByte, offSet, temp, 0, inputLen - offSet);
                    cache = rsa.Decrypt(temp, false);
                }
                aMS.Write(cache, 0, cache.Length);
                i++;
                offSet = i * MAX_DECRYPT_BLOCK;
            }
            cipherbytes = aMS.ToArray();

            return Encoding.UTF8.GetString(cipherbytes);
        }
         
    }
}

