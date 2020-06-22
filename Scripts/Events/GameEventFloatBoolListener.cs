using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ButtonGame.Events
{
    public class GameEventFloatBoolListener : MonoBehaviour
    {
        public GameEventFloatBool gameEvent;
        public UnityEvent<float, bool> response;

        private void OnEnable()
        {
            gameEvent.RegisterListener(this);
        }

        private void OnDisable()
        {
            gameEvent.UnregisterListener(this);
        }

        public void OnEventRaised(float i, bool isTrue)
        {
            response.Invoke(i, isTrue);
        }
    }
}
