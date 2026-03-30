using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class BattleFlowInfo
    {
        // 手動選択～選択終了まで
        // 行動選択中の者
        public BattlerInfo CurrentSelectBattler = null;
        // 選択中の行動情報
        public ActionInfo SelectActionInfo = null;
        // 選択中の対象
        public BattlerInfo SelectTargetBattler = null;

        // ターンの最初の行動開始者
        public BattlerInfo FirstActionBattler = null;
        // ターンの最初の行動
        public ActionInfo FirstActionInfo = null;

        // 1行動ルーチン中の情報
        public ActionInfo ActiveActionInfo = null;
        // 現アクションより前の割り込み
        public List<ActionInfo> InterruptActionInfos = new();
        public ActionInfo InterruptActionInfo = null;

        // 誘発した行動
        public List<ActionInfo> ReceiveActionInfos = new();
        public ActionInfo ReceiveActionInfo = null;

        public void AddActionInfo(ActionInfo actionInfo, bool IsInterrupt)
        {
            if (IsInterrupt)
            {
                //LogOutput.Log(actionInfo.Master.Id + "を割り込み");
                InterruptActionInfos.Add(actionInfo);
                InterruptActionInfo = InterruptActionInfos[0];
                return;
            }
            else
            {
                //LogOutput.Log(actionInfo.Master.Id + "を後に追加");
                ReceiveActionInfos.Add(actionInfo);
                ReceiveActionInfo = ReceiveActionInfos[0];
            }
        }
        
        public void PopActionInfo(ActionInfo actionInfo)
        {
            var findIndex = InterruptActionInfos.FindIndex(a => a == actionInfo);
            if (findIndex > -1)
            {
                InterruptActionInfos.RemoveAt(findIndex);
            }
            findIndex = ReceiveActionInfos.FindIndex(a => a == actionInfo);
            if (findIndex > -1)
            {
                ReceiveActionInfos.RemoveAt(findIndex);
            }
            if (actionInfo == FirstActionInfo)
            {
                FirstActionInfo = null;
            }
            InterruptActionInfo = InterruptActionInfos.Count > 0 ? InterruptActionInfos[0] : null;
            ReceiveActionInfo = ReceiveActionInfos.Count > 0 ? ReceiveActionInfos[0] : null;
        }

        public void ClearActionInfo()
        {
            ReceiveActionInfos.Clear();
            InterruptActionInfos.Clear();
        }
    }
}
