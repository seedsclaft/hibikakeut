using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Ariadne
{
    /// <Summary>
    /// Definitions of map attribute.
    /// </Summary>
    [Obsolete("Use MapAttributeData to define attributes instead.")]
    public static class MapAttributeDefine
    {
        public const int HallWay = 0;
        public const int Wall = 1;
        public const int Door = 2;
        public const int LockedDoor = 3;
        public const int DownStairs = 4;
        public const int Upstairs = 5;
        public const int Treasure = 6;
        public const int Messenger = 7;
        public const int Pillar = 8;
        public const int WallWithToach = 9;

        /// <Summary>
        /// Definitions of map attribute.
        /// </Summary>
        /// <param name="attrId">Specify map attribute ID.</param>
        [Obsolete("Use attributeName in MapAttributeRecord instead.")]
        public static string GetAttrNameById(int attrId)
        {
            string attrName = "";
            switch (attrId)
            {
                case Wall:
                    attrName = "Wall";
                    break;
                case Door:
                    attrName = "Door";
                    break;
                case LockedDoor:
                    attrName = "LockedDoor";
                    break;
                case DownStairs:
                    attrName = "Downstairs";
                    break;
                case Upstairs:
                    attrName = "Upstairs";
                    break;
                case Treasure:
                    attrName = "Treasure";
                    break;
                case Messenger:
                    attrName = "Messenger";
                    break;
                case Pillar:
                    attrName = "Pillar";
                    break;
                case WallWithToach:
                    attrName = "WallWithTorch";
                    break;
                default:
                    attrName = "Hallway";
                    break;
            }
            return attrName;
        }
    }
}