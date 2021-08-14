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
        Dictionary<LocationList, Dictionary<string, FeedeeEntry>> feedeeLookup = null;
        Dictionary<TownNodeList, SaveableClone> nodeFeedeeLookup = null;
        List<SaveableClone> saveableClones = null;
        FeedeeSpawner feedeeSpawner = null;
        LocationManager locationManager = null;

        private void Awake() 
        {
            feedeeSpawner = GetComponent<FeedeeSpawner>();
            locationManager = GetComponent<LocationManager>();
            nodeManager ??= GetComponentInChildren<MenuManager>();
        }

        public GameObject GetFeedeeAtNode(TownNodeList nodeQuery)
        {
            if(nodeFeedeeLookup.ContainsKey(nodeQuery))
            {
                return nodeFeedeeLookup[nodeQuery].gameObject;
            }
            return null;
        }

        private FeedeeEntry CreateNewFeedee(FeedeeClass newClass, TownNodeList node = TownNodeList.None)
        {
            BuildLookup();
            FeedeeEntry newFeedee = new FeedeeEntry();
            SaveableEntity saveable = GetComponent<SaveableEntity>();

            newFeedee.feedeeClass = newClass;
            newFeedee.activeNode = node;
            newFeedee.identifier = saveable.GenerateNewUniqueIdentifier("");
            newFeedee.name = newFeedee.identifier;

            return newFeedee;
        }

        private void BuildLookup()
        {
            if(nodeManager == null) return;

            feedeeLookup ??= new Dictionary<LocationList, Dictionary<string, FeedeeEntry>>();
            LocationList currentLocation = locationManager.GetCurrentLocation();

            foreach (LocationList location in nodeManager.GetCityLocations())
            {
                if (location == currentLocation && !feedeeLookup.ContainsKey(location))
                {
                    AddNewLookupLocation(location);
                }
            }
        }

        private void AddNewLookupLocation(LocationList location)
        {
            var feedeeLocationTable = new Dictionary<string, FeedeeEntry>();
            nodeFeedeeLookup = new Dictionary<TownNodeList, SaveableClone>();
            saveableClones = new List<SaveableClone>();

            foreach (TownNodeList townNode in nodeManager.GetLocationMainNodes(location))
            {
                for (int i = 0; i < nodeManager.GetNodeMenuCount(location, townNode); i++)
                {
                    if (nodeManager.HasNPCSpawn(location, townNode, i))
                    {
                        TownNodeList feedeeNode = nodeManager.GetConnectedNode(location, townNode, i);
                        FeedeeClass newFeedeeClass = ChooseFeedeeClass(feedeeNode);
                        FeedeeEntry newFeedee = CreateNewFeedee(newFeedeeClass, feedeeNode);
                        SaveableClone feedeeSaveable = feedeeSpawner.SpawnNewNPC(newFeedee.feedeeClass, newFeedee.identifier);

                        NPCInfo info = feedeeSaveable.GetComponent<NPCInfo>();
                        info.SetCharacterInfo("name", newFeedee.name);
                        info.SetCharacterInfo("rank", newFeedeeClass.ToString());

                        saveableClones.Add(feedeeSaveable);
                        feedeeLocationTable[newFeedee.identifier] = newFeedee;
                        AssignNPCToNode(feedeeSaveable, feedeeNode);
                    }
                }
                feedeeLookup[location] = feedeeLocationTable;
            }
        }

        private void AssignNPCToNode(SaveableClone feedee, TownNodeList node)
        {
            nodeFeedeeLookup[node] = feedee;
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

        private IEnumerator RestoreNPCsToLocation()
        {
            yield return null;
            LocationList currentLocation = locationManager.GetCurrentLocation();
            nodeFeedeeLookup = new Dictionary<TownNodeList, SaveableClone>();
            saveableClones = new List<SaveableClone>();

            if(feedeeLookup.ContainsKey(currentLocation))
            {
                foreach (FeedeeEntry feedee in feedeeLookup[currentLocation].Values)
                {
                    SaveableClone clone = feedeeSpawner.SpawnNewNPC(feedee.feedeeClass, feedee.identifier, feedee.state);
                    saveableClones.Add(clone);
                    AssignNPCToNode(clone, feedee.activeNode);
                }
            }
            else if(nodeManager != null)
            {
                AddNewLookupLocation(currentLocation);
            }
        }

        public object CaptureState()
        {
            BuildLookup();

            foreach (LocationList location in feedeeLookup.Keys)
            {
                foreach (SaveableClone saveable in saveableClones)
                {
                    // Autosaving takes place in two locations, so need to loop through to see which NPCs are loaded
                    if(!feedeeLookup[location].ContainsKey(saveable.GetUniqueIdentifier())) break;

                    feedeeLookup[location][saveable.GetUniqueIdentifier()].state = saveable.CaptureState();
                }
            }

            return feedeeLookup;
        }

        public void RestoreState(object state)
        {
            feedeeLookup = (Dictionary<LocationList, Dictionary<string, FeedeeEntry>>)state;
            
            StartCoroutine(RestoreNPCsToLocation());
        }

        [System.Serializable]
        private class FeedeeEntry
        {
            public string name;
            public FeedeeClass feedeeClass;
            public TownNodeList activeNode;
            public string identifier;
            public object state;
        }
    }
}