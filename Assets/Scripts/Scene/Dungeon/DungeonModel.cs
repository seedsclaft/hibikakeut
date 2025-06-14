using System.Linq;
using System.Collections.Generic;
using Ariadne;

namespace Ryneus
{
    public class DungeonModel : BaseModel
    {
        private MoveController _moveController;
        public ParameterInt SelectIndex = new(-1);
        public DungeonModel(MoveController moveController)
        {
            _moveController = moveController;
        }

        public int SelectedCharacterIndex()
        {
            var index = 0;
            var idx = 0;
            foreach (var battlerInfo in PartyUnit())
            {
                if (SelectIndex.Value == idx && battlerInfo.ActorInfo != null)
                {
                    index = battlerInfo.Index.Value;
                }
                idx++;
            }
            return index;
        }

        public void SwapSelectIndex(int selectIndex)
        {
            var toActorId = -1;
            var idx = 0;
            foreach (var battlerInfo in PartyUnit())
            {
                if (selectIndex == idx && battlerInfo.ActorInfo != null)
                {
                    toActorId = battlerInfo.ActorInfo.ActorId.Value;
                }
                idx++;
            }
            //if (toActorId > -1)
            {
                CurrentDeckInfo.SwapBattler(SelectIndex.Value+1,toActorId,selectIndex+1);
            }
            SelectIndex.SetValue(-1);
        }

        // 開示マスを復帰
        public void UpdateTraverses()
        {
            if (CurrentStage != null)
            {
                var traversDates = PartyInfo.GetDungeonTraverse(CurrentStage.StageId.Value);
                TraverseManager.Instance.UpdateTraverses(CurrentStage.StageId.Value,traversDates);
            }
        }

        public void SetPlayerPosition()
        {
            // 位置保存情報を復帰
            _moveController.SetPlayerPosition(CurrentDeckInfo.PositionX.Value,CurrentDeckInfo.PositionY.Value,CurrentDeckInfo.Direction.Value);
        }

        public void UpdateEventObjects()
        {
            // フラグ情報でイベント表示を制御
            var endStageEvents = EndStageEvents();
            foreach (var endStageEvent in endStageEvents)
            {
                if (endStageEvent.Type == StageEventType.GetItem || endStageEvent.Type == StageEventType.GetArtifact || endStageEvent.Type == StageEventType.GetSkill)
                {
                    _moveController.SetEventEndDeactiveEventObj(endStageEvent.PositionX,endStageEvent.PositionY);
                } else
                {
                    _moveController.SetDeactiveEventObj(endStageEvent.PositionX,endStageEvent.PositionY);
                }
            }
        }

        public void DungeonBusy(bool busy)
        {
            _moveController.isInDungeon = !busy;
        }

        public List<BattlerInfo> PartyUnit()
        {
            return PartyInfo.DeckEditBattlerInfos();
        }

        public bool CommandMoveEnd()
        {
            if (CurrentDeckInfo == null)
            {
                return false;
            }
            var playerDungeonId = PlayerPosition.Instance.currentDungeonId;
            var playerPosition = PlayerPosition.Instance.playerPos;
            var playerDirection = PlayerPosition.Instance.direction;
            var lastPositionX = CurrentDeckInfo.PositionX;
            var lastPositionY = CurrentDeckInfo.PositionY;
            if (lastPositionX.Value != playerPosition.x || lastPositionY.Value != playerPosition.y)
            {
                CurrentDeckInfo.SetPosition(playerDungeonId,playerPosition.x,playerPosition.y,(int)playerDirection);

                if (IsActiveDungeon())
                {
                    // ランダムエンカウントフラグ加算
                    int flag = UnityEngine.Random.Range(CurrentStage.Master.EncountMin, CurrentStage.Master.EncountMax);
                    CurrentDeckInfo.Encount.GainValue(flag,0,100);

                    // 残りターン数を減算
                    PartyInfo.TurnCount.GainValue(-1);
                }
                return true;
            }
            return false;
        }

        public int CheckHpHeal()
        {
            var hpHeal = 0;
            foreach (var item in PartyInfo.Items)
            {
                var itemData = DataSystem.Items.Find(a => a.Id == item.Key);
                if (itemData != null && itemData.ItemType == ItemType.Artifact)
                {
                    var skillData = DataSystem.FindSkill(itemData.Param1);
                    if (skillData.TriggerDates.Find(a => a.TriggerType == TriggerType.DungeonMoveEnd) != null)
                    {
                        foreach (var featureData in skillData.FeatureDates)
                        {
                            if (featureData.FeatureType == FeatureType.HpHeal)
                            {
                                hpHeal += featureData.Param1;
                            }
                        }
                    }
                }
            }
            foreach (var actorInfo in PartyInfo.CurrentDeckActorInfos())
            {
                if (actorInfo == null)
                {
                    continue;
                }
                actorInfo.ChangeHp(actorInfo.CurrentHp.Value + hpHeal);
            }
            return hpHeal;
        }

        public UnityEngine.Vector2Int GetForwardPosition()
        {
            return _moveController.GetForwardPosition();
        }

        public bool CheckDirectionEvent()
        {
            var position = GetForwardPosition();
            var stageEvent = StageEvents(EventTiming.Dungeon,position.x,position.y);
            return stageEvent.Count > 0 && (stageEvent[0].Type == StageEventType.GetItem || stageEvent[0].Type == StageEventType.GetArtifact || stageEvent[0].Type == StageEventType.GetSkill);
        }


        public bool EndDungeonByTurnCount()
        {
            if (CurrentStage == null)
            {
                return false;
            }
            return PartyInfo.TurnCount.Value <= 0;
        }

        public bool EncountEnemy()
        {
            if (CurrentDeckInfo == null)
            {
                return false;
            }
            return CurrentDeckInfo.Encount.Value >= 100;
        }

        public void ResetEncountValue()
        {
            if (CurrentDeckInfo == null)
            {
                return;
            }
            CurrentDeckInfo.Encount.SetValue(0);
        }

        public List<BattlerInfo> RandumTroopInfos()
        {
            var troopInfo = new TroopInfo(-1);
            troopInfo.MakeEnemyRandomTroopDates(CurrentStage.Master.StageLv,CurrentStage.Master.RandomTroopEnemyRates);
            return troopInfo.BattlerInfos;
        }

        public List<BattlerInfo> ForceBattleTroopInfos(int troopId)
        {
            var troopInfo = new TroopInfo(troopId);
            troopInfo.MakeEnemyTroopDates(CurrentStage.Master.StageLv);
            return troopInfo.BattlerInfos;
        }

        public TroopInfo RandumTroopInfo()
        {
            var troopInfo = new TroopInfo(-1);
            troopInfo.MakeEnemyRandomTroopDates(1,CurrentStage.Master.RandomTroopEnemyRates);
            return troopInfo;
        }

        public List<ActorInfo> AddSelectActorInfos()
        {
            // 未加入の仲間
            var actorDates = DataSystem.Actors.Where(a => PartyInfo.ActorInfos.Find(b => a.Value.Id == b.ActorId.Value) == null).ToList();
            var actorInfos = new List<ActorInfo>();
            foreach (var actorDate in actorDates)
            {
                actorInfos.Add(new ActorInfo(actorDate.Value));
            }
            return actorInfos;
        }

        public bool CanUseCurrencyHeal()
        {
            var notLimited = PartyInfo.CurrentDeckActorInfos().FindAll(a => a.CurrentHp.Value < a.MaxHp);
            return notLimited.Count > 0 && PartyInfo.Currency.Value > 0;
        }

        public void UseCurrencyHeal()
        {
            if (PartyInfo.Currency.Value <= 0)
            {
                return;
            }
            PartyInfo.Currency.GainValue(-1,0);
            PartyInfo.UseCurrencyHeal();
        }

        public void SaveBgmTiming()
        {
            var timeStamp = SoundManager.Instance.CurrentTimeStamp();
            CurrentDeckInfo.GetDungeonBgmTimeStamp().SetValue(timeStamp);
        }

        public float DungeonBgmTimeStamp()
        {
            if (CurrentDeckInfo != null && CurrentDeckInfo.DungeonBgmTimeStamp != null)
            {
                return CurrentDeckInfo.GetDungeonBgmTimeStamp().Value;
            }
            return 0;
        }

        public List<SystemData.CommandData> SideMenu()
        {
            var list = new List<SystemData.CommandData>();
            var status = new SystemData.CommandData
            {
                Id = 1,
                Name = "メンバー確認",
                Key = "Status"
            };
            list.Add(status);
            var @return = new SystemData.CommandData
            {
                Id = 1,
                Name = "帰還する",
                Key = "Return"
            };
            list.Add(@return);
            var option = new SystemData.CommandData
            {
                Id = 2,
                Name = DataSystem.GetText(13410),
                Key = "Option"
            };
            list.Add(option);
            var menuCommand = new SystemData.CommandData
            {
                Id = 3,
                Name = DataSystem.GetText(19700),
                Key = "Help"
            };
            list.Add(menuCommand);
            var dictionaryCommand = new SystemData.CommandData
            {
                Id = 11,
                Name = DataSystem.GetText(19730),
                Key = "Dictionary"
            };
            list.Add(dictionaryCommand);
            var saveCommand = new SystemData.CommandData
            {
                Id = 4,
                Name = DataSystem.GetText(19710),
                Key = "Save"
            };
            list.Add(saveCommand);
            var titleCommand = new SystemData.CommandData
            {
                Id = 5,
                Name = DataSystem.GetText(19720),
                Key = "Title"
            };
            list.Add(titleCommand);
            return list;
        }
    }
}