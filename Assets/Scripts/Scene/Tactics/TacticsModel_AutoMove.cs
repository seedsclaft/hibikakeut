using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public partial class TacticsModel : BaseModel
    {

        public void GetFindRoute(HexField hexField,HexField endHexUnit,bool serachUnit = false,bool throughSelfTeam = false)
        {
            var unitInfos = CurrentStage.AllFieldUnitInfos();
            if (throughSelfTeam)
            {
                // 障害物から味方ユニットを外す
                unitInfos = unitInfos.FindAll(a => !a.IsUnit || a.TeamId.Value == GetTurnTeam().TeamId.Value);
            }
            _hexRoute.SetUnitInfos(unitInfos);
            _hexRoute.FindRoute(MoveType.Normal,hexField,endHexUnit,serachUnit);
        }

        public List<HexField> GetHexReach(HexField hexField,int reachCost,bool serachUnit = false,bool throughSelfTeam = false)
        {
            var unitInfos = CurrentStage.AllFieldUnitInfos();
            if (throughSelfTeam)
            {
                // 障害物から味方ユニットを外す
                unitInfos = unitInfos.FindAll(a => !a.IsUnit || a.TeamId.Value != GetTurnTeam().TeamId.Value);
            }
            _hexRoute.SetUnitInfos(unitInfos);
            return _hexRoute.GetReachableArea(MoveType.Normal,hexField,reachCost,serachUnit);
        }

        /// <summary>
        /// 自動選択ユニットの移動先を決定
        /// </summary>
        public void DecideAutoMoveBattlerField()
        {
            var moveBattler = CurrentStage.OnFieldTurnUnitInfos()?.Find(a => a.Index.Value == SelectingHexUnitId.Value && a.IsUnit);
            if (moveBattler != null)
            {
                CurrentStage.GetTurnTeamInfo().AddMoveEndUnitId(moveBattler.Id.Value);
                // AIは単体の思考ルーチンに従う
                if (CurrentStage.TurnTeamId.Value != (int)TeamIdType.Home)
                {
                    switch (moveBattler.HexMoveType)
                    {
                        case UnitMoveType.MoveBasement:
                            // 相手の本拠地に向かう、先に敵がいたら戦闘
                            AutoMoveBasement(moveBattler);
                            return;
                        case UnitMoveType.MoveAttackNearest:
                            // 近くにいる敵に向かって移動し、射程に捉えれば攻撃してくる。
                            AutoMoveAttackNearest(moveBattler);
                            return;
                        case UnitMoveType.InMoveAttackOrWait:
                            // 射程内に攻撃可能な敵がいれば攻撃、いなければその場で待機。
                            AutoInMoveAttackOrWait(moveBattler);
                            return;
                        case UnitMoveType.InMoveAttackOrEscape:
                            // 射程内に攻撃可能な敵がいれば攻撃、いない場合は敵の射程に入っていれば射程外へ逃げる、入っていなければその場で待機。
                            AutoInMoveAttackOrEscape(moveBattler);
                            return;
                        case UnitMoveType.MoveRandom:
                            // ランダムに移動。
                            AutoMoveRandom(moveBattler);
                            return;
                        case UnitMoveType.InMoveAttackSeekRoute:
                            // 射程内に攻撃可能な敵がいれば攻撃、いなければ一定のルートを移動。
                            AutoInMoveAttackSeekRoute(moveBattler);
                            return;
                        case UnitMoveType.MovePoint:
                        case UnitMoveType.Retreat:
                            // 特定の目標に向かって移動。攻撃してこない。
                            AutoMovePoint(moveBattler);
                            return;
                        case UnitMoveType.InWaitAttackWait:
                        case UnitMoveType.None:
                            // 何もしない
                            return;
                    }
                }
            }
        }

        /// <summary>
        ///  相手の本拠地に向かう、先に敵がいたら戦闘
        /// </summary>
        private void AutoMoveBasement(HexUnitInfo moveBattler)
        {
            var basement = OnFieldInfos.Find(a => a.IsBasementUnit && a.TeamId.Value != CurrentStage.TurnTeamId.Value);
            if (basement != null)
            {
                var inMoveAreaTarget = InMoveAreaTarget(moveBattler,basement);
                if (inMoveAreaTarget)
                {
                    return;
                }
                OutMoveAreaTarget(moveBattler,basement);
            }
        }

        /// <summary>
        ///  近くにいる敵に向かって移動し、射程に捉えれば攻撃してくる。
        /// </summary>
        private void AutoMoveAttackNearest(HexUnitInfo moveBattler)
        {
            var opponents = CurrentStage.OpponentUnitInfos();
            if (opponents != null && opponents.Count > 0)
            {
                // 1番近い敵を捕捉
                HexUnitInfo opponent = SearchNearestTarget(moveBattler,opponents);
                // 移動先を確定
                var inMoveAreaTarget = InMoveAreaTarget(moveBattler,opponent);
                if (inMoveAreaTarget)
                {
                    return;
                }
                OutMoveAreaTarget(moveBattler,opponent);
            }
        }

        /// <summary>
        ///  射程内に攻撃可能な敵がいれば攻撃、いなければその場で待機。
        /// </summary>
        private void AutoInMoveAttackOrWait(HexUnitInfo moveBattler)
        {
            var opponents = CurrentStage.OpponentUnitInfos();
            if (opponents != null && opponents.Count > 0)
            {
                // 1番近い敵を捕捉
                HexUnitInfo opponent = SearchNearestTarget(moveBattler,opponents);
                // 移動先を確定
                var inMoveAreaTarget = InMoveAreaTarget(moveBattler,opponent);
                if (inMoveAreaTarget)
                {
                    return;
                }
            }
        }

        /// <summary>
        ///  射程内に攻撃可能な敵がいれば攻撃、いない場合は敵の射程に入っていれば射程外へ逃げる、入っていなければその場で待機。
        /// </summary>
        private void AutoInMoveAttackOrEscape(HexUnitInfo moveBattler)
        {
            var opponents = CurrentStage.OpponentUnitInfos();
            if (opponents != null && opponents.Count > 0)
            {
                // 1番近い敵を捕捉
                HexUnitInfo opponent = SearchNearestTarget(moveBattler,opponents);
                // 移動先を確定
                var inMoveAreaTarget = InMoveAreaTarget(moveBattler,opponent);
                if (inMoveAreaTarget)
                {
                    return;
                }
                // 敵の射程に入っていれば射程外へ逃げる
                var inAttackReach = false;
                var attacked = InMoveAreaTarget(opponent,moveBattler,true);
                if (attacked)
                {
                    inAttackReach = true;
                }
                if (inAttackReach)
                {
                    // opponentから遠くへ
                    var reachAreas = GetHexReach(moveBattler.HexField,moveBattler.UnitInfo.BattlerInfos[0].CurrentMov(),false);
                    HexField targetHex = reachAreas[0];
                    foreach (var reachArea in reachAreas)
                    {
                        GetFindRoute(opponent.HexField,targetHex,true);
                        var targetRoutePath = _hexRoute.Pathlist;
                        GetFindRoute(opponent.HexField,reachArea,true);
                        var routePath = _hexRoute.Pathlist;

                        if (routePath.Count > targetRoutePath.Count)
                        {
                            targetHex = reachArea;
                        }
                    }
                    CurrentStage.SetFieldPosition(targetHex);
                }
            }
        }

        private void AutoMoveRandom(HexUnitInfo moveBattler)
        {
            var reaches = GetHexReach(moveBattler.HexField,moveBattler.UnitInfo.BattlerInfos[0].CurrentMov());
            var pick = Random.Range(0,reaches.Count);
            CurrentStage.SetFieldPosition(reaches[pick]);
        }

        private void AutoInMoveAttackSeekRoute(HexUnitInfo moveBattler)
        {
            var opponents = CurrentStage.OpponentUnitInfos();
            if (opponents != null && opponents.Count > 0)
            {
                // 1番近い敵を捕捉
                HexUnitInfo opponent = SearchNearestTarget(moveBattler,opponents);
                // 移動先を確定
                var inMoveAreaTarget = InMoveAreaTarget(moveBattler,opponent);
                if (inMoveAreaTarget)
                {
                    return;
                }
            }
            // ルート目的地点に移動
            var moveParam = moveBattler.HexMoveParam;
            if (moveParam != null)
            {
                // FlagがFalseならParam1,2
                var targetHex = new HexField()
                {
                    X = moveParam.Flag ? moveParam.Param3 : moveParam.Param1,
                    Y = moveParam.Flag ? moveParam.Param4 : moveParam.Param2,
                };
                GetFindRoute(moveBattler.HexField,targetHex,true);
                var targetRoutePath = _hexRoute.Pathlist;
                targetRoutePath.Reverse();
                if (targetRoutePath.Count > 0)
                {
                    var mov = moveBattler.GetUnitMov();
                    var moveTarget = targetRoutePath.Count >= mov ? targetRoutePath[mov-1] : targetRoutePath[0];
                    if (targetHex.X == moveTarget.X && targetHex.Y == moveTarget.Y)
                    {
                        // 到着したらFlagを反転
                        moveBattler.FilpMoveParamFlag();
                    }
                    targetHex.X = moveTarget.X;
                    targetHex.Y = moveTarget.Y;
                    CurrentStage.SetFieldPosition(targetHex);
                }
            }
        }

        private void AutoMovePoint(HexUnitInfo moveBattler)
        {
            // ルート目的地点に移動
            var moveParam = moveBattler.HexMoveParam;
            if (moveParam != null)
            {
                var targetHex = new HexField()
                {
                    X = moveParam.Param1,
                    Y = moveParam.Param2,
                };
                GetFindRoute(moveBattler.HexField,targetHex,true);
                var targetRoutePath = _hexRoute.Pathlist;
                targetRoutePath.Reverse();
                if (targetRoutePath.Count > 0)
                {
                    var mov = moveBattler.GetUnitMov();
                    var moveTarget = targetRoutePath.Count >= mov ? targetRoutePath[mov-1] : targetRoutePath[0];
                    targetHex.X = moveTarget.X;
                    targetHex.Y = moveTarget.Y;
                    CurrentStage.SetFieldPosition(targetHex);
                }
            }
        }

        /// <summary>
        /// 移動圏内に対象がいた場合に移動経路を確定する
        /// </summary>
        /// <param name="moveBattler"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private bool InMoveAreaTarget(HexUnitInfo moveBattler,HexUnitInfo target,bool serachUnit = false)
        {
            var moveBattlerMax = moveBattler.GetUnitMov();
            // 移動圏内にいる場合
            var targetReaches = GetHexReach(target.HexField,0);
            var moveBattlerReaches = GetHexReach(moveBattler.HexField,moveBattlerMax,serachUnit);
            // 重なりを検知
            var findReach = targetReaches.Find(a => moveBattlerReaches.Find(b => a.X == b.X && a.Y == b.Y) != null);
            if (findReach != null)
            {
                CurrentStage.SetFieldPosition(findReach);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 移動圏外に対象がいた場合に移動経路を確定する
        /// </summary>
        /// <param name="moveBattler"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private void OutMoveAreaTarget(HexUnitInfo moveBattler,HexUnitInfo target)
        {
            var targetCost = 0;
            var moveBattlerCost = 0;
            var moveBattlerMax = moveBattler.GetUnitMov();
            var decide = false;
            var isTarget = true;
            // 移動圏外にいる場合
            while (!decide)
            {
                var targetReaches = GetHexReach(target.HexField,targetCost,true);
                var moveBattlerReaches = GetHexReach(moveBattler.HexField,moveBattlerCost,true);
                moveBattlerReaches.Reverse();
                // 重なりを検知
                var findReaches = targetReaches.FindAll(a => moveBattlerReaches.Find(b => a.X == b.X && a.Y == b.Y) != null);
                // 移動先が埋まっていなければ
                findReaches = findReaches.FindAll(a => CurrentStage.AllUnitInfos().Find(b => moveBattler != b && b.HexField.X == a.X && b.HexField.Y == b.HexField.Y) == null);
                if (findReaches.Count > 0)
                {
                    decide = true;
                    CurrentStage.SetFieldPosition(findReaches[0]);
                } else
                {
                    isTarget = !isTarget;
                    if (!isTarget && moveBattlerCost < moveBattlerMax)
                    {
                        moveBattlerCost++;
                    } else
                    {
                        targetCost++;
                    }
                }
            }
        }

        /// <summary>
        /// 一番近くにいる対象を取得
        /// </summary>
        /// <returns></returns>
        private HexUnitInfo SearchNearestTarget(HexUnitInfo moveBattler,List<HexUnitInfo> targets)
        {
            HexUnitInfo target = null;
            var moveBattlerReach = 0;
            // 1番近い敵を捕捉
            while (target == null)
            {
                var nearReaches = GetHexReach(moveBattler.HexField,moveBattlerReach,true);
                // 重なりを検知
                target = targets.Find(a => nearReaches.Find(b => a.HexField.X == b.X && a.HexField.Y == b.Y) != null);
                if (target == null)
                {
                    moveBattlerReach++;
                }
            }
            return target;
        }

        /// <summary>
        /// 移動後の行動を選択
        /// </summary>
        /// <returns></returns>
        public string DecideAutoMoveBattlerEnd()
        {
            var moveBattler = CurrentStage.FriendUnitInfos()?.Find(a => a.Index.Value == SelectingHexUnitId.Value && a.IsUnit);
            if (moveBattler != null)
            {
                // AIは単体の思考ルーチンに従う
                if (CurrentStage.TurnTeamId.Value != (int)TeamIdType.Home)
                {
                    switch (moveBattler.HexMoveType)
                    {
                        case UnitMoveType.MoveBasement:
                        case UnitMoveType.MoveAttackNearest:
                        case UnitMoveType.InMoveAttackOrWait:
                        case UnitMoveType.InMoveAttackOrEscape:
                        case UnitMoveType.InMoveAttackSeekRoute:
                        case UnitMoveType.InWaitAttackWait:
                            // 攻撃射程に敵がいたらバトル
                            return EndAutoAttack(moveBattler);
                        case UnitMoveType.MovePoint:
                        case UnitMoveType.Retreat:
                        case UnitMoveType.None:
                            // 何もしない
                            return UnitActEndCommand.Key;
                    }
                }
            }
            return UnitActEndCommand.Key;
        }

        /// <summary>
        ///  攻撃射程に敵がいたらバトル
        /// </summary>
        private string EndAutoAttack(HexUnitInfo moveBattler)
        {
            if (moveBattler != null)
            {
                // 攻撃範囲
                var opponents = CurrentStage.OpponentUnitInfos();
                var reach = 1;
                for (int i = 0;i < reach;i++)
                {
                    var reachPathes = GetHexReach(moveBattler.HexField,reach,true);
                    foreach (var reachPath in reachPathes)
                    {
                        var findBattler = opponents.Find(a => a.HexField.X == reachPath.X && a.HexField.Y == reachPath.Y);
                        if (findBattler != null)
                        {
                            return BattleCommand.Key;
                        }
                    }
                }
            }
            return UnitActEndCommand.Key;
        }
    }
}
