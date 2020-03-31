using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ButtonGame.UI.GuardText
{
    public class GuardTextSpawner : MonoBehaviour
    {
        [SerializeField] GuardText guardText = null;
        [SerializeField] GuardText perfGuardText = null;

        public void Spawn(bool isPerfectGuard)
        {
            if (guardText == null) return;

            if (isPerfectGuard)
            {
                GuardText instance = Instantiate<GuardText>(perfGuardText, transform);
            }
            else
            {
                GuardText instance = Instantiate<GuardText>(guardText, transform);
            }
        }
    }
}
