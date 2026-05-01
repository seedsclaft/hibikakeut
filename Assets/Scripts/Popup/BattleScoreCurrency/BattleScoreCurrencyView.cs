using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Ryneus
{
    public class BattleScoreCurrencyView : BaseView, IInputHandlerEvent
    {
        [SerializeField] private TextMeshProUGUI battleScoreText = null;
        [SerializeField] private TextMeshProUGUI getCurrencyText = null;
        [SerializeField] private ConfirmAnimation confirmAnimation = null;

        private int _score = 0;
        public override void Initialize()
        {
            base.Initialize();
            SetBaseAnimation(confirmAnimation);
            OpenAnimation();
        }

        public void OpenAnimation()
        {
            confirmAnimation.OpenAnimation(UiRoot.transform, () =>
            {
                //AnimationUtility.CountUpText(battleScoreText, _score, 0);
            });
        }

        public void SetBattleScoreCurrencyInfo(BattleScoreCurrencyInfo rankupInfo)
        {
            _score = rankupInfo.Score.Value;
            UIComponent.SetText(battleScoreText, _score);
            UIComponent.SetText(getCurrencyText, rankupInfo.GetCurrency.Value + DataSystem.GetText(1000));
        }

        public void InputHandler(List<InputKeyType> keyTypes, bool pressed)
        {
            if (keyTypes.Count > 0)
            {
                BackEvent?.Invoke();
            }
        }
    }

    public class BattleScoreCurrencyInfo
    {
        public ParameterInt Score = new();
        public ParameterInt GetCurrency = new();
        public BattleScoreCurrencyInfo(int score, int currency)
        {
            Score.SetValue(score);
            GetCurrency.SetValue(currency);
        }
    }
}