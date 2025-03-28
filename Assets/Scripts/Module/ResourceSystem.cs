﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using Effekseer;
using UnityEngine.U2D;

namespace Ryneus
{
    public class ResourceSystem : MonoBehaviour
    {
        private static GameObject _lastScene = null;
        private static List<Object> _lastLoadAssets = new List<Object>();

        private static string _bgmPath = "Audios/BGM/";
        private static string _bgsPath = "Audios/BGS/";
        private static string _sePath = "Audios/Se/";

        public static void ReleaseScene()
        {
            if (_lastScene != null)
            {
                Addressables.ReleaseInstance(_lastScene);
                _lastScene = null;
            }
        }

        public static async UniTask<T> LoadAsset<T>(string address)
        {
            var handle = await Addressables.LoadAssetAsync<T>(address).Task;
            _lastLoadAssets.Add(handle as Object);
            return handle;
        }

        public static void ReleaseAssets()
        {
            foreach (var lastLoadAssets in _lastLoadAssets)
            {
                Addressables.Release(lastLoadAssets);
            }
            _lastLoadAssets.Clear();
        }

        public async static UniTask<List<AudioClip>>LoadBGMAsset(string bgmKey)
        {    
            var bGMData = DataSystem.BGM.Find(a => a.Key == bgmKey);
            var data = new List<string>();
            if (bGMData.CrossFade != null && bGMData.CrossFade != "")
            {
                data.Add(_bgmPath + bGMData.FileName + "");
                data.Add(_bgmPath + DataSystem.BGM.Find(a => a.Key == bGMData.CrossFade).FileName + "");
            } else
            if (bGMData.Loop)
            {
                data.Add(_bgmPath + bGMData.FileName + "_intro");
                data.Add(_bgmPath + bGMData.FileName + "_loop");
            } else{
                data.Add(_bgmPath + bGMData.FileName + "");
            }
            AudioClip result2 = null;
            var result1 = await LoadAssetResources<AudioClip>(data[0]);
            if (bGMData.Loop || (bGMData.CrossFade != null && bGMData.CrossFade != ""))
            {
                result2 = await LoadAssetResources<AudioClip>(data[1]);
            }
            return new List<AudioClip>()
            {
                result1,result2
            };
        }

        public async static UniTask<AudioClip>LoadBGSAsset(string fileName)
        {    
            var data = _bgsPath + fileName;
            AudioClip result = await LoadAssetResources<AudioClip>(data);
            return result;
        }

        public async static UniTask<AudioClip>LoadSeAsset(string fileName)
        {    
            var data = _sePath + fileName;
            AudioClip result = await LoadAssetResources<AudioClip>(data);
            return result;
        }

        public static async UniTask<AudioClip> LoadAssetResources<T>(string address)
        {
            var handle = Resources.LoadAsync<AudioClip>(address);
            await handle;
            return handle.asset as AudioClip;
        }

        static string ActorTexturePath => "Texture/Character/Actors/";
        public static string SystemTexturePath => "Texture/System/";
        public static string PrefabPath => "Prefabs/";

        public static AudioClip LoadSeAudio(string path)
        {
            return LoadResource<AudioClip>("Audios/SE/" + path);
        } 

        public static T LoadResource<T>(string path) where T : Object
        {
            return Resources.Load<T>(path);
        } 

        public static T[] LoadResources<T>(string path) where T : Object
        {
            return Resources.LoadAll<T>(path);
        } 
        
        public static Sprite LoadActorMainSprite(string path)
        {
            return LoadResource<Sprite>(ActorTexturePath + path + "/Main");
        }

        public static Sprite LoadActorMainFaceSprite(string path)
        {
            return LoadResource<Sprite>(ActorTexturePath + path + "/MainFace");
        }

        public static Sprite LoadActorCutinSprite(string path)
        {
            return LoadResource<Sprite>(ActorTexturePath + path + "/Cutin");
        }

        public static Sprite LoadActorAwakenSprite(string path)
        {
            return LoadResource<Sprite>(ActorTexturePath + path + "/Awaken");
        }

        public static Sprite LoadActorAwakenFaceSprite(string path)
        {
            return LoadResource<Sprite>(ActorTexturePath + path + "/AwakenFace");
        }

        public static Sprite LoadActorClipSprite(string path)
        {
            return LoadResource<Sprite>(ActorTexturePath + path + "/Clip");
        }

        public static GameObject LoadActor3DModel(string path)
        {
            return LoadResource<GameObject>("3DModels/" + path + "/" + path);
        }

        public static GameObject LoadEnemy3DModel(string path)
        {
            return LoadResource<GameObject>("3DModels/Enemy/" + path);
        }

        public static Sprite LoadEnemySprite(string enemyImage)
        {
            return LoadResource<Sprite>("Texture/Character/Enemies/" + enemyImage);
        }

        public static Sprite LoadBackGround(string fileName)
        {
            return  LoadResource<Sprite>("Texture/BG/" + fileName);
        }

        public static GameObject LoadBattleBackGround(string fileName)
        {
            return  LoadResource<GameObject>("Prefabs/BG/" + fileName);
        }

        public static EffekseerEffectAsset LoadResourceEffect(string path)
        {
            return LoadResource<EffekseerEffectAsset>("Animations/" + path);
        } 

        public static SpriteAtlas LoadSpellIcons()
        {
            return LoadResource<SpriteAtlas>("Texture/SpellIcons");
        }

        public static SpriteAtlas LoadUnitTypeIcons()
        {
            return LoadResource<SpriteAtlas>("Texture/UnitType");
        }

        public static SpriteAtlas LoadUnitTypeBackIcons()
        {
            return LoadResource<SpriteAtlas>("Texture/UnitTypeBack");
        }

        public static SpriteAtlas LoadIcons()
        {
            return LoadResource<SpriteAtlas>("Texture/Icons");
        } 

        public static Sprite LoadGuideSprite(string path)
        {
            return LoadResource<Sprite>("Texture/Guide/" + path);
        }
    }

    public static class ResourceRequestExtenion
    {
        // Resources.LoadAsyncの戻り値であるResourceRequestにGetAwaiter()を追加する
        public static TaskAwaiter<Object> GetAwaiter(this ResourceRequest resourceRequest)
        {
            var tcs = new TaskCompletionSource<Object>();
            resourceRequest.completed += operation =>
            {
                // ロードが終わった時点でTaskCompletionSource.TrySetResult
                tcs.TrySetResult(resourceRequest.asset);
            };

            // TaskCompletionSource.Task.GetAwaiter()を返す
            return tcs.Task.GetAwaiter();
        }
    }

    public enum Scene
    {
        None,
        Base,
        Map,
        Boot,
        Title,
        NameEntry,
        MainMenu,
        Battle,
        Status,
        Tactics,
        Strategy,
        Slot,
        FastBattle,
        Result,
    }
}