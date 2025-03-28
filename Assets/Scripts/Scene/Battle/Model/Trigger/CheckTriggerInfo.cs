using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class CheckTriggerInfo 
    {
        public ActionInfo ActionInfo;
        public List<ActionResultInfo> ActionResultInfos;
        public BattlerInfo BattlerInfo;
        public List<BattlerInfo> Friends;
        public List<BattlerInfo> FriendFrontBattlerInfos;
        public List<BattlerInfo> FriendBackBattlerInfos;
        public List<BattlerInfo> Opponents;
        public List<BattlerInfo> OpponentsFrontBattlerInfos;
        public List<BattlerInfo> OpponentsBackBattlerInfos;
        public List<BattlerInfo> ReserveMembers;
        public int Turns;
        public CheckTriggerInfo(int turns,BattlerInfo battlerInfo,List<BattlerInfo> party,List<BattlerInfo> troops,List<BattlerInfo> reserveMembers,ActionInfo actionInfo = null,List<ActionResultInfo> actionResultInfos = null)
        {
            BattlerInfo = battlerInfo;
            Friends = battlerInfo.IsActor ? party : troops;
            Opponents = battlerInfo.IsActor ? troops : party;
            FriendFrontBattlerInfos = Friends.FindAll(a => a.LineIndex == LineType.Front);
            FriendBackBattlerInfos = Friends.FindAll(a => a.LineIndex == LineType.Back);
            OpponentsFrontBattlerInfos = Opponents.FindAll(a => a.LineIndex == LineType.Front);
            OpponentsBackBattlerInfos = Opponents.FindAll(a => a.LineIndex == LineType.Back);
            ActionInfo = actionInfo;
            ActionResultInfos = actionResultInfos;
            Turns = turns;
            ReserveMembers = reserveMembers;
        }

        public BattlerInfo GetBattlerInfo(int index)
        {
            var battlerInfo = Friends.Find(a => a.Index.Value == index);
            if (battlerInfo == null)
            battlerInfo = Opponents.Find(a => a.Index.Value == index);
            return battlerInfo;
        }

        public List<BattlerInfo> AliveBattlerInfos(bool IsFriend)
        {
            if (IsFriend)
            {
                return Friends.FindAll(a => a.IsAlive());
            } else
            {
                return Opponents.FindAll(a => a.IsAlive());
            }
        }

        public bool IsFriend(BattlerInfo targetBattler)
        {
            return BattlerInfo.IsActor == targetBattler.IsActor;
        }
    }
}
