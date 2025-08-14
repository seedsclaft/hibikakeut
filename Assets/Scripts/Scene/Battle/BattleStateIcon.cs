using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class BattleStateIcon : MonoBehaviour
    {
        [SerializeField] private StateInfoComponent stateInfoComponent = null;
        public void SetStateInfo(StateInfo stateInfo)
        {
            stateInfoComponent.UpdateInfo(stateInfo);
        }
    }
}
