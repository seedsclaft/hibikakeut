using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ryneus
{
    public class BattleStateIcon : MonoBehaviour
    {
        [SerializeField] private Image icon = null;
        [SerializeField] private Image iconBack = null;
        
        public void SetStateImage(Sprite sprite)
        {
            icon.sprite = sprite;
        }
    }
}
