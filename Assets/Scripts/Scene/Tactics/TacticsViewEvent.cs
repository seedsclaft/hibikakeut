using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    namespace Tactics
    {
        public enum CommandType
        {
            None = 0,
            CallTacticsCommand,
            CallStatus,
            OnClickSymbol,
            OnCancelSymbol,
            SelectSymbol,
            SymbolDetailInfo,
            PopupSkillInfo,
            SelectRecord,
            CancelSymbolRecord,
            CancelSelectSymbol,
            //SelectActorResource,
            DecideRecord,
            CallEnemyInfo,
            CallAddActorInfo,
            CallBattleMemberSelect,
            CallBattleMemberSelectEnd,
            Back,
            SelectSideMenu,
            StageHelp,
            CancelRecordList, // レコードリストを非表示にする
            ScorePrize,
            AlcanaCheck,
            SelectAlcanaList,
            HideAlcanaList,
            EndShopSelect,
            SelectCharaLayer,
        }
    }

}
