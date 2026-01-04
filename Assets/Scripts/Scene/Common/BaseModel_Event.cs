using System;
using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public partial class BaseModel
    {
        public List<StageEventData> StageEventDates => CurrentStage.GetStageEvents();
    
        public List<StageEventData> StageEvents(EventTiming eventTiming)
        {
            var eventKeys = CurrentGameInfo.ReadEventKeys;
            return StageEventDates.FindAll(a => a.Timing == eventTiming && !eventKeys.Contains(a.EventKey));
        }

        public List<StageEventData> StageEvents(EventTiming eventTiming, int positionX, int positionY)
        {
            var eventKeys = CurrentGameInfo.ReadEventKeys;
            return StageEventDates.FindAll(a => a.Timing == eventTiming && a.PositionX == positionX && a.PositionY == positionY && !eventKeys.Contains(a.EventKey));
        }

        public List<StageEventData> EndStageEvents()
        {
            var eventKeys = CurrentGameInfo.ReadEventKeys;
            return StageEventDates.FindAll(a => eventKeys.Contains(a.EventKey));
        }

        public List<StageEventData> NotEndStageEvents()
        {
            var eventKeys = CurrentGameInfo.ReadEventKeys;
            return StageEventDates.FindAll(a => !eventKeys.Contains(a.EventKey));
        }

        public void AddEventReadFlag(StageEventData stageEventDates)
        {
            if (!stageEventDates.ReadFlag)
            {
                return;
            }
            CurrentGameInfo.AddEventReadFlag(stageEventDates.EventKey);
        }

        public void AddEventReadFlag(string eventKey)
        {
            CurrentGameInfo.AddEventReadFlag(eventKey);
        }

        public void AddEventReadFlagForce(StageEventData stageEventDates)
        {
            AddEventReadFlag(stageEventDates.EventKey);
        }

        public List<AdvData> GetAdvDates(EventTiming eventTiming, bool checkReadKeys = true, Func<AdvData, bool> func = null)
        {
            var eventKeys = CurrentGameInfo.ReadEventKeys;
            if (!checkReadKeys)
            {
                eventKeys = new List<string>();
            }
            var events = DataSystem.Adventures.FindAll(a => a.Timing == eventTiming && !eventKeys.Contains(a.EventKey));
            if (func != null)
            {
                events = events.FindAll(a => func(a));
            }
            return events;
        }

        public string GetAdvFile(int id)
        {
            var adventureFile = DataSystem.Adventures.Find(a => a.Id == id);
            if (adventureFile == null)
            {
                return "";
            }
            if (adventureFile.PrizeSetId > 0)
            {
                var prizeSets = DataSystem.PrizeSets.FindAll(a => a.Id == adventureFile.PrizeSetId);
                foreach (var prizeSet in prizeSets)
                {
                    var getItemInfo = new GetItemInfo(prizeSet.GetItem);
                    AddGetItemInfo(getItemInfo);
                }
            }
            return adventureFile.AdvName;
        }
    }
}
