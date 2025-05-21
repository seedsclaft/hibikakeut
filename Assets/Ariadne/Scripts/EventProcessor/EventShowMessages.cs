using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ariadne
{
    /// <Summary>
    /// Event process of showing messages.
    /// </Summary>
    public class EventShowMessages : AriadneEventStrategyBase, IAriadneEvent, IEventKeyNotify
    {
        [SerializeField]
        protected float fadeTime = 0.5f;

        [SerializeField]
        protected string messageWindowImageObjectName = "MessageWindow";

        [SerializeField]
        protected string messageWindowTextObjectName = "MessageText";

        [SerializeField]
        protected string argNameMessageList = "MessageList";

        protected bool waitingMessage = false;

        /// <Summary>
        /// Execute method of event process.
        /// </Summary>
        /// <param name="callbackObj">GameObject which receives call.</param>
        /// <param name="eventPos">The position of the event.</param>
        /// <param name="processData">The event process data.</param>
        /// <param name="debugMode">The flag for debug.</param>
        public virtual void ExecuteAriadneEvent(GameObject callbackObj, Vector2Int eventPos, EventProcessData processData, bool debugMode)
        {
            PreEventProcess(callbackObj, debugMode);

            Image windowImage = GameObject.Find(messageWindowImageObjectName).GetComponent<Image>();
            Text messageText = GameObject.Find(messageWindowTextObjectName).GetComponent<Text>();

            // Get the message list.
            List<string> msgList = new List<string>();
            msgList = processData.eventArgs.Find(arg => arg.argName == argNameMessageList).argListValue;

            if (msgList.Count > 0)
            {
                StartCoroutine(ShowMessages(msgList, windowImage, messageText));
            }
            else
            {
                PostEventProcess();
            }
        }

        /// <Summary>
        /// Showing message process.
        /// </Summary>
        /// <param name="msgList">Message list to show.</param>
        protected virtual IEnumerator ShowMessages(List<string> msgList, Image windowImage, Text messageText)
        {
            messageText.text = "";

            if (windowImage != null)
            {
                StartCoroutine(AriadneFadeManager.FadeInImage(windowImage, fadeTime));
            }

            if (messageText != null)
            {
                StartCoroutine(AriadneFadeManager.FadeInText(messageText, fadeTime));
            }

            yield return new WaitForSeconds(fadeTime);

            for (int page = 0; page < msgList.Count; page++)
            {
                messageText.text = msgList[page];
                waitingMessage = true;

                while (waitingMessage)
                {
                    yield return null;
                }
            }

            // Fade out message window and text.
            if (windowImage != null)
            {
                StartCoroutine(AriadneFadeManager.FadeOutImage(windowImage, fadeTime));
            }

            if (messageText != null)
            {
                StartCoroutine(AriadneFadeManager.FadeOutText(messageText, fadeTime));
            }

            yield return new WaitForSeconds(fadeTime);

            base.PostEventProcess();
        }

        protected virtual void Update()
        {
            if (waitingMessage)
            {
                
            }
        }

        /// <Summary>
        /// Get the key input of waiting in message event.
        /// </Summary>
        protected virtual void MsgKeyWait()
        {
            if (Input.GetKeyUp(KeyCode.Space))
            {
                waitingMessage = false;
            }
        }

        /// <Summary>
        /// Recieve the notification of return key.
        /// </Summary>
        public virtual void OnPressedReturnKey()
        {
            if (waitingMessage)
            {
                waitingMessage = false;
            }
        }

        /// <Summary>
        /// Recieve the notification of input button.
        /// </Summary>
        public virtual void OnPressedReturnButton()
        {
            if (waitingMessage)
            {
                waitingMessage = false;
            }
        }
    }
}