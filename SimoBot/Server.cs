﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimoBot
{
	class Server
	{
        public string server;
		public List<Channel> channels;
		public Server(string server, List<Channel> channels)
		{
			this.server = server;
			this.channels = channels;
		}
	}
}
