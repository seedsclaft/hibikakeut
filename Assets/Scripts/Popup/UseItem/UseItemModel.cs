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

        public List<ItemInfo> UseItemInfos()
        {
            var allItems = PartyInfo.GetOwnItemInfos();
            var list = new List<ItemInfo>();
            foreach (var allItem in allItems)
            {
                if (_sceneParam != null)
                {
                    if (EnableUse(allItem))
                    {
                        list.Add(allItem);
                    }
                    continue;
                }
                list.Add(allItem);
            }
            if (_sceneParam != null)
            {
                foreach (var allItem in allItems)
                {
                    if (!EnableUse(allItem))
                    {
                        list.Add(allItem);
                    }
                }
            }
            return list;
        }

        public bool EnableUse(ItemInfo itemInfo)
        {
            if (_sceneParam != null)
            {
                return _sceneParam.UsableItemTypes.Contains((UseItemType)itemInfo.Master.Param1);
            }
            return true;
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
            switch ((UseItemType)itemInfo.Master.Param1)
            {
                case UseItemType.EncountRate:
                case UseItemType.DungeonTurn:
                    return true;
                case UseItemType.Heal:
                    return CanUseRecoveryHeal();
                case UseItemType.Exp:
                    return _actorInfo != null && _actorInfo.Level < _actorInfo.Master.MaxLv;
                case UseItemType.AttributeUp:
                    var getAttibute = (AttributeType)itemInfo.Master.Param2;
                    return _actorInfo != null &&_actorInfo.AttributeRanks(PartyInfo.ActorInfos)[(int)getAttibute] != AttributeRank.S;
                case UseItemType.StatusUp:
                    return _actorInfo != null;
                case UseItemType.ClassChange:
                    return _actorInfo != null && !_actorInfo.IsClassChenged.Value;
            }
            return false;
        }

        public bool CanUseRecoveryHeal()
        {
            var notLimited = PartyInfo.CurrentDeckActorInfos().FindAll(a => a.CurrentHp.Value < a.MaxHp);
            return notLimited.Count > 0;// && !CurrentDeckInfo.Cursed.Value;
        }

        public void UseItemHeal(int heal)
        {
            PartyInfo.UseItemHeal(heal);
        }

        public int GetExpValue(ItemInfo itemInfo)
        {
            var getExp = itemInfo.Master.Param2;
            if (CurrentActor.Level <= itemInfo.Master.Param3)
            {
                getExp *= 2;
            }
            return getExp;
        }
    }

    public class UseItemSceneInfo
    {
        public List<UseItemType> UsableItemTypes;
        public ActorInfo CurrentActor;
    }
}