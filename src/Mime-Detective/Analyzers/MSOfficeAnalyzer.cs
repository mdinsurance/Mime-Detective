using System;
using System.Collections.Generic;
using System.Text;

namespace MimeDetective.Analyzers
{
    //TODO maybe turn this into an OLE Doc type analyzer
    public class MsOfficeAnalyzer : IReadOnlyFileAnalyzer
    {
        public FileType Key { get; } = MimeTypes.MS_OFFICE;

        public static FileType[] MsDocTypes { get; } = new FileType[] { MimeTypes.PPT, MimeTypes.WORD, MimeTypes.EXCEL, MimeTypes.MSG };

        private readonly DictionaryTrie dictTrie;

        public MsOfficeAnalyzer()
        {
            dictTrie = new DictionaryTrie(MsDocTypes);
        }

        public FileType Search(in ReadResult readResult)
        {
            return dictTrie.Search(in readResult) ?? Key;
        }
    }
}
