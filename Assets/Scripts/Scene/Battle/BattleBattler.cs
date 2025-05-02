using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class BattleBattler : ListItem, IListViewItem
    {
        [SerializeField] private BattlerInfoComponent battlerInfoComponent;
        [SerializeField] private RectTransform battlerRect;
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
                gameObject.SetActive(battlerInfo != null && battlerInfo.Index.Value > 0);
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

        public void SetDisable()
        {
            if (Disable != null)
            {
                Disable.SetActive(true);
            }
        }
    }
}