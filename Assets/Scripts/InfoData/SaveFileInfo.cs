using System;
using UnityEngine;

namespace Ryneus
{
	[Serializable]
    public class SaveFileInfo
    {
        public int SaveNo = 0;
        public string SaveTime;
        public long PlayTime;
        public int ActorId;
        public int StageNo;
        public int ClearCount;
    }
}
