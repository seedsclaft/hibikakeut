using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ariadne
{
    /// <Summary>
    /// Drawing dungeon walls.
    /// </Summary>
    public class DrawDungeonWall : DrawDungeon, IDrawer
    {
        protected float centerHeight;
        protected float groundHeight;
        protected float ceilHeight;
        protected Dictionary<string, GameObject> axisObjDict = new Dictionary<string, GameObject>();

        /// <Summary>
        /// Instantiate dungeon walls and objects according to map attribute.
        /// </Summary>
        protected virtual void DrawWalls()
        {
            Vector3 basePos = Vector3.zero;

            GameObject groundPrefab = dungeonBasePartsData.groundObj;
            Vector3 groundSize = new Vector3(groundPrefab.transform.localScale.x, groundPrefab.transform.localScale.y, groundPrefab.transform.localScale.z);

            GameObject sizePrefab = dungeonBasePartsData.sizeBaseObj;
            Vector3 unitSize = new Vector3(sizePrefab.transform.localScale.x, sizePrefab.transform.localScale.y, sizePrefab.transform.localScale.z);
            centerHeight = (groundSize.y + unitSize.y) / 2;
            groundHeight = groundSize.y / 2;
            ceilHeight = groundHeight + unitSize.y;

            Direction direction = new Direction();
            // Instanciate parts on X-Z plane.
            for (int zAxis = 0; zAxis < currentFloorMapData.floorSizeVertical; zAxis++)
            {
                for (int xAxis = 0; xAxis < currentFloorMapData.floorSizeHorizontal; xAxis++)
                {
                    int index = xAxis + zAxis * currentFloorMapData.floorSizeHorizontal;
                    int mapAttrId = currentFloorMapData.mapInfo[index].mapAttr;
                    int objectTypeId = currentFloorMapData.mapInfo[index].objectTypeId;
                    Vector3 posInLoop = new Vector3(basePos.x + xAxis * unitSize.x, basePos.y + centerHeight, basePos.z + zAxis * unitSize.z);

                    MapAttributeRecord record = DataRecordUtil.GetMapAttributeRecordById(mapAttributeDataList, mapAttrId);
                    string mapAttributeName = record == null ? "" : record.attributeName;
                    string axisKey = xAxis.ToString() + AriadneSystemUseName.AxisConnecter + zAxis.ToString();
                    string objName = mapAttributeName + "_" + axisKey;
                    Vector3 rotation = direction.GetRotationOfDirection(currentFloorMapData.mapInfo[index].objectFront);
                    InstantiateWall(mapAttrId, posInLoop, axisKey, objName, rotation, objectTypeId, index);
                }
            }

            // Instanciate outside walls
            if (drawOutsideWall)
            {
                // Horizontal walls - South
                string outsideObjName = "OutsideWall_South_Horizontal";
                Vector3 scale = new Vector3(unitSize.x * (currentFloorMapData.floorSizeHorizontal + outsideWallSize * 2), unitSize.y, unitSize.z * outsideWallSize);
                float outSideWallOffsetX = (1 + outsideWallSize) * 0.5f * unitSize.x;
                float outSideWallOffsetZ = (1 + outsideWallSize) * 0.5f * unitSize.z;
                Vector3 baseOffset = new Vector3(unitSize.x / 2, 0f, unitSize.z / 2);
                float posX = baseOffset.x + scale.x / 2 - outSideWallOffsetX * 2;
                float posZ = -1 * outSideWallOffsetZ;
                Vector3 pos = new Vector3(posX, centerHeight, posZ);
                InstantiateOutsideWalls(pos, scale, outsideObjName);

                // Horizontal walls - North
                outsideObjName = "OutsideWall_North_Horizontal";
                posZ = (currentFloorMapData.floorSizeVertical - 1) * unitSize.z + outSideWallOffsetZ;
                pos = new Vector3(posX, centerHeight, posZ);
                InstantiateOutsideWalls(pos, scale, outsideObjName);

                // Vertical walls - West
                outsideObjName = "OutsideWall_West_Vertial";
                scale = new Vector3(unitSize.x * outsideWallSize, unitSize.y, unitSize.z * currentFloorMapData.floorSizeVertical);
                posX = -1 * outSideWallOffsetX;
                posZ = baseOffset.z + (scale.z + outsideWallSize * 2 * unitSize.z) / 2 - outSideWallOffsetZ * 2;
                pos = new Vector3(posX, centerHeight, posZ);
                InstantiateOutsideWalls(pos, scale, outsideObjName);

                // Vertical walls - East
                outsideObjName = "OutsideWall_East_Vertial";
                posX = (currentFloorMapData.floorSizeHorizontal - 1) * unitSize.x + outSideWallOffsetX;
                pos = new Vector3(posX, centerHeight, posZ);
                InstantiateOutsideWalls(pos, scale, outsideObjName);
            }
        }

        /// <Summary>
        /// Receiver of OnDraw message.
        /// </Summary>
        public virtual void OnDraw()
        {
            base.GetSettings();
            DrawWalls();
        }

        /// <Summary>
        /// Receiver of OnRedraw message.
        /// Before instantiate new ceiling object, this method calls RemoveChildObjects() method.
        /// </Summary>
        public virtual void OnRedraw()
        {
            base.GetSettings();
            base.RemoveChildObjects();
            DrawWalls();
        }

        /// <Summary>
        /// Remove dungeon walls.
        /// </Summary>
        public virtual void OnRemoveObjects()
        {
            base.RemoveChildObjects();
            InitializeAxisObjDict();
        }

        /// <Summary>
        /// Initialize object dictionary corresponds to axis. 
        /// </Summary>
        protected virtual void InitializeAxisObjDict()
        {
            axisObjDict = new Dictionary<string, GameObject>();
        }

        /// <Summary>
        /// Instantiate dungeon walls and objects.
        /// </Summary>
        /// <param name="mapAttrId">The map attribute id of the position.</param>
        /// <param name="pos">The position of the object.</param>
        /// <param name="axisKey">The axis string of the object.</param>
        /// <param name="objName">The name of the object.</param>
        /// <param name="rotation">The rotation of the object.</param>
        /// <param name="objectTypeId">Object type ID in DungeonPartsData.</param>
        /// <param name="index">Index of the position in MapInfo list.</param>
        protected virtual void InstantiateWall(int mapAttrId, Vector3 pos, string axisKey, string objName, Vector3 rotation, int objectTypeId, int index)
        {
            if (dungeonPartsDataList == null)
            {
                return;
            }

            // Return when DungeonPartsData has no prefabs corresponds to mapAttrId.
            DungeonPartsRecord record = DataRecordUtil.GetDungeonPartsRecordById(dungeonPartsDataList, mapAttrId, objectTypeId);
            if (record == null)
            {
                return;
            }

            GameObject prefab = record.partsObject;
            if (prefab == null)
            {
                return;
            }

            if (axisObjDict.ContainsKey(axisKey))
            {
                return;
            }

            float heightOffset = 0f;
            switch (record.heightAnchor)
            {
                case PartsHeightAnchor.Center:
                    heightOffset = centerHeight;
                    break;
                case PartsHeightAnchor.Ground:
                    heightOffset = groundHeight;
                    break;
                case PartsHeightAnchor.Ceiling:
                    heightOffset = ceilHeight;
                    break;
            }
            pos.y = heightOffset;

            GameObject wall = (GameObject) Instantiate(prefab, pos, Quaternion.identity);
            wall.transform.SetParent(gameObject.transform);
            wall.transform.Rotate(rotation);
            wall.name = objName;

            // Add a new object to the object dictionary.
            if (axisObjDict.ContainsKey(axisKey))
            {
                axisObjDict[axisKey] = wall;
            }
            else
            {
                axisObjDict.Add(axisKey, wall);
            }
        }

        /// <Summary>
        /// Instantiate dungeon outside walls.
        /// </Summary>
        /// <param name="pos">The position of the wall.</param>
        /// <param name="scale">The scale of the wall.</param>
        /// <param name="objName">The name of the wall.</param>
        protected virtual void InstantiateOutsideWalls(Vector3 pos, Vector3 scale, string objName)
        {
            GameObject prefab = dungeonBasePartsData.outsideWallObj;
            GameObject wall = (GameObject) Instantiate(prefab, pos, Quaternion.identity);
            wall.transform.SetParent(gameObject.transform);
            wall.transform.localScale = scale;
            wall.name = objName;

            int tile = 2;
            int texScaleX = tile * outsideWallSize;
            int texScaleZ = tile;
            if (scale.x >= scale.z)
            {
                texScaleX = (currentFloorMapData.floorSizeHorizontal + outsideWallSize * 2) * tile;
            }
            else
            {
                texScaleX = currentFloorMapData.floorSizeVertical * tile;
            }
            wall.GetComponent<Renderer>().material.mainTextureScale = new Vector2(texScaleX, texScaleZ);
        }

        /// <Summary>
        /// Returns an object which corresponds to arg axis.
        /// </Summary>
        /// <param name="xAxis">X-axis on the map position.</param>
        /// <param name="zAxis">Z-axis on the map position.</param>
        public virtual GameObject GetWallObjectByAxis(int xAxis, int zAxis)
        {
            string axisKey = xAxis.ToString() + AriadneSystemUseName.AxisConnecter + zAxis.ToString();
            if (axisObjDict.ContainsKey(axisKey))
            {
                return axisObjDict[axisKey];
            }
            else
            {
                return null;
            }
        }
    }
}
