using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ButtonGame.Stats
{
    [CreateAssetMenu(fileName = "LocationDB", menuName = "Stats/LocationDB")]
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
        public string name;
        public string description;
        public int distance;
        public bool isAvailable;
        public bool hasTraveled;
    }
}
