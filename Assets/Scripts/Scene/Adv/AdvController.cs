using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
            advInputButton.onClick.AddListener(() =>
            {
                advUguiManager.OnInput();
            });
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
            AssetFileManager.GetCustomLoadManager().OnFindAsset += FindAsset;
        }

        private void FindAsset(AssetFileManager mangager, AssetFileInfo fileInfo, IAssetFileSettingData settingData, ref AssetFileBase asset)
        {
            asset = new SampleCustomFile(mangager, fileInfo, settingData);
        }

        public virtual void OnBeginShow(AdvSelectionManager manager)
        {
            _onOffButtons.Clear();
            var onOffButton = advUguiManager.CurrentSelection.ListView.Content.GetComponentsInChildren<OnOffButton>();
            foreach (var item in onOffButton)
            {
                _onOffButtons.Add(item);
            }
            _selectIndex = 0;
            _onOffButtons[0].SetSelect();
            if (_onOffButtons.Count > 1)
            {
                _onOffButtons[1].SetUnSelect();
            }
        }

        public void StartAdv()
        {
            _advPlaying = true;
            UIComponent.SetActive(advInputButton?.gameObject, true);
            UpdateSkipButton();
        }

        public void EndAdv()
        {
            _advPlaying = false;
            _selectIndex = -1;
            _onOffButtons.Clear();
            SaveSystem.SaveOptionStart(GameSystem.OptionData);
            UIComponent.SetActive(advInputButton?.gameObject, false);
        }

        public void InputHandler(List<InputKeyType> keyTypes, bool pressed)
        {
            if (!_advPlaying)
            {
                return;
            }
            if (keyTypes.Contains(InputKeyType.Decide) || keyTypes.Contains(InputKeyType.Cancel))
            {
                if (_selectIndex > -1 && advUguiManager.Engine.SelectionManager.IsWaitInput)
                {
                    advUguiManager.Engine.SelectionManager.Select(_selectIndex);
                    SoundManager.Instance.PlayStaticSe(SEType.Decide);
                    _selectIndex = -1;
                }
                else
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
            }
            else
            if (keyTypes.Contains(InputKeyType.Up))
            {
                if (_onOffButtons.Count > 0)
                {
                    _onOffButtons[0].SetSelect();
                    SoundManager.Instance.PlayStaticSe(SEType.Cursor);
                }
                if (_onOffButtons.Count > 1)
                {
                    _onOffButtons[1].SetUnSelect();
                    SoundManager.Instance.PlayStaticSe(SEType.Cursor);
                }
                _selectIndex = 0;
            }
        }

        private void Update()
        {
            if (advUguiManager.Engine.SelectionManager.IsWaitInput && (HelpWindow.LastKey != "ADV_SELECTING" || HelpWindow.LastKey != "ADV_SELECTING_ONE"))
            {
                _lastKey = HelpWindow.LastKey;
                if (advUguiManager.Engine.SelectionManager.TotalCount == 1)
                {
                    HelpWindow.SetInputInfo("ADV_SELECTING_ONE");

                }
                else
                {
                    HelpWindow.SetInputInfo("ADV_SELECTING");
                }
            }
            if (!advUguiManager.Engine.SelectionManager.IsWaitInput && HelpWindow.LastKey != "ADV_READING")
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
                UIComponent.SetActive(a.Cursor, auto)
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
                UIComponent.SetActive(a.Cursor, skip)
            );
            if (GameSystem.OptionData != null)
            {
                GameSystem.OptionData.EventTextSkipIndex = skip;
            }
        }
    }

    //自作のファイルマネージャーと連結するサンプル
    public class SampleCustomFile : AssetFileBase
    {
        public SampleCustomFile(AssetFileManager mangager, AssetFileInfo fileInfo, IAssetFileSettingData settingData)
            : base(mangager, fileInfo, settingData)
        {
        }

        //ロード処理
        public override IEnumerator LoadAsync(System.Action onComplete, System.Action onFailed)
        {
            IsLoadEnd = true;
            InitFromCustomFileManager();
            onComplete();
            yield break;
        }

        //ローカルまたはキャッシュあるか（つまりサーバーからDLする必要があるか）
        public override bool CheckCacheOrLocal() { return false; }

        //アンロード処理
        public override void Unload()
        {
            IsLoadEnd = false;

            //宴からの参照がなくなったということ
            //自作のファイルマネージャーのアンロード処理を呼ぶ
            //このタイミングで行う必要がなければここでおわり
        }

        //以下、自作のファイルマネージャーから、オブジェクトの参照を行う
        async Task InitFromCustomFileManager()
        {
            //Resources.Loadの部分を、自作のファイルマネージャーからのオブジェクト参照に切り替える
            string path = FilePathUtil.GetPathWithoutExtension(FileInfo.FileName);
            switch (FileType)
            {
                case AssetFileType.Text:        //テキスト
                    //Text = Resources.Load<TextAsset>(path);
                    break;
                case AssetFileType.Texture:     //テクスチャ
                    Texture = await ResourceSystem.LoadAsset<Texture2D>(path);
                    break;
                case AssetFileType.Sound:       //サウンド
                    Sound = await ResourceSystem.LoadAsset<AudioClip>(path);
                    break;
                case AssetFileType.UnityObject:     //Unityオブジェクト（プレハブとか）
                    UnityObject = await ResourceSystem.LoadAsset<GameObject>(path.Replace("Texture/Character/" , ""));
                    break;
                default:
                    break;
            }
        }
    }
}