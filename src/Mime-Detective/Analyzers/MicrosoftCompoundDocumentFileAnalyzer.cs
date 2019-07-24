using OpenMcdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MimeDetective.Analyzers
{
    public class MicrosoftCompoundDocumentFileAnalyzer : IReadOnlyFileAnalyzer
    {
        public FileType Key { get; } = MimeTypes.MS_OFFICE;

        public static FileType[] CompoundDocumentTypes { get; } = new FileType[] { MimeTypes.PPT, MimeTypes.WORD, MimeTypes.EXCEL, MimeTypes.MSG };

        private readonly DictionaryTrie dictTrie;

        public MicrosoftCompoundDocumentFileAnalyzer()
        {
            this.dictTrie = new DictionaryTrie(CompoundDocumentTypes);
        }

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

            if (locallyCreatedStream)
            {
                mStream.Dispose();
            }

            using (var cf = new CompoundFile(mStream, CFSUpdateMode.ReadOnly, CFSConfiguration.NoValidationException))
            {

                if (!(cf.RootStorage.TryGetStream("WordDocument") is null))
                {
                    return MimeTypes.WORD;
                }

                if (!(cf.RootStorage.TryGetStream("Workbook") is null))
                {
                    return MimeTypes.EXCEL;
                }

                if (!(cf.RootStorage.TryGetStream("Book") is null))
                {
                    return MimeTypes.EXCEL;
                }

                if (!(cf.RootStorage.TryGetStream("Powerpoint Document") is null))
                {
                    return MimeTypes.PPT;
                }

                if (!(cf.RootStorage.TryGetStream("__properties_version1.0") is null))
                {
                    return MimeTypes.MSG;
                }
            }

            if (string.IsNullOrWhiteSpace(mimeHint) == false)
            {
                var mimeMatch = CompoundDocumentTypes.SingleOrDefault(m => m.Mime == mimeHint);
                if (!(mimeHint is null))
                {
                    return mimeMatch;
                }
            }

            if (string.IsNullOrWhiteSpace(extensionHint) == false)
            {
                extensionHint = extensionHint.TrimStart('.');
                var extensionMatch = CompoundDocumentTypes.SingleOrDefault(m => m.Extension.Split(',').Contains(extensionHint));
                if (!(extensionMatch is null))
                {
                    return extensionMatch;
                }
            }

            if (!(mimeHint is null) || !(extensionHint is null))
            {
                return new FileType(MimeTypes.MS_OFFICE.Header, extensionHint ?? MimeTypes.MS_OFFICE.Extension, mimeHint ?? MimeTypes.MS_OFFICE.Mime, MimeTypes.ZIP.HeaderOffset);
            }

            return MimeTypes.MS_OFFICE;
        }
    }
}
