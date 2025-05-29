using System;
using System.Collections.Generic;

namespace Ryneus
{
    public partial class TacticsModel : BaseModel
    {
    }

    public class TacticsSceneInfo
    {
        // バトル直前に戻る
        public bool ReturnBeforeBattle;
        public bool ReturnNextBattle;
        // 消滅予定のユニット
        public List<HexUnitInfo> LostUnitInfos = new();
    }

    public class TacticsActorInfo
    {
        public ActorInfo ActorInfo;
        public List<ActorInfo> ActorInfos;
        public TacticsCommandType TacticsCommandType;
        public string DisableText;
    }

    public class TacticsCommandData
    {
        public string Title;
    }
}