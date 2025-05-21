using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ariadne
{
    /// <Summary>
    /// Draw icons on the map.
    /// Icons are instantiated as GameObject.
    /// </Summary>
    public class DrawMapIcon : MonoBehaviour, IDirtyMarkerMap
    {
        protected int showLengthHorizontal = 4;
        protected int showLengthVertical = 4;
        protected int drawSmoothness = 2;
        protected DungeonMasterData dungeonData;
        protected FloorMapMasterData floorMapData;
        protected List<MapAttributeData> mapAttributeDataList;

        public GameObject iconPrefab;

        protected Dictionary<Vector2Int, GameObject> posIconDict;
        protected RectTransform parentRt;
        protected RectTransform mapRt;
        protected Vector2 posLerp = Vector2.zero;
        protected Vector3 iconScale = new Vector3(1.0f, 1.0f, 1.0f);
        protected GameObject gameController;
        protected DungeonSettings dungeonSettings;
        protected MapShowingSettings mapSettings;

        protected virtual void Start()
        {
            GetFloorData();
            GetMapSettings();

            DrawIcon();
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
        }

        /// <Summary>
        /// Instantiate icon GameObjects according to map attribute.
        /// When they have been instantiated already, this method updates their position and visible state.
        /// </Summary>
        protected virtual void DrawIcon()
        {
            if (posIconDict == null)
            {
                posIconDict = new Dictionary<Vector2Int, GameObject>();
            }

            if (mapRt == null)
            {
                mapRt = gameObject.GetComponent<RectTransform>();
            }
            Vector3 centerPos = mapRt.transform.localPosition;

            float cellWidth = mapRt.sizeDelta.x / (1 + showLengthHorizontal * 2);
            float cellHeight = mapRt.sizeDelta.y / (1 + showLengthVertical * 2);
            Vector2 unitSize = new Vector2(cellWidth, cellHeight);
            float widthOffset = cellWidth / 2;
            float heightOffset = cellHeight / 2;
            
            if (parentRt == null)
            {
                parentRt = GameObject.Find(AriadneSceneObjectName.MapParent).GetComponent<RectTransform>();
            }
            Vector2 parentPos = parentRt.transform.localPosition;
            float parentWidth = parentRt.sizeDelta.x;
            float parentHeight = parentRt.sizeDelta.y;

            int signX = centerPos.x < 0 ? -1 : 1;
            int signY = centerPos.y < 0 ? -1 : 1;

            float basePosX = parentWidth / 2 - signX * centerPos.x - widthOffset - posLerp.x * unitSize.x;
            float basePosY = parentHeight / 2 - signY * centerPos.y - heightOffset - posLerp.y * unitSize.y;

            for (int yAxis = 0; yAxis < floorMapData.floorSizeVertical; yAxis++)
            {
                for (int xAxis = 0; xAxis < floorMapData.floorSizeHorizontal; xAxis++)
                {

                    int index = xAxis + yAxis * floorMapData.floorSizeHorizontal;
                    int mapAttrId = floorMapData.mapInfo[index].mapAttr;
                    if (!CheckDrawIconFlag(mapAttrId))
                    {
                        continue;
                    }

                    Vector3 basePosInLoop = new Vector3(basePosX + unitSize.x * xAxis, basePosY + unitSize.y * yAxis);
                    Vector2Int axisPos = new Vector2Int(xAxis, yAxis);
                    Vector2 iconPos = new Vector2(basePosInLoop.x + unitSize.x / 2, basePosInLoop.y + unitSize.y / 2);
                    SetIconObj(iconPos, mapAttrId, axisPos, unitSize);
                }
            }
        }

        /// <Summary>
        /// Returns the value of drawMapIcon in MapAttributeData.
        /// </Summary>
        protected virtual bool CheckDrawIconFlag(int attrId)
        {
            if (mapAttributeDataList == null)
            {
                return false;
            }

            MapAttributeRecord record = DataRecordUtil.GetMapAttributeRecordById(mapAttributeDataList, attrId);
            if (record == null)
            {
                return false;
            }
            return record.drawMapIcon;
        }

        /// <Summary>
        /// Set icon GameObjects.
        /// </Summary>
        /// <param name="iconPos">The icon position on the map.</param>
        /// <param name="mapAttrId">The map attribute id.</param>
        /// <param name="axisPos">The position on the dungeon.</param>
        /// <param name="sizeSet">The Icon size.</param>
        protected virtual void SetIconObj(Vector2 iconPos, int mapAttrId, Vector2Int axisPos, Vector2 sizeSet)
        {
            if (mapAttributeDataList == null)
            {
                return;
            }

            string objName = "Icon_" + axisPos.x.ToString() + "-" + axisPos.y.ToString();
            Sprite sp = null;

            // Get MapAttributeRecord.
            MapAttributeRecord record = DataRecordUtil.GetMapAttributeRecordById(mapAttributeDataList, mapAttrId);
            if (record == null)
            {
                return;
            }

            objName = objName + "_" + record.attributeName;
            sp = record.mapIcon;

            Transform trans = transform.Find(objName);
            if (trans == null)
            {
                ShowIconObj(iconPos, axisPos, sizeSet, sp, objName);
                return;
            }

            GameObject iconObj = trans.gameObject;
            if (posIconDict.ContainsKey(axisPos))
            {
                iconObj.transform.localPosition = iconPos;
                iconObj.transform.localScale = iconScale;
                iconObj.GetComponent<RectTransform>().sizeDelta = sizeSet;
                SetIconVisibleState(axisPos, trans.gameObject);
            }
            else
            {
                ShowIconObj(iconPos, axisPos, sizeSet, sp, objName);
            }
        }

        /// <Summary>
        /// Instantiate icon GameObjects.
        /// </Summary>
        /// <param name="iconPos">The icon position on the map.</param>
        /// <param name="axisPos">The position on the dungeon.</param>
        /// <param name="sizeSet">The Icon size.</param>
        /// <param name="sprite">The sprite image of the icon.</param>
        /// <param name="objName">The name of icon object.</param>
        protected virtual void ShowIconObj(Vector2 iconPos, Vector2Int axisPos, Vector2 sizeSet, Sprite sprite, string objName)
        {
            GameObject icon = (GameObject)Instantiate(iconPrefab, iconPos, Quaternion.identity);

            icon.GetComponent<Image>().sprite = sprite;
            SetIconVisibleState(axisPos, icon);

            icon.transform.SetParent(gameObject.transform);
            icon.transform.localPosition = iconPos;
            icon.transform.localScale = iconScale;
            icon.GetComponent<RectTransform>().sizeDelta = sizeSet;
            icon.name = objName;

            if (posIconDict.ContainsKey(axisPos))
            {
                posIconDict[axisPos] = icon;
            }
            else
            {
                posIconDict.Add(axisPos, icon);
            }
        }

        /// <Summary>
        /// Move icon objects.
        /// </Summary>
        /// <param name="prePos">The pre-position of the icon.</param>
        /// <param name="destPos">The dest position of the icon.</param>
        protected virtual void MoveIcon(Vector2 prePos, Vector2 destPos)
        {
            float cellWidth = mapRt.sizeDelta.x / (1 + showLengthHorizontal * 2);
            float cellHeight = mapRt.sizeDelta.y / (1 + showLengthVertical * 2);
            Vector2 unitSize = new Vector2(cellWidth, cellHeight);

            float deltaX = (destPos.x - prePos.x) * unitSize.x;
            float deltaY = (destPos.y - prePos.y) * unitSize.y;
            foreach (KeyValuePair<Vector2Int, GameObject> pair in posIconDict)
            {
                // Check if the position is traversed.
                SetIconVisibleState(pair.Key, pair.Value);

                Vector2 pos = pair.Value.transform.localPosition;
                pos.x -= deltaX;
                pos.y -= deltaY;
                pair.Value.transform.localPosition = pos;
                pair.Value.GetComponent<RectTransform>().sizeDelta = unitSize;
            }
        }

        /// <Summary>
        /// Set the visible state of the icon according to traverse data.
        /// </Summary>
        /// <param name="axisPos">The position on the dungeon.</param>
        /// <param name="obj">The icon object.</param>
        protected virtual void SetIconVisibleState(Vector2Int axisPos, GameObject obj)
        {
            bool isTraversed = TraverseManager.GetPositionTraverseData(dungeonData.dungeonId, floorMapData.floorId, axisPos);
            obj.GetComponent<Image>().enabled = isTraversed;
        }

        /// <Summary>
        /// Receiver of OnSetDirty message.
        /// </Summary>
        public virtual void OnSetDirty()
        {
            GetFloorData();
            GetMapSettings();
            DrawIcon();
        }

        /// <Summary>
        /// Receiver of OnSetDirtyLerp message.
        /// </Summary>
        /// <param name="time">The moveWait in MoveController.</param>
        public virtual void OnSetDirtyLerp(float time)
        {
            GetFloorData();
            GetMapSettings();
            DrawIcon();
            StartCoroutine(DrawIntermediateMap(time));
        }

        /// <Summary>
        /// Draw intermediate map according to the value of drawSmoothness.
        /// </Summary>
        /// <param name="time">The moveWait in MoveController.</param>
        protected virtual IEnumerator DrawIntermediateMap(float time)
        {
            yield return null;
            Vector2Int sourcePos = PlayerPosition.playerPosPre;
            Vector2Int destPos = PlayerPosition.playerPos;
            float finishTime = Time.time + time;

            Vector2 prePosLerp = Vector2.zero;

            while (true)
            {
                float diff = finishTime - Time.time;
                if (diff <= 0)
                {
                    break;
                }

                float rate = 1 - Mathf.Clamp01(diff / time);
                prePosLerp = posLerp;
                posLerp = Vector2.Lerp(sourcePos, destPos, rate);
                MoveIcon(prePosLerp, posLerp);

                for (int i = 0; i < drawSmoothness; i++)
                {
                    yield return null;
                }
            }
            prePosLerp = posLerp;
            posLerp = destPos;
            MoveIcon(prePosLerp, posLerp);
        }

        /// <Summary>
        /// Receiver of OnSetNewMap message.
        /// </Summary>
        public virtual void OnSetNewMap()
        {
            // Remove map icons.
            RemoveMapIcons();

            // Get new floor data.
            GetFloorData();

            // Draw icons.
            DrawIcon();
        }

        /// <Summary>
        /// Removes icon objects.
        /// </Summary>
        protected virtual void RemoveMapIcons()
        {
            posIconDict = new Dictionary<Vector2Int, GameObject>();
            foreach (Transform child in gameObject.transform)
            {
                GameObject childObj = child.gameObject;
                Destroy(childObj);
            }
        }
    }
}