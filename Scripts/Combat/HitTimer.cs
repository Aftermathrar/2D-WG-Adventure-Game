using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ButtonGame.Core;
using ButtonGame.UI;

namespace ButtonGame.Combat
{
    public class HitTimer : MonoBehaviour, IAction
    {
        private Fighter fighter;
        private float maxFillTime;
        private float currentFillTime;
        private float t;
        private float tVert;
        private float timeToMoveHoriz;
        private float timeToMoveVert = 0.2f;
        private float xOffset;
        private float yOffset;
        private Vector3 startPosition;
        private Vector3 targetPosition;

        private bool isAttackComplete = false;

        [SerializeField] Image fillOverlay;
        [SerializeField] Image[] myImage;
        [SerializeField] CanvasGroup canvas;

        // Allows return to pool
        public HitTimerSpawner Spawner;

        private void Start() 
        {
            if (transform.parent.parent.CompareTag("Player"))
            {
                xOffset = 50f;
                yOffset = 10f;
            }
            else
            {
                xOffset = -50f;
                yOffset = -10f;
            }
            if (fighter == null)
            {
                GetFighterComponent();
            }
        }

        private void OnEnable() 
        {
            isAttackComplete = false;
            fillOverlay.fillAmount = 0;
            canvas.alpha = 1f;
            if(myImage.Length >= 3)
            {
                myImage[2].fillAmount = 0;
                myImage[3].fillAmount = 1f;
            }
        }

        private void GetFighterComponent()
        {
            fighter = GetComponentInParent<Fighter>();
        }

        public void SetFighter(Fighter _fighter)
        {
            fighter = _fighter;
        }

        public void SetPosition(Vector3 spawnPos)
        {
            transform.localPosition = spawnPos;
        }

        public void SetSprite(Sprite sprite)
        {
            for(int i = 0; i < 2; i++)
            {
                myImage[i].sprite = sprite;
            }
        }

        public void SetFillTime(float windupTime)
        {
            if (fighter == null)
            {
                GetFighterComponent();
            }

            maxFillTime = windupTime;
            currentFillTime = 0;

            fighter.activeAttack += UpdateBoxFill;
        }

        public void SetMoveTime(float timeToHit)
        {
            if (fighter == null)
            {
                GetFighterComponent();
            }

            fighter.activeAttack += MoveHorizontally;
            t = 0;
            timeToMoveHoriz = timeToHit;
            startPosition = transform.localPosition;
            targetPosition = new Vector3(transform.localPosition.x + xOffset, transform.localPosition.y, 0);
        }

        private void UpdateBoxFill()
        {
            currentFillTime += Time.deltaTime;
            float fillPercent = 1;
            if (currentFillTime >= maxFillTime)
            {
                fighter.activeAttack -= UpdateBoxFill;
                fighter.activeAttack += MoveVertically;

                tVert = 0f;
                startPosition = transform.localPosition;
                targetPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + yOffset, 0);

                isAttackComplete = true;
            }
            else
            {
                fillPercent = 1 - Mathf.Clamp01((maxFillTime - currentFillTime) / maxFillTime);
            }
            fillOverlay.fillAmount = fillPercent;
        }

        private void MoveVertically()
        {
            if (tVert == 0) StartCoroutine(FadeOutOverTime(timeToMoveVert));
            tVert += Time.deltaTime;
            if(tVert > timeToMoveVert)
            {
                tVert = timeToMoveVert;
            }
            transform.localPosition = Vector3.Lerp(startPosition, targetPosition, tVert/timeToMoveVert);
        }

        public IEnumerator EnemyBorderFill(float leadTime, Color32 color)
        {
            float t = 0;
            myImage[2].color = color;
            do
            {
                float tPercent = Mathf.Clamp01(t/leadTime);
                myImage[2].fillAmount = tPercent;
                myImage[3].fillAmount = 1-tPercent;
                t += Time.deltaTime;
                yield return null;
            } while (t < leadTime && !isAttackComplete);

            myImage[2].fillAmount = 1f;
            myImage[3].fillAmount = 0;
        }

        IEnumerator FadeOutOverTime(float timeToFade)
        {
            isAttackComplete = true;
            for (float tCurrent = 0; tCurrent < timeToFade; tCurrent+=Time.deltaTime)
            {
                float normalizedTime = 1 - tCurrent/timeToFade;
                canvas.alpha = normalizedTime;
                yield return null;
            }

            fighter.activeAttack -= MoveVertically;
            Spawner.ReturnToStack(this);
        }

        private void MoveHorizontally()
        {
            t += Time.deltaTime;
            if(t > timeToMoveHoriz)
            {
                t = timeToMoveHoriz;
                fighter.activeAttack -= MoveHorizontally;
            }
            transform.localPosition = Vector3.Lerp(startPosition, targetPosition, t/timeToMoveHoriz);
        }

        public void Cancel()
        {
            if(!isAttackComplete)
            {
                if (fighter == null)
                {
                    GetFighterComponent();
                }
                fighter.activeAttack -= UpdateBoxFill;
                fighter.activeAttack -= MoveHorizontally;

                tVert = 0;
                startPosition = transform.localPosition;
                targetPosition = new Vector3(transform.localPosition.x, transform.localPosition.y - yOffset*3, 0);

                fighter.activeAttack += MoveVertically;
            }
        }
    }
}