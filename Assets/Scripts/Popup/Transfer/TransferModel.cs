using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class TransferModel : BaseModel
    {
        public TransferModel()
        {
        }

        public List<ActorInfo> TransferActorInfos()
        {
            return PartyInfo.TransferActorInfos();
        }

        public int TransferGetItem(ActorInfo actorInfo)
        {
            actorInfo.Transfer.SetValue(true);
            PartyInfo.AddTransferActorInfos(actorInfo);
            return actorInfo.TransferGetItem();
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
                MakeGetItemInfo(GetItemType.Evaluate, actorInfo.TransferGetItem()),
                // Exp
                MakeGetItemInfo(GetItemType.Exp, actorInfo.ActorId.Value, actorInfo.TransferGetExp(PartyInfo.Chapter.Value)),
                // Nu
                MakeGetItemInfo(GetItemType.Currency, actorInfo.TransferGetCurrency(PartyInfo.Chapter.Value))
            };
            return list;
        }
    }
}