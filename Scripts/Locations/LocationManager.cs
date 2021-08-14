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

        // Status for Combat ChangeSceneButtons
        [SerializeField] LocationList previousLocation = LocationList.None;
        [SerializeField] LocationList travelDestination = LocationList.None;
        int distanceRemaining = 0;

        private void OnEnable()
        {
            BuildLookup();
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

        public string GetLocationName(LocationList locName)
        {
            BuildLookup();
            if(locationLookup.ContainsKey(locName))
            {
                return locationLookup[locName].name;
            }

            return "Location not found.";
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

        public int GetDistanceRemaining()
        {
            return distanceRemaining;
        }

        public LocationList GetTravelDestination()
        {
            return travelDestination;
        }

        public LocationList GetReturnLocation()
        {
            return previousLocation;
        }

        // Set
        // Comes from SceneChangeObj, which is on buttons that change scene (Save Slots/Travel Buttons)
        public void SetCurrentLocation(string locName)
        {
            previousLocation = currentLocation;
            if(!Enum.TryParse<LocationList>(locName, out currentLocation))
            {
                Debug.LogError("Location not found.");
                return;
            }
            // SceneChangeObj gets data from PlayerInfo (for Save Slots), so update location in it
            playerInfo.SetPlayerInfo("location", GetLocationName(currentLocation));
            currentNode = TownNodeList.Main;
        }

        private void CheckIfTraveled()
        {
            if (locationLookup.ContainsKey(currentLocation))
            {
                locationLookup[currentLocation].hasTraveled = true;
            }
        }

        public void SetAvailable(LocationList locName, bool value)
        {
            if(locationLookup.ContainsKey(locName))
            {
                locationLookup[locName].isAvailable = value;
                BuildAvailableList();
            }
        }

        // Method from dialogue trigger
        public void SetAvailable(string[] parameters)
        {
            LocationList location = (LocationList)Enum.Parse(typeof(LocationList), parameters[0]);
            bool newIsAvailable = bool.Parse(parameters[1]);

            SetAvailable(location, newIsAvailable);
        }

        public void DecrementDistanceRemaining()
        {
            distanceRemaining--;
        }

        public void SetDistanceRemaining(int newDistance)
        {
            distanceRemaining = newDistance;
        }

        public void SetDistanceRemaining(LocationList location)
        {
            if(locationLookup.ContainsKey(location))
            {
                distanceRemaining = locationLookup[location].distance;
            }   
        }

        public void SetTravelDestination(LocationList location)
        {
            if(locationLookup.ContainsKey(location))
            {
                previousLocation = currentLocation;
                travelDestination = location;
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
                newLocation.isCombatArea = location.isCombatArea;
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
            public LocationList previousLocation;
            public LocationList travelDestination;
            public int distanceRemaining;
        }

        public object CaptureState()
        {
            LocationState state = new LocationState();
            state.currentLocation = currentLocation;
            state.locationLookup = locationLookup;
            state.previousLocation = previousLocation;
            state.travelDestination = travelDestination;
            state.distanceRemaining = distanceRemaining;
            return state;
        }

        public void RestoreState(object state)
        {
            LocationState locationState = (LocationState)state;
            locationLookup = locationState.locationLookup;
            currentLocation = locationState.currentLocation;
            previousLocation = locationState.previousLocation;
            travelDestination = locationState.travelDestination;
            distanceRemaining = locationState.distanceRemaining;
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
                newLocation.isCombatArea = location.isCombatArea;
                newLocation.hasTraveled = location.hasTraveled;

                locationLookup[newLocation.location] = newLocation;
            }

            BuildAvailableList();
            CheckIfTraveled();
            if(!locationLookup[currentLocation].isCombatArea)
            {
                menuManager.MakeMainMenu(currentLocation);
            }
        }
    }
}
