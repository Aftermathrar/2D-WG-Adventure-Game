using System;
using System.Collections.Generic;
using UnityEngine;

namespace ButtonGame.Events
{
    [CreateAssetMenu(fileName = "NewGameEvent", menuName = "Events/GameEventString", order = 2)]
    public class GameEventString : ScriptableObject
    {
        private List<GameEventStringListener> listeners = new List<GameEventStringListener>();

        public void RaiseEvent(string s)
        {
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                listeners[i].OnEventRaised(s);
            }
        }

        public void RegisterListener(GameEventStringListener listener)
        {
            listeners.Add(listener);
        }

        public void UnregisterListener(GameEventStringListener listener)
        {
            listeners.Remove(listener);
        }
    }
}