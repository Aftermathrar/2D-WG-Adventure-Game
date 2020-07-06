using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ButtonGame.Events
{
    public class GameEventListener : MonoBehaviour
    {
        public GameEvent gameEvent;
        public UnityEvent response;
        public StringResponse stringResponse;
        [System.Serializable]
        public class StringResponse : UnityEvent<string> { }
        public UnityEvent<float, bool> floatBoolResponse;

        private void OnEnable()
        {
            gameEvent.RegisterListener(this);
        }

        private void OnDisable()
        {
            gameEvent.UnregisterListener(this);
        }

        public void OnEventRaised()
        {
            response.Invoke();
        }

        public void OnEventRaised(string s)
        {
            stringResponse.Invoke(s);
        }

        public void OnEventRaised(float i, bool isTrue)
        {
            floatBoolResponse.Invoke(i, isTrue);
        }
    }
}
