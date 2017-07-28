using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TIMpal_to_JASC
{
    class Program
    {
        static bool debug = false;
        static void Main(string[] args)
        {
            if(debug)
            {
                TIM tim = new TIM(@"D:/window.tim");
                string[] retme = tim.ConvertToPS;
                System.IO.File.WriteAllLines(@"D:/window.txt", retme);
                Console.WriteLine("Converted!");
            }
            if(args.Length !=2)
            {
                Console.WriteLine("TIM to Paint Shop Pro");
                Console.WriteLine("Usage:\nTIMpal_PSP.exe file.tim out.psp\nInput: file.tim - specify 8 BPP or 4 BPP TIM file\tOutput file: out.txt specify output file\n");
                return;
            }
            if(args.Length == 2)
            {
                TIM tim = new TIM(args[0]);
                string[] retme = tim.ConvertToPS;
                System.IO.File.WriteAllLines(args[1], retme);
                Console.WriteLine("Converted!");
            }
        }
    }
}
