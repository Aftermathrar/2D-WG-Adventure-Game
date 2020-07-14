using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ButtonGame.UI
{
    public class FollowerSkillTooltipEnabler : MonoBehaviour
    {
        private void OnDisable() 
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(true);
            }
        }
    }
}
