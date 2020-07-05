using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WindowGroupButtons : MonoBehaviour
{
    [SerializeField] Sprite defaultSprite;
    [SerializeField] Sprite selectedSprite;
    [SerializeField] WindowGroup[] groupWindows;

    [System.Serializable]
    private struct WindowGroup
    {
        public string name;
        public GameObject window;
        public Image buttonImage;
    }

    public void ShowWindow(GameObject obj)
    {
        foreach (var window in groupWindows)
        {
            if(window.window == obj)
            {
                obj.SetActive(true);
                window.buttonImage.sprite = selectedSprite;
            }
            else
            {
                window.buttonImage.sprite = defaultSprite;
                window.window.SetActive(false);
            }
        }
    }
}
