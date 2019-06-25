﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MimeDetective.Analyzers
{
    public sealed class DictionaryTrie : IFileAnalyzer
    {
        private const ushort NullStandInValue = 256;

        //root dictionary contains the nodes with offset values
        private Dictionary<ushort, Node> Nodes { get; } = new Dictionary<ushort, Node>();

        /// <summary>
        /// Constructs an empty DictionaryBasedTrie
        /// </summary>
        public DictionaryTrie()
        {

        }

        /// <summary>
        /// Constructs a DictionaryBasedTrie from an Enumerable of FileTypes
        /// </summary>
        /// <param name="types"></param>
        public DictionaryTrie(IEnumerable<FileType> types)
        {
            if (types is null)
                ThrowHelpers.FileTypeEnumerableIsNull();

            foreach (var type in types)
            {
                if ((object)type != null)
                    Insert(type);
            }
        }

        public FileType Search(in ReadResult readResult)
        {
            FileType match = null;
            var enumerator = Nodes.GetEnumerator();
            int highestMatchingCount = 0;

            while (enumerator.MoveNext())
            {
                Node node = enumerator.Current.Value;
                int i = node.Value;

                while (i < readResult.ReadLength)
                {
                    Node prevNode = node;

                    if (!prevNode.Children.TryGetValue(readResult.Array[i], out node)
                        && !prevNode.Children.TryGetValue(NullStandInValue, out node))
                        break;

                    i++;

                    if (i > highestMatchingCount && (object)node.Record != null)
                    {
                        match = node.Record;
                        highestMatchingCount = i;
                    }
                }
            }

            if (match == null && (readResult.ReadLength < 1 || (int)readResult.Array[0] < 31))
            {
                return null;
            }

            return match ?? MimeTypes.UNKNOWN;
        }

        public void Insert(FileType type)
        {
            if (type is null)
                ThrowHelpers.FileTypeArgumentIsNull();

            if (!Nodes.TryGetValue(type.HeaderOffset, out var offsetNode))
            {
                offsetNode = new Node(type.HeaderOffset);
                Nodes.Add(type.HeaderOffset, offsetNode);
            }

            if (type.Header.Length == 0)
            {
                return;
            }

            int i = 0;
            ushort value = type.Header[i] ?? NullStandInValue;

            if (!offsetNode.Children.TryGetValue(value, out Node node))
            {
                node = new Node(value);
                offsetNode.Children.Add(value, node);
            }

            i++;

            for (; i < type.Header.Length; i++)
            {
                value = type.Header[i] ?? NullStandInValue;

                if (!node.Children.ContainsKey(value))
                {
                    Node newNode = new Node(value);
                    node.Children.Add(value, newNode);
                }

                node = node.Children[value];
            }

            node.Record = type;
        }

        private sealed class Node
        {
            public Dictionary<ushort, Node> Children = new Dictionary<ushort, Node>();

            //if complete node then this not null
            public FileType Record;

            public ushort Value;

            public Node(ushort value)
            {
                Value = value;
            }
        }
    }
}

