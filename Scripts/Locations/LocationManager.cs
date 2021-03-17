using System;
using System.Collections;
using System.Collections.Generic;
using ButtonGame.Attributes;
using ButtonGame.Saving;
using UnityEngine;

namespace ButtonGame.Locations
{
    public class LocationManager : MonoBehaviour, ISaveable
    {
        [SerializeField] LocationDB locationDB = null;
        [SerializeField] LocationList currentLocation;
        [SerializeField] TownNodeList currentNode;
        [SerializeField] PlayerInfo playerInfo;
        [SerializeField] MenuManager menuManager;
        Dictionary<LocationList, Location> locationLookup = null;
        List<LocationList> availableLocations = new List<LocationList>();

        private void OnEnable() 
        {
            BuildLookup();
            if(locationLookup.ContainsKey(currentLocation))
            {
                locationLookup[currentLocation].hasTraveled = true;
            }
        }

        public IEnumerable<LocationList> GetAvailableLocations()
        {
            foreach (LocationList location in availableLocations)
            {
                yield return location;
            }
        }

        public LocationList GetCurrentLocation()
        {
            return currentLocation;
        }

        public string GetLocationDescription(LocationList locName)
        {
            BuildLookup();
            if (locationLookup.ContainsKey(locName))
            {
                return locationLookup[locName].description;
            }

            return "Location not found.";
        }

        public int GetLocationDistance(LocationList locName)
        {
            BuildLookup();
            if (locationLookup.ContainsKey(locName))
            {
                return locationLookup[locName].distance;
            }

            return -1;
        }

        public bool? IsCombatArea(LocationList locName)
        {
            Location location;
            if(locationLookup.TryGetValue(locName, out location))
            {
                return location.isCombatArea;
            }
            return null;
        }

        public bool HasTraveled(LocationList locName)
        {
            if (locationLookup.ContainsKey(locName))
            {
                return locationLookup[locName].hasTraveled;
            }
            return false;
        }

        // Set
        // Comes from SceneChangeObj, which is on buttons that change scene (Save Slots/Travel Buttons)
        public void SetCurrentLocation(string locName)
        {
            if(!Enum.TryParse<LocationList>(locName, out currentLocation))
            {
                Debug.LogError("Location not found.");
                return;
            }
            // SceneChangeObj gets data from PlayerInfo (for Save Slots), so update location in it
            playerInfo.SetPlayerInfo("location", locName);
            currentNode = TownNodeList.Main;
        }

        public void SetAvailable(LocationList locName, bool value)
        {
            if(locationLookup.ContainsKey(locName))
            {
                locationLookup[locName].isAvailable = value;
                BuildAvailableList();
            }
        }

        // Private
        private void BuildLookup()
        {
            if (locationLookup != null) return;

            locationLookup = new Dictionary<LocationList, Location>();
            foreach (Location location in locationDB.GetLocations())
            {
                Location newLocation = new Location();
                newLocation.location = location.location;
                newLocation.name = location.name;
                newLocation.description = location.description;
                newLocation.distance = location.distance;
                newLocation.isAvailable = location.isAvailable;
                newLocation.hasTraveled = location.hasTraveled;

                locationLookup[newLocation.location] = newLocation;
            }

            BuildAvailableList();
        }

        private void BuildAvailableList()
        {
            BuildLookup();

            availableLocations.Clear();
            foreach (LocationList location in locationLookup.Keys)
            {
                if (locationLookup[location].isAvailable && location != currentLocation)
                {
                    availableLocations.Add(location);
                }
            }
        }

        // Save system
        [System.Serializable]
        private class LocationState
        {
            public LocationList currentLocation;
            public Dictionary<LocationList, Location> locationLookup;
        }

        public object CaptureState()
        {
            LocationState state = new LocationState();
            state.currentLocation = currentLocation;
            state.locationLookup = locationLookup;
            return state;
        }

        public void RestoreState(object state)
        {
            LocationState locationState = (LocationState)state;
            locationLookup = locationState.locationLookup;
            currentLocation = locationState.currentLocation;
            currentNode = TownNodeList.Main;

            foreach (Location location in locationDB.GetLocations())
            {
                if(locationLookup.ContainsKey(location.location)) continue;
                
                Location newLocation = new Location();
                newLocation.location = location.location;
                newLocation.name = location.name;
                newLocation.description = location.description;
                newLocation.distance = location.distance;
                newLocation.isAvailable = location.isAvailable;
                newLocation.hasTraveled = location.hasTraveled;

                locationLookup[newLocation.location] = newLocation;
            }

            BuildAvailableList();
            menuManager.MakeMainMenu(currentLocation);
        }
    }
}
