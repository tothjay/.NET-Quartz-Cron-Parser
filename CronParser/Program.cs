using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CronParserLib;

namespace CronParser
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser cronParser = new Parser();
            bool isContinue=false;
            do
            {
                Console.WriteLine("Please input cron:");
                string cron=Console.ReadLine();
                try
                {
                    Console.WriteLine(cronParser.CronToChineseParser(cron));
                    
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                Console.Write("Continue?");
                if (Console.ReadLine().ToLower() == "y")
                    isContinue = true;
                else
                    isContinue = false;

            } while (isContinue);
        }
    }
}
