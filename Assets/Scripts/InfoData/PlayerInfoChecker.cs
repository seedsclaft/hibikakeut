using UnityEngine;

namespace Ryneus
{
    public class PlayerInfoChecker : SingletonMonoBehaviour<PlayerInfoChecker>
    {
        [SerializeField] private PlayerInfo playerInfo = null;
        public void UpdateInfo()
        {
            playerInfo = GameSystem.CurrentData.PlayerInfo;
        }
    }
}
