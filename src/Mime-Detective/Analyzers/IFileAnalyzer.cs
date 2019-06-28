namespace MimeDetective.Analyzers
{
    public interface IReadOnlyFileAnalyzer
    {
        FileType Search(in ReadResult readResult, string mimeHint = null, string extensionHint = null);
    }

    public interface IFileAnalyzer : IReadOnlyFileAnalyzer
    {
        void Insert(FileType fileType);
    }
}
