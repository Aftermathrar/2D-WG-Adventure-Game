using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ButtonGame.Saving
{
    public class SaveFileCheck : MonoBehaviour
    {
        private void Start() 
        {
            if(!File.Exists(Path.Combine(Application.persistentDataPath, "save.sav")))
            {
                gameObject.SetActive(false);
            }
        }
    }
}
