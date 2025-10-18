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
    }
}