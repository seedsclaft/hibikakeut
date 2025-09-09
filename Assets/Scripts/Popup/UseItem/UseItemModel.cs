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

        public void ChangeEncountRate(int rate, int turn)
        {
            CurrentDeckInfo.EncountRate.SetValue(rate);
            CurrentDeckInfo.EncountRateTurn.SetValue(turn);
        }
    }
}