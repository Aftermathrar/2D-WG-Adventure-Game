using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ButtonGame.UI.EffectIcon
{
    public class EffectIconSpawner : MonoBehaviour
    {
        [SerializeField] EffectIconImage fxIcon = null;
        Dictionary<string, EffectIconImage> iconInstances = new Dictionary<string, EffectIconImage>();

        public void Spawn(string id, int count, Sprite sprite, Color32 color)
        {
            float spawnPosX = transform.position.x + (50f * count);
            Vector3 spawnPos = new Vector3(spawnPosX, transform.position.y, transform.position.z);
            EffectIconImage instance = Instantiate<EffectIconImage>(fxIcon, spawnPos, Quaternion.identity, transform);
            instance.SetIcon(sprite);
            instance.SetColor(color);
            iconInstances[id] = instance;
        }

        public void Destroy(string id)
        {
            if(iconInstances.ContainsKey(id))
            {
                EffectIconImage instance = iconInstances[id];
                if(transform.childCount > 1)
                {
                    int fxIndex = instance.transform.GetSiblingIndex()+1;
                    for (int i = fxIndex; i < transform.childCount; i++)
                    {
                        EffectIconImage icon = transform.GetChild(i).GetComponent<EffectIconImage>();
                        icon.StartCoroutine(icon.MoveHorizontal());
                    }
                }
                Destroy(instance.gameObject);
            }
        }

        public void UpdateIconFill(string id, float fillPercent)
        {
            if (iconInstances.ContainsKey(id))
            {
                EffectIconImage instance = iconInstances[id];
                instance.UpdateIconFill(fillPercent);
            }
        }

        public void UpdateStacks(string id, float stacks)
        {
            if(iconInstances.ContainsKey(id))
            {
                EffectIconImage instance = iconInstances[id];
                instance.UpdateStacks(stacks.ToString());
            }
        }
    }
}
