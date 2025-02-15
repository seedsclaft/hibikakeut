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
        public int ActorId = 0;
        public int StageNo;
        public int ClearCount = 0;
    }
}
