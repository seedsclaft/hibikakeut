﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Ryneus
{
    [Serializable]
    public class SkillInfo 
    {
        public SkillData Master => DataSystem.FindSkill(Id.Value);
        public ParameterInt Id = new();

        private bool _enable;
        public bool Enable => _enable;
        public void SetEnable(bool IsEnable)
        {
            _enable = IsEnable;
        }
        public AttributeType Attribute => Master.Attribute;

        private LearningState _learningState;
        public LearningState LearningState => _learningState;
        public void SetLearningState(LearningState learningState)
        {
            _learningState = learningState;
        }

        public ParameterInt LearningCost = new();
        public ParameterInt LearningLv = new();

        private List<SkillData.FeatureData> _featureDates = new();
        public List<SkillData.FeatureData> FeatureDates => _featureDates;

        private List<SkillData.TriggerData> _triggerDates = new();
        public List<SkillData.TriggerData> TriggerDates => _triggerDates;
        public void SetTriggerDates(List<SkillData.TriggerData> triggerDates)
        {
            _triggerDates = triggerDates;
        }

        private int _weight = 100;
        public int Weight => _weight;
        public void SetWeight(int weight)
        {
            _weight = weight;
        }

        public ParameterInt UseCount = new();

        private int _minusCountTurn = 0;
        public void SetMinusCountTurn(int countTurn)
        {
            _minusCountTurn = countTurn;
        }

        public void InitCountTurn()
        {
            var count = Mathf.Max(Master.CountTurn - _minusCountTurn);
            CountTurn.SetValue(count);
        }

        public ParameterInt CountTurn = new();

        public SkillInfo(int id)
        {
            Id.SetValue(id);
            _learningState = LearningState.None;
            if (Master != null && Master.FeatureDates != null)
            {
                var list = new List<SkillData.FeatureData>();
                foreach (var featureData in Master.FeatureDates)
                {
                    list.Add(featureData.CopyData());
                }
                _featureDates = list;
            }
        }
        

        public bool IsUnison()
        {
            return FeatureDates.Find(a => a.FeatureType == FeatureType.AddState && (StateType)a.Param1 == StateType.Wait) != null;
        }

        public int ActionAfterGainAp()
        {
            var gainAp = 0;
            foreach (var featureData in FeatureDates)
            {
                if (featureData.FeatureType == FeatureType.ActionAfterGainAp)
                {
                    gainAp += featureData.Param1;
                }
            }
            return gainAp;
        }

        public bool IsEnhanceSkill()
        {
            return Master.SkillType == SkillType.Enhance;
        }

        public bool IsBattleActiveSkill()
        {
            return Master.SkillType == SkillType.Active || Master.SkillType == SkillType.Awaken;
        }

        public bool IsBattlePassiveSkill()
        {
            return Master.SkillType == SkillType.Passive || Master.SkillType == SkillType.Unique;
        }

        public bool IsBattleSpecialSkill()
        {
            return Master.IsBattleSpecialSkill();
        }

        public string ConvertHelpText(BattlerInfo battlerInfo = null)
        {
            var help = Master.ConvertHelpText(Master.Help);
            var regex = new Regex(@"\[.+?\]");
            var splits = regex.Matches(help);
            if (splits.Count > 0)
            {
                foreach (var split in splits)
                {
                    var paramText = "";
                    var array = split.ToString().Substring(1,5).Split(",");
                    
                    var p1 = array[0];
                    var p2 = int.Parse(array[1]);
                    var p3 = int.Parse(array[2]);
                    if (p1 == "f")
                    {
                        var targetFeature = FeatureDates[p2];
                        
                        if (p3 == 1)
                        {
                            paramText = targetFeature.Param1.ToString();
                        } else
                        if (p3 == 2)
                        {
                            paramText = targetFeature.Param2.ToString();
                        } else
                        if (p3 == 3)
                        {
                            paramText = targetFeature.Param3.ToString();
                        }
                        Regex reg1 = new Regex("/f");
                        help = reg1.Replace(help,paramText,1);
                    }
                    help = help.Replace(split.ToString(),"");
                }
            }
            if (Master.Name.Contains("+"))
            {
                help = help.Replace("/n",Master.Name.Replace("+",""));
            }
            // ステート説明挿入
            var state = FeatureDates.Find(a => a.FeatureType == FeatureType.AddState);
            if (state != null)
            {
                var stateMaster = DataSystem.States.Find(a => (int)a.StateType == state.Param1);
                string effectText = stateMaster.Help;
                if (battlerInfo != null)
                {
                    var effect = battlerInfo.GetStateEffectAll(stateMaster.StateType);
                    effectText = effectText.Replace("\\d",effect.ToString());
                } else
                {
                    var effect = state.Param3;
                    effectText = effectText.Replace("\\d",effect.ToString());
                }
                help = help.Replace("/s","【" + stateMaster.Name + "】" + "\n" + effectText);
            }
            return help;
        }
    }
}