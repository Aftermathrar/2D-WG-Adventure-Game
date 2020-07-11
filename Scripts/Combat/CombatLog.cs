using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ButtonGame.Combat
{
    public class CombatLog : MonoBehaviour
    {
        // Combat log
        [SerializeField] int maxMessages = 25;
        [SerializeField] GameObject chatPanel;
        [SerializeField] GameObject textObject;
        [SerializeField] List<Message> messageList = new List<Message>();
        Color32 defaultColor = new Color32(221, 136, 50, 255);

        public void SendMessageToChat(string text, Color32 color)
        {
            if (messageList.Count >= maxMessages)
            {
                Destroy(messageList[0].textObject.gameObject);
                messageList.Remove(messageList[0]);
            }

            Message newMessage = new Message();
            newMessage.text = text;

            GameObject newText = Instantiate(textObject, chatPanel.transform);
            newMessage.textObject = newText.GetComponent<Text>();
            newMessage.textObject.text = newMessage.text;
            newMessage.textObject.color = color;

            messageList.Add(newMessage);
        }

        public void DamageTakenCombatLog(float damage, bool isCrit)
        {
            Message damageMessage = new Message();
            Color32 messageColor = defaultColor;
            // Round damage for display
            damage = Mathf.Round(damage);
            if (isCrit)
            {
                damageMessage.text = "Critical hit! You took " + damage + " damage!";
                messageColor = new Color32(219, 126, 90, 255);
            }
            else
            {
                damageMessage.text = "Hit taken for " + damage + " damage!";
            }

            SendMessageToChat(damageMessage.text, messageColor);
        }

        public void BlockSuccessCombatLog(bool isPerfect)
        {
            Message guardMessage = new Message();
            Color32 messageColor = defaultColor;
            if (isPerfect)
            {
                guardMessage.text = "Perfect block! Attack has been reflected!";
                messageColor = new Color32(219, 204, 90, 255);
            }
            else
            {
                guardMessage.text = "Blocked! Damage has been reduced.";
                messageColor = new Color32(204, 166, 60, 255);
            }

            SendMessageToChat(guardMessage.text, messageColor);
        }
    }

    [System.Serializable]
    public class Message
    {
        public string text;
        public Text textObject;
    }
}
