using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ButtonGame.UI.DamageText
{
    public class DamageTextSpawner : MonoBehaviour
    {
        [SerializeField] DamageText damageText = null;
        [SerializeField] DamageText critDamageText = null;

        public void Spawn(float damageAmount, bool isCrit = false)
        {
            if(damageText == null) return;

            if(isCrit)
            {
                DamageText instance = Instantiate<DamageText>(critDamageText, transform);
                instance.SetValue(damageAmount);
            }
            else
            {
                DamageText instance = Instantiate<DamageText>(damageText, transform);
                instance.SetValue(damageAmount);
            }
        }
    }
}
