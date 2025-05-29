using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ryneus.UnitInfoList;

namespace Ryneus
{
    public class UnitInfoListView : BaseView
    {
    }

    namespace UnitInfoList
    {
        public enum CommandType
        {
            None = 0,
            DecideUnit = 1,
            EndOpenAnimation = 2,
            DecideBattlerInfo = 3,
            SelectUnitInfo = 4,
            SelectMainBattler = 5,
            SelectSubBattler = 6,
            InputSelectUnitInfo = 7,
            CallStatus = 8,
        }
    }
}
