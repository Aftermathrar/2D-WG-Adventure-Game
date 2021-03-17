using System.Collections;
using System.Collections.Generic;
using ButtonGame.Attributes;
using ButtonGame.Control;
using ButtonGame.Locations;
using ButtonGame.Saving;
using ButtonGame.Stats.Enums;
using UnityEngine;

namespace ButtonGame.Core
{
    public class FeedeeManager : MonoBehaviour, ISaveable
    {
        [SerializeField] MenuManager nodeManager;
        [SerializeField] FeedeeClass[] classChoices;
        // [SerializeField] List<FeedeeInstance> feedees = null;
        Dictionary<LocationList, Dictionary<string, FeedeeInstance>> feedeeLookup = null;
        List<SaveableClone> saveableClones = null;
        FeedeeSpawner feedeeSpawner = null;
        LocationManager locationManager = null;

        private void Awake() 
        {
            feedeeSpawner = GetComponent<FeedeeSpawner>();
            locationManager = GetComponent<LocationManager>();
            if(nodeManager == null) GetComponentInChildren<MenuManager>();
        }

        private FeedeeInstance CreateNewFeedee(FeedeeClass newClass, TownNodeList node = TownNodeList.None)
        {
            BuildLookup();
            FeedeeInstance newFeedee = new FeedeeInstance();
            SaveableEntity saveable = GetComponent<SaveableEntity>();

            newFeedee.feedeeClass = newClass;
            newFeedee.activeNode = node;
            newFeedee.identifier = saveable.GenerateNewUniqueIdentifier("");
            newFeedee.name = newFeedee.identifier;

            return newFeedee;
        }

        private void BuildLookup()
        {
            if(feedeeLookup != null) return;

            feedeeLookup = new Dictionary<LocationList, Dictionary<string, FeedeeInstance>>();
            LocationList currentLocation = locationManager.GetCurrentLocation();

            foreach (LocationList location in nodeManager.GetLocations())
            {
                if (location == currentLocation && !feedeeLookup.ContainsKey(location))
                {
                    AddNewLookupLocation(location);
                }
            }
        }

        private void AddNewLookupLocation(LocationList location)
        {
            var feedeeLocationTable = new Dictionary<string, FeedeeInstance>();
            saveableClones = new List<SaveableClone>();

            foreach (TownNodeList townNode in nodeManager.GetLocationMainNodes(location))
            {
                for (int i = 0; i < nodeManager.GetNodeMenuCount(location, townNode); i++)
                {
                    if (nodeManager.HasNPCSpawn(location, townNode, i))
                    {
                        TownNodeList feedeeNode = nodeManager.GetConnectedNode(location, townNode, i);
                        FeedeeClass newFeedeeClass = ChooseFeedeeClass(feedeeNode);
                        FeedeeInstance newFeedee = CreateNewFeedee(newFeedeeClass, feedeeNode);
                        SaveableClone feedeeSaveable = feedeeSpawner.SpawnNewNPC(newFeedee.feedeeClass, newFeedee.identifier);

                        NPCInfo info = feedeeSaveable.GetComponent<NPCInfo>();
                        info.SetCharacterInfo("name", newFeedee.name);
                        info.SetCharacterInfo("rank", newFeedeeClass.ToString());

                        saveableClones.Add(feedeeSaveable);
                        feedeeLocationTable[newFeedee.identifier] = newFeedee;
                    }
                }
                feedeeLookup[location] = feedeeLocationTable;
            }
        }

        private FeedeeClass ChooseFeedeeClass(TownNodeList nodeList)
        {
            switch (nodeList)
            {
                case TownNodeList.Bakery:
                    return FeedeeClass.Baker;
                case TownNodeList.Bar:
                    return FeedeeClass.Bartender;
                case TownNodeList.Blacksmith:
                case TownNodeList.Furnishings:
                case TownNodeList.Tailor:
                    return FeedeeClass.TradeProfession;
                case TownNodeList.BountyCounter:
                case TownNodeList.QuestCounter:
                    return FeedeeClass.GuildMember;
                case TownNodeList.Cafe:
                case TownNodeList.Restaurant:
                    return FeedeeClass.Waitress;
                case TownNodeList.FoodVendor:
                    return FeedeeClass.Cook;
                default:
                    return FeedeeClass.Default;
            }
        }

        public object CaptureState()
        {
            BuildLookup();

            LocationList currentLocation = locationManager.GetCurrentLocation();
            if(feedeeLookup.ContainsKey(currentLocation))
            {
                foreach (SaveableClone saveable in saveableClones)
                {
                    feedeeLookup[currentLocation][saveable.GetUniqueIdentifier()].state = saveable.CaptureState();
                }
            }

            return feedeeLookup;
        }

        public void RestoreState(object state)
        {
            if(state == null) return;
            feedeeLookup = (Dictionary<LocationList, Dictionary<string, FeedeeInstance>>)state;
        }

        [System.Serializable]
        private class FeedeeInstance
        {
            public string name;
            public FeedeeClass feedeeClass;
            public TownNodeList activeNode;
            public string identifier;
            public object state;
        }
    }
}