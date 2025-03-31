using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utage;

namespace Ryneus
{
    public class AdvController : BaseView, IInputHandlerEvent
    {
        [SerializeField] private AdvUguiManager advUguiManager = null;
        [SerializeField] private Button advInputButton = null;
        [SerializeField] private List<BaseCommand> skipButtonList = null;
        [SerializeField] private List<BaseCommand> autoButtonList = null;

        private bool _advPlaying = false;

        private string _lastKey = "";
        private List<OnOffButton> _onOffButtons = new();
        private int _selectIndex = -1;
        public override void Initialize() 
        {
            base.Initialize();
            advInputButton.onClick.AddListener(() => {advUguiManager.OnInput();});
            autoButtonList.ForEach(a => a.SetCallHandler(() => 
            {
                OnClickAuto();
            }));
            UpdateAutoButton();
            skipButtonList.ForEach(a => a.SetCallHandler(() => 
            {
                OnClickSkip();
            }));
            UpdateSkipButton();
            advUguiManager.Engine.SelectionManager.OnBeginWaitInput.AddListener(OnBeginShow);
        }

		public virtual void OnBeginShow( AdvSelectionManager manager )
		{
            _onOffButtons.Clear();
            var onOffButton = advUguiManager.CurrentSelection.ListView.Content.GetComponentsInChildren<OnOffButton>();
            foreach (var item in onOffButton)
            {
                _onOffButtons.Add(item);
            }
            if (_onOffButtons.Count > 1)
            {
                _onOffButtons[1].SetUnSelect();
                _onOffButtons[0].SetSelect();
                _selectIndex = 0;
            }
		}

        public void StartAdv()
        {
            _advPlaying = true;
            advInputButton.gameObject.SetActive(true);
            UpdateSkipButton();
        }

        public void EndAdv()
        {
            _advPlaying = false;
            _selectIndex = -1;
            _onOffButtons.Clear();
            SaveSystem.SaveOptionStart(GameSystem.OptionData);
            advInputButton.gameObject.SetActive(false);
        }
        
        public void InputHandler(List<InputKeyType> keyTypes,bool pressed)
        {
            if (_advPlaying == false) return;
            if (keyTypes.Contains(InputKeyType.Decide) || keyTypes.Contains(InputKeyType.Cancel))
            {
                if (_selectIndex > -1)
                {
                    advUguiManager.Engine.SelectionManager.Select(_selectIndex);
                    SoundManager.Instance.PlayStaticSe(SEType.Decide);
                    _selectIndex = -1;
                } else
                {
                    advUguiManager.OnInput();
                }
            }
            if (keyTypes.Contains(InputKeyType.Option1))
            {
                advUguiManager.Engine.Config.ToggleSkip();        
                GameSystem.OptionData.EventTextSkipIndex = advUguiManager.Engine.Config.IsSkip;
            }
            // 選択肢操作
            if (keyTypes.Contains(InputKeyType.Down))
            {
                if (_onOffButtons.Count > 1)
                {
                    _onOffButtons[0].SetUnSelect();
                    _onOffButtons[1].SetSelect();
                    SoundManager.Instance.PlayStaticSe(SEType.Cursor);
                    _selectIndex = 1;
                }
            } else
            if (keyTypes.Contains(InputKeyType.Up))
            {
                if (_onOffButtons.Count > 0)
                {
                    _onOffButtons[1].SetUnSelect();
                    _onOffButtons[0].SetSelect();
                    SoundManager.Instance.PlayStaticSe(SEType.Cursor);
                    _selectIndex = 0;
                }
            }
        }

        private new void Update() 
        {
            base.Update();
            if (advUguiManager.Engine.SelectionManager.IsWaitInput == true && (HelpWindow.LastKey != "ADV_SELECTING" || HelpWindow.LastKey != "ADV_SELECTING_ONE"))
            {
                _lastKey = HelpWindow.LastKey;
                if (advUguiManager.Engine.SelectionManager.TotalCount == 1)
                {
                    HelpWindow.SetInputInfo("ADV_SELECTING_ONE");

                } else
                {
                    HelpWindow.SetInputInfo("ADV_SELECTING");

                }
            }
            if (advUguiManager.Engine.SelectionManager.IsWaitInput == false && HelpWindow.LastKey != "ADV_READING")
            {
                _lastKey = HelpWindow.LastKey;
                HelpWindow.SetInputInfo("ADV_READING");
            }
        }

        private void OnClickAuto()
        {
            advUguiManager.Engine.Config.ToggleAuto();
            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            UpdateAutoButton();
        }

        private void UpdateAutoButton()
        {
            var auto = advUguiManager.Engine.Config.IsAutoBrPage;
            autoButtonList.ForEach(a => 
                a.Cursor.SetActive(auto)
            );
        }

        private void OnClickSkip()
        {
            advUguiManager.Engine.Config.ToggleSkip();
            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            UpdateSkipButton();
        }

        private void UpdateSkipButton()
        {
            var skip = advUguiManager.Engine.Config.IsSkip;
            skipButtonList.ForEach(a => 
                a.Cursor.SetActive(skip)
            );
            if (GameSystem.OptionData != null)
            {
                GameSystem.OptionData.EventTextSkipIndex = skip;
            }
        }
    }
}