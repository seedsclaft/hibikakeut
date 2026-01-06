using System;

namespace Ryneus
{
    [Serializable]
    public class SaveFileInfo
    {
        public int SaveNo = 0;
        public string SaveTime;
        public long SaveTimeLong;
        public long PlayTime;
        public int ActorId = 0;
        public int Chapter = 0;
        public int Period = 0;
        public int Rank = 0;
        public int StageNo;
        public int ClearCount = 0;
        public string State;

        public void UpdateTimeData(TempInfo tempInfo)
        {
            SaveTimeLong = DateTime.Now.ToFileTime();
            SaveTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            if (tempInfo != null)
            {
                PlayTime = tempInfo.PlayingTime.Value + tempInfo.NowEpochTime();
            }
        }

        public void UpdatePartyData(PartyInfo partyInfo)
        {
            if (partyInfo == null)
            {
                return;
            }
            Chapter = partyInfo.Chapter.Value;
            Period = partyInfo.Period.Value;
            Rank = partyInfo.MissionRank.Value;
        }
    }
}
