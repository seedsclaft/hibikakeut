using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class TransferModel : BaseModel
    {
        public TransferModel()
        {
        }

        public void TransferGetItem(ActorInfo actorInfo)
        {
            PartyInfo.EvaluationValue.GainValue(actorInfo.TransferGetItem(),0,100);
            actorInfo.Transfer.SetValue(true);
            PartyInfo.AddTransferActorInfos(actorInfo);
        }
    }
}