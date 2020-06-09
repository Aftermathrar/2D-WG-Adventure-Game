using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using ButtonGame.Combat;

public class GuardController : MonoBehaviour
{
    [SerializeField] List<Image> guardImages = new List<Image>();
    [SerializeField] ParticleSystem guardEffect = null;
    [SerializeField] ParticleSystem justGuardEffect = null;
    [SerializeField] GuardSuccessEvent guardSuccess;
    Fighter fighter = null;
    AtkBtnScript blockingAtkBtn = null;

    [System.Serializable]
    public class GuardSuccessEvent : UnityEvent<bool> { }

    float timeSinceGuardStart = Mathf.Infinity;
    float blockDuration = 0;
    float timeToFade = 0.1f;
    bool isBlocking = false;
    bool fadeCoroutineRunning = false;

    private void Start() 
    {
        fighter = GetComponent<Fighter>();
    }
    
    public void StartBlock(float blockTime, AtkBtnScript atkBtn)
    {
        if (fadeCoroutineRunning)
        {
            StopAllCoroutines();
        }
        foreach (Image gImg in guardImages)
        {
            Color temp = gImg.color;
            temp.a = 0.8235f;
            gImg.color = temp;
        }
        if (!isBlocking)
        {
            guardEffect.Play();
            fighter.activeAttack += BlockTimer;
        }
        timeSinceGuardStart = 0;
        blockDuration = blockTime;
        blockingAtkBtn = atkBtn;
        isBlocking = true;
    }

    public void StopBlock()
    {
        fighter.activeAttack -= BlockTimer;
        guardEffect.Stop();
        StartCoroutine(FadeOutOverTime(timeToFade));
        timeSinceGuardStart = Mathf.Infinity;
        isBlocking = false;
    }

    public void CancelBlock()
    {
        if(isBlocking)
        {
            StopBlock();
        }
    }
    
    public bool IsBlocking(bool shouldReflect = true)
    {
        if(isBlocking && shouldReflect)
        {
            GuardSuccess();
        }
        return isBlocking;
    }

    private void GuardSuccess()
    {
        float perfGuardTime = 0.5f;
        if (timeSinceGuardStart > perfGuardTime)
        {
            guardSuccess.Invoke(false);
        }
        else
        {
            blockingAtkBtn.CalculateReflectDamage();
            justGuardEffect.Play();
            guardSuccess.Invoke(true);
        }
    }

    public void BlockTimer()
    {
        timeSinceGuardStart += Time.deltaTime;

        if(timeSinceGuardStart > blockDuration)
        {
            StopBlock();
        }
    }

    IEnumerator FadeOutOverTime(float fadeTime)
    {
        fadeCoroutineRunning = true;
        for (float tCurrent = 0; tCurrent < fadeTime; tCurrent += Time.deltaTime)
        {
            float normalizedTime = .8235f - (tCurrent / fadeTime)*.8235f;
            for (int i = 0; i < guardImages.Count; i++)
            {
                Color temp = guardImages[i].color;
                temp.a = normalizedTime;
                guardImages[i].color = temp;
                yield return null;
            }
        }
        foreach (Image gImg in guardImages)
        {
            Color temp = gImg.color;
            temp.a = 0;
            gImg.color = temp;
        }
        fadeCoroutineRunning = false;
    }
}
