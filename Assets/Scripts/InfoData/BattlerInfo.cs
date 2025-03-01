using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ryneus
{
    [Serializable]
    public class BattlerInfo 
    {
        private StatusInfo _status = null;
        public StatusInfo Status => _status;
        public StatusInfo CurrentStatus(bool isNoEffect)
        {
            var currentStatus = new StatusInfo();
            currentStatus.SetParameter(MaxHp,MaxMp,CurrentAtk(isNoEffect),CurrentDef(isNoEffect),CurrentSpd(isNoEffect));
            return currentStatus;
        }
        public ParameterInt Index = new();
        public ParameterInt EnemyIndex = new();
        private bool _isActor = false;
        public bool IsActor => _isActor;
        // 見た目上は味方か
        private bool _isActorView = false;
        public bool IsActorView => _isActorView;
        private bool _isAlcana = false;
        public bool isAlcana => _isAlcana;
        public ParameterInt CharaId = new();
        public ParameterInt Level = new();
        public int MaxHp => _status.GetParameter(StatusParamType.Hp) + StateEffectAll(StateType.MaxHpUp);
        public int MaxMp => _status.GetParameter(StatusParamType.Mp) + StateEffectAll(StateType.MaxMpUp);
        public ParameterInt Hp = new();
        public float HpRate => Hp.Value > 0 ? Hp.Value / (float)MaxHp : 0;
        public ParameterInt Mp = new();
        public float MpRate => Mp.Value > 0 ? Mp.Value / (float)MaxMp : 0;
        private float _preserveMinusAp = 0;
        public ParameterFloat Ap = new();
        
        private List<SkillInfo> _skills = new ();
        public List<SkillInfo> Skills => _skills;
        private List<SkillInfo> _enhanceSkills = new ();
        public List<SkillInfo> EnhanceSkills => _enhanceSkills;
        private List<int> _passiveActiveSkillIds = new ();
        private ActorInfo _actorInfo;
        public ActorInfo ActorInfo => _actorInfo;
        private ParameterInt EnemyId = new();
        public EnemyData EnemyData => DataSystem.Enemies.Find(a => a.Id == EnemyId.Value);
        private List<KindType> _kinds = new ();
        public List<KindType> Kinds => _kinds;
        private List<KindType> _weakPoints = new ();
        public List<KindType> WeakPoints => _weakPoints;
        public ParameterInt LastSelectSkill = new();
        private List<StateInfo> _stateInfos = new ();
        public List<StateInfo> StateInfos => _stateInfos;

        private bool _isAwaken = false;
        public bool IsAwaken => _isAwaken;

        private LineType _lineIndex = 0;
        public LineType LineIndex => _lineIndex;

        private bool _bossFlag = false;
        public bool BossFlag => _bossFlag;
        
        public ParameterInt ChainSuccessCount = new();
        public ParameterInt PayBattleMp = new();
        public ParameterInt AttackedCount = new();
        public ParameterInt MaxDamage = new();
        public ParameterInt DodgeCount = new();
        public ParameterInt HealCount = new();
        public ParameterInt BeCriticalCount = new();
        public ParameterInt DamagedValue = new();
        
        private int _lastTargetIndex = 0;
        public void SetLastTargetIndex(int index)
        {
            _lastTargetIndex = index;
        }

        public ParameterInt TurnCount = new();
        private int _demigodParam = 0;
        public int DemigodParam => _demigodParam;

        private bool _preserveAlive = false;
        public bool PreserveAlive => _preserveAlive;

        private List<SkillTriggerInfo> _skillTriggerInfos = new ();
        public List<SkillTriggerInfo> SkillTriggerInfos => _skillTriggerInfos;

        private List<int> _passiveSkillIds = new ();
        public List<int> PassiveSkillIds => _passiveSkillIds;
        public bool CheckPassiveSkillId(int skillId)
        {
            return _passiveActiveSkillIds.Contains(skillId);
        }
        public void AddPassiveSkillId(int skillId)
        {
            if (_passiveSkillIds.Contains(skillId))
            {
                return;
            }
            _passiveSkillIds.Add(skillId);
        }
        public void RemovePassiveSkillId(int skillId)
        {
            if (_passiveSkillIds.Contains(skillId))
            {
                _passiveSkillIds.Remove(skillId);
            }
        }

        public BattlerInfo(ActorInfo actorInfo,int index)
        {
            _skillTriggerInfos = actorInfo.SkillTriggerInfos;
            CharaId.SetValue(actorInfo.ActorId.Value);
            Level.SetValue(actorInfo.Level);
            var statusInfo = new StatusInfo();
            statusInfo.SetParameter(
                actorInfo.CurrentParameter(StatusParamType.Hp),
                actorInfo.CurrentParameter(StatusParamType.Mp),
                actorInfo.CurrentParameter(StatusParamType.Atk),
                actorInfo.CurrentParameter(StatusParamType.Def),
                actorInfo.CurrentParameter(StatusParamType.Spd)
            );
            _status = statusInfo;
            Index.SetValue(index);

            _skills.Clear();
            foreach (var equipmentSkillId in actorInfo.EquipmentSkillIds)
            {
                var battleSkill = new SkillInfo(equipmentSkillId.Value);
                battleSkill.InitCountTurn();
                _skills.Add(battleSkill);
            }
            
            // _skills確定後に強化する
            var enhanceSkills = _skills.FindAll(a => a.IsEnhanceSkill());
            foreach (var enhanceSkill in enhanceSkills)
            {
                var result = new ActionResultInfo(this,this,enhanceSkill.FeatureDates,enhanceSkill.Id.Value);
            }
            _enhanceSkills = enhanceSkills;


            _demigodParam = actorInfo.DemigodParam;
            _isActor = true;
            _isAlcana = false;
            
            _actorInfo = actorInfo;
            _isActorView = true;
            Hp.SetValue(actorInfo.CurrentHp.Value);
            Mp.SetValue(actorInfo.CurrentMp.Value);
            _lineIndex = actorInfo.LineIndex;

            if (actorInfo.LastSelectSkillId == 0)
            {
                LastSelectSkill.SetValue(_skills.Find(a => a.Id.Value > 100).Id.Value);
            } else
            {
                LastSelectSkill.SetValue(actorInfo.LastSelectSkillId);
            }
            InitKindTypes(actorInfo.Master.Kinds);
            InitSkillCount();
            ResetAp(true);
        }

        public BattlerInfo(EnemyData enemyData,int lv,int index,LineType lineIndex,bool isBoss)
        {
            EnemyIndex = new ParameterInt();
            EnemyId.SetValue(enemyData.Id);
            CharaId.SetValue(enemyData.Id);
            Level.SetValue(lv);
            _bossFlag = isBoss;
            InitParamInfos(enemyData);
            Index.SetValue(index + 100);
            _isActor = false;
            _isAlcana = false;
            _lineIndex = lineIndex;
            _isActorView = enemyData.Id > 1000;
            if (_isActorView)
            {
                _actorInfo = new ActorInfo(DataSystem.FindActor(enemyData.Id-1000));
            }
        }

        public void InitParamInfos(EnemyData enemyData)
        {
            var statusInfo = new StatusInfo();
            int plusHpParam = _bossFlag == true ? 50 : 0;
            statusInfo.SetParameter(
                (int)(enemyData.BaseStatus.Hp + (plusHpParam + Level.Value * enemyData.HpGrowth * 0.01f)),
                Math.Min(50, (int)(enemyData.BaseStatus.Mp + (Level.Value * enemyData.MpGrowth * 0.01f))),
                (int)(enemyData.BaseStatus.Atk + (Level.Value * enemyData.AtkGrowth * 0.01f)),
                (int)(enemyData.BaseStatus.Def + (Level.Value * enemyData.DefGrowth * 0.01f)),
                Math.Min(100, (int)(enemyData.BaseStatus.Spd + (Level.Value * enemyData.SpdGrowth * 0.01f)))
            );
            _demigodParam = Level.Value / 2;
            _status = statusInfo;
            Hp.SetValue(_status.Hp);
            Mp.SetValue(_status.Mp);

            _skills.Clear();
            var enhanceSkills = new List<SkillInfo>();
            for (int i = 0;i < enemyData.LearningSkills.Count;i++)
            {
                if (Level.Value >= enemyData.LearningSkills[i].Level)
                {
                    var skillInfo = new SkillInfo(enemyData.LearningSkills[i].SkillId);
                    if (skillInfo.IsEnhanceSkill())
                    {
                        enhanceSkills.Add(skillInfo);
                    } else
                    {
                        skillInfo.SetTriggerDates(enemyData.LearningSkills[i].TriggerDates);
                        skillInfo.SetWeight(enemyData.LearningSkills[i].Weight);
                        _skills.Add(skillInfo);
                    }
                }
            }
            InitKindTypes(enemyData.Kinds);
            foreach (var enhanceSkill in enhanceSkills)
            {
                var result = new ActionResultInfo(this,this,enhanceSkill.FeatureDates,enhanceSkill.Id.Value);
            }
            _enhanceSkills = enhanceSkills;
            _skills.Sort((a,b) => a.Weight > b.Weight ? -1:1);
            _skillTriggerInfos.Clear();
            foreach (var skillInfo in _skills)
            {
                var skillTriggerData = DataSystem.Enemies.Find(a => a.Id == enemyData.Id).SkillTriggerDates.Find(a => a.SkillId == skillInfo.Id.Value);
                if (skillTriggerData == null)
                {
                    continue;
                }
                var skillTriggerInfo = new SkillTriggerInfo(enemyData.Id,skillInfo);
                var SkillTriggerData1 = DataSystem.SkillTriggers.Find(a => a.Id == skillTriggerData.Trigger1);
                var SkillTriggerData2 = DataSystem.SkillTriggers.Find(a => a.Id == skillTriggerData.Trigger2);
                skillTriggerInfo.UpdateTriggerDates(new List<SkillTriggerData>(){SkillTriggerData1,SkillTriggerData2});
                _skillTriggerInfos.Add(skillTriggerInfo);
            }
            _skillTriggerInfos = _skillTriggerInfos.FindAll(a => _skills.Find(b => b.Id.Value == a.SkillId) != null);
            //_skillTriggerInfos.Sort((a,b) => a.Priority - b.Priority > 0 ? -1 : 1);
            InitSkillCount();
            ResetAp(true);
        }

        private void InitKindTypes(List<KindType> kindTypes)
        {
            SetKindTypes(kindTypes);
            AddKindPassive();
        }

        private void InitSkillCount()
        {
            foreach (var skill in _skills)
            {
                skill.UseCount.SetValue(0);
                skill.CountTurn.SetValue(0);
            }            
        }

        private void SetKindTypes(List<KindType> kindTypes)
        {
            _kinds.Clear();
            foreach (var kind in kindTypes)
            {
                _kinds.Add(kind);
            }
        }

        public void SetWeakPoint(KindType weakPoint)
        {
            if (!_weakPoints.Contains(weakPoint))
            {
                _weakPoints.Add(weakPoint);
            }
        }

        public BattlerInfo(List<SkillInfo> skillInfos,bool isActor,int index)
        {
            CharaId.SetValue(index + 1000);
            var statusInfo = new StatusInfo();
            statusInfo.SetParameter(
                1,
                0,
                0,
                0,
                0
            );
            _status = statusInfo;
            Hp.SetValue(1);
            Index.SetValue(index + 1000);
            _isActor = isActor;
            _isAlcana = true;
            _skills = skillInfos;
            foreach (var skillInfo in skillInfos)
            {
                var skillTrigger = new SkillTriggerInfo(Index.Value,skillInfo);
                _skillTriggerInfos.Add(skillTrigger);
            }
            InitSkillCount();
            ResetAp(true);
        }

        public void ResetData(int level)
        {
            Level.SetValue(level);
            _stateInfos.Clear();
            GainHp(_status.Hp);
            //GainMp(_status.Mp);
            _isAwaken = false;
            _preserveAlive = false;
            ChainSuccessCount.SetValue(0);
            PayBattleMp.SetValue(0);
            AttackedCount.SetValue(0);
            HealCount.SetValue(0);
            BeCriticalCount.SetValue(0);
            DodgeCount.SetValue(0);
            DamagedValue.SetValue(0);
            TurnCount.SetValue(0);
            ResetAp(true);
            _passiveSkillIds = new ();
        }

        private void AddKindPassive()
        {
            /*
            var kindSkills = new List<SkillInfo>();
            foreach (var kind in _kinds)
            {
                if (kind > 0)
                {
                    var skillData = DataSystem.Skills.ContainsKey((int)kind * 10 + 10000);
                    if (skillData)
                    {
                        var skillInfo = new SkillInfo((int)kind * 10 + 10000);
                        if (kindSkills.Find(a => a.Id == skillInfo.Id) == null)
                        {
                            kindSkills.Add(skillInfo);
                            _skills.Add(skillInfo);
                        }
                    }
                }
            }
            foreach (var skillInfo in kindSkills)
            {
                var triggerId = _isActor ? _index : EnemyData.Id;
                var skillTriggerInfo = new SkillTriggerInfo(triggerId,skillInfo);
                var SkillTriggerData1 = DataSystem.SkillTriggers.Find(a => a.Id == 0);
                var SkillTriggerData2 = DataSystem.SkillTriggers.Find(a => a.Id == 0);
                skillTriggerInfo.UpdateTriggerDates(new List<SkillTriggerData>(){SkillTriggerData1,SkillTriggerData2});
                _skillTriggerInfos.Add(skillTriggerInfo);
            }
            */
        }

        public void ResetAp(bool IsBattleStart)
        {
            int rand = 0;
            if (IsBattleStart == true)
            {
                //rand = new Random().Next(-50, 50);
            }
            var speed = CurrentSpd(false);
            var baseSpeed = new List<int>{50,75,100,150};
            Ap.SetValue(1000 + rand);
            if (_preserveMinusAp > 0)
            {
                Ap.GainValue(_preserveMinusAp);
                _preserveMinusAp = 0;
            }
            var speedCount = -1;
            for (var i = 0;i < baseSpeed.Count;i++)
            {
                if (speed > baseSpeed[i])
                {
                    speedCount = i;
                }
            }
            if (speedCount > -1)
            {
                for (var i = 0;i <= speedCount;i++)
                {
                    if (i == 0)
                    {
                        Ap.GainValue(baseSpeed[i] * (8/(i+1)));
                    } else
                    {
                        Ap.GainValue((baseSpeed[i] - baseSpeed[i-1]) * (8/(i+1)));
                    }
                }
                var over = speed - baseSpeed[speedCount];
                if (over > 0)
                {
                    Ap.GainValue(over * (8/(speedCount+2)));
                }
            } else
            {
                Ap.GainValue(speed * 8);
            }
            Ap.SetValue(Math.Max(Ap.Value,200));
        }

        public int ResetApFrame()
        {
            var speed = CurrentSpd(false);
            var baseSpeed = new List<int>{50,75,100,150};
            var ap = 1000;
            var speedCount = -1;
            for (var i = 0;i < baseSpeed.Count;i++)
            {
                if (speed > baseSpeed[i])
                {
                    speedCount = i;
                }
            }
            if (speedCount > -1)
            {
                for (var i = 0;i <= speedCount;i++)
                {
                    if (i == 0)
                    {
                        ap -= baseSpeed[i] * (8/(i+1));
                    } else
                    {
                        ap -= (baseSpeed[i] - baseSpeed[i-1]) * (8/(i+1));
                    }
                }
                var over = speed - baseSpeed[speedCount];
                if (over > 0)
                {
                    ap -= over * (8/(speedCount+2));
                }
            } else
            {
                ap -= speed * 8;
            }
            return Math.Max(ap,200);
        }

        public void UpdateAp()
        {
            Ap.GainValue(UpdateApValue());
        }

        public int UpdateApValue()
        {
            if (IsState(StateType.Death) || IsState(StateType.Stun) || IsState(StateType.Wait))
            {
                return 0;
            }
            return -4;
        }

        public float WaitFrame(int turn)
        {
            var wait = Ap.Value;
            if (turn > 0)
            {
                wait += ResetApFrame() * turn;
            }
            if (IsState(StateType.Stun))
            {
                wait += StateTurn(StateType.Stun);
            }/* else
            if (IsState(StateType.Chain))
            {
                wait += StateTurn(StateType.Chain) * 6;
            }*/ /* else
            if (IsState(StateType.Benediction))
            {
                wait += StateTurn(StateType.Benediction);
            } */ /*else
            if (IsState(StateType.RevengeAct))
            {
                wait += StateTurn(StateType.RevengeAct) * 2;
            } */ /*else
            if (IsState(StateType.Heist) && IsState(StateType.Slow))
            {
            } */ /*else
            if (IsState(StateType.Heist))
            {
                wait -= StateTurn(StateType.Heist) * 1.5f;
            } */ /*else
            if (IsState(StateType.Slow))
            {
                wait += StateTurn(StateType.Slow) * 2;
            }
            */
            return wait;
        }

        public void ChangeAp(float value)
        {
            Ap.GainValue(value);
            if (Ap.Value < 0)
            {
                _preserveMinusAp = Ap.Value * -1;
            }
        }

        public void SetAp(int value)
        {
            Ap.SetValue(value);
            Ap.GainValue(0,0);
        }

        public int LastTargetIndex()
        {
            if (IsActor)
            {
                return _lastTargetIndex;
            }
            return -1;
        }
        
        public void SetHp(int value)
        {
            Hp.SetValue(value);
        }

        public void GainHp(int value)
        {
            Hp.GainValue(value,0,MaxHp);
            if (Hp.Value <= 0)
            {
                for (var i = _stateInfos.Count-1;i >= 0;i--)
                {
                    if (_stateInfos[i].Master.RemoveByDeath)
                    {
                        if (_stateInfos[i].IsStartPassive() == false)
                        {
                            RemoveState(_stateInfos[i],true);
                        }
                    }
                }
                var stateInfo = new StateInfo(StateType.Death,0,0,Index.Value,Index.Value,-1);
                AddState(stateInfo,true);
            }
        }

        public void GainMp(int value)
        {
            Mp.GainValue(value,0,MaxMp);
        }

        public void InitCountTurn(int skillId)
        {
            var skill = _skills.Find(a => a.Id.Value == skillId);
            skill?.InitCountTurn();
        }
        
        public void SeekCountTurn(int seekCount,int skillId = -1)
        {
            // その魔法のCtのみを回復
            if (skillId > -1)
            {
                var find = _skills.Find(a => a.Id.Value == skillId);
                find?.CountTurn.GainValue(seekCount);
                return;
            }
            foreach (var skill in _skills)
            {
                skill.CountTurn.GainValue(seekCount);
            }
        }

        public void GainUseCount(int skillId)
        {
            var skill = _skills.Find(a => a.Id.Value == skillId);
            skill?.UseCount.GainValue(1);
        }

        public bool IsAlive()
        {
            return Hp.Value > 0;
        }

        public bool CanMove()
        {
            if (IsState(StateType.Death))
            {
                return false;
            }
            if (IsState(StateType.Stun))
            {
                return false;
            }
            /*
            if (IsState(StateType.Chain))
            {
                return false;
            }
            */
            if (IsState(StateType.Wait))
            {
                return false;
            }
            return true;
        }

        public bool IsState(StateType stateType)
        {
            return _stateInfos.Find(a => a.StateType == stateType) != null;
        }

        public StateInfo GetStateInfo(StateType stateType)
        {
            return _stateInfos.Find(a => a.StateType == stateType);
        }

        public List<StateInfo> GetStateInfoAll(StateType stateType)
        {
            return _stateInfos.FindAll(a => a.StateType == stateType);
        }

        public int GetStateEffectAll(StateType stateType)
        {
            var effect = 0;
            foreach (var stateInfo in GetStateInfoAll(stateType))
            {
                effect += stateInfo.Effect.Value;
            }
            return effect;
        }

        // ステートを消す
        public void EraseStateInfo(StateType stateType)
        {
            var getStateInfoAll = GetStateInfoAll(stateType);

            for (int i = getStateInfoAll.Count-1;i >= 0;i--)
            {
                RemoveState(getStateInfoAll[i],true);
            }
        }

        public int StateTurn(StateType stateType)
        {
            int turns = 0;
            if (IsState(stateType))
            {
                turns += _stateInfos.Find(a => a.StateType == stateType).Turns.Value;
            }
            return turns;
        }

        public int StateEffect(StateType stateType)
        {
            int effect = 0;
            if (IsState(stateType))
            {
                effect += _stateInfos.Find(a => a.StateType == stateType).Effect.Value;
            }
            return effect;
        }

        public int StateEffectAll(StateType stateType)
        {
            int effect = 0;
            if (IsState(stateType))
            {
                var stateInfos = GetStateInfoAll(stateType);
                effect = stateInfos.Sum(a => a.Effect.Value);
            }
            return effect;
        }

        public bool AddState(StateInfo stateInfo,bool doAdd)
        {
            bool IsAdded = false;
            if (IsState(StateType.Barrier))
            {
                if (stateInfo.Master.Abnormal)
                {
                    return false;
                }
            }
            if (IsState(StateType.Undead))
            {
                if (stateInfo.Master.StateType == StateType.Regenerate)
                {
                    return false;
                }
            }
            var overLapCount = GetStateInfoAll(stateInfo.StateType).Count;
            if (_stateInfos.Find(a => a.CheckOverWriteState(stateInfo,overLapCount) == true) == null)
            {
                if (doAdd)
                {
                    _stateInfos.Add(stateInfo);
                    if (stateInfo.Master.StateType == StateType.MaxHpUp)
                    {
                        GainHp(stateInfo.Effect.Value);
                    }
                    if (stateInfo.Master.StateType == StateType.MaxMpUp)
                    {
                        //GainMp(stateInfo.Effect);
                    }
                }
                IsAdded = true;
            }
            return IsAdded;
        }

        public bool RemoveState(StateInfo stateInfo,bool doRemove)
        {
            bool IsRemoved = false;
            int RemoveIndex = _stateInfos.FindIndex(a => a.StateType == stateInfo.StateType && (a.SkillId == stateInfo.SkillId || stateInfo.SkillId.Value == -1));
            if (RemoveIndex > -1)
            {
                if (doRemove)
                {
                    if (stateInfo.SkillId.Value == -1)
                    {
                        // 効果による解除は全て複数効果あっても全部解除する
                        for (int i = _stateInfos.Count-1;0 <= i;i--)
                        {
                            if (_stateInfos[i].StateType == (StateType)stateInfo.Master.StateType)
                            {
                                _stateInfos.Remove(_stateInfos[i]);
                            }
                        }
                    } else
                    {
                        _stateInfos.RemoveAt(RemoveIndex);
                    }
                    if (stateInfo.StateType == StateType.Death)
                    {
                        _preserveAlive = true;
                        //if (_hp == 0)_hp = 1;
                    }
                }
                IsRemoved = true;
            }
            return IsRemoved;
        }

        public List<StateInfo> UpdateState(RemovalTiming removalTiming)
        {
            var stateInfos = new List<StateInfo>();
            for (var i = _stateInfos.Count-1;i >= 0;i--)
            {
                var stateInfo = _stateInfos[i];
                if (stateInfo.RemovalTiming == removalTiming)
                {
                    bool IsRemove = stateInfo.UpdateTurn();
                    if (IsRemove)
                    {
                        RemoveState(stateInfo,true);
                        stateInfos.Add(stateInfo);
                    }
                }
            }
            return stateInfos;
        }

        public void UpdateStateTurn(RemovalTiming removalTiming,int stateId)
        {
            for (var i = _stateInfos.Count-1;i >= 0;i--)
            {
                var stateInfo = _stateInfos[i];
                if (stateInfo.RemovalTiming == removalTiming && stateInfo.StateType == (StateType)stateId)
                {
                    bool IsRemove = stateInfo.UpdateTurn();
                    if (IsRemove)
                    {
                        RemoveState(stateInfo,true);
                    }
                }
            }
        }

        public void UpdateStateCount(RemovalTiming removalTiming,StateInfo stateInfo)
        {
            if (stateInfo.RemovalTiming == removalTiming)
            {
                bool IsRemove = stateInfo.UpdateTurn();
                if (IsRemove)
                {
                    RemoveState(stateInfo,true);
                }
            }
        }

        public float MpCostRate()
        {
            var mpCost = 1f;
            foreach (var mpCostStateInfo in GetStateInfoAll(StateType.MpCostRate))
            {
                mpCost += mpCostStateInfo.Effect.Value * -0.01f;
            }
            return mpCost;
        }

        public int CalcMpCost(int mpCost)
        {
            return (int)Math.Ceiling(mpCost * MpCostRate());
        }

        /// <summary>
        /// 攻撃力
        /// </summary>
        /// <param name="isNoEffect">バフ込みか</param>
        /// <returns></returns>
        public int CurrentAtk(bool isNoEffect = false)
        {
            int atk = Status.Atk;
            if (isNoEffect == false)
            {
                if (IsState(StateType.Demigod))
                {
                    atk += _demigodParam;
                }
                if (IsState(StateType.StatusUp))
                {
                    atk += StateEffectAll(StateType.StatusUp);
                }
                if (IsState(StateType.AtkUp))
                {
                    atk += StateEffectAll(StateType.AtkUp);
                }
                if (IsState(StateType.AtkUpOver))
                {
                    atk += StateEffectAll(StateType.AtkUpOver);
                }
                if (IsState(StateType.AtkDown))
                {
                    atk -= (int)DeBuffUpperParam(StateEffectAll(StateType.AtkDown));
                }
                if (IsState(StateType.AtkDownPer))
                {
                    atk = (int)(atk * ((100 - DeBuffUpperParam(StateEffectAll(StateType.AtkDownPer))) * 0.01f));
                }
            }
            return atk;
        }
        
        public int CurrentDef(bool isNoEffect = false)
        {
            int def = Status.Def;
            if (isNoEffect == false)
            {
                if (IsState(StateType.Demigod))
                {
                    def += _demigodParam;
                }
                if (IsState(StateType.StatusUp))
                {
                    def += StateEffectAll(StateType.StatusUp);
                }
                if (IsState(StateType.DefUp))
                {
                    def += StateEffectAll(StateType.DefUp);
                }
                if (IsState(StateType.DefDown))
                {
                    def -= (int)DeBuffUpperParam(StateEffectAll(StateType.DefDown));
                }
                if (IsState(StateType.DefPerDown))
                {
                    def = (int)(def * ((100 - DeBuffUpperParam(StateEffectAll(StateType.DefPerDown))) * 0.01f));
                }
            }
            return def;
        }
        
        public int CurrentSpd(bool isNoEffect = false)
        {
            int spd = Status.Spd;
            if (isNoEffect == false)
            {
                if (IsState(StateType.Demigod))
                {
                    spd += _demigodParam;
                }
                if (IsState(StateType.SpdUp))
                {
                    spd += StateEffectAll(StateType.SpdUp);
                }
                if (IsState(StateType.Accel))
                {
                    spd += StateEffect(StateType.Accel) * StateTurn(StateType.Accel);
                }
                if (IsState(StateType.StatusUp))
                {
                    spd += StateEffectAll(StateType.StatusUp);
                }
            }
            return spd;
        }

        public int CurrentHit()
        {
            int hit = 0;
            hit += StateEffectAll(StateType.HitUp) + StateEffectAll(StateType.HitUpOver);
            hit -= (int)DeBuffUpperParam(StateEffectAll(StateType.HitDown));
            return hit;
        }

        public int CurrentEva()
        {
            int eva = 0;
            eva += StateEffectAll(StateType.EvaUp) + StateEffectAll(StateType.EvaUpOver);
            eva -= (int)DeBuffUpperParam(StateEffectAll(StateType.EvaDown));
            return eva;
        }

        public float CurrentDamageRate(bool isNoEffect = false)
        {
            float DamageRate = 1;
            if (isNoEffect == false)
            {
                DamageRate += StateEffectAll(StateType.DamageUp) * 0.01f;
            }
            return DamageRate;
        }

        private float DeBuffUpperParam(int param)
        {
            return param * (1f + StateEffectAll(StateType.DeBuffUpper) * 0.01f);
        }

        public int TargetRate()
        {
            int rate = 100;
            if (IsState(StateType.Shadow))
            {
                rate = 0;
            }
            if (LineIndex == LineType.Front)
            {
                rate *= 2;
            }
            rate = Math.Max(0,rate);
            return rate;
        }

        public void SetAwaken()
        {
            _isAwaken = true;
        }

        public void GainMaxDamage(int value)
        {
            if (value > MaxDamage.Value)
            {
                MaxDamage.SetValue(value);
            }
        }

        public void TurnEnd()
        {
            TurnCount.GainValue(1);
            // アクセル
            if (IsState(StateType.Accel))
            {
                var stateInfo = GetStateInfo(StateType.Accel);
                stateInfo.Turns.SetValue(stateInfo.Turns.Value + 1);
            }
        }

        public void TurnEndSkillSeekCountTurn(List<SkillInfo> skillInfos)
        {
            // 使用しなかった魔法のカウントを進める
            foreach (var skillInfo in _skills)
            {
                if (skillInfos.Find(a => a.Id == skillInfo.Id) == null)
                {
                    skillInfo.CountTurn.GainValue(-1);
                }
            }
        }

        public List<SkillInfo> ActiveSkills()
        {
            return _skills.FindAll(a => a.IsBattleActiveSkill());
        }

        public List<SkillInfo> PassiveSkills()
        {
            return _skills.FindAll(a => a.IsBattlePassiveSkill());
        }

        public List<StateInfo> IconStateInfos()
        {
            var iconStates = new List<StateInfo>();
            foreach (var stateInfo in _stateInfos)
            {
                if (stateInfo.Master.IconPath != "" && stateInfo.Master.IconPath != "\"\"")
                {
                    iconStates.Add(stateInfo);
                }
            }
            return iconStates;
        }

        public void SetPreserveAlive(bool preserveAlive)
        {
            _preserveAlive = preserveAlive;
        }

        // バフ解除効果で解除するstateを取得
        public List<StateInfo> GetRemovalBuffStates()
        {
            return _stateInfos.FindAll(a => a.Master.Removal);
        }    
        
        public int Evaluate()
        {
            int statusValue = MaxHp * 6
            + MaxMp * 4
            + _status.Atk * 8
            + _status.Def * 8
            + _status.Spd * 8;
            float magicValue = 0;
            foreach (var skillInfo in _skills)
            {
                var rate = 1.0f;
                magicValue += (rate * 100);
            }
            int total = statusValue + (int)magicValue + _demigodParam * 10;
            return total;
        }

        public int SlipDamage()
        {
            var slipDamage = 0;
            slipDamage += GetStateEffectAll(StateType.BurnDamage);
            var perDamageValue = GetStateEffectAll(StateType.BurnDamagePer);
            slipDamage += (int)Math.Round(MaxHp * 0.01f * perDamageValue);
            return slipDamage;
        }

        public int RegenerateHpValue()
        {
            var regenerate = 0;
            regenerate += GetStateEffectAll(StateType.Regenerate);
            var perDamageValue = GetStateEffectAll(StateType.Undead);
            regenerate += (int)Math.Round(MaxHp * 0.01f * perDamageValue);
            return regenerate;
        }
    }
}