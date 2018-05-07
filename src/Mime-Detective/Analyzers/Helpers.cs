using System;
using System.Collections.Generic;
using System.Text;

namespace MimeDetective.Analyzers
{
    internal static class ThrowHelpers
    {
        public static void GreaterThanMaxHeaderSize()
        {
            throw new ArgumentException("Offset cannot be greater than MaxHeaderSize - 1");
        }
    }
}
