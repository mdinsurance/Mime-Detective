using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MimeDetective.Analyzers
{
    //this is somehow off by one in the insert...
    //on the single insert search test
    //record is inserted on the 3rd node, when it should be on teh second
    public sealed class LinearTrie : IFileAnalyzer
    {
        private const int NullStandInValue = 256;
        private const int MaxNodeSize = 257;

        private OffsetNode[] OffsetNodes = new OffsetNode[10];
        private int offsetNodesLength = 1;

        [StructLayout(LayoutKind.Auto)]
        private struct OffsetNode
        {
            public ushort Offset;
            public int TrieLength;
            public Node[] Trie;

            public OffsetNode(ushort offset)
            {
                Offset = offset;
                //this is the issue resizing this is dropping the reference in the insert and search algs
                Trie = new Node[64];
                TrieLength = 1;
            }
        }

        //if we do an offset here we could cut this in half
        [StructLayout(LayoutKind.Auto)]
        private unsafe struct Node
        {
            //if complete node then this not null
            public FileType Record;

            public fixed ushort Children[MaxNodeSize];
        }

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

            //iterate through offset nodes
            for (int offsetNodeIndex = 0; offsetNodeIndex < offsetNodesLength; offsetNodeIndex++)
            {
                ref OffsetNode offsetNode = ref OffsetNodes[offsetNodeIndex];
                ref Node node = ref offsetNode.Trie[0];
                int i = offsetNode.Offset;

                //todo currently loops longer than it should
                while (i < readResult.ReadLength)
                {
                    int arrayPos = readResult.Array[i];
                    int triePos = node.Children[arrayPos];

                    if (triePos <= 0)
                    {
                        triePos = node.Children[NullStandInValue];

                        if (triePos <= 0)
                            break;
                    }

                    node = ref offsetNode.Trie[triePos];
                    i++;

                    //collect the record
                    if (i > highestMatchingCount && !(node.Record is null))
                    {
                        match = node.Record;
                        highestMatchingCount = i;
                    }
                }
            }
            
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
                ref OffsetNode currentNode = ref OffsetNodes[offsetNodeIndex];

                if (currentNode.Offset == type.HeaderOffset)
                {
                    offsetNode = ref currentNode;
                    matchFound = true;
                    break;
                }
            }

            //handle adding new offsetNode and offsetNOde array resize
            if (!matchFound)
            {
                if (offsetNodesLength >= OffsetNodes.Length)
                {
                    //TODO put max size check
                    int newOffsetNodeCount = OffsetNodes.Length * 2;
                    var newOffsetNodes = new OffsetNode[newOffsetNodeCount];
                    Array.Copy(OffsetNodes, newOffsetNodes, offsetNodesLength);
                    OffsetNodes = newOffsetNodes;
                }

                offsetNode = ref OffsetNodes[offsetNodesLength];
                offsetNode = new OffsetNode(type.HeaderOffset);
                offsetNodesLength++;
            }

            //setup variables for walking the trie
            int i = 0;
            ref Node node = ref offsetNode.Trie[0];

            while (i < type.Header.Length)
            {
                int arrayPos = type.Header[i] ?? NullStandInValue;
                int triePos = node.Children[arrayPos];

                //insert new node, handle possible resize
                if (triePos <= 0)
                {
                    triePos = offsetNode.TrieLength;
                    node.Children[arrayPos] = (ushort)triePos;

                    if (offsetNode.TrieLength >= offsetNode.Trie.Length)
                    {
                        //TODO put max size check
                        int newTrieNodeCount = offsetNode.Trie.Length * 2;
                        var newTrieNodes = new Node[newTrieNodeCount];
                        Array.Copy(offsetNode.Trie, newTrieNodes, offsetNode.TrieLength);
                        offsetNode.Trie = newTrieNodes;
                    }

                    offsetNode.TrieLength++;
                }

                node = ref offsetNode.Trie[triePos];
                i++;
            }

            node.Record = type;
        }
    }
}