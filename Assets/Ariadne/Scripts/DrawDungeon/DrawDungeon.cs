using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ariadne
{
    /// <Summary>
    /// Base class of drawing dungeon classes.
    /// </Summary>
    public class DrawDungeon : AriadneSystemBase
    {
        protected FloorMapMasterData currentFloorMapData;
        protected GameObject gameController;
        protected DungeonMasterData dungeonData;
        protected DungeonBasePartsData dungeonBasePartsData;
        protected List<DungeonPartsData> dungeonPartsDataList;
        protected List<MapAttributeData> mapAttributeDataList;
        protected bool drawOutsideWall;
        protected int outsideWallSize;

        /// <Summary>
        /// Get settings such as dungeon data from DungeonSettings class.
        /// </Summary>
        protected virtual void GetSettings()
        {
            gameController = GameObject.FindGameObjectWithTag(AriadneSceneObjectTag.GameController);
            DungeonSettings ds = gameController.GetComponent<DungeonSettings>();
            dungeonData = ds.dungeonData;
            currentFloorMapData = ds.GetCurrentFloorData();
            dungeonBasePartsData = currentFloorMapData.dungeonBasePartsData;
            dungeonPartsDataList = ds.GetDungeonPartsDataList();
            mapAttributeDataList = ds.GetMapAttributeList();
            
            if (dungeonPartsDataList == null)
            {
                Debug.LogError("DungeonPartsData is missing. Check the DungeonPartsData setting in your FloorData.");
            }

            drawOutsideWall = ds.drawOutsideWall;
            outsideWallSize = ds.outsideWallSize;
        }

        /// <Summary>
        /// Remove child objects of the parent of dungeon parts.
        /// </Summary>
        protected virtual void RemoveChildObjects()
        {
            foreach (Transform child in gameObject.transform)
            {
                GameObject childObj = child.gameObject;
                Destroy(childObj);
            }
        }
    }
}
