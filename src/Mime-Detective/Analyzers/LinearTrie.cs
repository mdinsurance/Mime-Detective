using System;
using System.Collections.Generic;

namespace MimeDetective.Analyzers
{
    public sealed class LinearTrie : IFileAnalyzer
    {
        private const int NullStandInValue = 256;
        private const int MaxNodeSize = 257;

        private OffsetNode[] OffsetNodes = new OffsetNode[10];
        private int offsetNodesLength = 1;

        /// <summary>
        /// Constructs an empty ArrayBasedTrie, <see cref="Insert(FileType)"/> to add definitions
        /// </summary>
        public LinearTrie()
        {
            OffsetNodes[0] = new OffsetNode(0);
        }

        /// <summary>
        /// Constructs an ArrayBasedTrie from an Enumerable of FileTypes, <see cref="Insert(FileType)"/> to add more definitions
        /// </summary>
        /// <param name="types"></param>
        public LinearTrie(IEnumerable<FileType> types)
        {
            if (types is null)
                ThrowHelpers.FileTypeEnumerableIsNull();

            OffsetNodes[0] = new OffsetNode(0);

            foreach (var type in types)
            {
                if ((object)type != null)
                    Insert(type);
            }
        }

        //TODO need tests for highestmatching count behavior
        public unsafe FileType Search(in ReadResult readResult)
        {
            FileType match = null;
            int highestMatchingCount = 0;

            /*
            //iterate through offset nodes
            for (int offsetNodeIndex = 0; offsetNodeIndex < offsetNodesLength; offsetNodeIndex++)
            {
                ref OffsetNode offsetNode = ref OffsetNodes[offsetNodeIndex];
                int i = offsetNode.Offset;
                int triePos = offsetNode.Children[i] - 1;

           

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
            */
            return match;
        }

        public unsafe void Insert(FileType type)
        {
            if (type is null)
                ThrowHelpers.FileTypeArgumentIsNull();

            ref OffsetNode offsetNode = ref OffsetNodes[0];
            bool matchFound = false;

            for (int offsetNodeIndex = 0; offsetNodeIndex < offsetNodesLength; offsetNodeIndex++)
            {
                ref var currentNode = ref OffsetNodes[offsetNodeIndex];

                if (currentNode.Offset == type.HeaderOffset)
                {
                    offsetNode = ref currentNode;
                    matchFound = true;
                    break;
                }
            }

            //handle offset array resize
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

                offsetNode = ref OffsetNodes[newNodePos];
                offsetNode = new OffsetNode(type.HeaderOffset);
                offsetNodesLength++;
            }

            int i = 0;
            Node[] trie = offsetNode.Trie;
            ref Node node = ref trie[0];
            int childArrayPos = type.Header[i] ?? NullStandInValue;
            int triePos = offsetNode.Children[childArrayPos] - 1;

            for (; i < type.Header.Length;)
            {
                //insert new node, handle possible resize
                if (triePos < 0)
                {
                    int newTriePos = offsetNode.TrieLength;
                    if (newTriePos >= OffsetNodes.Length)
                    {
                        int newTrieNodeCount = offsetNode.Trie.Length * 2 + 1;
                        var newTrieNodes = new Node[newTrieNodeCount];
                        Array.Copy(trie, newTrieNodes, newTriePos);
                        offsetNode.Trie = trie = newTrieNodes;
                    }

                    node = ref trie[newTriePos];
                    offsetNode.Children[childArrayPos] = newTriePos;
                    offsetNode.TrieLength++;
                }
                else
                {
                    node = ref trie[triePos];
                }

                i++;

                childArrayPos = type.Header[i] ?? NullStandInValue;
                triePos = node.Children[childArrayPos] - 1;
            }

            node.Record = type;
        }

        private unsafe struct OffsetNode
        {
            public ushort Offset;
            public int TrieLength;
            public Node[] Trie;
            public fixed int Children[257];

            public OffsetNode(ushort offset)
            {
                Offset = offset;
                Trie = new Node[MaxNodeSize];
                TrieLength = 0;
            }
        }

        private unsafe struct Node
        {
            //if complete node then this not null
            public FileType Record;

            public fixed int Children[257];
        }
    }
}