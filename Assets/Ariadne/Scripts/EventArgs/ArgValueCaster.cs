using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Ariadne
{
    /// <Summary>
    /// Utility class for casting event args.
    /// </Summary>
    public static class ArgValueCaster
    {
        /// <Summary>
        /// Returns a Vector2 value from string.
        /// </Summary>
        /// <param name="rString">String value convert to Vector2.</param>
        public static Vector2 GetVector2Value(string rString)
        {
            string warnText = "Casting to 'Vector2' has been failed. String value : " + rString;

            string valueStr = GetBracketsRemovedString(rString);
            string[] temp = valueStr.Split(',');

            float x = 0f;
            bool success = float.TryParse(temp[0], out x);
            if (!success)
            {
                Debug.LogWarning(warnText);
            }

            float y = 0f;
            success = float.TryParse(temp[1], out y);
            if (!success)
            {
                Debug.LogWarning(warnText);
            }

            Vector2 rValue = new Vector2(x, y);
            return rValue;
        }

        /// <Summary>
        /// Returns a Vector2Int value from string.
        /// </Summary>
        /// <param name="rString">String value convert to Vector2Int.</param>
        public static Vector2Int GetVector2IntValue(string rString)
        {
            string warnText = "Casting to 'Vector2Int' has been failed. String value : " + rString;

            string valueStr = GetBracketsRemovedString(rString);
            string[] temp = valueStr.Split(',');

            int x = 0;
            bool success = int.TryParse(temp[0], out x);
            if (!success)
            {
                Debug.LogWarning(warnText);
            }

            int y = 0;
            success = int.TryParse(temp[1], out y);
            if (!success)
            {
                Debug.LogWarning(warnText);
            }

            Vector2Int rValue = new Vector2Int(x, y);
            return rValue;
        }

        /// <Summary>
        /// Returns a Vector3 value from string.
        /// </Summary>
        /// <param name="rString">String value convert to Vector3.</param>
        public static Vector3 GetVector3Value(string rString)
        {
            string warnText = "Casting to 'Vector3' has been failed. String value : " + rString;
            
            string valueStr = GetBracketsRemovedString(rString);
            string[] temp = valueStr.Split(',');

            float x = 0f;
            bool success = float.TryParse(temp[0], out x);
            if (!success)
            {
                Debug.LogWarning(warnText);
            }

            float y = 0f;
            success = float.TryParse(temp[1], out y);
            if (!success)
            {
                Debug.LogWarning(warnText);
            }

            float z = 0f;
            success = float.TryParse(temp[2], out z);
            if (!success)
            {
                Debug.LogWarning(warnText);
            }

            Vector3 rValue = new Vector3(x, y, z);
            return rValue;
        }

        /// <Summary>
        /// Returns a Vector3Int value from string.
        /// </Summary>
        /// <param name="rString">String value convert to Vector3Int.</param>
        public static Vector3Int GetVector3IntValue(string rString)
        {
            string warnText = "Casting to 'Vector3Int' has been failed. String value : " + rString;
            
            string valueStr = GetBracketsRemovedString(rString);
            string[] temp = valueStr.Split(',');

            int x = 0;
            bool success = int.TryParse(temp[0], out x);
            if (!success)
            {
                Debug.LogWarning(warnText);
            }

            int y = 0;
            success = int.TryParse(temp[1], out y);
            if (!success)
            {
                Debug.LogWarning(warnText);
            }

            int z = 0;
            success = int.TryParse(temp[2], out z);
            if (!success)
            {
                Debug.LogWarning(warnText);
            }

            Vector3Int rValue = new Vector3Int(x, y, z);
            return rValue;
        }

        /// <Summary>
        /// Returns a Vector4 value from string.
        /// </Summary>
        /// <param name="rString">String value convert to Vector4.</param>
        public static Vector4 GetVector4Value(string rString)
        {
            string warnText = "Casting to 'Vector4' has been failed. String value : " + rString;
            
            string valueStr = GetBracketsRemovedString(rString);
            string[] temp = valueStr.Split(',');

            float x = 0f;
            bool success = float.TryParse(temp[0], out x);
            if (!success)
            {
                Debug.LogWarning(warnText);
            }

            float y = 0f;
            success = float.TryParse(temp[1], out y);
            if (!success)
            {
                Debug.LogWarning(warnText);
            }

            float z = 0f;
            success = float.TryParse(temp[2], out z);
            if (!success)
            {
                Debug.LogWarning(warnText);
            }

            float w = 0f;
            success = float.TryParse(temp[3], out w);
            if (!success)
            {
                Debug.LogWarning(warnText);
            }

            Vector4 rValue = new Vector4(x, y, z, w);
            return rValue;
        }

        /// <Summary>
        /// Returns a Color value from string.
        /// </Summary>
        /// <param name="rString">String value convert to Color.</param>
        public static Color GetColorValue(string rString)
        {
            string warnText = "Casting to 'Color' has been failed. String value : " + rString;
            
            string valueStr = GetColorBracketsRemovedString(rString);
            string[] temp = valueStr.Split(',');

            float x = 0f;
            bool success = float.TryParse(temp[0], out x);
            if (!success)
            {
                Debug.LogWarning(warnText);
            }

            float y = 0f;
            success = float.TryParse(temp[1], out y);
            if (!success)
            {
                Debug.LogWarning(warnText);
            }

            float z = 0f;
            success = float.TryParse(temp[2], out z);
            if (!success)
            {
                Debug.LogWarning(warnText);
            }

            float w = 0f;
            success = float.TryParse(temp[3], out w);
            if (!success)
            {
                Debug.LogWarning(warnText);
            }

            Color rValue = new Color(x, y, z, w);
            return rValue;
        }

        /// <Summary>
        /// Returns a Enum value from string.
        /// </Summary>
        /// <param name="rString">String value convert to Enum.</param>
        public static bool GetTryParseEnum<TEnum>(string rString, out TEnum enumValue) where TEnum : struct
        {
            enumValue = default(TEnum);
            bool success = false;

            string[] enumNames = Enum.GetNames(typeof(TEnum));
            foreach (string enumName in enumNames)
            {
                if (enumName != rString)
                {
                    continue;
                }

                enumValue = (TEnum)Enum.Parse(typeof(TEnum), enumName);
                success = true;
            }

            return success;
        }

        /// <Summary>
        /// Returns a string value without brackets.
        /// </Summary>
        /// <param name="valueStr">String value to remove brakets.</param>
        static string GetBracketsRemovedString(string valueStr)
        {
            string str = valueStr.Replace("(", "");
            str = str.Replace(")", "");
            return str;
        }

        /// <Summary>
        /// Returns a string value without brackets and color names.
        /// </Summary>
        /// <param name="valueStr">String value to remove brakets.</param>
        static string GetColorBracketsRemovedString(string valueStr)
        {
            string str = valueStr.Replace("(", "");
            str = str.Replace(")", "");
            str = str.Replace("RGBA", "");
            return str;
        }
    }
}