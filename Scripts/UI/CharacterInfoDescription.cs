using System.Collections;
using System.Collections.Generic;
using ButtonGame.Dialogue;
using UnityEngine;
using TMPro;

namespace ButtonGame.UI
{
    public class CharacterInfoDescription : MonoBehaviour
    {
        [SerializeField] bool isPlayer;
        [SerializeField] TextMeshProUGUI descriptionText = null;

        private void OnEnable()
        {
            GameObject character = null;
            string sceneToLoad;
            if(isPlayer)
            {
                character = GameObject.FindGameObjectWithTag("Player");
                sceneToLoad = "PlayerDescriptionTest";
            }
            else
            {
                character = GameObject.FindGameObjectWithTag("Follower");
                sceneToLoad = "DescriptionTest";
            }
            if (character == null) return;

            SceneText sceneText = Resources.Load(sceneToLoad) as SceneText;
            string s = sceneText.GetText(character);
            descriptionText.text = s;
        }
    }
}
