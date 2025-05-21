using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace Ariadne
{
    /// <Summary>
    /// Processes about entering the dungeon.
    /// </Summary>
    [Obsolete("Refer to 'DemoSceneManager' to integrate to your system.")]
    public class EnterDungeonManager : MonoBehaviour, IExitDungeon
    {
        [SerializeField]
        GameObject enterButtonObj;
        Button enterButton;

        [SerializeField]
        GameObject moveController;

        [SerializeField]
        GameObject screenMaskObject;
        Image screenMaskImage;

        [SerializeField]
        float screenFadeTime = 1.0f;

        void Start()
        {
            SetObjRef();
        }

        /// <Summary>
        /// Set object references to cache them.
        /// </Summary>
        void SetObjRef()
        {
            enterButton = enterButtonObj.GetComponent<Button>();
            screenMaskImage = screenMaskObject.GetComponent<Image>();
        }

        /// <Summary>
        /// Fade in screen and dungeon entering button.
        /// </Summary>
        IEnumerator FadeIn()
        {
            enterButtonObj.SetActive(true);
            yield return StartCoroutine(AriadneFadeManager.FadeOutImage(screenMaskImage, screenFadeTime));
            enterButton.interactable = true;
        }

        /// <Summary>
        /// Fade out screen and dungeon entering button.
        /// </Summary>
        IEnumerator FadeOut()
        {
            yield return StartCoroutine(AriadneFadeManager.FadeInImage(screenMaskImage, screenFadeTime));
            enterButtonObj.SetActive(false);
            yield return new WaitForSeconds(screenFadeTime);
            SendEnterDungeonMessage(moveController);
        }

        /// <Summary>
        /// Receiver of exit dungeon message.
        /// </Summary>
        public void OnExitDungeon()
        {
            StartCoroutine(FadeIn());
        }
        
        /// <Summary>
        /// Entering the dungeon button listener.
        /// </Summary>
        public void OnPressedEnterButton()
        {
            enterButton.interactable = false;
            StartCoroutine(FadeOut());
        }

        /// <Summary>
        /// Send a message which notifies entering the dungeon.
        /// </Summary>
        /// <param name="obj">The GameObject that holds the MoveConteroller component.</param>
        void SendEnterDungeonMessage(GameObject obj)
        {
            ExecuteEvents.Execute<IEnterDungeon>(
                target: obj,
                eventData: null,
                functor: EnterDungeonMsg
            );
        }

        /// <Summary>
        /// The functor of SendEnterDungeonMessage method.
        /// </Summary>
        void EnterDungeonMsg(IEnterDungeon enterDungeon, BaseEventData eventData)
        {
            enterDungeon.OnEnterDungeon();
        }
    }
}