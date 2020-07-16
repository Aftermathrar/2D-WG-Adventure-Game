using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace ButtonGame.SceneManagement
{
    public class ChangeSceneButton : MonoBehaviour
    {
        [SerializeField] int sceneToLoad = -1;
        [SerializeField] UnityEvent onSceneChange;

        public void ChangeScene()
        {
            transform.parent = null;
            onSceneChange?.Invoke();
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
            LoadFader loadFader = FindObjectOfType<LoadFader>();
            SavingWrapper savingWrapper = FindObjectOfType<SavingWrapper>();

            loadFader.FadeOutImmediate();

            savingWrapper.Save();

            yield return SceneManager.LoadSceneAsync(sceneToLoad);
            
            yield return savingWrapper.Load();

            savingWrapper.Save();
            loadFader.FadeInImmediate();
            Time.timeScale = 1f;

            Destroy(this.gameObject);
        }
    }
}
