using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class UseItemModel : BaseModel
    {
        private UseItemSceneInfo _sceneParam;
        private ActorInfo _actorInfo;
        public ActorInfo CurrentActor => _actorInfo;
        public UseItemModel()
        {
            var sceneParam = (UseItemSceneInfo)GameSystem.SceneStackManager.LastPopupInfo.template;
            if (sceneParam != null)
            {
                _sceneParam = sceneParam;
                _actorInfo = _sceneParam.CurrentActor;
            }
        }

        public List<ItemInfo> DungeonUseItemInfos()
        {
            if (_sceneParam != null)
            {
                return PartyInfo.GetOwnUseItemInfos(_sceneParam.UsableItemTypes);
            }
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
                case (int)UseItemType.Exp:
                    return _actorInfo != null && _actorInfo.Level < _actorInfo.Master.MaxLv;
                case (int)UseItemType.AttributeUp:
                    var getAttibute = (AttributeType)itemInfo.Master.Param2;
                    return _actorInfo != null &&_actorInfo.AttributeRanks(PartyInfo.ActorInfos)[(int)getAttibute] != AttributeRank.S;
                case (int)UseItemType.StatusUp:
                    return _actorInfo != null;
                case (int)UseItemType.ClassChange:
                    return _actorInfo != null && !_actorInfo.IsClassChenged.Value;
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

    public class UseItemSceneInfo
    {
        public List<UseItemType> UsableItemTypes;
        public ActorInfo CurrentActor;
    }
}