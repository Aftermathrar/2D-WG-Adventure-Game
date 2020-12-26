using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ButtonGame.Control
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        Camera mainCamera = null;

        private void Awake() 
        {
            mainCamera = GetComponent<Camera>();
        }
        
        void Start()
        {
            
            Debug.Log("Screen width: " + Screen.width + " Screen Height: " + Screen.height);
            Vector3 cameraOffset = new Vector3(Screen.width / 2, Screen.height / 2, -10);
            transform.position = cameraOffset;
            mainCamera.orthographicSize = Screen.height / 2;
        }
    }
}
