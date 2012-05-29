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
            DictionaryLoader DirLdr = new DictionaryLoader();
            ExternalMySQL ems = new ExternalMySQL();
            Parser pr = new Parser();
            Engine SimoEngine = new Engine(pr, ems, DirLdr);
            StartUp StartUp = new StartUp(SimoEngine);

            bool setupSucceeded = StartUp.connectQs();
            if (setupSucceeded)
            {
                while (SimoEngine.bgwIrcReader.IsBusy)
                {
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }




        // I'll just leave this stuff here
        //          I
        //          I
        //          V

        /*
public void OnMessageEvent(object sender, MessageEventArgs e)
{
    WriteConsole(e.Message);
}

public void WriteConsole(string msg)
{
    Console.WriteLine(msg);
}
 */


        /*
        public void SetText(string msg)
        {
            // InvokeRequired is set to true if you are accessing the control from a different thread.
            // In that case, you need to invoke your function using a delegate.
            if (lsbMsgBox.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                lsbMsgBox.Invoke(d, new object[] { msg });
                // Basically this runs the same function again, but now the run order comes from the same thread, so InvokeRequired
                // is set to false, and it goes straight to the else below.
            }
            else
            {
                lsbMsgBox.Items.Add(msg); // Just add the text to the listbox.
                lsbMsgBox.TopIndex = lsbMsgBox.Items.Count - 1; // Scroll the listbox to the lowest item. ...Count -1 because indexes start from 0 and the number of items from 1.
            }
        }
         */
    }
}
