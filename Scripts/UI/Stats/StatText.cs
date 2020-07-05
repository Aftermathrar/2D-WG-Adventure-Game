using System.Collections;
using System.Collections.Generic;
using ButtonGame.Stats;
using ButtonGame.Stats.Enums;
using TMPro;
using UnityEngine;

namespace ButtonGame.UI.Stats
{
    public class StatText : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI statName = null;
        [SerializeField] TextMeshProUGUI statValue = null;
        [SerializeField] Stat stat = Stat.AttackPower;
        [SerializeField] bool isPercentage;

        public void SetName(string s)
        {
            statName.text = s;
        }

        public void SetValue(float value)
        {
            string s = null;
            if(isPercentage)
            {
                s = "%";
            }
            statValue.text = string.Format("{0:0}{1}", value, s);
        }

        public Stat GetStat()
        {
            return stat;
        }
    }
}
