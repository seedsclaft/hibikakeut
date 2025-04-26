using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public partial class TacticsModel : BaseModel
    {

        public List<HexField> GetHexReach(HexField hexField,int reachCost,bool serachUnit = false)
        {
            _hexRoute.SetUnitInfos(CurrentStage.AllFieldUnitInfos());
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
                HexUnitInfo opponent = null;
                var moveBattlerReach = 0;
                // 1番近い敵を捕捉
                while (opponent == null)
                {
                    var nearReaches = GetHexReach(moveBattler.HexField,moveBattlerReach,true);
                    // 重なりを検知
                    opponent = opponents.Find(a => nearReaches.Find(b => a.HexField.X == b.X && a.HexField.Y == b.Y) != null);
                    if (opponent == null)
                    {
                        moveBattlerReach++;
                    }
                }
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
                HexUnitInfo opponent = null;
                var moveBattlerReach = 0;
                // 1番近い敵を捕捉
                while (opponent == null)
                {
                    var nearReaches = GetHexReach(moveBattler.HexField,moveBattlerReach,true);
                    // 重なりを検知
                    opponent = opponents.Find(a => nearReaches.Find(b => a.HexField.X == b.X && a.HexField.Y == b.Y) != null);
                    if (opponent == null)
                    {
                        moveBattlerReach++;
                    }
                }
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
                HexUnitInfo opponent = null;
                var moveBattlerReach = 0;
                // 1番近い敵を捕捉
                while (opponent == null)
                {
                    var nearReaches = GetHexReach(moveBattler.HexField,moveBattlerReach,true);
                    // 重なりを検知
                    opponent = opponents.Find(a => nearReaches.Find(b => a.HexField.X == b.X && a.HexField.Y == b.Y) != null);
                    if (opponent == null)
                    {
                        moveBattlerReach++;
                    }
                }
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
                        var targetRoute = _hexRoute.FindRoute(MoveType.Normal,opponent.HexField,targetHex,true);
                        var targetRoutePath = _hexRoute.Pathlist;
                        var route = _hexRoute.FindRoute(MoveType.Normal,opponent.HexField,reachArea,true);
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

        /// <summary>
        /// 移動圏内に対象がいた場合に移動経路を確定する
        /// </summary>
        /// <param name="moveBattler"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private bool InMoveAreaTarget(HexUnitInfo moveBattler,HexUnitInfo target,bool serachUnit = false)
        {
            var moveBattlerMax = moveBattler.UnitInfo.BattlerInfos[0].CurrentMov();
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
            var moveBattlerMax = 2;
            var decide = false;
            var isTarget = true;
            // 移動圏外にいる場合
            while (!decide)
            {
                var targetReaches = GetHexReach(target.HexField,targetCost);
                var moveBattlerReaches = GetHexReach(moveBattler.HexField,moveBattlerCost);
                moveBattlerReaches.Reverse();
                // 重なりを検知
                var findReach = targetReaches.Find(a => moveBattlerReaches.Find(b => a.X == b.X && a.Y == b.Y) != null);
                if (findReach != null)
                {
                    decide = true;
                    CurrentStage.SetFieldPosition(findReach);
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
                            // 攻撃射程に敵がいたらバトル
                            return EndAutoAttack(moveBattler);
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
