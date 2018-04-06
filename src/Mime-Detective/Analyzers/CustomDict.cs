using MimeDetective;

/*
namespace System.Collections.Generic
{
    /// <summary>
    /// Used internally to control behavior of insertion into a <see cref="Dictionary{TKey, TValue}"/>.
    /// </summary>
    internal enum InsertionBehavior : byte
    {
        /// <summary>
        /// The default insertion behavior.
        /// </summary>
        None = 0,

        /// <summary>
        /// Specifies that an existing entry with the same key should be overwritten if encountered.
        /// </summary>
        OverwriteExisting = 1,

        /// <summary>
        /// Specifies that if an existing entry with the same key is encountered, an exception should be thrown.
        /// </summary>
        ThrowOnExisting = 2
    }

    public class CustomDictionary<TKey, TValue>
    {
        private struct Entry
        {
            public int hashCode;    // Lower 31 bits of hash code, -1 if unused
            public int next;        // Index of next entry, -1 if last
            public ushort key;           // Key of entry
            public Node value;         // Value of entry
        }

        private int[] _buckets;
        private Entry[] _entries;
        private int _count;
        private int _freeList;
        private int _freeCount;

        public CustomDictionary(int capacity = 17)
        {
            Initialize(capacity);
        }

        private int FindEntry(ushort key)
        {
            int i = -1;
            int[] buckets = _buckets;
            Entry[] entries = _entries;

            if (buckets != null)
            {
                int hashCode = key & 0x7FFFFFFF;
                // Value in _buckets is 1-based
                i = buckets[hashCode % buckets.Length] - 1;
                do
                {
                    // Should be a while loop https://github.com/dotnet/coreclr/issues/15476
                    // Test in if to drop range check for following array access
                    if ((uint)i >= (uint)entries.Length ||
                        (entries[i].hashCode == hashCode && entries[i].key == key))
                    {
                        break;
                    }

                    i = entries[i].next;
                } while (true);
            }

            return i;
        }

        private int Initialize(int capacity)
        {
            int size = HashHelpers.GetPrime(capacity);

            _freeList = -1;
            _buckets = new int[size];
            _entries = new Entry[size];

            return size;
        }

        public bool TryInsert(ushort key, Node value, InsertionBehavior behavior)
        {
            if (_buckets == null)
            {
                Initialize(0);
            }

            Entry[] entries = _entries;

            int hashCode = key & 0x7FFFFFFF;

            int collisionCount = 0;
            ref int bucket = ref _buckets[hashCode % _buckets.Length];
            // Value in _buckets is 1-based
            int i = bucket - 1;

            do
            {
                // Should be a while loop https://github.com/dotnet/coreclr/issues/15476
                // Test uint in if rather than loop condition to drop range check for following array access
                if ((uint)i >= (uint)entries.Length)
                {
                    break;
                }

                if (entries[i].hashCode == hashCode && EqualityComparer<TKey>.Default.Equals(entries[i].key, key))
                {
                    if (behavior == InsertionBehavior.OverwriteExisting)
                    {
                        entries[i].value = value;
                        return true;
                    }

                    if (behavior == InsertionBehavior.ThrowOnExisting)
                    {
                        ThrowHelper.ThrowAddingDuplicateWithKeyArgumentException(key);
                    }

                    return false;
                }

                i = entries[i].next;
                if (collisionCount >= entries.Length)
                {
                    // The chain of entries forms a loop; which means a concurrent update has happened.
                    // Break out of the loop and throw, rather than looping forever.
                    ThrowHelper.ThrowInvalidOperationException_ConcurrentOperationsNotSupported();
                }
                collisionCount++;
            } while (true);

            // Can be improved with "Ref Local Reassignment"
            // https://github.com/dotnet/csharplang/blob/master/proposals/ref-local-reassignment.md
            bool resized = false;
            bool updateFreeList = false;
            int index;
            if (_freeCount > 0)
            {
                index = _freeList;
                updateFreeList = true;
                _freeCount--;
            }
            else
            {
                int count = _count;
                if (count == entries.Length)
                {
                    Resize();
                    resized = true;
                }
                index = count;
                _count = count + 1;
                entries = _entries;
            }

            ref int targetBucket = ref resized ? ref _buckets[hashCode % _buckets.Length] : ref bucket;
            ref Entry entry = ref entries[index];

            if (updateFreeList)
            {
                _freeList = entry.next;
            }

            entry.hashCode = hashCode;
            // Value in _buckets is 1-based
            entry.next = targetBucket - 1;
            entry.key = key;
            entry.value = value;
            // Value in _buckets is 1-based
            targetBucket = index + 1;

            // Value types never rehash
            if (collisionCount > HashHelpers.HashCollisionThreshold)
            {
                Resize(entries.Length, true);
            }

            return true;
        }

        private void Resize() => Resize(HashHelpers.ExpandPrime(_count), false);

        private void Resize(int newSize, bool forceNewHashCodes)
        {
            int[] buckets = new int[newSize];
            Entry[] entries = new Entry[newSize];

            int count = _count;
            Array.Copy(_entries, 0, entries, 0, count);

            if (default(TKey) == null && forceNewHashCodes)
            {
                for (int i = 0; i < count; i++)
                {
                    if (entries[i].hashCode >= 0)
                    {
                        entries[i].hashCode = (entries[i].key.GetHashCode() & 0x7FFFFFFF);
                    }
                }
            }

            for (int i = 0; i < count; i++)
            {
                if (entries[i].hashCode >= 0)
                {
                    int bucket = entries[i].hashCode % newSize;
                    // Value in _buckets is 1-based
                    entries[i].next = buckets[bucket] - 1;
                    // Value in _buckets is 1-based
                    buckets[bucket] = i + 1;
                }
            }

            _buckets = buckets;
            _entries = entries;
        }

        public bool TryGetValue(ushort key, out Node value)
        {
            int i = FindEntry(key);
            if (i >= 0)
            {
                value = _entries[i].value;
                return true;
            }
            value = default;
            return false;
        }
    }
}
*/