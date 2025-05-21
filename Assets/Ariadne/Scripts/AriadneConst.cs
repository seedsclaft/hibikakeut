using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ariadne
{
    public class AriadneSceneObjectName
    {
        public static readonly string EventFlagManager = "AriadneEventFlagManager";
        public static readonly string ItemDataManager = "AriadneItemDataManager";
        public static readonly string MoveController = "AriadneMoveController";
        public static readonly string MapParent = "MapParent";
        public static readonly string GroundParent = "GroundParent";
        public static readonly string WallParent = "WallParent";
        public static readonly string CeilingParent = "CeilingParent";
    }

    public class AriadneSceneObjectTag
    {
        public static readonly string GameController = "GameController";
        public static readonly string Player = "Player";
    }

    public class AriadneMapPartsName
    {
        public static readonly string MapBackground = "MapBackground";
        public static readonly string MapBase = "MapBase";
        public static readonly string MapHall = "MapHall";
        public static readonly string MapIcon = "MapIcon";
        public static readonly string MapGrid = "MapGrid";
    }

    public class AriadneSystemUseName
    {
        public static readonly string AxisConnecter = "-";
    }

    public class AriadneMenuOrder
    {
        public const int DungeonData = 100;
        public const int DungeonBasePartsData = 110;
        public const int DungeonPartsData = 111;
        public const int EventArgumentTypeData = 200;
        public const int EventCategoryData = 201;
        public const int EventDataList = 202;
        public const int ItemData = 300;
        public const int ItemDataList = 301;
        public const int MapAttributeData = 400;
    }
}