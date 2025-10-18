using System;

namespace Ryneus
{
    [Serializable]
    public class PartyStatInfo
    {
        // 出撃回数
        public ParameterInt DepartureCount = new();
        public ParameterInt DepartureBattleFieldCount = new();
        // 勝利回数
        public ParameterInt BattleVictoryCount = new();
        // Nu消費レベルアップ回数
        public ParameterInt TacticsLvupCount = new();
        // バトル評価値
        public ParameterInt BattleScore = new();
        // 与ダメージ
        public ParameterInt TotalDamage = new();

        // 編成コマンド回数
        public ParameterInt DeckEditCommandCount = new();
        // 献上コマンド回数
        public ParameterInt PresentCommandCount = new();
        // 救済コマンド回数
        public ParameterInt ReliefCommandCount = new();
        // 転送コマンド回数
        public ParameterInt TransferCommandCount = new();
        // 解放コマンド回数
        public ParameterInt ReleaseCommandCount = new();
        // 取引コマンド回数
        public ParameterInt TradeCommandCount = new();
        // 魔法編成回数
        public ParameterInt StatusSkillChangeCount = new();

        // 覚醒スキル使用回数
        public ParameterInt UseAwakeSkillCount = new();
        // 交代スキル使用回数
        public ParameterInt UseChangeLineCount = new();

    }
}
