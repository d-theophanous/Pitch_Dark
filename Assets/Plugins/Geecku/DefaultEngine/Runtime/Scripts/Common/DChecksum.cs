using System;
using System.Collections;
using UnityEngine;

namespace Geecku.DefaultEngine.Common
{
    public struct DChecksum : IEquatable<DChecksum>
    {
        public static DChecksum Empty = new DChecksum();
        public string Hash;

        public DChecksum(string hash)
        {
            Hash = hash;
        }

        #region Operators
        public static bool operator ==(DChecksum a, DChecksum b) => a.Equals(b);
        public static bool operator !=(DChecksum a, DChecksum b) => !(a == b);
        public override bool Equals(object obj) => obj is DChecksum _check && Equals(_check);
        public override int GetHashCode() => Hash.GetHashCode();
        public bool Equals(DChecksum other)
        {
            if (Hash == null)
                return Hash == other.Hash;
            return Hash.Equals(other.Hash);
        }
        #endregion


        public override string ToString()
        {
            if (string.IsNullOrEmpty(Hash))
                return "null";
            return Hash.Substring(0, Math.Min(5, Hash.Length));
        }
    }
}