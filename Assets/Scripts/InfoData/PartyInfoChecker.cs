using UnityEngine;

namespace Ryneus
{
    public class PartyInfoChecker : SingletonMonoBehaviour<PartyInfoChecker>
    {
        [SerializeField] private PartyInfo partyInfo = null;
        public void UpdateInfo()
        {
            partyInfo = GameSystem.GameInfo.PartyInfo;
        }
    }
}
