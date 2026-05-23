using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class SystemData : ScriptableObject
    {
        public List<CommandData> TacticsCommandData;
        public List<CommandData> StatusCommandData;
        public List<OptionCommand> OptionCommandData;
        public List<CommandData> TitleCommandData;
        public int InitCurrency;
        public int PartyMemberNum;
        public int HpHealValue;
        public int WeakPointRate;
        public int StartStageId;
        public int PeriodTurns;
        public int RecoveryCount;
        public int EquipSkillCount;
        public int ClassChangePlusSkill;
        public int CheatArtifactMinus;
        public List<TextData> SystemTextData;

        public List<InputData> InputDataList;

        [Serializable]
        public class CommandData
        {

            public int Id;
            public string Key;
            public string Name;
            public string Help;
        }

        [Serializable]
        public class OptionCommand
        {
            public int Id;
            public string Key;
            public string Name;
            public string Help;
            public int Category;
            public OptionButtonType ButtonType;
            public int ToggleText1;
            public int ToggleText2;
            public int ToggleText3;
            public bool ExistWindows;
            public bool ExistAndroid;
        }

        [Serializable]
        public class InputData
        {
            public string Key;
            public int KeyId;
            public string Name;
        }
    }

    public enum TacticsCommandType
    {
        None,
        Paradigm = 1,
        Status = 2,
        Train,
        Alchemy,
        Symbol,
    }

    public enum TitleCommandType
    {
        NewGame = 1,
        Continue,
    }


    public enum StatusCommandType
    {
        SkillActionList = 0,
        Strength = 1,
    }

    public enum ConfirmCommandType
    {
        Yes = 0,
        No = 1,
    }

    [Serializable]
    public class GetItemData
    {
        public GetItemType Type;
        public int Param1;
        public int Param2;
    }

    public enum GetItemType
    {
        None = 0,
        Currency = 2,
        Demigod = 3,
        ReBirth = 4,
        Exp = 5,
        SkillMastary = 6,
        AttributeUp = 7,
        AddActor = 11,
        LevelUp = 21,
        StatusUp = 22,
        Regeneration = 31,
        //LearnSkill = 41,
        SelectAddActor = 52,
        Skill = 100,
        SelectRelic = 101,
        SelectSkill = 102,
        Ending = 210,
        Item = 1010,
        Equipment = 1020,
        SelectEquipment = 1021,

        RankUp = 2010,
        Evaluate = 2020,
        AddReliefCommandCount = 2030,
        AddRecoveryCount = 2040,
        ClearStage = 3010,
        RandumItem = 4010,
        RandumMagic = 4020,
        BattleSocreCurrency = 5010,
    }
}