using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ButtonGame.Core
{
    [System.Serializable]
    public class TextDescriptionCondition
    {
        [SerializeField]
        TextDescriptionPredicate predicate;
        [SerializeField]
        List<DescriptionParameter> descParameters = new List<DescriptionParameter>();

        public TextDescriptionPredicate GetPredicate()
        {
            return predicate;
        }

        public void SetPredicate(TextDescriptionPredicate newPredicate)
        {
            predicate = newPredicate;
        }

        public string GetDescriptionText(float value)
        {
            string text = "";
            foreach (DescriptionParameter description in descParameters)
            {
                if(value > description.limit)
                {
                    text = description.text;
                    continue;
                }
                break;
            }
            return text;
        }

        public int GetDescriptionCount()
        {
            return descParameters.Count;
        }

        public float GetDescriptionLimit(int index)
        {
            return descParameters[index].limit;
        }

        public string GetDescriptionText(int index)
        {
            return descParameters[index].text;
        }

        [System.Serializable]
        class DescriptionParameter
        {
            [SerializeField]
            public float limit;
            [SerializeField]
            public string text;
        }
    }
}
