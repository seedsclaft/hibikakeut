using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ryneus
{
    [Serializable]
    public class ActorInfo
    {
        private ActorData _master = null;
        public ActorData Master => _master == null ? DataSystem.FindActor(ActorId.Value) : _master;
        public ParameterInt ActorId = new();

        public ParameterInt Exp = new();
        public int NextExp => 100 - (Exp.Value % 100);
        public int BeforeExp => Exp.Value % 100;
        public int Level => (Exp.Value / 100) + 1;
        public void SetLevel(int level)
        {
            Exp.SetValue((level - 1) * 100);
        }

        [SerializeField] private List<int> _equipmentIds = new();
        public List<int> EquipmentIds => _equipmentIds;
        public void ChangeEquipment(int changeEquipmentId, int index)
        {
            if (_equipmentIds.Contains(changeEquipmentId))
            {
                return;
            }
            _equipmentIds[index] = changeEquipmentId;
        }

        public void RemoveEquipment(int removeEquipmentId)
        {
            var findIndex = _equipmentIds.FindIndex(a => a == removeEquipmentId);
            if (findIndex > -1)
            {
                _equipmentIds[findIndex] = 10;
            }
        }

        [SerializeField] private List<int> _mastarySkillIds = new();
        public List<int> MastarySkillIds => _mastarySkillIds;
        public void GainSkillMastary(int skillId)
        {
            if (_mastarySkillIds.Contains(skillId))
            {
                return;
            }
            _mastarySkillIds.Add(skillId);
        }
        private Dictionary<int, int> _mastarySkillExps = new();
        public bool GainSkillExp(int skillId, int gain)
        {
            if (!_mastarySkillExps.ContainsKey(skillId))
            {
                _mastarySkillExps[skillId] = 0;
            }
            _mastarySkillExps[skillId] += gain;
            if (_mastarySkillExps[skillId] >= NeedMastarySkillExp(skillId))
            {
                return true;
            }
            return false;
        }

        public int NeedMastarySkillExp(int skillId)
        {
            return 100;
            var basecost = EquipSkillCost(skillId, null, null);
            if (basecost == 0)
            {
                return 0;
            }
            return basecost * 10;
        }

        public float MastarySkillExp(int skillId)
        {
            if (!_mastarySkillExps.ContainsKey(skillId))
            {
                return 0;
            }
            return Math.Min(100, _mastarySkillExps[skillId]);
        }

        public float MastarySkillRate(int skillId)
        {
            if (IsLearnedSkill(skillId))
            {
                return 1;
            }
            if (!_mastarySkillExps.ContainsKey(skillId))
            {
                return 0;
            }
            var basecost = NeedMastarySkillExp(skillId);
            if (basecost == 0)
            {
                return 1;
            }
            return _mastarySkillExps[skillId] * 0.01f / (basecost * 0.01f);
        }

        public int GetSkillExp(AttributeType attributeType, int rate, List<ActorInfo> stageMembers)
        {
            var param = AttributeRanks(stageMembers)[(int)attributeType];
            var skillExpRate = TacticsUtility.AttributeRankSkillExp(param);
            return (int)MathF.Ceiling(rate * skillExpRate);
        }

        private List<ParameterInt> _equipmentSkillIds = new();
        public List<ParameterInt> EquipmentSkillIds => _equipmentSkillIds;
        public void ChangeEquipSkill(int changeSkillId, int removeSkillId)
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
                _equipmentSkillIds.Insert(insertIndex, insert);
            }
            GainSkillExp(changeSkillId, 0);
            RecommendActiveSkill();
            SortEquipmentSkillIds();
        }

        private void SortEquipmentSkillIds()
        {
            // Active > Awaken > Unique > Passive
            var sortIds = new List<ParameterInt>();
            var sortIds1 = new List<ParameterInt>();
            var sortIds2 = new List<ParameterInt>();
            var sortIds3 = new List<ParameterInt>();
            foreach (var equipmentSkillId in _equipmentSkillIds)
            {
                if (equipmentSkillId.Value < 1000)
                {
                    continue;
                }
                var skill = DataSystem.FindSkill(equipmentSkillId.Value);
                if (!skill.IsBattlePassiveSkill() && !skill.IsBattleSpecialSkill())
                {
                    sortIds1.Add(equipmentSkillId);
                    continue;
                }
                if (skill.IsBattleSpecialSkill())
                {
                    sortIds2.Add(equipmentSkillId);
                    continue;
                }
                if (skill.IsBattlePassiveSkill())
                {
                    sortIds3.Add(equipmentSkillId);
                    continue;
                }
            }
            sortIds.AddRange(sortIds1);
            sortIds.AddRange(sortIds2);
            sortIds.AddRange(sortIds3);
            _equipmentSkillIds = sortIds;
        }

        public StatusInfo CurrentStatus => LevelUpStatus(Level);
        public int MaxHp => CurrentStatus.Hp;
        public int MaxMp => CurrentStatus.Mp;
        public int MaxCost => CurrentStatus.Cost;
        public ParameterInt CurrentHp = new();
        public ParameterInt CurrentMp = new();
        public ParameterInt CurrentCost = new();
        private List<int> _learnSkillIds = new();
        public List<int> LearnSkillIds => _learnSkillIds;
        public void LearnSkill(int skillId)
        {
            if (_learnSkillIds.Contains(skillId))
            {
                return;
            }
            _learnSkillIds.Add(skillId);
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

        public int DemigodParam => 0;
        public ParameterBool Lost = new(false);

        public ParameterInt BattleIndex = new();
        private StatusInfo _plusStatus = new();
        public StatusInfo PlusStatus => _plusStatus;

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
            CurrentCost.SetValue(Master.InitStatus.Cost);
            InitSkillInfo();
            InitSkillTriggerInfos();
            _equipmentIds.Add(10);
            _equipmentIds.Add(10);
        }

        public void CopyData(ActorInfo baseActorInfo)
        {
            _plusStatus.SetParameter(
                baseActorInfo._plusStatus.GetParameter(StatusParamType.Hp),
                baseActorInfo._plusStatus.GetParameter(StatusParamType.Mp),
                baseActorInfo._plusStatus.GetParameter(StatusParamType.Atk),
                baseActorInfo._plusStatus.GetParameter(StatusParamType.Def),
                baseActorInfo._plusStatus.GetParameter(StatusParamType.Spd),
                baseActorInfo._plusStatus.GetParameter(StatusParamType.Mov),
                baseActorInfo._plusStatus.GetParameter(StatusParamType.Cost),
                baseActorInfo._plusStatus.GetParameter(StatusParamType.Cri)
            );
            _lastSelectSkillId = baseActorInfo.LastSelectSkillId;
            CurrentHp.SetValue(baseActorInfo.CurrentHp.Value);
            CurrentMp.SetValue(baseActorInfo.CurrentMp.Value);
            CurrentCost.SetValue(baseActorInfo.CurrentCost.Value);
            BattleIndex.SetValue(baseActorInfo.BattleIndex.Value);
            _lineIndex = baseActorInfo._lineIndex;
            _skillTriggerInfos = baseActorInfo._skillTriggerInfos;
        }

        private void SetInitialParameter(ActorData actorData)
        {
            _plusStatus.SetParameter(actorData.PlusStatus);
        }

        public void AddStatusUpper(StatusParamType statusParamType, int upper)
        {
            _plusStatus.AddParameter(statusParamType, upper);
        }

        private void InitSkillInfo()
        {
            _lastSelectSkillId = 0;
            SkillInfo selectSkill = null;
            foreach (var skillInfo in LearningSkillInfos())
            {
                if (skillInfo.Id.Value < 1000)
                {
                    continue;
                }
                if (selectSkill != null)
                {
                    _lastSelectSkillId = selectSkill.Id.Value;
                }
                if (skillInfo.LearningState == LearningState.Learned)
                {
                    var learned = new ParameterInt(skillInfo.Id.Value);
                    _equipmentSkillIds.Add(learned);
                    _mastarySkillExps[learned.Value] = 0;
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
                if (learningData.SkillId < 1000)
                {
                    continue;
                }
                if (list.Find(a => a.Id.Value == learningData.SkillId) != null)
                {
                    continue;
                }

                var skillInfo = new SkillInfo(learningData.SkillId);
                if (Level >= learningData.Level && learningData.Level >= 0 || _learnSkillIds.Contains(learningData.SkillId))
                {
                    skillInfo.SetLearningState(LearningState.Learned);
                    skillInfo.PrimitiveLearned.SetValue(true);
                }
                else
                {
                    skillInfo.LearningLv.SetValue(learningData.Level);
                    skillInfo.SetLearningState(LearningState.NotLearn);
                }
                skillInfo.SetEnable(Level >= learningData.Level && learningData.Level >= 0 || _learnSkillIds.Contains(learningData.SkillId));
                list.Add(skillInfo);
            }
            return list;
        }

        public int EquipSkillCost(int skillId, List<ActorInfo> stageMembers, List<SkillInfo> buildingSkills)
        {
            var skillData = DataSystem.FindSkill(skillId);
            // 装備スキルなら
            if (skillData.SkillType == SkillType.Equip)
            {
                return SkillData.ConvertRankCost(skillData.Rank);
            }
            if (skillData.Attribute == AttributeType.None)
            {
                return 0;
            }
            // 自前で会得済みならコスト0
            if (IsLearnedSkill(skillId))
            {
                return 0;
            }
            var rankCost = SkillData.ConvertRankCost(skillData.Rank);
            var param = AttributeRanks(stageMembers)[(int)skillData.Attribute];
            var cost = TacticsUtility.EquipAttributeRankCost(param);
            int result = cost + rankCost - 1;
            // 会得済みなら-1
            if (_mastarySkillIds.Contains(skillId))
            {
                result -= 1;
            }
            // 施設効果で-1
            if (buildingSkills != null)
            {
                foreach (var buildingSkill in buildingSkills)
                {
                    var featureDates = buildingSkill.FeatureDates.FindAll(a => a.FeatureType == FeatureType.AttributeRateUp && a.Param1 == (int)skillData.Attribute);
                    if (featureDates.Count > 0)
                    {
                        result -= 1;
                        return Math.Max(0, result);
                    }
                }
            }
            return Math.Max(0, result);
        }

        public int LearningMagicCost(AttributeType attributeType, List<ActorInfo> stageMembers, RankType rank = RankType.None)
        {
            if (attributeType == AttributeType.None)
            {
                return 0;
            }
            var rankCost = SkillData.ConvertRankCost(rank);
            var param = AttributeRanks(stageMembers)[(int)attributeType];
            var cost = TacticsUtility.EquipAttributeRankCost(param);

            return cost + rankCost - 1;
        }

        public int EquipSlotCount()
        {
            IsClassChenged ??= new();
            return IsClassChenged.Value ? (DataSystem.System.EquipSkillCount + DataSystem.System.ClassChangePlusSkill) : DataSystem.System.EquipSkillCount;
        }

        public LevelUpInfo LevelUp(int useCost, int stageId)
        {
            var levelUpInfo = new LevelUpInfo
            (
                ActorId.Value, useCost, stageId
            );
            // 次のLvに必要なExpを加算
            Exp.GainValue(NextExp);
            levelUpInfo.SetLevel(Level);
            ChangeHp(CurrentParameter(StatusParamType.Hp));
            ChangeMp(CurrentParameter(StatusParamType.Mp));
            ChangeCost(CurrentParameter(StatusParamType.Cost));
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
                statusInfo.AddParameter(StatusParamType.Hp, Master.InitStatus.Hp);
                statusInfo.AddParameter(StatusParamType.Mp, Master.InitStatus.Mp);
                statusInfo.AddParameter(StatusParamType.Atk, Master.InitStatus.Atk);
                statusInfo.AddParameter(StatusParamType.Def, Master.InitStatus.Def);
                statusInfo.AddParameter(StatusParamType.Spd, Master.InitStatus.Spd);
                //statusInfo.AddParameter(StatusParamType.Mov, Master.InitStatus.Mov);
                //statusInfo.AddParameter(StatusParamType.Cost, Master.InitStatus.Cost);

                statusInfo.AddParameter(StatusParamType.Hp, LevelGrowthRate(StatusParamType.Hp, level));
                statusInfo.AddParameter(StatusParamType.Mp, LevelGrowthRate(StatusParamType.Mp, level));
                statusInfo.AddParameter(StatusParamType.Atk, LevelGrowthRate(StatusParamType.Atk, level));
                statusInfo.AddParameter(StatusParamType.Def, LevelGrowthRate(StatusParamType.Def, level));
                statusInfo.AddParameter(StatusParamType.Spd, LevelGrowthRate(StatusParamType.Spd, level));
                //statusInfo.AddParameter(StatusParamType.Mov, LevelGrowthRate(StatusParamType.Mov, level));
                //statusInfo.AddParameter(StatusParamType.Cost, LevelGrowthRate(StatusParamType.Cost, level));

                if (IsClassChenged.Value)
                {
                    // 1割増し
                    statusInfo.AddParameter(StatusParamType.Hp, statusInfo.Hp * 0.1f);
                    statusInfo.AddParameter(StatusParamType.Mp, statusInfo.Mp * 0.1f);
                    statusInfo.AddParameter(StatusParamType.Atk, statusInfo.Atk * 0.1f);
                    statusInfo.AddParameter(StatusParamType.Def, statusInfo.Def * 0.1f);
                    statusInfo.AddParameter(StatusParamType.Spd, statusInfo.Spd * 0.1f);
                    statusInfo.AddParameter(StatusParamType.Cost, statusInfo.Cost * 0.1f);
                }
                statusInfo.AddParameter(StatusParamType.Hp, _plusStatus.GetParameter(StatusParamType.Hp));
                statusInfo.AddParameter(StatusParamType.Mp, _plusStatus.GetParameter(StatusParamType.Mp));
                statusInfo.AddParameter(StatusParamType.Atk, _plusStatus.GetParameter(StatusParamType.Atk));
                statusInfo.AddParameter(StatusParamType.Def, _plusStatus.GetParameter(StatusParamType.Def));
                statusInfo.AddParameter(StatusParamType.Spd, _plusStatus.GetParameter(StatusParamType.Spd));
                //statusInfo.AddParameter(StatusParamType.Mov, _plusStatus.GetParameter(StatusParamType.Mov));
                //statusInfo.AddParameter(StatusParamType.Cost, _plusStatus.GetParameter(StatusParamType.Cost));
            }
            return statusInfo;
        }

        public int LevelGrowthRate(StatusParamType statusParamType, int level)
        {
            return (int)Mathf.Round(Master.NeedStatus.GetParameter(statusParamType) * 0.01f * (level - 1));
        }

        public List<SkillInfo> LearningSkills(int plusLv = 0)
        {
            return LearningSkillInfos().FindAll(a => a.LearningState == LearningState.NotLearn && a.LearningLv.Value != -1 && a.LearningLv.Value <= (Level + plusLv));
        }

        public List<SkillInfo> ActorInfoSkills(bool learnedSkillOnly = true)
        {
            var list = new List<SkillInfo>();
            var learnedSkills = LearningSkillInfos();
            // アクター習得済み
            foreach (var learnedSkill in learnedSkills)
            {
                if (learnedSkill.LearningState == LearningState.Learned)
                {
                    list.Add(learnedSkill);
                }
            }
            // 装備から習得済み
            foreach (var learnSkillId in MastarySkillIds)
            {
                var learnedSkill = new SkillInfo(learnSkillId);
                // 強化スキルなら除外
                var skillData = learnedSkill.Master;
                if (learnedSkill.FeatureDates.Find(a => a.FeatureType == FeatureType.EquipmentStatusUp) != null)
                {
                    continue;
                }
                learnedSkill.SetLearningState(LearningState.Learned);
                learnedSkill.SetEnable(true);
                list.Add(learnedSkill);
            }
            // 装備時発動
            foreach (var equipmentId in _equipmentIds)
            {
                var equipment = DataSystem.FindEquipment(equipmentId);
                foreach (var learningDate in equipment.LearningDates)
                {
                    if (!learningDate.EquipmentOnly)
                    {
                        continue;
                    }
                    var equipmentSkill = new SkillInfo(learningDate.SkillId);
                    equipmentSkill.SetLearningState(LearningState.Learned);
                    equipmentSkill.SetEnable(true);
                    list.Add(equipmentSkill);
                }
            }
            var insertIndex = list.FindAll(a => a.Id.Value > 1000).Count;
            // カインドを追加
            foreach (var kind in Master.Kinds)
            {
                if (kind > 0 && (int)kind < 10)
                {
                    var skillInfo = new SkillInfo((int)kind * 10 + 10100);
                    skillInfo.SetEnable(true);
                    list.Insert(insertIndex, skillInfo);
                    insertIndex++;
                }
            }
            // アクター未習得
            if (!learnedSkillOnly)
            {
                foreach (var learnedSkill in learnedSkills)
                {
                    if (learnedSkill.LearningState != LearningState.Learned)
                    {
                        list.Add(learnedSkill);
                    }
                }
            }
            return list;
        }

        public List<SkillInfo> SealedSkills()
        {
            return LearningSkillInfos().FindAll(a => a.LearningState == LearningState.NotLearn && a.LearningLv.Value == -1);
        }

        public bool IsLearnedSkill(int skillId)
        {
            var learnedSkill = LearningSkillInfos().FindAll(a => a.LearningState == LearningState.Learned);
            return _learnSkillIds.Contains(skillId) || learnedSkill.Find(a => a.Id.Value == skillId) != null;
        }

        public LevelUpInfo LearnSkill(int skillId, int cost, int stageId)
        {
            var skillLevelUpInfo = new LevelUpInfo(ActorId.Value, cost, stageId);
            skillLevelUpInfo.SetSkillId(skillId);
            return skillLevelUpInfo;
        }

        public int CurrentParameter(StatusParamType statusParamType)
        {
            return LevelUpStatus(Level).GetParameter(statusParamType);
        }

        public void ChangeHp(int hp)
        {
            CurrentHp.SetValue(Math.Min(hp, CurrentParameter(StatusParamType.Hp)));
        }

        public void ChangeMp(int mp)
        {
            CurrentMp.SetValue(Math.Min(mp, CurrentParameter(StatusParamType.Mp)));
        }

        public void ChangeCost(int cost)
        {
            CurrentCost.SetValue(Math.Min(cost, CurrentParameter(StatusParamType.Cost)));
        }

        // 属性適正
        private Dictionary<AttributeType, int> _attributeExp = new();
        private int GetAttributeExp(AttributeType attributeType)
        {
            if (!_attributeExp.ContainsKey(attributeType))
            {
                _attributeExp[attributeType] = 0;
            }
            return _attributeExp[attributeType] / 20;
        }

        public int NeedAttributeExp(AttributeType attributeType)
        {
            var need = 20;
            if (_attributeExp.ContainsKey(attributeType))
            {
                need *= _attributeExp[attributeType];
            }
            return need;
        }

        public bool GainAttributeExp(AttributeType attributeType, int count)
        {
            if (!_attributeExp.ContainsKey(attributeType))
            {
                _attributeExp[attributeType] = 0;
            }
            _attributeExp[attributeType] += count;
            if (_attributeExp[attributeType] >= NeedAttributeExp(attributeType))
            {
                return true;
            }
            return false;
        }

        private Dictionary<AttributeType, int> _attributeUpper = new();
        public void AddAttributeUpper(AttributeType attributeType)
        {
            if (!_attributeUpper.ContainsKey(attributeType))
            {
                _attributeUpper[attributeType] = 0;
            }
            //_attributeUpper[attributeType] += 1;
            // 装備しているコストを変更
        }

        public List<AttributeRank> GetAttributeRank()
        {
            var list = new List<AttributeRank>();
            foreach (var attribute in Master.Attribute)
            {
                list.Add(attribute);
            }
            return list;
        }

        public Dictionary<int, AttributeRank> AttributeRanks(List<ActorInfo> actorInfos)
        {
            var alchemyFeatures = new List<SkillData.FeatureData>();
            if (actorInfos != null)
            {
                foreach (var actorInfo in actorInfos)
                {
                    var skillInfos = actorInfo.LearningSkillInfos().FindAll(a => a.Master.FeatureDates.Find(b => b.FeatureType == FeatureType.MagicAlchemy) != null);
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
            var attributeValues = new Dictionary<int, AttributeRank>();
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
                if (_attributeUpper.ContainsKey((AttributeType)idx))
                {
                    attributeValue -= _attributeUpper[(AttributeType)idx];
                }
                attributeValue -= GetAttributeExp((AttributeType)idx);
                if (attributeValue < 0)
                {
                    attributeValue = AttributeRank.S;
                }
                attributeValues[idx] = attributeValue;
                idx++;
            }
            return attributeValues;
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
                        switch (Master.Attribute[(int)skillInfo.Attribute - 1])
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

        // Period間の魔法使用回数
        private Dictionary<int, int> _useSkillCountDict = new();

        public int GetSkillUseCount(int skillId)
        {
            return !_useSkillCountDict.ContainsKey(skillId) ? 0 : _useSkillCountDict[skillId];
        }

        public void SetSkillUseCount(int skillId, int count)
        {
            if (!_useSkillCountDict.ContainsKey(skillId))
            {
                _useSkillCountDict[skillId] = 0;
            }
            _useSkillCountDict[skillId] = count;
        }

        public void ClearSkillUseCount()
        {
            _useSkillCountDict.Clear();
        }

        public ParameterBool Transfer = new();

        public int TransferGetItem(int perPeriod)
        {
            return (Level * 2) / (perPeriod + 1);
        }

        public string TransferGetItemText(int perPeriod)
        {
            return "+" + TransferGetItem(perPeriod).ToString();
        }

        public int TransferGetExp(int chapter, int turns)
        {
            var baseLv = (chapter * 5) - Level;
            var baseExp = 100 + (baseLv * 10);
            var turnCount = turns + 1;
            return baseExp * turnCount;
        }

        public string TransferGetExpText(int chapter, int turns)
        {
            return "+" + TransferGetExp(chapter, turns);
        }

        public int TransferGetCurrency(int chapter, int turns)
        {
            var baseLv = Level - (chapter * 5);
            var baseExp = baseLv * 2;
            var turnCount = turns + 1;
            return Math.Max(1, baseExp) * turnCount;
        }

        public string TransferGetCurrencyText(int chapter, int turns)
        {
            return "+" + TransferGetCurrency(chapter, turns) + DataSystem.GetText(1000);
        }

        public ParameterBool IsClassChenged = new();

        private List<SkillTriggerInfo> _skillTriggerInfos = new();
        public List<SkillTriggerInfo> SkillTriggerInfos => _skillTriggerInfos;

        public void InitSkillTriggerInfos()
        {
            var skillTriggerDates = Master.SkillTriggerDates;
            for (int i = 0; i < skillTriggerDates.Count; i++)
            {
                var skillTriggerData = skillTriggerDates[i];
                var skillTriggerInfo = new SkillTriggerInfo(ActorId.Value, new SkillInfo(skillTriggerData.SkillId));
                skillTriggerInfo.SetPriority(i);
                var skillTriggerData1 = DataSystem.SkillTriggers.Find(a => a.Id == skillTriggerData.Trigger1);
                var skillTriggerData2 = DataSystem.SkillTriggers.Find(a => a.Id == skillTriggerData.Trigger2);
                skillTriggerInfo.UpdateTriggerDates(new List<SkillTriggerData>() { skillTriggerData1, skillTriggerData2 });
                _skillTriggerInfos.Add(skillTriggerInfo);
            }
        }

        public void AddSkillTriggerSkill(int skillId)
        {
            for (int i = 0; i < _skillTriggerInfos.Count; i++)
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

        public void SetSkillTriggerSkill(int index, SkillInfo skillInfo)
        {
            if (_skillTriggerInfos.Count > index)
            {
                _skillTriggerInfos[index].SetSkillInfo(skillInfo);
            }
        }

        public void SetSkillTriggerTrigger(int index, int triggerIndex, SkillTriggerData triggerType)
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
                    }
                    else
                    {
                        triggerData1 = triggerType;
                        triggerData2 = triggerTypes[1];
                    }
                }
                else
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
                upTriggerData.SetPriority(index - 1);
                downTriggerData.SetPriority(index);
            }
            _skillTriggerInfos.Sort((a, b) => a.Priority - b.Priority > 0 ? 1 : -1);
        }

        public void SetTriggerIndexDown(int index)
        {
            if (index + 1 >= _skillTriggerInfos.Count)
            {
                return;
            }
            var upTriggerData = _skillTriggerInfos[index + 1];
            var downTriggerData = _skillTriggerInfos[index];
            upTriggerData.SetPriority(index);
            downTriggerData.SetPriority(index + 1);
            _skillTriggerInfos.Sort((a, b) => a.Priority - b.Priority > 0 ? 1 : -1);
        }

        private void InsertSkillTriggerSkills(List<SkillInfo> skillInfos, bool isOnlyCheckEnemy = true)
        {
            foreach (var learnSkill in skillInfos)
            {
                if (_skillTriggerInfos.Find(a => a.SkillId == learnSkill.Id.Value) == null)
                {
                    var skillTriggerInfo = new SkillTriggerInfo(ActorId.Value, new SkillInfo(learnSkill.Id.Value));
                    var skillTriggerData1 = DataSystem.SkillTriggers.Find(a => a.Id == 0);
                    var skillTriggerData2 = DataSystem.SkillTriggers.Find(a => a.Id == 0);
                    // 敵データに同じスキルがあればコピーする

                    var enemyDates = DataSystem.Dates[DataType.Enemies].FindAll<EnemyData>(a => a.SkillTriggerDates.Find(b => b.SkillId == learnSkill.Id.Value) != null);
                    if (enemyDates.Count > 0)
                    {
                        var enemyData = enemyDates[enemyDates.Count - 1];
                        var skillTriggerData = enemyData.SkillTriggerDates.Find(a => a.SkillId == learnSkill.Id.Value);
                        skillTriggerData1 = DataSystem.SkillTriggers.Find(a => a.Id == skillTriggerData.Trigger1);
                        skillTriggerData2 = DataSystem.SkillTriggers.Find(a => a.Id == skillTriggerData.Trigger2);
                    }
                    skillTriggerInfo.UpdateTriggerDates(new List<SkillTriggerData>() { skillTriggerData1, skillTriggerData2 });

                    var findIndex = _skillTriggerInfos.FindIndex(a => DataSystem.FindSkill(a.SkillId).SkillType == SkillType.Active);
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
                    if (!isOnlyCheckEnemy)
                    {
                        _skillTriggerInfos.Insert(findIndex, skillTriggerInfo);
                    }
                    else
                    {
                        if (enemyDates.Count > 0)
                        {
                            _skillTriggerInfos.Insert(findIndex, skillTriggerInfo);
                        }
                    }
                }
            }
        }

        public void BeforeAutoSetSkill()
        {
            ChangeCost(CurrentParameter(StatusParamType.Cost));
            _equipmentSkillIds.Clear();
        }

        public void AutoSetSkill(List<SkillInfo> skillInfos, List<ActorInfo> actorInfos, Dictionary<int, int> enqmySkillWeights)
        {
            // 初期とLearning習得したものを設定
            InitSkillInfo();
            var remainCost = CurrentCost.Value;
            var enableSkillInfos = skillInfos.FindAll(a => a.Enable);
            var weightKey = enqmySkillWeights; //skillId, weight

            enableSkillInfos.Sort((a, b) => weightKey[a.Id.Value] - weightKey[b.Id.Value] >= 0 ? 1 : -1);
            // 得意属性順
            var attributeRanks = AttributeRanks(actorInfos);
            var sortAttributeRanks = attributeRanks.OrderBy(i => (int)i.Value).ToDictionary(i => i.Key, i => (int)i.Value);
            foreach (var attributeRank in sortAttributeRanks)
            {
                var findAllSkillInfos = enableSkillInfos.FindAll(a => a.Master.Attribute == (AttributeType)attributeRank.Key);
                foreach (var skillInfo in findAllSkillInfos)
                {
                    if (skillInfo.LearningCost.Value <= remainCost)
                    {
                        remainCost -= skillInfo.LearningCost.Value;
                        ChangeEquipSkill(skillInfo.Id.Value, 0);
                    }
                    if (remainCost <= 0)
                    {
                        break;
                    }
                }
            }
            RecommendActiveSkill();
            SortEquipmentSkillIds();
        }

        public void RecommendActiveSkill()
        {
            _skillTriggerInfos.Clear();
            // 初期設定に戻す
            InitSkillTriggerInfos();
            var skills = new List<SkillInfo>();
            foreach (var equipmentSkillId in _equipmentSkillIds)
            {
                var skillInfo = new SkillInfo(equipmentSkillId.Value);
                skills.Add(skillInfo);
            }
            var addActive = skills.FindAll(a => a.IsBattleActiveSkill());
            // 新たに追加したアクティブをアクティブの下に入れる
            InsertSkillTriggerSkills(addActive, false);
            var addPassive = skills.FindAll(a => a.IsBattlePassiveSkill());

            // その他のパッシブを加える
            InsertSkillTriggerSkills(addPassive, true);

            for (int i = 0; i < _skillTriggerInfos.Count; i++)
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

