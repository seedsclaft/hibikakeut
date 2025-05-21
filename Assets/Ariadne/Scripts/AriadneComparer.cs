using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ariadne
{
    /// <Summary>
    /// Compare values which are used in events.
    /// </Summary>
    public static class AriadneComparer
    {
        /// <Summary>
        /// Returns the result of comparing int value.
        /// </Summary>
        /// <param name="comparison">A comparison operator of the event parts.</param>
        /// <param name="value">A target value to compare.</param>
        /// <param name="compareNum">A criteria value.</param>
        public static bool GetComparedResult(AriadneComparison comparison, int value, int compareNum)
        {
            bool isMatched = false;
            switch (comparison)
            {
                case AriadneComparison.Equals:
                    if (value == compareNum)
                    {
                        isMatched = true;
                    }
                    break;
                case AriadneComparison.NotEqual:
                    if (value != compareNum)
                    {
                        isMatched = true;
                    }
                    break;
                case AriadneComparison.GreaterThan:
                    if (value > compareNum)
                    {
                        isMatched = true;
                    }
                    break;
                case AriadneComparison.GreaterOrEqual:
                    if (value >= compareNum)
                    {
                        isMatched = true;
                    }
                    break;
                case AriadneComparison.LessThan:
                    if (value < compareNum)
                    {
                        isMatched = true;
                    }
                    break;
                case AriadneComparison.LessOrEqual:
                    if (value <= compareNum)
                    {
                        isMatched = true;
                    }
                    break;
            }
            return isMatched;
        }

        /// <Summary>
        /// Returns the result of comparing float value.
        /// </Summary>
        /// <param name="comparison">A comparison operator of the event parts.</param>
        /// <param name="value">A target value to compare.</param>
        /// <param name="compareNum">A criteria value.</param>
        public static bool GetComparedResult(AriadneComparison comparison, float value, float compareNum)
        {
            bool isMatched = false;
            switch (comparison)
            {
                case AriadneComparison.Equals:
                    if (value == compareNum)
                    {
                        isMatched = true;
                    }
                    break;
                case AriadneComparison.NotEqual:
                    if (value != compareNum)
                    {
                        isMatched = true;
                    }
                    break;
                case AriadneComparison.GreaterThan:
                    if (value > compareNum)
                    {
                        isMatched = true;
                    }
                    break;
                case AriadneComparison.GreaterOrEqual:
                    if (value >= compareNum)
                    {
                        isMatched = true;
                    }
                    break;
                case AriadneComparison.LessThan:
                    if (value < compareNum)
                    {
                        isMatched = true;
                    }
                    break;
                case AriadneComparison.LessOrEqual:
                    if (value <= compareNum)
                    {
                        isMatched = true;
                    }
                    break;
            }
            return isMatched;
        }
    }
}