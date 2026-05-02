using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Ryneus
{
    public class BattleUtility
    {
        public static List<string> AnimationResourcePaths(List<BattlerInfo> battlerInfos)
        {
            var list = new List<string>();
            foreach (var battlerInfo in battlerInfos)
            {
                foreach (var skillInfo in battlerInfo.Skills)
                {
                    var skillData = skillInfo.Master;
                    var animationData = AnimationData(skillData.AnimationId);
                    if (animationData != null && !list.Contains(animationData.AnimationPath) && animationData.AnimationPath != "")
                    {
                        list.Add(animationData.AnimationPath);
                    }
                }
            }
            return list;
        }

        public static AnimationData AnimationData(int animationId)
        {
            return DataSystem.FindAnimation(animationId);
        }

        public static List<TriggerTiming> StartTriggerTimings()
        {
            return new List<TriggerTiming>(){
                TriggerTiming.StartBattle,
            };
        }

        public static List<TriggerTiming> BeforeTriggerTimings()
        {
            return new List<TriggerTiming>(){
            };
        }

        public static List<TriggerTiming> AfterTriggerTimings()
        {
            return new List<TriggerTiming>(){
                TriggerTiming.After,
            };
        }

        public static List<TriggerTiming> HpDamagedTriggerTimings()
        {
            return new List<TriggerTiming>()
            {
                TriggerTiming.HpDamaged,
            };
        }

        // 行動前パッシブ
        public static List<TriggerTiming> BeforeActionInfoTriggerTimings()
        {
            return new List<TriggerTiming>()
            {
                TriggerTiming.BeforeSelfUse,
                TriggerTiming.BeforeFriendUse,
                TriggerTiming.BeforeOpponentUse,
            };
        }

        public static bool IsInterruptTiming(TriggerTiming triggerTiming)
        {
            return triggerTiming == TriggerTiming.Interrupt || triggerTiming == TriggerTiming.BeforeSelfUse || triggerTiming == TriggerTiming.BeforeOpponentUse || triggerTiming == TriggerTiming.BeforeFriendUse || triggerTiming == TriggerTiming.PrimaryInterrupt;
        }

        // 対象決定後パッシブ
        public static List<TriggerTiming> AfterActionInfoTriggerTimings()
        {
            return new List<TriggerTiming>()
            {
                TriggerTiming.AfterOpponentUse,
            };
        }
        // 計算メソッドなど

        /// <summary>
        /// 作戦結果対象に複数候補がある場合に列に近い方のIndexを取得
        /// </summary>
        /// <param name="battlerInfo"></param>
        /// <param name="targetIndexList"></param>
        /// <returns></returns>
        public static int NearTargetIndex(BattlerInfo battlerInfo, List<int> targetIndexList, int targetBattlerIndex)
        {
            if (targetIndexList.Count == 0)
            {
                return -1;
            }
            if (targetIndexList.Contains(targetBattlerIndex))
            {
                return targetBattlerIndex;
            }
            // 複数候補は列が近い方を選ぶ
            var selfIndex = battlerInfo.Index.Value % 100;
            for (int i = 0; i < 3; i++)
            {
                var same = targetIndexList.FindIndex(a => a % 100 == (selfIndex + (i * -1)));
                if (same > -1)
                {
                    UnityEngine.Debug.Log(selfIndex);
                    UnityEngine.Debug.Log("same: " +same);
                    return targetIndexList[same];
                }
                if (i > 0)
                {
                    var reBound = targetIndexList.FindIndex(a => a % 100 == (selfIndex + i));
                    if (reBound > -1)
                    {
                        return targetIndexList[reBound];
                    }
                }
            }
            return targetIndexList[0];
        }

        public static int NearTargetIndex(BattlerInfo battlerInfo, List<BattlerInfo> targetBattlerInfos, int targetBattlerIndex)
        {
            if (targetBattlerInfos.Count == 1)
            {
                return targetBattlerInfos[0].Index.Value;
            }
            var targetIndexList = new List<int>();
            foreach (var targetBattlerInfo in targetBattlerInfos)
            {
                targetIndexList.Add(targetBattlerInfo.Index.Value);
            }
            if (targetBattlerIndex > -1 && targetBattlerInfos.Find(a => a.Index.Value == targetBattlerIndex) != null)
            {
                return targetBattlerIndex;
            }
            return NearTargetIndex(battlerInfo, targetIndexList, targetBattlerIndex);
        }

    }
}