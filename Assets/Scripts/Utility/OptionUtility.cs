using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ryneus
{
    public class OptionUtility
    {
        public static void ApplyOptionData()
        {
            var saveOptionInfo = GameSystem.OptionData;
            if (saveOptionInfo != null)
            {
                ChangeBGMValue(saveOptionInfo.BgmVolume);
                ChangeSEValue(saveOptionInfo.SeVolume);
                ChangeScreenMode(saveOptionInfo.ScreenMode);
                SetResolution(saveOptionInfo.ScreenWidth,saveOptionInfo.ScreenHeight);
                ChangeGraphicIndex(saveOptionInfo.GraphicIndex);
                ChangeEventSkipIndex(saveOptionInfo.EventSkipIndex);
                ChangeCommandEndCheck(saveOptionInfo.CommandEndCheck);
                ChangeBattleWait(saveOptionInfo.BattleWait);
                ChangeBattleAnimation(saveOptionInfo.BattleAnimationSkip);
                ChangeInputType(saveOptionInfo.InputType);
                ChangeBattleAuto(saveOptionInfo.BattleAuto);
                SetBattleSpeed(saveOptionInfo.BattleSpeed);
                ChangeTutorialCheck(saveOptionInfo.TutorialCheck);
            }
        }

        public static void ChangeScreenMode(bool isFullScreen)
        {
            GameSystem.OptionData.ScreenMode = isFullScreen;
            Screen.fullScreen = isFullScreen;
        }

        private static List<Resolution> _resolutions = new();
        public static void ChangeScreenSize(bool plus)
        {
            if (_resolutions.Count == 0)
            {
                _resolutions = Screen.resolutions.ToList();
            }
            var findIndex = _resolutions.FindIndex(a => a.width == GameSystem.OptionData.ScreenWidth && a.height == GameSystem.OptionData.ScreenHeight);
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
            GameSystem.OptionData.ScreenWidth = width;
            GameSystem.OptionData.ScreenHeight = height;
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
            GameSystem.OptionData.GraphicIndex = graphicIndex;
            QualitySettings.SetQualityLevel(graphicIndex);
        }

        public static void ChangeEventSkipIndex(bool eventSkipIndex)
        {
            GameSystem.OptionData.EventSkipIndex = eventSkipIndex;
        }

        public static void ChangeCommandEndCheck(bool commandEndCheck)
        {
            GameSystem.OptionData.CommandEndCheck = commandEndCheck;
        }

        public static void ChangeBattleWait(bool battleWait)
        {
            GameSystem.OptionData.BattleWait = battleWait;
        }

        public static void ChangeBattleAnimation(bool battleAnimation)
        {
            GameSystem.OptionData.BattleAnimationSkip = battleAnimation;
        }

        public static void ChangeInputType(InputType inputType)
        {
            GameSystem.OptionData.InputType = inputType;
        }
        
        public static void ChangeBattleAuto(bool battleAuto)
        {
            GameSystem.OptionData.BattleAuto = battleAuto;
        }

        public static List<float> SpeedList = new List<float>(){0,1f,2f,3f};
        public static void SetBattleSpeed(float battleSpeed)
        {
            GameSystem.OptionData.BattleSpeed = battleSpeed;
        }
        public static void ChangeBattleSpeed(int plus)
        {
            var current = SpeedList.FindIndex(a => a == GameSystem.OptionData.BattleSpeed);
            var next = current + plus;
            if (next < 0)
            {
                GameSystem.OptionData.BattleSpeed = SpeedList[SpeedList.Count-1];
            } else
            if (next > SpeedList.Count-1)
            {
                GameSystem.OptionData.BattleSpeed = SpeedList[1];
            } else
            {
                GameSystem.OptionData.BattleSpeed = SpeedList[next];
            }
        }

        public static string CurrentBattleSpeedText()
        {
            var current = SpeedList.FindIndex(a => a == GameSystem.OptionData.BattleSpeed);
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
            GameSystem.OptionData.TutorialCheck = tutorialCheck;
        }
    }
}
