using System.Collections;
using ButtonGame.Saving;
using UnityEngine;

namespace ButtonGame.SceneManagement
{
    public class SavingWrapper : MonoBehaviour
    {
        [SerializeField] KeyCode saveKey = KeyCode.K;
        [SerializeField] KeyCode loadKey = KeyCode.L;
        [SerializeField] KeyCode deleteKey = KeyCode.Delete;
        const string defaultSaveFile = "auto";
        
        private void Awake() 
        {
            StartCoroutine(LoadLastScene());
        }

        private IEnumerator LoadLastScene() {
            yield return GetComponent<SavingSystem>().LoadLastScene(defaultSaveFile);
        }

        private void Update() {
            if (Input.GetKeyDown(saveKey))
            {
                Save();
            }
            if (Input.GetKeyDown(loadKey))
            {
                StartCoroutine(Load());
            }
            if (Input.GetKeyDown(deleteKey))
            {
                Delete();
            }
        }

        public IEnumerator Load()
        {
            yield return GetComponent<SavingSystem>().Load(defaultSaveFile);
        }

        public IEnumerator Load(string saveFile)
        {
            yield return GetComponent<SavingSystem>().Load(saveFile);
        }

        public void Save()
        {
            GetComponent<SavingSystem>().Save(defaultSaveFile);
        }

        public void Save(string saveFile)
        {
            GetComponent<SavingSystem>().Save(saveFile);
        }

        public void Delete()
        {
            GetComponent<SavingSystem>().Delete(defaultSaveFile);
        }

        public void Delete(string saveFile)
        {
            GetComponent<SavingSystem>().Delete(saveFile);
        }
    }
}