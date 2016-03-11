using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.IO;

namespace percip.io
{
    class CouchDBDataSaver : IDataSaver
    {
        public T Load<T>(string filename)
        {
            try
            {
                using (var fs = File.Open(filename, FileMode.Open))
                {
                    return (T)(new DataContractJsonSerializer(typeof(T))).ReadObject(fs);
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

        public void Save<T>(string filename, T obj)
        {
            try
            {
                using (var fs = File.Open(filename, FileMode.Create))
                {
                new DataContractJsonSerializer(typeof(T)).WriteObject(fs, obj);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Environment.Exit(-1);
            }
        }
    }
}
