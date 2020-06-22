using ButtonGame.Attributes;
using UnityEngine;

namespace ButtonGame.Combat
{
    [RequireComponent(typeof(Health))]
    public class CombatTarget : MonoBehaviour, ITargetable
    {
        Health myHealth = null;
        
        private void Awake() 
        {
            myHealth = GetComponent<Health>();
        }
        
        public void HandleAttack(IAtkSkill callingScript)
        {
            callingScript.SetTarget(myHealth);
        }
    }
}