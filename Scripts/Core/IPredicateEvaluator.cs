using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ButtonGame.Core
{
    public interface IPredicateEvaluator
    {
        bool? Evaluate(ConditionPredicate predicate, List<string> parameters);
    }
}
