using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace percip.io
{
    public class TimeStampCollection
    {
        private List<TimeStamp> stamps = new List<TimeStamp>();
        private TimeSpan workingTimeSpan = new TimeSpan(0);

        public List<TimeStamp> TimeStamps
        {
            get { return stamps; }
            set { stamps = value; }
        }

        public TimeSpan WorkingTime
        {
            get { return this.workingTimeSpan; }
            set { this.workingTimeSpan = value; }
        }

        public override string ToString()
        {
            this.stamps.Sort();
            DateTime dtIn = DateTime.MinValue;
            DateTime dtOut = DateTime.MinValue;
            DateTime currentDay = DateTime.MinValue;

            List<string> days = new List<string>();
            TimeSpan globalWorkingTime = new TimeSpan(0);

            StringBuilder sbOut = new StringBuilder();
            for (int i = 0; i < this.stamps.Count; i++)
            {
                DateTime nextDay = (i < this.stamps.Count - 1) ? this.stamps[i + 1].Stamp.Date : DateTime.Now.Date;

                if (currentDay == DateTime.MinValue || nextDay > currentDay)//1st run
                {
#if DEBUG
                    Console.WriteLine("Today is {0}, the next entry is from {1}; Changing day", currentDay, nextDay);
#endif
                    if (DateTime.MinValue != dtIn && DateTime.MinValue != dtOut && dtIn.Date == dtOut.Date)
                        globalWorkingTime += AddTimeString(ref sbOut, dtIn, dtOut);

                    currentDay = this.stamps[i].Stamp.Date;
                    if (this.stamps[i].Direction == Direction.In)//first unlock is start of work
                    {
                        dtIn = this.stamps[i].Stamp;
                        dtOut = DateTime.MinValue;
                    }
                }
                if (this.stamps[i].Direction == Direction.Out && (nextDay > currentDay))//lock is end of work
                    dtOut = this.stamps[i].Stamp;
            }
            globalWorkingTime += AddTimeString(ref sbOut, dtIn, dtOut);

            sbOut.AppendFormat("Total hours over working time: {0:hh\\:mm}.", globalWorkingTime);

            return sbOut.ToString();
        }

        private TimeSpan AddTimeString(ref StringBuilder sb, DateTime dtIn, DateTime dtOut)
        {
            TimeSpan ret = new TimeSpan(0);
            try
            {
                if (dtOut == DateTime.MinValue)
                    sb.AppendFormat("{0:yyyy-MM-dd ddd}\t {1:HH\\:mm} in and till now ({2:HH\\:mm}) {3:hh\\:mm} h of work", dtIn.Date, dtIn, DateTime.Now, (DateTime.Now - dtIn)).AppendLine();
                else
                {
                    ret = dtOut - dtIn;
                    ret = ret - this.workingTimeSpan;
                    sb.AppendFormat("{0:yyyy-MM-dd ddd}\t {1:HH\\:mm} in and {2:HH\\:mm} out. {3:hh\\:mm} h of work ({4:hh\\:mm})", dtIn.Date, dtIn, dtOut, (dtOut - dtIn), ret).AppendLine();
                }
            }
            catch (FormatException)
            {
                sb.AppendFormat("ERROR: dtIn {0}, dtOut {1}", dtIn, dtOut);
            }
            return ret;
        }
    }
    public class TimeStamp:IComparable<TimeStamp>
    {
        private DateTime timeStamp;
        private string user;
        private Direction direction;

        public DateTime Stamp
        {
            get { return this.timeStamp; }
            set { this.timeStamp = value; }
        }
        public string User
        {
            get { return this.user; }
            set { this.user = value; }
        }

        public Direction Direction
        {
            get { return this.direction; }
            set
            {
                this.direction = value;
            }
        }

        public int CompareTo(TimeStamp other)
        {
            if (other == null)
                return 1;
            else
                return this.Stamp.CompareTo(other.Stamp);
        }
    }
    public enum Direction
    {
        In,
        Out
    }
}
