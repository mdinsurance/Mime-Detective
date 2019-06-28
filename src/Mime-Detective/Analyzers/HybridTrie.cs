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
                this.Offset = offset;
                this.Children = new Node[MaxNodeSize];
            }
        }

        /// <summary>
        /// Constructs an empty DictionaryBasedTrie
        /// </summary>
        public HybridTrie()
        {
            this.OffsetNodes[0] = new OffsetNode(0);
        }

        /// <summary>
        /// Constructs a DictionaryBasedTrie from an Enumerable of FileTypes
        /// </summary>
        /// <param name="types"></param>
        public HybridTrie(IEnumerable<FileType> types)
        {
            if (types is null)
            {
                ThrowHelpers.FileTypeArgumentIsNull();
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

        public FileType Search(in ReadResult readResult, string mimeHint = null, string extensionHint = null)
        {
            FileType match = null;
            var highestMatchingCount = 0;

            //iterate through offset nodes
            for (var offsetNodeIndex = 0; offsetNodeIndex < this.offsetNodesLength; offsetNodeIndex++)
            {
                //get offset node
                var offsetNode = this.OffsetNodes[offsetNodeIndex];
                int i = offsetNode.Offset;

                if (!(i < readResult.ReadLength))
                {
                    continue;
                }

                var node = offsetNode.Children[readResult.Array[i]];

                if (node == null)
                {
                    node = offsetNode.Children[NullStandInValue];

                    if (node is null)
                    {
                        continue;
                    }
                }

                i++;

                if (i > highestMatchingCount && !(node.Record is null))
                {
                    match = node.Record;
                    highestMatchingCount = i;
                }

                while (i < readResult.ReadLength)
                {
                    var prevNode = node;

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

            //handle expanding collection
            if (!matchFound)
            {
                if (this.offsetNodesLength >= this.OffsetNodes.Length)
                {
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

            var i = 0;
            var value = type.Header[i];
            int arrayPos = value ?? NullStandInValue;

            var node = match.Children[arrayPos];

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
                var prevNode = node;

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
                this.Value = value;
                this.Clear(DefaultSize);
            }

            public bool TryGetValue(ushort key, out Node value)
            {
                var entry = this.Find(key);

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
                var entry = this.Find(key);

                if (entry != null)
                {
                    throw new ArgumentException("entry already added");
                }

                this.UncheckedAdd(key, value);
            }

            public void Clear(int capacity = DefaultSize)
            {
                this._buckets = new Entry[capacity];
                this._numEntries = 0;
            }

            private Entry Find(ushort key)
            {
                var bucket = this.GetBucket(key);
                var entry = this._buckets[bucket];
                while (entry != null)
                {
                    if (key == entry._value.Value)
                    {
                        return entry;
                    }

                    entry = entry._next;
                }
                return null;
            }

            private Entry UncheckedAdd(ushort key, Node value)
            {
                var entry = new Entry
                {
                    _value = value
                };

                var bucket = this.GetBucket(key);
                entry._next = this._buckets[bucket];
                this._buckets[bucket] = entry;

                this._numEntries++;
                if (this._numEntries > (this._buckets.Length * 2))
                {
                    this.ExpandBuckets();
                }

                return entry;
            }

            private void ExpandBuckets()
            {
                var newNumBuckets = this._buckets.Length * 2 + 1;
                var newBuckets = new Entry[newNumBuckets];
                for (var i = 0; i < this._buckets.Length; i++)
                {
                    var entry = this._buckets[i];
                    while (entry != null)
                    {
                        var nextEntry = entry._next;

                        var bucket = this.GetBucket(entry._value.Value, newNumBuckets);
                        entry._next = newBuckets[bucket];
                        newBuckets[bucket] = entry;

                        entry = nextEntry;
                    }
                }
                this._buckets = newBuckets;
            }

            private int GetBucket(ushort key, int numBuckets = 0)
            {
                int h = key;
                h &= 0x7fffffff;
                return (h % (numBuckets == 0 ? this._buckets.Length : numBuckets));
            }
        }
    }
}