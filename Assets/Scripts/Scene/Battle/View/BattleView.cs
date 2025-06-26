using System;
using System.Collections.Generic;
using UnityEngine;
using Effekseer;
using TMPro;
using Cysharp.Threading.Tasks;
using Ryneus.Battle;

namespace Ryneus
{
    public partial class BattleView : BaseView, IInputHandlerEvent
    {
        [SerializeField] private BattleBattlerList battleActorList = null;
        [SerializeField] private BattleBattlerList battleEnemyList = null;
        [SerializeField] private BattleGridLayer battleGridLayer = null;
        [SerializeField] private BattleThumb battleThumb;
        [SerializeField] private TextMeshProUGUI turns;
        [SerializeField] private TextMeshProUGUI maxTurns;

        [SerializeField] private GameObject animRoot = null;
        [SerializeField] private GameObject animPrefab = null;
        [SerializeField] private SkillInfoComponent skillInfoComponent = null;
        [SerializeField] private GameObject currentSkillBg = null;
        [SerializeField] private MakerEffekseerEmitter effekseerEmitter;
        [SerializeField] private OnOffButton battleAutoButton = null;
        [SerializeField] private OnOffButton battleSpeedButton = null;
        [SerializeField] private OnOffButton battleSkipButton = null;
        [SerializeField] private OnOffButton skillLogButton = null;
        [SerializeField] private BattleCutinAnimation battleCutinAnimation = null;
        [SerializeField] private GameObject battleBackGroundRoot = null;
        [SerializeField] private EffekseerEmitter demigodCutinAnimation = null;
        [SerializeField] private BattleAwakenAnimation battleAwakenAnimation = null;
        [SerializeField] private MagicList magicList = null;
        [SerializeField] private OnOffButton formationButton = null;
        [SerializeField] private InputInfoComponent formationInpurKey = null;
        [SerializeField] private OnOffButton decideButton = null;
        [SerializeField] private InputInfoComponent decideInpurKey = null;
        private BattleBackGroundAnimation _backGroundAnimation = null;

        private BattleStartAnim _battleStartAnim = null;
        public bool StartAnimIsBusy => _battleStartAnim.IsBusy;

        private bool _battleBusy = false;
        public bool BattleBusy => _battleBusy;
        public void SetBattleBusy(bool isBusy)
        {
            _battleBusy = isBusy;
        }
        private bool _animationBusy = false;
        public bool AnimationBusy => _animationBusy;
        public void SetAnimationBusy(bool isBusy)
        {
            _animationBusy = isBusy;
        }

        private List<MakerEffectData.SoundTimings> _soundTimings = null;

        private readonly Dictionary<int,BattlerInfoComponent> _battlerComps = new();

        private bool _skipBattle = false;
        public override void Initialize() 
        {
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.Battle);
            ClearCurrentSkillData();

            InitializeSelectCharacter();
            InitializeActorList();
            InitializeEnemyLayer();
            SideMenuButton.OnClickAddListener(() =>
            {
                CallSideMenu();
            });
            battleSpeedButton.OnClickAddListener(() =>
            {
                CallChangeBattleSpeed(1);
            });
            battleSkipButton.OnClickAddListener(() =>
            {
                CallBattleSkip();
            });
            if (decideInpurKey != null)
            {
                decideInpurKey.UpdateGuideIcon(12);
            }
            if (decideButton != null)
            {
                decideButton.OnClickAddListener(() =>
                {
                    CallViewEvent(CommandType.DecideBattle);
                });
            }
            if (formationInpurKey != null)
            {
                formationInpurKey.UpdateGuideIcon(6);
            }
            if (formationButton != null)
            {
                formationButton.OnClickAddListener(() =>
                {
                    CallViewEvent(CommandType.Formation);
                });
            }
            skillLogButton.OnClickAddListener(() =>
            {
                if (!skillLogButton.gameObject.activeSelf)
                {
                    return;
                }
                CallViewEvent(CommandType.SkillLog);
            });
            SetBattleSkipActive(false);
            battleCutinAnimation.Initialize();
            InitializeMagicList();
            _ = new BattlePresenter(this);
        }

        public void CreateBattleBackGround(GameObject gameObject)
        {
            //var prefab = Instantiate(gameObject);
            //prefab.transform.SetParent(battleBackGroundRoot.transform,false);
            //_backGroundAnimation = prefab.GetComponent<BattleBackGroundAnimation>();
        }

        private void InitializeActorList()
        {
            battleActorList.Initialize();
            battleActorList.SetInputHandler(InputKeyType.Decide,() => CallViewEvent(CommandType.SelectCharacter,battleActorList.Index));
            battleActorList.SetInputHandler(InputKeyType.Cancel,() => CallViewEvent(CommandType.EndFormation));
            AddViewActives(battleActorList);
        }

        public void SetActors(List<ListData> battlerInfos)
        {
            battleActorList.SetData(battlerInfos);
            foreach (var battlerInfo in battlerInfos)
            {
                var data = (BattlerInfo)battlerInfo.Data;
                if (data.Index.Value > 0)
                {
                    _battlerComps[data.Index.Value] = battleActorList.GetBattlerInfoComp(data.Index.Value);
                }
            }
        }

        private void InitializeEnemyLayer()
        {
            battleEnemyList.Initialize();
            battleEnemyList.SetInputHandler(InputKeyType.Up,() => OnSelectTarget(InputKeyType.Up));
            battleEnemyList.SetInputHandler(InputKeyType.Down,() => OnSelectTarget(InputKeyType.Down));
            battleEnemyList.SetInputHandler(InputKeyType.Right,() => OnSelectTarget(InputKeyType.Right));
            battleEnemyList.SetInputHandler(InputKeyType.Left,() => OnSelectTarget(InputKeyType.Left));
            battleEnemyList.SetInputHandler(InputKeyType.Decide,OnDecideEnemy);
            battleEnemyList.SetInputHandler(InputKeyType.Cancel,() => CallViewEvent(CommandType.OnCancelEnemy));
            //battleEnemyUnitList.SetSelectedHandler(TargetSelectCursor);
            AddViewActives(battleEnemyList);
        }

        public void SetEnemies(List<ListData> battlerInfos)
        {
            battleEnemyList.SetData(battlerInfos);
            //battleEnemyUnitList.SetSelectedHandler(() => CallSelectEnemyList());
            foreach (var battlerInfo in battlerInfos)
            {
                var data = (BattlerInfo)battlerInfo.Data;
                if (data.Index.Value > 0)
                {
                    _battlerComps[data.Index.Value] = battleEnemyList.GetBattlerInfoComp(data.Index.Value);
                    _battlerComps[data.Index.Value].HideStatus();
                }
            }
        }

        public void UpdateSelectCursor(List<int> targetIndexes)
        {
            battleActorList.UpdateSelectIndexList(targetIndexes);
            battleEnemyList.UpdateSelectIndexList(targetIndexes);
        }

        public void SelectActorList(List<int> selectIndexes)
        {
            magicList.gameObject.SetActive(false);
            battleThumb.HideThumb();
            SetActivate(battleActorList);
            //battleActorList.UpdateSelectIndex(selectIndexes[0]);
            var selects = new List<int>();
            foreach (var select in selectIndexes)
            {
                selects.Add(select-1);
            }
            battleActorList.SetSelectIndexes(selects);
            battleActorList.UpdateSelectIndex(selectIndexes[0]);
        }

        public void SelectEnemyList(List<int> selectIndexes)
        {
            magicList.gameObject.SetActive(false);
            battleThumb.HideThumb();
            SetActivate(battleEnemyList);
            battleEnemyList.SetSelectIndexes(selectIndexes);
            battleEnemyList.UpdateSelectIndex(selectIndexes[0]-101);
        }

        public void SetGridMembers(List<BattlerInfo> battlerInfos)
        {
            battleGridLayer.SetGridMembers(battlerInfos);
        }

        public void UpdateGridLayer()
        {
            battleGridLayer.UpdatePosition();
        }

        private void InitializeMagicList()
        {
            magicList.Initialize();
            magicList.SetInputHandler(InputKeyType.Decide,OnDecideSkill);
            //magicList.SetInputHandler(InputKeyType.Right,() => OnSelectTarget(InputKeyType.Right));
            //magicList.SetInputHandler(InputKeyType.Left,() => OnSelectTarget(InputKeyType.Left));
            magicList.gameObject.SetActive(false);
            magicList.SetSelectedHandler(OnSelectMagic);
            AddViewActives(magicList);
        }

        private void OnDecideSkill()
        {
            var listData = magicList.ListItemData<SkillInfo>();
            if (listData != null && listData.Enable)
            {
                CallViewEvent(CommandType.OnDecideSkill,listData);
            }
        }

        private void OnSelectMagic()
        {
            var listData = magicList.ListItemData<SkillInfo>();
            if (listData != null && listData.Enable)
            {
                CallViewEvent(CommandType.OnSelectSkill,listData);
            }
        }

        private void OnSelectTarget(InputKeyType inputKeyType)
        {
            CallViewEvent(CommandType.OnSelectTarget,inputKeyType);
        }

        private void OnDecideEnemy()
        {
            var listData = battleEnemyList.ListItemData<BattlerInfo>();
            if (listData != null)
            {
                CallViewEvent(CommandType.OnDecideEnemy,listData);
            }
        }

        private void OnDecideActor()
        {
            var listData = battleActorList.ListItemData<BattlerInfo>();
            if (listData != null)
            {
                CallViewEvent(CommandType.OnDecideActor,listData);
            }
        }

        public void EndActionSelect()
        {
            UpdateSelectCursor(new List<int>(){});
            SetActivate(null);
            battleThumb.HideThumb();
            magicList.gameObject.SetActive(false);
            battleEnemyList.ClearSelect();
            battleActorList.ClearSelect();
        }

        public void SetActiveBeforeBattles(bool isActive)
        {
            formationButton.gameObject.SetActive(isActive);
            decideButton.gameObject.SetActive(isActive);
        }

        public void StartFormation()
        {
            SetActivate(battleActorList);
            battleActorList.UpdateSelectIndex(0);
        }

        public void CancelFormation()
        {
            battleActorList.UpdateSelectIndex(-1);
            SetActivate(null);
        }

        public void EndFormation()
        {
            battleActorList.SetInputHandler(InputKeyType.Decide,() => OnDecideActor());
            battleActorList.SetInputHandler(InputKeyType.Cancel,() => CallViewEvent(CommandType.OnCancelActor));
            battleActorList.UpdateSelectIndex(-1);
            SetActivate(null);
        }

        private void InitializeSelectCharacter()
        {
        }

        public void SetBattleAutoButton(bool isAuto)
        {
            battleAutoButton.gameObject.SetActive(false);
            battleAutoButton.OnClickAddListener(() =>
            {
                if (!battleAutoButton.gameObject.activeSelf)
                {
                    return;
                }
                CallViewEvent(CommandType.ChangeBattleAuto);
            },() =>
            {
                ChangeBattleAuto(isAuto);
            });
            ChangeBattleAuto(isAuto);
        }

        public void SetActiveBattleAutoButton(bool isActive)
        {
            battleAutoButton.gameObject.SetActive(isActive);
            battleSpeedButton.gameObject.SetActive(isActive);
        }

        public void SetBattleSkipActive(bool isActive)
        {
            //battleSkipButton.gameObject.SetActive(isActive);
            //skillLogButton.gameObject.SetActive(isActive);
        }

        public void SetBattleSpeedButton(string commandName)
        {
            battleSpeedButton.SetText(commandName);
            battleSpeedButton.UpdateViewItem();
        }

        public void SetBattleSkipButton(string commandName)
        {
            battleSkipButton.SetText(commandName);
            battleSkipButton.UpdateViewItem();
        }

        public void SetSkillLogButton(string commandName)
        {
            skillLogButton.SetText(commandName);
            skillLogButton.UpdateViewItem();
        }

        public void UpdateStartActivate()
        {
            //if (GameSystem.ConfigData.InputType)
            {
                //battleEnemyUnitList.Activate();
                //battleEnemyUnitList.UpdateSelectIndex(0);
                //battleActorList.Deactivate();
            }
        }

        private void CallBattleSkip()
        {
            if (!battleSkipButton.gameObject.activeSelf)
            {
                return;
            }
            _skipBattle = true;
            CallViewEvent(CommandType.SkipBattle);
        }

        private void CallChangeBattleSpeed(int plus)
        {
            if (!battleSpeedButton.gameObject.activeSelf)
            {
                return;
            }
            CallViewEvent(CommandType.ChangeBattleSpeed,plus);
        }

        public void CreateObject()
        {
            var prefab = Instantiate(animPrefab);
            prefab.transform.SetParent(animRoot.transform, false);
            _battleStartAnim = prefab.GetComponent<BattleStartAnim>();
            _battleStartAnim.gameObject.SetActive(false);
        }

        public void StartBattleStartAnim(string text)
        {
            _battleStartAnim.SetText(text);
            _battleStartAnim.StartAnim(true);
            _battleStartAnim.gameObject.SetActive(true);
        }

        public void StartUIAnimation()
        {
            battleActorList.gameObject.SetActive(true);
            //battleEnemyUnitList.gameObject.SetActive(true);
            var duration = 0.8f;
            /*
            var actorListRect = battleActorList.GetComponent<RectTransform>();
            AnimationUtility.LocalMoveToTransform(battleActorList.gameObject,
                new Vector3(actorListRect.localPosition.x - 240,actorListRect.localPosition.y,0),
                new Vector3(actorListRect.localPosition.x,actorListRect.localPosition.y,0),
                duration);
            var enemyListRect = battleEnemyUnitList.GetComponent<RectTransform>();
            AnimationUtility.LocalMoveToTransform(enemyListRect.gameObject,
                new Vector3(enemyListRect.localPosition.x + 240,enemyListRect.localPosition.y,0),
                new Vector3(enemyListRect.localPosition.x,enemyListRect.localPosition.y,0),
                duration);
                */
                /*
            var borderRect = battleGridLayer.GetComponent<RectTransform>();
            AnimationUtility.LocalMoveToTransform(borderRect.gameObject,
                new Vector3(borderRect.localPosition.x,borderRect.localPosition.y,0),
                new Vector3(borderRect.localPosition.x,borderRect.localPosition.y-480,0),
                duration);
                */
        }

        public void ChangeSideMenuButtonActive(bool isActive)
        {
            //SideMenuButton.gameObject.SetActive(isActive);
        }

        private void OnClickBack()
        { 
            CallViewEvent(CommandType.Back);
            SetInputFrame(1);
        }

        public void ShowMagicList(List<ListData> skillInfos,bool resetScrollRect,int selectIndex)
        {
            SetActivate(magicList);
            battleActorList.gameObject.SetActive(true);
            magicList.gameObject.SetActive(true);
            magicList.SetData(skillInfos,resetScrollRect);
            if (resetScrollRect)
            {
                magicList.UpdateSelectIndex(selectIndex);
            }
            OnSelectMagic();
        }

        public new void SetHelpText(string text)
        {
            HelpWindow.SetHelpText(text);
        }

        private void CallEnemyDetailInfo(List<BattlerInfo> battlerInfos)
        {
            if (_animationBusy) return;
            var selectedIndex = battleEnemyList.SelectedIndex;
            var battlerInfo = battlerInfos.Find(a => a.Index.Value == selectedIndex);
            if (battlerInfo != null)
            {
                CallViewEvent(CommandType.EnemyDetail,selectedIndex);
            }
        }

        public void SelectedCharacter(BattlerInfo battlerInfo)
        {
            battleThumb.ShowBattleThumb(battlerInfo);
            battleThumb.gameObject.SetActive(true);
            // 敵のstateEffectを非表示
            HideEnemyStateOverlay();
            //HideActorStateOverlay();
        }

        public void ShowCutinBattleThumb(BattlerInfo battlerInfo)
        {
            battleThumb.ShowCutinBattleThumb(battlerInfo);
            battleThumb.gameObject.SetActive(true);
        }

        public void HideSkillActionList(bool isSideMenuClose = true)
        {
        }

        public void HideBattleThumb()
        {
            battleThumb.HideThumb();
        }
        
        public void RefreshMagicList(List<ListData> skillInfos,int selectIndex)
        {
            //selectCharacter.SetActiveTab(SelectCharacterTabType.Detail,false);
        }

        public void SetCondition(List<ListData> stateInfos)
        {
        }


        public void RefreshPartyBattlerList(List<ListData> battlerInfos)
        {
            battleActorList.SetTargetListData(battlerInfos);
            foreach (var item in _battlerComps)
            {
                var battlerInfo = battlerInfos.Find(a => item.Key == ((BattlerInfo)a.Data).Index.Value);
                if (battlerInfo != null)
                {
                    var selectable = battlerInfo.Enable;
                    item.Value.SetThumbAlpha(selectable);
                }
            }
        }

        public void RefreshEnemyBattlerList(List<ListData> battlerInfos)
        {
            battleEnemyList.SetTargetListData(battlerInfos);
            foreach (var item in _battlerComps)
            {
                var battlerInfo = battlerInfos.Find(a => item.Key == ((BattlerInfo)a.Data).Index.Value);
                if (battlerInfo != null)
                {
                    var selectable = battlerInfo.Enable;
                    item.Value.SetThumbAlpha(selectable);
                }
            }
        }

        public void BattlerBattleClearSelect()
        {
            //battleActorList.ClearSelect();
            //battleEnemyUnitList.ClearSelect();
        }

        public void HideEnemyStateOverlay()
        {
            foreach (var item in _battlerComps)
            {
                item.Value.HideEnemyStateOverlay();
            }
        }

        public void ShowStateOverlay()
        {
            foreach (var item in _battlerComps)
            {
                item.Value.ShowStateOverlay();
            }
        }

        public void HideStateOverlay()
        {
            foreach (var item in _battlerComps)
            {
                item.Value.HideStateOverlay();
            }
        }

        public void SetCurrentSkillData(SkillInfo skillInfo,BattlerInfo battlerInfo)
        {
            skillInfoComponent.gameObject.SetActive(true);
            skillInfoComponent.UpdateInfo(skillInfo);
            var convertHelpText = skillInfo.ConvertHelpText(battlerInfo);
            var length = convertHelpText.Split("\n").Length;
            var height = 32 + 28 * length;
            currentSkillBg.GetComponent<RectTransform>().sizeDelta = new Vector2(480,height);
        }

        public void ClearCurrentSkillData()
        {
            skillInfoComponent.gameObject.SetActive(false);
            skillInfoComponent.Clear();
        }

        public void StartAnimation(int targetIndex,EffekseerEffectAsset effekseerEffectAsset,AnimationPosition animationPosition,float animationScale = 1.0f,float animationSpeed = 1.0f)
        {
            if (!_battlerComps.ContainsKey(targetIndex))
            {
                return;
            }
            magicList.gameObject.SetActive(false);
            if (GameSystem.OptionData.BattleAnimationSkip == true) 
            {
                return;
            }
            animationSpeed *= GameSystem.OptionData.BattleSpeed;
            _battlerComps[targetIndex].StartAnimation(effekseerEffectAsset,animationPosition,animationScale,animationSpeed);
        }

        public void StartAnimationAll(EffekseerEffectAsset effekseerEffectAsset,AnimationPosition animationPosition,float animationScale = 1.0f,float animationSpeed = 1.0f)
        {
            magicList.gameObject.SetActive(false);
            if (GameSystem.OptionData.BattleAnimationSkip == true) 
            {
                return;
            }
            animationSpeed *= GameSystem.OptionData.BattleSpeed;

            effekseerEmitter.transform.localScale = new Vector3(animationScale,animationScale,animationScale);
            if (effekseerEffectAsset == null)
            {
                effekseerEmitter.Stop();
                return;
            }
            effekseerEmitter.Stop();
            effekseerEmitter.speed = animationSpeed;
            effekseerEmitter.Play(effekseerEffectAsset);
        }

        public void StartAnimationDemigod(BattlerInfo battlerInfo,SkillData skillData,float speedRate)
        {
            battleCutinAnimation.StartAnimation(battlerInfo,skillData,speedRate);
            //var handle = EffekseerSystem.PlayEffect(effekseerEffectAsset, centerAnimPosition.transform.position);
        }

        public async UniTask StartAnimationMessiah(BattlerInfo battlerInfo,Sprite actorSprite)
        {
            var speed = GameSystem.OptionData.BattleSpeed;
            if (!GameSystem.OptionData.BattleAnimationSkip)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Demigod);
                battleAwakenAnimation.StartAnimation(battlerInfo,actorSprite,speed);
                HideStateOverlay();
                SetAnimationBusy(true);
                await UniTask.DelayFrame((int)(60 / speed));
            }
        }

        public void ClearDamagePopup()
        {
            foreach (var item in _battlerComps)
            {
                item.Value.ClearDamagePopup();
            }
        }

        public void StartDamage(int targetIndex,DamageType damageType,int value,bool needPopupDelay = true)
        {
            if (!_battlerComps.ContainsKey(targetIndex))
            {
                return;
            }
            _battlerComps[targetIndex].StartDamage(damageType,value,needPopupDelay);
        }

        public void StartBlink(int targetIndex)
        {
            if (!_battlerComps.ContainsKey(targetIndex))
            {
                return;
            }
            _battlerComps[targetIndex].StartBlink();
        }

        public void StartHeal(int targetIndex,DamageType damageType, int value, bool needPopupDelay = true)
        {
            if (!_battlerComps.ContainsKey(targetIndex))
            {
                return;
            }
            _battlerComps[targetIndex].StartHeal(damageType,value,needPopupDelay);
        }

        public void StartStatePopup(int targetIndex, DamageType damageType, string stateName, bool buff = false, bool debuff = false)
        {
            if (!_battlerComps.ContainsKey(targetIndex))
            {
                return;
            }
            _battlerComps[targetIndex].StartStatePopup(damageType,stateName,buff,debuff);
        }

        public void StartDeathAnimation(int targetIndex)
        {
            if (!_battlerComps.ContainsKey(targetIndex))
            {
                return;
            }
            _battlerComps[targetIndex].StartDeathAnimation();
        }

        public void StartAliveAnimation(int targetIndex)
        {
            if (!_battlerComps.ContainsKey(targetIndex))
            {
                return;
            }
            _battlerComps[targetIndex].ResetDeathMaterial();
        }

        public void BattleVictory(int mvpActorId)
        {
        }

        public void RefreshStatus()
        {
            battleGridLayer.RefreshStatus();
            foreach (var item in _battlerComps)
            {
                item.Value.RefreshStatus();
            }
        }

        public void RefreshTurn(int turn)
        {
            turns?.SetText(turn.ToString());
        }

        public void SetBattlerThumbAlpha(bool selectable)
        {
            foreach (var item in _battlerComps)
            {
                item.Value.SetThumbAlpha(selectable);
            }
        }

        public void ShowEnemiesStatus()
        {
            foreach (var item in _battlerComps)
            {
                item.Value.SetActiveStatus(true);
            }
        }

        public void HideEnemiesStatus()
        {
            SetBattlerActiveStatus(new List<int>());
        }

        public void SetBattlerActiveStatus(List<int> selectableIndexes)
        {
            foreach (var item in _battlerComps)
            {
                item.Value.SetActiveStatus(selectableIndexes.Contains(item.Key));
            }
        }

        private new void Update()
        {
            base.Update();
            if (_battleBusy)
            {
                return;
            }
            CallViewEvent(CommandType.UpdateAp);
        }


        private void CallSideMenu()
        {
            CallViewEvent(CommandType.SelectSideMenu);
        }

        public void InputHandler(List<InputKeyType> keyTypes,bool pressed)
        {
            if (InputSystem.GetInputDate(InputKeyType.Start).IsDownTrigger())
            {
                CallViewEvent(CommandType.DecideBattle);
            } else
            if (InputSystem.GetInputDate(InputKeyType.Option1).IsDownTrigger())
            {
                CallViewEvent(CommandType.Formation);
            } else
            if (InputSystem.GetInputDate(InputKeyType.Option2).IsDownTrigger())
            {
                if (!battleAutoButton.gameObject.activeSelf)
                {
                    return;
                }
                CallViewEvent(CommandType.ChangeBattleAuto);
            } else
            if (InputSystem.GetInputDate(InputKeyType.SideLeft1).IsDownTrigger())
            {
                if (!battleSpeedButton.gameObject.activeSelf)
                {
                    return;
                }
                CallViewEvent(CommandType.ChangeBattleSpeed,-1);
            } else
            if (InputSystem.GetInputDate(InputKeyType.SideRight1).IsDownTrigger())
            {
                if (!battleSpeedButton.gameObject.activeSelf)
                {
                    return;
                }
                CallViewEvent(CommandType.ChangeBattleSpeed,1);
            }
        }

        public void ChangeBattleAuto(bool isAuto)
        {
            battleAutoButton.Cursor.SetActive(isAuto);
        }

        public async UniTask StartAnimationDemigod(BattlerInfo battlerInfo,SkillData skillData)
        {
            var speed = GameSystem.OptionData.BattleSpeed;
            if (GameSystem.OptionData.BattleAnimationSkip == false)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Demigod);
                StartAnimationDemigod(battlerInfo,skillData,speed);
                HideStateOverlay();
                SetAnimationBusy(true);
                await UniTask.DelayFrame((int)(20f / speed));
                SoundManager.Instance.PlayStaticSe(SEType.Awaken);
                await UniTask.DelayFrame((int)(90f / speed));
            }
        }

        public void StartAnimationBeforeSkill(int subjectIndex,EffekseerEffectAsset effekseerEffect)
        {
            if (!_battlerComps.ContainsKey(subjectIndex))
            {
                return;
            }
            SoundManager.Instance.PlayStaticSe(SEType.Skill);
            StartAnimation(subjectIndex, effekseerEffect,0, 1.5f, 0.75f);
            _battlerComps[subjectIndex].SetActiveBeforeSkillThumb(true);
        }

        public void StartAnimationSlipDamage(List<int> targetIndexes)
        {
            var animation = ResourceSystem.LoadResourceEffect("NA_Effekseer/NA_Fire_001");
            foreach (var targetIndex in targetIndexes)
            {
                StartAnimation(targetIndex,animation,0);
            }
        }

        public void StartAnimationRegenerate(List<int> targetIndexes)
        {
            var animation = ResourceSystem.LoadResourceEffect("tktk01/Cure1");
            foreach (var targetIndex in targetIndexes)
            {
                StartAnimation(targetIndex,animation,0);
            }
        }
    }
}