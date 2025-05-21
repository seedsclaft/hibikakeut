using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Ariadne
{
    /// <Summary>
    /// Utility class for MapEditor and EventEditor.
    /// </Summary>
    public static class MapEditorUtil
    {
        // Temp file of floor map data.
        public static readonly string AssetRootPrefix = "Assets/Ariadne/";
        public static readonly string TempFileName = "MapDataTemp";
        public static readonly string TempFilePath = "Editor/MapEditorTemp/";
        public static readonly string InfoFileName = "tempInfo";

        public static readonly string EventDefaultName = "New Event";
        public static readonly string EventDataPathPrefix = "Assets/Ariadne/Resources/";
        public static readonly string EventDataPath = "EventData/";
        public static readonly string EventFilePrefix = "event_";

        public static readonly string NotAssignedText = "*** Not Assigned ***";

        /// <Summary>
        /// Validation of the index.
        /// </Summary>
        /// <param name="index">Index to check.</param>
        /// <param name="listSize">Size of map array.</param>
        public static bool CheckEditMapIndexIsValid(int index, int listSize)
        {
            bool isValid = false;
            if (index >= 0 && index < listSize)
            {
                isValid = true;
            }
            return isValid;
        }

        /// <Summary>
        /// Validation of the floor size.
        /// </Summary>
        /// <param name="horizontal">Horizontal size of the floor.</param>
        /// <param name="vertical">Vertical size of the floor.</param>
        public static bool CheckFloorSizeIsValid(int horizontal, int vertical)
        {
            bool isValid = false;
            if (horizontal > 0 && vertical > 0)
            {
                isValid = true;
            }
            return isValid;
        }

        /// <Summary>
        /// Returns a texture of map base.
        /// </Summary>
        /// <param name="tex">Texture to set pixel data.</param>
        /// <param name="cellSize">Cell size of the map.</param>
        /// <param name="gridColor">Color of the grid line on map.</param>
        /// <param name="baseColor">Color of the map base.</param>
        public static Texture2D SetMapColor(Texture2D tex, int cellSize, Color gridColor, Color baseColor)
        {
            for (int y = 0; y < tex.height; y++)
            {
                for (int x = 0; x < tex.width; x++)
                {
                    if (CheckGridPoint(x, y, cellSize))
                    {
                        tex.SetPixel(x, y, gridColor);
                    }
                    else
                    {
                        tex.SetPixel(x, y, baseColor);
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        /// <Summary>
        /// Check whether draw grid line color of the pixel.
        /// </Summary>
        /// <param name="x">Horizontal axis of the pixel.</param>
        /// <param name="y">Vertical axis of the pixel.</param>
        /// <param name="cellSize">Cell size of the map.</param>
        static bool CheckGridPoint(int x, int y, int cellSize)
        {
            bool isValid = false;
            if (x % cellSize == 0 || x % cellSize == cellSize - 1)
            {
                isValid = true;
            }
            else if (y % cellSize == 0 || y % cellSize == cellSize - 1)
            {
                isValid = true;
            }
            
            return isValid;
        }

        /// <Summary>
        /// Returns new texture of the map.
        /// </Summary>
        /// <param name="horizontalSize">Horizontal size of the map.</param>
        /// <param name="verticalSize">Vertical size of the map.</param>
        /// <param name="cellSize">Cell size of the map.</param>
        public static Texture2D RepaintMapTexture(int horizontalSize, int verticalSize, int cellSize, Color gridColor, Color baseColor)
        {
            Texture2D tex = new Texture2D(16, 16, TextureFormat.ARGB32, false);
            if (CheckFloorSizeIsValid(horizontalSize, verticalSize))
            {
                int texWidth = cellSize * horizontalSize;
                int texHeight = cellSize * verticalSize;

                tex = new Texture2D(texWidth, texHeight, TextureFormat.ARGB32, false);
                tex = SetMapColor(tex, cellSize, gridColor, baseColor);
            }
            else
            {
                Debug.LogError("Floor size might be invalid value. Input a value which is greater than 0.");
            }
            return tex;
        }

        /// <Summary>
        /// Returns rect for map to select the axis on the dungeon.
        /// </Summary>
        /// <param name="baseLayerRect">Texture of base layer of the map.</param>
        /// <param name="horizontalSize">Horizontal size of the map.</param>
        /// <param name="verticalSize">Vertical size of the map.</param>
        /// <param name="cellSize">Cell size of the map.</param>
        public static Rect[,] GetMapRectArray(Rect baseLayerRect, int horizontalSize, int verticalSize, int cellSize)
        {
            Rect[,] mapRect = new Rect[horizontalSize, verticalSize];
            for (int y = 0; y < verticalSize; y++)
            {
                for (int x = 0; x < horizontalSize; x++)
                {
                    Vector2 pos = baseLayerRect.position;
                    pos.x += cellSize * x + GUI.skin.box.padding.left;
                    pos.y -= cellSize * (y + 1 - verticalSize) - GUI.skin.box.padding.top;
                    Rect r = new Rect(pos.x, pos.y, cellSize, cellSize);
                    mapRect[x, y] = r;
                }
            }
            return mapRect;
        }

        /// <Summary>
        /// Returns map icon textures.
        /// </Summary>
        public static Texture2D[] GetMapIconArray(List<MapAttributeData> mapAttributeDataList)
        {
            List<Texture2D> iconList = new List<Texture2D>();
            if (mapAttributeDataList == null)
            {
                return iconList.ToArray();
            }

            foreach (MapAttributeData mapAttributeData in mapAttributeDataList)
            {
                if (mapAttributeData == null)
                {
                    continue;
                }

                foreach (MapAttributeRecord record in mapAttributeData.mapAttributeRecords)
                {
                    if (record.notInUse)
                    {
                        continue;
                    }

                    if (record.mapIcon != null)
                    {
                        iconList.Add(record.mapIcon.texture);
                    }
                    else
                    {
                        Texture2D t = new Texture2D(32, 32);
                        iconList.Add(t);
                    }
                }
            }

            Texture2D[] mapIconList = iconList.ToArray();
            return mapIconList;
        }

        /// <Summary>
        /// Returns attribute id which corresponds to map icon.
        /// </Summary>
        public static int[] GetMapAttributeArray(List<MapAttributeData> mapAttributeDataList)
        {
            List<int> attrList = new List<int>();
            if (mapAttributeDataList == null)
            {
                return attrList.ToArray();
            }

            foreach (MapAttributeData mapAttributeData in mapAttributeDataList)
            {
                if (mapAttributeData == null)
                {
                    continue;
                }

                foreach (MapAttributeRecord record in mapAttributeData.mapAttributeRecords)
                {
                    if (record.notInUse)
                    {
                        continue;
                    }
                    attrList.Add(record.attributeId);
                }
            }
            
            int[] mapAttributeArray = attrList.ToArray();
            return mapAttributeArray;
        }

        /// <Summary>
        /// Returns tool icon textures.
        /// </Summary>
        public static Texture2D[] GetToolIconArray()
        {
            // Tool icon
            Texture2D toolSelectIcon = LoadIconAsset(MapEditorTools.SelectIconName);
            Texture2D toolDrawIcon = LoadIconAsset(MapEditorTools.DrawIconName);
            Texture2D toolRectIcon = LoadIconAsset(MapEditorTools.RectIconName);
            Texture2D toolRectFillIcon = LoadIconAsset(MapEditorTools.FillRectIconName);
            Texture2D[] drawToolIconList = new Texture2D[]{toolSelectIcon, toolDrawIcon, toolRectIcon, toolRectFillIcon};
            return drawToolIconList;
        }

        /// <Summary>
        /// Load icon texture by arg.
        /// </Summary>
        static Texture2D LoadIconAsset(string textureName)
        {
            // Load icon asset.
            string[] guids = AssetDatabase.FindAssets(textureName, null);

            Texture2D iconTexture = null;
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                iconTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                if (iconTexture != null)
                {
                    break;
                }
            }
            return iconTexture;
        }

        /// <Summary>
        /// Returns an event icon textures.
        /// </Summary>
        public static Texture2D GetEventIcon()
        {
            Texture2D eventIcon = LoadIconAsset(MapEditorTools.EventIconName);
            return eventIcon;
        }

        /// <Summary>
        /// Returns an entrance icon textures.
        /// </Summary>
        public static Texture2D GetEntranceIcon()
        {
            Texture2D entranceIcon = LoadIconAsset(MapEditorTools.EntranceIconName);
            return entranceIcon;
        }

        /// <Summary>
        /// Returns a warning icon textures.
        /// </Summary>
        public static Texture2D GetWarningIcon()
        {
            Texture2D warningIcon = LoadIconAsset(MapEditorTools.WarningIconName);
            return warningIcon;
        }

        /// <Summary>
        /// Returns the axis of selected point on the grid.
        /// </Summary>
        /// <param name="pos">Position on the map texture GUI.</param>
        /// <param name="horizontalSize">Horizontal size of the map.</param>
        /// <param name="verticalSize">Vertical size of the map.</param>
        /// <param name="cellSize">Cell size of the map.</param>
        public static Vector2Int GetPosInGrid(Vector2 pos, int horizontalSize, int verticalSize, int cellSize)
        {
            // The origin is left bottom
            Vector2Int gridPos = Vector2Int.zero;

            gridPos.x = (int)pos.x / cellSize;
            if (gridPos.x > horizontalSize - 1)
            {
                gridPos.x = horizontalSize - 1;
            }
            else if (gridPos.x < 0)
            {
                gridPos.x = 0;
            }

            gridPos.y = (int)pos.y / cellSize;
            gridPos.y = verticalSize - 1 - gridPos.y;
            if (gridPos.y > verticalSize - 1)
            {
                gridPos.y = verticalSize - 1;
            }
            else if (gridPos.y < 0)
            {
                gridPos.y = 0;
            }
            return gridPos;
        }

        /// <Summary>
        /// Returns the array of GUIDs specified type.
        /// </Summary>
        /// <param name="dataType">Type name to search asset.</param>
        public static string[] GetGuidArray(string dataType)
        {
            string filter = "t:" + dataType;
            string[] guidArray = AssetDatabase.FindAssets(filter);
            return guidArray;
        }

        /// <Summary>
        /// Check the draw icon flag in MapAttributeData.
        /// </Summary>
        public static bool CheckDrawEditorIconFlag(int attributeId, List<MapAttributeData> mapAttributeDataList)
        {
            if (mapAttributeDataList == null)
            {
                return false;
            }

            MapAttributeRecord record = DataRecordUtil.GetMapAttributeRecordById(mapAttributeDataList, attributeId);
            if (record == null)
            {
                return false;
            }
            return record.drawIconOnEditor;
        }
    }
}