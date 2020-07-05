using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ButtonGame.Stats.Follower
{
    public class BaseFollowerAttackStats : MonoBehaviour
    {
        [SerializeField] FollowerAttackDB followerAttackDB;

        // public float GetAttackStat(string propertyName, FollowerAttackName attackName)
        // {
        //     FollowerAttackStats stats = followerAttackDB.GetAttackStat(attackName);

        //     return 0;
        // }
    }
}
