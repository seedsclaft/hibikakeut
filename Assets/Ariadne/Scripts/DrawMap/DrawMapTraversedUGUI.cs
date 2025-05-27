using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ariadne
{
    /// <Summary>
    /// Draw traversed position on the map.
    /// Traversed positions are drawn as meshes.
    /// </Summary>
    public class DrawMapTraversedUGUI : MaskableGraphic, IDirtyMarkerMap
    {
        protected int showLengthHorizontal = 4;
        protected int showLengthVertical = 4;
        protected int drawSmoothness = 2;

        protected DungeonMasterData dungeonData;
        protected FloorMapMasterData floorMapData;
        protected List<MapAttributeData> mapAttributeDataList;

        protected RectTransform parentRt;
        protected RectTransform mapRt;
        protected bool enableAutoMapping = true;
        protected Vector2 posLerp = Vector2.zero;
        protected GameObject gameController;
        protected DungeonSettings dungeonSettings;
        protected MapShowingSettings mapSettings;

        protected readonly Vector2 leftBottomUv = new Vector2(0f, 0f);
        protected readonly Vector2 leftTopUv = new Vector2(0f, 1f);
        protected readonly Vector2 rightTopUv = new Vector2(1f, 1f);
        protected readonly Vector2 rightBottomUv = new Vector2(1f, 0f);

        /// <Summary>
        /// Populate meshes.
        /// This method is called when the canvas is updated.
        /// </Summary>
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            GetFloorData();
            GetMapSettings();
            DrawMap(vh);
        }

        /// <Summary>
        /// Get floor data from the DungeonSettings component.
        /// </Summary>
        protected virtual void GetFloorData()
        {
            if (gameController == null)
            {
                gameController = GameObject.FindGameObjectWithTag(AriadneSceneObjectTag.GameController);
            }

            if (dungeonSettings == null)
            {
                dungeonSettings = gameController.GetComponent<DungeonSettings>();
            }

            dungeonData = dungeonSettings.dungeonData;
            floorMapData = dungeonSettings.GetCurrentFloorData();
            mapAttributeDataList = dungeonSettings.GetMapAttributeList();
        }

        /// <Summary>
        /// Get settings about map from the MapShowingSettings component.
        /// </Summary>
        protected virtual void GetMapSettings()
        {
            if (gameController == null)
            {
                gameController = GameObject.FindGameObjectWithTag(AriadneSceneObjectTag.GameController);
            }

            if (mapSettings == null)
            {
                mapSettings = gameController.GetComponent<MapShowingSettings>();
            }

            this.showLengthHorizontal = mapSettings.showLengthHorizontal;
            this.showLengthVertical = mapSettings.showLengthVertical;
            this.drawSmoothness = mapSettings.smoothness;
            this.enableAutoMapping = mapSettings.enableAutoMapping;
        }

        /// <Summary>
        /// Generates meshes of traversed position.
        /// If "enableAutoMapping" is false, all hallways in the map are shown on the map.
        /// </Summary>
        protected virtual void DrawMap(VertexHelper vh)
        {
            if (mapRt == null)
            {
                mapRt = gameObject.GetComponent<RectTransform>();
            }
            Vector3 centerPos = mapRt.transform.localPosition;

            float cellWidth = mapRt.sizeDelta.x / (1 + showLengthHorizontal * 2);
            float cellHeight = mapRt.sizeDelta.y / (1 + showLengthVertical * 2);
            float widthOffset = cellWidth / 2;
            float heightOffset = cellHeight / 2;

            if (parentRt == null)
            {
                parentRt = GameObject.Find(AriadneSceneObjectName.MapParent).GetComponent<RectTransform>();
            }

            Vector2 parentPos = parentRt.transform.localPosition;
            float parentWidth = parentRt.sizeDelta.x;
            float parentHeight = parentRt.sizeDelta.y;

            Rect r = mapRt.rect;
            r.x += parentPos.x + centerPos.x;
            r.y += parentPos.y + centerPos.y;
            SetClipRect(r, true);

            int signX = centerPos.x < 0 ? -1 : 1;
            int signY = centerPos.y < 0 ? -1 : 1;

            Vector2 unitSize = new Vector2(cellWidth, cellHeight);
            float basePosX = parentWidth / 2 - signX * centerPos.x - widthOffset - posLerp.x * unitSize.x;
            float basePosY = parentHeight / 2 - signY * centerPos.y - heightOffset - posLerp.y * unitSize.y;
            Vector2 basePos = new Vector2(basePosX, basePosY);

            for (int yAxis = 0; yAxis < floorMapData.floorSizeVertical; yAxis++)
            {
                for (int xAxis = 0; xAxis < floorMapData.floorSizeHorizontal; xAxis++)
                {
                    int index = xAxis + yAxis * floorMapData.floorSizeHorizontal;
                    Vector3 basePosInLoop = new Vector3(basePos.x + unitSize.x * xAxis, basePos.y + unitSize.y * yAxis);
                    
                    Vector2Int pos = new Vector2Int(xAxis, yAxis);

                    bool traversed = TraverseManager.Instance.GetPositionTraverseData(dungeonData.dungeonId, floorMapData.floorId, pos);

                    if (enableAutoMapping)
                    {
                        if (!traversed)
                        {
                            continue;
                        }
                    }

                    int mapAttrId = floorMapData.mapInfo[index].mapAttr;
                    MapAttributeRecord record = DataRecordUtil.GetMapAttributeRecordById(mapAttributeDataList, mapAttrId);
                    if (record == null)
                    {
                        continue;
                    }

                    if (!record.drawAsWallOnMap)
                    {
                        PositionSet posSet = GetPositionSet(basePosInLoop, unitSize);
                        DrawMTraversedPos(vh, posSet);
                    }
                }
            }
        }

        /// <Summary>
        /// Generate meshes.
        /// </Summary>
        protected virtual void DrawMTraversedPos(VertexHelper vh, PositionSet pos)
        {
            // Left bottom
            UIVertex leftBottom = UIVertex.simpleVert;
            leftBottom.position = pos.bottomLeft;
            leftBottom.uv0 = leftBottomUv;

            // Left Top
            UIVertex leftTop = UIVertex.simpleVert;
            leftTop.position = pos.topLeft;
            leftTop.uv0 = leftTopUv;

            // Right Top
            UIVertex rightTop = UIVertex.simpleVert;
            rightTop.position = pos.topRight;
            rightTop.uv0 = rightTopUv;

            // Right bottom
            UIVertex rightBottom = UIVertex.simpleVert;
            rightBottom.position = pos.bottomRight;
            rightBottom.uv0 = rightBottomUv;

            vh.AddUIVertexQuad(new UIVertex[]
            {
                leftBottom, rightBottom, rightTop, leftTop
            });
        }

        /// <Summary>
        /// Returns Ariadne.PositionSet data to define the position of the drawing mesh.
        /// </Summary>
        protected virtual PositionSet GetPositionSet(Vector3 basePosInLoop, Vector2 unitSize)
        {
            PositionSet posSet = new PositionSet();
            posSet.bottomLeft = new Vector3(basePosInLoop.x, basePosInLoop.y, 0f);
            posSet.topLeft = new Vector3(basePosInLoop.x, basePosInLoop.y + unitSize.y, 0f);
            posSet.topRight = new Vector3(basePosInLoop.x + unitSize.x, basePosInLoop.y + unitSize.y, 0f);
            posSet.bottomRight = new Vector3(basePosInLoop.x + unitSize.x, basePosInLoop.y, 0f);
            return posSet;
        }

        /// <Summary>
        /// Receiver of OnSetDirty message.
        /// </Summary>
        public virtual void OnSetDirty()
        {
            SetAllDirty();
        }

        /// <Summary>
        /// Receiver of OnSetDirtyLerp message.
        /// </Summary>
        /// <param name="time">The moveWait in MoveController.</param>
        public virtual void OnSetDirtyLerp(float time)
        {
            StartCoroutine(DrawIntermediateMap(time));
        }

        /// <Summary>
        /// Draw intermediate map according to the value of drawSmoothness.
        /// </Summary>
        /// <param name="time">The moveWait in MoveController.</param>
        protected virtual IEnumerator DrawIntermediateMap(float time)
        {
            yield return null;
            Vector2Int sourcePos = PlayerPosition.Instance.playerPosPre;
            Vector2Int destPos = PlayerPosition.Instance.playerPos;
            float finishTime = Time.time + time;

            while (true)
            {
                float diff = finishTime - Time.time;
                if (diff <= 0)
                {
                    break;
                }
                float rate = 1 - Mathf.Clamp01(diff / time);
                posLerp = Vector2.Lerp(sourcePos, destPos, rate);

                // To update meshes, call Graphic.SetAllDirty() method.
                SetAllDirty();
                for (int i = 0; i < drawSmoothness; i++)
                {
                    yield return null;
                }
            }
            posLerp = destPos;
            SetAllDirty();
        }

        /// <Summary>
        /// Receiver of OnSetNewMap message.
        /// </Summary>
        public virtual void OnSetNewMap()
        {
            // Get new floor data.
            GetFloorData();

            // Draw new floor.
            SetAllDirty();
        }
    }
}