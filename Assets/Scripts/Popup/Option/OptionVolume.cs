using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Ryneus
{
    public class OptionVolume : MonoBehaviour
    {
        [SerializeField] private Slider volumeSlider = null;
        [SerializeField] private TextMeshProUGUI volumeValue = null;
        [SerializeField] private Button muteButton = null;
        [SerializeField] private List<Sprite> muteSprites = null;

        private float _sliderValue = 0;
        private bool _isMute = false;
        private Action<float, OptionInfo> _callEvent;
        private OptionInfo _optionInfo;

        public void Initialize(OptionInfo optionInfo)
        {
            _callEvent = optionInfo.SliderEvent;
            _optionInfo = optionInfo;
            volumeSlider.onValueChanged.AddListener(ValueChanged);
            muteButton.onClick.AddListener(() =>
            {
                _isMute = !_isMute;
                UpdateMute();
                optionInfo.MuteEvent(_isMute, _optionInfo);
            });
        }

        private void ValueChanged(float sliderValue)
        {
            _sliderValue = sliderValue;
            UpdateValue();
            _callEvent?.Invoke(sliderValue, _optionInfo);
        }

        private void UpdateValue()
        {
            volumeValue.text = ((int)Math.Round(_sliderValue * 100)).ToString("D");
        }

        private void UpdateMute()
        {
            muteButton.image.sprite = _isMute ? muteSprites[0] : muteSprites[1];
        }

        public void ChangeMute()
        {
            _isMute = !_isMute;
            UpdateMute();
        }

        public void UpdateValue(float volume, bool isMute)
        {
            _sliderValue = volume;
            _isMute = isMute;
            UpdateValue();
            UpdateMute();
            volumeSlider.value = volume;
        }
    }
}