using System.Collections;
using System.Collections.Generic;
using ButtonGame.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace ButtonGame.SceneManagement
{
    public class ChangeSceneButton : MonoBehaviour
    {
        [SerializeField] int sceneToLoad = -1;
        [SerializeField] string destination;
        [SerializeField] UnityEvent onSceneChange;
        [SerializeField] GameEvent locationChangeEvent;

        public void ChangeScene()
        {
            transform.SetParent(null);
            onSceneChange?.Invoke();
            StartCoroutine(Transition());
        }

        public void ChangeScene(string saveFile)
        {
            transform.SetParent(null);
            onSceneChange?.Invoke();
            StartCoroutine(Transition(saveFile));
        }

        public void SetSceneToLoad(int newScene)
        {
            sceneToLoad = newScene;
        }

        public void SetDestination(string newDestination)
        {
            destination = newDestination;
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
            locationChangeEvent.RaiseEvent(destination);
            loadFader.FadeInImmediate();
            Time.timeScale = 1f;

            Destroy(this.gameObject);
        }

        private IEnumerator Transition(string saveFile)
        {
            if (sceneToLoad < 0)
            {
                Debug.LogError("Scene to load not set");
                yield break;
            }

            DontDestroyOnLoad(this.gameObject);
            LoadFader loadFader = FindObjectOfType<LoadFader>();
            SavingWrapper savingWrapper = FindObjectOfType<SavingWrapper>();

            loadFader.FadeOutImmediate();

            // savingWrapper.Save(saveFile);

            yield return SceneManager.LoadSceneAsync(sceneToLoad);

            yield return savingWrapper.Load(saveFile);

            // savingWrapper.Save(saveFile);
            locationChangeEvent.RaiseEvent(destination);
            loadFader.FadeInImmediate();
            Time.timeScale = 1f;

            Destroy(this.gameObject);
        }
    }
}
