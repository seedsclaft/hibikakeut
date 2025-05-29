using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace Ryneus
{
    public class TacticsView : BaseView
    {

    }

    namespace Tactics
    {
        public enum CommandType
        {
            None = 0,
            CallTacticsCommand,
            CancellTacticsCommand,
            CallStatus,
            SelectHexUnit,
            CancelHexUnit,
            SymbolDetailInfo,
            PopupSkillInfo,
            DecideBattleMemberSelect,
            CancelBattleMemberSelect,
            CallEnemyInfo,
            CallAddActorInfo,
            Back,
            SelectSideMenu,
            StageHelp,
            ScorePrize,
            AlcanaCheck,
            SelectAlcanaList,
            HideAlcanaList,
            EndShopSelect,
            SelectCharaLayer,
            SelectHexMap,
            MoveHexMap,
            EndMoveBattler,
            EndLostBattler,
            EndHealUnits,
            EndAnimation
        }
    }
}