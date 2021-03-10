using System.Collections;
using System.Collections.Generic;
using ButtonGame.Locations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ButtonGame.UI.Menus
{
    public class LocationMenuSlotUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI titleText;
        [SerializeField] TextMeshProUGUI subtitleText;
        [SerializeField] Image image;
        Button button;
        MenuManager manager;
        int slot;

        private void Awake() 
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(() => LoadConnectedNode());
            manager = GetComponentInParent<MenuManager>();
        }

        public void SlotSetup(string title, string subtitle, Sprite sprite, Color color, int index)
        {
            titleText.text = title;
            subtitleText.text = subtitle;
            image.sprite = sprite;
            image.color = color;
            slot = index;
        }

        private void LoadConnectedNode()
        {
            manager.ChangeNodeOnClick(slot);
        }
    }
}
