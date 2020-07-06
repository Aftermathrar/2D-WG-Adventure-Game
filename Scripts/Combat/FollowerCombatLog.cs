using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ButtonGame.Combat
{
    public class FollowerCombatLog : MonoBehaviour
    {
        // Combat log
        [SerializeField] int maxMessages = 25;
        [SerializeField] GameObject chatPanel;
        [SerializeField] GameObject textObject;
        [SerializeField] List<Message> messageList = new List<Message>();
        Color32 defaultColor = new Color32(232, 100, 248, 255);

        public void SendMessageToChat(string text)
        {
            if (messageList.Count >= maxMessages)
            {
                Destroy(messageList[0].textObject.gameObject);
                messageList.Remove(messageList[0]);
            }

            Message newMessage = new Message();
            newMessage.text = text;

            GameObject newText = Instantiate(textObject, chatPanel.transform);
            newText.transform.SetAsFirstSibling();
            newMessage.textObject = newText.GetComponent<Text>();
            newMessage.textObject.text = newMessage.text;
            newMessage.textObject.color = defaultColor;

            messageList.Add(newMessage);
        }
    }
}
