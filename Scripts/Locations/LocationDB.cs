using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ButtonGame.Locations
{
    [CreateAssetMenu(fileName = "LocationDB", menuName = "Locations/LocationDB")]
    public class LocationDB : ScriptableObject
    {
        [SerializeField] Location[] locations = null;

        Dictionary<string, Location> locationLookup = null;

        public IEnumerable<Location> GetLocations()
        {
            BuildLookup();
            foreach (Location location in locations)
            {
                yield return location;
            }
        }

        public IEnumerable<string> GetLocationNames()
        {
            BuildLookup();
            foreach (var key in locationLookup.Keys)
            {
                yield return key;
            }
        }

        private void BuildLookup()
        {
            if(locationLookup != null) return;

            locationLookup = new Dictionary<string, Location>();
            foreach (var location in locations)
            {
                locationLookup[location.name] = location;
            }
        }
    }

    [System.Serializable]
    public class Location
    {
        public LocationList location;
        public string name;
        public string description;
        public int distance;
        public bool isAvailable;
        public bool isCombatArea;
        public bool hasTraveled;
    }
}
