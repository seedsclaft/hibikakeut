using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ryneus
{
    public class DeckEditModel : BaseModel
    {
        public ParameterInt ChangeActorId = new();

        public DeckEditModel()
        {
        }

        public List<BattlerInfo> PartyUnit()
        {
            return PartyInfo.CurrentDeckBattlerInfos();
        }

        public void SwapBattler(int toActorId)
        {
            CurrentDeckInfo.SwapBattler(ChangeActorId.Value,toActorId);
        }
    }

    public class DeckEditSceneInfo
    {
        public DeckEditSceneInfo()
        {
        }

        public bool IsLoad = false;
    }
}