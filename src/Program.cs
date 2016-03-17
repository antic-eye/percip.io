using Appccelerate.CommandLineParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Win32.TaskScheduler;
using System.Reflection;
using System.Runtime.Serialization;
using percip.io.Properties;

namespace percip.io
{
    public enum TriggerType
    {
        Logon,
        Unlock,
        Lock,
        Logoff
    }
    public enum ExitCode
    {
        Exception = -2,
        ArgumentError = -1,
        OK = 0
    }
    public class Program
    {
        private static string dbFile = Environment.CurrentDirectory + "\\times.db";
        private static string taskPrefix = "__percip.io__";
        private static IDataSaver Saver = new CouchDBDataSaver();

        static void Main(string[] args)
        {
            string direction = string.Empty;
            bool query = false;
            bool raw = false;
            bool init = false;
            bool deInit = false;
            bool help = false;
            bool delete = false;
            string inject = string.Empty;
            bool pause = false;

            var configuration = CommandLineParserConfigurator
                .Create()
                .WithSwitch("q", () => query = true).HavingLongAlias("query").DescribedBy("Call the db and get your working times.")
                .WithSwitch("r", () => raw = true).HavingLongAlias("raw").DescribedBy("Get all logged events")
                .WithSwitch("i", () => init = true).HavingLongAlias("init").DescribedBy("Create windows tasks (you need elevated permissions for this one!")
                .WithSwitch("d", () => deInit = true).HavingLongAlias("deinit").DescribedBy("Remove windows tasks (you need elevated permissions for this one!")
                .WithSwitch("b", () => pause = true).HavingLongAlias("pause").DescribedBy("Manage your breaks")
                .WithSwitch("del", () => delete = true).HavingLongAlias("delete").DescribedBy("Delete all Configuration")
                .WithSwitch("h", () => help = true).HavingLongAlias("help").DescribedBy("Show this usage screen.")
                .WithNamed("j", I => inject = I).HavingLongAlias("inject").DescribedBy("Time|Direction\"", "Use this for debugging only! You can inject timestamps. 1 for lock, 0 for unlock")
                .WithPositional(d => direction = d).DescribedBy("lock", "tell me to \"lock\" for \"out\" and keep empty for \"in\"")
                .BuildConfiguration();
            var parser = new CommandLineParser(configuration);

            var parseResult = parser.Parse(args);

            if (!parseResult.Succeeded)
            {
                ShowUsage(configuration, parseResult.Message);
                Environment.Exit((int)ExitCode.ArgumentError);
            }
            else
            {
                if (help)
                {
                    ShowUsage(configuration);
                    Environment.Exit((int)ExitCode.OK);
                }
                if(delete)
                {
                    Settings.Default.Reset();
                    Environment.Exit((int)ExitCode.OK);
                }
                if (init)
                {
                    using (TaskService ts = new TaskService())
                    {
                        try
                        {
                            DefineTask(ts, "myLock lock screen", "lock_pc", TriggerType.Lock);
                            DefineTask(ts, "myLock unlock screen", "unlock_pc", TriggerType.Unlock);
                            DefineTask(ts, "myLock login to pc", "login_pc", TriggerType.Logon);
                            DefineTask(ts, "myLock logout from pc", "logout_pc", TriggerType.Logoff);
                            Console.WriteLine("Initialization complete.");
                            Environment.Exit((int)ExitCode.OK);
                        }
                        catch (UnauthorizedAccessException)
                        {
                            Console.Error.WriteLine("Access denied, please use an elevated prompt.");
                            Environment.Exit((int)ExitCode.Exception);
                        }
                    }
                }
                if (deInit)
                {
                    using (TaskService ts = new TaskService())
                    {
                        try
                        {
                            //Remove tasks
                            foreach (var t in ts.RootFolder.AllTasks)
                                if (t.Name.StartsWith(taskPrefix))
                                    ts.RootFolder.DeleteTask(t.Name);

                            Console.WriteLine("Deinitialization complete.");
                            Environment.Exit((int)ExitCode.OK);
                        }
                        catch (UnauthorizedAccessException)
                        {
                            Console.Error.WriteLine("Access denied, please use an elevated prompt.");
                            Environment.Exit((int)ExitCode.Exception);
                        }
                    }
                }
                if (!string.IsNullOrEmpty(inject))
                {
                    TimeStampCollection col;
                    try
                    {
                        col = Saver.Load<TimeStampCollection>(dbFile);
                    }
                    catch (FileNotFoundException)
                    {
                        col = new TimeStampCollection();
                    }
                    TimeStamp stamp = new TimeStamp();
                    string[] injection = inject.Split('|');
                    stamp.Stamp = DateTime.Parse(injection[0]);
                    if (injection.Length > 1)
                        stamp.Direction = (Direction)(Convert.ToInt32(injection[1]));
                    if (injection.Length > 2)
                        stamp.User = injection[2];

                    col.TimeStamps.Add(stamp);
                    col.TimeStamps.Sort();
                    Saver.Save<TimeStampCollection>(dbFile, col);

                    Console.WriteLine("Injection successfull.");
                    Console.WriteLine("Values were: {0}", inject);

                    Environment.Exit((int)ExitCode.OK);
                }

                if (raw)
                {
                    TimeStampCollection col = Saver.Load<TimeStampCollection>(dbFile);
                    foreach (var t in col.TimeStamps)
                        Console.WriteLine("{0} {1} {2}", t.Stamp, t.User, t.Direction);

                    Console.WriteLine("EOF");

                    Environment.Exit((int)ExitCode.OK);
                }
                if (pause)
                {
                    interactivebreaktime(0);
                }
                if (!query)
                    LogTimeStamp(direction);
                else
                    QueryWorkingTimes();
            }
        }

        private static void interactivebreaktime(int offset)
        {
            Console.Clear();
            Console.WriteLine(@"
    ____                  __                                              
   / __ )________  ____ _/ /______ ___  ____ _____  ____ _____ ____  _____
  / __  / ___/ _ \/ __ `/ //_/ __ `__ \/ __ `/ __ \/ __ `/ __ `/ _ \/ ___/
 / /_/ / /  /  __/ /_/ / ,< / / / / / / /_/ / / / / /_/ / /_/ /  __/ /    
/_____/_/   \___/\__,_/_/|_/_/ /_/ /_/\__,_/_/ /_/\__,_/\__, /\___/_/     
                                                       /____/             
Here are your times:

");
            TimeStampCollection col = Saver.Load<TimeStampCollection>(dbFile);
            var max = col.TimeStamps.Count;
            for (int i = 1; i <= 10; i++)
            {
                if (max - offset - i > -1)
                {
                    Console.WriteLine("\t\t({0})\t{1}\t{2}", i, col.TimeStamps[max - offset - i].Stamp, col.TimeStamps[max - offset - i].Direction.ToString());
                }
                else
                {
                    Console.WriteLine("\t\tNo more times");
                    break;
                }
            }
            Console.WriteLine("\t\t(11)\tNone of the above");
            Console.Write("\nWhich ist the begin of your break?[1-10] ");
            try
            {
                int answer = Convert.ToInt32(Console.ReadLine());
                if (answer == 11)
                {
                    interactivebreaktime(offset + 10);
                    return;
                }
                TimeStamp start = col.TimeStamps[max - offset - answer];
                if (start.Direction != Direction.Out) throw new DirectionException();
                interactivesteptwo(max - offset - answer);

            }
            catch (DirectionException)
            {
                Console.Write("You chose an illegal time. Please retry!");
                interactivebreaktime(offset);
            }
            catch (Exception)
            {
                Console.Write("There is some Problem with you answer. Please retry!");
                interactivebreaktime(offset);
            }

        }

        private static void interactivesteptwo(int start)
        {
            Console.WriteLine("Possible end:");
            TimeStampCollection col = Saver.Load<TimeStampCollection>(dbFile);
            var max = col.TimeStamps.Count;
            for (int i = 1; i <= 15; i++)
            {
                if (max - start + i > -1)
                {
                    Console.WriteLine("\t\t({0})\t{1}\t{2}", i, col.TimeStamps[start + i].Stamp, col.TimeStamps[start + i].Direction.ToString());
                }
                else
                {
                    Console.WriteLine("\t\tNo more times");
                    break;
                }
            }
            Console.Write("\nPlease choose:[1-15] ");
            int answer = Convert.ToInt32(Console.ReadLine());
            int end = start + answer;
            TimeStamp endStamp = col.TimeStamps[end];
            if (endStamp.Direction != Direction.In) throw new DirectionException();
            Console.Write("Are you sure to irreversibly mark {0} to {1} as your break?[Y/n] ", col.TimeStamps[start].Stamp, col.TimeStamps[end].Stamp);
            if (Console.ReadLine() != "n")
            {
                do
                {
                    col.TimeStamps[start++].Direction = Direction.BR;
                } while (start <= end);
            }
            else interactivebreaktime(0);
            Saver.Save(dbFile, col);
            Environment.Exit(0);
        }

        private static void ShowUsage(CommandLineConfiguration configuration, string parseResult = "")
        {
            Usage usage = new UsageComposer(configuration).Compose();

            if (String.IsNullOrEmpty(parseResult))
            {
                var build = ((AssemblyInformationalVersionAttribute)Assembly
                              .GetAssembly(typeof(Program))
                              .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)[0])
                              .InformationalVersion;
                Console.WriteLine(@"
 ██▓███  ▓█████  ██▀███   ▄████▄   ██▓ ██▓███        ██▓ ▒█████  
▓██░  ██▒▓█   ▀ ▓██ ▒ ██▒▒██▀ ▀█  ▓██▒▓██░  ██▒     ▓██▒▒██▒  ██▒
▓██░ ██▓▒▒███   ▓██ ░▄█ ▒▒▓█    ▄ ▒██▒▓██░ ██▓▒     ▒██▒▒██░  ██▒
▒██▄█▓▒ ▒▒▓█  ▄ ▒██▀▀█▄  ▒▓▓▄ ▄██▒░██░▒██▄█▓▒ ▒     ░██░▒██   ██░
▒██▒ ░  ░░▒████▒░██▓ ▒██▒▒ ▓███▀ ░░██░▒██▒ ░  ░ ██▓ ░██░░ ████▓▒░
▒▓▒░ ░  ░░░ ▒░ ░░ ▒▓ ░▒▓░░ ░▒ ▒  ░░▓  ▒▓▒░ ░  ░ ▒▓▒ ░▓  ░ ▒░▒░▒░ 
░▒ ░      ░ ░  ░  ░▒ ░ ▒░  ░  ▒    ▒ ░░▒ ░      ░▒   ▒ ░  ░ ▒ ▒░ 
░░          ░     ░░   ░ ░         ▒ ░░░        ░    ▒ ░░ ░ ░ ▒  
            ░  ░   ░     ░ ░       ░             ░   ░      ░ ░  
                         ░                       ░               
Percip.io {0} - The working time logger by antic_eye ;)

Use this tool to track your productivity. MyLock generates an
encrypted database file that contains timestamps and ""in""
or ""out"".

When you call myLock with ""lock"" it tracks:
01.01.2016T08: 15 Max.Mustermann Out

When you call without args it tracks:
01.01.2016T08: 19 Max.Mustermann In

When you want to show your times, call it with ""--query"".It will
read the db and calculate your working time beginning with the
first ""in"" per day, ending with the last ""out"".

To automate the tracking, use ""--init"" and myLock will generate
Windows Scheduled tasks for screen lock/unlock and session
login/-out. You will need administrative permissions for this
task. Open an elevated command prompt.

", build);
                Console.WriteLine("Usage: percip.io.exe {0}", usage.Arguments);
                Console.WriteLine();
                Console.WriteLine(usage.Options);
                Console.WriteLine("Exit codes:");

                foreach (var e in Enum.GetValues(typeof(ExitCode)))
                    Console.WriteLine(string.Format("{0,4}\t{1}",
                        (int)e,
                        e.ToString()));
            }
            else
            {
                Console.WriteLine(parseResult);
                Console.WriteLine();
            }
        }

        private static void DefineTask(TaskService ts, string description, string taskName, TriggerType trigger)
        {
            string executable = string.Format("{0}\\percip.io.exe", AssemblyDirectory);
            TaskDefinition td = ts.NewTask();
            td.Principal.RunLevel = TaskRunLevel.Highest;
            td.Principal.LogonType = TaskLogonType.S4U;

            td.RegistrationInfo.Description = description;

            switch (trigger)
            {
                case TriggerType.Logon:
                    td.Triggers.Add(new LogonTrigger());
                    td.Actions.Add(new ExecAction(executable, "", AssemblyDirectory));
                    break;
                case TriggerType.Unlock:
                    td.Triggers.Add(new SessionStateChangeTrigger(TaskSessionStateChangeType.SessionUnlock));
                    td.Actions.Add(new ExecAction(executable, "", AssemblyDirectory));
                    break;
                case TriggerType.Logoff:
                    EventTrigger eTrigger = new EventTrigger();
                    eTrigger.Subscription = @"<QueryList><Query Id='1'><Select Path='System'>*[System[(EventID = 1074 or EventID = 7002 or EventID=42)]]</Select></Query></QueryList>";
                    eTrigger.ValueQueries.Add("Name", "Value");
                    td.Actions.Add(new ExecAction(executable, "lock", AssemblyDirectory));
                    td.Triggers.Add(eTrigger);
                    break;
                case TriggerType.Lock:
                    td.Triggers.Add(new SessionStateChangeTrigger(TaskSessionStateChangeType.SessionLock));
                    td.Actions.Add(new ExecAction(executable, "lock", AssemblyDirectory));
                    break;
            }
            ts.RootFolder.RegisterTaskDefinition(taskPrefix + taskName, td);
        }

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
        private static void QueryWorkingTimes()
        {
            //#if !DEBUG
            TimeStampCollection col = Saver.Load<TimeStampCollection>(dbFile);
            //#else
            //                    TimeStampCollection col = new TimeStampCollection();
            //                    col.TimeStamps.Add(new TimeStamp()
            //                    {
            //                        Direction = Direction.In,
            //                        Stamp = new DateTime(2015, 01, 20, 8, 30, 0),
            //                        User = Environment.UserName
            //                    });
            //                    col.TimeStamps.Add(new TimeStamp()
            //                    {
            //                        Direction = Direction.Out,
            //                        Stamp = new DateTime(2015, 01, 20,12, 30, 0),
            //                        User = Environment.UserName
            //                    });
            //                    col.TimeStamps.Add(new TimeStamp()
            //                    {
            //                        Direction = Direction.In,
            //                        Stamp = new DateTime(2015, 01, 20, 12, 45, 0),
            //                        User = Environment.UserName
            //                    });
            //                    col.TimeStamps.Add(new TimeStamp()
            //                    {
            //                        Direction = Direction.Out,
            //                        Stamp = new DateTime(2015, 01, 20, 16, 35, 0),
            //                        User = Environment.UserName
            //                    });
            //                    col.TimeStamps.Add(new TimeStamp()
            //                    {
            //                        Direction = Direction.In,
            //                        Stamp = new DateTime(2015, 01, 21, 8, 00, 0),
            //                        User = Environment.UserName
            //                    });
            //                    col.TimeStamps.Add(new TimeStamp()
            //                    {
            //                        Direction = Direction.Out,
            //                        Stamp = new DateTime(2015, 01, 21, 11, 45, 0),
            //                        User = Environment.UserName
            //                    });
            //                    col.TimeStamps.Add(new TimeStamp()
            //                    {
            //                        Direction = Direction.In,
            //                        Stamp = new DateTime(2015, 01, 21, 12, 30, 0),
            //                        User = Environment.UserName
            //                    });
            //                    col.TimeStamps.Add(new TimeStamp()
            //                    {
            //                        Direction = Direction.Out,
            //                        Stamp = new DateTime(2015, 01, 21, 18, 58, 0),
            //                        User = Environment.UserName
            //                    });
            //                    col.TimeStamps.Add(new TimeStamp()
            //                    {
            //                        Direction = Direction.In,
            //                        Stamp = new DateTime(2015, 01, 22, 8, 30, 0),
            //                        User = Environment.UserName
            //                    });
            //                    col.TimeStamps.Add(new TimeStamp()
            //                    {
            //                        Direction = Direction.Out,
            //                        Stamp = new DateTime(2015, 01, 22, 16, 30, 0),
            //                        User = Environment.UserName
            //                    });
            //#endif
            col.TimeStamps.Sort();
            DateTime dtIn = DateTime.MinValue;
            DateTime dtOut = DateTime.MinValue;
            DateTime currentDay = DateTime.MinValue;
            bool overlap = false;
            bool first = true;
            bool changed = false;
            bool retryday = false;

            List<string> days = new List<string>();

            for (int i = 0; i < col.TimeStamps.Count; i++)
            {
                DateTime nextDay = (i < col.TimeStamps.Count - 1) ? col.TimeStamps[i + 1].Stamp.Date : DateTime.Now.Date;
                if (!overlap && (currentDay == DateTime.MinValue || nextDay > currentDay || retryday))//1st run
                {
#if DEBUG
                    Console.WriteLine("Today is {0}, the next entry is from {1}; Changing day", currentDay, nextDay);
#endif
                    if (DateTime.MinValue != dtIn && DateTime.MinValue != dtOut && dtIn.Date == dtOut.Date && !retryday)
                        PrintTime(dtIn, dtOut);

                    currentDay = col.TimeStamps[i].Stamp.Date;
                    retryday = false;
                    if (first)
                    {
                        if (col.TimeStamps[i].Direction == Direction.In)//first unlock is start of work
                        {
                            dtIn = col.TimeStamps[i].Stamp;
                            dtOut = DateTime.MinValue;
                            first = false;
                        }
                        else
                        {
                            changed = true;
                            retryday = true;
                            Console.Write("The {0:yyyy-MM-dd ddd} starts with an Out at {1}. Do you want to add an In or delete the wrong Entry?[in/DEL]", currentDay, col.TimeStamps[i].Stamp);
                            string answer = Console.ReadLine();
                            switch (answer)
                            {
                                case "in":
                                case "i":
                                case "In":
                                    col = insert(col, i);
                                    i -= 2;
                                    break;
                                case "DEL":
                                case "del":
                                case "Del":
                                case "d":
                                default:
                                    col.TimeStamps.RemoveAt(i);
                                    i--;
                                    break;
                            }
                        }
                    }
                }
                if (col.TimeStamps[i].Direction == Direction.Out && ((nextDay > currentDay) || overlap))//lock is end of work
                {
                    dtOut = col.TimeStamps[i].Stamp;
                    overlap = false;
                    first = true;
                }
                else
                {
                    if (nextDay > currentDay && col.TimeStamps[i].Direction == Direction.In && !first)
                    {
                        changed = true;
                        Console.Write("Your last entry from {0:yyyy-MM-dd ddd} is In. Did you really work over night? [y/N]", currentDay);
                        string answer = Console.ReadLine();
                        switch (answer)
                        {
                            case "y":
                            case "j":
                            case "Y":
                            case "J":
                                overlap = true;
                                first = true;
                                break;
                            case "n":
                            case "N":
                            default:
                                col = Repair(col, currentDay, i);
                                i--;
                                break;
                        }
                    }
                }

            }
            PrintTime(dtIn, dtOut);
            if (changed)
            {
                Saver.Save(dbFile, col);
            }
        }

        private static TimeStampCollection insert(TimeStampCollection col, int i)
        {
            Console.Write("Please enter the TimeStamp to add:");
            string answer = Console.ReadLine();
            try
            {
                TimeStamp toadd = new TimeStamp();
                toadd.Stamp = DateTime.Parse(answer);
                toadd.Direction = Direction.In;
                toadd.User = "REPAIR";
                col.TimeStamps.Insert(i - 1, toadd);
            }
            catch (FormatException)
            {
                Console.WriteLine("The TimeStamp could not be read. Please try again");
                col = insert(col, i);
            }
            return col;
        }

        private static TimeStampCollection Repair(TimeStampCollection col, DateTime currentDay, int i)
        {
            string answer;
            Console.Write("Please enter the time you stopped working on {0:yyyy-MM-dd ddd}. [{1}]", currentDay, col.TimeStamps[i].Stamp.AddSeconds(1));
            answer = Console.ReadLine();
            if (answer == "")
            {
                answer = col.TimeStamps[i].Stamp.AddSeconds(1).ToString();
            }
            try
            {
                TimeStamp toadd = new TimeStamp();
                toadd.Stamp = DateTime.Parse(answer);
                toadd.Direction = Direction.Out;
                toadd.User = "REPAIR";
                col.TimeStamps.Insert(i + 1, toadd);
            }
            catch (FormatException)
            {
                Console.WriteLine("The Timestamp could not be read, please try again or simply press Enter to enter the shown Timestamp.");
                col = Repair(col, currentDay, i);
            }

            return col;
        }

        private static void PrintTime(DateTime dtIn, DateTime dtOut)
        {
            try
            {
                if (dtOut == DateTime.MinValue)
                    Console.WriteLine("{0:yyyy-MM-dd ddd}\t {1:HH\\:mm} in and till now ({2:HH\\:mm}) {3:hh\\:mm} h of work", dtIn.Date, dtIn, DateTime.Now, breaked(DateTime.Now,dtIn));
                else
                    Console.WriteLine("{0:yyyy-MM-dd ddd}\t {1:HH\\:mm} in and {2:HH\\:mm} out. {3:hh\\:mm} h of work", dtIn.Date, dtIn, dtOut, breaked(dtOut,dtIn));
            }
            catch (FormatException)
            {
                Console.Error.WriteLine("dtIn {0}", dtIn);
                Console.Error.WriteLine("dtOut {0}", dtOut);
            }
        }

        private static TimeSpan breaked(DateTime dtOut, DateTime dtIn)
        {
            TimeStampCollection col = Saver.Load<TimeStampCollection>(dbFile);
            List<TimeStamp> range = col.TimeStamps.Where(me => dtIn <= me.Stamp && me.Stamp <= dtOut).ToList();
            range.Sort();
            TimeSpan result = dtOut - dtIn;
            try
            {
                TimeStamp first;
                TimeStamp second;
                for (int i = 0; i < range.Count; i++)
                {
                    first = range[i];
                    second = range[i + 1];
                    if (first.Direction == Direction.BR && second.Direction == Direction.BR)
                    {
                        result -= second.Stamp - first.Stamp;
                    }
                }
            }
            catch { }
            return result;

        }

        private static void LogTimeStamp(string direction)
        {
            Direction dir = Direction.In;
            if (direction.Trim().ToLowerInvariant() == "lock")
                dir = Direction.Out;

            TimeStamp stamp = new TimeStamp()
            {
                Direction = dir,
                Stamp = DateTime.Now,
                User = Environment.UserName
            };


            TimeStampCollection col = null;

            try
            {
                col = Saver.Load<TimeStampCollection>(dbFile);
            }
            catch (FileNotFoundException)
            {
                col = new TimeStampCollection();
            }



            col.TimeStamps.Add(stamp);
            Saver.Save<TimeStampCollection>(dbFile, col);
            Console.WriteLine("Saved: {0}|{1}|{2}", stamp.Stamp, stamp.User, stamp.Direction);
        }
    }

    [Serializable]
    internal class DirectionException : Exception
    {
        public DirectionException()
        {
        }

        public DirectionException(string message) : base(message)
        {
        }

        public DirectionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DirectionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
