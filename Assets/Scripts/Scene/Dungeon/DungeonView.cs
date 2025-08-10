using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ryneus.Dungeon;
using TMPro;
using DG.Tweening;

namespace Ryneus
{
    public class DungeonView : BaseView, IInputHandlerEvent
    {
        [SerializeField] private Ariadne.DungeonViewManager dungeonViewManager = null;
        [SerializeField] private Ariadne.MoveController moveController = null;
        public Ariadne.MoveController MoveController => moveController;
        [SerializeField] private BattleBattlerList partyUnitList = null;
        [SerializeField] private StageInfoComponent stageInfoComponent = null;
        [SerializeField] private PartyInfoComponent partyInfoComponent = null;
        [SerializeField] private OnOffButton formationButton = null;
        [SerializeField] private InputInfoComponent formationInpurKey = null;
        [SerializeField] private OnOffButton healButton = null;
        [SerializeField] private InputInfoComponent healInpurKey = null;
        [SerializeField] private OnOffButton decideButton = null;
        [SerializeField] private InputInfoComponent decideInpurKey = null;
        [SerializeField] private AlcanaInfoComponent alcanaInfoComponent;
        [SerializeField] private OnOffButton alcanaInfoButton;
        [SerializeField] private TextMeshProUGUI minusVictoryBonus;
        [SerializeField] private TextMeshProUGUI minusEvaluate;
        [SerializeField] private InputInfoComponent sideMenuInput = null;
        private readonly Dictionary<int,BattlerInfoComponent> _battlerComps = new();
        private List<Sequence> _sequences = new();
        //[SerializeField] private OnOffButton healButton = null;

        public override void Initialize()
        {
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.Dungeon);
            InitializePartyUnitList();
            if (decideButton != null)
            {
                decideButton.OnClickAddListener(() =>
                {
                    CallViewEvent(CommandType.DecideDirectEvent);
                });
            }
            if (decideInpurKey != null)
            {
                decideInpurKey.UpdateGuideIcon(InputKeyType.Start);
            }
            if (healButton != null)
            {
                healButton.OnClickAddListener(() =>
                {
                    CallViewEvent(CommandType.Heal);
                });
            }
            if (healInpurKey != null)
            {
                healInpurKey.UpdateGuideIcon(InputKeyType.SideRight1);
            }
            if (formationButton != null)
            {
                formationButton.OnClickAddListener(() =>
                {
                    CallFormation();
                });
            }
            if (formationInpurKey != null)
            {
                formationInpurKey.UpdateGuideIcon(InputKeyType.Option1);
            }
            if (alcanaInfoButton != null)
            {
                alcanaInfoButton.OnClickAddListener(() => CallViewEvent(CommandType.Aritifact));
            }
            SideMenuButton.OnClickAddListener(() =>
            {
                CallSideMenu();
            });
            sideMenuInput.UpdateGuideIcon(InputKeyType.Option2);
            CommandRefresh();
            _ = new DungeonPresenter(this);
        }

        public void SetupDungeon()
        {
            dungeonViewManager.Initialize();
            dungeonViewManager.SetMoveController(moveController);
            dungeonViewManager.SetMoveEndEvent(() => CallViewEvent(CommandType.MoveEnd));
        }

        private void InitializePartyUnitList()
        {
            partyUnitList.Initialize();
            partyUnitList.SetInputHandler(InputKeyType.Decide,() => CallViewEvent(CommandType.SelectCharacter,partyUnitList.Index));
            partyUnitList.SetInputHandler(InputKeyType.Cancel,() => CallViewEvent(CommandType.EndFormation));
            AddViewActives(partyUnitList);
        }

        public void SetPartyUnitList(List<ListData> listDatas)
        {
            partyUnitList.SetData(listDatas);
            SetActivate(null);
            foreach (var battlerInfo in listDatas)
            {
                var data = (BattlerInfo)battlerInfo.Data;
                if (data.Index.Value > 0)
                {
                    _battlerComps[data.Index.Value] = partyUnitList.GetBattlerInfoComp(data.Index.Value);
                }
            }
        }

        public void UpdatePartyUnitList(List<ListData> listDatas)
        {
            var lastIndex = partyUnitList.Index;
            partyUnitList.SetData(listDatas);
            partyUnitList.UpdateSelectIndex(lastIndex);
        }

        public void UpdateSelectCursor(List<int> targetIndexes)
        {
            partyUnitList.UpdateSelectIndexList(targetIndexes);
        }

        private void CallFormation()
        {
            if (partyUnitList.Active)
            {
                return;
            }
            CallViewEvent(CommandType.Formation);
        }

        private void CallSideMenu()
        {
            if (partyUnitList.Active)
            {
                return;
            }
            CallViewEvent(CommandType.SelectSideMenu);
        }

        public void CommandRefresh()
        {
            stageInfoComponent.UpdateCurrent();
            partyInfoComponent.UpdateCurrentInfo();
            alcanaInfoComponent.UpdateCurrentInfo();
        }

        public void StartFormation()
        {
            SetActivate(partyUnitList);
            partyUnitList.UpdateSelectIndex(0);
        }

        public void EndFormation()
        {
            partyUnitList.UpdateSelectIndex(-1);
            SetActivate(null);
        }

        public void SetHelpWindow()
        {
            HelpWindow.SetHelpText("");
            HelpWindow.SetInputInfo("");
        }

        public void InputHandler(List<InputKeyType> keyTypes, bool pressed)
        {
            if (InputSystem.GetInputDate(InputKeyType.SideRight1).IsDownTrigger())
            {
                CallViewEvent(CommandType.Heal);
            }
            if (InputSystem.GetInputDate(InputKeyType.SideLeft1).IsDownTrigger())
            {
                CallViewEvent(CommandType.Aritifact);
            }
            if (keyTypes.Contains(InputKeyType.Option2))
            {
                CallSideMenu();
            }else
            if (keyTypes.Contains(InputKeyType.Option1))
            {
                CallFormation();
            } else
            if (keyTypes.Contains(InputKeyType.Decide))
            {
                if (decideButton.gameObject.activeSelf && !partyUnitList.Active)
                {
                    CallViewEvent(CommandType.DecideDirectEvent);
                }
            }
            moveController.UpdateKey(keyTypes);
        }

        public void SetActiveDisplayEventKey(bool isActive)
        {
            if (decideButton == null)
            {
                return;
            }
            decideButton.gameObject.SetActive(isActive);
        }

        public void SetActiveHealButton(bool isActive)
        {
            if (healButton == null)
            {
                return;
            }
            healButton.gameObject.SetActive(isActive);
        }

        public void SetActiveFormationButton(bool isActive)
        {
            if (formationButton == null)
            {
                return;
            }
            formationButton.gameObject.SetActive(isActive);
        }

        public void SetActiveStageInfo(bool isActive)
        {
            if (stageInfoComponent == null)
            {
                return;
            }
            stageInfoComponent.gameObject.SetActive(isActive);
        }

        public void ChangeSkybox(Material material)
        {
            RenderSettings.skybox = material;
        }

        public void StartDamage(int value)
        {
            foreach (var battlerComp in _battlerComps)
            {
                StartDamage(battlerComp.Key, DamageType.HpDamage, value);
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

        public void StartHeal(int value)
        {
            foreach (var battlerComp in _battlerComps)
            {
                StartHeal(battlerComp.Key, DamageType.HpHeal, value);
            }
        }

        private void StartHeal(int targetIndex,DamageType damageType, int value, bool needPopupDelay = true)
        {
            if (!_battlerComps.ContainsKey(targetIndex))
            {
                return;
            }
            _battlerComps[targetIndex].StartHeal(damageType,value,needPopupDelay);
        }

        public void MinusVictoryBonus(float minus)
        {
            SeekTweens();
            var lastY = 334;//minusVictoryBonus.transform.localPosition.y;
            minusVictoryBonus.transform.DOLocalMoveY(lastY, 0);
            minusVictoryBonus.SetText("-" + minus.ToString());
            minusVictoryBonus.DOFade(1f, 0);
            var sequence = DOTween.Sequence()
                .Append(minusVictoryBonus.transform.DOLocalMoveY(lastY - 24, 0.8f))
                .Append(minusVictoryBonus.transform.DOLocalMoveY(lastY - 24, 2f))
                .Append(minusVictoryBonus.DOFade(0f, 0.8f))
                .SetEase(Ease.OutQuart)
                .OnComplete(() =>
                {
                    minusVictoryBonus.transform.DOLocalMoveY(lastY, 0);
                });
            _sequences.Add(sequence);
        }

        public void MinusEvaluate(int minus)
        {
            SeekTweens();
            var lastY = 224;//minusEvaluate.transform.localPosition.y;
            minusEvaluate.transform.DOLocalMoveY(lastY, 0);
            minusEvaluate.SetText("-" + minus.ToString());
            minusEvaluate.DOFade(1f, 0);
            var sequence = DOTween.Sequence()
                .Append(minusEvaluate.transform.DOLocalMoveY(lastY - 24, 0.8f))
                .Append(minusEvaluate.transform.DOLocalMoveY(lastY - 24, 2f))
                .Append(minusEvaluate.DOFade(0f, 0.8f))
                .SetEase(Ease.OutQuart)
                .OnComplete(() =>
                {
                    minusEvaluate.transform.DOLocalMoveY(lastY, 0);
                });
            _sequences.Add(sequence);
        }

        private void SeekTweens()
        {
            foreach (var sequences in _sequences)
            {
                sequences.Complete();
            }
        }

        void OnDestroy()
        {
            foreach (var sequences in _sequences)
            {
                sequences.Kill();
            }
        }
    }

    namespace Dungeon
    {
        public enum CommandType
        {
            None = 0,
            MoveEnd,
            DecideDirectEvent,
            Heal,
            Formation,
            SelectCharacter,
            EndFormation,
            Aritifact,
            SelectSideMenu
        }
    }
}