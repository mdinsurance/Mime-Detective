using System.Collections.Generic;

namespace MimeDetective.Analyzers
{
    public sealed class DictionaryTrie : IFileAnalyzer
    {
        private const ushort NullStandInValue = 256;

        //root dictionary contains the nodes with offset values
        private Dictionary<ushort, Node> Nodes { get; } = new Dictionary<ushort, Node>();

        /// <summary>
        /// Constructs an empty DictionaryBasedTrie
        /// </summary>
        public DictionaryTrie()
        {

        }

        /// <summary>
        /// Constructs a DictionaryBasedTrie from an Enumerable of FileTypes
        /// </summary>
        /// <param name="types"></param>
        public DictionaryTrie(IEnumerable<FileType> types)
        {
            if (types is null)
            {
                ThrowHelpers.FileTypeEnumerableIsNull();
            }

            foreach (var type in types)
            {
                if (!(type is null))
                {
                    this.Insert(type);
                }
            }
        }

        public FileType Search(in ReadResult readResult, string mimeHint = null, string extensionHint = null)
        {
            FileType match = null;
            var enumerator = this.Nodes.GetEnumerator();
            var highestMatchingCount = 0;

            while (enumerator.MoveNext())
            {
                var node = enumerator.Current.Value;
                int i = node.Value;

                while (i < readResult.ReadLength)
                {
                    var prevNode = node;

                    if (!prevNode.Children.TryGetValue(readResult.Array[i], out node)
                        && !prevNode.Children.TryGetValue(NullStandInValue, out node))
                    {
                        break;
                    }

                    i++;

                    if (i > highestMatchingCount && !(node.Record is null))
                    {
                        match = node.Record;
                        highestMatchingCount = i;
                    }
                }
            }

            if (match == null && (readResult.ReadLength < 1 || readResult.Array[0] < 31))
            {
                return null;
            }

            return match ?? MimeTypes.UNKNOWN;
        }

        public void Insert(FileType type)
        {
            if (type is null)
            {
                ThrowHelpers.FileTypeArgumentIsNull();
            }

            if (!this.Nodes.TryGetValue(type.HeaderOffset, out var offsetNode))
            {
                offsetNode = new Node(type.HeaderOffset);
                this.Nodes.Add(type.HeaderOffset, offsetNode);
            }

            if (type.Header.Length == 0)
            {
                return;
            }

            var i = 0;
            var value = type.Header[i] ?? NullStandInValue;

            if (!offsetNode.Children.TryGetValue(value, out var node))
            {
                node = new Node(value);
                offsetNode.Children.Add(value, node);
            }

            i++;

            for (; i < type.Header.Length; i++)
            {
                value = type.Header[i] ?? NullStandInValue;

                if (!node.Children.ContainsKey(value))
                {
                    var newNode = new Node(value);
                    node.Children.Add(value, newNode);
                }

                node = node.Children[value];
            }

            node.Record = type;
        }

        private sealed class Node
        {
            public Dictionary<ushort, Node> Children = new Dictionary<ushort, Node>();

            //if complete node then this not null
            public FileType Record;

            public ushort Value;

            public Node(ushort value)
            {
                this.Value = value;
            }
        }
    }
}

