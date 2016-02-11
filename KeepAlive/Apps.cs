using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeepAlive
{
    public class Apps
    {
        public static Settings Settings { get; set; }
    }
    public class Settings
    {
        public Task[] Tasks { get; set; }
    }

    public class Task
    {
        public string Url { get; set; }
        public double Interval { get; set; }
    }
}
