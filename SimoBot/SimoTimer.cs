using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace SimoBot
{
    class SimoTimer : Timer
    {
		public TimerHandler th;
        public SimoTimer(string nick, string message, DateTime time, TimerHandler th)
            : base()
        {
			this.th = th;
			this.nick = nick;
			this.message = message;
			this.time = time;
			this.AutoReset = false;
        }



		public string message
		{
			get; set;
		}

		public string nick
		{
			get; set;
		}

		public DateTime time
		{
			get; set;
		}
    }
}
