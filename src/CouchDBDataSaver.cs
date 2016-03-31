using System;
using System.Linq;
using System.IO;
using Newtonsoft.Json.Linq;
using percip.io.Properties;
using LoveSeat;

namespace percip.io
{
    class CouchDBDataSaver : IDataSaver
    {
        public T Load<T>(string filename)
        {
            //#if DEBUG
            //            Settings.Default["Docid"] = null;
            //#endif
            CouchDatabase db = connect(Path.GetFileNameWithoutExtension(filename));
            try
            {
                return db.GetDocument(Settings.Default.Docid).ToObject<T>();
            }
            catch (Exception ex)
            {

                Console.Error.WriteLine(ex.Message);
                Environment.Exit(-1);
                return default(T);
            }
        }

        private CouchDatabase connect(string dbname)
        {
            var client = new CouchClient(
                Settings.Default.CouchIP, 
                Settings.Default.CouchPort, 
                Settings.Default.CouchAdmin,
                Settings.Default.CouchPW,
                Settings.Default.isHTTPS, AuthenticationType.Basic);

            if (!client.HasDatabase(dbname))
                client.CreateDatabase(dbname);

            var db = client.GetDatabase(dbname);
            return db;
        }

        public void Save<T>(string filename, T obj)
        {
            CouchDatabase db = connect(Path.GetFileNameWithoutExtension(filename));
            Document working;
            //#if DEBUG
            //            Settings.Default["Docid"] = null;
            //#endif
            try
            {
                if (!String.IsNullOrEmpty(Settings.Default.Docid))
                {
                    working = db.GetDocument(Settings.Default.Docid);
                    using (var reader = working.CreateReader())
                    {
                        var writer = new JObject();
                        var temp = JToken.Load(reader).Cast<JToken>().ToArray();
                        temp[2] = JToken.FromObject(obj).First;
                        foreach (var item in temp)
                            writer.Add(item);

                        db.SaveDocument(new Document(writer));
                    }
                }
                else
                {
                    working = new Document(JToken.FromObject(obj).ToString());
                    working.Add("_id", string.Format("{0}-{1}-{2}",
                        JProperty.FromObject(obj.ToString().Split('.').Last()), obj.GetHashCode(), this.GetHashCode()));
                    Settings.Default.Docid = working.Value<string>("_id");
                    Settings.Default.Save();
                    Console.WriteLine(working.Last);
                    db.SaveDocument(working);
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
