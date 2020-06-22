using System;
using System.Collections.Generic;
using UnityEngine;

namespace ButtonGame.Events
{
    [CreateAssetMenu(fileName = "NewGameEvent", menuName = "Events/GameEventFloatBool", order = 1)]
    public class GameEventFloatBool : ScriptableObject
    {
        private List<GameEventFloatBoolListener> listeners = new List<GameEventFloatBoolListener>();

        public void RaiseEvent(float f, bool isTrue)
        {
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                listeners[i].OnEventRaised(f, isTrue);
            }
        }

        public void RegisterListener(GameEventFloatBoolListener listener)
        {
            listeners.Add(listener);
        }

        public void UnregisterListener(GameEventFloatBoolListener listener)
        {
            listeners.Remove(listener);
        }
    }
}