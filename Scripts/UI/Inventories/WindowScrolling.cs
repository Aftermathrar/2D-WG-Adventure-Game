using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ButtonGame.UI.Inventories
{
    public class WindowScrolling : MonoBehaviour
    {
        [SerializeField] Transform content = null;
        [Range(5, 50)]
        [SerializeField] float scrollDistance = 30f;

        void Update()
        {
            Vector3 pos = content.position;
            pos.y -= Input.mouseScrollDelta.y * scrollDistance;
            content.position = pos;
        }
    }
}
