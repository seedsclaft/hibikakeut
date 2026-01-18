using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Ryneus.Interlude;

namespace Ryneus
{
    public class InterludeView : BaseView, IInputHandlerEvent
    {

        [SerializeField] private Image backgroundImage = null;
        [SerializeField] private StrategyActorList strategyActorList = null;
        [SerializeField] private CanvasGroup strategyResultCanvasGroup = null;
        [SerializeField] private GameObject rightParts = null;
        [SerializeField] private BaseList strategyResultList = null;
        public bool StrategyResultListActive => strategyResultList.gameObject.activeSelf;
        [SerializeField] private BaseList commandList = null;
        [SerializeField] private BaseList statusList = null;
        [SerializeField] private MagicList alcanaSelectList = null;
        [SerializeField] private TextMeshProUGUI title = null;
        [SerializeField] private ActorInfoComponent actorInfoComponent = null;
        [SerializeField] private Button lvUpStatusButton = null;
        [SerializeField] private GameObject animRoot = null;
        [SerializeField] private GameObject animPrefab = null;
        [SerializeField] private GameObject rankScoreObj = null;
        [SerializeField] private TextMeshProUGUI rankScoreText = null;
        [SerializeField] private GameObject claerStageNumObj = null;
        [SerializeField] private TextMeshProUGUI claerStageNumText = null;
        [SerializeField] private GameObject partyEvaluateObj = null;
        [SerializeField] private TextMeshProUGUI partyEvaluateText = null;

        private BattleStartAnim _battleStartAnim = null;
        private bool _animationBusy = false;

        public override void Initialize()
        {
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.Interlude);
            InitializeActorList();
            InitializeStatusList();
            InitializeCommandList();
            InitializeLearnSkillList();

            GameObject prefab = Instantiate(animPrefab);
            prefab.transform.SetParent(animRoot.transform, false);
            _battleStartAnim = prefab.GetComponent<BattleStartAnim>();
            _battleStartAnim.gameObject.SetActive(false);
            lvUpStatusButton.onClick.AddListener(() => CallLvUpNext());
            lvUpStatusButton.gameObject.SetActive(false);

            var rect = actorInfoComponent.gameObject.GetComponent<RectTransform>();
            rect.localPosition = new Vector3(0,0,0);
            actorInfoComponent.MainThumb.DOFade(0,0);

            strategyResultCanvasGroup.alpha = 0;
            claerStageNumObj?.SetActive(false);
            rankScoreObj?.SetActive(false);
            partyEvaluateObj?.SetActive(false);
            if (rightParts != null)
            {
                rightParts.SetActive(false);
            }
            _ = new InterludePresenter(this);
        }

        private void InitializeActorList()
        {
            strategyActorList.Initialize();
            AddViewActives(strategyActorList);
            strategyActorList.gameObject.SetActive(false);
        }

        public void StartResultAnimation(List<ListData> actorInfos, List<ActorInfo> bonusActorInfos = null)
        {
            SetDeactivate();
            strategyActorList.SetData(actorInfos);
            strategyActorList.StartResultAnimation(actorInfos.Count, bonusActorInfos, () =>
            {
                CallEndAnimation();
            });
            strategyActorList.gameObject.SetActive(true);
        }

        private void CallLvUpNext()
        {
            lvUpStatusButton.gameObject.SetActive(false);
            actorInfoComponent.gameObject.SetActive(false);
            statusList.gameObject.SetActive(false);
            CallViewEvent(CommandType.LvUpNext);
        }

        public void StartTitleAnimation()
        {
            _battleStartAnim.SetText(DataSystem.GetText(20411));
            _battleStartAnim.StartAnim(false, 0, () => CallViewEvent(CommandType.EndAnimation));
            _battleStartAnim.gameObject.SetActive(true);
            _animationBusy = true;
        }

        public void StartResultAnimation()
        {
            _battleStartAnim.SetText(DataSystem.GetText(20410));
            _battleStartAnim.StartAnim(false, 0, () => CallViewEvent(CommandType.EndAnimation));
            _battleStartAnim.gameObject.SetActive(true);
            _animationBusy = true;
        }

        private void InitializeStatusList()
        {
            statusList.Initialize();
            statusList.SetInputHandler(InputKeyType.Decide,CallLvUpNext);
            AddViewActives(strategyActorList);
        }

        public void ShowLvUpActor(ActorInfo actorInfo,List<ListData> status)
        {
            lvUpStatusButton.gameObject.SetActive(true);
            actorInfoComponent.gameObject.SetActive(true);
            actorInfoComponent.Clear();
            actorInfoComponent.UpdateInfo(actorInfo,null);

            var rect = actorInfoComponent.gameObject.GetComponent<RectTransform>();
            rect.localPosition = new Vector3(0,0,0);
            actorInfoComponent.MainThumb.DOFade(0,0);

            BaseAnimation.MoveAndFade(rect,actorInfoComponent.MainThumb,24,1);

            HelpWindow.SetInputInfo("LEVELUP");
            statusList.gameObject.SetActive(true);
            statusList.SetData(status);
            SetActivate(statusList);
        }

        private void InitializeCommandList()
        {
            commandList.Initialize();
            AddViewActives(commandList);
            commandList.SetInputHandler(InputKeyType.Decide,CallResultCommand);
            commandList.gameObject.SetActive(false);
        }

        private void InitializeLearnSkillList()
        {
            alcanaSelectList.Initialize();
            alcanaSelectList.SetInputHandler(InputKeyType.Decide,() =>
            {
                if (LearnSelectSkillInfo() != null)
                {
                    CallViewEvent(CommandType.SelectLearnSkillList,LearnSelectSkillInfo());
                }
            });
            AddViewActives(alcanaSelectList);
            alcanaSelectList.Hide();
        }

        public void SetTitle()
        {
            title.SetText(DataSystem.GetText(20410));
        }

        public void SetHelpWindow()
        {
            HelpWindow.SetInputInfo("");
            HelpWindow.SetHelpText(DataSystem.GetText(20020));
        }

        public void InitResultList(List<ListData> confirmCommands)
        {
            strategyResultList.Initialize();
            AddViewActives(strategyResultList);
            strategyResultList.gameObject.SetActive(false);
            strategyResultCanvasGroup.alpha = 0;

            commandList.SetData(confirmCommands);
            commandList.UpdateSelectIndex(0);
        }

        private void CallEndAnimation()
        {
            CallViewEvent(CommandType.EndAnimation);
        }

        public void ShowResultList(List<ListData> getItemInfos,string saveHuman,string claerStageNum,string rankScore,string partyEvaluate = null,string attackPer = null,string defeatedCount = null)
        {
            if (rightParts != null)
            {
                rightParts.SetActive(true);
            }
            strategyResultCanvasGroup.alpha = 1;
            claerStageNumObj?.SetActive(claerStageNum != null);
            rankScoreObj?.SetActive(rankScore != null);
            partyEvaluateObj?.SetActive(partyEvaluate != null);
            claerStageNumText?.SetText(claerStageNum);
            rankScoreText?.SetText(rankScore);
            partyEvaluateText?.SetText(partyEvaluate);
            commandList.gameObject.SetActive(true);
            SetActivate(commandList);
            strategyResultList.gameObject.SetActive(true);
            strategyResultList.SetData(getItemInfos);
            strategyResultList.Activate();
            SetHelpInputInfo("STRATEGY");
        }

        private void CallResultCommand()
        {
            var data = commandList.ListItemData<SystemData.CommandData>();
            if (data != null)
            {
                CallViewEvent(CommandType.ResultClose,data);
            }
        }

        public void HideResultList()
        {
            strategyResultList.gameObject.SetActive(false);
        }

        private new void Update()
        {
            if (_animationBusy)
            {
                CheckAnimationBusy();
                return;
            }
            base.Update();
        }

        private void CheckAnimationBusy()
        {
            if (!_battleStartAnim.IsBusy)
            {
                _animationBusy = false;
                CallViewEvent(CommandType.EndLvUpAnimation);
            }
        }

        public void EndShinyEffect()
        {
            strategyActorList.SetShinyReflect(false);
        }

        public void StartGetExpAnimation(Dictionary<ActorInfo,(float,float)> expDict)
        {
            //strategyActorList.StartGetExpAnimation(expDict,CallEndAnimation);
        }

        public void FadeOut()
        {
            backgroundImage.DOFade(0,0.4f);
        }

        public void HideLearnSkillList()
        {
            alcanaSelectList.Hide();
        }

        public void SetLearnSkillInfos(List<ListData> skillInfos)
        {
            SetBackEvent(() => {});
            alcanaSelectList.SetData(skillInfos);
            alcanaSelectList.Show();
            SetActivate(alcanaSelectList);
        }

        public SkillInfo LearnSelectSkillInfo()
        {
            var data = alcanaSelectList.ListItemData<SkillInfo>();
            if (data != null)
            {
                if (data != null && data.Enable)
                {
                    return data;
                }
            }
            return null;
        }

        public void InputHandler(List<InputKeyType> keyTypes, bool pressed)
        {
            if (lvUpStatusButton.gameObject.activeSelf)
            {
                if (keyTypes.Contains(InputKeyType.Decide) || keyTypes.Contains(InputKeyType.Cancel))
                {
                    CallLvUpNext();
                }
            }
        }
    }

    namespace Interlude
    {
        public enum CommandType
        {
            None = 0,
            StartStrategy = 1,
            EndAnimation = 2,
            PopupSkillInfo = 3,
            CallEnemyInfo = 4,
            ResultClose = 5,
            LvUpNext = 7,
            SelectLearnSkillList = 8,
            EndLvUpAnimation = 9,
        }
    }
}