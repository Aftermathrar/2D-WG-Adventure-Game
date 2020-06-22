using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ButtonGame.Events
{
    public class GameEventStringListener : MonoBehaviour
    {
        public GameEventString gameEvent;
        [System.Serializable]
        public class Response : UnityEvent<string> { }
        [SerializeField] Response response;

        private void OnEnable()
        {
            gameEvent.RegisterListener(this);
        }

        private void OnDisable()
        {
            gameEvent.UnregisterListener(this);
        }

        public void OnEventRaised(string s)
        {
            response.Invoke(s);
        }
    }
}