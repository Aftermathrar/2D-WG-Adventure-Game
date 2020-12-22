using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ButtonGame.Dialogue;
using TMPro;
using UnityEngine.UI;

namespace ButtonGame.UI
{
    public class DialogueUI : MonoBehaviour
    {
        PlayerConversant playerConversant;
        [SerializeField] TextMeshProUGUI AIText;
        [SerializeField] Button nextButton;
        [SerializeField] Button quitButton;
        [SerializeField] GameObject AIResponse;
        [SerializeField] Transform choiceRoot;
        [SerializeField] GameObject choicePrefab;
        [SerializeField] TextMeshProUGUI conversantName;


        // Start is called before the first frame update
        void Start()
        {
            playerConversant = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerConversant>();
            nextButton.onClick.AddListener(() => playerConversant.Next());
            quitButton.onClick.AddListener(() => playerConversant.Quit());
            playerConversant.onConversationUpdated += UpdateUI;
            UpdateUI();
        }

        void UpdateUI()
        {
            gameObject.SetActive(playerConversant.IsActive());
            if(!playerConversant.IsActive())
            {
                return;
            }
            conversantName.text = playerConversant.GetCurrentConversantName();
            AIResponse.SetActive(!playerConversant.IsChoosing());
            choiceRoot.gameObject.SetActive(playerConversant.IsChoosing());
            if(playerConversant.IsChoosing())
            {
                BuildChoiceList();
            }
            else
            {
                AIText.text = playerConversant.GetText();
                nextButton.gameObject.SetActive(playerConversant.HasNext());
            }
        }

        private void BuildChoiceList()
        {
            foreach (Transform item in choiceRoot)
            {
                Destroy(item.gameObject);
            }
            foreach (DialogueNode choiceNode in playerConversant.GetChoices())
            {
                GameObject choiceInstance = Instantiate(choicePrefab, choiceRoot);
                TextMeshProUGUI textComp = choiceInstance.GetComponentInChildren<TextMeshProUGUI>();
                textComp.text = choiceNode.GetText();

                Button button = choiceInstance.GetComponentInChildren<Button>();
                button.onClick.AddListener(() => 
                {
                    playerConversant.SelectChoice(choiceNode);
                });
            }
        }
    }
}
