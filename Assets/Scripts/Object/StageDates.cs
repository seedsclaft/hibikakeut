using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class StageDates : ScriptableObject
    {
        public List<StageData> Data = new();
        public List<SymbolGroupData> SymbolGroupData = new();
    }

    [Serializable]
    public class StageData
    {   
        public int Id;
        public int StageNo;
        public string Name;
        public string AchieveText;
        public bool Selectable;
        public string Help;
        public int StageLv;
        public List<int> PartyMemberIds;
        public int RandomTroopWeight;
        public List<StageEnemyRate> RandomTroopEnemyRates;
        public string BackGround;
        public int BGMId;
        public int BossBGMId;
        public int MenuBGMId;
        public List<StageEventData> StageEvents;
        public List<StageSymbolData> StageSymbols;
    }



    [Serializable]
    public class StageEventData
    {
        public string EventKey;
        public int Turns;
        public EventTiming Timing;
        public StageEventType Type;
        public int Param;
        public bool ReadFlag;
    }

    [Serializable]
    public class StageSymbolData
    {
        public int StageId;
        public int Seek;
        public int SeekIndex;
        public SymbolType SymbolType;
        public int Rate;
        public int Param1;
        public int Param2;
        public int PrizeSetId;
        public int ClearCount;

        public void ConvertSymbolGroupData(SymbolGroupData symbolGroupData)
        {
            SymbolType = symbolGroupData.SymbolType;
            Param1 = symbolGroupData.Param1;
            Param2 = symbolGroupData.Param2;
            PrizeSetId = symbolGroupData.PrizeSetId;
            ClearCount = 0;
        }

        public void CopyData(StageSymbolData stageSymbolData)
        {
            CopyParamData(stageSymbolData);
            StageId = stageSymbolData.StageId;
            Seek = stageSymbolData.Seek;
            SeekIndex = stageSymbolData.SeekIndex;
        }

        public void CopyParamData(StageSymbolData stageSymbolData)
        {
            SymbolType = stageSymbolData.SymbolType;
            Param1 = stageSymbolData.Param1;
            Param2 = stageSymbolData.Param2;
            PrizeSetId = stageSymbolData.PrizeSetId;
            ClearCount = stageSymbolData.ClearCount;
        }

        public bool IsGroupSymbol()
        {
            return SymbolType > SymbolType.Group;
        }
    }

    [Serializable]
    public class SymbolGroupData
    {   
        public int GroupId;
        public SymbolType SymbolType;
        public int Rate;
        public int Param1;
        public int Param2;
        public int PrizeSetId;
    }

    [Serializable]
    public class StageEnemyRate
    {
        public int EnemyId;
        public int Weight;
    }

    public enum SymbolType
    {
        Random = -1,
        None = 0,
        Battle = 10,
        Boss = 11,
        Event = 20,
        Alcana = 30,
        Actor = 40,
        Resource = 50,
        SelectActor = 60,
        Shop = 70,
        Group = 99, // 99以上はグループ指定
    }

    public enum EventTiming
    {
        None = 0,
        BeforeTactics = 110,
        BattleVictory = 210,
    }

    public enum StageEventType
    {
        None = 0,
        CommandDisable = 1, // コマンドを制限する
        TutorialBattle = 2, // バトルをチュートリアルで固定する
        NeedAllTactics = 3, // 全員コマンドを選ばないと進まない
        IsSubordinate = 4, // 隷従属度フラグを管理
        IsAlcana = 5, // アルカナフラグを管理
        SelectAddActor = 6, // 仲間を選んで加入する
        SaveCommand = 7, // セーブを行う,
        SetDefineBossIndex = 8, // ボスの選択番号を設定する
        NeedUseSp = 9, // SPを消費しないと進まない
        SelectActorAdvStart = 12, // IDにActorIDを加算してADV再生
        RouteSelectEvent = 13, // ルート分岐イベント
        SetRouteSelectParam = 14, // ルート分岐パラメータを保存
        RouteSelectMoveEvent = 15, // ルート分岐ステージイベント
        ClearStage = 21, // ステージをクリアする
        ChangeRouteSelectStage = 31, // ルート分岐でステージに移動
        RouteSelectBattle = 32, // ルート分岐敵グループを生成
        SetDisplayTurns = 33, // 表示残りターンをマスターから取得
        RebornSkillEffect = 41, // 継承スキル演出再生
        MoveStage = 51, // ステージ移動
        SetDefineBoss = 61, // 中ボスを設定する
        SetLastBoss = 62, // 上位者ボスを設定する
        AdvStart = 100, // ADV再生
        ForceBattle = 110, // 今のステージシンボルの〇SeekIndexのバトルを開始
        SurvivalMode = 201, // サバイバルモードにする
    }

    public enum TutorialType
    {
        None = 0,
        TacticsCommandTrain = 1, // TacticsでTrain選択
        TacticsCommandAlchemy = 2, // TacticsでAlchemy選択
        TacticsCommandRecover = 3, // TacticsでRecover選択
        TacticsCommandBattle = 4, // TacticsでBattle選択
        TacticsCommandResource = 5, // TacticsでResource選択
        TacticsSelectTacticsActor = 11, // TacticsTrainでアクターを選択
        TacticsSelectTacticsDecide = 12, // TacticsTrainで決定を選択
        TacticsSelectEnemy = 21, // TacticsBattleで敵を選択
        TacticsSelectAlchemyMagic = 22, // TacticsAlchemyで魔法を選択
    }

    public enum EndingType
    {
        A,
        B,
        C,
    }

    public enum RankingType
    {
        None = 0,
        Evaluate = 1,
        Turns = 2
    }
}