using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class StatusModel : BaseModel
    {
        private StatusViewInfo _sceneParam;
        public StatusViewInfo SceneParam => _sceneParam;
        public ParameterInt SelectEquipmentIndex = new();
        private List<ActorInfo> _actorInfos = null;
        public List<ActorInfo> ActorInfos => _actorInfos;
        public ParameterInt CurrentIndex = new();
        public ActorInfo CurrentActor => _actorInfos[CurrentIndex.Value];
        public StatusModel()
        {
            _sceneParam = (StatusViewInfo)GameSystem.SceneStackManager.LastStatusViewInfo;
            _actorInfos = _sceneParam.ActorInfos;
        }
        private SkillInfo _selectSkillInfo = null;
        public SkillInfo SelectSkillInfo => _selectSkillInfo;
        public void SetSelectSkillInfo(SkillInfo skillInfo) => _selectSkillInfo = skillInfo;

        private AttributeType _filterAttribute = AttributeType.None;
        public void ChangeFilterAttribute(bool plus)
        {
            if (plus)
            {
                _filterAttribute += 1;
            }
            else
            {
                _filterAttribute -= 1;
            }
            if (_filterAttribute < 0)
            {
                _filterAttribute = AttributeType.Void;
            }
            if (_filterAttribute > AttributeType.Void)
            {
                _filterAttribute = AttributeType.None;
            }
        }

        public string FilterText()
        {
            var textId = 14121 + _filterAttribute;
            return DataSystem.GetText((int)textId);
        }

        public void ChangeEquipSkill(int changeSkillId)
        {
            CurrentActor.ChangeEquipSkill(changeSkillId, _selectSkillInfo.Id.Value);
        }

        public void RemoveEquipSkill(ActorInfo actorInfo, int removeSkillId)
        {
            actorInfo.ChangeEquipSkill(0, removeSkillId);
        }

        public void UpdateActorRemainCost()
        {
            var cost = 0;
            foreach (var slotSkill in EquipSkills())
            {
                cost += slotSkill.LearningCost.Value;
            }
            CurrentActor.ChangeCost(CurrentActor.MaxCost - cost);
        }

        public List<SkillInfo> Skills()
        {
            var list = new List<SkillInfo>();
            var learnedSkills = CurrentActor.LearningSkillInfos();
            // アクター習得済み
            foreach (var learnedSkill in learnedSkills)
            {
                if (learnedSkill.LearningState == LearningState.Learned)
                {
                    list.Add(learnedSkill);
                }
            }
            // 装備から習得済み
            foreach (var learnSkillId in CurrentActor.LearnSkillIds)
            {
                list.Add(new SkillInfo(learnSkillId));
            }
            // アクター未習得
            foreach (var learnedSkill in learnedSkills)
            {
                if (learnedSkill.LearningState != LearningState.Learned)
                {
                    list.Add(learnedSkill);
                }
            }
            var insertIndex = list.FindAll(a => a.Id.Value > 1000).Count;
            // カインドを追加
            foreach (var kind in CurrentActor.Master.Kinds)
            {
                if (kind > 0 && (int)kind < 10)
                {
                    var skillInfo = new SkillInfo((int)kind * 10 + 10100);
                    skillInfo.SetEnable(true);
                    list.Insert(insertIndex, skillInfo);
                    insertIndex++;
                }
            }
            return list;
        }

        public List<EquipmentInfo> ActorEquipmentInfos()
        {
            var list = new List<EquipmentInfo>();
            if (CurrentActor.EquipmentIds.Count > 0)
            {
                foreach (var equipmentId in CurrentActor.EquipmentIds)
                {
                    EquipmentInfo equipmentInfo = new();
                    equipmentInfo.EquipmentId.SetValue(equipmentId);
                    equipmentInfo.LearningInfos = new();
                    foreach (var learningDate in DataSystem.FindEquipment(equipmentInfo.EquipmentId.Value).LearningDates)
                    {
                        EquipmentLearningInfo equipmentLearningInfo = new();
                        equipmentLearningInfo.SkillId.SetValue(learningDate.SkillId);
                        equipmentLearningInfo.LearningRate.SetValue(learningDate.Rate);
                        equipmentLearningInfo.LearningExp.SetValue(CurrentActor.MastarySkillRate(learningDate.SkillId));
                        equipmentInfo.LearningInfos.Add(equipmentLearningInfo);
                    }
                    list.Add(equipmentInfo);
                }
            }
            return list;
        }

        public List<EquipmentInfo> EquipmentInfos()
        {
            var list = new List<EquipmentInfo>();
            EquipmentInfo removeEquipmentInfo = new();
            removeEquipmentInfo.EquipmentId.SetValue(10);
            list.Add(removeEquipmentInfo);
            foreach (var equipmentId in PartyInfo.EquipmentIds)
            {
                EquipmentInfo equipmentInfo = new();
                equipmentInfo.EquipmentId.SetValue(equipmentId);
                equipmentInfo.LearningInfos = new();
                foreach (var learningDate in DataSystem.FindEquipment(equipmentInfo.EquipmentId.Value).LearningDates)
                {
                    EquipmentLearningInfo equipmentLearningInfo = new();
                    equipmentLearningInfo.SkillId.SetValue(learningDate.SkillId);
                    equipmentLearningInfo.LearningRate.SetValue(learningDate.Rate);
                    equipmentLearningInfo.LearningExp.SetValue(CurrentActor.MastarySkillRate(learningDate.SkillId));
                    equipmentInfo.LearningInfos.Add(equipmentLearningInfo);
                }
                list.Add(equipmentInfo);
            }
            return list;
        }

        public void ChangeEquipment(EquipmentInfo equipmentInfo)
        {
            CurrentActor.ChangeEquipment(equipmentInfo.EquipmentId.Value, SelectEquipmentIndex.Value);
        }

        public List<SkillInfo> EquipSkills()
        {
            var list = new List<SkillInfo>();
            if (_sceneParam.AddActor.Value)
            {
                // 加入の時は未習得魔法を表示
                list.AddRange(CurrentActor.LearningSkillInfos());
            }
            else
            {
                list.AddRange(EquipSkills(CurrentActor));
            }

            var insertIndex = list.FindAll(a => a.Id.Value > 1000).Count;
            // カインドを追加
            foreach (var kind in CurrentActor.Master.Kinds)
            {
                if (kind > 0 && (int)kind < 10)
                {
                    var skillInfo = new SkillInfo((int)kind * 10 + 10100);
                    skillInfo.SetEnable(true);
                    list.Insert(insertIndex, skillInfo);
                    insertIndex++;
                }
            }
            return list;
        }

        public List<SkillInfo> ChangeAbleSkills()
        {
            // マイナスSP計算
            var cost = _selectSkillInfo != null ? CurrentActor.EquipSkillCost(_selectSkillInfo.Master.Id, PartyInfo.ActorInfos, null) : 0;
            var changeAbleSkills = ChangeAbleSkills(CurrentActor, cost);
            // はずすを挿入
            var removeSkill = new SkillInfo(1);
            removeSkill.SetEnable(true);
            changeAbleSkills.Insert(0, removeSkill);
            // フィルタ
            if (_filterAttribute != AttributeType.None)
            {
                changeAbleSkills = changeAbleSkills.FindAll(a => a.Master.Attribute == _filterAttribute || a.Master.Id == 1);
            }
            return changeAbleSkills;
        }

        public ActorInfo EquipmentSkill(SkillInfo skillInfo)
        {
            if (skillInfo.Master.SkillType != SkillType.Equip)
            {
                return null;
            }
            var find = PartyInfo.ActorInfos.Find(a => a.EquipmentSkillIds.Find(b => b.Value == skillInfo.Id.Value) != null);
            return find;
        }

        public List<ItemInfo> UseItemInfos()
        {
            var list = PartyInfo.UseItemInfos();
            list.AddRange(PartyInfo.DungeonUseItemInfos());
            return list;
        }

        public string HelpText()
        {
            return DataSystem.GetText(18010);
        }

        public void SelectActor(int actorId)
        {
            var index = _actorInfos.FindIndex(a => a.ActorId.Value == actorId);
            CurrentIndex.SetValue(index);
        }

        public void ChangeActorIndex(int value)
        {
            CurrentIndex.GainValue(value, 0, _actorInfos.Count - 1, true);
        }

        public void AutoSetSkill()
        {
            CurrentActor.BeforeAutoSetSkill();
            var changeAbleSkills = ChangeAbleSkills(CurrentActor, 0).FindAll(a => EquipmentSkill(a) == null);
            CurrentActor.AutoSetSkill(changeAbleSkills, PartyInfo.CurrentDeckActorInfos(), TempInfo.EnqmySkillWeights(changeAbleSkills));
        }

        public void SetActorLastSkillId(int selectSkillId)
        {
            CurrentActor.SetLastSelectSkillId(selectSkillId);
        }

        public List<SkillTriggerInfo> SkillTrigger(int selectIndex = -1)
        {
            return CurrentActor.SkillTriggerInfos;
        }

        public int LevelUpCost()
        {
            return ActorLevelUpCost(CurrentActor);
        }

        public int LevelUpBeforeExp()
        {
            return CurrentActor.BeforeExp;
        }

        public int LevelUpAfterExp()
        {
            return ActorLevelUpAfterExp(CurrentActor);
        }

        public void DecideActor()
        {
            var getItemInfo = MakeGetItemInfo(GetItemType.AddActor, CurrentActor.ActorId.Value);
            AddGetItemInfo(getItemInfo);
        }

        public bool CanUseItem(ItemInfo itemInfo)
        {
            if (itemInfo.Master.ItemType != ItemType.UseItem)
            {
                return false;
            }
            switch (itemInfo.Master.Param1)
            {
                case (int)UseItemType.Exp:
                    return CurrentActor.Level < CurrentActor.Master.MaxLv;
                case (int)UseItemType.AttributeUp:
                    var getAttibute = (AttributeType)itemInfo.Master.Param2;
                    return CurrentActor.AttributeRanks(PartyInfo.ActorInfos)[(int)getAttibute] != AttributeRank.S;
                case (int)UseItemType.StatusUp:
                    return true;
                case (int)UseItemType.ClassChange:
                    return !CurrentActor.IsClassChenged.Value;
            }
            return true;
        }

        public bool IsUseItemBatch()
        {
            if (_sceneParam.AddActor.Value)
            {
                return false;
            }
            var achievements = PartyInfo.AchievementInfos;
            return achievements.Find(a => !a.Achieved.Value && a.Master.ConditionType == AchievementConditionType.TacticsLvupCount) != null;
        }

        public bool IsChangeSkillBatch()
        {
            if (_sceneParam.AddActor.Value)
            {
                return false;
            }
            var achievements = PartyInfo.AchievementInfos;
            return achievements.Find(a => !a.Achieved.Value && a.Master.ConditionType == AchievementConditionType.StatusSkillChangeCount) != null;
        }
    }
}