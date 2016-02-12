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

        public List<TimeStamp> TimeStamps
        {
            get { return stamps; }
            set { stamps = value; }
        }

        public override string ToString()
        {
            this.stamps.Sort();
            DateTime dtIn = DateTime.MinValue;
            DateTime dtOut = DateTime.MinValue;
            DateTime currentDay = DateTime.MinValue;

            List<string> days = new List<string>();

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
                        AddTimeSTring(ref sbOut, dtIn, dtOut);

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
            AddTimeSTring(ref sbOut, dtIn, dtOut);

            return sbOut.ToString();
        }

        private void AddTimeSTring(ref StringBuilder sb, DateTime dtIn, DateTime dtOut)
        {
            try
            {
                if (dtOut == DateTime.MinValue)
                    sb.AppendFormat("{0:yyyy-MM-dd ddd}\t {1:HH\\:mm} in and till now ({2:HH\\:mm}) {3:hh\\:mm} h of work", dtIn.Date, dtIn, DateTime.Now, (DateTime.Now - dtIn)).AppendLine();
                else
                    sb.AppendFormat("{0:yyyy-MM-dd ddd}\t {1:HH\\:mm} in and {2:HH\\:mm} out. {3:hh\\:mm} h of work", dtIn.Date, dtIn, dtOut, (dtOut - dtIn)).AppendLine();
            }
            catch (FormatException)
            {
                sb.AppendFormat("ERROR: dtIn {0}, dtOut {1}", dtIn, dtOut);
            }
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
