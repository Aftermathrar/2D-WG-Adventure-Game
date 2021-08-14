using System.Collections;
using System.Collections.Generic;
using ButtonGame.Dialogues;
using UnityEngine;
using TMPro;
using ButtonGame.Core;

namespace ButtonGame.UI.Menus
{
    public class CharacterInfoDescription : MonoBehaviour
    {
        [SerializeField] bool isPlayer;
        [SerializeField] TextMeshProUGUI descriptionText = null;

        public void SetDefaultInfo()
        {
            GameObject character = null;
            string sceneToLoad;
            if (isPlayer)
            {
                character = GameObject.FindGameObjectWithTag("Player");
                sceneToLoad = "PlayerDescriptionTest";
            }
            else
            {
                character = GameObject.FindGameObjectWithTag("Follower");
                sceneToLoad = "DescriptionTest";
            }
            if (!character) return;

            PopulateDescriptionText(sceneToLoad, character);
        }

        public void SetCharacterInfo(GameObject character)
        {
            if(!character) return;

            string sceneToLoad = "DescriptionTest";
            PopulateDescriptionText(sceneToLoad, character);
        }

        public void OnFollowerChange()
        {
            FollowerManager followerManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<FollowerManager>();
            GameObject followerGO;
            if(followerManager.GetActiveFollowerObject(out followerGO))
            {
                string sceneToLoad = "DescriptionTest";
                PopulateDescriptionText(sceneToLoad, followerGO);
            }
        }

        private void PopulateDescriptionText(string sceneToLoad, GameObject character)
        {
            SceneText sceneText = Resources.Load(sceneToLoad) as SceneText;
            string s = sceneText.GetText(character);
            descriptionText.text = s;
        }
    }
}
