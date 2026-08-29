using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using Effekseer;
using UnityEngine.U2D;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


namespace Ryneus
{
    public class ResourceSystem
    {
        private static GameObject _lastScene = null;
        private static List<Object> _lastLoadAssets = new();
        private static Dictionary<string, AsyncOperationHandle> _loadAssets = new();

        private static string _bgmPath = "Audios/BGM/";
        private static string _bgsPath = "Audios/BGS/";
        private static string _sePath = "Audios/SE/";

        public static void Initialize()
        {
            _loadAssets.Clear();
        }
/*
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
*/

        public static async UniTask<List<AudioClip>> LoadBGMAsset(string bgmKey)
        {
            var bGMData = DataSystem.GetBGMByKey(bgmKey);
            var data = new List<string>();
            if (bGMData.CrossFade != null && bGMData.CrossFade != "")
            {
                data.Add(_bgmPath + bGMData.FileName + "");
                data.Add(_bgmPath + DataSystem.GetBGMByKey(bGMData.CrossFade).FileName + "");
            }
            else
            if (bGMData.Loop)
            {
                data.Add(_bgmPath + bGMData.FileName + "_intro");
                data.Add(_bgmPath + bGMData.FileName + "_loop");
            }
            else
            {
                data.Add(_bgmPath + bGMData.FileName + "");
            }
            AudioClip result2 = null;
            var result1 = await LoadAsset<AudioClip>(data[0]);
            if (bGMData.Loop || (bGMData.CrossFade != null && bGMData.CrossFade != ""))
            {
                result2 = await LoadAsset<AudioClip>(data[1]);
            }
            return new List<AudioClip>()
            {
                result1,result2
            };
        }

        public static async Task<AudioClip> LoadBGSAsset(string fileName)
        {
            var data = _bgsPath + fileName;
            AudioClip result = await LoadAsset<AudioClip>(data);
            return result;
        }

        public static async Task<AudioClip> LoadSeAsset(string fileName)
        {
            return await LoadAsset<AudioClip>(_sePath + fileName);
        }

        static string ActorTexturePath => "Texture/Character/Actors/";
        public static string SystemTexturePath => "Texture/System/";
        public static string PrefabPath => "Prefabs/";

        public static T LoadResource<T>(string path) where T : Object
        {
            return Resources.Load<T>(path);
        }

        public static T[] LoadResources<T>(string path) where T : Object
        {
            return Resources.LoadAll<T>(path);
        }

        public static T GetAsset<T>(string path) where T : Object
        {
            /*
            if (_loadAssets.ContainsKey(path))
            {
                return _loadAssets[path] as T;
            }
            */
            return null;
        }

        public static void LoadAssetData<T>(string path, System.Action<T> endCall = null) where T : Object
        {
            var task = Addressables.LoadAssetAsync<T>(path);
            _loadAssets[path] = task;
            task.Completed += handle =>
            {
                endCall.Invoke(task.Result);
            };
        }

        public static async Task<T> LoadAsset<T>(string path) where T : Object
        {
            // 既にロード済みの場合はキャッシュから返す
            if (_loadAssets.TryGetValue(path, out var existingHandle))
            {
                return existingHandle.Result as T;
            }
            // 非同期ロードを実行
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(path);
            
            // メインスレッドをフリーズさせずに待つ
            await handle.Task;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _loadAssets[path] = handle; // ハンドルを保存
                return handle.Result;
            }
            return null;
        }

        // メモリ解放
        public static void ReleaseAsset(string path)
        {
            if (_loadAssets.TryGetValue(path, out var handle))
            {
                Addressables.Release(handle);
                _loadAssets.Remove(path);
            }
        }

        public static string ActorMainSpritePath(string path)
        {
            return ActorTexturePath + path + "/Main";
        }

        public static string ActorMainFaceSpritePath(string path)
        {
            return ActorTexturePath + path + "/MainFace";
        }

        public static string ActorCutinSpritePath(string path)
        {
            return ActorTexturePath + path + "/Cutin";
        }

        public static string ActorAwakenSpritePath(string path)
        {
            return ActorTexturePath + path + "/Awaken";
        }

        public static string ActorBattleThumbPath(string path)
        {
            return ActorTexturePath + path + "/MainThumb";
        }

        public static string ActorReliefSpritePath(string path)
        {
            return ActorTexturePath + path + "/Relief";
        }

        public static string ActorAwakenFaceSpritePath(string path)
        {
            return ActorTexturePath + path + "/AwakenFace";
        }

        public static string ActorClipSpritePath(string path)
        {
            return ActorTexturePath + path + "/Clip";
        }

        public static Sprite[] LoadActorAnimation(string path)
        {
            return LoadResources<Sprite>(ActorTexturePath + path);
        }

        public static async Task<GameObject> LoadActorFieldBattler(string path)
        {
            return await LoadAsset<GameObject>("FieldBattler/Actors/FieldBattler_" + path);
        }

        public static async Task<GameObject> LoadEnemyFieldBattler(string path)
        {
            return await LoadAsset<GameObject>("FieldBattler/Enemies/FieldBattler_Enemy");
        }

        public static GameObject LoadActor3DModel(string path)
        {
            return LoadResource<GameObject>("3DModels/" + path + "/" + path);
        }

        public static GameObject LoadEnemy3DModel(string path)
        {
            return LoadResource<GameObject>("3DModels/Enemy/" + path);
        }

        public static string EnemySpritePath(string enemyImage)
        {
            return "Texture/Character/Enemies/" + enemyImage;
        }

        public static async Task<Sprite> LoadBackGround(string fileName)
        {
            return await LoadAsset<Sprite>("Texture/BG/" + fileName);
        }

        public static string BackGroundPath(string fileName)
        {
            return "Texture/BG/" + fileName;
        }

        public static GameObject LoadBattleBackGround(string fileName)
        {
            return LoadResource<GameObject>("Prefabs/BG/" + fileName);
        }

        public static EffekseerEffectAsset LoadResourceEffect(string path)
        {
            return LoadResource<EffekseerEffectAsset>("Animations/" + path);
        }

        public static async Task<EffekseerEffectAsset> LoadResourceEffectAsset(string path)
        {
            return await LoadAsset<EffekseerEffectAsset>("Animations/" + path);
        }

        public static async Task<AudioClip> LoadEffectSeAsset(string fileName)
        {
            return await LoadAsset<AudioClip>("Animations/Sound/" + fileName);
        }

        public static Sprite LoadBuildingSprite(string fileName)
        {
            return LoadResource<Sprite>("Texture/Symbol/" + fileName);
        }

        public static SpriteAtlas LoadSpellIcons()
        {
            return LoadResource<SpriteAtlas>("Texture/SpellIcons");
        }

        public static Sprite[] LoadSpellIconBases()
        {
            return LoadResources<Sprite>("Texture/Buff/IconBases");
        }

        public static Sprite LoadSpellIconBase(AttributeType attributeType)
        {
            var iconIndex = 0;
            switch (attributeType)
            {
                case AttributeType.None:
                    iconIndex = 33;
                    break;
                case AttributeType.Fire:
                    iconIndex = 2;
                    break;
                case AttributeType.Thunder:
                    iconIndex = 8;
                    break;
                case AttributeType.Ice:
                    iconIndex = 18;
                    break;
                case AttributeType.Shine:
                    iconIndex = 13;
                    break;
                case AttributeType.Dark:
                    iconIndex = 14;
                    break;
                default:
                    iconIndex = 32;
                    break;
            }
            return LoadSpellIconBases()[iconIndex];
        }

        public static Sprite LoadItemIconBase(ItemType itemType, AttributeType attributeType = AttributeType.None)
        {
            var iconIndex = 0;
            switch (itemType)
            {
                case ItemType.RandumAddEquipment:
                case ItemType.SelectAddEquipment:
                    return LoadSpellIconBase(attributeType);
                case ItemType.Artifact:
                    iconIndex = 15;
                    break;
                case ItemType.Currency:
                    iconIndex = 38;
                    break;
            }
            return LoadSpellIconBases()[iconIndex];
        }

        public static Sprite LoadStateIconBase(int iconIndex)
        {
            return LoadSpellIconBases()[iconIndex];
        }

        public static Sprite[] LoadBuffIcons()
        {
            return LoadResources<Sprite>("Texture/Buff/Icons");
        }

        public static Sprite LoadSBuffIcon(int iconIndex)
        {
            return LoadBuffIcons()[iconIndex];
        }

        public static Sprite[] LoadElementIcon()
        {
            return LoadResources<Sprite>(SystemTexturePath + "ElementIcon");
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

        public static SpriteAtlas LoadUnitIcons()
        {
            return LoadResource<SpriteAtlas>("Texture/UnitIcons");
        }

        public static Sprite LoadGuideSprite(string path)
        {
            return LoadResource<Sprite>("Texture/Guide/" + path);
        }

        public static Sprite[] LoadMapCellIcons()
        {
            return LoadResources<Sprite>("Texture/MapCellIcons");
        }

        public static Sprite LoadMapCell(int iconIndex)
        {
            return LoadMapCellIcons()[iconIndex];
        }

        public static Ariadne.DungeonMasterData LoadDungeonMaster(string path)
        {
            return LoadResource<Ariadne.DungeonMasterData>("Data/Dungeon" + path);
        }

        public static Material LoadSkyboxMaterial(string path)
        {
            return LoadResource<Material>("Material/" + path);
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
        None = 0,
        Boot = 10,
        Title = 20,
        NameEntry = 30,
        MainMenu = 40,
        Battle = 50,
        Tactics = 60,
        Strategy = 70,
        Dungeon = 80,
        Interlude = 90,
        Result = 100,
        Demo = 110,
    }
}