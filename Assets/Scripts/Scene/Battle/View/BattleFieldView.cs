using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using Effekseer;
using Cysharp.Threading.Tasks;
using System;

namespace Ryneus
{
    public class BattleFieldView : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private List<GameObject> actorPositions;
        [SerializeField] private List<GameObject> enemyPositions;
        [SerializeField] private List<GameObject> actorDamagePositions;
        [SerializeField] private List<GameObject> enemyDamagePositions;
        private Dictionary<int, CharacterAnimationImages> _battlers = new();
        private Dictionary<int, BattlerInfoComponent> _battlerInfoComponents = new();
        public Dictionary<int, BattlerInfoComponent> BattlerInfoComponents => _battlerInfoComponents;
        private bool _busy = false;
        public bool Busy => _busy;

        private List<Sequence> _sequences = new();

        private float _zoomValue = 1.2f;
        private float _rangeWidth = 480;
        private float _moveDuration => 0.4f / GameSystem.OptionData.BattleSpeed;

        public void Intialize()
        {

        }

        public async Task SetFieldActors(List<BattlerInfo> battlerInfos, Action<BattlerInfo> decideEvent, Action<BattlerInfo> selectEvent)
        {
            await SetActorInfo(battlerInfos.FindAll(a => a.IsActor), decideEvent, selectEvent);
        }

        public async Task SetFieldEnemies(List<BattlerInfo> battlerInfos, Action<BattlerInfo> decideEvent, Action<BattlerInfo> selectEvent)
        {
            MoveCameraEnemies();
            ZoomIn();
            await SetEnemyInfo(battlerInfos.FindAll(a => !a.IsActor), decideEvent, selectEvent);
            SetAnimationBattlerAll(AnimationState.BeforeStart);
        }

        public async Task UpdateFieldMembers(List<BattlerInfo> battlerInfos, Action<BattlerInfo> decideEvent, Action<BattlerInfo> selectEvent)
        {
            foreach (var battlers in _battlers)
            {
                Destroy(battlers.Value.gameObject);
            }
            _battlers.Clear();
            _battlerInfoComponents.Clear();
            await SetActorInfo(battlerInfos.FindAll(a => a.IsActor), decideEvent, selectEvent);
            await SetEnemyInfo(battlerInfos.FindAll(a => !a.IsActor), decideEvent, selectEvent);
            SetAnimationBattlerAll(AnimationState.BeforeStart);
        }

        public async Task SetActorInfo(List<BattlerInfo> battlerInfos, Action<BattlerInfo> decideEvent, Action<BattlerInfo> selectEvent)
        {
            for (int i = 0; i < battlerInfos.Count; i++)
            {
                var asset = await ResourceSystem.LoadActorFieldBattler(battlerInfos[i].ActorInfo.Master.ImagePath);
                var prefab = Instantiate(asset);
                prefab.transform.SetParent(actorPositions[battlerInfos[i].Index.Value - 1].transform, false);
                var comp = prefab.GetComponent<CharacterAnimationImages>();
                comp.Initilize(decideEvent, selectEvent);
                comp.UpdateInfo(battlerInfos[i]);
                comp.ReplaceDamageRoot(actorDamagePositions[i]);
                _battlers[battlerInfos[i].Index.Value] = comp;
                _battlerInfoComponents[battlerInfos[i].Index.Value] = prefab.GetComponentInChildren<BattlerInfoComponent>();
            }
        }

        public async Task SetEnemyInfo(List<BattlerInfo> battlerInfos, Action<BattlerInfo> decideEvent, Action<BattlerInfo> selectEvent)
        {
            for (int i = 0; i < battlerInfos.Count; i++)
            {
                var asset = await ResourceSystem.LoadEnemyFieldBattler(battlerInfos[i].EnemyData.ImagePath);
                var prefab = Instantiate(asset);
                prefab.transform.SetParent(enemyPositions[battlerInfos[i].Index.Value - 101].transform, false);
                var comp = prefab.GetComponent<CharacterAnimationImages>();
                comp.Initilize(decideEvent, selectEvent);
                comp.UpdateInfo(battlerInfos[i]);
                comp.ReplaceDamageRoot(enemyDamagePositions[i]);
                _battlers[battlerInfos[i].Index.Value] = comp;
                _battlerInfoComponents[battlerInfos[i].Index.Value] = prefab.GetComponentInChildren<BattlerInfoComponent>();
            }
        }

        public void SetAnimationBattlerAll(AnimationState animationState)
        {
            foreach (var battler in _battlers)
            {
                if (animationState == AnimationState.Idle && battler.Value.IsStateDeath)
                {
                    continue;
                }
                battler.Value.SetAnimationState(animationState);
            }
            if (animationState == AnimationState.Start)
            {
                MoveCameraActors();
            }
            if (animationState == AnimationState.Idle)
            {
                MoveCameraCenter();
                ZoomOut();
            }
        }

        public void MoveCameraActors()
        {
            _busy = true;
            var sequence = DOTween.Sequence()
                .Append(cameraTransform.DOLocalMoveX(_rangeWidth, _moveDuration))
                //.SetEase(Ease.OutQuart)
                .OnComplete(() =>
                {
                    _busy = false;
                });
            _sequences.Add(sequence);
        }

        public void MoveCameraEnemies()
        {
            var sequence = DOTween.Sequence()
                .Append(cameraTransform.DOLocalMoveX(-_rangeWidth, _moveDuration));
            _sequences.Add(sequence);
        }

        public void MoveCameraCenter()
        {
            var sequence = DOTween.Sequence()
                .Append(cameraTransform.DOLocalMoveX(0, _moveDuration));
            _sequences.Add(sequence);
        }

        public void ZoomIn()
        {
            _busy = true;
            var sequence = DOTween.Sequence()
                .Append(cameraTransform.DOScale(_zoomValue, _moveDuration))
                //.SetEase(Ease.OutQuart)
                .OnComplete(() =>
                {
                    _busy = false;
                });
            _sequences.Add(sequence);
        }

        public void ZoomOut()
        {
            _busy = true;
            var sequence = DOTween.Sequence()
                .Append(cameraTransform.DOScale(1f, _moveDuration))
                //.SetEase(Ease.OutQuart)
                .OnComplete(() =>
                {
                    _busy = false;
                });
            _sequences.Add(sequence);
        }

        private void KillSequences()
        {
            AnimationUtility.Clear(_sequences);
            _sequences.Clear();
        }

        public async Task SetStartActorMagic(int battlerInfoIndex, bool isActor)
        {
            if (!_battlers.ContainsKey(battlerInfoIndex))
            {
                return;
            }
            KillSequences();
            if (isActor)
            {
                MoveCameraActors();
            }
            else
            {
                MoveCameraEnemies();
            }
            ZoomIn();
            _battlers[battlerInfoIndex].SetAnimationState(AnimationState.Magic);
            await UniTask.WaitUntil(() => !_busy);
        }

        public void SetHit(int battlerInfoIndex, bool isActor)
        {
            if (!_battlers.ContainsKey(battlerInfoIndex))
            {
                return;
            }
            KillSequences();
            if (isActor)
            {
                MoveCameraActors();
            }
            else
            {
                MoveCameraEnemies();
            }
            ZoomIn();
            _battlers[battlerInfoIndex].SetAnimationState(AnimationState.Hit);
        }

        public async Task StartAnimation(int battlerInfoIndex, EffekseerEffectAsset effectAsset, AnimationPosition animationPosition, float animationScale = 1.0f, float animationSpeed = 1.0f, bool soundPlay = true)
        {
            if (!_battlers.ContainsKey(battlerInfoIndex))
            {
                return;
            }

            KillSequences();
            if (battlerInfoIndex < 100)
            {
                MoveCameraActors();
            }
            else
            {
                MoveCameraEnemies();
            }
            ZoomIn();
            await UniTask.WaitUntil(() => !_busy);
            _battlers[battlerInfoIndex].StartAnimation(effectAsset, animationPosition, animationScale, animationSpeed, soundPlay);
        }

        public void StartDeathAnimation(int battlerInfoIndex)
        {
            if (!_battlers.ContainsKey(battlerInfoIndex))
            {
                return;
            }
            _battlers[battlerInfoIndex].SetAnimationState(AnimationState.Death);
        }

        public void UpdateSelectIndexList(List<int> indexes)
        {
            foreach (var battler in _battlers)
            {
                battler.Value.SetActivecandidateSelect(indexes.Contains(battler.Key));
            }
        }

        public void SetSelectIndexes(int index)
        {
            foreach (var battler in _battlers)
            {
                battler.Value.SetSelectArrow(index == battler.Key);
            }
        }

        public void ClearSelect()
        {
            foreach (var battler in _battlers)
            {
                battler.Value.SetSelectArrow(false);
            }
        }
    }
}
