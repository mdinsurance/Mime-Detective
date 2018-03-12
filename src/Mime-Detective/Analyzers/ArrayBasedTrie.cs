using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace MimeDetective.Analyzers
{
    public sealed class ArrayBasedTrie : IFileAnalyzer
    {
        public const int NullStandInValue = 256;
        public const int MaxNodeSize = 257;

        private List<OffsetNode> Nodes = new List<OffsetNode>(10);

        /// <summary>
        /// Constructs an empty ArrayBasedTrie, <see cref="Insert(FileType)"/> to add definitions
        /// </summary>
        public ArrayBasedTrie()
        {
        }

        /// <summary>
        /// Constructs an ArrayBasedTrie from an Enumerable of FileTypes, <see cref="Insert(FileType)"/> to add more definitions
        /// </summary>
        /// <param name="types"></param>
        public ArrayBasedTrie(IEnumerable<FileType> types)
        {
            if (types is null)
                throw new ArgumentNullException(nameof(types));

            foreach (var type in types)
            {
                if ((object)type != null)
                    Insert(type);
            }

            Nodes = Nodes.OrderBy(x => x.Offset).ToList();
        }

        public FileType Search(in ReadResult readResult)
        {
            FileType match = null;

            //iterate through offset nodes
            for (int offsetNodeIndex = 0; offsetNodeIndex < Nodes.Count; offsetNodeIndex++)
            {
                //get offset node
                var offsetNode = Nodes[offsetNodeIndex];

                int i = offsetNode.Offset;
                byte value = readResult.Array[i];

                var node = offsetNode.Children[value];

                if (node is null)
                {
                    node = offsetNode.Children[NullStandInValue];

                    if (node is null)
                        break;
                }

                if ((object)node.Record != null)
                    match = node.Record;

                i++;

                //iterate through the current trie
                for (; i < readResult.ReadLength; i++)
                {
                    value = readResult.Array[i];

                    var prevNode = node;
                    node = node.Children[value];

                    if (node is null)
                    {
                        node = prevNode.Children[NullStandInValue];

                        if (node is null)
                            break;
                    }

                    if ((object)node.Record != null)
                        match = node.Record;
                }

                if ((object)match != null)
                    break;
            }
       
            return match;
        }

        public void Insert(FileType type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            OffsetNode match = null;

            foreach (var offsetNode in Nodes)
            {
                if (offsetNode.Offset == type.HeaderOffset)
                {
                    match = offsetNode;
                    break;
                }
            }

            if (match is null)
            {
                match = new OffsetNode(type.HeaderOffset);
                Nodes.Add(match);
            }

            match.Insert(type);
        }

        private sealed class OffsetNode
        {
            public readonly ushort Offset;
            public readonly Node[] Children;

            public OffsetNode(ushort offset)
            {
                if (offset > (MimeTypes.MaxHeaderSize - 1))
                    throw new ArgumentException("Offset cannot be greater than MaxHeaderSize - 1");

                Offset = offset;
                Children = new Node[MaxNodeSize];
            }

            public void Insert(FileType type)
            {
                int i = 0;
                byte? value = type.Header[i];
                int arrayPos = value ?? NullStandInValue;

                var node = Children[arrayPos];

                if (node is null)
                {
                    node = new Node(value);
                    Children[arrayPos] = node;
                }

                i++;

                for (; i < type.Header.Length; i++)
                {
                    value = type.Header[i];
                    arrayPos = value ?? NullStandInValue;
                    var prevNode = node;
                    node = node.Children[arrayPos];

                    if (node is null)
                    {
                        var newNode = new Node(value);

                        if (i == type.Header.Length - 1)
                            newNode.Record = type;

                        node = prevNode.Children[arrayPos] = newNode;
                    }
                }
            }
        }

        private sealed class Node
        {
            public readonly Node[] Children;

            //if complete node then this not null
            public FileType Record;

            public readonly byte? Value;

            public Node(byte? value)
            {
                Value = value;
                Children = new Node[MaxNodeSize];
                Record = null;
            }
        }
    }
}