﻿using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace MimeDetective.Analyzers
{
    public class ZipFileAnalyzer : IReadOnlyFileAnalyzer
    {
        public FileType Key { get; } = MimeTypes.ZIP;

        //todo if creating a memorysteam there should be a check for min size to avoid most exceptions
        //should this catch exceptions generated by passing in a zip header but no the whole file?
        /// <summary>
        /// 
        /// </summary>
        /// <param name="readResult"></param>
        /// <returns>Any resulting match or <see cref="MimeTypes.ZIP"/> for no match</returns>
        public FileType Search(in ReadResult readResult, string mimeHint = null, string extensionHint = null)
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
            {
                mStream = readResult.Source;
            }

            if (mStream.CanSeek && mStream.Position > 0)
                mStream.Seek(0, SeekOrigin.Begin);

            using (ZipArchive zipData = new ZipArchive(mStream, ZipArchiveMode.Read, leaveOpen: true))
            {
                //check for office xml formats
                FileType officeXml = CheckForOpenXMLDocument(zipData);

                if (officeXml != null)
                    return officeXml;

                //check for open office formats
                FileType openOffice = CheckForOpenDocument(zipData);

                if (openOffice != null)
                    return openOffice;
            }

            if (locallyCreatedStream)
                mStream.Dispose();

            if (!(mimeHint is null) || !(extensionHint is null))
            {
                return new FileType(MimeTypes.ZIP.Header, extensionHint ?? MimeTypes.ZIP.Extension, mimeHint ?? MimeTypes.ZIP.Mime, MimeTypes.ZIP.HeaderOffset);
            }

            return MimeTypes.ZIP;
        }

        public FileType CheckForOpenXMLDocument(ZipArchive zipData)
        {
            System.Collections.Generic.List<string> entryFullNames = zipData.Entries.Select(e => e.FullName).ToList();

            foreach (string entry in entryFullNames)
            {
                if (entry.StartsWith("word/"))
                {
                    using (Stream s = zipData.Entries.Single(x => x.FullName == "[Content_Types].xml").Open())
                    {
                        using (XmlReader reader = XmlReader.Create(s))
                        {
                            XDocument doc = XDocument.Load(reader);

                            string contentType = doc.Descendants().Where(d => d.Name.LocalName == "Override")
                                .Where(x => x.Attribute("PartName").Value == "/word/document.xml")
                                .Attributes("ContentType").Single().Value;

                            if (contentType == "application/vnd.ms-word.document.macroEnabled.main+xml")
                            {
                                return MimeTypes.WORDM;
                            }
                        }
                    }

                    return MimeTypes.WORDX;
                }
                else if (entry.StartsWith("xl/"))
                {
                    if (entryFullNames.Contains("xl/worksheets/binaryIndex1.bin"))
                    {
                        return MimeTypes.EXCELB;
                    }

                    using (Stream s = zipData.Entries.Single(x => x.FullName == "[Content_Types].xml").Open())
                    {
                        using (XmlReader reader = XmlReader.Create(s))
                        {
                            XDocument doc = XDocument.Load(reader);

                            string contentType = doc.Descendants().Where(d => d.Name.LocalName == "Override")
                                .Where(x => x.Attribute("PartName").Value == "/xl/workbook.xml")
                                .Attributes("ContentType").Single().Value;

                            if (contentType == "application/vnd.ms-excel.sheet.macroEnabled.main+xml")
                            {
                                return MimeTypes.EXCELM;
                            }
                        }
                    }

                    return MimeTypes.EXCELX;
                }
                else if (entry.StartsWith("ppt/"))
                {
                    return MimeTypes.PPTX;
                }
            }

            return null;
        }

        //check for open doc formats
        public FileType CheckForOpenDocument(ZipArchive zipFile)
        {
            ZipArchiveEntry ooMimeType = null;

            foreach (ZipArchiveEntry entry in zipFile.Entries)
            {
                if (entry.FullName == "mimetype")
                {
                    ooMimeType = entry;
                    break;
                }
            }

            if (ooMimeType is null)
                return null;

            using (StreamReader textReader = new StreamReader(ooMimeType.Open()))
            {
                string mimeType = textReader.ReadToEnd();

                if (mimeType == MimeTypes.ODT.Mime)
                    return MimeTypes.ODT;
                else if (mimeType == MimeTypes.ODS.Mime)
                    return MimeTypes.ODS;
                else if (mimeType == MimeTypes.ODP.Mime)
                    return MimeTypes.ODP;
                else if (mimeType == MimeTypes.ODG.Mime)
                    return MimeTypes.ODG;
                else
                    return null;
            }
        }
    }
}
