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
    }
}
