using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class BattleBattler : ListItem, IListViewItem
    {
        [SerializeField] private BattlerInfoComponent battlerInfoComponent;
        [SerializeField] private RectTransform battlerRect;
        [SerializeField] private RectTransform positionRect;
        [SerializeField] private float normalScale = 1;
        [SerializeField] private float smallScale = 0.75f;
        [SerializeField] private float rightRectSize = 24;
        public BattlerInfoComponent BattlerInfoComponent => battlerInfoComponent;

        public void SetDamageRoot(GameObject damageRoot)
        {
            battlerInfoComponent.SetDamageRoot(damageRoot);
        }

        public void SetStatusRoot(GameObject statusRoot)
        {
            battlerInfoComponent.SetStatusRoot(statusRoot);
        }

        public void UpdateViewItem()
        {
            if (ListData == null)
            {
                return;
            }
            var battlerInfo = ListItemData<BattlerInfo>();
            //battlerInfoComponent.SetSelectable(ListData.Enable);
            battlerInfoComponent.UpdateInfo(battlerInfo);
            if (!battlerInfo.IsActor)
            {
                //gameObject.SetActive(battlerInfo != null && battlerInfo.Index.Value > 0);
            }
            battlerInfoComponent.RefreshStatus();
            if (!battlerInfo.IsActorView)
            {
                battlerInfoComponent.UpdateEnemyImageNativeSize();
            }
            if (battlerRect != null && battlerInfo.Index.Value > 100)
            {
                var x = battlerInfo.LineIndex == LineType.Front ? 0 : -80;
                battlerRect.localPosition = new Vector3(x,0);
            }
            //UpdateLocalPosition(battlerInfo);
            if (Disable != null)
            {
                Disable.SetActive(!ListData.Enable);
            }
        }

        public void SetSmallScale()
        {
            battlerRect.localScale = new Vector2(smallScale,smallScale);
            positionRect.localPosition = new Vector3(rightRectSize,0,0);
        }

        public void SetDisable()
        {
            if (Disable != null)
            {
                Disable.SetActive(true);
            }
        }
    }
}