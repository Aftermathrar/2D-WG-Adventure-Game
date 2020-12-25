using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ButtonGame.Saving
{
    public class SavingSystem : MonoBehaviour
    {
        public IEnumerator LoadLastScene(string saveFile)
        {
            Dictionary<string, object> state = LoadFile(saveFile);
            int buildIndex = SceneManager.GetActiveScene().buildIndex;
    
            if(buildIndex > 0)
            {
                if (state.ContainsKey("lastSceneBuildIndex"))
                {
                    buildIndex = (int)state["lastSceneBuildIndex"];
                }
                yield return SceneManager.LoadSceneAsync(buildIndex);
                RestoreState(LoadFile(saveFile));
            }
        }

        public void Save(string saveFile)
        {
            int buildIndex = SceneManager.GetActiveScene().buildIndex;

            if (buildIndex > 0)
            {
                Dictionary<string, object> state = LoadFile(saveFile);
                CaptureState(state);
                SaveFile(saveFile, state);
            }
        }

        public IEnumerator Load(string saveFile)
        {
            int buildIndex = SceneManager.GetActiveScene().buildIndex;
            yield return SceneManager.LoadSceneAsync(buildIndex);
            RestoreState(LoadFile(saveFile));
            yield return null;
        }

        public void Delete(string saveFile)
        {
            string path = GetPathFromSaveFile(saveFile);
            File.Delete(path);
        }

        public bool HasSaveFile(string saveFile)
        {
            string path = GetPathFromSaveFile(saveFile);
            return File.Exists(path);
        }

        private Dictionary<string, object> LoadFile(string saveFile)
        {
            string path = GetPathFromSaveFile(saveFile);
            if(!File.Exists(path))
            {
                return new Dictionary<string, object>();
            }
            using (FileStream stream = File.Open(path, FileMode.Open))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return (Dictionary<string, object>)formatter.Deserialize(stream);
            }
        }

        private void SaveFile(string saveFile, object state)
        {
            string path = GetPathFromSaveFile(saveFile);
            print("Saving to " + path);
            using (FileStream stream = File.Open(path, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, state);
            }
        }

        private void CaptureState(Dictionary<string, object> state)
        {
            foreach (SaveableEntity saveable in FindObjectsOfType<SaveableEntity>())
            {
                state[saveable.GetUniqueIdentifier()] = saveable.CaptureState();
            }

            state["lastSceneBuildIndex"] = SceneManager.GetActiveScene().buildIndex;
        }

        private void RestoreState(Dictionary<string, object> state)
        {
            foreach (SaveableEntity saveable in FindObjectsOfType<SaveableEntity>())
            {
                string id = saveable.GetUniqueIdentifier();
                if (state.ContainsKey(id))
                {
                    saveable.RestoreState(state[id]);
                }
            }
        }

        private string GetPathFromSaveFile(string saveFile)
        {
            return Path.Combine(Application.persistentDataPath, saveFile + ".sav");
        }
    }
}