using System.Collections;
using System.Collections.Generic;
using ButtonGame.Saving;
using UnityEngine;

namespace ButtonGame.Stats
{
    public class LocationManager : MonoBehaviour, ISaveable
    {
        [SerializeField] LocationDB locationDB = null;
        [SerializeField] string currentLocation;
        Dictionary<string, Location> locationLookup = null;
        List<string> availableLocations = new List<string>();

        private void OnEnable() 
        {
            BuildLookup();
            if(locationLookup.ContainsKey(currentLocation))
            {
                locationLookup[currentLocation].hasTraveled = true;
            }
        }

        public IEnumerable<string> GetAvailableLocations()
        {
            foreach (string location in availableLocations)
            {
                yield return location;
            }
        }

        public string GetCurrentLocation()
        {
            return currentLocation;
        }

        public string GetLocationDescription(string locName)
        {
            BuildLookup();
            if (locationLookup.ContainsKey(locName))
            {
                return locationLookup[locName].description;
            }

            return "Location not found.";
        }

        public int GetLocationDistance(string locName)
        {
            BuildLookup();
            if (locationLookup.ContainsKey(locName))
            {
                return locationLookup[locName].distance;
            }

            return -1;
        }

        public bool HasTraveled(string locName)
        {
            if (locationLookup.ContainsKey(locName))
            {
                return locationLookup[locName].hasTraveled;
            }
            return false;
        }

        // Set
        public void SetCurrentLocation(string locName)
        {
            currentLocation = locName;
        }

        public void SetAvailable(string locName, bool value)
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

            locationLookup = new Dictionary<string, Location>();
            foreach (Location location in locationDB.GetLocations())
            {
                Location newLocation = new Location();
                newLocation.name = location.name;
                newLocation.description = location.description;
                newLocation.distance = location.distance;
                newLocation.isAvailable = location.isAvailable;
                newLocation.hasTraveled = location.hasTraveled;

                locationLookup[newLocation.name] = newLocation;
            }

            BuildAvailableList();
        }

        private void BuildAvailableList()
        {
            BuildLookup();

            availableLocations.Clear();
            foreach (string location in locationLookup.Keys)
            {
                if (locationLookup[location].isAvailable && location != currentLocation)
                {
                    availableLocations.Add(location);
                }
            }
        }

        // Save system
        public object CaptureState()
        {
            return locationLookup;
        }

        public void RestoreState(object state)
        {
            locationLookup = (Dictionary<string, Location>)state;

            foreach (Location location in locationDB.GetLocations())
            {
                if(locationLookup.ContainsKey(location.name)) continue;
                
                Location newLocation = new Location();
                newLocation.name = location.name;
                newLocation.description = location.description;
                newLocation.distance = location.distance;
                newLocation.isAvailable = location.isAvailable;
                newLocation.hasTraveled = location.hasTraveled;

                locationLookup[newLocation.name] = newLocation;
            }

            BuildAvailableList();
        }
    }
}
