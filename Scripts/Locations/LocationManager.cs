using System;
using System.Collections;
using System.Collections.Generic;
using ButtonGame.Saving;
using UnityEngine;

namespace ButtonGame.Locations
{
    public class LocationManager : MonoBehaviour, ISaveable
    {
        [SerializeField] LocationDB locationDB = null;
        [SerializeField] LocationList currentLocation;
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

        public bool HasTraveled(LocationList locName)
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
            if(!Enum.TryParse<LocationList>(locName, out currentLocation))
            {
                Debug.LogError("Location not found.");
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
        public object CaptureState()
        {
            return locationLookup;
        }

        public void RestoreState(object state)
        {
            locationLookup = (Dictionary<LocationList, Location>)state;

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
        }
    }
}
