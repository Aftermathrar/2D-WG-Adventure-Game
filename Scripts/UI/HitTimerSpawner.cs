using System.Collections;
using System.Collections.Generic;
using ButtonGame.Combat;
using UnityEngine;

namespace ButtonGame.UI
{
    public class HitTimerSpawner : MonoBehaviour
    {
        [SerializeField] HitTimer hitTimer;
        Stack<HitTimer> hitTimerStack = new Stack<HitTimer>();

        public HitTimer SpawnHitTimer(float xOffset)
        {
            Vector3 spawnPos = new Vector3(xOffset, 0, 0);
            HitTimer hitTimerInstance = null;
            if(hitTimerStack.Count == 0)
            {
                hitTimerInstance = Instantiate(hitTimer, transform);
                hitTimerInstance.SetPosition(spawnPos);
                hitTimerInstance.Spawner = this;
            }
            else
            {
                hitTimerInstance = hitTimerStack.Pop();
                hitTimerInstance.SetPosition(spawnPos);
                hitTimerInstance.gameObject.SetActive(true);
            }
            return hitTimerInstance;
        }

        public void ReturnToStack(HitTimer instance)
        {
            instance.gameObject.SetActive(false);
            hitTimerStack.Push(instance);
        }
    }
}