using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Ariadne
{
    /// <Summary>
    /// Consts for editor scripts.
    /// </Summary>
    public static class EditorPreferences
    {
        public static readonly int LabelWidth = 100;
        public static readonly int NameLabelWidth = 150;
        public static readonly int IdLabelWidth = 50;
        public static readonly int IconLabelWidth = 150;
        public static readonly int ButtonWidth = 100;
        public static readonly int MapIconSize = 24;
        public static readonly int AttributeRecordHeight = 36;
        public static readonly int RemoveRecordButtonWidth = 32;
        public static readonly int IndentWidth = 20;
        public static readonly int ProcessIntervalHeight = 20;
        public static readonly int ChangeIdWidth = 300;
    }

    public static class EditorStyles
    {
        public static readonly GUIContent SaveButtonContent = new GUIContent("Save");
        public static readonly GUIContent AddButtonContent = new GUIContent("Add");
        public static readonly GUIContent RemoveButtonContent = new GUIContent("Remove");
        public static readonly GUIContent SortButtonContent = new GUIContent("Sort");
        public static readonly GUIContent RemoveRecordButtonContent = new GUIContent("x");
        public static readonly GUIContent ChangeIdButtonContent = new GUIContent("Change ID");
        public static readonly GUIContent ApplyButtonContent = new GUIContent("Apply");
        public static readonly GUIContent AddArgumentButtonContent = new GUIContent("Add Arg");
        public static readonly GUIContent RemoveArgumentButtonContent = new GUIContent("Remove Arg");
    }

    public static class DefaultAttribute
    {
        public static readonly string DefaultMapAttrDataName = "MapAttributeData";
        public static readonly int AttrId = 1000;
        public static readonly string AttrName = "Hallway";
    }

    public static class DefaultEventCategory
    {
        public static readonly string DefaultEventCategoryDataName = "EventCategoryData";
        public static readonly int EventCategoryId = 1000;
        public static readonly string EventCategoryName = "None";
    }

    public static class DefaultEventArgumentType
    {
        public static readonly string DefaultEventArgumentTypeDataName = "EventArgumentTypeData";
        public static readonly int EventArgumentTypeId = 1000;
        public static readonly string EventArgumentTypeName = "Argument";
    }

    public static class DefaultEventDataList
    {
        public static readonly string DefaultEventDataListName = "EventDataList";
    }

    public static class DefaultDungeonPartsData
    {
        public static readonly string DefaultDungeonPartsName = "DungeonParts";
        public static readonly string DefaultDungeonBasePartsName = "DungeonBaseParts";
    }

    public static class MapEditorTools
    {
        public const int Select = 0;
        public const int Draw = 1;
        public const int Rect = 2;
        public const int FillRect = 3;
        public static readonly string SelectIconName = "tool_select";
        public static readonly string DrawIconName = "tool_pen";
        public static readonly string RectIconName = "tool_rect";
        public static readonly string FillRectIconName = "tool_rect_fill";
        public static readonly string EventIconName = "event";
        public static readonly string EntranceIconName = "entrance";
        public static readonly string WarningIconName = "warning";
        public static readonly int IconWidth = 32;
        public static readonly int IconHeight = 32;
    }

    public static class AriadneEventSettings
    {
        public static readonly string IdDigits = "D6";
    }
}