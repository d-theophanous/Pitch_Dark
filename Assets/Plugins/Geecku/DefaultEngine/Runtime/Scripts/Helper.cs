using Geecku.DefaultEngine.Common;
using Geecku.GlobalMangers;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Geecku.DefaultEngine
{
    /// <summary>
    /// This class is a helper-class that contains a lot of usefull methodes
    /// </summary>
    public static class Helper
    {
        #region Collider
        /// <summary>
        /// Checks and gets a Component if it is on a Collider
        /// </summary>
        /// <typeparam name="T">Specified Component to search for</typeparam>
        /// <param name="colls">The collider(s) to search through</param>
        /// <returns>Returns the founded Component or its default (null)</returns>
        public static T CheckCollider<T>(params Collider[] colls) where T : MonoBehaviour
        {
            foreach (var item in colls)
                if (item.GetComponent<T>() is T t)
                    return t;
            return default(T);
        }
        /// <summary>
        /// Checks and gets all Components if they're on a Collider
        /// </summary>
        /// <typeparam name="T">Specified Component to search for</typeparam>
        /// <param name="colls">The collider(s) to search through</param>
        /// <returns>Returns the founded Components or their defaults (null) as a list</returns>
        public static List<T> CheckColliders<T>(params Collider[] colls) where T : MonoBehaviour
        {
            List<T> output = new();
            foreach (var item in colls)
                if (item.GetComponent<T>() is T t)
                    output.Add(t);
            return output;
        }

        #endregion

        #region RectTransform
        /// <summary>
        /// Sets the sizeDelta of a MonoBehaviour. It must have a RectTransform to function!
        ///     (Shortcut-Methode. Can be accessed inside of the specified class).
        /// </summary>
        /// <param name="game_object">The MonoBehaviour to set the size on</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public static void SetSize(this MonoBehaviour game_object, float width, float height)
        {
            SetSize(game_object, new Vector2(width, height));
        }
        /// <summary>
        /// Sets the sizeDelta of a MonoBehaviour. It must have a RectTransform to function!
        ///     (Shortcut-Methode. Can be accessed inside of the specified class).
        /// </summary>
        /// <param name="game_object">The MonoBehaviour to set the size on</param>
        /// <param name="size">The new sizeDelta of the object</param>
        public static void SetSize(this MonoBehaviour game_object, Vector2 size)
        {
            game_object.GetComponent<RectTransform>().sizeDelta = size;
        }
        /// <summary>
        /// Gets the current sizeDelta of a MonoBehaivour. It must have a RectTransform to function!
        ///     (Shortcut-Methode. Can be accessed inside of the specified class).
        /// </summary>
        /// <param name="game_object">The MonoBehaviour that is called</param>
        /// <returns>sizeDelta of the MonoBehaviour</returns>
        public static Vector2 GetSize(this MonoBehaviour game_object)
        {
            return game_object.GetComponent<RectTransform>().sizeDelta;
        }
        /// <summary>
        /// Gets the current RectTransform of a MonoBehaivour.
        ///     (Shortcut-Methode. Can be accessed inside of the specified class).
        /// </summary>
        /// <param name="game_object">The MonoBehaviour that is called</param>
        /// <returns>RectTransform of the MonoBehaviour or null if it does not exist</returns>
        public static RectTransform Rect(this MonoBehaviour game_object)
        {
            return game_object.GetComponent<RectTransform>();
        }
        /// <summary>
        /// Clamps a RectTransform inside of a limited bounding-box acording to two vectors.
        ///     (Does not work on Scaled Objects if parents are scaled)
        /// </summary>
        /// <param name="transform">The transform that should be clamped</param>
        /// <param name="x_bounds">The X-bounds where vector.x is the lowest x-value and vector.y is the highest</param>
        /// <param name="y_bounds">The Y-bounds where vector.x is the lowest y-value and vector.y is the highest</param>
        /// <param name="canvas_scale">Scaling factor for corrections. Default is 1 - usualy you should define this with your Canvas.</param>
        public static void Clamp(RectTransform transform, Vector2 x_bounds, Vector2 y_bounds, float canvas_scale = 1f)
        {
            //- Limit x
            float window_width = transform.rect.width * canvas_scale;
            float window_x_pos = transform.position.x;
            if (window_x_pos < transform.pivot.x * window_width + x_bounds.x)
                transform.position = new Vector3(transform.pivot.x * window_width + x_bounds.x, transform.position.y, transform.position.z);
            if (window_x_pos + window_width > x_bounds.y + transform.pivot.x * window_width)
                transform.position = new Vector3(x_bounds.y + transform.pivot.x * window_width - window_width, transform.position.y, transform.position.z);

            //- Limit y
            float window_height = transform.rect.height * canvas_scale;
            float window_y_pos = transform.position.y;
            if (window_y_pos < transform.pivot.y * window_height + y_bounds.x)
                transform.position = new Vector3(transform.position.x, transform.pivot.y * window_height + y_bounds.x, transform.position.z);
            if (window_y_pos + window_height > y_bounds.y + transform.pivot.y * window_height)
                transform.position = new Vector3(transform.position.x, y_bounds.y + transform.pivot.y * window_height - window_height, transform.position.z);
        }
        /// <summary>
        /// Clamps a RectTransform inside of the bounding box of the current screen.
        ///     (Does not work on Scaled Objects if parents are scaled)
        /// </summary>
        /// <param name="transform">The transform that should be clamped</param>
        /// <param name="canvas_scale">Scaling factor for corrections. Default is 1 - usualy you should define this with your Canvas.</param>
        public static void Clamp(RectTransform transform, float canvas_scale = 1f)
        {
            Clamp(transform, new Vector2(0, Screen.width), new Vector2(0, Screen.height), canvas_scale);
        }
        #endregion

        #region IEnumerators
        /// <summary>
        /// Evaluates an Action after a enumerator has finished (usefull for Coroutines).
        /// </summary>
        /// <param name="enumerator">The Enumerator-Methode that should be called before the Action</param>
        /// <param name="after_done">Action after the Enumerator</param>
        /// <returns></returns>
        public static IEnumerator DoAfter(IEnumerator enumerator, Action after_done)
        {
            yield return enumerator;
            after_done?.Invoke();
        }

        public static IEnumerator WaitForKey(KeyCode key)
        {
            return new WaitUntil(() => Input.GetKeyUp(key));
        }
        #endregion

        #region Automatic Width
        /// <summary>
        /// Updates the sizeDelta of this GameObject depended on preferredWidth and its current height.
        ///     (Shortcut-Methode. Can be accessed inside of the specified class).
        /// </summary>
        /// <param name="text"></param>
        public static void UpdateWidth(this Text text)
        {
            text.rectTransform.sizeDelta = new Vector2(text.preferredWidth, text.rectTransform.sizeDelta.y);
        }
        /// <summary>
        /// Updates the sizeDelta of this GameObject depended on preferredWidth and its current height.
        ///     (Shortcut-Methode. Can be accessed inside of the specified class).
        /// </summary>
        /// <param name="text"></param>
        public static void UpdateWidth(this TextMeshProUGUI text)
        {
            text.rectTransform.sizeDelta = new Vector2(text.preferredWidth, text.rectTransform.sizeDelta.y);
        }
        /// <summary>
        /// Updates the sizeDelta of this HorizontalLayoutGroup depended on its children.
        /// It will ignore a Child if it has LayoutElement.ignoreLayout and if its not a pre-defined child, sizeDelta.x will be used for calculating global Width.
        ///     (Shortcut-Methode. Can be accessed inside of the specified class).
        /// </summary>
        /// <param name="group"></param>
        public static void UpdateWidth(this HorizontalLayoutGroup group)
        {
            RectTransform rect = group.GetComponent<RectTransform>();
            int child_count = 0;
            float total_width = group.padding.left + group.padding.right;
            foreach (Transform item in group.transform)
            {
                if (!item.gameObject.activeSelf)
                    continue;
                LayoutElement layout;
                if ((layout = item.gameObject.GetComponent<LayoutElement>()) != null && layout.ignoreLayout)
                    continue;
                child_count++;
                float local_adder = 0;

                RectTransform local_rect = item.GetComponent<RectTransform>();
                if (local_rect != null)
                    local_adder = local_rect.sizeDelta.x;

                TextMeshProUGUI local_text = item.GetComponent<TextMeshProUGUI>();
                if (local_text != null)
                    local_adder = local_text.preferredWidth;

                total_width += local_adder;
            }
            total_width += Mathf.Max(0, (child_count - 1) * group.spacing);

            rect.sizeDelta = new Vector2(total_width, rect.sizeDelta.y);
        }

        /// <summary>
        /// Get the theoretical optimal width of this GameObject dependent on its Children.
        /// It will ignore a Child if it has LayoutElement.ignoreLayout.
        ///     (Shortcut-Methode. Can be accessed inside of the specified class).
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public static float GetOptimalWidth(this GridLayoutGroup group)
        {
            int child_count = 0;
            float total_width = group.padding.left + group.padding.right;
            foreach (Transform item in group.transform)
            {
                if (!item.gameObject.activeSelf)
                    continue;
                LayoutElement layout;
                if ((layout = item.gameObject.GetComponent<LayoutElement>()) != null && layout.ignoreLayout)
                    continue;
                child_count++;

                float local_adder = group.cellSize.x;

                total_width += local_adder;
            }
            total_width += Mathf.Max(0, (child_count - 1) * group.spacing.x);

            return total_width;
        }
        /// <summary>
        /// Get the theoretical optimal width of this GameObject dependent on its Children.
        /// It will ignore a Child if it has LayoutElement.ignoreLayout and if its not a pre-defined child, sizeDelta.x will be used for calculating global Width.
        ///     (Shortcut-Methode. Can be accessed inside of the specified class).
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public static float GetOptimalWidth(this HorizontalLayoutGroup group)
        {
            int child_count = 0;
            float total_width = group.padding.left + group.padding.right;
            foreach (Transform item in group.transform)
            {
                if (!item.gameObject.activeSelf)
                    continue;
                LayoutElement layout;
                if ((layout = item.gameObject.GetComponent<LayoutElement>()) != null && layout.ignoreLayout)
                    continue;
                child_count++;
                float local_adder = 0;

                RectTransform local_rect = item.GetComponent<RectTransform>();
                if (local_rect != null)
                    local_adder = local_rect.sizeDelta.x;

                TextMeshProUGUI local_text = item.GetComponent<TextMeshProUGUI>();
                if (local_text != null)
                    local_adder = local_text.preferredWidth;

                total_width += local_adder;
            }
            total_width += Mathf.Max(0, (child_count - 1)) * group.spacing;

            return total_width;
        }
        #endregion

        #region Automatic Heigth
        /// <summary>
        /// ToDo
        /// </summary>
        /// <param name="group"></param>
        public static void UpdateHeigth(this GridLayoutGroup group)
        {
            //- ToDo
        }
        /// <summary>
        ///  Description ToDo, the methodes works as indented.
        /// </summary>
        /// <param name="group"></param>
        public static void UpdateHeigth(this VerticalLayoutGroup group)
        {
            RectTransform rect = group.GetComponent<RectTransform>();
            int child_count = 0;
            float total_height = group.padding.top + group.padding.bottom;
            foreach (Transform item in group.transform)
            {
                if (!item.gameObject.activeSelf)
                    continue;
                LayoutElement layout;
                if ((layout = item.gameObject.GetComponent<LayoutElement>()) != null && layout.ignoreLayout)
                    continue;
                child_count++;
                float local_adder = 0;

                RectTransform local_rect = item.GetComponent<RectTransform>();
                if (local_rect != null)
                    local_adder = local_rect.sizeDelta.y;

                TextMeshProUGUI local_text = item.GetComponent<TextMeshProUGUI>();
                if (local_text != null)
                    local_adder = local_text.preferredHeight;

                total_height += local_adder;
            }
            total_height += Mathf.Max(0, (child_count - 1) * group.spacing);

            rect.sizeDelta = new Vector2(rect.sizeDelta.x, total_height);
        }
        /// <summary>
        ///  Description ToDo, the methodes works as indented.
        /// </summary>
        /// <param name="group"></param>
        /// <param name="fixed_child_heigth"></param>
        public static void UpdateHeigth(this VerticalLayoutGroup group, float fixed_child_heigth)
        {
            RectTransform rect = group.GetComponent<RectTransform>();
            int child_count = 0;
            foreach (Transform item in group.transform)
            {
                if (!item.gameObject.activeSelf)
                    continue;
                LayoutElement layout;
                if ((layout = item.gameObject.GetComponent<LayoutElement>()) != null && layout.ignoreLayout)
                    continue;
                child_count++;
            }
            float total_height = group.padding.top + group.padding.bottom
                + child_count * fixed_child_heigth + Mathf.Max(0, (child_count - 1) * group.spacing);

            rect.sizeDelta = new Vector2(rect.sizeDelta.x, total_height);
        }
        /// <summary>
        ///  Description ToDo, the methodes works as indented.
        /// </summary>
        /// <param name="text"></param>
        public static void UpdateHeigth(this Text text)
        {
            text.rectTransform.sizeDelta = new Vector2(text.rectTransform.sizeDelta.x, text.preferredHeight);
        }
        /// <summary>
        ///  Description ToDo, the methodes works as indented.
        /// </summary>
        /// <param name="text"></param>
        public static void UpdateHeigth(this TextMeshProUGUI text)
        {
            text.rectTransform.sizeDelta = new Vector2(text.rectTransform.sizeDelta.x, text.preferredHeight);
        }
        #endregion

        #region Random
        public static int GetRandomIndex(float[] probabilities)
        {
            if (probabilities == null || probabilities.Length == 0)
            {
                Debug.LogError("Probabilities is null or zero");
                return -1;
            }
            float sum = 0;
            for (int k = 0; k < probabilities.Length; k++)
                sum += probabilities[k];

            float r = (float)Engine.Random.NextDouble() * sum;
            float min_border = 0;
            for (int k = 0; k < probabilities.Length; k++)
            {
                if (min_border <= r && r < min_border + probabilities[k])
                {
                    return k;
                }
                min_border += probabilities[k];
            }
            Debug.LogError("Error at generating random value for list.");
            return -1;
        }
        #endregion

        #region Format
        public static Color32 AttributeColor => new Color32(255, 228, 109, 255);
        public static string ToFormat(int value)
        {
            return value.ToString();
        }
        public static string ToFormat(float value)
        {
            return ToFormat(value, true, 0, false);
        }
        public static string ToFormat(float value, bool is_relative, int decimals, bool signed = true)
        {
            if (is_relative)
            {
                if (value >= 0)
                {
                    return (signed ? "+ " : "") + Math.Round(value * 100, decimals) + "%";
                }
                else
                {
                    return "- " + Math.Round(Mathf.Abs(value) * 100, decimals) + "%";
                }
            }
            if (value >= 0)
            {
                return (signed ? "+ " : "") + Math.Round(value, decimals);
            }
            else
            {
                return "- " + Math.Round(Mathf.Abs(value), decimals);
            }
        }
        public static string ToAttributeFormat(float value, bool is_percent = false)
        {
            return "<color=" + ToHex(AttributeColor) + ">" + ToFormat(value, !is_percent, 2) + "</color>";
        }
        public static string ToLength(int value, int length, char fill_char = '0')
        {
            if (value.ToString().Length == length)
                return value.ToString();

            string txt = value.ToString();
            for (int i = 0; i < length - txt.Length; i++)
            {
                txt = fill_char + txt;
            }
            return txt;
        }
        public static string ToFormat(bool value)
        {
            return value ? "Ja" : "Nein";
        }

        //- Num Format
        public static string ToNumberFormat(int value)
        {
            return value.ToString("N0", new System.Globalization.CultureInfo("de-DE"));
        }
        public static string ToNumberFormat(float value, bool add_zero = true)
        {
            if (value == 0)
                return "0,0 K.";
            //- 12.546.168,165
            if (value < 1000)
                return AddZeroIfNessesarry(Mathf.RoundToInt(value * 100f) / 100f, add_zero);
            if (value < 1000000)    //- 74561 >/10> 7456
                return AddZeroIfNessesarry(Mathf.RoundToInt(value / 10f) / 100f, add_zero) + " K.";
            if (value < 1000000000)    //- 74561147 >/10000> 7456
                return AddZeroIfNessesarry(Mathf.RoundToInt(value / 10000f) / 100f, add_zero) + " M.";
            if (value < 1000000000000)    //- 74561147 >/10000000> 7456
                return AddZeroIfNessesarry(Mathf.RoundToInt(value / 10000000f) / 100f, add_zero) + " B.";
            return Mathf.RoundToInt(value).ToString();
        }
        private static string AddZeroIfNessesarry(float value, bool add_zero)
        {
            if (add_zero && value == Mathf.RoundToInt(value))
                return value.ToString() + ",0";
            return value.ToString();
        }
        public static string ToIncomeFormat(float value, bool color_it = true, string suffix = " /m")
        {
            bool sign = value >= 0;
            if (value == 0)
                return "0,0" + suffix;
            string txt;
            string color_text;
            if (sign)
            {
                txt = "+ ";
                color_text = DDefines.LightGreenColorHex;
            }
            else
            {
                txt = "- ";
                color_text = DDefines.RedColorHex;
            }
            txt += ToNumberFormat(Mathf.Abs(value)) + suffix;
            if (color_it)
                return "<color=" + color_text + ">" + txt + "</color>";
            return txt;
        }
        public static string ToIncomeFormat(float value, bool color_it, string suffix, out Color color)
        {
            color = Color.white;
            bool sign = value >= 0;
            if (value == 0)
            {
                if (color_it)
                    color = Color.yellow;
                return "0,0" + suffix;
            }
            string txt;
            string color_text;
            if (sign)
            {
                txt = "+ ";
                color_text = DDefines.LightGreenColorHex;
            }
            else
            {
                txt = "- ";
                color_text = DDefines.RedColorHex;
            }
            txt += ToNumberFormat(Mathf.Abs(value)) + suffix;
            if (color_it)
            {
                var b = ColorUtility.TryParseHtmlString(color_text, out Color c);
                color = c;
                return "<color=" + color_text + ">" + txt + "</color>";
            }
            return txt;
        }
        #endregion

        #region Dics
        /// <summary>
        /// Converts a Dictionary into a (new) List containing only its Keys.
        /// </summary>
        /// <typeparam name="T">Key-type of the dictionary</typeparam>
        /// <typeparam name="Value"></typeparam>
        /// <param name="dic"></param>
        /// <returns>A List with all keys</returns>
        public static List<T> ToKeyList<T, Value>(this Dictionary<T, Value> dic)
        {
            List<T> list = new List<T>();
            foreach (var item in dic)
            {
                list.Add(item.Key);
            }
            return list;
        }
        /// <summary>
        /// Converts a Dictionary into a (new) Array containing only its Keys.
        ///  (Hint:Not efficient, it will create a list and then convert to a array)
        /// </summary>
        /// <typeparam name="T">Key-type of the dictionary</typeparam>
        /// <typeparam name="Value"></typeparam>
        /// <param name="dic"></param>
        /// <returns>A Array with all keys</returns>
        public static T[] ToKeyArray<T, Value>(this Dictionary<T, Value> dic)
        {
            return ToKeyList(dic).ToArray();
        }
        /// <summary>
        /// Converts a Dictionary into a (new) List containing only its Values.
        /// </summary>
        /// <typeparam name="T">Value-type of the dictionary</typeparam>
        /// <typeparam name="Value"></typeparam>
        /// <param name="dic"></param>
        /// <returns>A List with all values</returns>
        public static List<T> ToValueList<Key, T>(this Dictionary<Key, T> dic)
        {
            List<T> list = new List<T>();
            foreach (var item in dic)
            {
                list.Add(item.Value);
            }
            return list;
        }
        /// <summary>
        /// Converts a Dictionary into a (new) Array containing only its Values.
        ///  (Hint:Not efficient, it will create a list and then convert to a array)
        /// </summary>
        /// <typeparam name="T">Value-type of the dictionary</typeparam>
        /// <typeparam name="Value"></typeparam>
        /// <param name="dic"></param>
        /// <returns>A Array with all values</returns>
        public static T[] ToValueArray<Key, T>(this Dictionary<Key, T> dic)
        {
            return ToValueList(dic).ToArray();
        }
        #endregion

        #region Colors
        /// <summary>
        /// Changes and returns a color based on the given color and the alpha value.
        /// </summary>
        /// <param name="color">The base-color that should be modified in return</param>
        /// <param name="alpha">The new alpha-value of the color (range=0..1f)</param>
        /// <returns>The new color with the same rgb values as color but the updated alpha value</returns>
        public static Color ChangeAlpha(Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, Mathf.Min(1, Mathf.Max(0, alpha)));
        }
        /// <summary>
        /// Changes and returns a color based on the given color and a multiply value. It will be multiplied to all color-rgb's.
        /// </summary>
        /// <param name="color">The base-color that should be modified in return</param>
        /// <param name="value">The multiply value that will be multiplied to all rgb values (range=0..1f)</param>
        /// <returns>The new color with the multiplied rgb values as color</returns>
        public static Color Multiply(Color color, float value)
        {
            return new Color(color.r * value, color.g * value, color.b * value, color.a);
        }
        /// <summary>
        /// Will convert a Color value to a hexadecimal text.
        /// </summary>
        /// <param name="color"></param>
        /// <returns>Returns the hex-value of a given color with a '#' in front of the value</returns>
        public static string ToHex(Color32 color) => "#" + ColorUtility.ToHtmlStringRGB(color);
        /// <summary>
        /// Converts a hex to a Unity.Color in rgb (0.0>>1.0)
        /// </summary>
        /// <param name="hex">The coresponding hex value with a #</param>
        /// <returns></returns>
        public static Color ToColor(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color color))
                return color;
            return Color.white;
        }
        /// <summary>
        /// Returns a color that is red-yellow-green of its proportion to the given value. Between 0 and 0.5 it will lerp between red and yellow and 0.5 to 1.0 between yellow and green
        /// </summary>
        /// <param name="value">The ratio value in range 0.0 to 1.0</param>
        /// <returns>A Color that represents a lerped value red-yellow-green of value.</returns>
        public static Color RYGLerp(float value, float midvalue = 0.5f)
        {
            return CustomColorLerp(value, midvalue, DDefines.RedColor, DDefines.YellowColor, DDefines.GreenColor);
        }
        /// <summary>
        /// Lerps between two pairs of colors with given value at the midvalue.
        /// </summary>
        /// <param name="value">The lerp value ranging from 0.0 to 1.0</param>
        /// <param name="midvalue">The midpoint (0.0..1 to 0.99..9) from witch the other color will be chosen</param>
        /// <param name="c1">First Color for range 0.0 to midvalue</param>
        /// <param name="c2">Second Color</param>
        /// <param name="c3">Third Color for range midvalue to 1.0</param>
        /// <returns></returns>
        public static Color CustomColorLerp(float value, float midvalue, Color c1, Color c2, Color c3)
        {
            if (value > 1)
            {
                Debug.LogWarning("CustomColorLerp: Value is > 1");
                return c3;
            }
            if (value < 0)
            {
                Debug.LogWarning("CustomColorLerp: Value is < 0");
                return c1;
            }
            if (value <= midvalue)
                return Color.Lerp(c1, c2, value / midvalue);
            return Color.Lerp(c2, c3, (value * midvalue) * (1 / midvalue));
        }
        public static Color ColorLerp(float value, Color bestcolor, float midvalue = 0.5f)
        {
            return CustomColorLerp(value, midvalue, DDefines.RedColor, DDefines.YellowColor, bestcolor);
        }
        #endregion

        #region Hover deacitvation
        /// <summary>
        /// Activates/Deactivates the HoverScript-class if its on that gameobject that should be activated/deactivated.
        /// Only used if that object has a active hover effect while its been deactivated.
        ///     (Shortcut-Methode. Can be accessed inside of the specified class).
        /// </summary>
        /// <param name="game_object">The gameobject that has a HoverScript</param>
        /// <param name="value"></param>
        /// <param name="set_hover">Should the HoverScript be deactivated? (default=true)</param>
        public static void SetActive(this GameObject game_object, bool value, bool set_hover)
        {
            if (!set_hover)
            {
                game_object.SetActive(false);
                return;
            }
            game_object.SetActive(value);
            if (!value)
                DeativateHoverOn(game_object.transform);
        }
        /// <summary>
        /// It will deactivate all HoverScript active hovers on the gameobject and all of its children
        /// </summary>
        /// <param name="transform_child">The gameobject that should be deactivated (HoverScript).</param>
        private static void DeativateHoverOn(Transform transform_child)
        {
            //if (transform_child.GetComponent<HoverScript>() != null)
            //    transform_child.GetComponent<HoverScript>().DeactivateHover();
            //foreach (Transform transform in transform_child)
            //    DeativateHoverOn(transform);
        }
        #endregion

        /// <summary>
        /// Has the gameobject an inactive parent? In other words: Will the gameobject be visible?
        /// </summary>
        /// <param name="mono"></param>
        /// <returns>Returns true if it is not visible</returns>
        public static bool HasInactiveParent(this MonoBehaviour mono)
        {
            Transform parent = mono.transform.parent;
            while (parent != null)
            {
                if (!parent.gameObject.activeSelf)
                {
                    return true;
                }
                parent = parent.parent;
            }
            return false;
        }

        #region Clear Children
        /// <summary>
        /// Clears and deletes all Children of this Object. The destroy command will be independent of the current Frame. This is usefull if you want to calculate on deleted Objects parent while in the same frame; for example updating the height of a LayoutGroup after deleting some of its children.
        ///  For independents it will move all 'should-be-deleted-objects' to a special GameObject 'DEngine.TrashBucket'. There it will be deleted in the next frame.
        /// </summary>
        /// <param name="mono">The parent object from that all of its children should be destroyed</param>
        public static void Clear(this MonoBehaviour mono)
        {
            List<Transform> move_me = new List<Transform>();
            foreach (Transform item in mono.transform)
            {
                move_me.Add(item);
                UnityEngine.Object.Destroy(item.gameObject);
            }
            if (Engine.TrashBucket != null)
                move_me.ForEach((t) => t.SetParent(Engine.TrashBucket));
        }
        /// <summary>
        /// Clears and deletes all Children of this Object. The destroy command will be independent of the current Frame. This is usefull if you want to calculate on deleted Objects parent while in the same frame; for example updating the height of a LayoutGroup after deleting some of its children.
        ///  For independents it will move all 'should-be-deleted-objects' to a special GameObject 'DEngine.TrashBucket'. There it will be deleted in the next frame.
        /// </summary>
        /// <param name="transform">The parent object from that all of its children should be destroyed</param>
        /// <param name="exclude_ignore_layout">Should it ignore a child if its ignoring the layout?</param>
        public static void Clear(this Transform transform, bool exclude_ignore_layout = false)
        {
            List<Transform> move_me = new List<Transform>();
            foreach (Transform item in transform)
            {
                if (exclude_ignore_layout && item.GetComponent<LayoutElement>() != null && item.GetComponent<LayoutElement>().ignoreLayout)
                    continue;
                move_me.Add(item);
                UnityEngine.Object.Destroy(item.gameObject);
            }
            if (Engine.TrashBucket != null)
                move_me.ForEach((t) => t.SetParent(Engine.TrashBucket));
        }
        /// <summary>
        /// Clears and deletes all Children of this Object. The destroy command will be independent of the current Frame. This is usefull if you want to calculate on deleted Objects parent while in the same frame; for example updating the height of a LayoutGroup after deleting some of its children.
        ///  For independents it will move all 'should-be-deleted-objects' to a special GameObject 'DEngine.TrashBucket'. There it will be deleted in the next frame.
        /// Additionaly only MonoBehaviour T will be cleard.
        /// </summary>
        /// <param name="mono">The parent object from that all of its children should be destroyed</param>
        public static void Clear<T>(this MonoBehaviour mono) where T : MonoBehaviour
        {
            List<Transform> move_me = new List<Transform>();
            foreach (Transform item in mono.transform)
            {
                if (item.GetComponent<T>() is T t)
                {
                    move_me.Add(item);
                    UnityEngine.Object.Destroy(item.gameObject);
                }
            }
            if (Engine.TrashBucket != null)
                move_me.ForEach((t) => t.SetParent(Engine.TrashBucket));
        }
        /// <summary>
        /// Clears and deletes all Children of this Object. The destroy command will be independent of the current Frame. This is usefull if you want to calculate on deleted Objects parent while in the same frame; for example updating the height of a LayoutGroup after deleting some of its children.
        ///  For independents it will move all 'should-be-deleted-objects' to a special GameObject 'DEngine.TrashBucket'. There it will be deleted in the next frame.
        /// 
        /// Only clears objects that NOT have the given tag.
        /// </summary>
        /// <param name="mono">The parent object from that all of its children should be destroyed</param>
        /// <param name="exception_tag">The non-matching tag of objects that will be deleted</param>
        public static void Clear(this MonoBehaviour mono, string exception_tag)
        {
            List<Transform> move_me = new List<Transform>();
            foreach (Transform item in mono.transform)
            {
                if (item.CompareTag(exception_tag))
                    continue;
                move_me.Add(item);
                UnityEngine.Object.Destroy(item.gameObject);
            }
            if (Engine.TrashBucket != null)
                move_me.ForEach((t) => t.SetParent(Engine.TrashBucket));
        }
        #endregion

        #region World 3D
        //private static Dictionary<MonoBehaviour, float> SmoothRotDic = new();
        //public static void SmoothRotation(this MonoBehaviour mono, float target_angle, float turn_time = 0.1f)
        //{
        //    float velocity = 0;
        //    if (SmoothRotDic.ContainsKey(mono))
        //    {
        //        velocity = SmoothRotDic[mono];
        //    }
        //    else
        //    {
        //        SmoothRotDic.Add(mono, velocity);
        //    }
        //    float angle = Mathf.SmoothDampAngle(mono.transform.eulerAngles.y, target_angle, ref velocity, turn_time);
        //    SmoothRotDic[mono] = velocity;
        //    mono.transform.rotation = Quaternion.Euler(new Vector3(0, angle, 0));
        //}
        public static Vector3 AngleTo2DPosition(float angle)
        {
            angle *= Mathf.Deg2Rad;
            float x = Mathf.Cos(angle);
            float y = Mathf.Sin(angle);
            return new Vector3(x, 0, -y);
        }
        public static Vector3 AngleTo2DPosition(Transform transform)
        {
            return AngleTo2DPosition(transform.rotation.eulerAngles.y - 90f);
        }
        public static Vector3 AngleTo2DPosition(GameObject game_object)
        {
            return AngleTo2DPosition(game_object.transform.rotation.eulerAngles.y - 90f);
        }
        public static Vector3 AngleTo2DPosition(MonoBehaviour game_object)
        {
            return AngleTo2DPosition(game_object.transform.rotation.eulerAngles.y - 90);
        }

        /// <summary>
        /// Divides componentvice the Vectors v and w and returns the result in a new vector.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="w"></param>
        /// <returns>Returns the component-multiplications of the input vectors</returns>
        public static Vector3 ScalarProduct(Vector3 v, Vector3 w) => new Vector3(v.x * w.x, v.y * w.y, v.z * w.z);
        public static Vector3 ComponentMultiplication(Vector3 v, Vector3 w) => ScalarProduct(v, w);
        public static Vector3 ScalarDivision(Vector3 v, Vector3 w) => new Vector3(v.x / w.x, v.y / w.y, v.z / w.z);
        public static Vector3 ComponentDivision(Vector3 v, Vector3 w) => ScalarDivision(v, w);
        #endregion

        #region Scroll
        /// <summary>
        /// Will scroll a ScrollRect to the top of all its content
        /// </summary>
        /// <param name="scrollRect"></param>
        public static void ScrollToTop(this ScrollRect scrollRect)
        {
            scrollRect.normalizedPosition = new Vector2(0, 1);
        }
        /// <summary>
        /// Will scroll a ScrollRect to the top of all its content
        /// </summary>
        /// <param name="scrollRect"></param>
        public static void ScrollToBottom(this ScrollRect scrollRect)
        {
            scrollRect.normalizedPosition = new Vector2(0, 0);
        }
        #endregion

        #region Layers
        public static void SetLayer(this GameObject gm, string layer)
        {
            SetLayer(gm, LayerMask.NameToLayer(layer));
        }
        public static void SetLayer(this GameObject gm, int layer)
        {
            gm.layer = layer;
            foreach (Transform child in gm.transform)
            {
                child.gameObject.layer = layer;

                Transform _HasChildren = child.GetComponentInChildren<Transform>();
                if (_HasChildren != null)
                    SetLayer(child.gameObject, layer);

            }
        }
        #endregion
    }

    public static class ListExtension
    {
        public static T[] GetNextItems<T>(this List<T> list, int length)
        {
            T[] local = new T[Math.Min(length, list.Count)];
            for (int i = 0; i < local.Length; i++)
                local[i] = list[i];
            list.RemoveRange(0, local.Length);
            return local;
        }
    }
}