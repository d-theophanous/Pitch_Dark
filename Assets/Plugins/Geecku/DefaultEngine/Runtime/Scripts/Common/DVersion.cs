using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Geecku.DefaultEngine.Common
{
    public enum DVersionStatus { Release, Alpha, Beta }
    [Serializable, InlineProperty(LabelWidth = 13)]
    public struct DVersion : IEquatable<DVersion>
    {
        [HideLabel, EnumToggleButtons]
        public DVersionStatus Status;

        [HorizontalGroup("v_group"), HideLabel]
        public byte Main;
        [HorizontalGroup("v_group"), HideLabel]
        public byte Update;
        [HorizontalGroup("v_group"), HideLabel]
        public string Patch;
        [HorizontalGroup("v_group"), HideLabel]
        public string Hotfix;
        public DChecksum Checksum;

        public bool IsEmpty => Main == 0 && Update == 0 && (string.IsNullOrEmpty(Patch) || Patch == "0") && (string.IsNullOrEmpty(Hotfix) || Hotfix == "0");

        public DVersion(byte main, byte update, string patch, string hotfix)
        {
            Status = DVersionStatus.Release;
            Main = main;
            Update = update;
            Patch = patch;
            Hotfix = hotfix;
            Checksum = DChecksum.Empty;
        }

        public bool Equals(DVersion other)
        {
            if (Checksum == DChecksum.Empty)
                return Main == other.Main && Update == other.Update && Patch == other.Patch && Hotfix == other.Hotfix;
            return Checksum == other.Checksum;
        }

        public override string ToString()
        {
            string prefix = "";
            string suffix = "";
            if (Status != DVersionStatus.Release)
                prefix = Status.ToString() + " ";
            if (Checksum != DChecksum.Empty)
                suffix = " (" + Checksum.ToString() + ")";
            if (string.IsNullOrEmpty(Hotfix) || Hotfix == "0")
                return prefix + Main + "." + Update + "." + Patch + suffix;
            return prefix + Main + "." + Update + "." + Patch + "." + Hotfix + suffix;
        }
    }
}
