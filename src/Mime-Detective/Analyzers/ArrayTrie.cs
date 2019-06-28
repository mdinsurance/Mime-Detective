using System;
using System.Collections.Generic;

namespace MimeDetective.Analyzers
{
    public sealed class ArrayTrie : IFileAnalyzer
    {
        private const int NullStandInValue = 256;
        private const int MaxNodeSize = 257;

        private OffsetNode[] OffsetNodes = new OffsetNode[10];
        private int offsetNodesLength = 1;

        /// <summary>
        /// Constructs an empty ArrayBasedTrie, <see cref="Insert(FileType)"/> to add definitions
        /// </summary>
        public ArrayTrie()
        {
            this.OffsetNodes[0] = new OffsetNode(0);
        }

        /// <summary>
        /// Constructs an ArrayBasedTrie from an Enumerable of FileTypes, <see cref="Insert(FileType)"/> to add more definitions
        /// </summary>
        /// <param name="types"></param>
        public ArrayTrie(IEnumerable<FileType> types)
        {
            if (types is null)
            {
                ThrowHelpers.FileTypeEnumerableIsNull();
            }

            this.OffsetNodes[0] = new OffsetNode(0);

            foreach (var type in types)
            {
                if (!(type is null))
                {
                    this.Insert(type);
                }
            }
        }

        //TODO need tests for highestmatching count behavior
        public FileType Search(in ReadResult readResult, string mimeHint = null, string extensionHint = null)
        {
            FileType match = null;
            var highestMatchingCount = 0;

            //iterate through offset nodes
            for (var offsetNodeIndex = 0; offsetNodeIndex < this.offsetNodesLength; offsetNodeIndex++)
            {
                var offsetNode = this.OffsetNodes[offsetNodeIndex];
                int i = offsetNode.Offset;
                var prevNode = offsetNode.Children;

                while (i < readResult.ReadLength)
                {
                    int currentVal = readResult.Array[i];
                    var node = prevNode[currentVal];

                    if (node.Children is null)
                    {
                        node = prevNode[NullStandInValue];

                        if (node.Children is null)
                        {
                            break;
                        }
                    }

                    //increment here
                    i++;

                    //collect the record
                    if (i > highestMatchingCount && !(node.Record is null))
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
            {
                ThrowHelpers.FileTypeArgumentIsNull();
            }

            ref OffsetNode match = ref this.OffsetNodes[0];
            var matchFound = false;

            for (var offsetNodeIndex = 0; offsetNodeIndex < this.offsetNodesLength; offsetNodeIndex++)
            {
                ref OffsetNode currentNode = ref this.OffsetNodes[offsetNodeIndex];

                if (currentNode.Offset == type.HeaderOffset)
                {
                    match = ref currentNode;
                    matchFound = true;
                    break;
                }
            }

            if (!matchFound)
            {
                if (this.offsetNodesLength >= this.OffsetNodes.Length)
                {
                    //TODO put max size check
                    var newOffsetNodeCalc = this.OffsetNodes.Length * 2;
                    var newOffsetNodeCount = newOffsetNodeCalc > 560 ? 560 : newOffsetNodeCalc;
                    var newOffsetNodes = new OffsetNode[newOffsetNodeCount];
                    Array.Copy(this.OffsetNodes, newOffsetNodes, this.offsetNodesLength);
                    this.OffsetNodes = newOffsetNodes;
                }

                match = ref this.OffsetNodes[this.offsetNodesLength];
                match = new OffsetNode(type.HeaderOffset);
                this.offsetNodesLength++;
            }

            var prevNode = match.Children;

            for (var i = 0; i < type.Header.Length; i++)
            {
                var arrayPos = type.Header[i] ?? NullStandInValue;
                ref Node node = ref prevNode[arrayPos];

                //TODO maybe short circuit it
                if (i == type.Header.Length - 1)
                {
                    node.Record = type;
                }

                if (node.Children is null)
                {
                    node.Children = new Node[MaxNodeSize];
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
                this.Offset = offset;
                this.Children = new Node[MaxNodeSize];
            }
        }

        private struct Node
        {
            public Node[] Children;

            public FileType Record;
        }
    }
}