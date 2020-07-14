using System.Collections;
using System.Collections.Generic;
using ButtonGame.SceneManagement;
using UnityEngine;

namespace ButtonGame.Saving
{
    public class Saver : MonoBehaviour
    {
        public void SaveGame()
        {
            SavingWrapper savingWrapper = (SavingWrapper)GameObject.FindObjectOfType(typeof(SavingWrapper));
            if(savingWrapper != null)
            {
                savingWrapper.Save();
            }
        }

        public void DeleteSave()
        {
            SavingWrapper savingWrapper = (SavingWrapper)GameObject.FindObjectOfType(typeof(SavingWrapper));
            if (savingWrapper != null)
            {
                savingWrapper.Delete();
            }
        }

        public void ExitGame()
        {
            Application.Quit();
        }
    }
}
