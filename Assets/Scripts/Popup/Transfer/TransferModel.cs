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
            return actorInfo.TransferGetItem();
        }
    }
}