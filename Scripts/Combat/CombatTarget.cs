using ButtonGame.Resources;
using UnityEngine;

namespace ButtonGame.Combat
{
    [RequireComponent(typeof(Health))]
    public class CombatTarget : MonoBehaviour, ITargetable
    {
        public void HandleAttack(AtkBtnScript callingScript)
        {
            callingScript.SetTarget(gameObject);
        }
    }
}