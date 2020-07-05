using UnityEngine;

namespace ButtonGame.Stats.Follower
{
    public class AppearanceStats : MonoBehaviour
    {
        [SerializeField] string Name;
        [SerializeField] float Height;
        [SerializeField] float Weight;
        [SerializeField] EyeColors eyeColor;
        [SerializeField] HairColors hairColor;
        [SerializeField] BodyTypes bodyType;
    }
}