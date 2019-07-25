using System;
using System.IO;
using System.Threading.Tasks;

namespace MimeDetective.Scratch
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var info = new FileInfo(@"C:\Program Files (x86)\Changemaker Studios\Papercut\Incoming\20190724171604131-500849.eml");

            Console.WriteLine("When no hints");
            var result1 = await info.GetFileTypeAsync(null, null);
            Console.WriteLine($"{result1.Mime} - {result1.Extension}");

            Console.WriteLine("When extension hint");
            var result2 = await info.GetFileTypeAsync(null, "eml");
            Console.WriteLine($"{result2.Mime} - {result2.Extension}");

            Console.WriteLine("When mime type hint");
            var result3 = await info.GetFileTypeAsync("message/rfc822", null);
            Console.WriteLine($"{result3.Mime} - {result3.Extension}");

            Console.WriteLine("When both hints");
            var result4 = await info.GetFileTypeAsync("message/rfc822", "eml");
            Console.WriteLine($"{result4.Mime} - {result4.Extension}");

            Console.ReadKey();
        }
    }
}
