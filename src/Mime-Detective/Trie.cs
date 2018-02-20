using System;
using System.Collections.Generic;
using System.Text;

namespace MimeDetective
{
    public sealed class Trie
    {
        public const ushort NullStandInValue = 256;
        public Dictionary<ushort, OffsetNode> Nodes { get; } = new Dictionary<ushort, OffsetNode>();

        public Trie(IEnumerable<FileType> types)
        {
            foreach (var type in types)
            {
                Insert(type);
            }
        }

        public FileType Search(in ReadResult readResult)
        {
            FileType match = null;
            var enumerator = Nodes.GetEnumerator();

            while (match is null && enumerator.MoveNext())
                match = enumerator.Current.Value.Search(in readResult);

            enumerator.Dispose();

            return match;
        }

        public void Insert(FileType type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            if (!Nodes.TryGetValue(type.HeaderOffset, out var offsetNode))
            {
                offsetNode = new OffsetNode(type.HeaderOffset);
                Nodes.Add(type.HeaderOffset, offsetNode);
            }

            offsetNode.Insert(type);
        }
    }

    //Node used to present offsets
    public sealed class OffsetNode
    {
        public ushort Offset { get; }

        public Dictionary<ushort, Node> Children { get; } = new Dictionary<ushort, Node>();

        public OffsetNode(ushort offsetValue)
        {
            Offset = offsetValue;
        }

        //get start at offset
        public FileType Search(in ReadResult readResult)
        {
            FileType match = null;
            int i = Offset;

            if (!Children.TryGetValue(readResult.Array[i], out Node node) && !Children.TryGetValue(Trie.NullStandInValue, out node))
                    return null;

            if (node.IsEndOfRecord)
                match = node.Record;

            i++;

            for (; i < readResult.ReadLength && i < MimeTypes.MaxHeaderSize; i++)
            {
                Node prevNode = node;

                if(!prevNode.Children.TryGetValue(readResult.Array[i], out node) && !prevNode.Children.TryGetValue(Trie.NullStandInValue, out node))
                        break;

                if (node.IsEndOfRecord)
                    match = node.Record;
            }

            return match;
        }

        public void Insert(FileType type)
        {
            int i = 0;
            ushort value = type.Header[i] ?? Trie.NullStandInValue;

            if (!Children.TryGetValue(value, out Node node))
            {
                node = new Node(value);
                Children.Add(value, node);
            }

            i++;

            for (; i < type.Header.Length; i++)
            {
                value = type.Header[i] ?? Trie.NullStandInValue;

                if (!node.Children.ContainsKey(value))
                {
                    Node newNode = new Node(value);
                    node.Children.Add(value, newNode);
                }

                node = node.Children[value];
            }

            node.Record = type;
        }
    }

    public sealed class Node
    {
        public Dictionary<ushort, Node> Children { get; } = new Dictionary<ushort, Node>();

        //if complete node then this not null
        public FileType Record { get; set; }

        public ushort Value { get; }

        public Node(ushort value)
        {
            Value = value;
        }

        public bool IsEndOfRecord { get => (object)Record != null; }
    }
}
