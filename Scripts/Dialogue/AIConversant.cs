using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ButtonGame.Dialogue
{
    public class AIConversant : MonoBehaviour
    {
        [SerializeField] Dialogue dialogue;
        // callingController.
        
        private void Start() 
        {
            StartCoroutine("DialogueTest");
        }
        
        private IEnumerator DialogueTest()
        {
            yield return new WaitForSeconds(2f);
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerConversant>().StartDialogue(this, dialogue);
        }
    }
}
