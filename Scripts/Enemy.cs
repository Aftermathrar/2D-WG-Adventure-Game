using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public float health = 100;
    public Text myText;

    private void Start() 
    {
        myText.text = string.Format("{0}", health);
    }

    public void HealthUpdate()
    {
        myText.text = string.Format("{0}", health);
    }

}
