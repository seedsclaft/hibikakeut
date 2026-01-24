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
            beforeRankText?.SetText(rankupInfo.From.Value.ToString());
            afterRankText?.SetText(rankupInfo.To.Value.ToString());
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
        public ParameterInt From = new();
        public ParameterInt To = new();
        public RankupInfo(int from, int to)
        {
            From.SetValue(from);
            To.SetValue(to);
        }
    }
}