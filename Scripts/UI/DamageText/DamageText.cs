using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ButtonGame.UI.DamageText
{
    public class DamageText : MonoBehaviour
    {
        [SerializeField] Text damageText = null;

        public void SetValue(float amount)
        {
            damageText.text = string.Format("{0:0}", amount);
            float newX = Random.Range(-100, 100);
            float newY = Random.Range(-50, 30);
            Vector3 spawnPos = new Vector3(newX, newY, transform.position.z);
            damageText.rectTransform.localPosition = spawnPos;
        }

        public void DestroyText()
        {
            Destroy(gameObject);
        }
    }
}

