using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Ryneus.Strategy;

namespace Ryneus
{
    public class StrategyView : BaseView, IInputHandlerEvent
    {
        [SerializeField] private Image backgroundImage = null;
        [SerializeField] private StrategyActorList strategyActorList = null;
        [SerializeField] private CanvasGroup strategyResultCanvasGroup = null;
        [SerializeField] private BaseList strategyResultList = null;
        public bool StrategyResultListActive => strategyResultList.gameObject.activeSelf;
        [SerializeField] private BaseList commandList = null;
        [SerializeField] private MagicList alcanaSelectList = null;
        [SerializeField] private TextMeshProUGUI title = null;
        [SerializeField] private GameObject animRoot = null;
        [SerializeField] private GameObject animPrefab = null;
        [SerializeField] private BattleScoreComponent battleScoreComponent = null;

        private BattleStartAnim _battleStartAnim = null;
        private bool _animationBusy = false;

        public override void Initialize()
        {
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.Strategy);
            InitializeActorList();
            //InitializeStatusList();
            InitializeCommandList();
            InitializeLearnSkillList();

            GameObject prefab = Instantiate(animPrefab);
            prefab.transform.SetParent(animRoot.transform, false);
            _battleStartAnim = prefab.GetComponent<BattleStartAnim>();
            _battleStartAnim.gameObject.SetActive(false);


            strategyResultCanvasGroup.alpha = 0;
            battleScoreComponent?.UpdateEmpty();
            _ = new StrategyPresenter(this);
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

        private void InitializeCommandList()
        {
            commandList.Initialize();
            AddViewActives(commandList);
            commandList.SetInputHandler(InputKeyType.Decide, CallResultCommand);
            commandList.gameObject.SetActive(false);
        }

        private void InitializeLearnSkillList()
        {
            alcanaSelectList.Initialize();
            alcanaSelectList.SetInputHandler(InputKeyType.Decide,() =>
            {
                if (LearnSelectSkillInfo() != null)
                {
                    CallViewEvent(CommandType.SelectLearnSkillList, LearnSelectSkillInfo());
                }
            });
            AddViewActives(alcanaSelectList);
            alcanaSelectList.Hide();
        }

        public void SetTitle(string text)
        {
            title.SetText(text);
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

        public void ShowResultList(List<ListData> getItemInfos, BattleScore score)
        {
            strategyResultCanvasGroup.alpha = 1;
            battleScoreComponent.UpdateScore(score);
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
                CallViewEvent(CommandType.ResultClose, data);
            }
        }

        public void HideResultList()
        {
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

        public void StartGetExpAnimation(List<StrategyActorLevelUpInfo> levelUpInfos)
        {
            strategyActorList.StartGetExpAnimation(levelUpInfos,CallEndAnimation);
        }

        public void FadeOut()
        {
            backgroundImage.DOFade(0, 0.4f);
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
        }
    }
}


namespace Ryneus
{
    namespace Strategy
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