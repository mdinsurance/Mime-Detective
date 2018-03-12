using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;

namespace MimeDetective.Analyzers
{
    public interface IReadOnlyFileAnalyzer
    {
        FileType Search(in ReadResult readResult);
    }

    public interface IFileAnalyzer : IReadOnlyFileAnalyzer
    {
        void Insert(FileType fileType);
    }
}
