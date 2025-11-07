using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class UseItemModel : BaseModel
    {
        public UseItemModel()
        {
        }

        public List<ItemInfo> DungeonUseItemInfos()
        {
            return PartyInfo.DungeonUseItemInfos();
        }

        public void ChangeEncountRate(int rate, int turns)
        {
            CurrentDeckInfo.EncountRate.SetValue(rate);
            CurrentDeckInfo.EncountRateTurn.SetValue(turns);
        }

        public void ChangeDungeonTurn(int turns)
        {
            PartyInfo.CurrentDeckInfo.TurnCount.GainValue(turns);
        }

        public bool CanUseItem(ItemInfo itemInfo)
        {
            switch (itemInfo.Master.Param1)
            {
                case (int)UseItemType.EncountRate:
                case (int)UseItemType.DungeonTurn:
                    return true;
                case (int)UseItemType.Heal:
                    return CanUseRecoveryHeal();
            }
            return false;
        }

        public bool CanUseRecoveryHeal()
        {
            var notLimited = PartyInfo.CurrentDeckActorInfos().FindAll(a => a.CurrentHp.Value < a.MaxHp);
            return notLimited.Count > 0 && !PartyInfo.Cursed.Value;
        }

        public void UseItemHeal(int heal)
        {
            PartyInfo.UseItemHeal(heal);
        }
    }
}