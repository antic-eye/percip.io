using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace percip.io
{
    class XMLDataSaver : IDataSaver
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filename"></param>
        /// <param name="encryptionKey"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="Exception"></exception>
        private T DecryptAndDeserialize<T>(string filename, string encryptionKey)
        {
            var key = new DESCryptoServiceProvider();
            int length = encryptionKey.Length / 2;
            byte[] k = Encoding.ASCII.GetBytes(encryptionKey.Substring(0, length));
            byte[] iV = Encoding.ASCII.GetBytes(encryptionKey.Substring(length));
            var d = key.CreateDecryptor(k, iV);
            try
            {
                using (var fs = File.Open(filename, FileMode.Open))
                {
                    using (var cs = new CryptoStream(fs, d, CryptoStreamMode.Read))
                    {
                        return (T)(new XmlSerializer(typeof(T))).Deserialize(cs);
                    }
                }
            }
            catch (FileNotFoundException)
            {
                throw new FileNotFoundException("{0} could not be found", filename);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filename"></param>
        /// <param name="obj"></param>
        /// <param name="encryptionKey"></param>
        /// <exception cref="Exception"></exception>
        private void EncryptAndSerialize<T>(string filename, T obj, string encryptionKey)
        {
            var key = new DESCryptoServiceProvider();
            int length = encryptionKey.Length / 2;
            byte[] k = Encoding.ASCII.GetBytes(encryptionKey.Substring(0, length));
            byte[] iV = Encoding.ASCII.GetBytes(encryptionKey.Substring(length));
            var e = key.CreateEncryptor(k, iV);
            try
            {
                using (var fs = File.Open(filename, FileMode.Create))
                {
                    using (var cs = new CryptoStream(fs, e, CryptoStreamMode.Write))
                    {
                        (new XmlSerializer(typeof(T))).Serialize(cs, obj);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private string GetKey()
        {
            string sMyKey = Environment.UserName + "@" + Environment.UserDomainName;
            int iBitSize = 16;
            if (sMyKey.Length > iBitSize)
                sMyKey = sMyKey.Substring(0, iBitSize);
            if (sMyKey.Length < iBitSize)
                for (int i = sMyKey.Length; i < iBitSize; i++)
                    sMyKey = "~" + sMyKey;
            return sMyKey;
        }

        public T Load<T>(string filename)
        {
            return DecryptAndDeserialize<T>(filename, GetKey());
        }

        public void Save<T>(string filename, T obj)
        {
            EncryptAndSerialize<T>(filename, obj, GetKey());
        }
    }
}
