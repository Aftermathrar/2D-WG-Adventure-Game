using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ButtonGame.UI.Menus
{

    public class ButtonSelectConfirm : MonoBehaviour
    {
        [SerializeField] Button button;
        [SerializeField] bool selectOnEnable = false;

        private void OnEnable() 
        {
            button.Select();
        }

        private void Update() 
        {
            if(Input.GetKeyDown(KeyCode.Return))
            {
                if(EventSystem.current.currentSelectedGameObject.GetInstanceID() == 
                    gameObject.GetInstanceID())
                {
                    button.onClick.Invoke();
                }
            }
        }
    }
}
