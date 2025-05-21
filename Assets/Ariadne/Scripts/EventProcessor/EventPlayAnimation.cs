using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ariadne
{
    /// <Summary>
    /// Play animation which specified in event args.
    /// </Summary>
    public class EventPlayAnimation : AriadneEventStrategyBase, IAriadneEvent
    {
        [SerializeField]
        protected string argNameSpecifyTargetObj = "SpecifyTargetObj";

        [SerializeField]
        protected string argNameAnimatorObjectName = "AnimatorObjectName";

        [SerializeField]
        protected string argNameParameterName = "ParameterName";

        [SerializeField]
        protected string argNameParameterType = "ParameterType";

        [SerializeField]
        protected string argNameIntValue = "IntValue";

        [SerializeField]
        protected string argNameFloatValue = "FloatValue";

        [SerializeField]
        protected string argNameBoolValue = "BoolValue";

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

            // Select animation target.
            GameObject animTargetObj = null;

            string argValue = processData.eventArgs.Find(arg => arg.argName == argNameSpecifyTargetObj).argListValue[0];
            bool specifyTargetObj = false;
            bool success = bool.TryParse(argValue, out specifyTargetObj);
            if (!success)
            {
                Debug.LogWarning("Event Position : " + eventPos + " / Argument : " + argNameSpecifyTargetObj + " has invalid data. Check your event data in EventEditor.");
            }

            if (specifyTargetObj)
            {
                // Find GameObject by name.
                string animObjName = processData.eventArgs.Find(arg => arg.argName == argNameAnimatorObjectName).argListValue[0];
                animTargetObj = GameObject.Find(animObjName);
                if (animTargetObj == null)
                {
                    Debug.LogWarning("Event Position : " + eventPos + " / Argument : " + argNameAnimatorObjectName + " Specified GameObject is not found. Check your event data in EventEditor.");
                }
            }
            else
            {
                // Get GameObject from dungeon walls by position.
                animTargetObj = GetEventObj(eventPos);
            }

            if (animTargetObj != null)
            {
                PlayAnimation(animTargetObj, eventPos, processData);
            }
            else
            {
                PostEventProcess();
            }
        }

        /// <Summary>
        /// Play animation of the specified object.
        /// </Summary>
        protected virtual void PlayAnimation(GameObject animationObject, Vector2Int eventPos, EventProcessData processData)
        {
            // Get a reference for animator.
            Animator animator = animationObject.GetComponent<Animator>();

            // Get parameter name and type.
            string paramName = processData.eventArgs.Find(arg => arg.argName == argNameParameterName).argListValue[0];
            string paramTypeString = processData.eventArgs.Find(arg => arg.argName == argNameParameterType).argListValue[0];
            int paramTypeValue = 0;
            bool successType = int.TryParse(paramTypeString, out paramTypeValue);
            if (!successType)
            {
                Debug.LogWarning("Event Position : " + eventPos + " / Argument : " + argNameParameterType + " has invalid data. Check your event data in EventEditor.");
            }

            AnimatorControllerParameterType paramType = (AnimatorControllerParameterType)System.Enum.ToObject(typeof(AnimatorControllerParameterType), paramTypeValue);

            // Call appropriate method.
            switch (paramType)
            {
                case AnimatorControllerParameterType.Float:
                    string animValue = processData.eventArgs.Find(arg => arg.argName == argNameFloatValue).argListValue[0];
                    float floatValue = 0f;

                    bool success = float.TryParse(animValue, out floatValue);
                    if (success)
                    {
                        animator.SetFloat(paramName, floatValue);
                    }
                    else
                    {
                        Debug.LogWarning("Event Position : " + eventPos + " / Argument : " + argNameFloatValue + " Specified animation value (float) is invalid. Check your event data in EventEditor.");
                    }
                    break;

                case AnimatorControllerParameterType.Int:
                    animValue = processData.eventArgs.Find(arg => arg.argName == argNameIntValue).argListValue[0];
                    int intValue = 0;
                    
                    success = int.TryParse(animValue, out intValue);
                    if (success)
                    {
                        animator.SetInteger(paramName, intValue);
                    }
                    else
                    {
                        Debug.LogWarning("Event Position : " + eventPos + " / Argument : " + argNameIntValue + " Specified animation value (int) is invalid. Check your event data in EventEditor.");
                    }
                    break;

                case AnimatorControllerParameterType.Bool:
                    animValue = processData.eventArgs.Find(arg => arg.argName == argNameBoolValue).argListValue[0];
                    bool boolValue = false;
                    
                    success = bool.TryParse(animValue, out boolValue);
                    if (success)
                    {
                        animator.SetBool(paramName, boolValue);
                    }
                    else
                    {
                        Debug.LogWarning("Event Position : " + eventPos + " / Argument : " + argNameIntValue + " Specified animation value (bool) is invalid. Check your event data in EventEditor.");
                    }
                    break;

                case AnimatorControllerParameterType.Trigger:
                    animator.SetTrigger(paramName);
                    break;
            }

            // Finish process.
            base.PostEventProcess();
        }
    }
}