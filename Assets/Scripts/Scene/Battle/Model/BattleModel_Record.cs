using System;
using System.Collections.Generic;

namespace Ryneus
{
    public partial class BattleModel : BaseModel
    {
        private Dictionary<int, BattleRecord> _battleRecords = new();
        public Dictionary<int, BattleRecord> BattleRecords => _battleRecords;
        public BattleRecord GetBattleRecord(BattlerInfo battlerInfo)
        {
            if (battlerInfo.ActorInfo != null)
            {
                return _battleRecords[battlerInfo.ActorInfo.ActorId.Value];
            }
            return _battleRecords[battlerInfo.Index.Value + 1000];
        }
    }
}
