using System;
using System.Collections;
using System.Collections.Generic;

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
            CurrentDeckInfo.SwapBattler(FromEditIndex.Value, toActorId);
            FromEditIndex.SetValue(-1);
        }

        public int FromEditSelectIndex()
        {
            var actorId = CurrentDeckInfo.ActorIdDict[FromEditIndex.Value];
            if (actorId > 0)
            {
                return PartyInfo.ActorInfos.FindIndex(a => a.ActorId.Value == actorId);
            }
            return -1;
        }

        public int FromEditSelectBattlerIndex()
        {
            var actorId = CurrentDeckInfo.ActorIdDict[FromEditIndex.Value];
            if (actorId > 0)
            {
                return PartyUnit().Find(a => a.ActorInfo.ActorId.Value == actorId).Index.Value;
            }
            return -1;
        }

        public bool AdjustEditIndexes()
        {
            return CurrentDeckInfo.AdjustEditIndexes();
        }

        public void AutoDeck()
        {
            // 戦力値順に並べる
            CurrentDeckInfo.InitUnitInfos();
            var actorInfos = PartyInfo.EditableActorInfos();
            actorInfos.Sort((a, b) => a.Evaluate() - b.Evaluate() < 0 ? 1 : -1);
            CurrentDeckInfo.SetAutoDeck(actorInfos);
        }
    }
}