using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Ariadne
{
    public static class FloorMapConst
    {
        public const int InitHorizontalSize = 6;
        public const int InitVerticalSize = 6;
    }

    /// <Summary>
    /// Definition of each position data.
    /// </Summary>
    [System.Serializable]
    public class MapInfo
    {
        public int eventId;
        public int mapAttr;
        public DungeonDir objectFront;
        public int regeonNo;
        public int objectTypeId;
    }

    /// <Summary>
    /// Definition of each floor data.
    /// </Summary>
    public class FloorMapMasterData : ScriptableObject
    {
        public int floorId;
        public string floorName;
        [Obsolete("Use dungeonPartsData instead.")]
        public DungeonPartsMasterData dungeonParts;
        public DungeonBasePartsData dungeonBasePartsData;
        public List<DungeonPartsData> dungeonPartsDataList;
        public List<MapAttributeData> mapAttributeDataList;
        public int floorSizeHorizontal;
        public int floorSizeVertical;
        public Vector2Int entrancePos;
        public DungeonDir enteringDir;
        public List<MapInfo> mapInfo;
        public int DungeonCompletion;
        public List<Ryneus.StageEventData> stageEvents;
    }
}