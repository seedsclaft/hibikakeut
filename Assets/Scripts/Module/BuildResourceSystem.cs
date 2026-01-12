using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Ryneus
{
    public class BuildResourceSystem
    {
#if UNITY_EDITOR
        [MenuItem ("Resources/DemoBuild")]
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
