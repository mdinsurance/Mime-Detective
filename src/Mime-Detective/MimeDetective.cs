using System.Collections.Generic;
using System.IO;

namespace MimeDetective
{
    public class MimeDetective
    {
        public static FileType LearnMimeType(FileInfo file, string mimeType, int headerSize, ushort offset = 0)
        {
            var data = new byte?[headerSize];

            using (var stream = file.OpenRead())
            {
                var b = 0;
                for (var i = 0; i < headerSize; i++)
                {
                    data[i] = (byte)((b = stream.ReadByte()) == -1 ? 0 : b);
                    if (b == -1)
                    {
                        break;
                    }
                }
            }

            return new FileType(data, file.Extension, mimeType, offset);
        }

        public static FileType LearnMimeType(FileInfo first, FileInfo second, string mimeType, int maxHeaderSize = 12, int minMatches = 2, int maxNonMatch = 3)
        {
            byte?[] header = null;

            var headerList = new List<byte?>();

            using (Stream firstFile = first.OpenRead())
            using (Stream secondFile = second.OpenRead())
            {
                var match = false;
                var missmatchCounter = 0;       // mismatches after first match

                int bFst = 0, bSnd = 0;         // current bytes
                var index = 0;
                ushort offset = 0;             // index of first match

                // Read from both files until one of the file streams reaches the end.
                while ((bFst = firstFile.ReadByte()) != -1 &&
                      (bSnd = secondFile.ReadByte()) != -1)
                {
                    bFst = firstFile.ReadByte();
                    bSnd = secondFile.ReadByte();

                    if (bFst == bSnd)
                    {
                        if (!match)
                        {
                            match = true;       // first match
                            offset = (ushort)index;
                        }

                        headerList.Add((byte)bFst);     // add match to header
                    }
                    else
                    {
                        if (match)
                        {      // if there was a match before
                               // no more matching

                            if (missmatchCounter < maxNonMatch)
                            {
                                headerList.Add(null);       // Add a null header, this could be non generic, file size for example
                                missmatchCounter++;
                            }
                            else
                            {
                                break;  // too much mismatches after the first match
                            }
                        }
                    }
                    if (headerList.Count == maxHeaderSize)
                    {
                        break;
                    }

                    index++;
                }

                uint count = 0;

                //FileType type = null;
                for (var i = 0; i < headerList.Count; i++)
                {
                    if (headerList[i] != null)
                    {
                        count++;
                    }
                }

                if (count >= minMatches)       // check for enough non null byte? ´s.
                {
                    header = headerList.ToArray();
                    return new FileType(header, first.Extension, mimeType, offset);
                }
                else
                {
                    return null;
                }
            }
        }
    }
}