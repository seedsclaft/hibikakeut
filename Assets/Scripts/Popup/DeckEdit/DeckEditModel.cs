using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ryneus
{
    public class DeckEditModel : BaseModel
    {
        public ParameterInt FromEditIndex = new();

        public DeckEditModel()
        {
        }

        public List<BattlerInfo> PartyUnit()
        {
            return PartyInfo.DeckEditBattlerInfos();
        }

        public void SwapBattler(int toActorId)
        {
            CurrentDeckInfo.SwapBattler(FromEditIndex.Value,toActorId);
        }
    }
}