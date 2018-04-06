using System;
using System.Collections.Generic;

namespace MimeDetective.Analyzers
{
    public sealed class DictionaryBasedTrie2 : IFileAnalyzer
    {
        private const ushort NullStandInValue = 256;

        //root dictionary contains the nodes with offset values
        private Dictionary<ushort, Node> Nodes { get; } = new Dictionary<ushort, Node>();

        /// <summary>
        /// Constructs an empty DictionaryBasedTrie
        /// </summary>
        public DictionaryBasedTrie2()
        {
        }

        /// <summary>
        /// Constructs a DictionaryBasedTrie from an Enumerable of FileTypes
        /// </summary>
        /// <param name="types"></param>
        public DictionaryBasedTrie2(IEnumerable<FileType> types)
        {
            if (types is null)
                throw new ArgumentNullException(nameof(types));

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
            {
                Node node = enumerator.Current.Value;

                for (int i = node.Value; i < readResult.ReadLength; i++)
                {
                    Node prevNode = node;

                    if (!prevNode.TryGetValue(readResult.Array[i], out node)
                        && !prevNode.TryGetValue(NullStandInValue, out node))
                        break;

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

            if (!Nodes.TryGetValue(type.HeaderOffset, out var offsetNode))
            {
                offsetNode = new Node(type.HeaderOffset);
                Nodes.Add(type.HeaderOffset, offsetNode);
            }

            offsetNode.Insert(type);
        }

        private sealed class Node
        {
            //public readonly Dictionary<ushort, Node> Children = new Dictionary<ushort, Node>();

            //if complete node then this not null
            public FileType Record;

            public readonly ushort Value;

            private sealed class Entry
            {
                public ushort _key;
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

            public void Insert(FileType type)
            {
                int i = 0;
                ushort value = type.Header[i] ?? NullStandInValue;

                if (!TryGetValue(value, out Node node))
                {
                    node = new Node(value);
                    Add(value, node);
                }

                i++;

                for (; i < type.Header.Length; i++)
                {
                    value = type.Header[i] ?? NullStandInValue;

                    if (!node.TryGetValue(value, out var newNode))
                    {
                        newNode = new Node(value);
                        node.Add(value, newNode);
                    }

                    node = newNode;//node[value];
                }

                node.Record = type;
            }

            private const int DefaultSize = 17;

            public Node this[ushort key]
            {
                get
                {
                    Entry entry = Find(key);
                    if (entry == null)
                        throw new KeyNotFoundException("Key not could for entry");
                    return entry._value;
                }
                set
                {
                    Entry entry = Find(key);
                    if (entry != null)
                        entry._value = value;
                    else
                        UncheckedAdd(key, value);
                }
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
                    if (key == entry._key)
                        return entry;

                    entry = entry._next;
                }
                return null;
            }

            private Entry UncheckedAdd(ushort key, Node value)
            {
                Entry entry = new Entry
                {
                    _key = key,
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

                        int bucket = GetBucket(entry._key, newNumBuckets);
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