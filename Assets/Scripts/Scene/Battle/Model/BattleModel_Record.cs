using System;
using System.Collections.Generic;

namespace Ryneus
{
    public partial class BattleModel : BaseModel
    {
        private Dictionary<int,BattleRecord> _battleRecords = new ();
        public Dictionary<int,BattleRecord> BattleRecords => _battleRecords;
    }
}
