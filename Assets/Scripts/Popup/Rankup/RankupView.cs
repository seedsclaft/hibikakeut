using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Ryneus
{
    public class RankupView : BaseView, IInputHandlerEvent
    {
        [SerializeField] private TextMeshProUGUI beforeRankText = null;
        [SerializeField] private TextMeshProUGUI afterRankText = null;
        [SerializeField] private ConfirmAnimation confirmAnimation = null;

        public override void Initialize()
        {
            base.Initialize();
            SetBaseAnimation(confirmAnimation);
            OpenAnimation();
        }

        public void OpenAnimation()
        {
            confirmAnimation.OpenAnimation(UiRoot.transform,null);
        }

        public void SetRankupInfo(RankupInfo rankupInfo)
        {
            beforeRankText?.SetText(rankupInfo.From.ToString());
            afterRankText?.SetText(rankupInfo.To.ToString());
        }

        public void InputHandler(List<InputKeyType> keyTypes, bool pressed)
        {
            if (keyTypes.Count > 0)
            {
                BackEvent?.Invoke();
            }
        }
    }

    public class RankupInfo
    {
        private int _from = 0;
        public int From => _from;
        private int _to = 0;
        public int To => _to;
        public RankupInfo(int from,int to)
        {
            _from = from;
            _to = to;
        }
    }
}