using System;
using System.Collections.Generic;

namespace MimeDetective.Trie
{


    public sealed class BaseNode
    {

        public SortedDictionary<ushort, OffsetNode> Nodes { get; set; }

        public FileType Get(in ReadResult readResult)
        {
            return null;
        }
    }


    //Node used to present offsets
    public sealed class OffsetNode
    {
        public ushort Offset { get; set; }

        private static ushort NumberOfPossibleValuesPerNode = 257;

        public Dictionary<byte?, Node> Nodes { get; } = new Dictionary<byte?, Node>(257);
    }

    public sealed class Node
    {
        private static ushort NumberOfPossibleValuesPerNode = 257;

        public Dictionary<byte?, Node> Nodes { get; } = new Dictionary<byte?, Node>(257);

        //if complete node then this not null
        public FileType Value;


        private Node GetNode(byte? _byte)
        {
            return Nodes[_byte];
        }

        private void SetNode(byte? _byte, Node node)
        {
            Nodes[_byte] = node;
        }


        public void Add(FileType type)
        {
            Add(type, 0);
        } 

        private void Add(FileType type, int index)
        {

        }

        public int FindCount(FileType type, int index)
        {
            return 0;
        }
    }


}
