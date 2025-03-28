using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class StateDates : ScriptableObject
    {
        public List<StateData> Data = new();
    }

    [Serializable]
    public class StateData
    {   
        public StateType StateType;
        public string Name;
        public string Help;
        public string IconPath;
        public RemovalTiming RemovalTiming;
        public bool OverWrite;
        public string EffectPath;
        public EffectPositionType EffectPosition;
        public float EffectScale;
        public int OverLap;
        public bool Removal;
        public bool Abnormal;
        public bool Buff;
        public bool DeBuff;
        public bool CheckHit; // 命中回避判定をするか
        public bool RemoveByAttack;
        // 付与者が戦闘不能になった時に効果が切れるか
        public bool RemoveByDeath;
    }


    public enum StateType
    {
        None = 0,
        Death = 1,
        Wait = 10,
        Demigod = 1010,
        StatusUp = 1011,
        MaxHpUp = 1020,
        MaxMpUp = 1030,
        AtkUp = 1040,
        AtkDown = 1041,
        AtkDownPer = 1043,
        AtkUpOver = 1044,
        DefUp = 1050,
        DefDown = 1051,
        DefPerDown = 1052,
        SpdUp = 1060,
        CriticalRateUp = 1070,
        CriticalDamageRateUp = 1074,
        HitUp = 1080,
        HitUpOver = 1081,
        HitDown = 1082,
        EvaUp = 1090,
        EvaDown = 1091,
        EvaUpOver = 1092,
        DamageCutRate = 1100,
        DamageCut = 1101,
        BurnDamage = 2010, // 火傷(ダメージ固定)
        BurnDamagePer = 2011, // 火傷(ダメージ割合)
        Darkness = 2020, // 暗闇
        CounterAura = 2030, // CA
        CounterAuraDamage = 2031,
        CounterAuraShell = 2032,
        Regenerate = 2040,
        NoDamage = 2050,
        Drain  = 2060,
        AntiDote = 2070,
        Counter = 2080, // 実際の反撃行動はスキル習得で管理
        NoPassive = 2090,
        DeBuffUpper  = 2120,
        Substitute = 2130,
        Freeze = 2140,
        Stun = 2150,
        Barrier = 2180,
        Extension = 2190,
        AfterHeal = 2210,
        Deadly = 2220,
        HealActionSelfHeal = 2230,
        AbsoluteHit = 2240,
        AssistHeal = 2250,
        Undead = 2270,
        Accel = 2280,
        DamageUp = 2290,
        RemoveBuff = 2320,
        Penetrate = 2330,
        EffectLine = 2340,
        EffectAll = 2341,
        HolyCoffin = 2350,
        MpCostZeroAddDamage = 2360,
        MpCostZeroAddState = 2370,
        Shadow = 2380,
        Silence = 2390,
        NotHeal = 2400,
        DamageShield = 2410,
        Combo = 2420,
        Cover = 2430,
        MpCostRate = 2440,
        Reraise = 2450,
        HealValueUp = 2460,
        Curse = 2470,
        Linkage  = 3020,
        NoApRecover = 9999
    }

    public enum RemovalTiming
    {
        None = 0,
        UpdateTurn = 1,
        UpdateAp = 2,
        UpdateChain = 3,
        UpdateCount = 4,
        NextSelfTurn = 5
    }

    public enum EffectPositionType
    {
        Center = 0,
        Down = 1
    }
}