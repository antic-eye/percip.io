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
    }
    public class TimeStamp : IComparable<TimeStamp>
    {
        private DateTime timeStamp;
        private string user;
        private Direction direction;
        private List<string> tags = new List<string>();

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

        public List<string> Tags
        {
            get { return this.tags; }
            set { this.tags = value; }
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
