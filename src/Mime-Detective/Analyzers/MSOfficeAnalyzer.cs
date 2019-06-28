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
            this.dictTrie = new DictionaryTrie(MsDocTypes);
        }

        public FileType Search(in ReadResult readResult, string mimeHint = null, string extensionHint = null)
        {
            var result = this.dictTrie.Search(in readResult);
            if (result == null || result == MimeTypes.UNKNOWN)
            {
                return MimeTypes.MS_OFFICE;
            }

            return result;
        }
    }
}
