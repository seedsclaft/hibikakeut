using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if TMPro
using TMPro;
#endif

namespace Ariadne
{
    /// <Summary>
    /// Static class to control fading.
    /// </Summary>
    public static class AriadneFadeManager
    {
        static bool fadingImage = false;
        static bool fadingText = false;

        /// <Summary>
        /// Fade process of an image component.
        /// </Summary>
        /// <param name="image">Specify image component to fade.</param>
        /// <param name="targetAlpha">Alpha value after fade.</param>
        /// <param name="fadeAnimTime">Fading time.</param>
        static IEnumerator FadeImageProcess(Image image, float targetAlpha, float fadeAnimTime)
        {
            while (fadingImage)
            {
                yield return null;
            }

            Color initColor = image.color;
            Color targetColor = new Color(initColor.r, initColor.g, initColor.b, targetAlpha);

            fadingImage = true;
            float fadeFinishTime = GetFadeFinishTime(fadeAnimTime);

            while (true)
            {
                float diff = fadeFinishTime - Time.time;
                if (diff <= 0)
                {
                    break;
                }

                float rate = 1 - Mathf.Clamp01(diff / fadeAnimTime);
                image.color = Color.Lerp(initColor, targetColor, rate);
                yield return null;
            }
            image.color = targetColor;
            fadingImage = false;
        }
        
        /// <Summary>
        /// Fade in process of an image component.
        /// </Summary>
        /// <param name="image">Specify image component to fade.</param>
        /// <param name="fadeAnimTime">Fading time.</param>
        public static IEnumerator FadeInImage(Image image, float fadeAnimTime)
        {
            float alpha = 1.0f;
            Debug.Log(image);
            yield return FadeImageProcess(image, alpha, fadeAnimTime);
        }

        /// <Summary>
        /// Fade out process of an image component.
        /// </Summary>
        /// <param name="image">Specify image component to fade.</param>
        /// <param name="fadeAnimTime">Fading time.</param>
        public static IEnumerator FadeOutImage(Image image, float fadeAnimTime)
        {
            float alpha = 0.0f;
            yield return FadeImageProcess(image, alpha, fadeAnimTime);
        }

        /// <Summary>
        /// Fade process of an text component.
        /// </Summary>
        /// <param name="text">Specify text component to fade.</param>
        /// <param name="targetAlpha">Alpha value after fade.</param>
        /// <param name="fadeAnimTime">Fading time.</param>
        static IEnumerator FadeTextProcess(Text text, float targetAlpha, float fadeAnimTime)
        {
            while (fadingText)
            {
                yield return null;
            }

            Color initColor = text.color;
            Color targetColor = new Color(initColor.r, initColor.g, initColor.b, targetAlpha);

            fadingText = true;
            float fadeFinishTime = GetFadeFinishTime(fadeAnimTime);

            while (true)
            {
                float diff = fadeFinishTime - Time.time;
                if (diff <= 0)
                {
                    break;
                }

                float rate = 1 - Mathf.Clamp01(diff / fadeAnimTime);
                text.color = Color.Lerp(initColor, targetColor, rate);
                yield return null;
            }
            text.color = targetColor;
            fadingText = false;
        }
        
        /// <Summary>
        /// Fade in process of an text component.
        /// </Summary>
        /// <param name="text">Specify text component to fade.</param>
        /// <param name="fadeAnimTime">Fading time.</param>
        public static IEnumerator FadeInText(Text text, float fadeAnimTime)
        {
            float alpha = 1.0f;
            yield return FadeTextProcess(text, alpha, fadeAnimTime);
        }

        /// <Summary>
        /// Fade out process of an text component.
        /// </Summary>
        /// <param name="text">Specify text component to fade.</param>
        /// <param name="fadeAnimTime">Fading time.</param>
        public static IEnumerator FadeOutText(Text text, float fadeAnimTime)
        {
            float alpha = 0.0f;
            yield return FadeTextProcess(text, alpha, fadeAnimTime);
        }

        /// <Summary>
        /// Returns the time fade will be finished.
        /// </Summary>
        static float GetFadeFinishTime(float time)
        {
            return Time.time + time;
        }

        /// <Summary>
        /// Initialize wait flags of fade in and out.
        /// </Summary>
        public static void InitializeWaitFlags()
        {
            fadingImage = false;
            fadingText = false;
        }
    }
}