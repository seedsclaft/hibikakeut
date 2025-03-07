using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ryneus
{
    public class ConfigUtility
    {
        public static void ApplyConfigData()
        {
            var saveConfigInfo = GameSystem.ConfigData;
            if (saveConfigInfo != null)
            {
                ChangeBGMValue(saveConfigInfo.BgmVolume);
                ChangeSEValue(saveConfigInfo.SeVolume);
                ChangeScreenMode(saveConfigInfo.ScreenMode);
                SetResolution(saveConfigInfo.ScreenWidth,saveConfigInfo.ScreenHeight);
                ChangeGraphicIndex(saveConfigInfo.GraphicIndex);
                ChangeEventSkipIndex(saveConfigInfo.EventSkipIndex);
                ChangeCommandEndCheck(saveConfigInfo.CommandEndCheck);
                ChangeBattleWait(saveConfigInfo.BattleWait);
                ChangeBattleAnimation(saveConfigInfo.BattleAnimationSkip);
                ChangeInputType(saveConfigInfo.InputType);
                ChangeBattleAuto(saveConfigInfo.BattleAuto);
                SetBattleSpeed(saveConfigInfo.BattleSpeed);
                ChangeTutorialCheck(saveConfigInfo.TutorialCheck);
            }
        }

        public static void ChangeScreenMode(bool isFullScreen)
        {
            GameSystem.ConfigData.ScreenMode = isFullScreen;
            Screen.fullScreen = isFullScreen;
        }

        private static List<Resolution> _resolutions = new();
        public static void ChangeScreenSize(bool plus)
        {
            if (_resolutions.Count == 0)
            {
                _resolutions = Screen.resolutions.ToList();
            }
            var findIndex = _resolutions.FindIndex(a => a.width == GameSystem.ConfigData.ScreenWidth && a.height == GameSystem.ConfigData.ScreenHeight);
            if (findIndex > -1)
            {
                var targetResolutionIndex = plus ? findIndex + 1 : findIndex - 1;
                if (targetResolutionIndex >= _resolutions.Count)
                {
                    return;
                } else
                if (targetResolutionIndex < 0)
                {
                    return;
                }
                SetResolution(_resolutions[targetResolutionIndex].width,_resolutions[targetResolutionIndex].height);
            }
        }

        public static void SetResolution(int width,int height)
        {
            Screen.SetResolution(width,height,Screen.fullScreen);
            GameSystem.ConfigData.ScreenWidth = width;
            GameSystem.ConfigData.ScreenHeight = height;
        }

        public static void ChangeBGMValue(float bgmVolume)
        {
            SoundManager.Instance.SetBgmVolume(bgmVolume);
            SoundManager.Instance.UpdateBgmVolume();
            if (bgmVolume > 0 && SoundManager.Instance.BGMMute == false)
            {
                ChangeBGMMute(false);
            }
            if (bgmVolume == 0 && SoundManager.Instance.BGMMute == true)
            {
                ChangeBGMMute(true);
            }
        }

        public static void ChangeBGMMute(bool bgmMute)
        {
            SoundManager.Instance.BGMMute = bgmMute;
            SoundManager.Instance.UpdateBgmVolume();
        }
        
        public static void ChangeSEValue(float seVolume)
        {
            SoundManager.Instance.SeVolume = seVolume;
            Effekseer.Internal.EffekseerSoundPlayer.SeVolume = seVolume;
            SoundManager.Instance.UpdateSeVolume();
            if (seVolume > 0 && SoundManager.Instance.SeMute == false)
            {
                ChangeSEMute(false);
            }
            if (seVolume == 0 && SoundManager.Instance.SeMute == true)
            {
                ChangeSEMute(true);
            }
        }

        public static void ChangeSEMute(bool seMute)
        {
            SoundManager.Instance.SeMute = seMute;
        }

        public static void ChangeGraphicIndex(int graphicIndex)
        {
            GameSystem.ConfigData.GraphicIndex = graphicIndex;
            QualitySettings.SetQualityLevel(graphicIndex);
        }

        public static void ChangeEventSkipIndex(bool eventSkipIndex)
        {
            GameSystem.ConfigData.EventSkipIndex = eventSkipIndex;
        }

        public static void ChangeCommandEndCheck(bool commandEndCheck)
        {
            GameSystem.ConfigData.CommandEndCheck = commandEndCheck;
        }

        public static void ChangeBattleWait(bool battleWait)
        {
            GameSystem.ConfigData.BattleWait = battleWait;
        }

        public static void ChangeBattleAnimation(bool battleAnimation)
        {
            GameSystem.ConfigData.BattleAnimationSkip = battleAnimation;
        }

        public static void ChangeInputType(InputType inputType)
        {
            GameSystem.ConfigData.InputType = inputType;
        }
        
        public static void ChangeBattleAuto(bool battleAuto)
        {
            GameSystem.ConfigData.BattleAuto = battleAuto;
        }

        public static List<float> SpeedList = new List<float>(){0,1f,2f,3f};
        public static void SetBattleSpeed(float battleSpeed)
        {
            GameSystem.ConfigData.BattleSpeed = battleSpeed;
        }
        public static void ChangeBattleSpeed(int plus)
        {
            var current = SpeedList.FindIndex(a => a == GameSystem.ConfigData.BattleSpeed);
            var next = current + plus;
            if (next < 0)
            {
                GameSystem.ConfigData.BattleSpeed = SpeedList[SpeedList.Count-1];
            } else
            if (next > SpeedList.Count-1)
            {
                GameSystem.ConfigData.BattleSpeed = SpeedList[1];
            } else
            {
                GameSystem.ConfigData.BattleSpeed = SpeedList[next];
            }
        }

        public static string CurrentBattleSpeedText()
        {
            var current = SpeedList.FindIndex(a => a == GameSystem.ConfigData.BattleSpeed);
            var option = DataSystem.System.OptionCommandData.Find(a => a.Key == "BATTLE_SPEED");
            switch (current)
            {
                case 1:
                return DataSystem.GetText(option.ToggleText1);
                case 2:
                return DataSystem.GetText(option.ToggleText2);
                case 3:
                return DataSystem.GetText(option.ToggleText3);
            }
            return "";
        }

        public static void ChangeTutorialCheck(bool tutorialCheck)
        {
            GameSystem.ConfigData.TutorialCheck = tutorialCheck;
        }
    }
}
