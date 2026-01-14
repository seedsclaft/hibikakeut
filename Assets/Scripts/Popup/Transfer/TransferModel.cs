using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class TransferModel : BaseModel
    {
        public TransferModel()
        {
        }

        public int TransferGetItem(ActorInfo actorInfo)
        {
            actorInfo.Transfer.SetValue(true);
            PartyInfo.AddTransferActorInfos(actorInfo);
            return actorInfo.TransferGetItem(DataSystem.System.PeriodTurns - PartyInfo.Period.Value);
        }

        public bool EnableTransfer(ActorInfo actorInfo)
        {
            if (PartyInfo.ActorInfos.Count > 2)
            {
                return PartyInfo.ActorInfos.FindIndex(a => a.ActorId.Value == actorInfo.ActorId.Value) >= 2;
            }
            return false;
        }

        public List<GetItemInfo> TransferGetItemInfos(ActorInfo actorInfo)
        {
            var list = new List<GetItemInfo>
            {
                // 信仰度
                MakeGetItemInfo(GetItemType.Evaluate, actorInfo.TransferGetItem(PartyInfo.Period.Value)),
                // Exp
                MakeGetItemInfo(GetItemType.Exp, actorInfo.ActorId.Value, actorInfo.TransferGetExp(PartyInfo.Chapter.Value, DataSystem.System.PeriodTurns - PartyInfo.Period.Value)),
                // Nu
                MakeGetItemInfo(GetItemType.Currency, actorInfo.TransferGetCurrency(PartyInfo.Chapter.Value, DataSystem.System.PeriodTurns - PartyInfo.Period.Value))
            };
            return list;
        }
    }
}