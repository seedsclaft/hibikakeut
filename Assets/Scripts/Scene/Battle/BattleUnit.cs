using UnityEngine;

namespace Ryneus
{
    public class BattleUnit : ListItem, IListViewItem
    {
        [SerializeField] private UnitInfoComponent unitInfoComponent;
        [SerializeField] private RectTransform battlerRect;
        public UnitInfoComponent UnitInfoComponent => unitInfoComponent;
        private UnitInfo _unitInfo = null;

        public BattlerInfoComponent FindBattlerInfoComponent(int battlerIndex)
        {
            var findIndex = _unitInfo.BattlerInfos.FindIndex(a => a.Index.Value == battlerIndex);
            if (findIndex == -1)
            {
                return null;
            }
            return findIndex == 0 ? unitInfoComponent.FrontBattler : unitInfoComponent.BackBattler;
        }

        public void SetDamageRoot(int battlerIndex,GameObject damageRoot)
        {
            FindBattlerInfoComponent(battlerIndex)?.SetDamageRoot(damageRoot);
        }

        public void SetStatusRoot(int battlerIndex,GameObject statusRoot)
        {
            FindBattlerInfoComponent(battlerIndex)?.SetStatusRoot(statusRoot);
        }

        public void UpdateViewItem()
        {
            if (ListData == null)
            {
                return;
            }
            var unitInfo = ListItemData<UnitInfo>();
            _unitInfo = unitInfo;
            unitInfoComponent.UpdateInfo(unitInfo);
            if (!ListData.Enable)
            {
                SetDisable();
            }
        }

        public void SetDisable()
        {
            if (Disable != null)
            {
                Disable.SetActive(true);
                clickButton.enabled = false;
            }
        }
    }
}
