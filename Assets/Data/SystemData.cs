using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
	public class SystemData : ScriptableObject
	{	
		public List<CommandData> TacticsCommandData;
		public List<CommandData> StatusCommandData;
		public List<OptionCommand> OptionCommandData;
		public List<CommandData> TitleCommandData;
		public int InitCurrency;
		public int TrainCount;
		public int AlchemyCount;
		public int RecoveryCount;
		public int BattleCount;
		public int ResourceCount;
		public int AlcanaSelectCount;
		public int BattleBonusValue;
		public int WeakPointRate;
		public int StartStageId;
		public List<TextData> SystemTextData;


		public List<InputData> InputDataList;
		public TextData GetTextData(int id)
		{
			var textData = SystemTextData.Find(a => a.Id == id);
			if (textData != null) 
			{
				return textData;
			}
			return null;
		}

		public string GetReplaceText(int id,string replace)
		{
			var textData = SystemTextData.Find(a => a.Id == id);
			if (textData != null) 
			{
				return textData.Text.Replace("\\d",replace);
			}
			return "";
		}

		[Serializable]
		public class CommandData
		{
			
			public int Id;
			public string Key;
			public string Name;
			public string Help;
		}

		[Serializable]
		public class OptionCommand
		{
			public int Id;
			public string Key;
			public string Name;
			public string Help;
			public int Category;
			public OptionButtonType ButtonType;
			public int ToggleText1;
			public int ToggleText2;
			public int ToggleText3;
			public bool ExistWindows;
			public bool ExistAndroid;
		}

		[Serializable]
		public class InputData
		{
			public string Key;
			public int KeyId;
			public string Name;
		}
	}

	public enum TacticsCommandType 
	{
		None,
		Paradigm = 1,
		Status = 2,
		Train,
		Alchemy,
		Symbol,
	}

	public enum TitleCommandType 
	{
		NewGame = 1,
		Continue,
	}


	public enum StatusCommandType 
	{
		SkillActionList = 0,
		Strength = 1,
	}

	public enum ConfirmCommandType 
	{
		Yes = 0,
		No = 1,
	}

	[Serializable]
	public class GetItemData
	{   
		public GetItemType Type;
		public int Param1;
		public int Param2;
	}

	public enum GetItemType
	{
		None = 0,
		Currency = 2,
		Demigod = 3,
		ReBirth = 4,
		Exp = 5,
		AddActor = 11,
		LevelUp = 21,
		StatusUp = 22,
		Regeneration = 31,
		//LearnSkill = 41,
		SelectAddActor = 52,
		BattleScoreBonus = 61,
		BattleNuminosBonus = 62,
		BattleEnemyLvUp = 63,
		Skill = 100,
		SelectRelic = 101,
		SelectSkill = 102,
		//ParallelHistory = 102,
		//Multiverse = 103,
		LvLink = 104,
		Ending = 210,
	}
}