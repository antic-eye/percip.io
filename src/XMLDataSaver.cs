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
                Console.Error.WriteLine("{0} could not be found", filename);
                Environment.Exit(-1);
                return default(T);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Exception during run: {0}", ex.Message);
                Environment.Exit(-2);
                return default(T);
            }
        }

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
                Console.Error.WriteLine(ex.Message);
                Environment.Exit(-1);
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
