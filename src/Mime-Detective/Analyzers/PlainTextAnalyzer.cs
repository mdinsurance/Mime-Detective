using System.IO;
using System.Linq;

namespace MimeDetective.Analyzers
{
    public class PlainTextAnalyzer : IReadOnlyFileAnalyzer
    {
        public FileType Key { get; } = MimeTypes.UNKNOWN;

        public static FileType[] PlainTextTypes { get; } = new FileType[] { MimeTypes.TXT, MimeTypes.CSV };

        public FileType Search(in ReadResult readResult, string mimeHint = null, string extensionHint = null)
        {
            var locallyCreatedStream = false;
            Stream mStream = null;

            if (readResult.Source is null)
            {
                //this should be the length of the array passed via ReadResult
                mStream = new MemoryStream(readResult.Array, 0, readResult.Array.Length);
                locallyCreatedStream = true;
            }
            else
            {
                mStream = readResult.Source;
            }

            if (mStream.CanSeek && mStream.Position > 0)
            {
                mStream.Seek(0, SeekOrigin.Begin);
            }

            var isNotCsv = false;
            using (var streamCopy = new MemoryStream())
            {
                mStream.CopyTo(streamCopy);
                mStream.Seek(0, SeekOrigin.Begin);
                streamCopy.Seek(0, SeekOrigin.Begin);
                using (var reader = new StreamReader(streamCopy, System.Text.Encoding.UTF8))
                {
                    var commaCount = 0;
                    var line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        isNotCsv = true;
                    }

                    if (isNotCsv == false)
                    {
                        do
                        {
                            var count = line.Count(x => char.Equals(x, ','));
                            if (count == 0)
                            {
                                isNotCsv = true;
                                break;
                            }
                            if (commaCount == 0)
                            {
                                commaCount = count;
                            }
                            if (commaCount != count)
                            {
                                isNotCsv = true;
                                break;
                            }
                            line = reader.ReadLine();
                        } while (string.IsNullOrWhiteSpace(line) == false && isNotCsv == false);
                    }
                }
            }

            if (locallyCreatedStream)
            {
                mStream.Dispose();
            }

            if (isNotCsv)
            {
                if (!(mimeHint is null) || !(extensionHint is null))
                {
                    return new FileType(MimeTypes.TXT.Header, extensionHint ?? MimeTypes.TXT.Extension, mimeHint ?? MimeTypes.TXT.Mime, MimeTypes.TXT.HeaderOffset);
                }

                return MimeTypes.TXT;
            }

            return MimeTypes.CSV;
        }
    }
}
