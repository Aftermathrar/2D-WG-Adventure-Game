using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ButtonGame.Dialogues
{
    public class AIConversant : MonoBehaviour
    {
        [SerializeField] Dialogue dialogue;
        
        public void StartDialogue()
        {
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerConversant>().StartDialogue(this, dialogue);
        }

        public void StartDialogue(Dialogue newDialogue)
        {
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerConversant>().StartDialogue(this, newDialogue);
        }
    }
}
