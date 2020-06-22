using System;
using System.Collections.Generic;
using UnityEngine;

namespace ButtonGame.Events
{
    [CreateAssetMenu(fileName = "NewGameEvent", menuName = "Events/GameEvent", order = 0)]
    public class GameEvent : ScriptableObject
    {
        private List<GameEventListener> listeners = new List<GameEventListener>();

        public void RaiseEvent()
        {
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                listeners[i].OnEventRaised();
            }
        }

        public void RegisterListener(GameEventListener listener)
        {
            listeners.Add(listener);
        }

        public void UnregisterListener(GameEventListener listener)
        {
            listeners.Remove(listener);
        }
    }
}