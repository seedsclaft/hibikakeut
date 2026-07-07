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
        [SerializeField] private float rightRectSize = 40;
        [SerializeField] private float topRectSize = 8;
        [SerializeField] private GameObject candidateSelect;
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
            if (battlerInfo == null || battlerInfo.IsEmpty)
            {
                battlerInfoComponent.Clear();
                return;
            }
            battlerInfoComponent.UpdateInfo(battlerInfo);
            if (!battlerInfo.IsActorView)
            {
                battlerInfoComponent.UpdateEnemyImageNativeSize();
            }
            if (battlerRect != null && battlerInfo.Index.Value > 100)
            {
                var x = battlerInfo.LineIndex == LineType.Front ? 0 : -80;
                battlerRect.localPosition = new Vector3(x, 0);
            }
            //UpdateLocalPosition(battlerInfo);
            UIComponent.SetActive(Disable, !ListData.Enable.Value);
        }

        public void SetSmallScale()
        {
            battlerRect.localScale = new Vector2(smallScale, smallScale);
            positionRect.localPosition = new Vector3(rightRectSize, topRectSize, 0);
            var cursorRect = Cursor.GetComponent<RectTransform>();
            cursorRect.anchoredPosition = new Vector2(26, -4);
        }

        public void SetDisable()
        {
            UIComponent.SetActive(Disable, true);
        }

        public void SetActivecandidateSelect(bool isActive)
        {
            UIComponent.SetActive(candidateSelect, isActive);
        }
    }
}