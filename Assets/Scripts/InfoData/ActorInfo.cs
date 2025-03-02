using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ryneus
{
    [Serializable]
    public class ActorInfo
    {
        public ActorData Master => DataSystem.FindActor(ActorId.Value);
        public ParameterInt ActorId = new();

        public ParameterInt Exp = new();
        public int NextExp => 100 - Exp.Value % 100;
        public int Level => (Exp.Value / 100) + 1;
        public void SetLevel(int level)
        {
            Exp.SetValue((level-1) * 100);
        }

        [SerializeField] private List<ParameterInt> _equipmentSkillIds = new ();
        public List<ParameterInt> EquipmentSkillIds => _equipmentSkillIds;
        public void ChangeEquipSkill(int changeSkillId,int removeSkillId)
        {
            var findIndex = _equipmentSkillIds.FindIndex(a => a.Value == removeSkillId);
            if (findIndex > -1)
            {
                _equipmentSkillIds.RemoveAt(findIndex);
            }
            var insertIndex = findIndex > -1 ? findIndex : _equipmentSkillIds.Count;
            if (_equipmentSkillIds.Find(a => a.Value == changeSkillId) == null)
            {
                var insert = new ParameterInt(changeSkillId);
                _equipmentSkillIds.Insert(insertIndex,insert);
            }
        }

        public StatusInfo CurrentStatus => LevelUpStatus(Level);
        public int MaxHp => CurrentStatus.Hp;
        public int MaxMp => CurrentStatus.Mp;
        public ParameterInt CurrentHp = new();
        public ParameterInt CurrentMp = new();

        public List<AttributeRank> GetAttributeRank()
        {
            var list = new List<AttributeRank>();
            foreach (var attribute in Master.Attribute)
            {
                list.Add(attribute);
            }
            return list;
        }

        public List<int> LearnSkillIds()
        {
            var list = new List<int>();
            return list;
        }

        private int _lastSelectSkillId = 0;
        public int LastSelectSkillId => _lastSelectSkillId;
        public void SetLastSelectSkillId(int selectSkillId)
        {
            _lastSelectSkillId = selectSkillId;
        }

        private LineType _lineIndex = LineType.Front;
        public LineType LineIndex => _lineIndex;
        public void SetLineIndex(LineType lineIndex)
        {
            _lineIndex = lineIndex;
        }

        // バトル勝利数
        public int DemigodParam => 0;

        public ParameterInt BattleIndex = new();
        private StatusInfo _plusStatus = new();

        public ActorInfo(ActorData actorData)
        {
            if (actorData == null)
            {
                return;
            }
            ActorId.SetValue(actorData.Id);
            SetInitialParameter(actorData);
            CurrentHp.SetValue(Master.InitStatus.Hp);
            CurrentMp.SetValue(Master.InitStatus.Mp);
            InitSkillInfo();
            InitSkillTriggerInfos();
        }

        public void CopyData(ActorInfo baseActorInfo)
        {
            _plusStatus.SetParameter(
                baseActorInfo._plusStatus.GetParameter(StatusParamType.Hp),
                baseActorInfo._plusStatus.GetParameter(StatusParamType.Mp),
                baseActorInfo._plusStatus.GetParameter(StatusParamType.Atk),
                baseActorInfo._plusStatus.GetParameter(StatusParamType.Def),
                baseActorInfo._plusStatus.GetParameter(StatusParamType.Spd)
            );
            _lastSelectSkillId = baseActorInfo.LastSelectSkillId;
            CurrentHp.SetValue(baseActorInfo.CurrentHp.Value);
            CurrentMp.SetValue(baseActorInfo.CurrentMp.Value);
            BattleIndex.SetValue(baseActorInfo.BattleIndex.Value);
            _lineIndex = baseActorInfo._lineIndex;
            _skillTriggerInfos = baseActorInfo._skillTriggerInfos;
        }

        private void SetInitialParameter(ActorData actorData)
        {
            _plusStatus.SetParameter(actorData.PlusStatus.Hp,actorData.PlusStatus.Mp,actorData.PlusStatus.Atk,actorData.PlusStatus.Def,actorData.PlusStatus.Spd);
        }

        private void InitSkillInfo()
        {
            _lastSelectSkillId = 0;
            SkillInfo selectSkill = null;
            foreach (var skillInfo in LearningSkillInfos())
            {
                if (selectSkill != null && skillInfo.Id.Value > 1000)
                {
                    _lastSelectSkillId = selectSkill.Id.Value;
                }
                if (skillInfo.LearningState == LearningState.Learned)
                {
                    var learned = new ParameterInt(skillInfo.Id.Value);
                    _equipmentSkillIds.Add(learned);
                }
            }
        }

        public List<SkillInfo> ChangeAbleSkills()
        {
            return LearningSkillInfos().FindAll(a => _equipmentSkillIds.Find(b => b.Value == a.Id.Value) == null);
        }

        /// <summary>
        /// Lvアップで習得する魔法リスト
        /// </summary>
        /// <returns></returns>
        public List<SkillInfo> LearningSkillInfos()
        {
            var list = new List<SkillInfo>();
            foreach (var learningData in Master.LearningSkills)
            {
                if (learningData.SkillId < 1000) continue;
                if (list.Find(a => a.Id.Value == learningData.SkillId) != null) continue;
                if (LearnSkillIds().Contains(learningData.SkillId)) continue;
                var skillInfo = new SkillInfo(learningData.SkillId);
                if (Level >= learningData.Level)
                {
                    skillInfo.SetLearningState(LearningState.Learned);
                    skillInfo.SetEnable(true);
                } else
                {
                    skillInfo.LearningLv.SetValue(learningData.Level);
                    skillInfo.SetLearningState(LearningState.NotLearn);
                    skillInfo.SetEnable(false);
                }
                list.Add(skillInfo);
            }
            return list;
        }

        public LevelUpInfo LevelUp(int useCost,int stageId,int seek,int seekIndex)
        {
            var levelUpInfo = new LevelUpInfo
            (
                ActorId.Value,useCost,stageId,seek,seekIndex
            );
            levelUpInfo.SetLevel(Level);
            ChangeHp(CurrentParameter(StatusParamType.Hp));
            ChangeMp(CurrentParameter(StatusParamType.Mp));
            return levelUpInfo;
        }

        public StatusInfo LevelUpStatus(int level)
        {
            return LevelUpStatusInfo(level);
        }

        private StatusInfo LevelUpStatusInfo(int level)
        {
            var statusInfo = new StatusInfo();
            if (Master != null)
            {
                statusInfo.AddParameter(StatusParamType.Hp,Master.InitStatus.Hp);
                statusInfo.AddParameter(StatusParamType.Mp,Master.InitStatus.Mp);
                statusInfo.AddParameter(StatusParamType.Atk,Master.InitStatus.Atk);
                statusInfo.AddParameter(StatusParamType.Def,Master.InitStatus.Def);
                statusInfo.AddParameter(StatusParamType.Spd,Master.InitStatus.Spd);
            
                statusInfo.AddParameter(StatusParamType.Hp,LevelGrowthRate(StatusParamType.Hp,level)); 
                statusInfo.AddParameter(StatusParamType.Mp,LevelGrowthRate(StatusParamType.Mp,level));  
                statusInfo.AddParameter(StatusParamType.Atk,LevelGrowthRate(StatusParamType.Atk,level));  
                statusInfo.AddParameter(StatusParamType.Def,LevelGrowthRate(StatusParamType.Def,level));  
                statusInfo.AddParameter(StatusParamType.Spd,LevelGrowthRate(StatusParamType.Spd,level));     
            }
            return statusInfo;
        }

        public int LevelGrowthRate(StatusParamType statusParamType,int level)
        {
            return (int)Mathf.Round(Master.NeedStatus.GetParameter(statusParamType) * 0.01f * (level-1));
        }

        public List<SkillInfo> LearningSkills(int plusLv = 0)
        {
            return LearningSkillInfos().FindAll(a => a.LearningState == LearningState.NotLearn && a.LearningLv.Value <= (Level+plusLv));
        }

        public bool IsLearnedSkill(int skillId)
        {
            var learnedSkill = LearningSkillInfos().FindAll(a => a.LearningState == LearningState.Learned);
            return LearnSkillIds().Contains(skillId) || learnedSkill.Find(a => a.Id.Value == skillId) != null;
        }

        public LevelUpInfo LearnSkill(int skillId,int cost,int stageId,int seek,int seekIndex = -1)
        {
            var skillLevelUpInfo = new LevelUpInfo(ActorId.Value,cost,stageId,seek,seekIndex);
            skillLevelUpInfo.SetSkillId(skillId);
            return skillLevelUpInfo;
        }

        public int CurrentParameter(StatusParamType statusParamType)
        {
            return LevelUpStatus(Level).GetParameter(statusParamType);
        }

        public void ChangeHp(int hp)
        {
            CurrentHp.SetValue(Math.Min(hp,CurrentParameter(StatusParamType.Hp)));
        }

        public void ChangeMp(int mp)
        {
            CurrentMp.SetValue(Math.Min(mp,CurrentParameter(StatusParamType.Mp)));
        }

        public List<AttributeRank> AttributeRanks(List<ActorInfo> actorInfos)
        {
            var alchemyFeatures = new List<SkillData.FeatureData>();
            if (actorInfos != null)
            {
                foreach (var actorInfo in actorInfos)
                {
                    var skillInfos = actorInfo.LearningSkillInfos().FindAll(a => a.Master.FeatureDates.Find(b => b.FeatureType == FeatureType.MagicAlchemy)!= null);
                    foreach (var skillInfo in skillInfos)
                    {
                        foreach (var featureData in skillInfo.Master.FeatureDates)
                        {
                            if (featureData.FeatureType == FeatureType.MagicAlchemy)
                            {
                                alchemyFeatures.Add(featureData);
                            }
                        }
                    }
                }
            }
            var attributeValues = new List<AttributeRank>();
            int idx = 1;
            foreach (var attribute in GetAttributeRank())
            {
                var attributeValue = attribute;
                foreach (var alchemyFeature in alchemyFeatures)
                {
                    if (alchemyFeature.Param2 == idx)
                    {
                        attributeValue -= alchemyFeature.Param3;
                    }
                }
                if (attributeValue < 0)
                {
                    attributeValue = AttributeRank.S;
                }
                attributeValues.Add(attributeValue);
                idx++;
            }
            return attributeValues;
        }

        private List<int> AlchemyAttributeRates(List<ActorInfo> actorInfos)
        {
            var attributeRanks = AttributeRanks(actorInfos);
            var rateList = new List<int>();
            foreach (var attributeRank in attributeRanks)
            {
                var rate = (int)AttributeRank.G - (int)attributeRank;
                rateList.Add(rate * 50);
            }
            return rateList;
        }

        public AttributeType AlchemyAttribute(List<ActorInfo> actorInfos)
        {
            var alchemyAttributeRates = AlchemyAttributeRates(actorInfos);
            int targetRand = UnityEngine.Random.Range(0,alchemyAttributeRates.Sum(a => a));
            int targetIndex = -1;
            for (int i = 0;i < alchemyAttributeRates.Count;i++)
            {
                targetRand -= alchemyAttributeRates[i];
                if (targetRand <= 0 && targetIndex == -1)
                {
                    targetIndex = i;
                }
            }
            return (AttributeType)(targetIndex+1);
        }

        public int Evaluate()
        {
            int statusValue = CurrentParameter(StatusParamType.Hp) * 7
            + CurrentParameter(StatusParamType.Mp) * 4
            + CurrentParameter(StatusParamType.Atk) * 8
            + CurrentParameter(StatusParamType.Def) * 8
            + CurrentParameter(StatusParamType.Spd) * 8;
            float magicValue = 0;
            foreach (var skillInfo in LearningSkillInfos())
            {
                if (skillInfo.LearningState == LearningState.Learned)
                {
                    var rate = 1.0f;
                    if (skillInfo.Attribute != AttributeType.None)
                    {
                        switch (Master.Attribute[(int)skillInfo.Attribute-1])
                        {
                            case AttributeRank.S:
                            case AttributeRank.A:
                                rate = 1.1f;
                                break;
                            case AttributeRank.B:
                            case AttributeRank.C:
                                rate = 0.9f;
                                break;
                            case AttributeRank.D:
                            case AttributeRank.E:
                            case AttributeRank.F:
                                rate = 0.8f;
                                break;
                            case AttributeRank.G:
                                rate = 0.7f;
                                break;
                        }
                    }
                    magicValue += rate * 100;
                    if (skillInfo.IsBattleSpecialSkill())
                    {
                        magicValue += 200;
                    }
                }
            }
            int total = statusValue + (int)magicValue + DemigodParam * 10;
            return total;
        }
    
        private List<SkillTriggerInfo> _skillTriggerInfos = new ();
        public List<SkillTriggerInfo> SkillTriggerInfos => _skillTriggerInfos;

        public void InitSkillTriggerInfos()
        {
            var skillTriggerDates = Master.SkillTriggerDates;
            for (int i = 0;i < skillTriggerDates.Count;i++)
            {
                var skillTriggerData = skillTriggerDates[i];
                var skillTriggerInfo = new SkillTriggerInfo(ActorId.Value,new SkillInfo(skillTriggerData.SkillId));
                skillTriggerInfo.SetPriority(i);
                var skillTriggerData1 = DataSystem.SkillTriggers.Find(a => a.Id == skillTriggerData.Trigger1);
                var skillTriggerData2 = DataSystem.SkillTriggers.Find(a => a.Id == skillTriggerData.Trigger2);
                skillTriggerInfo.UpdateTriggerDates(new List<SkillTriggerData>(){skillTriggerData1,skillTriggerData2});
                _skillTriggerInfos.Add(skillTriggerInfo);
            }
        }

        public void AddSkillTriggerSkill(int skillId)
        {
            for (int i = 0;i < _skillTriggerInfos.Count;i++)
            {
                var skillTriggerInfo = _skillTriggerInfos[i];
                if (skillTriggerInfo.SkillId == 0)
                {
                    var skillInfo = new SkillInfo(skillId);
                    // アクティブか覚醒なら自動で加える
                    if (skillInfo.IsBattleActiveSkill())
                    {
                        skillTriggerInfo.SetSkillInfo(new SkillInfo(skillId));
                        break;
                    }
                }
            }
        }
    
        public void SetSkillTriggerSkill(int index,SkillInfo skillInfo)
        {
            if (_skillTriggerInfos.Count > index)
            {
                _skillTriggerInfos[index].SetSkillInfo(skillInfo);
            }
        }
        
        public void SetSkillTriggerTrigger(int index,int triggerIndex,SkillTriggerData triggerType)
        {
            if (_skillTriggerInfos.Count > index)
            {
                var triggerTypes = _skillTriggerInfos[index].SkillTriggerDates;
                SkillTriggerData triggerData1 = null;
                SkillTriggerData triggerData2 = null;
                if (triggerIndex == 1)
                {
                    if (triggerType == null && triggerTypes[1] != null)
                    {
                        triggerData1 = triggerTypes[1];
                        triggerData2 = triggerType;
                    } else
                    {
                        triggerData1 = triggerType;
                        triggerData2 = triggerTypes[1];
                    }
                } else
                if (triggerIndex == 2)
                {
                    triggerData1 = triggerTypes[0];
                    triggerData2 = triggerType;
                }
                var list = new List<SkillTriggerData>
                {
                    triggerData1,
                    triggerData2
                };
                _skillTriggerInfos[index].UpdateTriggerDates(list);
            }
        }

        public void SetTriggerIndexUp(int index)
        {
            if (index > 0)
            {
                var upTriggerData = _skillTriggerInfos[index];
                var downTriggerData = _skillTriggerInfos[index - 1];
                upTriggerData.SetPriority(index-1);
                downTriggerData.SetPriority(index);
            }
            _skillTriggerInfos.Sort((a,b) => a.Priority - b.Priority > 0 ? 1 : -1);
        }

        public void SetTriggerIndexDown(int index)
        {
            if (index+1 >= _skillTriggerInfos.Count)
            {
                return;
            }
            var upTriggerData = _skillTriggerInfos[index+1];
            var downTriggerData = _skillTriggerInfos[index];
            upTriggerData.SetPriority(index);
            downTriggerData.SetPriority(index+1);
            _skillTriggerInfos.Sort((a,b) => a.Priority - b.Priority > 0 ? 1 : -1);
        }

        private void InsertSkillTriggerSkills(List<SkillInfo> skillInfos,bool isOnlyCheckEnemy = true)
        {
            foreach (var learnSkill in skillInfos)
            {
                if (_skillTriggerInfos.Find(a => a.SkillId == learnSkill.Id.Value) == null)
                {
                    var skillTriggerInfo = new SkillTriggerInfo(ActorId.Value,new SkillInfo(learnSkill.Id.Value));
                    var skillTriggerData1 = DataSystem.SkillTriggers.Find(a => a.Id == 0);
                    var skillTriggerData2 = DataSystem.SkillTriggers.Find(a => a.Id == 0);
                    // 敵データに同じスキルがあればコピーする

                    var enemyDates = DataSystem.Enemies.FindAll(a => a.SkillTriggerDates.Find(b => b.SkillId == learnSkill.Id.Value) != null);
                    if (enemyDates.Count > 0)
                    {
                        var enemyData = enemyDates[enemyDates.Count-1];
                        var skillTriggerData = enemyData.SkillTriggerDates.Find(a => a.SkillId == learnSkill.Id.Value);
                        skillTriggerData1 = DataSystem.SkillTriggers.Find(a => a.Id == skillTriggerData.Trigger1);
                        skillTriggerData2 = DataSystem.SkillTriggers.Find(a => a.Id == skillTriggerData.Trigger2);
                    }
                    skillTriggerInfo.UpdateTriggerDates(new List<SkillTriggerData>(){skillTriggerData1,skillTriggerData2});
            
                    var findIndex = _skillTriggerInfos.FindIndex(a => DataSystem.Skills[a.SkillId].SkillType == SkillType.Active);
                    if (findIndex == -1)
                    {
                        findIndex = 1;
                    }
                    findIndex++;
                    // パッシブは条件を設定している場合にのみ挿入する
                    if (learnSkill.Master.SkillType == SkillType.Passive)
                    {
                        if (skillTriggerData1.Id == 0)
                        {
                            continue;
                        }
                    }
                    if (isOnlyCheckEnemy == false)
                    {
                        _skillTriggerInfos.Insert(findIndex,skillTriggerInfo);
                    } else
                    {
                        if (enemyDates.Count > 0)
                        {
                            _skillTriggerInfos.Insert(findIndex,skillTriggerInfo);
                        }
                    }
                }
            }
        }
        
        public void RecommendActiveSkill()
        {
            _skillTriggerInfos.Clear();
            // 初期設定に戻す
            InitSkillTriggerInfos();
            var addActive = LearningSkillInfos().FindAll(a => a.Master.SkillType == SkillType.Active && a.Id.Value > 1000 && a.LearningState == LearningState.Learned);
            // 新たに追加したアクティブをアクティブの下に入れる
            InsertSkillTriggerSkills(addActive,false);
            var addPassive = LearningSkillInfos().FindAll(a => a.Master.SkillType == SkillType.Passive && a.Id.Value > 1000 && a.LearningState == LearningState.Learned);
            
            // その他のパッシブを加える
            InsertSkillTriggerSkills(addPassive,true);

            for (int i = 0;i < _skillTriggerInfos.Count;i++)
            {
                _skillTriggerInfos[i].SetPriority(i);
            }
        }
    }
    
    public enum AttributeRank 
    {
        S = 0,
        A = 1,
        B = 2,
        C = 3,
        D = 4,
        E = 5,
        F = 6,
        G = 7
    }
}    

