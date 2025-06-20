using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ryneus
{
    public class DeckEditModel : BaseModel
    {
        public ParameterInt FromEditIndex = new(-1);

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

        public int FromEditSelectIndex()
        {
            var actorId = CurrentDeckInfo.ActorIdDict[FromEditIndex.Value];
            if (actorId > 0)
            {
                return PartyInfo.ActorInfos.FindIndex(a => a.ActorId.Value == actorId);
            }
            return 0;
        }
    }
}