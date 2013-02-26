using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace SimoBot
{
    class Program
    {
        static void Main(string[] args)
        {
            Engine theEngine = new Engine();
            while (theEngine.bgwIrcReader.IsBusy)
            {
                System.Threading.Thread.Sleep(250);
            }
        }
    }
}
