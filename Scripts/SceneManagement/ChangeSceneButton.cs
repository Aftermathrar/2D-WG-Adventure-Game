using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ButtonGame.SceneManagement
{
    public class ChangeSceneButton : MonoBehaviour
    {
        [SerializeField] int sceneToLoad = -1;

        public void ChangeScene()
        {
            transform.parent = null;
            StartCoroutine(Transition());
        }

        private IEnumerator Transition()
        {
            if(sceneToLoad < 0)
            {
                Debug.LogError("Scene to load not set");
                yield break;
            }

            DontDestroyOnLoad(this.gameObject);
            SavingWrapper savingWrapper = FindObjectOfType<SavingWrapper>();

            savingWrapper.Save();

            yield return SceneManager.LoadSceneAsync(sceneToLoad);
            
            yield return savingWrapper.Load();

            savingWrapper.Save();
            Time.timeScale = 1f;

            Destroy(this.gameObject);
        }
    }
}
