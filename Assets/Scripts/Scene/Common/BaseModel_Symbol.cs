using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ryneus
{
    public partial class BaseModel
    {

        public List<HexUnitInfo> StageHexUnitInfos(List<StageSymbolData> stageSymbolDates,int clearCount = 0)
        {
            var symbolDates = stageSymbolDates.FindAll(a => a.InitX >= 0 && a.InitY >= 0 && a.ClearCount <= clearCount);
            var hexUnitInfos = new List<HexUnitInfo>();
            for (int i = 0;i < symbolDates.Count;i++)
            {
                var symbolMaster = symbolDates[i];
                var hexUnitInfo = new HexUnitInfo(i,symbolMaster);
                var randFlag = false;
                var stageSymbolData = new StageSymbolData();
                stageSymbolData.CopyData(symbolMaster);
                /*
                // グループ指定
                if (stageSymbolData.IsGroupSymbol())
                {
                    var groupId = (int)stageSymbolData.SymbolType;
                    var groupDates = DataSystem.SymbolGroups.FindAll(a => a.GroupId == groupId);
                    stageSymbolData = PickUpSymbolData(groupDates,symbolMaster);
                    hexUnitInfo = new HexUnitInfo(i,stageSymbolData);
                }
                if (stageSymbolData.SymbolType == SymbolType.Random)
                {
                    stageSymbolData = RandomSymbolData(stageSymbolDates,symbolMaster);
                    hexUnitInfo = new HexUnitInfo(i,stageSymbolData);
                }
                */
                // 報酬リスト
                var getItemInfos = new List<GetItemInfo>();
                switch (symbolMaster.UnitType)
                {
                    case HexUnitType.Battler:
                        // 敵ユニット
                        hexUnitInfo.SetTeamState(TeamState.Away);
                        hexUnitInfo.SetTroopInfo(BattleTroop(stageSymbolData));
                        if (randFlag)
                        {
                            var numinosGetItem = MakeEnemyRandomNuminos(stageSymbolData.StageId,stageSymbolData.InitX);
                            hexUnitInfo.TroopInfo.AddGetItemInfo(numinosGetItem);                  
                        }
                        
                        if (hexUnitInfo.TroopInfo != null && hexUnitInfo.TroopInfo.GetItemInfos.Count > 0)
                        {
                            getItemInfos.AddRange(hexUnitInfo.TroopInfo.GetItemInfos);
                        }
                        break;
                }
                foreach (var getItemInfo in hexUnitInfo.GetItemInfos)
                {
                    switch (getItemInfo.GetItemType)
                    {
                        case GetItemType.SelectRelic:
                            getItemInfos.AddRange(MakeSelectRelicGetItemInfos((RankType)getItemInfo.Param2,(AttributeType)getItemInfo.Param1));
                            break;
                        case GetItemType.SelectSkill:
                            getItemInfos.AddRange(MakeSelectSkillGetItemInfos((RankType)getItemInfo.Param2,(AttributeType)getItemInfo.Param1));
                            break;
                    }
                }
                hexUnitInfo.AddGetItemInfos(getItemInfos);
                hexUnitInfos.Add(hexUnitInfo);
            }
            return hexUnitInfos;
        }

        public GetItemInfo MakeGetItemInfo(GetItemType getItemType,int param1)
        {
            var getItemData = new GetItemData
            {
                Type = getItemType,
                Param1 = param1
            };
            return new GetItemInfo(getItemData);
        }



        private List<GetItemInfo> MakeSelectActorGetItemInfos(bool freeSelect)
        {
            var getItemInfos = new List<GetItemInfo>
            {
                // タイトル表示用
                MakeGetItemInfo(GetItemType.SelectAddActor, -1)
            };
            // 表示用に報酬設定
            if (freeSelect)
            {
                // 自由選択
            } else
            {
                // 選択できるアクターが3人まで
                /*
                var pastActorIdList = PartyInfo.PastActorIdList(CurrentStage.Id,CurrentStage.Seek,CurrentStage.WorldType);
                var actorInfos = PartyInfo.ActorInfos.FindAll(a => !pastActorIdList.Contains(a.ActorId));
                var count = 3;
                if (actorInfos.Count < count)
                {
                    count = actorInfos.Count;
                }
                if (count == 0)
                {
                    // 報酬設定
                    getItemInfos.Add(MakeGetItemInfo(GetItemType.Numinous,20));
                } else
                {
                    while (getItemInfos.Count <= count)
                    {
                        var rand = Random.Range(0,actorInfos.Count);
                        if (getItemInfos.Find(a => a.Param1 == actorInfos[rand].ActorId) == null)
                        {
                            getItemInfos.Add(MakeGetItemInfo(GetItemType.AddActor,actorInfos[rand].ActorId));
                        }
                    }
                }
                */
            }
            return getItemInfos;
        }

        private List<GetItemInfo> MakeSelectRelicGetItemInfos(RankType rankType,AttributeType attributeType = AttributeType.None)
        {
            var getItemInfos = new List<GetItemInfo>
            {
                // タイトル表示用
                MakeGetItemInfo(GetItemType.SelectRelic, -1)
            };
            var rank = rankType;
            var learningSkillIds = PartyInfo.LearningSkillIds;
            var skills = DataSystem.Skills.Where(a => a.Value.Rank == rank && a.Value.Id % 10 == 0 && !learningSkillIds.Contains(a.Value.Id)).ToList();
            if (attributeType != AttributeType.None)
            {
                skills = skills.FindAll(a => a.Value.Attribute == attributeType);
            }
            var count = 3;
            if (skills.Count < count)
            {
                count = skills.Count;
            }
            if (count == 0)
            {
                // 報酬設定
                //getItemInfos.Add(MakeGetItemInfo(GetItemType.Numinous,20));
            } else
            {
                while (getItemInfos.Count <= count)
                {
                    var rand = Random.Range(0,skills.Count);
                    if (getItemInfos.Find(a => a.Param1 == skills[rand].Value.Id) == null)
                    {
                        // 報酬設定
                        var getItemInfo = MakeGetItemInfo(GetItemType.SelectRelic,skills[rand].Value.Id);
                        if (getItemInfos.Find(a => a.Param1 == skills[rand].Value.Id) == null)
                        {
                            getItemInfos.Add(getItemInfo);
                        }
                    }
                }
            }
            return getItemInfos;
        }
        
        private List<GetItemInfo> MakeSelectSkillGetItemInfos(RankType rankType,AttributeType attributeType = AttributeType.None)
        {
            var getItemInfos = new List<GetItemInfo>{};
            var rank = rankType;
            var learningSkillIds = PartyInfo.LearningSkillIds;
            var skills = DataSystem.Skills.Where(a => a.Value.Rank == rank && a.Value.Id % 10 == 0 && !learningSkillIds.Contains(a.Value.Id)).ToList();
            if (attributeType != AttributeType.None)
            {
                skills = skills.FindAll(a => a.Value.Attribute == attributeType);
            }
            var count = 1;
            if (skills.Count < count)
            {
                count = skills.Count;
            }
            if (count == 0)
            {
                // 報酬設定
                //getItemInfos.Add(MakeGetItemInfo(GetItemType.Numinous,20));
            } else
            {
                while (getItemInfos.Count < count)
                {
                    var rand = Random.Range(0,skills.Count);
                    if (getItemInfos.Find(a => a.Param1 == skills[rand].Value.Id) == null)
                    {
                        // 報酬設定
                        var getItemInfo = MakeGetItemInfo(GetItemType.Skill,skills[rand].Value.Id);
                        if (getItemInfos.Find(a => a.Param1 == skills[rand].Value.Id) == null)
                        {
                            getItemInfos.Add(getItemInfo);
                        }
                    }
                }
            }
            return getItemInfos;
        }
        
        private StageSymbolData PickUpSymbolData(List<SymbolGroupData> groupDates,StageSymbolData symbolMaster)
        {
            int targetRand = Random.Range(0,groupDates.Sum(a => a.Rate));
            int targetIndex = -1;
            for (int i = 0;i < groupDates.Count;i++)
            {
                targetRand -= groupDates[i].Rate;
                if (targetRand <= 0 && targetIndex == -1)
                {
                    targetIndex = i;
                }
            }
            var stageSymbolData = new StageSymbolData
            {
                StageId = symbolMaster.StageId,
                InitX = symbolMaster.InitX,
                InitY = symbolMaster.InitY
            };
            stageSymbolData.ConvertSymbolGroupData(groupDates[targetIndex]);
            return stageSymbolData;
        }

        private StageSymbolData RandomSymbolData(List<StageSymbolData> stageSymbolDates,StageSymbolData symbolMaster)
        {
            // 候補を生成
            var stageSymbolList = stageSymbolDates.FindAll(a => a.InitX == 0);
            var stageSymbolData = new StageSymbolData
            {
                //SymbolType = SymbolType.None,
                StageId = symbolMaster.StageId,
                InitX = symbolMaster.InitX,
                InitY = symbolMaster.InitY
            };

            return stageSymbolData;
        }
        
        private TroopInfo BattleTroop(StageSymbolData stageSymbolData)
        {
            var troopId = stageSymbolData.Param1;
            var plusLv = 0;//PartyInfo.BattleEnemyLv(stageSymbolData.StageId,stageSymbolData.Seek,WorldType.Main);
            //var plusNuminos = 0;//PartyInfo.BattleEnemyLv(stageSymbolData.StageId,stageSymbolData.Seek,WorldType.Main);
            var troopInfo = new TroopInfo(troopId);
            var stageData = DataSystem.Stages.Find(a => a.Id == stageSymbolData.StageId);
            var lv = stageData.StageLv + plusLv;
            // ランダム生成
            if (troopInfo.TroopMaster == null)
            {
                troopInfo.MakeEnemyRandomTroopDates(stageSymbolData.InitX + lv,stageData.RandomTroopEnemyRates);
                //var numinosGetItem = MakeEnemyRandomNuminos(stageSymbolData.StageId,stageSymbolData.Seek);
                //troopInfo.AddGetItemInfo(numinosGetItem);
                // ランダム報酬データ設定
                // 70 = Rank1 Passive,
                // 7 = Rank2 Passive,
                // 15 = Rank1 Active,
                // 3 = Rank2 Active
                // 3 = Rank1 Enhance
                // 2 = Rank2 Enhance
                var getItemData = MakeSkillGetItemInfo();
                if (getItemData != null)
                {
                    troopInfo.AddGetItemInfo(new GetItemInfo(getItemData));
                }
                return troopInfo;
            }
            if (troopInfo.TroopMaster == null)
            {
                Debug.LogError("troopId" + troopId + "のデータが不足");
            } else
            {
                troopInfo.MakeEnemyTroopDates(lv);
            }
            return troopInfo;
        }

        private GetItemInfo MakeEnemyRandomNuminos(int stageId,int seek)
        {
            var numinosBonus = 0;//PartyInfo.BattleNuminosBonus(stageId,seek,WorldType.Main);
            var totalScore = 0;//(int)PartyInfo.TotalScore(WorldType.Main);
            // 確定報酬でNuminos
            var numinosGetItem = new GetItemData
            {
                Param1 = totalScore + seek + numinosBonus,
                Type = GetItemType.Currency
            };
            return new GetItemInfo(numinosGetItem);
        }

        private GetItemData MakeSkillGetItemInfo()
        {
            int rand = Random.Range(0, 100);
            GetItemData getItemData;
            if (rand < 70)
            {
                getItemData = AddSkillGetItemData(RankType.PassiveRank1);
            }
            else
            if (rand >= 70 && rand < 80)
            {
                getItemData = AddSkillGetItemData(RankType.PassiveRank2);
            }
            else
            if (rand >= 80 && rand < 85)
            {
                getItemData = AddSkillGetItemData(RankType.ActiveRank2);
            }
            else
            if (rand >= 85 && rand < 95)
            {
                getItemData = AddEnhanceSkillGetItemData(RankType.EnhanceRank1);
            }
            else
            {
                getItemData = AddEnhanceSkillGetItemData(RankType.EnhanceRank2);
            }
            // 候補なければ再抽選
            if (getItemData == null)
            {
                //return MakeSkillGetItemInfo();
            }
            return getItemData;
        }

        private GetItemData AddSkillGetItemData(RankType rankType)
        {
            var hasSkills = PartyInfo.LearningSkillIds;
            var skillList = new List<SkillData>(DataSystem.Skills.Values);
            var skills = skillList.FindAll(a => a.Rank == rankType && !hasSkills.Contains(a.Id));
            if (skills.Count > 0)
            {
                var skillRand = Random.Range(0,skills.Count);
                var getItemData = new GetItemData
                {
                    Param1 = skills[skillRand].Id,
                    Type = GetItemType.Skill
                };
                return getItemData;
            }
            return null;
        }

        private GetItemData AddEnhanceSkillGetItemData(RankType rankType)
        {
            /*
            var hasSkills = PartyInfo.CurrentAlchemyIdList(CurrentStage.Id,CurrentStage.Seek,CurrentStage.WorldType);
            var skillList = new List<SkillData>(DataSystem.Skills.Values);
            var skills = skillList.FindAll(a => a.Rank == rankType && !hasSkills.Contains(a.Id));
            var allSkillIds = PartyInfo.CurrentAllSkillIds(CurrentStage.Id,CurrentStage.Seek,CurrentStage.WorldType);
            // 強化可能な所持魔法に絞る
            skills = skills.FindAll(a => allSkillIds.Contains(a.Id - 400000));
            if (skills.Count > 0)
            {
                var skillRand = Random.Range(0,skills.Count);
                var getItemData = new GetItemData
                {
                    Param1 = skills[skillRand].Id,
                    Type = GetItemType.Skill
                };
                return getItemData;
            }
            */
            return null;
        }
    }
}
