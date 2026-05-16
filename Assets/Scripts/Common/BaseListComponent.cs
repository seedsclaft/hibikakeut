using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class BaseListComponent : ListItem, IListViewItem
    {
        [SerializeField] private bool useBattlerInfoComponent = false;
        [SerializeField] private BattlerInfoComponent battlerInfoComponent;
        [SerializeField] private bool useActorInfoComponent = false;
        [SerializeField] private ActorInfoComponent actorInfoComponent;
        [SerializeField] private bool useEnemyInfoComponent = false;
        [SerializeField] private EnemyInfoComponent enemyInfoComponent;
        [SerializeField] private bool useSkillInfoComponent = false;
        [SerializeField] private SkillInfoComponent skillInfoComponent;
        [SerializeField] private bool usePartyInfoComponent = false;
        [SerializeField] private PartyInfoComponent partyInfoComponent;
        [SerializeField] private bool useStageInfoComponent = false;
        [SerializeField] private StageInfoComponent stageInfoComponent;
        [SerializeField] private bool useItemInfoComponent = false;
        [SerializeField] private ItemInfoComponent itemInfoComponent;
        [SerializeField] private bool useEquipmentInfoComponent = false;
        [SerializeField] private EquipmentInfoComponent equipmentInfoComponent;
        [SerializeField] private bool useEquipmentLearningInfoComponent = false;
        [SerializeField] private EquipmentLearningInfoComponent equipmentLearningInfoComponent;


        public void UpdateViewItem()
        {
            if (ListData == null)
            {
                return;
            }
            if (battlerInfoComponent != null && useBattlerInfoComponent)
            {
                var battlerInfo = ListItemData<BattlerInfo>();
                battlerInfoComponent.UpdateInfo(battlerInfo);
            }
            if (actorInfoComponent != null && useActorInfoComponent)
            {
                var actorInfo = ListItemData<ActorInfo>();
                actorInfoComponent.UpdateInfo(actorInfo,null);
            }
            if (enemyInfoComponent != null && useEnemyInfoComponent)
            {
                var enemyInfo = ListItemData<BattlerInfo>();
                enemyInfoComponent.UpdateInfo(enemyInfo);
            }
            if (skillInfoComponent != null && useSkillInfoComponent)
            {
                var skillInfo = ListItemData<SkillInfo>();
                skillInfoComponent.UpdateInfo(skillInfo);
            }
            if (partyInfoComponent != null && usePartyInfoComponent)
            {
                var partyInfo = ListItemData<PartyInfo>();
                partyInfoComponent.UpdateInfo(partyInfo);
            }
            if (stageInfoComponent != null && useStageInfoComponent)
            {
                var stageInfo = ListItemData<StageInfo>();
                stageInfoComponent.UpdateInfo(stageInfo);
            }
            if (itemInfoComponent != null && useItemInfoComponent)
            {
                var itemInfo = ListItemData<ItemInfo>();
                itemInfoComponent.UpdateInfo(itemInfo);
            }
            if (equipmentInfoComponent != null && useEquipmentInfoComponent)
            {
                var equipmentInfo = ListItemData<EquipmentInfo>();
                equipmentInfoComponent.UpdateInfo(equipmentInfo);
            }
            if (equipmentLearningInfoComponent != null && useEquipmentLearningInfoComponent)
            {
                var equipmentLearningInfo = ListItemData<EquipmentLearningInfo>();
                equipmentLearningInfoComponent.UpdateInfo(equipmentLearningInfo);
            }
            UIComponent.SetActive(Disable, !ListData.Enable.Value);
            UIComponent.SetActive(Batch, ListData.Batch.Value);
        }
    }
}
