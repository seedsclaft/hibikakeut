using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

namespace Ryneus
{
    public partial class BattleModel : BaseModel
    {
        public BattleModel()
        {
            _sceneParam = (BattleSceneInfo)GameSystem.SceneStackManager.LastSceneParam;
            InitializeCheckTrigger();
        }

        private BattleSceneInfo _sceneParam;
        public BattleSceneInfo SceneParam => _sceneParam;
        private int _actionIndex = 0;
        private int _turnCount = 1;
        public int TurnCount => _turnCount;
        public void SeekTurnCount() { _turnCount++; }

        //private List<SkillLogListInfo> _skillLogs = new ();
        //public List<SkillLogListInfo> SkillLogs => _skillLogs;

        //private SaveBattleInfo _saveBattleInfo = new SaveBattleInfo();

        private List<BattlerInfo> _battlers = new();
        public List<BattlerInfo> Battlers => _battlers;
        private List<BattlerInfo> _reserveBattlers = new();

        private UnitInfo _party = null;
        private UnitInfo _troop = null;
        public UnitInfo GetFriendUnit(BattlerInfo battlerInfo)
        {
            return battlerInfo.IsActor ? _party : _troop;
        }
        public UnitInfo GetOpponentUnit(BattlerInfo battlerInfo)
        {
            return battlerInfo.IsActor ? _troop : _party;
        }

        /// <summary>
        /// Indexから取得
        /// </summary>
        public List<BattlerInfo> GetBattlerInfoByIndex(bool friends, bool aliveOnly)
        {
            if (aliveOnly)
            {
                return _battlers.FindAll(a => a.IsActor == friends && a.IsAlive());
            }
            else
            {
                return _battlers.FindAll(a => a.IsActor == friends);
            }
        }

        private Dictionary<int, List<ActionInfo>> _turnActionInfos = new();

        public void AddTurnActionInfos(ActionInfo actionInfo, bool Interrupt)
        {
            if (!_turnActionInfos.ContainsKey(_turnCount))
            {
                _turnActionInfos[_turnCount] = new List<ActionInfo>();
            }
            if (Interrupt)
            {
                _turnActionInfos[_turnCount].Insert(0, actionInfo);
            }
            else
            {
                _turnActionInfos[_turnCount].Add(actionInfo);
            }
        }

        private bool UsedTurnSameActionInfo(SkillInfo skillInfo, int subjectIndex)
        {
            return _turnActionInfos.ContainsKey(_turnCount) && _turnActionInfos[_turnCount].Find(a => a.Master.Id == skillInfo.Id.Value && a.SubjectIndex.Value == subjectIndex) != null;
        }

        private int UsedSameTurnActionInfo(SkillInfo skillInfo)
        {
            if (_turnActionInfos.ContainsKey(_turnCount))
            {
                var sameSkills = _turnActionInfos[_turnCount].FindAll(a => a.Master.Id == skillInfo.Id.Value);
                if (sameSkills != null)
                {
                    return sameSkills.Count;
                }
            }
            return 0;
        }

        public void CreateBattleRecords()
        {
            foreach (var battlerInfo in _battlers)
            {
                _battleRecords[battlerInfo.Index.Value] = new BattleRecord(battlerInfo.Index.Value);
            }
        }

        public List<BattlerInfo> FieldBattlerInfos()
        {
            return _battlers.FindAll(a => !a.isAlcana);
        }

        public List<BattlerInfo> GetFriendsAliveBattlerInfos(BattlerInfo battlerInfo)
        {
            return battlerInfo.IsActor ? _party.AliveBattlerInfos : _troop.AliveBattlerInfos;
        }

        public List<BattlerInfo> GetOpponentsAliveBattlerInfos(BattlerInfo battlerInfo)
        {
            return battlerInfo.IsActor ? _troop.AliveBattlerInfos : _party.AliveBattlerInfos;
        }

        public List<StateInfo> UpdateAp()
        {
            var removeStateList = new List<StateInfo>();
            foreach (var battler in FieldBattlerInfos())
            {
                if (battler.IsAlive())
                {
                    battler.UpdateAp();
                    var removeStates = battler.UpdateState(RemovalTiming.UpdateAp);
                    if (removeStates.Count > 0)
                    {
                        removeStateList.AddRange(removeStates);
                    }
                }
            }
            CheckApCurrentBattler();
            return removeStateList;
        }

        public void UpdateApModify(BattlerInfo battlerInfo)
        {
            var minusAp = 0f;
            if (battlerInfo.Ap.Value < 0)
            {
                minusAp = battlerInfo.Ap.Value;
            }
            foreach (var battler in FieldBattlerInfos())
            {
                if (battler.IsAlive())
                {
                    battler.ChangeAp(minusAp * -1f);
                }
            }
        }

        public void WaitCommand(ActionInfo actionInfo)
        {
            actionInfo.ActionResults.Clear();
        }

        public void AssignWaitBattler()
        {
        }

        public void RemoveOneMemberWaitBattlers()
        {
            var partyWaitBattlers = _party.AliveBattlerInfos.FindAll(a => a.CanMove());
            if (partyWaitBattlers.Count < 1)
            {
                foreach (var battlerInfo in _party.AliveBattlerInfos)
                {
                    battlerInfo.EraseStateInfo(StateType.Wait);
                }
            }
            var troopWaitBattlers = _troop.AliveBattlerInfos.FindAll(a => a.CanMove());
            if (troopWaitBattlers.Count < 1)
            {
                foreach (var battlerInfo in _troop.AliveBattlerInfos)
                {
                    battlerInfo.EraseStateInfo(StateType.Wait);
                }
            }
        }

        public List<BattlerInfo> ViewBattlerActors()
        {
            var list = new List<BattlerInfo>();
            for (int i = 1; i <= 6; i++)
            {
                var find = _battlers.Find(a => a.Index.Value == i);
                if (find != null)
                {
                    list.Add(find);
                }
                else
                {
                    var newUnitInfo = new BattlerInfo();
                    list.Add(newUnitInfo);
                }
            }
            return list;
        }

        public List<BattlerInfo> ViewBattlerEnemies()
        {
            return _troop.BattlerInfos;
        }

        public List<BattlerInfo> UnitBattlerActors()
        {
            return _battlers.FindAll(a => !a.isAlcana && a.IsActor);
        }

        public List<BattlerInfo> BattlerActors()
        {
            return FieldBattlerInfos().FindAll(a => a.IsActor);
        }

        public List<BattlerInfo> BattlerEnemies()
        {
            return FieldBattlerInfos().FindAll(a => !a.IsActor);
        }

        public BattlerInfo GetBattlerInfo(int index)
        {
            return _battlers.Find(a => a.Index.Value == index);
        }

        public List<SkillInfo> SkillActionList(BattlerInfo battlerInfo)
        {
            var skillInfos = battlerInfo.Skills.FindAll(a => a.Master.Id > 100);
            foreach (var skillInfo in skillInfos)
            {
                skillInfo.SetEnable(CheckCanUse(skillInfo, battlerInfo));
            }
            var insert = skillInfos.FindIndex(a => a.Master.SkillType == SkillType.Passive || a.Master.SkillType == SkillType.Equipment || a.Master.SkillType == SkillType.Kind);
            if (insert == -1)
            {
                insert = skillInfos.Count;
            }
            var changeSkill = new SkillInfo(6020);
            // 交代できる対象がいるか
            changeSkill.SetEnable(_battlers.Find(a => a.IsAlive() && a.Index.Value != battlerInfo.Index.Value) != null);
            skillInfos.Insert(insert, changeSkill);
            var noCommandSkill = new SkillInfo(6010);
            noCommandSkill.SetEnable(true);
            skillInfos.Insert(insert + 1, noCommandSkill);
            return skillInfos;
        }

        private bool CheckCanUse(SkillInfo skillInfo, BattlerInfo battlerInfo)
        {
            if (skillInfo.CountTurn.Value > 0)
            {
                return false;
            }
            /*
            if (CalcMpCost(battlerInfo, skillInfo.Master.MpCost) > battlerInfo.Mp.Value)
            {
                return false;
            }
            */
            // period中〇回以下使用
            var inPeriodUseCountUnder = skillInfo.Master.TriggerDates.Find(a => a.TriggerType == TriggerType.InPeriodUseCountUnder);
            if (inPeriodUseCountUnder != null)
            {
                if (inPeriodUseCountUnder.Param1 <= skillInfo.PeriodUseCount.Value)
                {
                    return false;
                }
            }
            if (skillInfo.Master.SkillType == SkillType.Passive)
            {
                return false;
            }
            if (skillInfo.Master.SkillType == SkillType.Artifact)
            {
                return false;
            }
            if (skillInfo.Master.SkillType == SkillType.Unique)
            {
                return false;
            }
            if (skillInfo.Master.SkillType == SkillType.Equipment)
            {
                return false;
            }
            if (battlerInfo.IsState(StateType.Silence) && skillInfo.Master.CountTurn != 0)
            {
                return false;
            }
            if (skillInfo.Master.SkillType == SkillType.Awaken)
            {
                if (!battlerInfo.IsState(StateType.Demigod))
                {
                    return false;
                }
            }
            if (skillInfo.IsUnison())
            {
                return FieldBattlerInfos().FindAll(a => a.IsAlive() && a.IsActor == battlerInfo.IsActor && a.CanMove()).Count > 1;
            }
            if (CanUseTrigger(skillInfo, battlerInfo) == false)
            {
                return false;
            }
            var targetIndexList = GetSkillTargetIndexList(skillInfo.Master.Id, battlerInfo.Index.Value, true);
            if (targetIndexList.Count == 0)
            {
                return false;
            }
            return true;
        }

        private bool CheckCanUsePassive(SkillInfo skillInfo, BattlerInfo battlerInfo)
        {
            if (skillInfo.CountTurn.Value > 0)
            {
                return false;
            }
            if (skillInfo.Master.SkillType == SkillType.Passive)
            {
                //return false;
            }
            if (skillInfo.Master.SkillType == SkillType.Unique)
            {
                //return false;
            }
            if (battlerInfo.IsState(StateType.Silence) && skillInfo.Master.CountTurn != 0)
            {
                return false;
            }
            if (battlerInfo.IsState(StateType.NoPassive))
            {
                return false;
            }
            if (!battlerInfo.CanMove())
            {
                return false;
            }
            if (skillInfo.Master.SkillType == SkillType.Awaken)
            {
                if (!battlerInfo.IsState(StateType.Demigod))
                {
                    return false;
                }
            }
            if (skillInfo.IsUnison())
            {
                return FieldBattlerInfos().FindAll(a => a.IsAlive() && a.IsActor == battlerInfo.IsActor && a.CanMove()).Count > 1;
            }
            if (CanUseTrigger(skillInfo, battlerInfo) == false)
            {
                return false;
            }
            return true;
        }

        // selectIndexを対象にした時の効果範囲を取得
        public List<int> ActionInfoTargetIndexes(ActionInfo actionInfo, int selectIndex, int counterSubjectIndex = -1, ActionInfo baseActionInfo = null, List<ActionResultInfo> baseActionResultInfos = null)
        {
            if (actionInfo == null)
            {
                return new List<int>();
            }
            var subject = GetBattlerInfo(actionInfo.SubjectIndex.Value);
            var targetIndexList = GetSkillTargetIndexList(actionInfo.Master.Id, subject.Index.Value, true, counterSubjectIndex, baseActionInfo, baseActionResultInfos);
            var scopeType = actionInfo.ScopeType;
            if (subject.IsState(StateType.EffectLine) && scopeType != ScopeType.All)
            {
                scopeType = ScopeType.Line;
            }
            if (subject.IsState(StateType.EffectAll))
            {
                scopeType = ScopeType.All;
            }
            // 挑発でselectIndexを変化
            if (subject != null && subject.IsState(StateType.Substitute))
            {
                // 対象と挑発した対象が同じパーティなら有効
                var substituteState = subject.GetStateInfo(StateType.Substitute);
                var substituteTarget = GetBattlerInfo(substituteState.BattlerId.Value);
                if (substituteTarget.IsAlive() && targetIndexList.FindIndex(a => GetBattlerInfo(a).IsActor == substituteTarget.IsActor) > -1)
                {
                    selectIndex = substituteState.BattlerId.Value;
                }
            }
            var targetBattler = GetBattlerInfo(selectIndex);
            switch (scopeType)
            {
                case ScopeType.All:
                case ScopeType.WithoutSelfAll:
                    break;
                case ScopeType.Line:
                case ScopeType.FrontLine:
                case ScopeType.WithoutSelfLine:
                    targetIndexList = targetIndexList.FindAll(a => GetBattlerInfo(a).LineIndex == targetBattler.LineIndex);
                    break;
                case ScopeType.One:
                case ScopeType.Self:
                    targetIndexList.Clear();
                    targetIndexList.Add(selectIndex);
                    break;
                case ScopeType.WithoutSelfOne:
                    targetIndexList.Clear();
                    if (subject.Index.Value != selectIndex)
                    {
                        targetIndexList.Add(selectIndex);
                    }
                    break;
                case ScopeType.OneAndNeighbor:
                case ScopeType.Neighbor:
                    targetIndexList.Clear();
                    targetIndexList.Add(selectIndex);
                    // 両隣を追加
                    var targetUnit = subject.IsActor ? _party.BattlerInfos : _troop.BattlerInfos;
                    if (actionInfo.TargetType == TargetType.Opponent)
                    {
                        targetUnit = subject.IsActor ? _troop.BattlerInfos : _party.BattlerInfos;
                    }
                    var before = targetUnit.FindAll(a => a.Index.Value < selectIndex);
                    if (before.Count > 0)
                    {
                        targetIndexList.Add(before[before.Count - 1].Index.Value);
                    }
                    var after = targetUnit.FindAll(a => a.Index.Value > selectIndex);
                    if (after.Count > 0)
                    {
                        targetIndexList.Add(after[0].Index.Value);
                    }
                    if (scopeType == ScopeType.Neighbor)
                    {
                        targetIndexList.Remove(subject.Index.Value);
                    }
                    break;
            }
            // 挑発
            /*
            if (subject != null && subject.IsState(StateType.Substitute))
            {
                // 対象と挑発した対象が同じパーティなら有効
                var substituteState = subject.GetStateInfo(StateType.Substitute);
                if (targetIndexList.FindIndex(a => GetBattlerInfo(a).IsActor == GetBattlerInfo(substituteState.BattlerId).IsActor) > -1)
                {
                    targetIndexList.Clear();
                    int substituteId = subject.GetStateInfo(StateType.Substitute).BattlerId;
                    if (targetIndexList.Contains(substituteId))
                    {
                        targetIndexList.Add(substituteId);
                    } else
                    {
                        var tempIndexList = GetSkillTargetIndexList(actionInfo.Master.Id,actionInfo.SubjectIndex,false);
                        if (tempIndexList.Contains(substituteId))
                        {
                            targetIndexList.Add(substituteId);
                        }
                    }
                }
            }
            */
            return targetIndexList;
        }

        public bool CanUseCondition(int skillId, BattlerInfo subject, int targetIndex, ActionInfo actionInfo = null)
        {
            bool isEnable = false;
            var skill = DataSystem.FindSkill(skillId);
            var target = GetBattlerInfo(targetIndex);
            foreach (var featureData in skill.FeatureDates)
            {
                switch (featureData.FeatureType)
                {
                    case FeatureType.HpDamage:
                        if (target.Hp.Value > 0)
                        {
                            isEnable = true;
                        }
                        break;
                    case FeatureType.HpConsumeDamage:
                        if (target.Hp.Value > 0)
                        {
                            var needHp = 0;
                            // 割合
                            if (featureData.Param2 == 1)
                            {
                                needHp = (int)(subject.MaxHp * featureData.Param3 * 0.01f);
                            }
                            else
                            {
                                needHp = featureData.Param3;
                            }
                            isEnable = subject.Hp.Value > needHp;
                        }
                        break;
                    case FeatureType.HpHeal:
                    case FeatureType.KindHeal:
                        if (subject != null && subject.IsActor)
                        {
                            {
                                if (!target.IsActor)
                                {
                                    isEnable = target.Kinds.Contains(KindType.Undead);
                                }
                                else
                                {
                                    isEnable = true;
                                }
                            }
                        }
                        else
                        {
                            if (target.Hp.Value < target.MaxHp)
                            {
                                if (!target.IsActor && !target.Kinds.Contains(KindType.Undead))
                                {
                                    isEnable = true;
                                }
                            }
                        }
                        break;
                    case FeatureType.CtHeal:
                        if (subject != null)
                        {
                            if (target.Skills.Find(a => a.CountTurn.Value > 0) != null)
                            {
                                isEnable = true;
                            }
                            else
                            {
                                if (actionInfo != null && actionInfo.Master.CountTurn > 0)
                                {
                                    // IsEnable = true;
                                }
                            }
                        }
                        break;
                    case FeatureType.CtDamage:
                        if (target.Skills.Find(a => a.CountTurn.Value < a.Master.CountTurn) != null)
                        {
                            isEnable = true;
                        }
                        break;
                    case FeatureType.AddState:
                    case FeatureType.AddStateNextTurn:
                        if ((StateType)featureData.Param1 == StateType.RemoveBuff)
                        {
                            // 消すバフがあれば有効
                            if (target.GetRemovalBuffStates().Count > 0)
                            {
                                isEnable = true;
                            }
                        }
                        else
                            if ((StateType)featureData.Param1 == StateType.Linkage)
                            {
                                // 後列がいれば有効
                                var linkage = _reserveBattlers.Find(a => a.Index.Value == subject.Index.Value + 3);
                                isEnable = linkage != null && linkage.IsAlive();
                            }
                            else
                            {
                                if (target != null)
                                {
                                    var targetStateInfos = target.StateInfos;
                                    var sameStateInfos = targetStateInfos.FindAll(a => a.Master.StateType == (StateType)featureData.Param1 && a.SkillId.Value == skillId);
                                    // 既にかかっているか
                                    if (sameStateInfos.Count > 0)
                                    {
                                        // 重複できるか
                                        var overLapCount = sameStateInfos[0].Master.OverLap;
                                        if (overLapCount > sameStateInfos.Count)
                                        {
                                            isEnable = true;
                                        }
                                    }
                                    else
                                    {
                                        isEnable = true;
                                    }
                                }
                                /*
                                if (!target.IsState((StateType)featureData.Param1) && !target.IsState(StateType.Barrier))
                                {
                                    IsEnable = true;
                                } else
                                if (subject != null && subject.IsActor || (StateType)featureData.Param1 == StateType.DamageUp)
                                {
                                    IsEnable = true;
                                } else
                                if (subject != null && !subject.IsActor && !target.IsState((StateType)featureData.Param1) && target.IsState(StateType.Barrier))
                                {
                                    if (UnityEngine.Random.Range(0,100) > 50)
                                    {
                                        IsEnable = true;
                                    }
                                }
                                */
                            }
                        break;
                    case FeatureType.RemoveState:
                        if (target.IsState((StateType)featureData.Param1))
                        {
                            isEnable = true;
                        }
                        break;
                    case FeatureType.RemoveAbnormalState:
                        if (target.StateInfos.Find(a => a.Master.Abnormal) != null)
                        {
                            isEnable = true;
                        }
                        break;
                    case FeatureType.BreakUndead:
                        if (subject.IsActor)
                        {
                            isEnable = true;
                        }
                        else
                        {
                            if (!target.IsActor && !target.Kinds.Contains(KindType.Undead))
                            {
                                isEnable = true;
                            }
                        }
                        break;
                    case FeatureType.NoResetAp:
                        break;
                    default:
                        isEnable = true;
                        break;
                }
            }
            return isEnable;
        }

        public List<int> CheckScopeTriggers(List<int> targetIndexList, List<SkillData.TriggerData> scopeTriggers, ActionInfo actionInfo, List<ActionResultInfo> actionResultInfos)
        {
            for (int i = targetIndexList.Count - 1; i >= 0; i--)
            {
                var target = GetBattlerInfo(targetIndexList[i]);
                var remove = false;
                foreach (var scopeTrigger in scopeTriggers)
                {
                    if (scopeTrigger.TriggerType == TriggerType.DemigodMagicAttribute)
                    {
                        if (target.Skills.Find(a => a.Master.SkillType == SkillType.Unique && a.Attribute == (AttributeType)scopeTrigger.Param1) == null)
                        {
                            remove = true;
                        }
                    }
                    else
                    {
                        if (!IsTriggeredSkillInfo(target, scopeTriggers, actionInfo, actionResultInfos))
                        {
                            remove = true;
                        }
                        /*
                        if (!scopeTrigger.IsTriggeredSkillInfo(target,BattlerActors(),BattlerEnemies()))
                        {
                            remove = true;
                        }
                        */
                    }
                }
                if (remove)
                {
                    targetIndexList.RemoveAt(i);
                }
            }
            return targetIndexList;
        }


        private void SetActorLastTarget(ActionInfo actionInfo, BattlerInfo subject, List<int> indexList)
        {
            if (indexList.Count == 0)
            {
                return;
            }
            if (subject.IsActor)
            {
                if (actionInfo.Master.TargetType == TargetType.Opponent)
                {
                    subject.SetLastTargetIndex(indexList[0]);
                }
            }
            if (actionInfo.Master.TargetType == TargetType.All)
            {
                if (indexList[0] > 100)
                {
                    subject.SetLastTargetIndex(indexList[0]);
                }
            }
        }


        // indexListにActionを使ったときのリザルトを生成
        public void MakeActionResultInfo(ActionInfo actionInfo, List<int> indexList, bool checkRepeatTimeZero = true, bool needCheckCover = false)
        {
            if (checkRepeatTimeZero && actionInfo.RepeatTime.Value == 0)
            {
                return;
            }
            actionInfo.SetCandidateTargetIndexList(indexList);
            //actionInfo.SeekRepeatTime();
            var subject = GetBattlerInfo(actionInfo.SubjectIndex.Value);
            // ターゲットの生死判定
            var aliveType = actionInfo.Master.AliveType;
            if (actionInfo != null)
            {
                indexList = CalcAliveTypeIndexList(indexList, aliveType);
            }
            SetActorLastTarget(actionInfo, subject, indexList);
            if (subject.IsState(StateType.Silence))
            {
                return;
            }
            if (subject.IsState(StateType.NoPassive) && actionInfo.Master.SkillType == SkillType.Passive)
            {
                return;
            }

            // かばうによるターゲット変更
            if (!actionInfo.TriggeredSkill)
            {
                indexList = CheckCoverIndexList(actionInfo, indexList);
                actionInfo.SetCandidateTargetIndexList(indexList);
            }

            var actionResultInfos = new List<ActionResultInfo>();

            foreach (var targetIndex in indexList)
            {
                var target = GetBattlerInfo(targetIndex);
                var featureDates = new List<SkillData.FeatureData>();
                foreach (var featureData in actionInfo.SkillInfo.FeatureDates)
                {
                    if (featureData.EnhanceFeature())
                    {
                        continue;
                    }
                    featureDates.Add(featureData);
                }

                var actionResultInfo = new ActionResultInfo(subject, target, featureDates, actionInfo.Master.Id, actionInfo.ScopeType == ScopeType.One);
                // Hpダメージ分の回復計算
                var DamageHealPartyResultInfos = CalcDamageHealParty(subject, featureDates, actionResultInfo.HpDamage.Value);
                actionResultInfos.AddRange(DamageHealPartyResultInfos);
                // 攻撃成功回数分の回復計算
                var AttackCountHealPartyResultInfos = CalcAttackCountHealParty(subject, featureDates, actionInfo.RepeatTime.Value);
                actionResultInfos.AddRange(AttackCountHealPartyResultInfos);
                var DamageMpHealPartyResultInfos = CalcDamageCtHealParty(subject, featureDates, actionResultInfo.HpDamage.Value);
                actionResultInfos.AddRange(DamageMpHealPartyResultInfos);
                // 倒した敵のHpに応じて回復計算
                var HpHealTargetMaxHpResultInfos = CalcHpHealTargetMaxHp(subject, featureDates, actionResultInfo.HpDamage.Value);
                actionResultInfos.AddRange(HpHealTargetMaxHpResultInfos);
                if (actionResultInfo.RemoveAttackStateDamage())
                {
                    // 攻撃を受けた時に外れるステートを管理
                }
                actionResultInfos.Add(actionResultInfo);
            }
            AdjustActionResultInfo(actionResultInfos);
            actionInfo.SetActionResult(actionResultInfos);
        }

        private List<int> CheckCoverIndexList(ActionInfo actionInfo, List<int> indexList)
        {
            var newIndexList = new List<int>();
            var coverBattlerIds = new List<int>();
            var subject = GetBattlerInfo(actionInfo.SubjectIndex.Value);
            var triggerTimings = new List<TriggerTiming>() { TriggerTiming.PrimaryInterrupt };
            foreach (var targetIndex in indexList)
            {
                var target = GetBattlerInfo(targetIndex);
                var friends = GetFriendUnit(target);
                BattlerInfo coverableBattlerInfo = null;
                var coverableBattlerInfos = friends.CoverableBattlerInfo(target);
                foreach (var battlerInfo in coverableBattlerInfos)
                {
                    if (battlerInfo.IsState(StateType.NoPassive))
                    {
                        continue;
                    }
                    foreach (var passiveInfo in battlerInfo.PassiveSkills())
                    {
                        if (!CheckCanPassiveSkill(battlerInfo, passiveInfo, triggerTimings))
                        {
                            continue;
                        }
                        var triggerDates = passiveInfo.Master.TriggerDates.FindAll(a => triggerTimings.Contains(a.TriggerTiming));

                        if (!IsTriggeredSkillInfo(battlerInfo, triggerDates, actionInfo, null, target.Index.Value))
                        {
                            continue;
                        }
                        //bool usable = CanUsePassiveCount(battlerInfo,passiveInfo.Id,triggerDates);
                        // 元の条件が成立
                        // 作戦で可否判定
                        var selectSkill = -1;
                        var selectTarget = -1;
                        var skillTriggerInfos = battlerInfo.SkillTriggerInfos;
                        var skillTriggerInfo = skillTriggerInfos.Find(a => a.SkillId == passiveInfo.Id.Value);
                        if (skillTriggerInfo == null)
                        {
                            skillTriggerInfo = new SkillTriggerInfo(battlerInfo.Index.Value, passiveInfo);
                            skillTriggerInfo.UpdateTriggerDates(new List<SkillTriggerData>());
                        }
                        (selectSkill, selectTarget) = SelectSkillTargetBySkillTriggerDates(battlerInfo, new List<SkillTriggerInfo>() { skillTriggerInfo }, actionInfo, null);
                        if (selectSkill != passiveInfo.Id.Value)
                        {
                            continue;
                        }
                        if (selectTarget == -1)
                        {
                            continue;
                        }
                        var IsInterrupt = true;
                        var result = MakePassiveSkillActionResults(battlerInfo, passiveInfo, IsInterrupt, selectTarget, actionInfo, null, triggerDates[0]);
                        coverableBattlerInfo = battlerInfo;
                        if (result != null && result.ActionResults.Count > 0)
                        {
                            //checkedSkillIds.Add(passiveInfo.Id.Value);
                            // 継続パッシブは保存
                            var addPassive = passiveInfo.FeatureDates.Find(a => a.FeatureType == FeatureType.AddState);
                            if (addPassive != null && addPassive.Param2 == 999 && passiveInfo.Master.SkillType == SkillType.Passive)
                            {
                                AddPassiveSkillId(battlerInfo, addPassive.Param1, passiveInfo.Id.Value);
                            }
                        }
                    }
                }
                if (coverableBattlerInfo != null && !coverBattlerIds.Contains(coverableBattlerInfo.Index.Value) && coverableBattlerInfo.IsActor != subject.IsActor && coverableBattlerInfo.Index.Value != targetIndex)
                {
                    // かばう成立
                    coverBattlerIds.Add(coverableBattlerInfo.Index.Value);
                    if (!newIndexList.Contains(coverableBattlerInfo.Index.Value))
                    {
                        newIndexList.Add(coverableBattlerInfo.Index.Value);
                    }
                }
                else
                {
                    if (!newIndexList.Contains(targetIndex))
                    {
                        newIndexList.Add(targetIndex);
                    }
                }
            }
            return newIndexList;
        }

        private List<ActionResultInfo> CalcDamageHealParty(BattlerInfo subject, List<SkillData.FeatureData> featureDates, int hpDamage)
        {
            var actionResultInfos = new List<ActionResultInfo>();
            var damageHealParty = featureDates.Find(a => a.FeatureType == FeatureType.DamageHpHealParty);
            if (damageHealParty != null)
            {
                var friends = GetFriendsAliveBattlerInfos(subject);
                var hpHeal = hpDamage * damageHealParty.Param3 * 0.01f;
                foreach (var friend in friends)
                {
                    var featureData = new SkillData.FeatureData
                    {
                        FeatureType = FeatureType.HpHeal,
                        Param1 = (int)hpHeal
                    };
                    var actionResultInfo = new ActionResultInfo(subject, GetBattlerInfo(friend.Index.Value), new List<SkillData.FeatureData>() { featureData }, -1);
                    actionResultInfo.NoAnimation.SetValue(true);
                    actionResultInfos.Add(actionResultInfo);
                }
            }
            return actionResultInfos;
        }

        private List<ActionResultInfo> CalcAttackCountHealParty(BattlerInfo subject, List<SkillData.FeatureData> featureDates, int attackCount)
        {
            var actionResultInfos = new List<ActionResultInfo>();
            var attackCountHealParty = featureDates.Find(a => a.FeatureType == FeatureType.AttackHpHealParty);
            if (attackCountHealParty != null)
            {
                var friends = GetFriendsAliveBattlerInfos(subject);
                var hpHeal = attackCount * attackCountHealParty.Param3;
                foreach (var friend in friends)
                {
                    var featureData = new SkillData.FeatureData
                    {
                        FeatureType = FeatureType.HpHeal,
                        Param1 = hpHeal
                    };
                    var actionResultInfo = new ActionResultInfo(subject, GetBattlerInfo(friend.Index.Value), new List<SkillData.FeatureData>() { featureData }, -1);
                    actionResultInfos.Add(actionResultInfo);
                }
            }
            return actionResultInfos;
        }

        private List<ActionResultInfo> CalcHpHealTargetMaxHp(BattlerInfo subject, List<SkillData.FeatureData> featureDates, int hpDamage)
        {
            var actionResultInfos = new List<ActionResultInfo>();
            var hpHealTargetMaxHp = featureDates.Find(a => a.FeatureType == FeatureType.HpHealTargetMaxHp);
            if (hpHealTargetMaxHp != null)
            {
                var target = GetBattlerInfo(subject.LastTargetIndex());
                if (target != null && !target.IsAlive())
                {
                    var hpHeal = target.MaxHp * hpHealTargetMaxHp.Param1 * 0.01f;
                    var featureData = new SkillData.FeatureData
                    {
                        FeatureType = FeatureType.HpHeal,
                        Param1 = (int)hpHeal
                    };
                    var actionResultInfo = new ActionResultInfo(subject, subject, new List<SkillData.FeatureData>() { featureData }, -1);
                    actionResultInfos.Add(actionResultInfo);
                }
            }
            return actionResultInfos;
        }

        private List<ActionResultInfo> CalcDamageCtHealParty(BattlerInfo subject, List<SkillData.FeatureData> featureDates, int hpDamage)
        {
            var actionResultInfos = new List<ActionResultInfo>();
            var damageHealParty = featureDates.Find(a => a.FeatureType == FeatureType.DamageMpHealParty);
            if (damageHealParty != null)
            {
                var friends = GetFriendsAliveBattlerInfos(subject);
                var ctHeal = hpDamage * damageHealParty.Param3 * 0.01f;
                foreach (var friend in friends)
                {
                    var featureData = new SkillData.FeatureData
                    {
                        FeatureType = FeatureType.CtHeal,
                        Param1 = (int)ctHeal
                    };
                    var actionResultInfo = new ActionResultInfo(subject, GetBattlerInfo(friend.Index.Value), new List<SkillData.FeatureData>() { featureData }, -1);
                    actionResultInfos.Add(actionResultInfo);
                }
            }
            return actionResultInfos;
        }

        private int CalcHpCost(ActionInfo actionInfo)
        {
            int hpCost = 0;
            var featureDates = actionInfo.Master.FeatureDates.FindAll(a => a.FeatureType == FeatureType.HpConsumeDamage);
            foreach (var featureData in featureDates)
            {
                // 割合
                if (featureData.Param2 == 1)
                {
                    hpCost += (int)(GetBattlerInfo(actionInfo.SubjectIndex.Value).MaxHp * featureData.Param3 * 0.01f);
                }
                else
                {
                    hpCost += featureData.Param3;
                }
            }
            return hpCost;
        }

        private int CalcMpCost(BattlerInfo battlerInfo, int mpCost)
        {
            return battlerInfo.CalcMpCost(mpCost);
        }

        private int CalcRepeatTime(BattlerInfo subject, ActionInfo actionInfo)
        {
            var repeatTime = actionInfo.Master.RepeatTime;
            // パッシブで回数増加を計算
            var addFeatures = subject.EnhanceSkills.FindAll(a => a.Master.FeatureDates.Find(b => b.FeatureType == FeatureType.ChangeSkillRepeatTime && actionInfo.Master.Id == b.Param1) != null);
            foreach (var addFeature in addFeatures)
            {
                foreach (var featureData in addFeature.FeatureDates)
                {
                    repeatTime = featureData.Param3;
                }
            }
            return repeatTime;
        }

        private ScopeType CalcScopeType(BattlerInfo subject, ActionInfo actionInfo)
        {
            var scopeType = actionInfo.Master.Scope;
            // パッシブで対象変更を計算
            var changeScopeFeature = subject.EnhanceSkills.Find(a => a.Master.FeatureDates.Find(b => b.FeatureType == FeatureType.ChangeSkillScope && actionInfo.Master.Id == b.Param1) != null);
            if (changeScopeFeature != null)
            {
                scopeType = (ScopeType)changeScopeFeature.FeatureDates[0].Param3;
            }
            return scopeType;
        }

        public Effekseer.EffekseerEffectAsset AwakenEffect(int actorId)
        {
            var result = ResourceSystem.LoadResourceEffect("NA_Effekseer/NA_cut-in_" + DataSystem.FindActor(actorId).ImagePath);
            if (result != null)
            {
                return result;
            }
            return null;
        }

        public void ExecCurrentAction(ActionInfo actionInfo, bool addSaveData)
        {
            if (actionInfo == null)
            {
                return;
            }
            var subject = GetBattlerInfo(actionInfo.SubjectIndex.Value);
            // 支払いは最後の1回
            if (actionInfo.RepeatTime.Value == 1)
            {
                // Hpの支払い
                subject.GainHp(actionInfo.HpCost.Value * -1);
                // Mpの支払い
                subject.GainMp(actionInfo.MpCost.Value * -1);
                //subject.GainPayBattleMp(actionInfo.MpCost);
                subject.InitCountTurn(actionInfo.SkillInfo.Id.Value);
                if (actionInfo.Master.IsBattleActiveSkill())
                {
                    subject.LastSelectSkill.SetValue(actionInfo.SkillInfo.Id.Value);
                }
                subject.GainUseCount(actionInfo.SkillInfo.Id.Value);
                if (actionInfo.SkillInfo.Master.SkillType == SkillType.Awaken)
                {
                    PartyInfo.PartyStatInfo.UseAwakeSkillCount.GainValue(1);
                }
                if (actionInfo.SkillInfo.Master.FeatureDates.Find(a => a.FeatureType == FeatureType.ActionAfterChange) != null)
                {
                    PartyInfo.PartyStatInfo.UseChangeLineCount.GainValue(1);
                }
                _battleRecords[subject.Index.Value].GainUseSkillCount(actionInfo.SkillInfo.Id.Value, 1);
            }
            if (actionInfo.Master.IsHpHealFeature())
            {
                subject.Examine.HealCount.GainValue(1);
            }
            /*
            if (addSaveData)
            {
                _saveBattleInfo.AddActionData(actionInfo);
            }
            */
            ExecActionResultInfos(actionInfo.ActionResults, false);
            actionInfo.AddActionedRepeatTimes(actionInfo.RepeatTime.Value);
            if (actionInfo.Master.IsRevengeHpDamageFeature())
            {
                // 受けたダメージをリセット
                subject.Examine.DamagedValue.SetValue(0);
            }
            // 行動回数を減らす
            actionInfo.SeekRepeatTime();
        }

        // 複数のActionResultのreDamageとreHealを1つにまとめる
        public void AdjustActionResultInfo(List<ActionResultInfo> actionResultInfos)
        {
            // ドレイン回復をまとめる
            var reHealResults = actionResultInfos.FindAll(a => a.ReHeal.Value > 0);
            if (reHealResults.Count > 1)
            {
                int reHeal = 0;
                foreach (var reHealResult in reHealResults)
                {
                    reHeal += reHealResult.ReHeal.Value;
                    reHealResult.ReHeal.SetValue(0);
                }
                reHealResults[^1].ReHeal.SetValue(reHeal);
            }
            // カウンターダメージをまとめる
            var counterResults = actionResultInfos.FindAll(a => a.ReDamage.Value > 0);
            if (counterResults.Count > 1)
            {
                int reDamage = 0;
                foreach (var counterResult in counterResults)
                {
                    reDamage += counterResult.ReDamage.Value;
                    counterResult.ReDamage.SetValue(0);
                }
                counterResults[^1].ReDamage.SetValue(reDamage);
            }
            if (reHealResults.Count > 0 && counterResults.Count > 0)
            {
                int heal = reHealResults[^1].ReHeal.Value;
                int damage = counterResults[^1].ReDamage.Value;
                if (heal > damage)
                {
                    reHealResults[^1].ReHeal.SetValue(heal - damage);
                    counterResults[^1].ReDamage.SetValue(0);
                }
                else
                {
                    reHealResults[^1].ReHeal.SetValue(0);
                    counterResults[^1].ReDamage.SetValue(damage - heal);
                }
            }
            // ReDamageによってDeadIndexを変更
            if (counterResults.Count > 0)
            {
                var result = counterResults[^1];
                if (result.ReDamage.Value > GetBattlerInfo(result.SubjectIndex.Value).Hp.Value && !result.DeadIndexList.Contains(result.SubjectIndex.Value))
                {
                    counterResults[^1].DeadIndexList.Add(result.SubjectIndex.Value);
                }
            }

            // Stateの重複をまとめる
            var addStates = new List<StateInfo>();
            var removeStates = new List<StateInfo>();
            var displayStates = new List<StateInfo>();
            var displayUpperStates = new List<StateInfo>();
            foreach (var actionResultInfo in actionResultInfos)
            {
                for (var i = actionResultInfo.AddedStates.Count - 1; i >= 0; i--)
                {
                    if (addStates.Find(a => a.CheckSameStateType(actionResultInfo.AddedStates[i])) == null)
                    {
                        addStates.Add(actionResultInfo.AddedStates[i]);
                    }
                    else
                    {
                        actionResultInfo.AddedStates.RemoveAt(i);
                    }
                }
                for (var i = actionResultInfo.RemovedStates.Count - 1; i >= 0; i--)
                {
                    if (removeStates.Find(a => a.CheckSameStateType(actionResultInfo.RemovedStates[i]) == true) == null)
                    {
                        removeStates.Add(actionResultInfo.RemovedStates[i]);
                    }
                    else
                    {
                        actionResultInfo.RemovedStates.RemoveAt(i);
                    }
                }
                for (var i = actionResultInfo.DisplayStates.Count - 1; i >= 0; i--)
                {
                    if (displayStates.Find(a => a.CheckSameStateType(actionResultInfo.DisplayStates[i]) == true) == null)
                    {
                        displayStates.Add(actionResultInfo.DisplayStates[i]);
                    }
                    else
                    {
                        actionResultInfo.DisplayStates.RemoveAt(i);
                    }
                }
                for (var i = actionResultInfo.DisplayUpperStates.Count - 1; i >= 0; i--)
                {
                    if (displayUpperStates.Find(a => a.CheckSameStateType(actionResultInfo.DisplayUpperStates[i]) == true) == null)
                    {
                        displayUpperStates.Add(actionResultInfo.DisplayUpperStates[i]);
                    }
                    else
                    {
                        actionResultInfo.DisplayUpperStates.RemoveAt(i);
                    }
                }
            }
        }

        public void ExecActionResultInfos(List<ActionResultInfo> actionResultInfos, bool addSaveData = true)
        {
            foreach (var actionResultInfo in actionResultInfos)
            {
                ExecActionResultInfo(actionResultInfo, addSaveData);
            }
        }

        /// <summary>
        /// ダメージなどを適用
        /// </summary>
        /// <param name="actionResultInfo"></param>
        private void ExecActionResultInfo(ActionResultInfo actionResultInfo, bool addSaveData = true)
        {
            var subject = GetBattlerInfo(actionResultInfo.SubjectIndex.Value);
            var target = GetBattlerInfo(actionResultInfo.TargetIndex.Value);
            foreach (var addState in actionResultInfo.AddedStates)
            {
                var addTarget = GetBattlerInfo(addState.TargetIndex.Value);
                addTarget.AddState(addState, true);
            }
            foreach (var removeState in actionResultInfo.RemovedStates)
            {
                var removeTarget = GetBattlerInfo(removeState.TargetIndex.Value);
                removeTarget.RemoveState(removeState, true);
            }
            if (actionResultInfo.HpDamage.Value != 0)
            {
                var hpDamage = actionResultInfo.HpDamage.Value;
                target.GainHp(-1 * hpDamage);
                target.Examine.DamagedValue.GainValue(hpDamage);
                _battleRecords[subject.Index.Value].GainAttackValue(hpDamage);
                _battleRecords[target.Index.Value].GainDamagedValue(hpDamage);
                if (actionResultInfo.WeakPoint)
                {
                    _battleRecords[subject.Index.Value].WeakAttackCount.GainValue(1);
                }
            }
            if (actionResultInfo.HpHeal.Value != 0 && (!actionResultInfo.DeadIndexList.Contains(target.Index.Value) || actionResultInfo.AliveIndexList.Contains(target.Index.Value)))
            {
                target.GainHp(actionResultInfo.HpHeal.Value);
                _battleRecords[subject.Index.Value].GainHealValue(actionResultInfo.HpHeal.Value);
            }
            if (actionResultInfo.CtDamage.Value != 0)
            {
                target.SeekCountTurn(-1 * actionResultInfo.CtDamage.Value);
            }
            if (actionResultInfo.PassiveCtDamage.Value != 0)
            {
                target.SeekPassiveCountTurn(-1 * actionResultInfo.PassiveCtDamage.Value);
            }
            if (actionResultInfo.CtHeal.Value != 0)
            {
                target.SeekCountTurn(actionResultInfo.CtHeal.Value, actionResultInfo.CtHealSkillId.Value);
            }
            if (actionResultInfo.ApHeal.Value != 0)
            {
                target.ChangeAp(actionResultInfo.ApHeal.Value * -1);
            }
            if (actionResultInfo.ApDamage.Value != 0)
            {
                target.ChangeAp(actionResultInfo.ApDamage.Value);
            }
            if (actionResultInfo.ReHeal.Value != 0)
            {
                subject.GainHp(actionResultInfo.ReHeal.Value);
            }
            if (actionResultInfo.ReDamage.Value != 0 || actionResultInfo.CurseDamage.Value != 0)
            {
                var reDamage = 0;
                if (target.IsAlive())
                {
                    reDamage += actionResultInfo.ReDamage.Value;
                }
                reDamage += actionResultInfo.CurseDamage.Value;
                if (reDamage > 0)
                {
                    subject.GainHp(-1 * reDamage);
                    _battleRecords[target.Index.Value].GainAttackValue(reDamage);
                    _battleRecords[subject.Index.Value].GainDamagedValue(reDamage);
                }
            }
            if (actionResultInfo.Missed)
            {
                target.Examine.DodgeCount.GainValue(1);
            }
            foreach (var targetIndex in actionResultInfo.ExecStateInfos)
            {
                var execTarget = GetBattlerInfo(targetIndex.Key);
                if (execTarget != null)
                {
                    foreach (var stateInfo in targetIndex.Value)
                    {
                        execTarget.UpdateStateCount(RemovalTiming.UpdateCount, stateInfo);
                    }
                }
            }
            foreach (var learnSkillId in actionResultInfo.LearnSkillIds)
            {
                if (target.Skills.Find(a => a.Id.Value == learnSkillId) == null)
                {
                    var learnSkill = new SkillInfo(learnSkillId);
                    target.Skills.Add(learnSkill);
                }
            }
            actionResultInfo.TurnCount.SetValue(_turnCount);
            /*
            if (addSaveData)
            {
                _saveBattleInfo.AddResultData(actionResultInfo);
            }
            */
        }

        public List<int> DeathBattlerIndex(List<ActionResultInfo> actionResultInfos)
        {
            var deathBattlerIndexes = new List<int>();
            foreach (var actionResultInfo in actionResultInfos)
            {
                foreach (var deadIndexList in actionResultInfo.DeadIndexList)
                {
                    // 例外
                    if (GetBattlerInfo(deadIndexList).IsState(StateType.Death))
                    {
                        deathBattlerIndexes.Add(deadIndexList);
                    }
                }
            }
            return deathBattlerIndexes;
        }

        public List<int> AliveBattlerIndex(List<ActionResultInfo> actionResultInfos)
        {
            var aliveBattlerIndex = new List<int>();
            foreach (var actionResultInfo in actionResultInfos)
            {
                foreach (var aliveIndexList in actionResultInfo.AliveIndexList)
                {
                    aliveBattlerIndex.Add(aliveIndexList);
                }
            }
            return aliveBattlerIndex;
        }

        public List<StateInfo> UpdateTurn()
        {
            var battleInfo = FirstActionBattler;
            var result = battleInfo.UpdateState(RemovalTiming.UpdateTurn);
            battleInfo.TurnEnd();
            var skillInfos = new List<SkillInfo>();
            foreach (var actionInfo in _turnActionInfos[_turnCount])
            {
                skillInfos.Add(actionInfo.SkillInfo);
            }
            battleInfo.TurnEndSkillSeekCountTurn(skillInfos);
            return result;
        }

        public List<StateInfo> UpdateTurns()
        {
            var list = new List<StateInfo>();
            foreach (var battleInfo in _battlers)
            {
                var result = battleInfo.UpdateState(RemovalTiming.TurnEnd);
                list.AddRange(result);
            }
            return list;
        }

        public int CheckActionAfterGainAp(ActionInfo actionInfo)
        {
            var gainAp = actionInfo.SkillInfo.ActionAfterGainAp();
            return gainAp;
        }

        public bool CheckActionAfterChange(ActionInfo actionInfo)
        {
            var chenage = actionInfo.SkillInfo.ActionAfterChange();
            if (chenage)
            {
                var subject = GetBattlerInfo(actionInfo.SubjectIndex.Value);
                var changeBattler = GetBattlerInfo(actionInfo.ActionResults[0].TargetIndex.Value);
                if (subject != null && changeBattler != null && changeBattler.IsAlive())
                {
                    ChangeUnitLineType(subject, changeBattler);
                    //_party.RemoveBattlerInfo(subject);
                    //_party.AddBattlerInfo(changeBattler);
                }
            }
            return chenage;
        }

        /// <summary>
        /// 前衛が戦闘不能で後衛が存在すれば交代する
        /// </summary>
        public void ChangeBattlerInfosLineType()
        {
            foreach (var fieldBattlerInfo in FieldBattlerInfos())
            {
                if (!fieldBattlerInfo.IsAlive())
                {
                    var changeBattler = _battlers.Find(a => a.IsActor == fieldBattlerInfo.IsActor && a.Index.Value == (fieldBattlerInfo.Index.Value + 3));
                    if (changeBattler != null && changeBattler.IsAlive())
                    {
                        ChangeUnitLineType(fieldBattlerInfo, changeBattler);
                        //_party.RemoveBattlerInfo(fieldBattlerInfo);
                        //_party.AddBattlerInfo(changeBattler);
                    }
                }
            }
        }

        public void ChangeUnitLineType(BattlerInfo subject, BattlerInfo changeBattler)
        {
            CurrentDeckInfo.SwapBattler(subject.Index.Value, changeBattler.ActorInfo != null ? changeBattler.ActorInfo.ActorId.Value : -1, changeBattler.Index.Value);
            bool adjust = CurrentDeckInfo.AdjustEditIndexes();

            foreach (var actorIdDict in CurrentDeckInfo.ActorIdDict)
            {
                var battlerInfo = _battlers.Find(a => a.ActorInfo != null && a.ActorInfo.ActorId.Value == actorIdDict.Value);
                if (battlerInfo != null)
                {
                    battlerInfo.Index.SetValue(actorIdDict.Key);
                    battlerInfo.SetLineIndex(actorIdDict.Key > 3 ? LineType.Back : LineType.Front);
                }
            }
            _party.SetBattlers(FieldBattlerInfos().FindAll(a => a.IsActor));
        }

        public void ActionAfterGainAp(int gainAp)
        {
            //_currentTurnBattler.GainMp(gainAp);
        }

        public List<StateInfo> UpdateNextSelfTurn(BattlerInfo battlerInfo)
        {
            var result = battlerInfo.UpdateState(RemovalTiming.NextSelfTurn);
            return result;
        }

        public bool CheckNoResetAp(ActionInfo actionInfo)
        {
            if (!actionInfo.TriggeredSkill)
            {
                var subject = GetBattlerInfo(actionInfo.SubjectIndex.Value);
                var noResetAp = actionInfo.SkillInfo.FeatureDates.Find(a => a.FeatureType == FeatureType.NoResetAp);
                return subject.IsAlive() && noResetAp != null;
            }
            return false;
        }

        public bool CheckLinkageBattlerInfo()
        {
            var battleInfo = FirstActionBattler;
            if (battleInfo != null && battleInfo.IsState(StateType.Linkage))
            {
                battleInfo.RemoveState(battleInfo.GetStateInfo(StateType.Linkage), true);
                var battlerIndex = battleInfo.Index.Value;
                var changeBattler = _reserveBattlers.Find(a => a.Index.Value == battlerIndex + 3);
                if (changeBattler != null)
                {
                    changeBattler.Index.SetValue(battlerIndex);
                    battleInfo.Index.SetValue(battlerIndex + 3);
                    changeBattler.SetAp(0);

                    _reserveBattlers.Remove(changeBattler);
                    _battlers.Remove(battleInfo);
                    _party.BattlerInfos.Remove(battleInfo);

                    _reserveBattlers.Add(battleInfo);
                    _battlers.Add(changeBattler);
                    _party.BattlerInfos.Add(changeBattler);
                    _battlers.Sort((a, b) => a.Index.Value - b.Index.Value > 0 ? 1 : -1);
                    return true;
                }
            }
            return false;
        }

        public bool CheckReaction(ActionInfo actionInfo)
        {
            var reAction = false;
            if (!actionInfo.TriggeredSkill)
            {
                var subject = GetBattlerInfo(actionInfo.SubjectIndex.Value);
                var noResetAp = actionInfo.SkillInfo.FeatureDates.Find(a => a.FeatureType == FeatureType.NoResetAp);
                if (subject.IsAlive() && noResetAp != null)
                {
                    reAction = true;
                }
                else
                {
                    subject.ResetAp();
                }
            }
            return reAction;
        }

        public void TurnEnd(ActionInfo actionInfo)
        {
            if (!actionInfo.TriggeredSkill)
            {
                var subject = GetBattlerInfo(actionInfo.SubjectIndex.Value);

                var afterApHalf = actionInfo.SkillInfo.FeatureDates.Find(a => a.FeatureType == FeatureType.SetAfterApHalf);
                if (afterApHalf != null)
                {
                    subject.ResetAp();
                    subject.SetAp((int)(subject.Ap.Value * afterApHalf.Param1 * 0.01f));
                }
                var afterAp = actionInfo.SkillInfo.FeatureDates.Find(a => a.FeatureType == FeatureType.SetAfterAp);
                if (afterAp != null)
                {
                    subject.SetAp(afterAp.Param1);
                }
                if (subject.IsState(StateType.Combo))
                {
                    var friends = GetFriendsAliveBattlerInfos(subject);
                    foreach (var friend in friends)
                    {
                        if (friend.Index != subject.Index)
                        {
                            friend.SetAp(0);
                        }
                    }
                }
            }
            var skillLog = new SkillLogListInfo
            {
                battlerInfo = GetBattlerInfo(actionInfo.SubjectIndex.Value),
                skillInfo = actionInfo.SkillInfo
            };
            //_skillLogs.Add(skillLog);
            PopActionInfo(actionInfo);
            SetSelectBattlerInfo(null);
        }

        public void CheckPlusSkill(ActionInfo actionInfo)
        {
            if (actionInfo == null)
            {
                return;
            }
            var subject = GetBattlerInfo(actionInfo.SubjectIndex.Value);
            var plusActionInfos = actionInfo.CheckPlusSkill();
            var plusTriggerSkillInfos = actionInfo.CheckPlusSkillTrigger();
            foreach (var skillInfo in plusTriggerSkillInfos)
            {
                var triggerDates = skillInfo.Master.TriggerDates;
                if (IsTriggeredSkillInfo(GetBattlerInfo(actionInfo.SubjectIndex.Value), triggerDates, actionInfo, actionInfo.ActionResults))
                {
                    plusActionInfos.Add(skillInfo);
                }
            }
            foreach (var plusActionInfo in plusActionInfos)
            {
                plusActionInfo.SetRangeType(CalcRangeType(plusActionInfo.Master, GetBattlerInfo(actionInfo.SubjectIndex.Value)));
            }
            var plusSkillParam = actionInfo.Master.FeatureDates.Find(a => a.FeatureType == FeatureType.PlusSkill);
            foreach (var plusActionInfo in plusActionInfos)
            {
                if (plusActionInfo.Master.SkillType == SkillType.Passive)
                {
                    if (!subject.ContainsPassiveSkillId(plusActionInfo.Master.Id))
                    {
                        subject.AddPassiveSkillId(plusActionInfo.Master.Id);
                    }
                }
                var selectIndexList = MakeAutoSelectIndex(plusActionInfo, -1, -1, actionInfo, actionInfo.ActionResults);
                if (selectIndexList.Count == 0 && plusActionInfo.Master.TargetType == TargetType.IsTriggerTarget)
                {
                    var triggerDates = plusActionInfo.Master.TriggerDates;
                    selectIndexList = TriggerTargetList(subject, triggerDates[0], actionInfo, actionInfo.ActionResults, plusActionInfo.Master.AliveType);
                }
                if (selectIndexList.Count > 0)
                {
                    AddReceiveActionInfo(plusActionInfo, selectIndexList, plusSkillParam != null && plusSkillParam.Param3 == 1);
                    continue;
                }
                if (selectIndexList.Count == 0)
                {
                    continue;
                }
                
                AddReceiveActionInfo(plusActionInfo, ActionInfoTargetIndexes(plusActionInfo, selectIndexList[0], -1, actionInfo), plusSkillParam != null && plusSkillParam.Param3 == 1);
            }
/*
            foreach (var skillInfo in plusTriggerSkillInfos)
            {
                var triggerDates = skillInfo.Master.TriggerDates;
                if (IsTriggeredSkillInfo(GetBattlerInfo(actionInfo.SubjectIndex.Value), triggerDates, actionInfo, actionInfo.ActionResults))
                {
                    if (skillInfo.Master.SkillType == SkillType.Passive)
                    {
                        if (!GetBattlerInfo(actionInfo.SubjectIndex.Value).ContainsPassiveSkillId(skillInfo.Master.Id))
                        {
                            GetBattlerInfo(actionInfo.SubjectIndex.Value).AddPassiveSkillId(skillInfo.Master.Id);
                        }
                    }
                    var plusTriggerActionInfo = new ActionInfo(skillInfo, _actionIndex, actionInfo.SubjectIndex.Value, -1, null);
                    plusTriggerActionInfo.SetTriggerSkill(true);
                    AddActionInfo(plusTriggerActionInfo, plusSkillParam != null && plusSkillParam.Param3 == 1);
                    AddTurnActionInfos(plusTriggerActionInfo, plusSkillParam != null && plusSkillParam.Param3 == 1);
                    plusTriggerActionInfo.SetRangeType(CalcRangeType(plusTriggerActionInfo.Master, GetBattlerInfo(actionInfo.SubjectIndex.Value)));
                }
            }
*/
        }

        public List<ActionResultInfo> CheckRegenerate(ActionInfo actionInfo)
        {
            var firstActionBattler = FirstActionBattler;
            var actionResultInfos = new List<ActionResultInfo>();
            var regenerateHp = firstActionBattler.RegenerateHpValue();
            if (regenerateHp > 0)
            {
                var featureData = new SkillData.FeatureData
                {
                    FeatureType = FeatureType.HpHeal,
                    Param1 = regenerateHp
                };
                var actionResultInfo = new ActionResultInfo(firstActionBattler, firstActionBattler, new List<SkillData.FeatureData>() { featureData }, -1);
                actionResultInfos.Add(actionResultInfo);
            }
            actionResultInfos.AddRange(AfterHealActionResults());
            if (actionInfo != null && actionInfo.HpDamageAction())
            {
                if (actionInfo == _battleFlowInfo.FirstActionInfo)
                {
                    actionResultInfos.AddRange(AssistHealActionResults(actionInfo));
                }
            }
            return actionResultInfos;
        }

        private List<ActionResultInfo> AfterHealActionResults()
        {
            var firstActionBattler = FirstActionBattler;
            var afterHealResults = new List<ActionResultInfo>();
            var afterSkillInfo = firstActionBattler.Skills.Find(a => a.FeatureDates.Find(b => b.FeatureType == FeatureType.AddState && (StateType)b.Param1 == StateType.AfterHeal) != null);
            if (firstActionBattler.IsState(StateType.AfterHeal) && afterSkillInfo != null)
            {
                var stateInfo = firstActionBattler.GetStateInfo(StateType.AfterHeal);
                var skillInfo = new SkillInfo(afterSkillInfo.Id.Value);
                var actionInfo = MakeActionInfo(firstActionBattler, skillInfo, false, false);

                if (actionInfo != null)
                {
                    var party = GetFriendsAliveBattlerInfos(firstActionBattler);
                    var targetIndexes = new List<int>();
                    foreach (var member in party)
                    {
                        if (firstActionBattler.Index != member.Index)
                        {
                            targetIndexes.Add(member.Index.Value);
                        }
                    }
                    foreach (var targetIndex in targetIndexes)
                    {
                        var featureData = new SkillData.FeatureData
                        {
                            FeatureType = FeatureType.HpHeal,
                            Param1 = stateInfo.Effect.Value
                        };

                        var actionResultInfo = new ActionResultInfo(GetBattlerInfo(targetIndex), GetBattlerInfo(targetIndex), new List<SkillData.FeatureData>() { featureData }, -1);
                        afterHealResults.Add(actionResultInfo);
                    }
                }
            }
            return afterHealResults;
        }

        private List<ActionResultInfo> AssistHealActionResults(ActionInfo actionInfo)
        {
            var assistHealResults = new List<ActionResultInfo>();
            var battlerInfo = GetBattlerInfo(actionInfo.SubjectIndex.Value);
            if (battlerInfo == null)
            {
                return assistHealResults;
            }
            var afterSkillInfo = battlerInfo.Skills.Find(a => a.FeatureDates.Find(b => b.FeatureType == FeatureType.AddState && (StateType)b.Param1 == StateType.AssistHeal) != null);
            if (battlerInfo.IsState(StateType.AssistHeal) && afterSkillInfo != null)
            {
                var stateInfo = battlerInfo.GetStateInfo(StateType.AssistHeal);
                var skillInfo = new SkillInfo(afterSkillInfo.Id.Value);
                var makeActionInfo = MakeActionInfo(battlerInfo, skillInfo, false, false);

                if (makeActionInfo != null)
                {
                    var party = GetFriendsAliveBattlerInfos(battlerInfo);
                    party = party.FindAll(a => a.IsAlive());
                    var targetIndexes = new List<int>();
                    foreach (var member in party)
                    {
                        targetIndexes.Add(member.Index.Value);
                    }
                    var healValue = makeActionInfo.ActionResults.FindAll(a => a.HpDamage.Value > 0).Count;
                    foreach (var targetIndex in targetIndexes)
                    {
                        var featureData = new SkillData.FeatureData
                        {
                            FeatureType = FeatureType.HpHeal,
                            Param1 = healValue * stateInfo.Effect.Value
                        };

                        var actionResultInfo = new ActionResultInfo(GetBattlerInfo(targetIndex), GetBattlerInfo(targetIndex), new List<SkillData.FeatureData>() { featureData }, -1);
                        assistHealResults.Add(actionResultInfo);
                    }
                }
            }
            return assistHealResults;
        }

        public List<ActionResultInfo> CheckSlipDamage()
        {
            var firstActionBattler = FirstActionBattler;
            var actionResultInfos = new List<ActionResultInfo>();
            var slipDamage = firstActionBattler.SlipDamage();
            if (slipDamage > 0)
            {
                var featureData = new SkillData.FeatureData
                {
                    FeatureType = FeatureType.HpSlipDamage,
                    Param1 = slipDamage
                };
                var actionResultInfo = new ActionResultInfo(firstActionBattler, firstActionBattler, new List<SkillData.FeatureData>() { featureData }, -1);
                actionResultInfos.Add(actionResultInfo);
            }
            return actionResultInfos;
        }

        // リザルトから発生するトリガースキルを生成
        public List<ActionInfo> CheckTriggerActiveInfos(TriggerTiming triggerTiming, ActionInfo actionInfo, List<ActionResultInfo> actionResultInfos, bool makeResult = false)
        {
            var madeActionInfos = new List<ActionInfo>();
            var actionInfos = new List<ActionInfo>();
            var triggeredSkills = new List<SkillInfo>();
            foreach (var battlers in _battlers)
            {
                var checkBattler = battlers;
                triggeredSkills.Clear();
                foreach (var skillInfo in checkBattler.ActiveSkills())
                {
                    if (UsedTurnSameActionInfo(skillInfo, checkBattler.Index.Value))
                    {
                        continue;
                    }
                    if (actionInfo == null || (actionInfo.Master.Id != skillInfo.Id.Value))
                    {
                        if (skillInfo != null && skillInfo.Master != null)
                        {
                            var triggerDates = skillInfo.Master.TriggerDates?.FindAll(a => a.TriggerTiming == triggerTiming);
                            if (IsTriggeredSkillInfo(checkBattler, triggerDates, actionInfo, actionResultInfos))
                            {
                                triggeredSkills.Add(skillInfo);
                            }
                        }
                    }
                }
                if (triggeredSkills.Count > 0)
                {
                    foreach (var triggeredSkill in triggeredSkills)
                    {
                        var interrupt = BattleUtility.IsInterruptTiming(triggerTiming);
                        if (triggeredSkill.Master.SkillType == SkillType.Unique && checkBattler.IsAwaken == false)
                        {
                            checkBattler.SetAwaken(true);
                        }
                        var makeActionInfo = MakeActionInfo(checkBattler, triggeredSkill, interrupt, true);
                        if (makeResult)
                        {
                            var counterSubjectIndex = actionInfo != null ? actionInfo.SubjectIndex.Value : -1;
                            var selectIndexList = MakeAutoSelectIndex(makeActionInfo, -1, counterSubjectIndex);
                            if (selectIndexList.Count == 0 && triggeredSkill.Master.TargetType == TargetType.IsTriggerTarget)
                            {
                                var triggerDates = triggeredSkill.Master.TriggerDates.FindAll(a => a.TriggerTiming == triggerTiming);
                                selectIndexList = TriggerTargetList(checkBattler, triggerDates[0], actionInfo, actionResultInfos, makeActionInfo.Master.AliveType);
                            }
                            if (selectIndexList.Count == 0)
                            {
                                continue;
                            }
                            AddReceiveActionInfo(makeActionInfo, ActionInfoTargetIndexes(makeActionInfo, selectIndexList[0], counterSubjectIndex, actionInfo, actionResultInfos), interrupt);
                        }
                        madeActionInfos.Add(makeActionInfo);
                    }
                }
            }

            return madeActionInfos;
        }

        public void CheckTriggerPassiveInfos(List<TriggerTiming> triggerTimings, ActionInfo actionInfo = null, List<ActionResultInfo> actionResultInfos = null)
        {
            // 同時発動制限管理
            var checkedSkillIds = new List<int>();
            foreach (var battlerInfo in _battlers)
            {
                if (battlerInfo.IsState(StateType.NoPassive))
                {
                    continue;
                }
                foreach (var passiveInfo in battlerInfo.PassiveSkills())
                {
                    if (!CheckCanPassiveSkill(battlerInfo, passiveInfo, triggerTimings))
                    {
                        continue;
                    }
                    var triggerDates = passiveInfo.Master.TriggerDates.FindAll(a => triggerTimings.Contains(a.TriggerTiming));

                    if (!IsTriggeredSkillInfo(battlerInfo, triggerDates, actionInfo, actionResultInfos))
                    {
                        continue;
                    }
                    //bool usable = CanUsePassiveCount(battlerInfo,passiveInfo.Id,triggerDates);
                    // 元の条件が成立
                    // 作戦で可否判定
                    var selectSkill = -1;
                    var selectTarget = -1;
                    var skillTriggerInfos = battlerInfo.SkillTriggerInfos;
                    var skillTriggerInfo = skillTriggerInfos.Find(a => a.SkillId == passiveInfo.Id.Value);
                    if (skillTriggerInfo == null)
                    {
                        skillTriggerInfo = new SkillTriggerInfo(battlerInfo.Index.Value, passiveInfo);
                        skillTriggerInfo.UpdateTriggerDates(new List<SkillTriggerData>());
                    }
                    (selectSkill, selectTarget) = SelectSkillTargetBySkillTriggerDates(battlerInfo, new List<SkillTriggerInfo>() { skillTriggerInfo }, actionInfo, actionResultInfos);
                    if (selectSkill != passiveInfo.Id.Value)
                    {
                        continue;
                    }
                    if (selectTarget == -1)
                    {
                        continue;
                    }
                    var interrupt = BattleUtility.IsInterruptTiming(triggerDates[0].TriggerTiming);
                    var result = MakePassiveSkillActionResults(battlerInfo, passiveInfo, interrupt, selectTarget, actionInfo, actionResultInfos, triggerDates[0]);
                    if (result != null && result.ActionResults.Count > 0)
                    {
                        checkedSkillIds.Add(passiveInfo.Id.Value);
                        // 継続パッシブは保存
                        var addPassive = passiveInfo.FeatureDates.Find(a => a.FeatureType == FeatureType.AddState);
                        if (addPassive != null && addPassive.Param2 == 999 && passiveInfo.Master.SkillType == SkillType.Passive)
                        {
                            AddPassiveSkillId(battlerInfo, addPassive.Param1, passiveInfo.Id.Value);
                        }
                    }
                }
            }
        }

        private bool CheckCanPassiveSkill(BattlerInfo battlerInfo, SkillInfo passiveInfo, List<TriggerTiming> triggerTimings)
        {
            var triggerDates = passiveInfo.Master.TriggerDates.FindAll(a => triggerTimings.Contains(a.TriggerTiming));

            // バトル中〇回以下使用
            var inBattleUseCountUnder = triggerDates.Find(a => a.TriggerType == TriggerType.InBattleUseCountUnder);
            if (inBattleUseCountUnder != null)
            {
                if (inBattleUseCountUnder.Param1 <= passiveInfo.UseCount.Value)
                {
                    return false;
                }
            }
            // period中〇回以下使用
            var inPeriodUseCountUnder = triggerDates.Find(a => a.TriggerType == TriggerType.InPeriodUseCountUnder);
            if (inPeriodUseCountUnder != null)
            {
                if (inPeriodUseCountUnder.Param1 <= passiveInfo.PeriodUseCount.Value)
                {
                    return false;
                }
            }
            // ターン中〇回以下使用
            var inTurnUseCountUnder = triggerDates.Find(a => a.TriggerType == TriggerType.InTurnUseCountUnder);
            if (inTurnUseCountUnder != null)
            {
                if (inTurnUseCountUnder.Param1 < UsedSameTurnActionInfo(passiveInfo))
                {
                    return false;
                }
            }
            if (battlerInfo.ContainsPassiveSkillId(passiveInfo.Id.Value))
            {
                return false;
            }
            if (passiveInfo.CountTurn.Value > 0)
            {
                return false;
            }
            if (passiveInfo.Master.TimingOnlyCount > 0)
            {
                if (passiveInfo.Master.TimingOnlyCount <= UsedSameTurnActionInfo(passiveInfo))
                {
                    return false;
                }
            }
            return true;
        }

        private void AddPassiveSkillId(BattlerInfo battlerInfo, int stateId, int passiveSkillId)
        {
            var stateData = DataSystem.FindState(stateId);
            if (stateData.OverLap == 0)
            {
                battlerInfo.AddPassiveSkillId(passiveSkillId);
            }
            else
            {
                var overLapCount = battlerInfo.GetStateInfoAll(stateData.StateType).Count;
                if (stateData.OverLap - 1 <= overLapCount)
                {
                    battlerInfo.AddPassiveSkillId(passiveSkillId);
                }
            }
        }

        private ActionInfo MakePassiveSkillActionResults(BattlerInfo battlerInfo, SkillInfo passiveInfo, bool IsInterrupt, int selectIndex, ActionInfo actionInfo = null, List<ActionResultInfo> actionResultInfos = null, SkillData.TriggerData triggerData = null)
        {
            if (!CheckCanUsePassive(passiveInfo, battlerInfo))
            {
                return null;
            }

            if (UsedTurnSameActionInfo(passiveInfo, battlerInfo.Index.Value))
            {
                return null;
            }
            var makeActionInfo = MakeActionInfo(battlerInfo, passiveInfo, IsInterrupt, true);
            var counterSubjectIndex = actionInfo != null ? actionInfo.SubjectIndex.Value : -1;
            if (selectIndex == -1)
            {
                // 対象を再取得
                var selectIndexList = MakeAutoSelectIndex(makeActionInfo, -1, counterSubjectIndex, actionInfo, actionResultInfos);
                if (selectIndexList.Count == 0 && passiveInfo.Master.TargetType == TargetType.IsTriggerTarget)
                {
                    selectIndexList = TriggerTargetList(battlerInfo, triggerData, actionInfo, actionResultInfos, makeActionInfo.Master.AliveType);
                }
                if (selectIndexList.Count == 0)
                {
                    return null;
                }
                selectIndex = selectIndexList[0];
            }
            if (makeActionInfo.Master.SkillType == SkillType.Unique && !battlerInfo.IsAwaken)
            {
                battlerInfo.SetAwaken(true);
            }
            AddReceiveActionInfo(makeActionInfo, ActionInfoTargetIndexes(makeActionInfo, selectIndex, counterSubjectIndex, actionInfo, actionResultInfos), IsInterrupt);
            passiveInfo.UseCount.GainValue(1);
            passiveInfo.InitCountTurn();
            return makeActionInfo;
        }

        public List<ActionResultInfo> CheckRemovePassiveInfos()
        {
            var actionResultInfos = new List<ActionResultInfo>();
            foreach (var battlerInfo in _battlers)
            {
                var passiveSkillIds = battlerInfo.PassiveSkillIds;
                for (int i = passiveSkillIds.Count - 1; i >= 0; i--)
                {
                    var passiveSkillData = DataSystem.FindSkill(passiveSkillIds[i]);
                    bool isRemove = false;
                    var featureDates = passiveSkillData.FeatureDates.FindAll(a => a.FeatureType == FeatureType.AddState);
                    foreach (var feature in featureDates)
                    {
                        var triggerDates = passiveSkillData.TriggerDates.FindAll(a => a.TriggerTiming == TriggerTiming.After || a.TriggerTiming == TriggerTiming.StartBattle);
                        if (!isRemove && triggerDates.Count > 0 && !IsTriggeredSkillInfo(battlerInfo, triggerDates, null, new List<ActionResultInfo>()))
                        {
                            isRemove = true;
                            var featureData = new SkillData.FeatureData
                            {
                                FeatureType = FeatureType.RemoveStatePassive,
                                Param1 = feature.Param1
                            };
                            if (passiveSkillData.Scope == ScopeType.Self)
                            {
                                var actionResultInfo = new ActionResultInfo(battlerInfo, battlerInfo, new List<SkillData.FeatureData>() { featureData }, passiveSkillData.Id);
                                if (actionResultInfos.Find(a => a.RemovedStates.Find(b => b.Master.StateType == (StateType)featureData.FeatureType) != null) != null)
                                {
                                }
                                else
                                {
                                    var stateInfos = battlerInfo.GetStateInfoAll((StateType)feature.Param1);
                                    if (battlerInfo.IsAlive() && stateInfos.Find(a => a.SkillId.Value == passiveSkillData.Id) != null)
                                    {
                                        actionResultInfos.Add(actionResultInfo);
                                        battlerInfo.RemovePassiveSkillId(passiveSkillIds[i]);
                                    }
                                }
                            }
                            else
                            if (passiveSkillData.Scope == ScopeType.All)
                            {
                                var partyMember = battlerInfo.IsActor ? BattlerActors() : BattlerEnemies();

                                switch (passiveSkillData.AliveType)
                                {
                                    case AliveType.DeathOnly:
                                        partyMember = partyMember.FindAll(a => !a.IsAlive());
                                        break;
                                    case AliveType.AliveOnly:
                                        partyMember = partyMember.FindAll(a => a.IsAlive());
                                        break;
                                    case AliveType.All:
                                        break;
                                }
                                foreach (var member in partyMember)
                                {
                                    var actionResultInfo = new ActionResultInfo(battlerInfo, member, new List<SkillData.FeatureData>() { featureData }, passiveSkillData.Id);
                                    if (actionResultInfos.Find(a => a.RemovedStates.Find(b => b.Master.StateType == (StateType)featureData.FeatureType) != null) != null)
                                    {
                                    }
                                    else
                                    {
                                        var stateInfos = battlerInfo.GetStateInfoAll((StateType)feature.Param1);
                                        if (member.IsAlive() && stateInfos.Find(a => a.SkillId.Value == passiveSkillData.Id) != null)
                                        {
                                            actionResultInfos.Add(actionResultInfo);
                                            member.RemovePassiveSkillId(passiveSkillIds[i]);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return actionResultInfos;
        }

        private bool IsTriggeredSkillInfo(BattlerInfo battlerInfo, List<SkillData.TriggerData> triggerDates, ActionInfo actionInfo, List<ActionResultInfo> actionResultInfos, int coverTargetIndex = -1)
        {
            var friends = GetFriendUnit(battlerInfo);
            var opponents = GetOpponentUnit(battlerInfo);
            bool isTriggered = false;
            var checkTriggerInfo = new CheckTriggerInfo(_turnCount, battlerInfo, BattlerActors(), BattlerEnemies(), _reserveBattlers, actionInfo, actionResultInfos, coverTargetIndex);
            if (triggerDates.Count > 0)
            {
                foreach (var triggerData in triggerDates)
                {
                    // 自身の行動前判定
                    if (triggerData.TriggerTiming == TriggerTiming.BeforeSelfUse)
                    {
                        if (battlerInfo.Index.Value != actionInfo.SubjectIndex.Value)
                        {
                            continue;
                        }
                    }
                    // 相手の行動前判定
                    if (triggerData.TriggerTiming == TriggerTiming.BeforeOpponentUse)
                    {
                        if (battlerInfo.Index.Value == actionInfo.SubjectIndex.Value)
                        {
                            continue;
                        }
                        if (battlerInfo.IsActor == GetBattlerInfo(actionInfo.SubjectIndex.Value).IsActor)
                        {
                            continue;
                        }
                    }
                    // 味方の行動前判定
                    if (triggerData.TriggerTiming == TriggerTiming.BeforeFriendUse)
                    {
                        if (battlerInfo.Index.Value == actionInfo.SubjectIndex.Value)
                        {
                            continue;
                        }
                        if (battlerInfo.IsActor != GetBattlerInfo(actionInfo.SubjectIndex.Value).IsActor)
                        {
                            continue;
                        }
                    }
                    /*
                    if (triggerTiming == TriggerTiming.Use)
                    {
                        if (actionInfo == null)
                        {
                            IsTriggered = false;
                            break;
                        }
                        if (actionInfo != null && actionInfo.SubjectIndex.Value != battlerInfo.Index)
                        {
                            IsTriggered = false;
                            break;
                        }
                    }
                    */
                    var key = (int)triggerData.TriggerType / 1000;
                    if (_checkTriggerDict.ContainsKey(key))
                    {
                        var checkTrigger = _checkTriggerDict[key];
                        isTriggered = checkTrigger.CheckTrigger(triggerData, battlerInfo, checkTriggerInfo);
                    }
                    else
                    {
                        // 個別判定
                        switch (triggerData.TriggerType)
                        {
                            case TriggerType.None:
                            case TriggerType.ExtendStageTurn: // 別処理で判定するためここではパス
                                isTriggered = true;
                                break;
                            case TriggerType.IsFriendBattler:
                                if (battlerInfo.IsAlive())
                                {
                                    if (actionResultInfos.Find(a => friends.AliveBattlerInfos.Find(b => GetBattlerInfo(a.TargetIndex.Value).IsActor == battlerInfo.IsActor) != null) != null)
                                    {
                                        isTriggered = true;
                                    }
                                }
                                break;
                            case TriggerType.IsOpponentBattler:
                                if (battlerInfo.IsAlive())
                                {
                                    if (actionResultInfos.Find(a => opponents.AliveBattlerInfos.Find(b => GetBattlerInfo(a.TargetIndex.Value).IsActor != battlerInfo.IsActor) != null) != null)
                                    {
                                        isTriggered = true;
                                    }
                                }
                                break;
                            case TriggerType.DeadWithoutSelf:
                                if (battlerInfo.IsAlive() && friends.DeadWithoutSelf(battlerInfo))
                                {
                                    isTriggered = true;
                                }
                                break;
                            case TriggerType.SelfDead:
                                if (actionResultInfos.Find(a => a.DeadIndexList.Contains(battlerInfo.Index.Value)) != null)
                                {
                                    isTriggered = true;
                                    var stateInfos = battlerInfo.GetStateInfoAll(StateType.Death);
                                    for (var i = 0; i < stateInfos.Count; i++)
                                    {
                                        battlerInfo.RemoveState(stateInfos[i], true);
                                        battlerInfo.SetPreserveAlive(true);
                                    }
                                }
                                break;
                            case TriggerType.AllEnemyCurseState:
                                /*
                                if (battlerInfo.IsAlive() && opponents.AliveBattlerInfos.Find(a => !a.IsState(StateType.DeBuffUpper)) == null && opponents.AliveBattlerInfos.FindAll(a => a.IsAlive()).Count > 0)
                                {
                                    IsTriggered = true;
                                }
                                */
                                break;
                            case TriggerType.AllEnemyFreezeState:
                                if (battlerInfo.IsAlive() && opponents.AliveBattlerInfos.Find(a => !a.IsState(StateType.Freeze)) == null && opponents.AliveBattlerInfos.FindAll(a => a.IsAlive()).Count > 0)
                                {
                                    isTriggered = true;
                                }
                                break;
                            case TriggerType.DemigodMemberCount:
                                if (battlerInfo.IsAlive())
                                {
                                    var demigodMember = opponents.AliveBattlerInfos.FindAll(a => a.IsState(StateType.Demigod));
                                    if (demigodMember.Count >= triggerData.Param1)
                                    {
                                        isTriggered = true;
                                    }
                                }
                                break;
                            case TriggerType.ActionResultAddState:
                                if (battlerInfo.IsAlive())
                                {
                                    if (actionInfo != null && battlerInfo.IsActor != GetBattlerInfo(actionInfo.SubjectIndex.Value).IsActor)
                                    {
                                        var states = actionInfo.SkillInfo.FeatureDates.FindAll(a => a.FeatureType == FeatureType.AddState);
                                        foreach (var state in states)
                                        {
                                            if (state.Param1 == (int)StateType.Stun || state.Param1 == (int)StateType.PoisunDamage || state.Param1 == (int)StateType.BurnDamage || state.Param1 == (int)StateType.Freeze)
                                            {
                                                isTriggered = true;
                                            }
                                        }
                                    }
                                }
                                break;
                            case TriggerType.DefeatEnemyByAttack:
                                if (actionInfo != null && actionResultInfos != null)
                                {
                                    var attackBattler = GetBattlerInfo(actionInfo.SubjectIndex.Value);
                                    if (battlerInfo.IsAlive() && attackBattler != null && battlerInfo.Index == attackBattler.Index)
                                    {
                                        foreach (var actionResultInfo in actionResultInfos)
                                        {
                                            foreach (var deadIndex in actionResultInfo.DeadIndexList)
                                            {
                                                if (battlerInfo.IsActor != GetBattlerInfo(deadIndex).IsActor)
                                                {
                                                    isTriggered = true;
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                            case TriggerType.AwakenDemigodAttribute:
                                var DemigodAttributes = friends.AliveBattlerInfos.FindAll(a => a.IsAwaken);
                                if (battlerInfo.IsAlive() && DemigodAttributes.Count > 0 && DemigodAttributes.Find(a => a.Skills.Find(b => b.Attribute == (AttributeType)triggerData.Param1 && b.Master.SkillType == SkillType.Unique) != null) != null)
                                {
                                    isTriggered = true;
                                }
                                break;
                            case TriggerType.ActionResultSelfDeath:
                                if (battlerInfo.IsAlive())
                                {
                                    if (actionResultInfos.Find(a => a.DeadIndexList.Contains(battlerInfo.Index.Value)) != null)
                                    {
                                        isTriggered = true;
                                    }
                                }
                                break;
                            case TriggerType.InterruptAttackDodge:
                                if (battlerInfo.IsAlive())
                                {
                                    if (actionInfo != null && battlerInfo.IsActor != GetBattlerInfo(actionInfo.SubjectIndex.Value).IsActor)
                                    {
                                        foreach (var actionResultInfo in actionInfo.ActionResults)
                                        {
                                            if (actionResultInfo.TargetIndex.Value == battlerInfo.Index.Value)
                                            {
                                                if (actionResultInfo.Missed)
                                                {
                                                    isTriggered = true;
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                            case TriggerType.HasMostCountTurnSKill:
                                if (battlerInfo.IsAlive())
                                {
                                    if (actionInfo != null && battlerInfo.IsActor == GetBattlerInfo(actionInfo.SubjectIndex.Value).IsActor)
                                    {
                                        var mostCountTurn = -1;
                                        var mostCountTurnIndex = -1;
                                        foreach (var targetIndex in actionInfo.ResultTargetIndexes())
                                        {
                                            var target = GetBattlerInfo(targetIndex);
                                            foreach (var skill in target.Skills)
                                            {
                                                if (skill.Master.CountTurn > mostCountTurn)
                                                {
                                                    mostCountTurn = skill.Master.CountTurn;
                                                    mostCountTurnIndex = targetIndex;
                                                }
                                            }
                                        }
                                        if (mostCountTurn > -1 && mostCountTurnIndex > -1)
                                        {
                                            foreach (var actionResultInfo in actionInfo.ActionResults)
                                            {
                                                if (actionResultInfo.TargetIndex.Value == mostCountTurnIndex)
                                                {
                                                    isTriggered = true;
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                        }
                    }
                    // Param3をAnd条件フラグにする
                    if (triggerData.Param3 == 1)
                    {
                        if (isTriggered)
                        {
                            isTriggered = false;
                            continue;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            return isTriggered;
        }

        private List<int> TriggerTargetList(BattlerInfo battlerInfo, SkillData.TriggerData triggerData, ActionInfo actionInfo, List<ActionResultInfo> actionResultInfos, AliveType aliveType)
        {
            var list = new List<int>();
            var key = (int)triggerData.TriggerType / 1000;
            if (_checkTriggerDict.ContainsKey(key))
            {
                var checkTriggerInfo = new CheckTriggerInfo(_turnCount, battlerInfo, BattlerActors(), BattlerEnemies(), _reserveBattlers, actionInfo, actionResultInfos);
                var checkTrigger = _checkTriggerDict[key];
                checkTrigger.AddTriggerTargetList(list, triggerData, battlerInfo, checkTriggerInfo);
            }
            if (actionInfo != null)
            {
                list = CalcAliveTypeIndexList(list, aliveType);
            }
            return list;
        }

        private bool CanUseTrigger(SkillInfo skillInfo, BattlerInfo battlerInfo)
        {
            bool canUse = true;
            if (skillInfo.TriggerDates.Count > 0)
            {
                canUse = IsTriggeredSkillInfo(battlerInfo, skillInfo.TriggerDates, null, new List<ActionResultInfo>());
            }
            return canUse;
        }

        private List<BattlerInfo> LineTargetBattlers(ScopeType scopeType, BattlerInfo targetBattler, List<BattlerInfo> targetBatterInfos)
        {
            var fronts = targetBatterInfos.FindAll(a => a.LineIndex == LineType.Front);
            var backs = targetBatterInfos.FindAll(a => a.LineIndex == LineType.Back);
            // この時点で有効なtargetIndexesが判定されているので人数で判定
            var lineTargets = new List<BattlerInfo>() { targetBattler };
            if (scopeType == ScopeType.Line)
            {
                lineTargets = targetBattler.LineIndex == LineType.Front ? fronts : backs;
            }
            else
            if (scopeType == ScopeType.All)
            {
                lineTargets = targetBatterInfos;
            }
            else
            if (scopeType == ScopeType.FrontLine)
            {
                lineTargets = fronts;
            }
            else
            if (scopeType == ScopeType.WithoutSelfAll)
            {
                lineTargets = targetBatterInfos;
                lineTargets.Remove(targetBattler);
            }
            return lineTargets;
        }

        public List<int> MakeAutoSelectIndex(ActionInfo actionInfo, int oneTargetIndex = -1, int counterSubjectIndex = -1, ActionInfo baseActionInfo = null, List<ActionResultInfo> baseActionResultInfos = null)
        {
            var indexList = new List<int>();
            // interruptされた行動の対象を引き継ぐ
            if (actionInfo.ActionResults.Count > 0)
            {
                foreach (var actionResultInfo in actionInfo.ActionResults)
                {
                    if (!actionResultInfo.CursedDamage && !indexList.Contains(actionResultInfo.TargetIndex.Value))
                    {
                        indexList.Add(actionResultInfo.TargetIndex.Value);
                    }
                }
                return indexList;
            }
            var targetIndexList = GetSkillTargetIndexList(actionInfo.Master.Id, actionInfo.SubjectIndex.Value, true, counterSubjectIndex, baseActionInfo, baseActionResultInfos);
            if (targetIndexList.Count == 0)
            {
                return targetIndexList;
            }
            var selectIndex = targetIndexList[0];
            if (oneTargetIndex > -1 && targetIndexList.Contains(oneTargetIndex))
            {
                selectIndex = oneTargetIndex;
            }
            return ActionInfoTargetIndexes(actionInfo, selectIndex, counterSubjectIndex, baseActionInfo, baseActionResultInfos);
        }

        private List<int> CalcAliveTypeIndexList(List<int> indexList, AliveType aliveType)
        {
            switch (aliveType)
            {
                case AliveType.DeathOnly:
                    indexList = indexList.FindAll(a => !_battlers.Find(b => a == b.Index.Value).IsAlive());
                    break;
                case AliveType.AliveOnly:
                    indexList = indexList.FindAll(a => _battlers.Find(b => a == b.Index.Value).IsAlive());
                    break;
                case AliveType.All:
                    break;
            }
            return indexList;
        }

        /// <summary>
        /// 繰り返し攻撃の時ターゲットを変える
        /// </summary>
        /// <param name="actionInfo"></param>
        public void ResetTargetIndexList(ActionInfo actionInfo)
        {
            var needReset = false;
            foreach (var targetIndex in actionInfo.CandidateTargetIndexList)
            {
                var target = GetBattlerInfo(targetIndex);
                if (!target.IsAlive() && actionInfo.Master.IsHpDamageFeature())
                {
                    needReset = true;
                }
            }
            if (needReset)
            {
                actionInfo.ActionResults.Clear();
                actionInfo.SetCandidateTargetIndexList(MakeAutoSelectIndex(actionInfo, -1, actionInfo.SubjectIndex.Value));
            }
        }

        public (int, int) MakeAutoSkillTriggerSkillId(BattlerInfo battlerInfo)
        {
            var skillInfos = battlerInfo.ActiveSkills().FindAll(a => CheckCanUse(a, battlerInfo));

            // トリガーデータからスキル検索
            // 使用可能なものに絞る
            var skillTriggerInfos = battlerInfo.SkillTriggerInfos.FindAll(a => skillInfos.Find(b => b.Id.Value == a.SkillId) != null);
            return SelectSkillTargetBySkillTriggerDates(battlerInfo, skillTriggerInfos);
        }

        public List<BattlerInfo> PreservedAliveEnemies()
        {
            var list = new List<BattlerInfo>();
            foreach (var battlerInfo in FieldBattlerInfos())
            {
                if (battlerInfo.PreserveAlive)
                {
                    list.Add(battlerInfo);
                    battlerInfo.SetPreserveAlive(false);
                }
            }
            return list;
        }

        public List<BattlerInfo> NotDeadMembers()
        {
            var list = new List<BattlerInfo>();
            foreach (var battlerInfo in FieldBattlerInfos())
            {
                if (battlerInfo.Hp.Value > 0 && battlerInfo.IsState(StateType.Death))
                {
                    var states = battlerInfo.GetStateInfoAll(StateType.Death);
                    foreach (var state in states)
                    {
                        battlerInfo.RemoveState(state, true);
                    }
                    list.Add(battlerInfo);
                }
            }
            return list;
        }

        // 戦闘不能の付与者のステート効果を解除する
        public List<StateInfo> EndRemoveState()
        {
            var removeStateInfos = new List<StateInfo>();
            foreach (var battler in FieldBattlerInfos())
            {
                if (!battler.IsAlive())
                {
                    for (var i = battler.StateInfos.Count - 1; i >= 0; i--)
                    {
                        if (battler.StateInfos[i].Master.RemoveByDeath)
                        {
                            if (!battler.StateInfos[i].IsStartPassive())
                            {
                                removeStateInfos.Add(battler.StateInfos[i]);
                                battler.RemoveState(battler.StateInfos[i], true);
                            }
                        }
                    }
                }
            }
            return removeStateInfos;
        }

        // 戦闘不能に聖棺がいたら他の対象に移す
        public List<StateInfo> EndHolyCoffinState()
        {
            var addStateInfos = new List<StateInfo>();
            //var StateTypes = RemoveDeathStateTypes();
            foreach (var battler in FieldBattlerInfos())
            {
                if (battler.IsAlive() == false)
                {
                    for (var i = battler.StateInfos.Count - 1; i >= 0; i--)
                    {
                        if (battler.StateInfos[i].StateType == StateType.HolyCoffin)
                        {
                            battler.RemoveState(battler.StateInfos[i], true);
                            var randTargets = GetFriendsAliveBattlerInfos(battler);
                            if (randTargets.Count > 0)
                            {
                                var rand = UnityEngine.Random.Range(0, randTargets.Count);
                                battler.StateInfos[i].TargetIndex.SetValue(randTargets[rand].Index.Value);
                                randTargets[rand].AddState(battler.StateInfos[i], true);
                                addStateInfos.Add(battler.StateInfos[i]);
                            }
                        }
                    }
                }
            }
            return addStateInfos;
        }

        // 味方に生存者がいなくなったら透明を外す
        public List<StateInfo> EndRemoveShadowState()
        {
            var units = new List<UnitInfo>();
            if (_party.AliveBattlerInfos.Count == 1)
            {
                units.Add(_party);
            }
            if (_troop.AliveBattlerInfos.Count == 1)
            {
                units.Add(_troop);
            }
            var removeStateInfos = new List<StateInfo>();
            foreach (var unit in units)
            {
                var aliveMember = unit.AliveBattlerInfos[0];
                if (aliveMember.IsState(StateType.Shadow))
                {
                    var shadowStates = aliveMember.GetStateInfoAll(StateType.Shadow);
                    for (int i = shadowStates.Count - 1; i >= 0; i--)
                    {
                        removeStateInfos.Add(shadowStates[i]);
                        aliveMember.RemoveState(shadowStates[i], true);
                    }
                }
            }
            return removeStateInfos;
        }

        public void GainAttackedCount(int targetIndex)
        {
            GetBattlerInfo(targetIndex).Examine.AttackedCount.GainValue(1);
        }

        public void GainBeCriticalCount(int targetIndex)
        {
            GetBattlerInfo(targetIndex).Examine.BeCriticalCount.GainValue(1);
        }

        public void GainMaxDamage(int targetIndex, int damage)
        {
            GetBattlerInfo(targetIndex).GainMaxDamage(damage);
        }

        public bool CheckVictory()
        {
            bool isVictory = _troop.BattlerInfos.Find(a => a.IsAlive()) == null;
            return isVictory;
        }

        public bool CheckIsOver()
        {
            return false;
            /*
            // 全員のMpが0
            bool over = FieldBattlerInfos().Find(a => SkillActionList(a).Count > 0) == null;
            return over;
            */
        }

        public void MakeBattleScore(bool isVictory, StrategySceneInfo strategySceneInfo)
        {
            var battleScore = new BattleScore();
            battleScore.ResultScore = -1;
            strategySceneInfo.BattleScore = battleScore;
            if (!isVictory)
            {
                return;
            }
            // ターン数の減点
            //var turns = (5 * _troop.BattlerInfos.Count) - _turnCount;
            //score += turns;
            //score = Math.Max(0, score);
            //score = Math.Min(100, score);
            // 与ダメージ - 被ダメージの加算
            var attack = 0;
            //var damaged = 0;
            //var remainHpPercent = 0f;
            var maxDamage = 0;
            var defeated = 0;
            var totalTurnCount = 0;
            var awakeCount = 0;
            var actorCount = 0;
            var weakAttackCount = 0;
            foreach (var battleRecord in _battleRecords)
            {
                if (battleRecord.Key < 10)
                {
                    var actorInfo = GetBattlerInfo(battleRecord.Key);
                    if (actorInfo == null)
                    {
                        actorInfo = _reserveBattlers.Find(a => a.Index.Value == battleRecord.Key);
                    }
                    if (actorInfo == null)
                    {
                        continue;
                    }
                    var actorMaxHp = actorInfo.MaxHp;
                    //remainHpPercent += 1f - (float)(actorMaxHp - actorInfo.Hp.Value) / actorMaxHp;
                    actorCount++;
                    if (battleRecord.Value.MaxDamage > maxDamage)
                    {
                        maxDamage = battleRecord.Value.MaxDamage;
                    }
                    attack += battleRecord.Value.AttackValue;
                    if (!actorInfo.IsAlive())
                    {
                        defeated += 1;
                    }
                    totalTurnCount += actorInfo.TurnCount.Value;
                    if (actorInfo.IsAwaken)
                    {
                        awakeCount += 1;
                    }
                    weakAttackCount += battleRecord.Value.WeakAttackCount.Value;
                }
            }
            var score = 0f;
            var actorLvMax = _party.BattlerInfos.Max(a => a.Level.Value);
            var enemyLvMax = _troop.BattlerInfos.Max(a => a.Level.Value);
            var enemyLvAvarage = _troop.BattlerInfos.Average(a => a.Level.Value);
            var lvPoint = (enemyLvMax - actorLvMax) * 5;
            // 戦闘不能数の少なさで加算
            if (defeated == 0 && lvPoint > -20)
            {
                score += 20 + lvPoint;
                battleScore.DefeatedCountScore += 20 + lvPoint;
            }
            // 戦闘不能数の数で減算
            if (defeated > 0)
            {
                score -= defeated * 20;
                battleScore.DefeatedCountScore -= defeated * 20;
            }
            // 敵Lvの高さに応じた基礎点
            var enemyLvBonus = (int)Math.Ceiling(enemyLvAvarage * 0.1f);
            score += (int)enemyLvAvarage * enemyLvBonus;
            battleScore.EnemyLvAvarageScore = (int)enemyLvAvarage * enemyLvBonus;
            if (totalTurnCount <= 30)
            {
                // ターン数の少なさとLv差で加算
                var turnValue = 30 - totalTurnCount + lvPoint;
                if (turnValue > 0)
                {
                    score += turnValue;
                    battleScore.TurnCountScore = turnValue;
                }
            }
            // 覚醒したキャラ数
            if (awakeCount > 0)
            {
                var awakenCountScore = awakeCount * 5;
                score += awakenCountScore;
                battleScore.AwakenCountScore = awakenCountScore;
            }
            // 被ダメージ率の加算
            /*
            if (remainHpPercent > 0)
            {
                score += (remainHpPercent / actorCount) * 100;
            }
            */
            // 最大ダメージ値の加算
            if (maxDamage > 0)
            {
                var maxDamageScore = maxDamage / 10;
                score += maxDamageScore;
                battleScore.MaxDamageScore = maxDamageScore;
            }
            // 弱点攻撃をした回数
            if (weakAttackCount > 0)
            {
                var weakAttackCountScore = (int)MathF.Min(10, weakAttackCount);
                score += weakAttackCountScore;
                battleScore.WeakAttackCountScore = weakAttackCountScore;
            }
            //battleScore.RemainHpPercent = (int)((remainHpPercent / actorCount) * 100);
            battleScore.MaxDamage = maxDamage;
            battleScore.DefeatedCount = defeated;
            battleScore.WeakAttackCount = weakAttackCount;
            battleScore.AwakenCount = awakeCount;
            battleScore.EnemyLvAvarage = (float)enemyLvAvarage;
            PartyInfo.PartyStatInfo.BattleScore.GainValue((int)score);
            PartyInfo.PartyStatInfo.GainBattleScoreTotal.GainValue((int)score);
            PartyInfo.PartyStatInfo.TotalDamage.GainValue(attack);
            CheckAchievements();
            battleScore.ResultScore = (int)score;
        }

        public List<GetItemInfo> MakeBattlerResult()
        {
            var list = new List<GetItemInfo>();
            var enemyInfos = BattlerEnemies().FindAll(a => !a.IsAlive());
            var bossLv = enemyInfos.Max(a => a.Level.Value);
            var exp = 20f;
            if (enemyInfos.Count == 2)
            {
                exp = 30f / 2f;
            }
            if (enemyInfos.Count == 3)
            {
                exp = 40f / 3f;
            }
            //var battleScore = PartyInfo.PartyStatInfo.BattleScore.Value;
            // 経験値アイテムを作る
            foreach (var actorInfo in UnitBattlerActors())
            {
                var gainExp = 0f;
                foreach (var enemyInfo in enemyInfos)
                {
                    gainExp += exp + ((enemyInfo.Level.Value - actorInfo.Level.Value) * 2);
                }
                /*
                if (_battleRecords[actorInfo.Index.Value].MaxDamage > 0 || _battleRecords[actorInfo.Index.Value].HealValue > 0)
                {
                    gainExp += (31 - (actorInfo.Level.Value - bossLv)) / 3;
                }
                */

                // 獲得経験値アップ付与
                var expRateUps = actorInfo.Skills.FindAll(a => a.Master.FeatureDates.Find(b => b.FeatureType == FeatureType.GetExpRateUp) != null);
                var upperRate = 0f;
                foreach (var expRateUp in expRateUps)
                {
                    upperRate += expRateUp.FeatureDates[0].Param1 * 0.01f;
                }
                /*
                // 施設効果経験値アップ付与
                var expRateUpBuildings = PartyInfo.BuildingSkills().FindAll(a => a.Master.FeatureDates.Find(b => b.FeatureType == FeatureType.GetExpRateUp) != null);
                // 重複はしない
                // triggerあり
                var upperBuildingsRate = 0f;
                foreach (var expRateUpBuilding in expRateUpBuildings)
                {
                    if (IsTriggeredSkillInfo(actorInfo, expRateUpBuilding.TriggerDates, null, null))
                    {
                        var rate = expRateUpBuilding.FeatureDates[0].Param1 * 0.01f;
                        if (rate > upperBuildingsRate)
                        {
                            upperBuildingsRate = rate;
                        }
                    }
                }
                upperRate += upperBuildingsRate;
                */
                if (upperRate > 0)
                {
                    gainExp *= (1 + upperRate);
                }
/*
                if (battleScore > 0)
                {
                    gainExp = (int)(gainExp * (1 + (battleScore * 0.0001f)));
                }
*/
                if (gainExp <= 0)
                {
                    gainExp = 1;
                }
                else
                if (gainExp > 100)
                {
                    //gainExp = 100;
                }

                var expData = new GetItemData
                {
                    Type = GetItemType.Exp,
                    // 誰に対して
                    Param1 = actorInfo.ActorInfo.ActorId.Value,
                    // いくつ
                    Param2 = (int)gainExp
                };
                var expItem = new GetItemInfo(expData);
                list.Add(expItem);
            }
            // スキル経験値を代入
            foreach (var battlerInfo in UnitBattlerActors())
            {
                var target = PartyInfo.ActorInfos.Find(a => a.ActorId.Value == battlerInfo.ActorInfo.ActorId.Value);
                foreach (var equipmentId in battlerInfo.ActorInfo.EquipmentIds)
                {
                    var equipmentData = DataSystem.FindEquipment(equipmentId);
                    foreach (var learningDate in equipmentData.LearningDates)
                    {
                        // スキル経験値 = 属性適正 x 習熟スピード
                        var skillExp = target.GetSkillExp(DataSystem.FindSkill(learningDate.SkillId).Attribute, learningDate.Rate, PartyInfo.EditableActorInfos());
                        var learned = target.GainSkillExp(learningDate.SkillId, skillExp);                        
                        // 会得していたら
                        if (learned && !target.MastarySkillIds.Contains(learningDate.SkillId) && !target.IsLearnedSkill(learningDate.SkillId))
                        {
                            var skillMastary = new GetItemData
                            {
                                Type = GetItemType.SkillMastary,
                                Param1 = battlerInfo.ActorInfo.ActorId.Value,
                                Param2 = learningDate.SkillId
                            };
                            var skillMastaryItem = new GetItemInfo(skillMastary);
                            list.Add(skillMastaryItem);
                            // 進化スキルがあれば習得する
                            /*
                            var nextSkill = DataSystem.FindSkill(learningDate.SkillId + 1);
                            if (nextSkill != null && nextSkill.Rank > 0 && !PartyInfo.LearningSkillIds.Contains(nextSkill.Id))
                            {
                                list.Add(MakeGetItemInfo(GetItemType.Skill, nextSkill.Id));
                            }
                            */
                        }
                    }
                }
                /*
                foreach (var useSkillCountDict in _battleRecords[battlerInfo.Index.Value].UseSkillCountDict)
                {
                    var skillData = DataSystem.FindSkill(useSkillCountDict.Key);
                    if (skillData.Id < 1000 || skillData.Rank <= RankType.ActiveRank1)
                    {
                        continue;
                    }
                    var target = PartyInfo.ActorInfos.Find(a => a.ActorId.Value == battlerInfo.ActorInfo.ActorId.Value);
                    var learned = target.GainSkillExp(useSkillCountDict.Key, useSkillCountDict.Value);
                    // 会得していたら
                    if (learned && !target.MastarySkillIds.Contains(useSkillCountDict.Key) && !target.IsLearnedSkill(useSkillCountDict.Key))
                    {
                        var skillMastary = new GetItemData
                        {
                            Type = GetItemType.SkillMastary,
                            Param1 = battlerInfo.ActorInfo.ActorId.Value,
                            Param2 = useSkillCountDict.Key
                        };
                        var skillMastaryItem = new GetItemInfo(skillMastary);
                        list.Add(skillMastaryItem);
                        // 進化スキルがあれば習得する
                        var nextSkill = DataSystem.FindSkill(skillData.Id + 1);
                        if (nextSkill != null && nextSkill.Rank > 0 && !PartyInfo.LearningSkillIds.Contains(nextSkill.Id))
                        {
                            list.Add(MakeGetItemInfo(GetItemType.Skill, nextSkill.Id));
                        }
                    }
                    // 属性適正値アップ
                    var attributeUp = target.GainAttributeExp(skillData.Attribute, useSkillCountDict.Value);
                    if (attributeUp && target.AttributeRanks(new List<ActorInfo>())[(int)skillData.Attribute - 1] != AttributeRank.S)
                    {
                        var attirbuteUpDate = new GetItemData
                        {
                            Type = GetItemType.AttributeUp,
                            Param1 = battlerInfo.ActorInfo.ActorId.Value,
                            Param2 = (int)skillData.Attribute
                        };
                        var attirbuteUpItem = new GetItemInfo(attirbuteUpDate);
                        list.Add(attirbuteUpItem);
                    }
                }
                */
            }
            if (_sceneParam.GetItemInfos != null)
            {
                list.AddRange(_sceneParam.GetItemInfos);
            }
            return list;
        }

        public bool CheckDefeat()
        {
            bool isDefeat = _party.BattlerInfos.Find(a => a.IsAlive()) == null;
            return isDefeat;
        }

        public bool IsEnableDefeat()
        {
            if (_sceneParam != null)
            {
                return _sceneParam.IsEnableDefeat;
            }
            return false;
        }

        public bool EnableEscape()
        {
            return false;
        }

        public void EndBattle()
        {
            if (TempInfo.InReplay)
            {
                TempInfo.SetInReplay(false);
            }
            // Hp等を同期
            foreach (var battler in _battlers)
            {
                if (battler.ActorInfo == null)
                {
                    continue;
                }
                var actorInfo = PartyInfo.ActorInfos.Find(a => a.ActorId == battler.ActorInfo.ActorId);
                actorInfo.ChangeHp(battler.Hp.Value);
                battler.SetAwaken(false);
                // Period回数制限の使用回数を設定
                foreach (var skill in battler.Skills)
                {
                    if (skill.Master.TriggerDates.Find(a => a.TriggerType == TriggerType.InPeriodUseCountUnder) != null)
                    {
                        actorInfo.SetSkillUseCount(skill.Id.Value, skill.PeriodUseCount.Value);
                    }
                }
            }
            SaveSystem.SaveOptionStart(GameSystem.OptionData);
        }

        public void AddEnemyInfoSkillId()
        {
            foreach (var battlerInfo in _battlers)
            {
                if (!battlerInfo.IsActor)
                {
                    foreach (var skillInfo in battlerInfo.Skills)
                    {
                        //AddPlayerInfoSkillId(skillInfo.Id);
                    }
                }
            }
        }

        public List<SystemData.CommandData> SideMenu()
        {
            var list = new List<SystemData.CommandData>();
            var menuCommand = new SystemData.CommandData
            {
                Id = 2,
                Name = DataSystem.GetText(19700),
                Key = "Help"
            };
            list.Add(menuCommand);
            return list;
        }

        public void ChangeBattleAuto()
        {
            OptionUtility.ChangeBattleAuto(!GameSystem.OptionData.BattleAuto);
        }

        public bool IsBattleAuto()
        {
            return GameSystem.OptionData.BattleAuto;
        }

        public int WaitFrameTime(int time)
        {
            var waitFrame = GameSystem.OptionData.BattleAnimationSkip ? 1 : time;
            return (int)(waitFrame / GameSystem.OptionData.BattleSpeed);
        }

        public string BattleStartText()
        {
            var textId = _sceneParam.BossBattle ? 16041 : 16040;
            return DataSystem.GetText(textId);
        }

        public void ForceVictory()
        {
            foreach (var enemy in BattlerEnemies())
            {
                enemy.GainHp(-9999);
            }
        }

        public void StopApCount(bool isStop)
        {
            var state = new StateInfo(StateType.NoApRecover, 9999, 9999, 0, 0, -1);
            if (isStop)
            {
                foreach (var battler in Battlers)
                {
                    battler.AddState(state, true);
                }
            }
            else
            {
                foreach (var battler in Battlers)
                {
                    battler.RemoveState(state, true);
                }
            }
        }

#if UNITY_EDITOR
        public List<TestActionData> testActionDates = new();
        public int testActionIndex = 0;
        public void MakeTestBattleAction()
        {
            testActionDates.Clear();
            testActionDates = Resources.Load<TestBattleData>("Data/TestBattle").TestActionDates;
        }

        public BattlerInfo TestBattler()
        {
            if (testActionDates.Count > testActionIndex)
            {
                return GetBattlerInfo(testActionDates[testActionIndex].BattlerIndex);
            }
            return null;
        }

        public int TestSkillId()
        {
            if (testActionDates.Count > testActionIndex)
            {
                return testActionDates[testActionIndex].SkillId;
            }
            return 0;
        }

        public void SeekActionIndex()
        {
            testActionIndex++;
        }
#endif
    }

    public class BattleSceneInfo
    {
        public List<ActorInfo> ActorInfos;
        //public List<BattlerInfo> ActorBattlerInfos;
        public List<UnitInfo> ActorUnitInfos;
        public List<BattlerInfo> EnemyInfos;
        public List<UnitInfo> EnemyUnitInfos;
        public List<GetItemInfo> GetItemInfos;
        public bool BossBattle;
        public bool IsEnableDefeat;
    }
}