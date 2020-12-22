using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ButtonGame.Core
{
    [System.Serializable]
    public class Condition
    {
        [SerializeField]
        ConditionPredicate predicate;
        [SerializeField]
        List<string> parameters = new List<string>();
        [SerializeField]
        bool negate = false;

        public ConditionPredicate GetPredicate()
        {
            return predicate;
        }

        public void SetPredicate(ConditionPredicate newPredicate)
        {
            if(predicate != newPredicate)
            {
                parameters.Clear();
                predicate = newPredicate;

                BuildBaseParameters();
            }
        }

        private void BuildBaseParameters()
        {
            if(predicate == ConditionPredicate.None) return;
            parameters.Add("");
            if(predicate == ConditionPredicate.CompleteObjective)
            {
                parameters.Add("Objective");
            }
            else if(predicate == ConditionPredicate.HasItem)
            {
                parameters.Add("1");
            }
        }

        public bool GetNegate()
        {
            return negate;
        }

        public bool Check(IEnumerable<IPredicateEvaluator> evaluators)
        {
            if(predicate == ConditionPredicate.None) return true;

            foreach (var evaluator in evaluators)
            {
                bool? result = evaluator.Evaluate(predicate, parameters);
                if(result == null)
                {
                    continue;
                }

                if(result == negate) return false;

            }
            return true;
        }

        public IEnumerable<string> GetParameters()
        {
            return parameters;
        }

        public void SetNegate(bool newNegate)
        {
            negate = newNegate;
        }

        public void SetParameters(IEnumerable<string> newParameters)
        {
            parameters.Clear();
            if(newParameters.Count() == 0)
            {
                return;
            }

            foreach (var newParameter in newParameters)
            {
                // if(!parameters.Contains(newParameter))
                parameters.Add(newParameter);
            }
        }
    }
} 