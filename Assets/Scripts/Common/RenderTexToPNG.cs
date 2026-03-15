using UnityEngine;
using UnityEngine.UI;

namespace Ryneus
{
    public class RenderTexToPNG : MonoBehaviour
    {
        public Camera targetCamera;
        public RenderTexture renderTexture;
        public RawImage renderImage;
        void Start()
        {
            // 画面サイズと同じ解像度でRenderTextureを作成
            renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
        }

        public void SaveRenderTextureToPNG()
        {
            targetCamera.targetTexture = renderTexture;
            // レンダリング先をRenderTextureに設定
            targetCamera.Render();

            // アクティブなRenderTextureを一時的に切り替える
            RenderTexture.active = renderTexture;

            // Texture2Dにピクセルデータを読み込む
            var savedTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false, true);
            savedTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            savedTexture.Apply();
            renderImage.texture = savedTexture;

            // 終了処理
            RenderTexture.active = null;
            targetCamera.targetTexture = null;
        }
    }
}
