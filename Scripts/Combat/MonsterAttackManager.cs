using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ButtonGame.Stats;
using ButtonGame.Stats.Enums;

namespace ButtonGame.Combat
{
    public class MonsterAttackManager : MonoBehaviour
    {
        [SerializeField] List<MonAtkName> atkMoveset;
        [SerializeField] List<float> hpTriggers;
        [SerializeField] MonAtkName atkTrigger;
        [SerializeField] List<float> timeTriggers;

        public Queue<MonAtkName> GetAttackPattern()
        {
            var atkPattern = new Queue<MonAtkName>();
            var sourceAtk = new List<MonAtkName>();
            foreach (MonAtkName atk in atkMoveset)
            {
                sourceAtk.Add(atk);
            }

            do
            {
                int randIndex = Random.Range(0, sourceAtk.Count);
                atkPattern.Enqueue(sourceAtk[randIndex]);
                sourceAtk.RemoveAt(randIndex);
            } while (sourceAtk.Count > 0);

            return atkPattern;
        }

        public MonAtkName CheckHPTrigger(float hp)
        {
            if(hpTriggers.Count > 0)
            {
                if (hp <= hpTriggers[0])
                {
                    hpTriggers.RemoveAt(0);
                    return atkTrigger;
                }
            }
            return MonAtkName.None;
        }
    }
}
