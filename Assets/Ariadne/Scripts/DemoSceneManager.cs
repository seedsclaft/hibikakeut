using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Ariadne
{
    /// <Summary>
    /// Manager class of the Demo Scene.
    /// </Summary>
    public class DemoSceneManager : AriadneSystemBase, IExitDungeon
    {
        [SerializeField]
        Button enterButton;

        [SerializeField]
        Image screenMaskImage;

        [SerializeField]
        float screenFadeTime = 1.0f;

        void Start()
        {
            // Initialize game settings.
            InitializeTraverseData();
            InitializeEventFlagData();
            InitializeItemData();

            // Set up the move controller script.
            CheckMoveControllerReference();
            moveController.SetPostMoveEventObject(gameObject);
            SendEnterDungeonMessage(moveController.gameObject);
        }

        /// <Summary>
        /// Initialize all traverse data.
        /// </Summary>
        void InitializeTraverseData()
        {
            TraverseManager.Instance.InitializeTraverseData();
        }

        /// <Summary>
        /// Initialize all event flag data.
        /// </Summary>
        void InitializeEventFlagData()
        {
            CheckEventFlagManagerReference();
            eventFlagManager.InitializeEventFlagList();
        }

        /// <Summary>
        /// Initialize all item data.
        /// </Summary>
        void InitializeItemData()
        {
            CheckItemDataManagerReference();
            itemDataManager.InitializeHoldItemDict();
        }

        /// <Summary>
        /// Fade in screen and dungeon entering button.
        /// </Summary>
        IEnumerator FadeIn()
        {
            enterButton.gameObject.SetActive(true);
            yield return StartCoroutine(AriadneFadeManager.FadeOutImage(screenMaskImage, screenFadeTime));
            enterButton.interactable = true;
        }

        /// <Summary>
        /// Fade out screen and dungeon entering button.
        /// </Summary>
        IEnumerator FadeOut()
        {
            yield return StartCoroutine(AriadneFadeManager.FadeInImage(screenMaskImage, screenFadeTime));
            enterButton.gameObject.SetActive(false);
            yield return new WaitForSeconds(screenFadeTime);
            SendEnterDungeonMessage(moveController.gameObject);
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