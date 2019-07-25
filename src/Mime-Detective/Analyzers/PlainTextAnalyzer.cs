using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MimeDetective.Analyzers
{
    public class PlainTextAnalyzer : IReadOnlyFileAnalyzer
    {
        public FileType Key { get; } = MimeTypes.UNKNOWN;

        public static FileType[] PlainTextTypes { get; } = new FileType[] { MimeTypes.TXT, MimeTypes.CSV, MimeTypes.HTML };

        public FileType Search(in ReadResult readResult, string mimeHint = null, string extensionHint = null)
        {
            using (var stream = readResult.GetStream())
            {
                var lines = this.GetLines(stream, 5);

                if (DetectHtml(lines))
                {
                    return MimeTypes.HTML;
                }

                if (DetectCsv(lines))
                {
                    return MimeTypes.CSV;
                }

                if (!(mimeHint is null) || !(extensionHint is null))
                {
                    return new FileType(MimeTypes.TXT.Header, extensionHint ?? MimeTypes.TXT.Extension, mimeHint ?? MimeTypes.TXT.Mime, MimeTypes.TXT.HeaderOffset);
                }

                return MimeTypes.TXT;
            }
        }

        private IEnumerable<string> GetLines(Stream stream, int lines, bool skipBlankHeader = true)
        {
            return InnerGetLines(stream, lines, skipBlankHeader).ToList();

            IEnumerable<string> InnerGetLines(Stream internalStream, int internalLines, bool internalSkipBlankHeader)
            {
                internalStream.Position = 0;
                using (var reader = new StreamReader(internalStream, System.Text.Encoding.UTF8, true, 1024, true))
                {
                    var line = reader.ReadLine();
                    if (!(line is null))
                    {
                        if (internalSkipBlankHeader)
                        {
                            while (string.IsNullOrWhiteSpace(line) && !(line is null))
                            {
                                line = reader.ReadLine();
                            }

                            if (string.IsNullOrWhiteSpace(line) == false)
                            {
                                yield return line;
                            }
                        }
                        else
                        {
                            if (!(line is null))
                            {
                                yield return line;
                            }
                        }

                        for (int i = 1; i < internalLines; i++)
                        {
                            line = reader.ReadLine();
                            if (line is null)
                            {
                                break;
                            }

                            yield return line;
                        }
                    }
                }
            }
        }

        private bool DetectHtml(IEnumerable<string> lines)
        {
            var firstLine = lines.FirstOrDefault()?.TrimStart();

            if (firstLine is null)
            {
                return false;
            }

            return firstLine.StartsWith("<html", StringComparison.OrdinalIgnoreCase)
                    || firstLine.StartsWith("<!DOCTYPE html", StringComparison.OrdinalIgnoreCase);
        }

        private static bool DetectCsv(IEnumerable<string> lines)
        {
            if (lines.Count() < 2)
            {
                return false;
            }

            //TODO: check for commas within quotes

            var commaCount = -1;
            foreach (var l in lines)
            {
                var newCount = l.Count(x => char.Equals(x, ','));
                if (newCount == 0)
                {
                    return false;
                }
                if (commaCount == -1)
                {
                    commaCount = newCount;
                }
                else if (commaCount != newCount)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
