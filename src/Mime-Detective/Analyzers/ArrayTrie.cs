﻿using System;
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
            OffsetNodes[0] = new OffsetNode(0);
        }

        /// <summary>
        /// Constructs an ArrayBasedTrie from an Enumerable of FileTypes, <see cref="Insert(FileType)"/> to add more definitions
        /// </summary>
        /// <param name="types"></param>
        public ArrayTrie(IEnumerable<FileType> types)
        {
            if (types is null)
                ThrowHelpers.FileTypeEnumerableIsNull();

            OffsetNodes[0] = new OffsetNode(0);

            foreach (FileType type in types)
            {
                if (!(type is null))
                    Insert(type);
            }
        }

        //TODO need tests for highestmatching count behavior
        public FileType Search(in ReadResult readResult, string mimeHint = null, string extensionHint = null)
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
                ThrowHelpers.FileTypeArgumentIsNull();

            ref OffsetNode match = ref OffsetNodes[0];
            bool matchFound = false;

            for (int offsetNodeIndex = 0; offsetNodeIndex < offsetNodesLength; offsetNodeIndex++)
            {
                ref OffsetNode currentNode = ref OffsetNodes[offsetNodeIndex];

                if (currentNode.Offset == type.HeaderOffset)
                {
                    match = ref currentNode;
                    matchFound = true;
                    break;
                }
            }

            if (!matchFound)
            {
                if (offsetNodesLength >= OffsetNodes.Length)
                {
                    //TODO put max size check
                    int newOffsetNodeCalc = OffsetNodes.Length * 2;
                    int newOffsetNodeCount = newOffsetNodeCalc > 560 ? 560 : newOffsetNodeCalc;
                    OffsetNode[] newOffsetNodes = new OffsetNode[newOffsetNodeCount];
                    Array.Copy(OffsetNodes, newOffsetNodes, offsetNodesLength);
                    OffsetNodes = newOffsetNodes;
                }

                match = ref OffsetNodes[offsetNodesLength];
                match = new OffsetNode(type.HeaderOffset);
                offsetNodesLength++;
            }

            Node[] prevNode = match.Children;

            for (int i = 0; i < type.Header.Length; i++)
            {
                int arrayPos = type.Header[i] ?? NullStandInValue;
                ref Node node = ref prevNode[arrayPos];

                //TODO maybe short circuit it
                if (i == type.Header.Length - 1)
                    node.Record = type;

                if (node.Children is null)
                    node.Children = new Node[MaxNodeSize];

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

            public FileType Record;
        }
    }
}