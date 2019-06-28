using System;
using System.Collections.Generic;

namespace MimeDetective.Analyzers
{
    public sealed class HybridTrie : IFileAnalyzer
    {
        private const int DefaultSize = 7;
        private const ushort NullStandInValue = 256;
        private const int MaxNodeSize = 257;

        private OffsetNode[] OffsetNodes = new OffsetNode[10];
        private int offsetNodesLength = 1;

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

        /// <summary>
        /// Constructs an empty DictionaryBasedTrie
        /// </summary>
        public HybridTrie()
        {
            OffsetNodes[0] = new OffsetNode(0);
        }

        /// <summary>
        /// Constructs a DictionaryBasedTrie from an Enumerable of FileTypes
        /// </summary>
        /// <param name="types"></param>
        public HybridTrie(IEnumerable<FileType> types)
        {
            if (types is null)
                ThrowHelpers.FileTypeArgumentIsNull();

            OffsetNodes[0] = new OffsetNode(0);

            foreach (FileType type in types)
            {
                if (!(type is null))
                    Insert(type);
            }
        }

        public FileType Search(in ReadResult readResult, string mimeHint = null, string extensionHint = null)
        {
            FileType match = null;
            int highestMatchingCount = 0;

            //iterate through offset nodes
            for (int offsetNodeIndex = 0; offsetNodeIndex < offsetNodesLength; offsetNodeIndex++)
            {
                //get offset node
                OffsetNode offsetNode = OffsetNodes[offsetNodeIndex];
                int i = offsetNode.Offset;

                if (!(i < readResult.ReadLength))
                    continue;

                Node node = offsetNode.Children[readResult.Array[i]];

                if (node == null)
                {
                    node = offsetNode.Children[NullStandInValue];

                    if (node is null)
                        continue;
                }

                i++;

                if (i > highestMatchingCount && !(node.Record is null))
                {
                    match = node.Record;
                    highestMatchingCount = i;
                }

                while (i < readResult.ReadLength)
                {
                    Node prevNode = node;

                    if (!prevNode.TryGetValue(readResult.Array[i], out node)
                        && !prevNode.TryGetValue(NullStandInValue, out node))
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

            //handle expanding collection
            if (!matchFound)
            {
                if (offsetNodesLength >= OffsetNodes.Length)
                {
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

            int i = 0;
            byte? value = type.Header[i];
            int arrayPos = value ?? NullStandInValue;

            Node node = match.Children[arrayPos];

            if (node is null)
            {
                node = new Node((ushort)arrayPos);
                match.Children[arrayPos] = node;
            }

            i++;

            for (; i < type.Header.Length; i++)
            {
                value = type.Header[i];
                arrayPos = value ?? NullStandInValue;
                Node prevNode = node;

                if (!node.TryGetValue((ushort)arrayPos, out node))
                {
                    node = new Node((ushort)arrayPos);

                    //if (i == type.Header.Length - 1)
                    //  node.Record = type;

                    prevNode.Add((ushort)arrayPos, node);
                }
            }

            node.Record = type;
        }

        //TODO make a base-1 dict
        private sealed class Node
        {
            //if complete node then this not null
            public FileType Record;

            public ushort Value;

            private sealed class Entry
            {
                //public ushort _key;
                public Node _value;
                public Entry _next;
            }

            private Entry[] _buckets;
            private int _numEntries;

            public Node(ushort value)
            {
                Value = value;
                Clear(DefaultSize);
            }

            public bool TryGetValue(ushort key, out Node value)
            {
                Entry entry = Find(key);

                if (entry != null)
                {
                    value = entry._value;
                    return true;
                }

                value = null;
                return false;
            }

            public void Add(ushort key, Node value)
            {
                Entry entry = Find(key);

                if (entry != null)
                    throw new ArgumentException("entry already added");

                UncheckedAdd(key, value);
            }

            public void Clear(int capacity = DefaultSize)
            {
                _buckets = new Entry[capacity];
                _numEntries = 0;
            }

            private Entry Find(ushort key)
            {
                int bucket = GetBucket(key);
                Entry entry = _buckets[bucket];
                while (entry != null)
                {
                    if (key == entry._value.Value)
                        return entry;

                    entry = entry._next;
                }
                return null;
            }

            private Entry UncheckedAdd(ushort key, Node value)
            {
                Entry entry = new Entry
                {
                    _value = value
                };

                int bucket = GetBucket(key);
                entry._next = _buckets[bucket];
                _buckets[bucket] = entry;

                _numEntries++;
                if (_numEntries > (_buckets.Length * 2))
                    ExpandBuckets();

                return entry;
            }

            private void ExpandBuckets()
            {
                int newNumBuckets = _buckets.Length * 2 + 1;
                Entry[] newBuckets = new Entry[newNumBuckets];
                for (int i = 0; i < _buckets.Length; i++)
                {
                    Entry entry = _buckets[i];
                    while (entry != null)
                    {
                        Entry nextEntry = entry._next;

                        int bucket = GetBucket(entry._value.Value, newNumBuckets);
                        entry._next = newBuckets[bucket];
                        newBuckets[bucket] = entry;

                        entry = nextEntry;
                    }
                }
                _buckets = newBuckets;
            }

            private int GetBucket(ushort key, int numBuckets = 0)
            {
                int h = key;
                h &= 0x7fffffff;
                return (h % (numBuckets == 0 ? _buckets.Length : numBuckets));
            }
        }
    }
}