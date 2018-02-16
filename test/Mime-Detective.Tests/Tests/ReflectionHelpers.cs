using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MimeDetective.Tests
{
    public static class ReflectionHelpers
    {
        public static IEnumerable<FileType> GetAllTypeValues()
        {
            var mimeTypes = typeof(MimeTypes);
            var fields = mimeTypes.GetFields();
            var filteredFields = fields.Where(x => x.FieldType == typeof(FileType));

            return filteredFields.Select(x => (FileType)x.GetValue(null));
        }

        public static IEnumerable<FieldInfo> GetAllTypeFields()
        {
            var mimeTypes = typeof(MimeTypes);
            var fields = mimeTypes.GetFields();
            return fields.Where(x => x.FieldType == typeof(FileType));
        }
    }
}
