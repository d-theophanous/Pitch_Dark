using UnityEngine;

namespace Geecku.DefaultEngine.Common
{
    public static class DDefines
    {
        #region Colors
        public const string RedColorHex = "#971515";
        public const string GreenColorHex = "#166924";
        public const string GrayColorHex = "#202020";
        public const string YellowColorHex = "#C2BC2F";
        public const string CyanColorHex = "#2CA1A8";
        public const string LightGreenColorHex = "#0ECB2F";
        public const string SoLGrayColorHex = "#787878";

        private static Color _RedColor = Color.white;
        public static Color RedColor
        {
            get
            {
                if (_RedColor.Equals(Color.white))
                    _RedColor = Helper.ToColor(RedColorHex);
                return _RedColor;
            }
        }
        private static Color _GreenColor = Color.white;
        public static Color GreenColor
        {
            get
            {
                if (_GreenColor.Equals(Color.white))
                    _GreenColor = Helper.ToColor(GreenColorHex);
                return _GreenColor;
            }
        }
        private static Color _YellowColor = Color.white;
        public static Color YellowColor
        {
            get
            {
                if (_YellowColor.Equals(Color.white))
                    _YellowColor = Helper.ToColor(YellowColorHex);
                return _YellowColor;
            }
        }
        private static Color _CyanColor = Color.white;
        public static Color CyanColor
        {
            get
            {
                if (_CyanColor.Equals(Color.white))
                    _CyanColor = Helper.ToColor(CyanColorHex);
                return _CyanColor;
            }
        }
        private static Color _SolGrayColor = Color.white;
        public static Color SolGrayColor
        {
            get
            {
                if (_SolGrayColor.Equals(Color.white))
                    _SolGrayColor = Helper.ToColor(SoLGrayColorHex);
                return _SolGrayColor;
            }
        }

        #endregion
    }
}
