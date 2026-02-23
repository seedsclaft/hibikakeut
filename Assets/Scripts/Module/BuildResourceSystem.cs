using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using System.IO;
#endif
using UnityEngine;

namespace Ryneus
{
    public class BuildResourceSystem
    {
#if UNITY_EDITOR
        [MenuItem("Resources/AutoAssetBundleName")]
        public static void AutoAssetBundleName()
        {
            var strs = new List<string>()
            {
                "Assets/AssetBundle/Audios/BGM",
                "Assets/AssetBundle/Audios/SE",
                "Assets/AssetBundle/Animations/MAGICALxSPIRAL",
                "Assets/AssetBundle/Animations/NA_Effekseer",
                "Assets/AssetBundle/Animations/Sound",
                "Assets/AssetBundle/Animations/tktk01",
                "Assets/AssetBundle/Animations/tktk02",
                "Assets/AssetBundle/Animations/Genfulew_Effect",
                "Assets/AssetBundle/Animations/MakerEffect",
                "Assets/AssetBundle/Texture/BG",
                "Assets/AssetBundle/Texture/Character/Enemies",
                "Assets/AssetBundle/Texture/Character/Npcs",
                "Assets/AssetBundle/FieldBattler/Actors",
                "Assets/AssetBundle/FieldBattler/Enemies",
            };
            for (int i = 1; i <= 10; i++)
            {
                strs.Add("Assets/AssetBundle/Texture/Character/Actors/" + i.ToString("D4"));
            }
            for (int i = 101; i <= 104; i++)
            {
                strs.Add("Assets/AssetBundle/Texture/Character/Actors/" + i.ToString("D4"));
            }
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                return;
            }
            var group = settings.FindGroup("Remote");
#if UNITY_STANDALONE_WIN
            group = settings.FindGroup("Windows");
#endif
            if (group == null)
            {
                return;
            }
            // グループ内の全エントリーを削除
            var entries = group.entries.ToList();
            foreach (var entry in entries)
            {
                group.RemoveAssetEntry(entry);
            }
            foreach (var str in strs)
            {
                string[] files = Directory.GetFiles(str, "*", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    if (file.Contains(".meta"))
                    {
                        continue;
                    }
                    if (file.Contains(".efkproj"))
                    {
                        continue;
                    }
                    if (file.Contains(".efkefc"))
                    {
                        continue;
                    }
                    if (file.Contains(".efkmodel"))
                    {
                        continue;
                    }
                    if (file.Contains(".efkmat"))
                    {
                        continue;
                    }
                    // アセットを登録
                    string guid = AssetDatabase.AssetPathToGUID(file);
                    var entry = settings.CreateOrMoveEntry(guid, group);

                    var address = file;
                    address = address.Replace("Assets/AssetBundle/", "");
                    address = address.Replace(".ogg", "");
                    address = address.Replace(".mp3", "");
                    address = address.Replace(".wav", "");
                    address = address.Replace(".png", "");
                    address = address.Replace(".jpg", "");
                    address = address.Replace(".asset", "");
                    address = address.Replace(".efkmodel", "");
                    address = address.Replace(".prefab", "");
                    address = address.Replace("\\", "/");
                    entry.address = address;
                }

            }
        }

        [MenuItem("Resources/AssetBundleName")]
        public static void AssetBundleName()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("AddressableAssetSettings not found.");
                return;
            }

            var groupName = "Remote";
            var group = settings.FindGroup(groupName);
            if (group == null)
            {
                Debug.LogError($"Group {groupName} not found.");
                return;
            }


            int count = 0;
            foreach (var entry in group.entries)
            {
                foreach (var subAsset in entry.SubAssets)
                {
                    string old = subAsset.address;
                    string address = subAsset.address;
                    address = address.Replace(".ogg", "");
                    address = address.Replace(".mp3", "");
                    if (old != address)
                    {
                        subAsset.SetAddress(address);
                        count++;
                    }
                }
            }
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryModified, null, true);


            AssetDatabase.SaveAssets();
            Debug.Log($"{count} entries updated in group {groupName}.");
        }

        [MenuItem("Resources/DemoBuild")]
        public static void DemoBuild()
        {
            // BGM
            var BGMDates = Resources.Load<SoundDates>("Data/BGM").Data;
            foreach (var BGMDate in BGMDates)
            {
                if (BGMDate.ResourceType == 0)
                {
                    var path = "Assets/Resources/Audios/BGM/" + BGMDate.FileName;
                    var ogg = ".ogg";
                    var result = AssetDatabase.DeleteAsset(path + ogg);
                    Debug.Log(path + "delete : " + result);
                    var mp3 = ".mp3";
                    result = AssetDatabase.DeleteAsset(path + mp3);
                    Debug.Log(path + "delete : " + result);
                    var wav = ".wav";
                    result = AssetDatabase.DeleteAsset(path + wav);
                    Debug.Log(path + "delete : " + result);
                }
            }

            // Animations
            var textureResources = new List<string>();
            var AnimationDates = Resources.Load<AnimationDates>("Data/Animations").Data;
            foreach (var AnimationData in AnimationDates)
            {
                var animation = ResourceSystem.LoadResourceEffect(AnimationData.AnimationPath);
                if (animation == null)
                {
                    continue;
                }
                if (animation.textureResources == null)
                {
                    continue;
                }
                foreach (var item in animation.textureResources)
                {
                    if (item.texture == null)
                    {
                        continue;
                    }
                    textureResources.Add(item.texture.name);
                }
            }

            var texturePath = "Animations/NA_Effekseer/Texture/";
            var textures = Resources.LoadAll<Texture>(texturePath);
            foreach (var texture in textures)
            {
                if (textureResources.Find(a => a == texture.name) == null)
                {
                    var path = "Assets/Resources/" + texturePath + texture.name + ".png";
                    var path2 = "Assets/Resources/" + texturePath + texture.name + ".jpg";
                    var result = AssetDatabase.DeleteAsset(path);
                    result = AssetDatabase.DeleteAsset(path2);
                }
            }

            texturePath = "Animations/Genfulew_Effect/Texture/";
            textures = Resources.LoadAll<Texture>(texturePath);
            foreach (var texture in textures)
            {
                if (textureResources.Find(a => a == texture.name) == null)
                {
                    var path = "Assets/MakerEffect/Resources/" + texturePath + texture.name + ".png";
                    var path2 = "Assets/MakerEffect/Resources/" + texturePath + texture.name + ".jpg";
                    var result = AssetDatabase.DeleteAsset(path);
                    result = AssetDatabase.DeleteAsset(path2);
                }
            }
            // Stage背景
            var backgrounds = new List<string>();
            var skyNames = new List<string>();
            var Stages = Resources.Load<StageDates>("Data/Stages").Data;
            foreach (var Stage in Stages)
            {
                if (Stage.Id % 1000 < 40)
                {
                    backgrounds.Add(Stage.BackGround);
                    skyNames.Add(Stage.SkyboxName);
                }
            }
            foreach (var Stage in Stages)
            {
                if (!backgrounds.Contains(Stage.BackGround))
                {
                    var path = "Assets/Resources/Texture/BG/" + Stage.BackGround + ".png";
                    var result = AssetDatabase.DeleteAsset(path);
                    path = "Assets/Resources/Texture/BG/" + Stage.BackGround + ".jpg";
                    result = AssetDatabase.DeleteAsset(path);
                }
                if (!skyNames.Contains(Stage.SkyboxName))
                {
                    var path = "Assets/Resources/Material/" + Stage.SkyboxName + ".mat";
                    var result = AssetDatabase.DeleteAsset(path);
                    path = "Assets/SkySeries Freebie/FreebieHdri/" + Stage.SkyboxName + ".hdr";
                    result = AssetDatabase.DeleteAsset(path);
                    path = "Assets/SkySeries Freebie/FreebieHdri/" + Stage.SkyboxName + "4k.hdr";
                    result = AssetDatabase.DeleteAsset(path);
                }
            }
            // Enemy
            var enemyTextures = new List<string>();
            var Enemies = Resources.Load<EnemyDates>("Data/Enemies").Data;
            foreach (var Enemy in Enemies)
            {
                enemyTextures.Add(Enemy.ImagePath);
            }
            var enemyTexturePath = "Texture/Character/Enemies/";
            var enemyTexs = Resources.LoadAll<Texture>(enemyTexturePath);
            foreach (var enemyTex in enemyTexs)
            {
                if (enemyTextures.Find(a => a == enemyTex.name) == null)
                {
                    var path = "Assets/Resources/" + enemyTexturePath + enemyTex.name + ".png";
                    var result = AssetDatabase.DeleteAsset(path);
                }
            }
        }
#endif
    }
}
