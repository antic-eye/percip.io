using System;
using System.Linq;
using System.IO;
using Newtonsoft.Json.Linq;
using percip.io.Properties;
using LoveSeat;
using System.Reflection;

namespace percip.io
{
    class CouchDBDataSaver : IDataSaver
    {
        public T Load<T>(string filename) where T : class
        {
            //#if DEBUG
            //            Settings.Default["Docid"] = null;
            //#endif
            CouchDatabase db;
            IDataSaver Saver = new XMLDataSaver();
            try
            {
                db = connect(Path.GetFileNameWithoutExtension(filename));
            }
            catch
            {
                return Saver.Load<T>(filename);
            }
            TimeStampCollection myversion = null;
            if (typeof(T) == typeof(TimeStampCollection))
            {
                myversion = Saver.Load<TimeStampCollection>(filename);
            }
            try
            {
                Document data = db.GetDocument(Settings.Default["Docid"] as string);
                if (typeof(T) == typeof(TimeStampCollection) && data.ToObject<TimeStampCollection>().Fingerprint != myversion.Fingerprint)
                {
                    return myversion as T;
                }
                return data.ToObject<T>();
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
            var config = Settings.Default;
            var client = new CouchClient((string)config["CouchIP"], (int)config["CouchPort"], (string)config["CouchAdmin"], (string)config["CouchPW"], (bool)config["isHTTPS"], AuthenticationType.Basic);
            if (!client.HasDatabase(dbname))
            {
                client.CreateDatabase(dbname);
            }
            var db = client.GetDatabase(dbname);
            return db;
        }

        public void Save<T>(string filename, T obj) where T : class
        {
            if (typeof(T) == typeof(TimeStampCollection))
            {
                var me = obj as TimeStampCollection;
                me.Fingerprint = Guid.NewGuid().ToString();
                obj = me as T;
            }
            new XMLDataSaver().Save(filename, obj);
            CouchDatabase db;
            try
            {
                db = connect(Path.GetFileNameWithoutExtension(filename));
            }
            catch { return; }
            Document working;
            //#if DEBUG
            //            Settings.Default["Docid"] = null;
            //#endif
            try
            {
                if (Settings.Default["Docid"] != null && Settings.Default["Docid"] as string != "")
                {
                    working = db.GetDocument(Settings.Default["Docid"] as string);
                    var reader = working.CreateReader();
                    var writer = new JObject();

                    var temp = JToken.Load(reader).Cast<JToken>().ToArray();
                    temp[2] = JToken.FromObject(obj).First;
                    foreach (var item in temp)
                    {
                        writer.Add(item);
                    }
                    db.SaveDocument(new Document(writer));
                }
                else
                {
                    working = new Document(JToken.FromObject(obj).ToString());
                    working.Add("_id", JProperty.FromObject(obj.ToString().Split('.').Last() + "-" + obj.GetHashCode() + "-" + this.GetHashCode()));
                    Settings.Default["Docid"] = working.Value<string>("_id");
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
