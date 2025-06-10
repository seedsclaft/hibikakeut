using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ariadne
{
    /// <Summary>
    /// Drawing dungeon ground.
    /// </Summary>
    public class DrawDungeonGround : DrawDungeon, IDrawer
    {
        /// <Summary>
        /// Instantiate dungeon ground.
        /// </Summary>
        protected virtual void DrawGround()
        {
            if (dungeonPartsDataList == null)
            {
                return;
            }

            if (gameObject.transform.childCount > 0)
            {
                return;
            }
            
            Vector3 basePos = Vector3.zero;
            GameObject groundPrefab = dungeonBasePartsData.groundObj;
            GameObject sizePrefab = dungeonBasePartsData.sizeBaseObj;
            Vector3 unitSize = new Vector3(sizePrefab.transform.localScale.x, sizePrefab.transform.localScale.y, sizePrefab.transform.localScale.z);

            outsideWallSize = drawOutsideWall ? outsideWallSize : 0;
            float sizeX = (currentFloorMapData.floorSizeHorizontal + outsideWallSize * 2) * unitSize.x;
            float sizeZ = (currentFloorMapData.floorSizeVertical + outsideWallSize * 2) * unitSize.z;
            Vector3 groundSize = new Vector3(sizeX, groundPrefab.transform.localScale.y, sizeZ);

            float outSideWallOffsetX = outsideWallSize * unitSize.x;
            float outSideWallOffsetZ = outsideWallSize * unitSize.z;
            float posX = (basePos.x + groundSize.x - unitSize.x) / 2 - outSideWallOffsetX;
            float posZ = (basePos.z + groundSize.z - unitSize.z) / 2 - outSideWallOffsetZ;
            Vector3 groundPos = new Vector3(posX, groundPrefab.transform.localPosition.y, posZ);

            GameObject ground = (GameObject) Instantiate(groundPrefab, groundPos, Quaternion.identity);
            ground.transform.localScale = groundSize;
            ground.transform.SetParent(gameObject.transform);
            ground.name = "Ground";

            int tile = 2;
            int texScaleX = (currentFloorMapData.floorSizeHorizontal + outsideWallSize * 2) * tile;
            int texScaleZ = (currentFloorMapData.floorSizeVertical + outsideWallSize * 2) * tile;
            ground.GetComponent<Renderer>().material.mainTextureScale = new Vector2(texScaleX, texScaleZ);
        }

        /// <Summary>
        /// Receiver of OnDraw message.
        /// </Summary>
        public virtual void OnDraw()
        {
            base.GetSettings();
            DrawGround();
        }

        /// <Summary>
        /// Receiver of OnRedraw message.
        /// Before instantiate new ceiling object, this method calls RemoveChildObjects() method.
        /// </Summary>
        public virtual void OnRedraw()
        {
            base.GetSettings();
            base.RemoveChildObjects();
            DrawGround();
        }

        /// <Summary>
        /// Remove dungeon ground.
        /// </Summary>
        public virtual void OnRemoveObjects()
        {
            base.RemoveChildObjects();
        }
    }
}
