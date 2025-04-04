using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Effekseer;

namespace Ryneus
{
    public class BattlerInfoComponent : MonoBehaviour
    {
        [SerializeField] private ActorInfoComponent actorInfoComponent;
        [SerializeField] private EnemyInfoComponent enemyInfoComponent;
        [SerializeField] private StatusInfoComponent statusInfoComponent;
        [SerializeField] private MakerEffekseerEmitter effekseerEmitter;
        [SerializeField] private _2dxFX_DestroyedFX deathAnimation;
        private GameObject _battleDamageRoot;
        public GameObject BattleDamageRoot => _battleDamageRoot;
        [SerializeField] private GameObject battleDamagePrefab;
        [SerializeField] private BattleStateOverlay battleStateOverlay;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI battlePosition;
        [SerializeField] private TextMeshProUGUI evaluate;
        [SerializeField] private Image additiveFaceThumb;
        [SerializeField] private Material grayScale;
        
        private BattlerInfo _battlerInfo = null;

        private List<BattleDamage> _battleDamages = new ();
        private float _deathAnimation = 0.0f;
        public void UpdateInfo(BattlerInfo battlerInfo)
        {
            _battlerInfo = battlerInfo;
            if (battlerInfo.IsActor || battlerInfo.IsActorView)
            {
                actorInfoComponent.UpdateInfo(battlerInfo.ActorInfo,null);
            } else
            {
                enemyInfoComponent.UpdateInfo(battlerInfo);
            }
            if (evaluate != null)
            {
                evaluate.SetText(battlerInfo.Evaluate().ToString());
            }
            if (additiveFaceThumb != null)
            {
                if (battlerInfo.IsActor || battlerInfo.IsActorView)
                {
                    var handle = ResourceSystem.LoadActorMainFaceSprite(battlerInfo.ActorInfo.Master.ImagePath);
                    if (additiveFaceThumb != null) 
                    {
                        additiveFaceThumb.sprite = handle;
                    }
                } else
                {
                    UpdateMainThumb(battlerInfo.EnemyData.ImagePath,0,0,1.0f);
                }
            }
        }

        private void UpdateMainThumb(string imagePath,int x,int y,float scale)
        {
            var handle = ResourceSystem.LoadEnemySprite(imagePath);
            if (additiveFaceThumb != null)
            {
                additiveFaceThumb.gameObject.SetActive(true);
                var rect = additiveFaceThumb.GetComponent<RectTransform>();
                rect.localPosition = new Vector3(x, y, 0);
                rect.localScale = new Vector3(scale, scale, 1);
                additiveFaceThumb.sprite = handle;
            }
        }

        public void SetDamageRoot(GameObject damageRoot)
        {
            _battleDamageRoot = damageRoot;
            _battleDamageRoot.SetActive(true);
            if (battleStateOverlay != null) 
            {
                battleStateOverlay.Initialize();
            }
        }

        public void SetStatusRoot(GameObject statusRoot)
        {
            if (statusInfoComponent == null)
            {
                return;
            }
            statusInfoComponent.HideStatus();
        }

        public void ChangeHp(int value)
        {
            if (statusInfoComponent == null)
            {
                return;
            }
            statusInfoComponent.UpdateHp(value,_battlerInfo.MaxHp);
        }

        private void ChangeHpAnimation(int fromValue,int toValue)
        {
            if (statusInfoComponent == null)
            {
                return;
            }
            statusInfoComponent.UpdateHp(toValue,_battlerInfo.MaxHp);
            statusInfoComponent.UpdateHpAnimation(fromValue,toValue,_battlerInfo.MaxHp);
        }

        public void ChangeMp(int value)
        {
            if (statusInfoComponent == null)
            {
                return;
            }
            statusInfoComponent.UpdateMp(value,_battlerInfo.MaxMp);
        }

        private void ChangeMpAnimation(int fromValue,int toValue)
        {
            if (statusInfoComponent == null)
            {
                return;
            }
            statusInfoComponent.UpdateMp(toValue,_battlerInfo.MaxMp);
            statusInfoComponent.UpdateMpAnimation(fromValue,toValue,_battlerInfo.MaxMp);
        }

        public void ChangeAtk(int value)
        {
            statusInfoComponent?.UpdateAtk(value);
        }

        public void ChangeDef(int value)
        {
            statusInfoComponent?.UpdateDef(value);
        }

        public void ChangeSpd(int value)
        {
            statusInfoComponent?.UpdateSpd(value);
        }

        public void RefreshStatus()
        {
            if (_battlerInfo == null)
            {
                return;
            }
            if (_battlerInfo.IsActor || _battlerInfo.IsActorView)
            {
                actorInfoComponent.UpdateInfo(_battlerInfo.ActorInfo,null);
                actorInfoComponent.SetAwakeMode(_battlerInfo.IsState(StateType.Demigod));
            } else
            {
                enemyInfoComponent.UpdateInfo(_battlerInfo);
            }
            ChangeHp(_battlerInfo.Hp.Value);
            ChangeMp(_battlerInfo.Mp.Value);
            ChangeAtk(_battlerInfo.CurrentAtk(false));
            ChangeDef(_battlerInfo.CurrentDef(false));
            ChangeSpd(_battlerInfo.CurrentSpd(false));
            if (battlePosition != null)
            {
                var textId = _battlerInfo.LineIndex == LineType.Front ? 2012 : 2013;
                battlePosition.text = DataSystem.GetText(textId);
            }
            if (battleStateOverlay != null) 
            {
                battleStateOverlay.SetStates(_battlerInfo.IconStateInfos());
            }
        }
        
        public void ShowStatus()
        {
            if (statusInfoComponent != null)
            {
                statusInfoComponent.ShowStatus();
            }
            battleStateOverlay?.gameObject.SetActive(true);
        }

        public void HideStatus()
        {
            if (statusInfoComponent != null)
            {
                statusInfoComponent.HideStatus();
            }
            battleStateOverlay?.gameObject.SetActive(false);
        }

        private BattleDamage CreatePrefab()
        {
            var prefab = Instantiate(battleDamagePrefab);
            prefab.transform.SetParent(_battleDamageRoot.transform, false);
            return prefab.GetComponent<BattleDamage>();
        }

        public void ClearDamagePopup()
        {
            foreach ( Transform n in _battleDamageRoot.transform )
            {
                //Destroy(n.gameObject);
            }
            _battleDamages.Clear();
        }

        public void StartDamage(DamageType damageType,int value,bool needPopupDelay)
        {
            var battleDamage = CreatePrefab();
            int delayCount = _battleDamages.Count;
            if (needPopupDelay == false)
            {
                delayCount = 0;
            }
            battleDamage.StartDamage(damageType,value,() => 
            {
                if (_battleDamages.Contains(battleDamage))
                {
                    _battleDamages.Remove(battleDamage);
                }
                if (battleDamage != null)
                {
                    Destroy(battleDamage.gameObject);
                }
            },delayCount);
            _battleDamages.Add(battleDamage);
            if (damageType == DamageType.HpDamage || damageType == DamageType.HpCritical)
            {
                ChangeHpAnimation(_battlerInfo.Hp.Value,value * -1 + _battlerInfo.Hp.Value);
            }
            if (damageType == DamageType.MpDamage)
            {
                ChangeMpAnimation(_battlerInfo.Mp.Value,value * -1 + _battlerInfo.Mp.Value);
            }
        }

        public void StartBlink()
        {
            var image = BattleImage();
            if (image == null) return;
            DOTween.Sequence()
                .Append(image.DOFade(0f, 0.05f))
                .Append(image.DOFade(1f, 0.05f))
                .SetLoops(3);
        }

        public void StartHeal(DamageType damageType,int value,bool needPopupDelay)
        {
            var battleDamage = CreatePrefab();
            int delayCount = _battleDamages.Count;
            if (needPopupDelay == false)
            {
                delayCount = 0;
            }
            battleDamage.StartHeal(damageType,value,() => 
            {
                if (_battleDamages.Contains(battleDamage))
                {
                    _battleDamages.Remove(battleDamage);
                }
                if (battleDamage != null)
                {
                    Destroy(battleDamage.gameObject);
                }
            },delayCount);
            _battleDamages.Add(battleDamage);
            if (damageType == DamageType.HpHeal)
            {
                ChangeHpAnimation(_battlerInfo.Hp.Value,value + _battlerInfo.Hp.Value);
            } else
            if (damageType == DamageType.MpHeal)
            {
                ChangeMpAnimation(_battlerInfo.Mp.Value,value + _battlerInfo.Mp.Value);
            }
        }

        public void StartStatePopup(DamageType damageType,string stateName)
        {
            var battleDamage = CreatePrefab();
            battleDamage.StartStatePopup(damageType,stateName,_battleDamages.Count,() => 
            {
                if (_battleDamages.Contains(battleDamage))
                {
                    _battleDamages.Remove(battleDamage);
                }
                if (battleDamage != null)
                {
                    Destroy(battleDamage.gameObject);
                }
            });
            _battleDamages.Add(battleDamage);
        }

        public void StartDeathAnimation()
        {
            if (grayScale != null)
            {
                if (_battlerInfo.IsActorView)
                {
                    actorInfoComponent.FaceThumb.material = new Material(grayScale);
                } else
                {
                    deathAnimation.enabled = true;
                    _deathAnimation = 0.01f;
                    //enemyInfoComponent.MainThumb.material = new Material(grayScale);
                }
            }
        }

        public void StartAliveAnimation()
        {
            if (grayScale != null)
            {
                if (_battlerInfo.IsActorView)
                {
                    actorInfoComponent.FaceThumb.material = null;
                } else
                {
                    enemyInfoComponent.MainThumb.material = null;
                }
            }
        }
        
        public void StartAnimation(EffekseerEffectAsset effectAsset,AnimationPosition animationPosition,float animationScale = 1.0f,float animationSpeed = 1.0f)
        {
            if (effectAsset == null)
            { 
                effekseerEmitter.Stop();
                return;
            } 
            var image = BattleImage();
            if (image == null) return;
            var imageRect = image.gameObject.GetComponent<RectTransform>();
            var effectRect = effekseerEmitter.gameObject.GetComponent<RectTransform>();
            var y = 0;
            if (_battlerInfo.IsActorView)
            {
                if (animationPosition == AnimationPosition.Center)
                {
                    effectRect.localPosition = new Vector2(0,0);
                } else
                if (animationPosition == AnimationPosition.Down)
                {
                    effectRect.localPosition = new Vector2(0,-imageRect.sizeDelta.y);
                }
            } else
            {
                if (animationPosition == AnimationPosition.Center)
                {
                    effectRect.localPosition = new Vector2(0,imageRect.sizeDelta.y / 2);
                } else
                if (animationPosition == AnimationPosition.Down)
                {
                    effectRect.localPosition = new Vector2(0,imageRect.sizeDelta.y / 2 - 48);
                }
            }
            effectRect.localScale = new Vector3(animationScale,animationScale,animationScale);
            effekseerEmitter.enabled = true;
            effekseerEmitter.Stop();
            effekseerEmitter.speed = animationSpeed;
            effekseerEmitter.Play(effectAsset);
        }

        public void SetThumbAlpha(bool isSelectable)
        {
            var image = BattleImage();
            if (image == null) return;
            float alpha = isSelectable == true ? 1 : 0.25f;
            if (!_battlerInfo.IsAlive() && !_battlerInfo.IsActor)
            {
                alpha = 0;
            }
            canvasGroup.alpha = alpha;
            //image.color = new Color(255,255,255,alpha);
            //effekseerEmitter.enabled = isSelectable;
        }

        public void SetActiveStatus(bool isSelectable)
        {
            if (statusInfoComponent == null)
            {
                return;
            }
            if (!_battlerInfo.IsActor)
            {
                if (isSelectable)
                {
                    statusInfoComponent.ShowStatus();
                } else
                {
                    statusInfoComponent.HideStatus();
                }
            }
        }

        public void HideEnemyStateOverlay()
        {
            if (!_battlerInfo.IsActor)
            {
                battleStateOverlay?.HideStateOverlay();
            }
        }

        public void ShowStateOverlay()
        {
            battleStateOverlay?.ShowStateOverlay();
        }

        public void HideStateOverlay()
        {
            battleStateOverlay?.HideStateOverlay();
        }

        private void Update() 
        {
            UpdateDeathAnimation();
        }
        
        private void UpdateDeathAnimation()
        {
            if (deathAnimation == null) return;
            if (_deathAnimation <= 0) return;

            deathAnimation.Destroyed = _deathAnimation;
            if (_deathAnimation >= 1)
            {
                _deathAnimation = 0;
                canvasGroup.alpha = 0;
                deathAnimation.enabled = false;
                deathAnimation.Destroyed = 0;
            } 
            else
            {
                _deathAnimation += 0.01f;
            }
        }
        
        private Image BattleImage()
        {
            if (_battlerInfo == null) return null;
            Image image;
            if (_battlerInfo.IsActor)
            {
                if (_battlerInfo.IsAwaken)
                {
                    image = actorInfoComponent.FaceThumb;
                    //image = actorInfoComponent.AwakenFaceThumb;
                } else
                {
                    image = actorInfoComponent.FaceThumb;
                }
            } else
            {
                image = enemyInfoComponent.MainThumb;
            }
            return image;
        }

        public void DisableEmitter()
        {
            effekseerEmitter.Stop();
            effekseerEmitter.enabled = false;
        }

        public void SetActiveBeforeSkillThumb(bool isActive)
        {
            if (additiveFaceThumb != null)
            {
                additiveFaceThumb.gameObject.SetActive(isActive);
                if (isActive)
                {
                    additiveFaceThumb.DOFade(1f, 0);
                    DOTween.Sequence()
                        .Append(additiveFaceThumb.DOFade(0f, 0.4f))
                        .OnComplete(() => 
                        {
                            additiveFaceThumb.gameObject.SetActive(false);
                        });
                }
            }
        }

        public void UpdateEnemyImageNativeSize()
        {
            enemyInfoComponent.UpdateNativeSize();
        }

        public void Clear()
        {
            _battlerInfo = null;
            enemyInfoComponent?.Clear();
            actorInfoComponent?.Clear();
            battlePosition?.SetText("");
        }
    }
}