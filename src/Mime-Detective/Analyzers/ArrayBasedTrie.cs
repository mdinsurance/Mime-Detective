using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Runtime.InteropServices;

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

        //TODO need tests for highestmatching count behavior
        public FileType Search(in ReadResult readResult)
        {
            FileType match = null;
            int highestMatchingCount = 0;

            //iterate through offset nodes
            for (int offsetNodeIndex = 0; offsetNodeIndex < offsetNodesLength; offsetNodeIndex++)
            {
                OffsetNode offsetNode = OffsetNodes[offsetNodeIndex];
                int i = offsetNode.Offset;
                Node[] prevNode = offsetNode.Children;

                while (i < readResult.ReadLength)
                {
                    int currentVal = readResult.Array[i];
                    Node node = prevNode[currentVal];

                    if (node.Children == null)
                    {
                        node = prevNode[NullStandInValue];

                        if (node.Children is null)
                            break;
                    }

                    //increment here
                    i++;

                    //collect the record
                    if (i > highestMatchingCount && (object)node.Record != null)
                    {
                        match = node.Record;
                        highestMatchingCount = i;
                    }

                    prevNode = node.Children;
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

                if (newNodePos >= OffsetNodes.Length)
                {
                    int newOffsetNodeCount = OffsetNodes.Length * 2 + 1;
                    var newOffsetNodes = new OffsetNode[newOffsetNodeCount];
                    Array.Copy(OffsetNodes, newOffsetNodes, offsetNodesLength);
                    OffsetNodes = newOffsetNodes;
                }

                match = ref OffsetNodes[newNodePos];
                match = new OffsetNode(type.HeaderOffset);
                offsetNodesLength++;
            }

            Node[] prevNode = match.Children;

            for (int i = 0; i < type.Header.Length; i++)
            {
                byte? value = type.Header[i];
                int arrayPos = value ?? NullStandInValue;
                ref Node node = ref prevNode[arrayPos];

                if (node.Children is null)
                {
                    FileType record = null;

                    if (i == type.Header.Length - 1)
                        record = type;

                    node = new Node(record);
                }

                prevNode = node.Children;
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

        private struct Node
        {
            public Node[] Children;

            //if complete node then this not null
            public FileType Record;

            public Node(FileType record)
            {
                Children = new Node[MaxNodeSize];
                Record = record;
            }
        }
    }
}