using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Input;

namespace IntegratedCalc.Hotkey
{
    public class KeyData : IReadOnlyList<byte>, IReadOnlyCollection<Keys>, IReadOnlyCollection<Key>, IEquatable<KeyData>
    {
        private IList<byte> bytes;

        public KeyData(int capacity)
        {
            if ((uint)capacity != capacity)
                throw new ArgumentException("Capacity must be greater then zero.");
            bytes = new List<byte>(capacity);
        }

        public KeyData()
        {
            bytes = new List<byte>();
        }

        public byte this[int index] => bytes[index];

        public int Count => bytes.Count;

        public bool Add(Keys key) => Add((byte)key);

        public bool Add(Key key) => Add((byte)KeyInterop.VirtualKeyFromKey(key));

        public bool Add(byte keyCode)
        {
            return keyCode != 0 && bytes.AddDistinct(keyCode);
        }

        public void Remove(Keys key) => Remove((byte)key);

        public void Remove(Key key) => Remove((byte)KeyInterop.VirtualKeyFromKey(key));

        public void Remove(byte keyCode)
        {
            bytes.RemoveAll(x => x == keyCode);
        }

        public bool Contains(Keys key) => Contains((byte)key);

        public bool Contains(Key key) => Contains((byte)KeyInterop.VirtualKeyFromKey(key));

        public bool Contains(byte keyCode)
        {
            for (int i = 0; i < bytes.Count; i++)
            {
                if (bytes[i] == keyCode)
                    return true;
            }
            return false;
        }

        public bool Contains(Keys key0, Keys key1) => Contains(key0) && Contains(key1);

        public bool Contains(Key key0, Key key1) => Contains(key0) && Contains(key1);

        public bool Contains(byte keyCode0, byte keyCode1) => Contains(keyCode0) && Contains(keyCode1);

        public bool Contains(Keys key0, Keys key1, Keys key2) => Contains(key0) && Contains(key1) && Contains(key2);

        public bool Contains(Key key0, Key key1, Key key2) => Contains(key0) && Contains(key1) && Contains(key2);

        public bool Contains(byte keyCode0, byte keyCode1, byte keyCode2) => Contains(keyCode0) && Contains(keyCode1) && Contains(keyCode2);

        public bool Contains(Keys key0, Keys key1, Keys key2, Keys key3) => Contains(key0) && Contains(key1) && Contains(key2) && Contains(key3);

        public bool Contains(Key key0, Key key1, Key key2, Key key3) => Contains(key0) && Contains(key1) && Contains(key2) && Contains(key3);

        public bool Contains(byte keyCode0, byte keyCode1, byte keyCode2, byte keyCode3) => Contains(keyCode0) && Contains(keyCode1) && Contains(keyCode2) && Contains(keyCode3);

        public bool Contains(params Keys[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                if (!Contains((byte)keys[i]))
                    return false;
            }
            return true;
        }

        public bool Contains(params Key[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                if (!Contains((byte)KeyInterop.VirtualKeyFromKey(keys[i])))
                    return false;
            }
            return true;
        }

        public bool Contains(params byte[] keyCodes)
        {
            for (int i = 0; i < keyCodes.Length; i++)
            {
                if (!Contains(keyCodes[i]))
                    return false;
            }
            return true;
        }

        public bool Contains(KeyData keyData)
        {
            for (int i = 0; i < keyData.bytes.Count; i++)
            {
                if (keyData.bytes[i] == 0)
                    continue;
                if (!bytes.Contains(keyData.bytes[i]))
                    return false;
            }
            return true;
        }

        public IEnumerator<byte> GetEnumerator() => bytes.GetEnumerator();

        IEnumerator<Keys> IEnumerable<Keys>.GetEnumerator()
        {
            foreach (byte k in bytes)
            {
                yield return (Keys)k;
            }
        }

        IEnumerator<Key> IEnumerable<Key>.GetEnumerator()
        {
            foreach (byte k in bytes)
            {
                yield return KeyInterop.KeyFromVirtualKey(k);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool Equals(KeyData other) => other.Contains(this) && Contains(other);

        public static bool operator ==(KeyData left, KeyData right) => left.Equals(right);

        public static bool operator !=(KeyData left, KeyData right) => !left.Equals(right);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;
            if (obj is null)
                return false;
            if (!(obj is KeyData other))
                return false;
            return Equals(other);
        }

        public override int GetHashCode() => base.GetHashCode();
    }
}
