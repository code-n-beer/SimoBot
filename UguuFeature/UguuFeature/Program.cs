using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimoBot;

namespace UguuFeature
{

    class Program
    {
        static void Main(string[] args)
        {
            MockSimo simo = new MockSimo();
            UguuFeature uguu = new UguuFeature();

            uguu.Init(simo);

            Console.WriteLine("!uguu:");
            simo.ExecuteCommand("!uguu");
            Console.WriteLine("!uguu Tsarpf:");
            simo.ExecuteCommand("!uguu Tsarpf");

            Console.ReadKey();
        }
    }
}
