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
        Dictionary<LocationList, Dictionary<SaveableClone, FeedeeInstance>> feedeeLookup = null;
        FeedeeSpawner feedeeSpawner = null;

        private void Awake() 
        {
            feedeeSpawner = GetComponent<FeedeeSpawner>();
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

            feedeeLookup = new Dictionary<LocationList, Dictionary<SaveableClone, FeedeeInstance>>();
            foreach (LocationList location in nodeManager.GetLocations())
            {
                // feedeeLookup[location] = new Dictionary<SaveableClone, FeedeeInstance>();
                var feedeeLocationTable = new Dictionary<SaveableClone, FeedeeInstance>();
                foreach (TownNodeList townNode in nodeManager.GetLocationMainNodes(location))
                {
                    for (int i = 0; i < nodeManager.GetNodeMenuCount(location, townNode); i++)
                    {
                        if(nodeManager.HasNPCSpawn(location, townNode, i))
                        {
                            TownNodeList feedeeNode = nodeManager.GetConnectedNode(location, townNode, i);
                            FeedeeClass newFeedeeClass = ChooseFeedeeClass(feedeeNode);
                            FeedeeInstance newFeedee = CreateNewFeedee(newFeedeeClass, feedeeNode);
                            SaveableClone feedeeSaveable = feedeeSpawner.SpawnNewNPC(newFeedee.feedeeClass, newFeedee.identifier);

                            NPCInfo info = feedeeSaveable.GetComponent<NPCInfo>();
                            info.SetCharacterInfo("name", newFeedee.name);
                            info.SetCharacterInfo("rank", newFeedeeClass.ToString());

                            feedeeLocationTable[feedeeSaveable] = newFeedee;
                        }
                    }
                    feedeeLookup[location] = feedeeLocationTable;
                }
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
            // return feedeeLookup;
            return null;
        }

        public void RestoreState(object state)
        {
            if(state == null) return;
            feedeeLookup = (Dictionary<LocationList, Dictionary<SaveableClone, FeedeeInstance>>)state;
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