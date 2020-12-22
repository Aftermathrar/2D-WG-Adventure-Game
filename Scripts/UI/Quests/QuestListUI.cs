using System.Collections;
using System.Collections.Generic;
using ButtonGame.Quests;
using UnityEngine;

namespace ButtonGame.UI.Quests
{
    public class QuestListUI : MonoBehaviour
    {
        [SerializeField] QuestItemUI questPrefab;
        QuestList questList;
        
        // Start is called before the first frame update
        void Start()
        {
            questList = GameObject.FindGameObjectWithTag("Player").GetComponent<QuestList>();
            questList.onQuestUpdated += Redraw;
            Redraw();
        }

        private void Redraw()
        {
            foreach (Transform item in transform)
            {
                Destroy(item.gameObject);
            }

            foreach (QuestStatus status in questList.GetStatuses())
            {
                QuestItemUI questItemUIInstance = Instantiate<QuestItemUI>(questPrefab, transform);
                questItemUIInstance.Setup(status);
            }
        }
    }
}
