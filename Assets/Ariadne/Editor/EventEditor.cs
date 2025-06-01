using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Ariadne
{
    /// <Summary>
    /// Editor class for event data.
    /// </Summary>
    public class EventEditor : EditorWindow
    {
        List<EventCategoryData> eventCategoryDataList;

        readonly string UndoName = "Change Event Setting";
        readonly int LeftPaneWidth = 400;

        // Event info
        int eventId = 0;
        [SerializeField]
        string eventName = "New Event";

        [SerializeField]
        EventMasterData editingEventData;

        [SerializeField]
        List<AriadneEventParts> eventPartsList;
        [SerializeField]
        AriadneEventParts editingEventParts;

        int moveUpTargetPartsIndex = -1;
        int moveDownTargetPartsIndex = -1;
        int removeTargetPartsIndex = -1;

        int selectedParts = -1;
        int selectedFlagIndex;
        
        Vector2 leftScrollPos = Vector2.zero;
        Vector2 rightScrollPos = Vector2.zero;
        
        GUIStyle removeStyle;
        GUIStyle editStyle;
        GUIStyle boldLabelStyle;

        Color removeColor = new Color(1.0f, 0.2f, 0.0f, 1.0f);

        [SerializeField]
        bool conditionNorth;
        [SerializeField]
        bool conditionEast;
        [SerializeField]
        bool conditionWest;
        [SerializeField]
        bool conditionSouth;

        // For event category.
        int[] eventCategoryIds;
        string[] eventCategoryNames;

        // For treasure event.
        int[] itemIds;
        string[] itemNames;

        // For event start condition.
        string[] flagNames;

        // Event category args
        List<EventArgumentTypeData> eventArgumentTypeDataList;
        int moveUpTargetProcessIndex = -1;
        int moveDownTargetProcessIndex = -1;
        int removeTargetProcessIndex = -1;

        // Key: Type name / Value: Type
        Dictionary<string, Type> typeDict = new Dictionary<string, Type>();
        // Key: Type name / Value: Is type Unity object
        Dictionary<string, bool> typeIsUnityObjDict = new Dictionary<string, bool>();
        int removeTargetValueIndex = -1;
        int removeTargetObjectIndex = -1;

        // Key: Type name / Value: Enum Values
        Dictionary<string, int[]> enumValueDict = new Dictionary<string, int[]>();
        // Key: Type name / Value: Enum Names
        Dictionary<string, string[]> enumNameDict = new Dictionary<string, string[]>();

        Texture2D highlightTex;
        Color highlightColor = new Color(0.0f, 0.5f, 1.0f, 0.2f);
        Color[] highlightColors;
        GUIStyle processStyle = new GUIStyle();

        /// <Summary>
        /// Show GUI for setting of the event data.
        /// </Summary>
        void OnGUI()
        {
            EditorGUILayout.LabelField("Ariadne Event Editor", boldLabelStyle);
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            {
                leftScrollPos = EditorGUILayout.BeginScrollView(leftScrollPos, GUI.skin.box, GUILayout.Width(LeftPaneWidth + 40f));
                {
                    EditorGUILayout.BeginVertical();
                    {
                        ShowEventSettingParts();
                        ShowEachEventPartsInformation();

                        EditorGUILayout.Space();
                        ShowSaveAndCancelButtonParts();
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndScrollView();

                rightScrollPos = EditorGUILayout.BeginScrollView(rightScrollPos, GUI.skin.box);
                {
                    EditorGUILayout.BeginVertical();
                    {
                        ShowEditingEventIndexParts();
                        ShowEditingPartsDetail();
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        /// <Summary>
        /// Show setting parts about event data such as event name.
        /// </Summary>
        void ShowEventSettingParts()
        {
            EditorGUILayout.BeginVertical("Box");
            {
                EditorGUILayout.LabelField("Event ID", eventId.ToString());

                EditorGUI.BeginChangeCheck();
                string newEventName = EditorGUILayout.TextField("Event Name", eventName);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(this, UndoName);
                    eventName = newEventName;
                }

                EditorGUILayout.Space();
            }
            EditorGUILayout.EndVertical();
        }

        /// <Summary>
        /// Show information parts about event parts data.
        /// </Summary>
        void ShowEachEventPartsInformation()
        {
            EditorGUILayout.BeginVertical();
            {
                for (int i = 0; i < eventPartsList.Count; i++)
                {
                    EditorGUILayout.BeginVertical("Box");
                    {
                        ShowEventPartsCategoryAndConditions(i);
                        ShowEditAndRemoveButtonParts(i);
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                }

                CheckPartsAction();
                
                if (GUILayout.Button("Add event parts"))
                {
                    var newParts = new AriadneEventParts();
                    Undo.RecordObject(this, UndoName);
                    eventPartsList.Add(newParts);
                }
            }
            EditorGUILayout.EndVertical();
        }

        /// <Summary>
        /// Show information parts about event parts data in detail.
        /// </Summary>
        /// <param name="i">Index of the event parts.</param>
        void ShowEventPartsCategoryAndConditions(int i)
        {
            if (selectedParts == i)
            {
                EditorGUILayout.LabelField("Event List Index : " + i.ToString() + " (In Editing)", boldLabelStyle);
            }
            else
            {
                EditorGUILayout.LabelField("Event List Index : " + i.ToString());
            }

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Start Condition");
                EditorGUILayout.LabelField(eventPartsList[i].startCondition.ToString());
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Event Trigger");
                EditorGUILayout.LabelField(eventPartsList[i].eventTrigger.ToString());
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Event Start Pos");
                EditorGUILayout.LabelField(eventPartsList[i].eventPos.ToString());
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <Summary>
        /// Show edit button and remove button.
        /// When remove button is pressed, confirming dialog will be shown.
        /// </Summary>
        /// <param name="i">Index of the event parts.</param>
        void ShowEditAndRemoveButtonParts(int i)
        {
            EditorGUILayout.BeginHorizontal();
            {
                if (eventPartsList.Count > 1)
                {
                    if (i != 0)
                    {
                        if (GUILayout.Button("Move Up", editStyle))
                        {
                            // Set move up index.
                            moveUpTargetPartsIndex = i;
                        }
                    }

                    if (i != eventPartsList.Count - 1)
                    {
                        if (GUILayout.Button("Move Down", editStyle))
                        {
                            // Set move down index.
                            moveDownTargetPartsIndex = i;
                        }
                    }

                    if (GUILayout.Button("Remove", removeStyle))
                    {
                        string confirmMsg = "Remove this event parts?" + "\n"
                                            + "Index : " + i.ToString();
                        bool dialogChoice = EditorUtility.DisplayDialog("Confirm",
                                                                    confirmMsg,
                                                                    "Remove",
                                                                    "Cancel");
                        if (dialogChoice)
                        {
                            selectedParts = -1;
                            removeTargetPartsIndex = i;
                        }
                    }
                }

                if (GUILayout.Button("Edit", editStyle))
                {
                    // Set event data of this index.
                    selectedParts = i;
                    // editingEventParts = new AriadneEventParts(eventPartsList[selectedParts]);
                    editingEventParts = eventPartsList[selectedParts];
                    GetExecutedFlagList();
                    CheckDirectionConditions();
                    CheckArgumentsData();
                    ClearUndoStack();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <Summary>
        /// Show save button and cancel button.
        /// </Summary>
        void ShowSaveAndCancelButtonParts()
        {
            EditorGUILayout.BeginVertical("Box");
            {
                if (GUILayout.Button("Save", editStyle))
                {
                    SaveEventFile();
                }
            }
            EditorGUILayout.EndVertical();
        }

        /// <Summary>
        /// Show index of editing event parts.
        /// </Summary>
        void ShowEditingEventIndexParts()
        {
            EditorGUILayout.BeginVertical("Box");
            {
                EditorGUILayout.LabelField("Event Edit", boldLabelStyle);
                if (selectedParts >= 0)
                {
                    EditorGUILayout.LabelField("Editing event index : " + selectedParts.ToString());
                }
            }
            EditorGUILayout.EndVertical();
        }

        /// <Summary>
        /// Show setting GUI for event parts.
        /// </Summary>
        void ShowEditingPartsDetail()
        {
            if (selectedParts < 0)
            {
                return;
            }

            EditorGUILayout.BeginVertical();
            {
                ShowEventStartConditionParts();
                ShowEventExecutedFlagParts();
                ShowEventTriggerAndStartPositionParts();
                ShowStartDirectionParts();
                ShowEventProcessParts();
            }
            EditorGUILayout.EndVertical();

            // ShowSelectDestinationMapParts();
            EditorGUILayout.Space();
            ShowApplyAndCancelButtonParts();
        }

        /// <Summary>
        /// Show setting GUI for start condition.
        /// </Summary>
        void ShowEventStartConditionParts()
        {
            EditorGUILayout.BeginVertical("Box");
            {
                EditorGUILayout.LabelField("Condition", boldLabelStyle);

                EditorGUI.BeginChangeCheck();
                AriadneEventCondition startCondition = (AriadneEventCondition)EditorGUILayout.EnumPopup("Event start condition", editingEventParts.startCondition);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(this, UndoName);
                    editingEventParts.startCondition = startCondition;
                }

                if (editingEventParts.startCondition == AriadneEventCondition.Flag)
                {
                    // Show flag name popup.
                    selectedFlagIndex = Array.IndexOf(flagNames, editingEventParts.startFlagName);
                    selectedFlagIndex = EditorGUILayout.Popup("Event start flag", selectedFlagIndex, flagNames);
                    if (selectedFlagIndex >= 0 && selectedFlagIndex < flagNames.Length)
                    {
                        Undo.RecordObject(this, UndoName);
                        editingEventParts.startFlagName = flagNames[selectedFlagIndex];
                    }
                }
                else if (editingEventParts.startCondition == AriadneEventCondition.Item)
                {
                    // Show item condition.
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("When the player has ");

                        EditorGUI.BeginChangeCheck();
                        int itemId = EditorGUILayout.IntPopup(editingEventParts.startItemId, itemNames, itemIds);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(this, UndoName);
                            editingEventParts.startItemId = itemId;
                        }

                        EditorGUI.BeginChangeCheck();
                        AriadneComparison comparison = (AriadneComparison)EditorGUILayout.EnumPopup(editingEventParts.comparisonOperator);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(this, UndoName);
                            editingEventParts.comparisonOperator = comparison;
                        }

                        EditorGUI.BeginChangeCheck();
                        int startNum = EditorGUILayout.IntField(editingEventParts.startNum);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(this, UndoName);
                            editingEventParts.startNum = startNum;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                else if (editingEventParts.startCondition == AriadneEventCondition.Money)
                {
                    // Show money condition.
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("When the player has Money ");

                        EditorGUI.BeginChangeCheck();
                        AriadneComparison comparison = (AriadneComparison)EditorGUILayout.EnumPopup(editingEventParts.comparisonOperator);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(this, UndoName);
                            editingEventParts.comparisonOperator = comparison;
                        }

                        EditorGUI.BeginChangeCheck();
                        int startNum = EditorGUILayout.IntField(editingEventParts.startNum);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(this, UndoName);
                            editingEventParts.startNum = startNum;
                        }

                    }
                    EditorGUILayout.EndHorizontal();
                } 
            }
            EditorGUILayout.EndVertical();
        }

        /// <Summary>
        /// Show setting GUI for executed flag and name of the flag.
        /// </Summary>
        void ShowEventExecutedFlagParts()
        {
            EditorGUILayout.BeginVertical("Box");
            {
                EditorGUILayout.LabelField("Executed Flag", boldLabelStyle);

                EditorGUI.BeginChangeCheck();
                bool executedFlag = EditorGUILayout.Toggle("Event has executed flag", editingEventParts.hasExecutedFlag);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(this, UndoName);
                    editingEventParts.hasExecutedFlag = executedFlag;
                }

                EditorGUI.BeginChangeCheck();
                string flagName = EditorGUILayout.TextField("Flag name", editingEventParts.executedFlagName);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(this, UndoName);
                    editingEventParts.executedFlagName = flagName;
                }

            }
            EditorGUILayout.EndVertical();
        }

        /// <Summary>
        /// Show setting GUI for start trigger and start position of the event.
        /// </Summary>
        void ShowEventTriggerAndStartPositionParts()
        {
            EditorGUILayout.BeginVertical("Box");
            {
                EditorGUILayout.LabelField("Trigger and Position", boldLabelStyle);

                EditorGUI.BeginChangeCheck();
                AriadneEventTrigger eventTrigger = (AriadneEventTrigger)EditorGUILayout.EnumPopup("Event Trigger", editingEventParts.eventTrigger);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(this, UndoName);
                    editingEventParts.eventTrigger = eventTrigger;
                }

                EditorGUI.BeginChangeCheck();
                AriadneEventPosition eventPosition = (AriadneEventPosition)EditorGUILayout.EnumPopup("Event Position", editingEventParts.eventPos);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(this, UndoName);
                    editingEventParts.eventPos = eventPosition;
                }

            }
            EditorGUILayout.EndVertical();
        }

        /// <Summary>
        /// Show setting GUI for start direction of the event.
        /// </Summary>
        void ShowStartDirectionParts()
        {
            EditorGUILayout.BeginVertical("Box");
            {
                EditorGUILayout.LabelField("Event Start Direction", boldLabelStyle);
                EditorGUI.BeginChangeCheck();
                bool dirCondition = EditorGUILayout.Toggle("Use direction condition", editingEventParts.hasDirectionCondition);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(this, "Change Direction Condition");
                    editingEventParts.hasDirectionCondition = dirCondition;
                }

                EditorGUILayout.LabelField("Event Starts When Player Towards");
                EditorGUI.BeginChangeCheck();
                bool dirNorth = EditorGUILayout.Toggle("North", conditionNorth);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(this, "Change Direction Condition");
                    conditionNorth = dirNorth;
                }

                EditorGUI.BeginChangeCheck();
                bool dirEast = EditorGUILayout.Toggle("East", conditionEast);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(this, "Change Direction Condition");
                    conditionEast = dirEast;
                }

                EditorGUI.BeginChangeCheck();
                bool dirSouth = EditorGUILayout.Toggle("South", conditionSouth);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(this, "Change Direction Condition");
                    conditionSouth = dirSouth;
                }

                EditorGUI.BeginChangeCheck();
                bool dirWest = EditorGUILayout.Toggle("West", conditionWest);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(this, "Change Direction Condition");
                    conditionWest = dirWest;
                }
            }
            EditorGUILayout.EndVertical();
        }

        /// <Summary>
        /// Check the direction condition list.
        /// </Summary>
        void CheckDirectionConditions()
        {
            if (editingEventParts.directionConditions == null)
            {
                editingEventParts.directionConditions = new List<DungeonDir>();
            }

            foreach (DungeonDir direction in editingEventParts.directionConditions)
            {
                if (direction == DungeonDir.North)
                {
                    conditionNorth = true;
                }

                if (direction == DungeonDir.East)
                {
                    conditionEast = true;
                }

                if (direction == DungeonDir.South)
                {
                    conditionSouth = true;
                }

                if (direction == DungeonDir.West)
                {
                    conditionWest = true;
                }
            }
        }

        /// <Summary>
        /// Check argument data.
        /// </Summary>
        void CheckArgumentsData()
        {
            foreach (EventProcessData processData in editingEventParts.eventProcessList)
            {
                List<EventArgs> processArgs = GetEventArgList(processData.eventCategoryId);
                foreach (EventArgs args in processArgs)
                {
                    EventArgs savedArgs = processData.eventArgs.Find(a => a.argName == args.argName && a.argTypeId == args.argTypeId);
                    if (savedArgs == null)
                    {
                        continue;
                    }

                    args.argListValue = savedArgs.argListValue;
                    args.argObjListValue = savedArgs.argObjListValue;
                }
                processData.eventArgs = processArgs;
            }
        }

        /// <Summary>
        /// Show setting GUI for the process of the event.
        /// </Summary>
        void ShowEventProcessParts()
        {
            EditorGUILayout.BeginVertical("Box");
            {
                EditorGUILayout.LabelField("Event Process", boldLabelStyle);

                if (editingEventParts.eventProcessList == null)
                {
                    editingEventParts.eventProcessList = new List<EventProcessData>();
                }

                for (int index = 0; index < editingEventParts.eventProcessList.Count; index++)
                {
                    ShowEventCategoryParts(editingEventParts.eventProcessList[index], index);
                    GUILayout.Space(EditorPreferences.ProcessIntervalHeight);
                }
                EditorGUILayout.Space();

                CheckProcessAction();
                
                if (GUILayout.Button("Add Event Process"))
                {
                    EventProcessData newProcessData = new EventProcessData();
                    newProcessData.eventCategoryId = 0;
                    newProcessData.eventArgs = GetEventArgList(newProcessData.eventCategoryId);
                    Undo.RecordObject(this, UndoName);
                    editingEventParts.eventProcessList.Add(newProcessData);
                }
            }
            EditorGUILayout.EndVertical();
        }

        /// <Summary>
        /// Check the action for parts. (Move up, move down and remove)
        /// </Summary>
        void CheckPartsAction()
        {
            // Check move up target record.
            if (moveUpTargetPartsIndex != -1)
            {
                Undo.RecordObject(this, "Move Up Parts");
                SwapListItem<AriadneEventParts>(eventPartsList, moveUpTargetPartsIndex, moveUpTargetPartsIndex - 1);
                Repaint();
            }
            moveUpTargetPartsIndex = -1;

            // Check move down target record.
            if (moveDownTargetPartsIndex != -1)
            {
                Undo.RecordObject(this, "Move Down Parts");
                SwapListItem<AriadneEventParts>(eventPartsList, moveDownTargetPartsIndex, moveDownTargetPartsIndex + 1);
                Repaint();
            }
            moveDownTargetPartsIndex = -1;

            // Check remove target record.
            if (removeTargetPartsIndex != -1)
            {
                Undo.RecordObject(this, "Remove Parts Record");
                eventPartsList.RemoveAt(removeTargetPartsIndex);
                Repaint();
            }
            removeTargetPartsIndex = -1;
        }

        /// <Summary>
        /// Check the action for process. (Move up, move down and remove)
        /// </Summary>
        void CheckProcessAction()
        {
            // Check move up target record.
            if (moveUpTargetProcessIndex != -1)
            {
                Undo.RecordObject(this, "Move Up Process");
                SwapListItem<EventProcessData>(editingEventParts.eventProcessList, moveUpTargetProcessIndex, moveUpTargetProcessIndex - 1);
                Repaint();
            }
            moveUpTargetProcessIndex = -1;

            // Check move down target record.
            if (moveDownTargetProcessIndex != -1)
            {
                Undo.RecordObject(this, "Move Down Process");
                SwapListItem<EventProcessData>(editingEventParts.eventProcessList, moveDownTargetProcessIndex, moveDownTargetProcessIndex + 1);
                Repaint();
            }
            moveDownTargetProcessIndex = -1;

            // Check remove target record.
            if (removeTargetProcessIndex != -1)
            {
                Undo.RecordObject(this, "Remove Arg Record");
                editingEventParts.eventProcessList.RemoveAt(removeTargetProcessIndex);
                Repaint();
            }
            removeTargetProcessIndex = -1;
        }

        /// <Summary>
        /// Swaps elements in specified list.
        /// </Summary>
        void SwapListItem<T>(List<T> swapList, int targetIndex, int destIndex)
        {
            if (targetIndex < 0 || targetIndex >= swapList.Count)
            {
                Debug.LogWarning("targetIndex : " + targetIndex + " is invalid. Please check index.");
                return;
            }

            if (destIndex < 0 || destIndex >= swapList.Count)
            {
                Debug.LogWarning("destIndex : " + destIndex + " is invalid. Please check index.");
                return;
            }

            T tempData = swapList[targetIndex];
            swapList[targetIndex] = swapList[destIndex];
            swapList[destIndex] = tempData;
        }

        // /// <Summary>
        // /// Swaps elements in specified list.
        // /// </Summary>
        // void SwapProcessListItem(List<EventProcessData> swapList, int targetIndex, int destIndex)
        // {
        //     if (targetIndex < 0 || targetIndex >= swapList.Count)
        //     {
        //         Debug.LogWarning("targetIndex : " + targetIndex + " is invalid. Please check index.");
        //         return;
        //     }

        //     if (destIndex < 0 || destIndex >= swapList.Count)
        //     {
        //         Debug.LogWarning("destIndex : " + destIndex + " is invalid. Please check index.");
        //         return;
        //     }

        //     EventProcessData tempData = swapList[targetIndex];
        //     swapList[targetIndex] = swapList[destIndex];
        //     swapList[destIndex] = tempData;
        // }

        /// <Summary>
        /// Show setting GUI for the category of the event.
        /// </Summary>
        void ShowEventCategoryParts(EventProcessData processData, int processIndex)
        {
            EditorGUILayout.BeginHorizontal();
            {
                // Indent
                GUILayout.Space(EditorPreferences.IndentWidth);

                EditorGUILayout.BeginVertical(processStyle);
                {
                    string label = "Process " + processIndex.ToString();
                    EditorGUILayout.LabelField(label, boldLabelStyle);
                    EditorGUI.BeginChangeCheck();
                    int categoryId = EditorGUILayout.IntPopup("Event Category", processData.eventCategoryId, eventCategoryNames, eventCategoryIds);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(this, "Change EventCategory");
                        processData.eventCategoryId = categoryId;

                        processData.eventArgs = GetEventArgList(categoryId);
                    }
                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField("Arguments", boldLabelStyle);
                    if (processData.eventArgs.Count > 0)
                    {
                        foreach (EventArgs args in processData.eventArgs)
                        {
                            ShowEventArgumentSettingParts(args);
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("This event category has no arguments.");
                    }
                    EditorGUILayout.Space();

                    // Process Buttons
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(EditorPreferences.IndentWidth);

                        if (processIndex != 0)
                        {
                            if (GUILayout.Button("Move Up", GUILayout.Width(EditorPreferences.ButtonWidth)))
                            {
                                moveUpTargetProcessIndex = processIndex;
                            }
                        }
                        
                        if (processIndex != editingEventParts.eventProcessList.Count - 1)
                        {
                            if (GUILayout.Button("Move Down", GUILayout.Width(EditorPreferences.ButtonWidth)))
                            {
                                moveDownTargetProcessIndex = processIndex;
                            }
                        }

                        if (GUILayout.Button("Remove Process", GUILayout.Width(EditorPreferences.ButtonWidth)))
                        {
                            string eventCategory = GetEventCategoryNameById(processData.eventCategoryId);
                            string confirmMsg = "Remove this event process?" + "\n"
                                                + "Process " + processIndex.ToString() + " \n"
                                                + "Event Category : " + eventCategory;
                            bool dialogChoice = EditorUtility.DisplayDialog("Confirm",
                                                                        confirmMsg,
                                                                        "Remove",
                                                                        "Cancel");
                            if (dialogChoice)
                            {
                                removeTargetProcessIndex = processIndex;
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <Summary>
        /// Set up event arg list which corresponds to the event category.
        /// </Summary>
        List<EventArgs> GetEventArgList(int categoryId)
        {
            EventCategoryRecord record = DataRecordUtil.GetEventCategoryRecordById(eventCategoryDataList, categoryId);
            if (record == null)
            {
                return new List<EventArgs>();
            }

            List<EventArgs> categoryArgs = record.eventArgs;
            if (categoryArgs == null)
            {
                Debug.LogWarning("Sprecified event category ID : " + categoryId + " is missing in the EventCategoryData.");
                return new List<EventArgs>();
            }

            List<EventArgs> processArgs = new List<EventArgs>();
            foreach (EventArgs args in categoryArgs)
            {
                EventArgs newArgs = new EventArgs();
                newArgs.argName = args.argName;
                newArgs.argTypeId = args.argTypeId;
                newArgs.isList = args.isList;
                processArgs.Add(newArgs);
            }

            return processArgs;
        }

        /// <Summary>
        /// Show setting GUI about event arguments.
        /// </Summary>
        void ShowEventArgumentSettingParts(EventArgs args)
        {
            EditorGUILayout.BeginHorizontal();
            {
                // Indent
                GUILayout.Space(EditorPreferences.IndentWidth);

                EditorGUILayout.BeginVertical("Box");
                {
                    // Argument Name
                    EditorGUILayout.LabelField("Arg Name : " + args.argName, boldLabelStyle);
                    

                    // Argument Value
                    string argTypeName = "";
                    EventArgumentTypeRecord record = DataRecordUtil.GetEventArgumentTypeRecordById(eventArgumentTypeDataList, args.argTypeId);
                    if (record != null)
                    {
                        argTypeName = record.eventArgTypeDisplayName;
                    }

                    EditorGUILayout.LabelField("Arg Type : " + argTypeName, boldLabelStyle);
                    EditorGUILayout.Space();

                    if (typeDict.ContainsKey(argTypeName))
                    {
                        var type = typeDict[argTypeName];
                        if (type != null)
                        {
                            ShowEachArgumentSettingParts(args, argTypeName, type);
                        }
                        else
                        {
                            Debug.Log("type : " + argTypeName + " is not found.");
                        }
                    }
                    else
                    {
                        Debug.Log("argTypeName : " + argTypeName + " is not found in arg types. Check EventCategoryEditor and EventArgumentTypeEditor.");
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <Summary>
        /// Show setting GUI about event arguments.
        /// </Summary>
        void ShowEachArgumentSettingParts(EventArgs args, string argTypeName, Type argType)
        {
            bool isUnityObject = typeIsUnityObjDict[argTypeName];
            if (args.isList)
            {
                if (isUnityObject)
                {
                    ShowObjectListArgParts(args, argType);
                }
                else
                {
                    ShowValueListArgParts(args, argType);
                }
            }
            else
            {
                if (isUnityObject)
                {
                    ShowObjectArgParts(args, argType);
                }
                else
                {
                    ShowValueArgParts(args, argType);
                }
            }
            EditorGUILayout.Space();

            // Check remove target record.
            if (removeTargetObjectIndex != -1)
            {
                Undo.RecordObject(this, "Remove Arg Record");
                args.argObjListValue.RemoveAt(removeTargetObjectIndex);
                Repaint();
            }
            removeTargetObjectIndex = -1;

            if (removeTargetValueIndex != -1)
            {
                Undo.RecordObject(this, "Remove Arg Record");
                args.argListValue.RemoveAt(removeTargetValueIndex);
                Repaint();
            }
            removeTargetValueIndex = -1;
        }

        /// <Summary>
        /// Check if the type of argument is derived from UnityEngine.Object.
        /// </Summary>
        bool CheckIfUnityObject(Type type)
        {
            bool isUnityObj = false;

            var derived = type;
            while (derived != null)
            {
                derived = derived.BaseType;

                if (derived != null)
                {
                    if (derived == typeof(UnityEngine.Object))
                    {
                        isUnityObj = true;
                    }
                }
            }

            return isUnityObj;
        }

        /// <Summary>
        /// Returns the type object which corresponds to specified name.
        /// </Summary>
        Type GetTypeByName(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type != null)
            {
                return type;
            }

            string assemblyName = "UnityEngine.dll";
            type = System.Reflection.Assembly.Load(assemblyName).GetType(typeName);
            if (type != null)
            {
                return type;
            }

            assemblyName = "Assembly-CSharp";
            type = System.Reflection.Assembly.Load(assemblyName).GetType(typeName);
            return type;
        }

        /// <Summary>
        /// Show setting GUI about event arguments list.
        /// </Summary>
        void ShowValueListArgParts(EventArgs args, Type argType)
        {
            CheckListValueState(args);

            for (int index = 0; index < args.argListValue.Count; index++)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    ShowEventArgumentTypeParts(args, argType, index);

                    if (args.argListValue.Count > 1)
                    {
                        // Event Arg Remove Button
                        if (GUILayout.Button(EditorStyles.RemoveButtonContent, GUILayout.Width(EditorPreferences.ButtonWidth)))
                        {
                            removeTargetValueIndex = index;
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add Value", GUILayout.Width(EditorPreferences.ButtonWidth)))
            {
                args.argListValue.Add("");
            }
        }

        /// <Summary>
        /// Show setting GUI about an event argument.
        /// </Summary>
        void ShowValueArgParts(EventArgs args, Type argType)
        {
            CheckListValueState(args);

            if (args.argListValue.Count > 1)
            {
                args.argListValue.RemoveRange(1, args.argListValue.Count - 1);
            }

            for (int index = 0; index < args.argListValue.Count; index++)
            {
                ShowEventArgumentTypeParts(args, argType, index);
            }
        }

        /// <Summary>
        /// Show setting GUI about event arguments object list.
        /// </Summary>
        void ShowObjectListArgParts(EventArgs args, Type argType)
        {
            CheckListObjectState(args);

            for (int index = 0; index < args.argObjListValue.Count; index++)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    ShowObjectFieldParts(args, argType, index);

                    if (args.argObjListValue.Count > 1)
                    {
                        // Event Arg Remove Button
                        if (GUILayout.Button(EditorStyles.RemoveButtonContent, GUILayout.Width(EditorPreferences.ButtonWidth)))
                        {
                            removeTargetObjectIndex = index;
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add Value", GUILayout.Width(EditorPreferences.ButtonWidth)))
            {
                args.argObjListValue.Add(null);
            }
        }

        /// <Summary>
        /// Show setting GUI about an event argument object.
        /// </Summary>
        void ShowObjectArgParts(EventArgs args, Type argType)
        {
            CheckListObjectState(args);

            if (args.argObjListValue.Count > 1)
            {
                args.argObjListValue.RemoveRange(1, args.argObjListValue.Count - 1);
            }

            for (int index = 0; index < args.argObjListValue.Count; index++)
            {
                ShowObjectFieldParts(args, argType, index);
            }
        }

        /// <Summary>
        /// Check the value list field is initialized.
        /// </Summary>
        void CheckListValueState(EventArgs args)
        {
            if (args.argListValue == null)
            {
                args.argListValue = new List<string>();
            }

            if (args.argListValue.Count < 1)
            {
                args.argListValue.Add("");
            }
        }

        /// <Summary>
        /// Check the object list field is initialized.
        /// </Summary>
        void CheckListObjectState(EventArgs args)
        {
            if (args.argObjListValue == null)
            {
                args.argObjListValue = new List<UnityEngine.Object>();
            }

            if (args.argObjListValue.Count < 1)
            {
                args.argObjListValue.Add(null);
            }
        }

        /// <Summary>
        /// Show setting GUI about event arguments by type.
        /// </Summary>
        void ShowEventArgumentTypeParts(EventArgs args, Type argType, int argIndex)
        {
            string label = "Value";
            if (argIndex > 0)
            {
                label += argIndex.ToString();
            }

            string argUndoName = "Change Arg Value";
            string specifier = "G";

            // Show an appropriate parts.
            if (argType == typeof(int))
            {
                if (args.argListValue[argIndex] == "" || args.argListValue[argIndex] == null)
                {
                    args.argListValue[argIndex] = "0";
                }

                int castValue = 0;
                bool success = int.TryParse(args.argListValue[argIndex], out castValue);
                if (!success)
                {
                    Debug.LogWarning("Casting to 'int' has been failed. The argument field is initialized.");
                    castValue = 0;
                }

                EditorGUI.BeginChangeCheck();
                int value = EditorGUILayout.IntField(label, castValue);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(this, argUndoName);
                    args.argListValue[argIndex] = value.ToString();
                }
                return;
            }

            if (argType == typeof(long))
            {
                if (args.argListValue[argIndex] == "" || args.argListValue[argIndex] == null)
                {
                    args.argListValue[argIndex] = "0";
                }

                long castValue = 0L;
                bool success = long.TryParse(args.argListValue[argIndex], out castValue);
                if (!success)
                {
                    Debug.LogWarning("Casting to 'long' has been failed. The argument field is initialized.");
                    castValue = 0L;
                }

                EditorGUI.BeginChangeCheck();
                long value = EditorGUILayout.LongField(label, castValue);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(this, argUndoName);
                    args.argListValue[argIndex] = value.ToString();
                }
                return;
            }

            if (argType == typeof(float))
            {
                if (args.argListValue[argIndex] == "" || args.argListValue[argIndex] == null)
                {
                    args.argListValue[argIndex] = "0.0";
                }

                float castValue = 0f;
                bool success = float.TryParse(args.argListValue[argIndex], out castValue);
                if (!success)
                {
                    Debug.LogWarning("Casting to 'float' has been failed. The argument field is initialized.");
                    castValue = 0f;
                }

                EditorGUI.BeginChangeCheck();
                float value = EditorGUILayout.FloatField(label, castValue);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(this, argUndoName);
                    args.argListValue[argIndex] = value.ToString();
                }
                return;
            }

            if (argType == typeof(double))
            {
                if (args.argListValue[argIndex] == "" || args.argListValue[argIndex] == null)
                {
                    args.argListValue[argIndex] = "0.0";
                }

                double castValue = 0d;
                bool success = double.TryParse(args.argListValue[argIndex], out castValue);
                if (!success)
                {
                    Debug.LogWarning("Casting to 'double' has been failed. The argument field is initialized.");
                    castValue = 0d;
                }

                EditorGUI.BeginChangeCheck();
                double value = EditorGUILayout.DoubleField(label, castValue);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(this, argUndoName);
                    args.argListValue[argIndex] = value.ToString();
                }
                return;
            }

            if (argType == typeof(string))
            {
                if (args.argListValue[argIndex] == "" || args.argListValue[argIndex] == null)
                {
                    args.argListValue[argIndex] = "";
                }

                EditorGUI.BeginChangeCheck();
                string value = EditorGUILayout.TextField(label, args.argListValue[argIndex]);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(this, argUndoName);
                    args.argListValue[argIndex] = value;
                }
                return;
            }

            if (argType == typeof(bool))
            {
                if (args.argListValue[argIndex] == "" || args.argListValue[argIndex] == null)
                {
                    args.argListValue[argIndex] = "False";
                }

                bool castValue = false;
                bool success = bool.TryParse(args.argListValue[argIndex], out castValue);
                if (!success)
                {
                    Debug.LogWarning("Casting to 'bool' has been failed. The argument field is initialized.");
                    castValue = false;
                }

                EditorGUI.BeginChangeCheck();
                bool value = EditorGUILayout.Toggle(label, castValue);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(this, argUndoName);
                    args.argListValue[argIndex] = value.ToString();
                }
                return;
            }

            if (argType == typeof(Vector2Int))
            {
                if (args.argListValue[argIndex] == "" || args.argListValue[argIndex] == null)
                {
                    args.argListValue[argIndex] = Vector2Int.zero.ToString();
                }
                Vector2Int castValue = ArgValueCaster.GetVector2IntValue(args.argListValue[argIndex]);

                EditorGUI.BeginChangeCheck();
                Vector2Int value = EditorGUILayout.Vector2IntField(label, castValue);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(this, argUndoName);
                    args.argListValue[argIndex] = value.ToString();
                }
                return;
            }

            if (argType == typeof(Vector2))
            {
                if (args.argListValue[argIndex] == "" || args.argListValue[argIndex] == null)
                {
                    args.argListValue[argIndex] = Vector2.zero.ToString();
                }
                Vector2 castValue = ArgValueCaster.GetVector2Value(args.argListValue[argIndex]);

                EditorGUI.BeginChangeCheck();
                Vector2 value = EditorGUILayout.Vector2Field(label, castValue);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(this, argUndoName);
                    args.argListValue[argIndex] = value.ToString(specifier);
                }
                return;
            }

            if (argType == typeof(Vector3Int))
            {
                if (args.argListValue[argIndex] == "" || args.argListValue[argIndex] == null)
                {
                    args.argListValue[argIndex] = Vector3Int.zero.ToString();
                }
                Vector3Int castValue = ArgValueCaster.GetVector3IntValue(args.argListValue[argIndex]);

                EditorGUI.BeginChangeCheck();
                Vector3Int value = EditorGUILayout.Vector3IntField(label, castValue);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(this, argUndoName);
                    args.argListValue[argIndex] = value.ToString();
                }
                return;
            }

            if (argType == typeof(Vector3))
            {
                if (args.argListValue[argIndex] == "" || args.argListValue[argIndex] == null)
                {
                    args.argListValue[argIndex] = Vector3.zero.ToString();
                }
                Vector3 castValue = ArgValueCaster.GetVector3Value(args.argListValue[argIndex]);

                EditorGUI.BeginChangeCheck();
                Vector3 value = EditorGUILayout.Vector3Field(label, castValue);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(this, argUndoName);
                    args.argListValue[argIndex] = value.ToString(specifier);
                }
                return;
            }

            if (argType == typeof(Vector4))
            {
                if (args.argListValue[argIndex] == "" || args.argListValue[argIndex] == null)
                {
                    args.argListValue[argIndex] = Vector4.zero.ToString();
                }
                Vector4 castValue = ArgValueCaster.GetVector4Value(args.argListValue[argIndex]);

                EditorGUI.BeginChangeCheck();
                Vector4 value = EditorGUILayout.Vector4Field(label, castValue);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(this, argUndoName);
                    args.argListValue[argIndex] = value.ToString(specifier);
                }
                return;
            }

            if (argType == typeof(Color))
            {
                if (args.argListValue[argIndex] == "" || args.argListValue[argIndex] == null)
                {
                    args.argListValue[argIndex] = Color.black.ToString();
                }
                Color castValue = ArgValueCaster.GetColorValue(args.argListValue[argIndex]);

                EditorGUI.BeginChangeCheck();
                Color value = EditorGUILayout.ColorField(label, castValue);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(this, argUndoName);
                    args.argListValue[argIndex] = value.ToString();
                }
                return;
            }

            if (argType.IsEnum)
            {
                string argTypeName = "";
                EventArgumentTypeRecord record = DataRecordUtil.GetEventArgumentTypeRecordById(eventArgumentTypeDataList, args.argTypeId);
                if (record != null)
                {
                    argTypeName = record.eventArgTypeDisplayName;
                }

                int[] targetEnumValues = enumValueDict[argTypeName];
                string[] targetEnumNames = enumNameDict[argTypeName];

                if (args.argListValue[argIndex] == "" || args.argListValue[argIndex] == null)
                {
                    int defaultValue = targetEnumValues[0];
                    args.argListValue[argIndex] = defaultValue.ToString();
                }

                int castValue = 0;
                bool success = int.TryParse(args.argListValue[argIndex], out castValue);
                if (!success)
                {
                    Debug.LogWarning("Casting enum value to 'int' has been failed. The argument field is initialized.");
                    castValue = 0;
                }

                EditorGUI.BeginChangeCheck();
                int value = EditorGUILayout.IntPopup(label, castValue, targetEnumNames, targetEnumValues);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(this, argUndoName);
                    args.argListValue[argIndex] = value.ToString();
                }
                return;
            }
        }

        /// <Summary>
        /// Show setting GUI about object field.
        /// </Summary>
        void ShowObjectFieldParts(EventArgs args, Type argType, int argIndex)
        {
            string label = "Value";
            if (argIndex > 0)
            {
                label += argIndex.ToString();
            }

            string argUndoName = "Change Arg Value";

            EditorGUI.BeginChangeCheck();
            var objectValue = EditorGUILayout.ObjectField(label, args.argObjListValue[argIndex], argType, false);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(this, argUndoName);
                args.argObjListValue[argIndex] = objectValue;
            }
        }

        /// <Summary>
        /// Show apply button and cancel button.
        /// </Summary>
        void ShowApplyAndCancelButtonParts()
        {
            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Cancel"))
                    {
                        selectedParts = -1;
                    }

                    GUIStyle applyStyle = new GUIStyle(GUI.skin.button);
                    applyStyle.fontStyle = FontStyle.Bold;

                    if (GUILayout.Button("Apply", applyStyle))
                    {
                        SetDirectionConditionsToList();
                        eventPartsList[selectedParts] = editingEventParts;
                        editingEventParts = new AriadneEventParts(eventPartsList[selectedParts]);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        /// <Summary>
        /// Clear Undo objects.
        /// </Summary>
        void CloseWindowProcess()
        {
            Undo.FlushUndoRecordObjects();
            Close();
        }

        /// <Summary>
        /// Initialize settings of event editor.
        /// </Summary>
        public void InitializeEvent(int selectedEventId, EventMasterData eventMasterData)
        {
            SetEventId(selectedEventId);

            LoadDefaultEventCategoryData();
            LoadDefaultEventArgumentTypeData();
            SetFilePathStrings();
            InitializeProperties();
            SetEventData(eventMasterData);

            SetUpArgTypeDictionary();
            SetUpArgTypeIsUnityObjectDictionary();
            SetUpArgTypeEnumArrayDictionary();

            SetUpProcessBgColor();
            InitializeStyles();
            SetCallbackForUndo();
            ClearUndoStack();
        }

        /// <Summary>
        /// Set event id to EventEditor.
        /// </Summary>
        /// <param name="eventId">Event ID provided in MapEditor.</param>
        void SetEventId(int eventId)
        {
            this.eventId = eventId;
        }

        /// <Summary>
        /// Set event data to EventEditor.
        /// </Summary>
        /// <param name="eventMasterData">EventMasterData provided in MapEditor.</param>
        void SetEventData(EventMasterData eventMasterData)
        {
            this.editingEventData = eventMasterData;
            this.eventName = eventMasterData.eventName;
            this.eventPartsList = GetEventPartsListForEdit(eventMasterData.eventParts);
        }

        /// <Summary>
        /// Get file path from MapEditorUtil and set them to EventEditor.
        /// </Summary>
        void SetFilePathStrings()
        {
            eventName = MapEditorUtil.EventDefaultName;
        }

        /// <Summary>
        /// Register action for the callback of Undo/Redo.
        /// </Summary>
        void SetCallbackForUndo()
        {
            Undo.undoRedoPerformed += () => {
                Repaint();
            };
        }

        /// <Summary>
        /// Clear Undo/Redo stack to prevent setting data of another file.
        /// </Summary>
        void ClearUndoStack()
        {
            Undo.FlushUndoRecordObjects();
            Undo.ClearAll();
        }

        /// <Summary>
        /// Set up the background color of process boxes.
        /// </Summary>
        void SetUpProcessBgColor()
        {
            highlightColors = new Color[1]{highlightColor};
            highlightTex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            highlightTex.SetPixels(0, 0, 1, 1, highlightColors);
            highlightTex.Apply();

            processStyle.normal.background = highlightTex;
        }

        /// <Summary>
        /// Ready event parts for edit to avoid changing ScriptableObject directly.
        /// </Summary>
        List<AriadneEventParts> GetEventPartsListForEdit(List<AriadneEventParts> partsList)
        {
            List<AriadneEventParts> partsListForEdit = new List<AriadneEventParts>();
            if (partsList == null)
            {
                return partsListForEdit;
            }

            foreach (AriadneEventParts parts in partsList)
            {
                if (parts == null)
                {
                    continue;
                }
                AriadneEventParts partsForEdit = new AriadneEventParts(parts);
                partsListForEdit.Add(partsForEdit);
            }
            return partsListForEdit;
        }

        /// <Summary>
        /// Save EventMasterData as ScriptableObject.
        /// </Summary>
        void SaveEventFile()
        {
            var eventData = ScriptableObject.CreateInstance<EventMasterData>();
            eventData.eventId = this.eventId;
            eventData.eventName = this.eventName;
            eventData.eventParts = this.eventPartsList;

            string path = AssetDatabase.GetAssetPath(editingEventData);
            if (path.Length == 0)
            {
                return;
            }

            var asset = (EventMasterData)AssetDatabase.LoadAssetAtPath(path, typeof(EventMasterData));
            if (asset == null)
            {
                AssetDatabase.CreateAsset(eventData, path);
            }
            else
            {
                EditorUtility.CopySerialized(eventData, asset);
                AssetDatabase.SaveAssets();
            }
            AssetDatabase.Refresh();

            editingEventData = asset;
        }

        /// <Summary>
        /// Initialize properties of EventEditor.
        /// </Summary>
        void InitializeProperties()
        {
            this.eventName = MapEditorUtil.EventDefaultName;
            this.eventPartsList = new List<AriadneEventParts>();
            this.editingEventParts = null;

            SetEventCategoryList();
            GetTreasureItemList();
            GetExecutedFlagList();
        }

        /// <Summary>
        /// Set GUIStyles of labels and buttons.
        /// </Summary>
        void InitializeStyles()
        {
            removeStyle = new GUIStyle(GUI.skin.button);
            removeStyle.normal.textColor = removeColor;
            removeStyle.fontStyle = FontStyle.Bold;

            editStyle = new GUIStyle(GUI.skin.button);
            editStyle.fontStyle = FontStyle.Bold;

            boldLabelStyle = new GUIStyle(GUI.skin.label);
            boldLabelStyle.fontStyle = FontStyle.Bold;
        }

        /// <Summary>
        /// Set event category lists from EventCategoryData.
        /// </Summary>
        void SetEventCategoryList()
        {
            List<int> eventCategoryIdList = new List<int>();
            List<string> eventCategoryNameList = new List<string>();

            if (eventCategoryDataList == null)
            {
                return;
            }

            foreach (EventCategoryData data in eventCategoryDataList)
            {
                if (data == null)
                {
                    continue;
                }

                if (data.eventCategoryRecords == null)
                {
                    continue;
                }

                foreach (EventCategoryRecord record in data.eventCategoryRecords)
                {
                    eventCategoryIdList.Add(record.eventCategoryId);
                    eventCategoryNameList.Add(record.eventCategoryName);
                }
            }

            eventCategoryIds = eventCategoryIdList.ToArray();
            eventCategoryNames = eventCategoryNameList.ToArray();
        }

        /// <Summary>
        /// Get an item list from item data.
        /// </Summary>
        void GetTreasureItemList()
        {
            List<int> itemIdList = new List<int>();
            List<string> itemNameList = new List<string>();

            string filter = "t:" + typeof(ItemMasterData).ToString();
            string[] guids = AssetDatabase.FindAssets(filter, null);

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ItemMasterData itemData = AssetDatabase.LoadAssetAtPath<ItemMasterData>(path);
                if (itemData == null)
                {
                    continue;
                }
                itemIdList.Add(itemData.itemId);
                itemNameList.Add(itemData.itemName);
                
            }
            itemIds = itemIdList.ToArray();
            itemNames = itemNameList.ToArray();
        }

        /// <Summary>
        /// Get executed flag list from event data.
        /// </Summary>
        void GetExecutedFlagList()
        {
            List<string> flagNameList = new List<string>();

            string filter = "t:" + typeof(EventMasterData).ToString();
            string[] guids = AssetDatabase.FindAssets(filter, null);

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                EventMasterData eventData = AssetDatabase.LoadAssetAtPath<EventMasterData>(path);
                if (eventData == null)
                {
                    continue;
                }

                if (eventData.eventParts == null)
                {
                    continue;
                }

                foreach (AriadneEventParts parts in eventData.eventParts)
                {
                    if (parts == null)
                    {
                        continue;
                    }

                    if (parts.hasExecutedFlag && parts.executedFlagName != "")
                    {
                        flagNameList.Add(parts.executedFlagName);
                    }
                }
            }
            flagNames = flagNameList.ToArray();
        }

        /// <Summary>
        /// Load EventCategoryData files in the project.
        /// </Summary>
        void LoadDefaultEventCategoryData()
        {
            // Load EventCategoryData files.
            string filter = "t:" + typeof(EventCategoryData).ToString();
            string[] guids = AssetDatabase.FindAssets(filter, null);

            eventCategoryDataList = new List<EventCategoryData>();
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                EventCategoryData eventCategoryData = AssetDatabase.LoadAssetAtPath<EventCategoryData>(assetPath);
                if (eventCategoryData != null)
                {
                    eventCategoryDataList.Add(eventCategoryData);
                }
            }
        }

        /// <Summary>
        /// Load EventArgumentTypeData files in the project.
        /// </Summary>
        void LoadDefaultEventArgumentTypeData()
        {
            // Load EventArgumentTypeData files.
            string filter = "t:" + typeof(EventArgumentTypeData).ToString();
            string[] guids = AssetDatabase.FindAssets(filter, null);

            eventArgumentTypeDataList = new List<EventArgumentTypeData>();
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                EventArgumentTypeData eventArgumentTypeData = AssetDatabase.LoadAssetAtPath<EventArgumentTypeData>(assetPath);
                if (eventArgumentTypeData != null)
                {
                    eventArgumentTypeDataList.Add(eventArgumentTypeData);
                }
            }
        }

        /// <Summary>
        /// Returns an event category name.
        /// </Summary>
        /// <param name="categoryId">ID of event category.</param>
        string GetEventCategoryNameById(int categoryId)
        {
            string eventCategory = "";
            if (eventCategoryDataList == null)
            {
                return eventCategory;
            }

            EventCategoryRecord record = DataRecordUtil.GetEventCategoryRecordById(eventCategoryDataList, categoryId);
            if (record != null)
            {
                eventCategory = record.eventCategoryName;
            }
            return eventCategory;
        }

        /// <Summary>
        /// Set direction conditions to the event parts data.
        /// </Summary>
        void SetDirectionConditionsToList()
        {
            List<DungeonDir> directionConditions = new List<DungeonDir>();
            if (conditionNorth)
            {
                directionConditions.Add(DungeonDir.North);
            }

            if (conditionEast)
            {
                directionConditions.Add(DungeonDir.East);
            }

            if (conditionSouth)
            {
                directionConditions.Add(DungeonDir.South);
            }

            if (conditionWest)
            {
                directionConditions.Add(DungeonDir.West);
            }

            editingEventParts.directionConditions = directionConditions;
        }

        /// <Summary>
        /// Set up the dictionary which holds event argument types.
        /// </Summary>
        void SetUpArgTypeDictionary()
        {
            typeDict = new Dictionary<string, Type>();

            if (eventArgumentTypeDataList == null)
            {
                return;
            }

            foreach (EventArgumentTypeData data in eventArgumentTypeDataList)
            {
                if (data == null)
                {
                    continue;
                }

                if (data.eventArgTypeRecords == null)
                {
                    continue;
                }

                foreach (EventArgumentTypeRecord record in data.eventArgTypeRecords)
                {
                    if (record.eventArgTypeDisplayName == "" || record.eventArgTypeName == "")
                    {
                        continue;
                    }

                    string typeName = record.eventArgTypeName;
                    Type type = GetTypeByName(typeName);
                    if (!typeDict.ContainsKey(record.eventArgTypeDisplayName))
                    {
                        typeDict.Add(record.eventArgTypeDisplayName, type);
                    }
                }
            }
        }

        /// <Summary>
        /// Set up the dictionary which holds bool values which represent that event arg is derived from UnityEngine.Object.
        /// </Summary>
        void SetUpArgTypeIsUnityObjectDictionary()
        {
            typeIsUnityObjDict = new Dictionary<string, bool>();
            foreach (KeyValuePair<string, Type> pair in typeDict)
            {
                bool isUnityObj = CheckIfUnityObject(pair.Value);
                typeIsUnityObjDict.Add(pair.Key, isUnityObj);
            }
        }

        /// <Summary>
        /// Set up the dictionary which holds id arrays and name arrays of enum types.
        /// </Summary>
        void SetUpArgTypeEnumArrayDictionary()
        {
            enumValueDict = new Dictionary<string, int[]>();
            enumNameDict = new Dictionary<string, string[]>();

            foreach (KeyValuePair<string, Type> pair in typeDict)
            {
                if (!pair.Value.IsEnum)
                {
                    continue;
                }

                int[] enumValues = (int[])Enum.GetValues(pair.Value);
                string[] enumNames = (string[])Enum.GetNames(pair.Value);
                enumValueDict.Add(pair.Key, enumValues);
                enumNameDict.Add(pair.Key, enumNames);
            }
        }
    }

    /// <Summary>
    /// Struct to hold floor ID and name.
    /// </Summary>
    public struct DungeonFloorStruct
    {
        public int[] floorIds;
        public string[] floorNames;
/*
        /// <Summary>
        /// Constructor of DungeonFloorStruct.
        /// </Summary>
        /// <param name="dungeonData">Dungeon data to get floor list.</param>
        public DungeonFloorStruct(DungeonMasterData dungeonData)
        {
            List<int> floorIdList = new List<int>();
            List<string> floorNameList = new List<string>();
            if (dungeonData != null)
            {
                foreach (FloorMapMasterData floor in dungeonData.floorList)
                {
                    floorIdList.Add(floor.floorId);
                    floorNameList.Add(floor.name);
                }
            }
            floorIds = floorIdList.ToArray();
            floorNames = floorNameList.ToArray();
        }
*/
    }
}