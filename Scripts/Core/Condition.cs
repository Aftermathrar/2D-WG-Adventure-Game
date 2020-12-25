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
        List<Disjunction> and = new List<Disjunction>();


        public bool Check(IEnumerable<IPredicateEvaluator> evaluators)
        {
            foreach (Disjunction or in and)
            {
                if(!or.Check(evaluators))
                {
                    return false;
                }
            }
            return true;
        }

        public int GetConditionSize()
        {
            return and.Count;
        }

        public void AddRootCondition()
        {
            Disjunction dis = new Disjunction();
            dis.AddRootCondition();
            and.Add(dis);
        }

        internal void AddNewCondition(int index)
        {
            and[index].AddRootCondition();
        }

        public void RemoveCondition(int indexAnd, int indexOr)
        {
            if(!and[indexAnd].RemoveCondition(indexOr))
            {
                and.RemoveAt(indexAnd);
            }
        }

        public void RemoveAllConditions()
        {
            and.Clear();
        }

        public IEnumerable<ConditionPredicate> GetPredicate(int indexAnd)
        {
            return and[indexAnd].GetPredicate();
        }

        public void SetPredicate(ConditionPredicate newPredicate, int indexAnd, int indexOr)
        {
            and[indexAnd].SetPredicate(newPredicate, indexOr);
        }

        public IEnumerable<string> GetParameters(int indexAnd, int indexOr)
        {
            return and[indexAnd].GetParameters(indexOr);
        }

        public void SetParameters(IEnumerable<string> newParameters, int indexAnd, int indexOr)
        {
            and[indexAnd].SetParameters(newParameters, indexOr);
        }

        public bool GetNegate(int indexAnd, int indexOr)
        {
            return and[indexAnd].GetNegate(indexOr);
        }

        public void SetNegate(bool newNegate, int indexAnd, int indexOr)
        {
            and[indexAnd].SetNegate(newNegate, indexOr);
        }

        public int GetObjectiveIndex(int indexAnd, int indexOr)
        {
            return and[indexAnd].GetObjectiveIndex(indexOr);
        }

        public void SetObjectiveIndex(int newIndex, int indexAnd, int indexOr)
        {
            and[indexAnd].SetObjectiveIndex(newIndex, indexOr);
        }

        public bool GetFoldout(int index)
        {
            return and[index].GetFoldout();
        }

        public void SetFold(bool newFold, int index)
        {
            and[index].SetFoldout(newFold);
        }

        [System.Serializable]
        class Disjunction
        {
            [SerializeField]
            List<Predicate> or = new List<Predicate>();
            bool foldout = true;

            public bool Check(IEnumerable<IPredicateEvaluator> evaluators)
            {
                foreach (Predicate predicate in or)
                {
                    // Debug.Log("Predicate: " + predicate.GetPredicate().ToString() + " " + predicate.GetParameters().ToArray()[0].ToString() + " is " + predicate.Check(evaluators).ToString());
                    if(predicate.Check(evaluators))
                    {
                        return true;
                    }
                }
                return false;
            }

            public IEnumerable<ConditionPredicate> GetPredicate()
            {
                foreach (Predicate predicate in or)
                {
                    yield return predicate.GetPredicate();
                }
            }

            public void SetPredicate(ConditionPredicate newPredicate, int index)
            {
                or[index].SetPredicate(newPredicate);
            }

            public IEnumerable<string> GetParameters(int index)
            {
                return or[index].GetParameters();
            }

            public void SetParameters(IEnumerable<string> newParameters, int index)
            {
                or[index].SetParameters(newParameters);
            }

            public bool GetNegate(int index)
            {
                return or[index].GetNegate();
            }

            public void SetNegate(bool newNegate, int index)
            {
                or[index].SetNegate(newNegate);
            }

            public int GetObjectiveIndex(int index)
            {
                return or[index].GetObjectiveIndex();
            }

            public void SetObjectiveIndex(int newIndex, int indexOr)
            {
                or[indexOr].SetObjectiveIndex(newIndex);
            }

            internal void AddRootCondition()
            {
                Predicate pred = new Predicate();
                pred.BuildBaseParameters();
                or.Add(pred);
            }

            internal bool RemoveCondition(int indexOr)
            {
                or.RemoveAt(indexOr);
                return or.Count > 0;
            }

            public bool GetFoldout()
            {
                return foldout;
            }

            public void SetFoldout(bool newFold)
            {
                foldout = newFold;
            }
        }

        [System.Serializable]
        class Predicate
        {
            [SerializeField]
            ConditionPredicate predicate;
            [SerializeField]
            List<string> parameters = new List<string>();
            [SerializeField]
            bool negate = false;
            int objectiveIndex;

            public ConditionPredicate GetPredicate()
            {
                return predicate;
            }

            public void SetPredicate(ConditionPredicate newPredicate)
            {
                if (predicate != newPredicate)
                {
                    parameters.Clear();
                    predicate = newPredicate;

                    BuildBaseParameters();
                }
            }

            internal void BuildBaseParameters()
            {
                if (predicate == ConditionPredicate.None) return;
                parameters.Add("");
                if (predicate == ConditionPredicate.CompleteObjective)
                {
                    parameters.Add("Objective");
                }
                else if (predicate == ConditionPredicate.HasItem)
                {
                    parameters.Add("1");
                }
            }

            public IEnumerable<string> GetParameters()
            {
                return parameters;
            }

            public void SetParameters(IEnumerable<string> newParameters)
            {
                parameters.Clear();
                if (newParameters.Count() == 0)
                {
                    return;
                }

                foreach (var newParameter in newParameters)
                {
                    // if(!parameters.Contains(newParameter))
                    parameters.Add(newParameter);
                }
            }

            public bool GetNegate()
            {
                return negate;
            }

            public void SetNegate(bool newNegate)
            {
                negate = newNegate;
            }

            public int GetObjectiveIndex()
            {
                return objectiveIndex;
            }

            public void SetObjectiveIndex(int newIndex)
            {
                objectiveIndex = newIndex;
            }

            public bool Check(IEnumerable<IPredicateEvaluator> evaluators)
            {
                if (predicate == ConditionPredicate.None) return true;

                foreach (var evaluator in evaluators)
                {
                    bool? result = evaluator.Evaluate(predicate, parameters);
                    if (result == null)
                    {
                        continue;
                    }

                    if (result == negate) return false;

                }
                return true;
            }
        }
    }
} 