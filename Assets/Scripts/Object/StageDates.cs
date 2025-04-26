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
        public AchieveType AchieveType;
        public string AchieveText;
        public int EnemyBasementId;
        public bool Selectable;
        public string Help;
        public int StageLv;
        public List<int> PartyMemberIds;
        public int Width;
        public int MinX;
        public int MaxX;
        public int Height;
        public int MinY;
        public int MaxY;
        public int InitX;
        public int InitY;
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
        public int Id;
        public int StageId;
        public int InitX;
        public int InitY;
        public HexUnitType UnitType;
        public TeamIdType InitTeamId;
        public int Rate;
        public int Param1;
        public int Param2;
        public int PrizeSetId;
        public int ClearCount;
        public UnitMoveType MoveType;
        public MoveTypeParam MoveTypeParam;

        public void ConvertSymbolGroupData(SymbolGroupData symbolGroupData)
        {
            //SymbolType = symbolGroupData.SymbolType;
            Param1 = symbolGroupData.Param1;
            Param2 = symbolGroupData.Param2;
            PrizeSetId = symbolGroupData.PrizeSetId;
            ClearCount = 0;
        }

        public void CopyData(StageSymbolData stageSymbolData)
        {
            CopyParamData(stageSymbolData);
            StageId = stageSymbolData.StageId;
            InitX = stageSymbolData.InitX;
            InitY = stageSymbolData.InitY;
        }

        public void CopyParamData(StageSymbolData stageSymbolData)
        {
            //SymbolType = stageSymbolData.SymbolType;
            Param1 = stageSymbolData.Param1;
            Param2 = stageSymbolData.Param2;
            PrizeSetId = stageSymbolData.PrizeSetId;
            ClearCount = stageSymbolData.ClearCount;
        }

        public bool IsGroupSymbol()
        {
            return false;//SymbolType > SymbolType.Group;
        }
    }

    [Serializable]
    public class SymbolGroupData
    {
        public int GroupId;
        //public SymbolType SymbolType;
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

    [Serializable]
    public class MoveTypeParam
    {
        public int SymbolId;
        public int Param1;
        public int Param2;
        public int Param3;
        public int Param4;
        public bool Flag = false; // 動作管理用
    }

    /*
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
    */

    public enum AchieveType
    {
        ConquerEnemyBasement = 1
    }

    public enum EventTiming
    {
        None = 0,
        BeforeTactics = 110,
        BattleVictory = 210,
        StartTutorial = 1010,
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
        TurnEndCommandEnable = 1010, // ターン終了コマンド操作
        TurnEndCommandDisable = 1011, // ターン終了コマンド操作
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

    [Serializable]
    public enum HexUnitType
    {
        None = 0, // 存在のないマス
        Basement = 20,
        Alcana = 30,
        GetItem = 40,
        SelectActor = 70,
        Battler = 1000,
        Reach = 2000,
        ReachAttack = 2010,
    }

    [Serializable]
    public enum UnitMoveType
    {
        None = 0, // 移動しない
        MoveBasement = 1, // 索敵攻撃。相手陣営の本拠地に向かう
        MoveAttackNearest = 10, // 近くにいる敵に向かって移動し、射程に捉えれば攻撃してくる。
        InMoveAttackOrWait = 20, // 射程内に攻撃可能な敵がいれば攻撃、いなければその場で待機。
        InMoveAttackOrEscape = 30, // 射程内に攻撃可能な敵がいれば攻撃、いない場合は敵の射程に入っていれば射程外へ逃げる、入っていなければその場で待機。
        MoveRandom = 40, // ランダムに移動。
        InMoveAttackSeekRoute = 50, // 射程内に攻撃可能な敵がいれば攻撃、いなければ一定のルートを移動。
        InWaitAttackWait = 60, // その場を動かない。その場から攻撃可能なら攻撃する。隣接など攻撃可能でないと挑発は出来ない。
        MovePoint = 70, // 特定の目標に向かって移動。攻撃してこない。
        Retreat = 80, // 離脱ポイントへ向かう。攻撃してこない。
    }

    [Serializable]
    public enum TeamIdType
    {
        None = 0, // 壁、移動範囲など
        Home = 1, // 味方
        Away = 2, // 敵
        Neutral = 3, // 中立
    }
}