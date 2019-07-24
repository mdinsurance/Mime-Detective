using System;
using System.IO;
using System.Threading.Tasks;

namespace MimeDetective.Scratch
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var info = new FileInfo(@"X:\Tools\Test Documents\New link on The Learning Hub - iLearn Connect.msg");

            Console.WriteLine("When no hints");
            var result1 = await info.GetFileTypeAsync(null, null);
            Console.WriteLine($"{result1.Mime} - {result1.Extension}");

            Console.WriteLine("When extension hint");
            var result2 = await info.GetFileTypeAsync(null, "msg");
            Console.WriteLine($"{result2.Mime} - {result2.Extension}");

            Console.WriteLine("When mime type hint");
            var result3 = await info.GetFileTypeAsync("application/vnd.ms-outlook", null);
            Console.WriteLine($"{result3.Mime} - {result3.Extension}");

            Console.WriteLine("When both hints");
            var result4 = await info.GetFileTypeAsync("application/vnd.ms-outlook", "msg");
            Console.WriteLine($"{result4.Mime} - {result4.Extension}");

            Console.ReadKey();
        }
    }
}
