using System;
using System.Collections.Generic;

namespace Ryneus
{
    [Serializable]
    public class SaveOptionInfo
    {
        public float BgmVolume;
        public bool BgmMute;
        public float SeVolume;
        public bool SeMute;
        public bool ScreenMode;
        public int ScreenWidth;
        public int ScreenHeight;
        public int GraphicIndex;
        public Language Language;
        public bool EventTextSkipIndex;
        public bool EventSkipIndex;
        public bool CommandEndCheck;
        public bool BattleTurnSkip;
        public bool BattleAnimationSkip;
        public bool BattlePassiveAnimationSkip;
        public bool BattleCameraMove;
        public InputType InputType;
        public bool BattleAuto;
        public float BattleSpeed = 1f;
        public bool TutorialCheck;
        public SaveOptionInfo()
        {
            InitParameter();
        }

        public void InitParameter()
        {
            BgmVolume = 0.7f;
            BgmMute = false;
            SeVolume = 0.6f;
            SeMute = false;
            ScreenMode = false;
            ScreenWidth = 1280;
            ScreenHeight = 720;
            GraphicIndex = 2;
            Language = Language.English;
            EventSkipIndex = false;
            EventTextSkipIndex = false;
            CommandEndCheck = true;
            BattleTurnSkip = false;
            BattleAnimationSkip = false;
            BattlePassiveAnimationSkip = true;
            BattleCameraMove = true;
            InputType = InputType.All;
            BattleAuto = false;
            BattleSpeed = 1f;
            TutorialCheck = true;
        }

        public void UpdateSoundParameter(float bgmVolume, bool bgmMute, float seVolume, bool seMute)
        {
            BgmVolume = bgmVolume;
            BgmMute = bgmMute;
            SeVolume = seVolume;
            SeMute = seMute;
        }
    }

    public enum InputType
    {
        All = 0,
        MouseOnly = 1,
        KeyboardOnly = 2,
    }

    public enum ScreenSizeType
    {
        HD = 1280, // 1280,720を等倍
        FWXGA = 1366,
        HD_PLUS = 1600,
        Full_HD = 1920,
        WQHD = 2560,
        UHD = 3840,
    }

    public enum Language
    {
        English = 1,
        Japanese = 2,
    }
}