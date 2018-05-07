using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace MimeDetective.Analyzers
{
    public sealed class ArrayBasedTrie : IFileAnalyzer
    {
        private const int NullStandInValue = 256;
        private const int MaxNodeSize = 257;

        private OffsetNode[] OffsetNodes = new OffsetNode[10];
        private int offsetNodesLength = 1;

        /// <summary>
        /// Constructs an empty ArrayBasedTrie, <see cref="Insert(FileType)"/> to add definitions
        /// </summary>
        public ArrayBasedTrie()
        {
            OffsetNodes[0] = new OffsetNode(0);
        }

        /// <summary>
        /// Constructs an ArrayBasedTrie from an Enumerable of FileTypes, <see cref="Insert(FileType)"/> to add more definitions
        /// </summary>
        /// <param name="types"></param>
        public ArrayBasedTrie(IEnumerable<FileType> types)
        {
            if (types is null)
                throw new ArgumentNullException(nameof(types));

            OffsetNodes[0] = new OffsetNode(0);

            foreach (var type in types)
            {
                if ((object)type != null)
                    Insert(type);
            }
        }

        public FileType Search(in ReadResult readResult)
        {
            FileType match = null;

            //iterate through offset nodes
            for (int offsetNodeIndex = 0; offsetNodeIndex < offsetNodesLength && match is null; offsetNodeIndex++)
            {
                //get offset node
                var offsetNode = OffsetNodes[offsetNodeIndex];
                int i = offsetNode.Offset;
                Node[] prevNode = offsetNode.Children;
                Node node = offsetNode.Children[readResult.Array[i]];

                //iterate through the current trie
                for (i++; i < readResult.ReadLength; i++)
                {
                    if (node is null)
                    {
                        node = prevNode[NullStandInValue];

                        if (node is null)
                            break;
                    }

                    //collect the record
                    if ((object)node.Record != null)
                        match = node.Record;

                    prevNode = node.Children;
                    node = node.Children[readResult.Array[i]];
                }
            }

            return match;
        }

        public void Insert(FileType type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            ref OffsetNode match = ref OffsetNodes[0];
            bool matchFound = false;

            for (int offsetNodeIndex = 0; offsetNodeIndex < offsetNodesLength; offsetNodeIndex++)
            {
                ref var currentNode = ref OffsetNodes[offsetNodeIndex];

                if (currentNode.Offset == type.HeaderOffset)
                {
                    match = ref currentNode;
                    matchFound = true;
                    break;
                }
            }

            if (!matchFound)
            {
                int newNodePos = offsetNodesLength;

                if (newNodePos > OffsetNodes.Length)
                {
                    int newOffsetNodeCount = OffsetNodes.Length * 2 + 1;
                    var newOffsetNodes = new OffsetNode[newOffsetNodeCount];
                    Array.Copy(OffsetNodes, newOffsetNodes, newOffsetNodeCount);
                    OffsetNodes = newOffsetNodes;
                }

                match = ref OffsetNodes[newNodePos];
                match = new OffsetNode(type.HeaderOffset);
                offsetNodesLength++;
            }


            int i = 0;
            byte? value = type.Header[i];
            int arrayPos = value ?? NullStandInValue;

            var node = match.Children[arrayPos];

            if (node is null)
            {
                node = new Node();
                match.Children[arrayPos] = node;
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
                    node = new Node();

                    if (i == type.Header.Length - 1)
                        node.Record = type;

                    prevNode.Children[arrayPos] = node;
                }
            }
        }

        private readonly struct OffsetNode
        {
            public readonly ushort Offset;
            public readonly Node[] Children;

            public OffsetNode(ushort offset)
            {
                Offset = offset;
                Children = new Node[MaxNodeSize];
            }
        }

        private sealed class Node
        {
            public Node[] Children;

            //if complete node then this not null
            public FileType Record;

            //public byte? Value;

            public Node()
            {
                //Value = value;
                Children = new Node[MaxNodeSize];
                Record = null;
            }
        }
    }
}