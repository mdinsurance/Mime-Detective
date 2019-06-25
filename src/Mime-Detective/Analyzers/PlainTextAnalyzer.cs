using System.IO;
using System.Linq;

namespace MimeDetective.Analyzers
{
    public class PlainTextAnalyzer : IReadOnlyFileAnalyzer
    {
        public FileType Key { get; } = MimeTypes.UNKNOWN;

        public static FileType[] PlainTextTypes { get; } = new FileType[] { MimeTypes.TXT, MimeTypes.CSV };

        public FileType Search(in ReadResult readResult)
        {
            bool locallyCreatedStream = false;
            Stream mStream = null;

            if (readResult.Source is null)
            {
                //this should be the length of the array passed via ReadResult
                mStream = new MemoryStream(readResult.Array, 0, readResult.Array.Length);
                locallyCreatedStream = true;
            }
            else
                mStream = readResult.Source;

            if (mStream.CanSeek && mStream.Position > 0)
                mStream.Seek(0, SeekOrigin.Begin);

            using (var reader = new StreamReader(mStream, System.Text.Encoding.UTF8))
            {
                int commaCount = 0;
                var line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                {
                    return MimeTypes.TXT;
                }

                do
                {
                    var count = line.Count(x => char.Equals(x, ','));
                    if (count == 0)
                    {
                        return MimeTypes.TXT;
                    }
                    if (commaCount == 0)
                    {
                        commaCount = count;
                    }
                    if (commaCount != count)
                    {
                        return MimeTypes.TXT;
                    }
                    line = reader.ReadLine();
                } while (string.IsNullOrWhiteSpace(line) == false);
            }

            if (locallyCreatedStream)
                mStream.Dispose();

            return MimeTypes.CSV;
        }
    }
}
