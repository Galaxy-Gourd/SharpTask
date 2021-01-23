using System;
using System.Collections.Generic;

namespace GGSharpTask
{
    /// <summary>
    /// Evaluates task conditions based on task eval type
    /// </summary>
    public static class TaskConditionEvaluator
    {
        #region Evaluation

        public static bool EvaluateTaskConditions(
            ConditionsEvalOptions mode,
            List<Func<bool>> conditions)
        {
            bool evalPassed;
            switch (mode)
            {
                case ConditionsEvalOptions.All:
                    evalPassed = EvaluateTaskConditionsAll(conditions);
                    break;
                case ConditionsEvalOptions.One:
                    evalPassed = EvaluateTaskConditionsOne(conditions);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return evalPassed;
        }

        private static bool EvaluateTaskConditionsAll(List<Func<bool>> conditions)
        {
            bool evalPassed = true;
            foreach (var condition in conditions)
            {
                if (!condition())
                {
                    evalPassed = false;
                    break;
                }
            }
            return evalPassed;
        }

        private static bool EvaluateTaskConditionsOne(List<Func<bool>> conditions)
        {
            bool evalPassed = false;
            foreach (var condition in conditions)
            {
                if (condition())
                {
                    evalPassed = true;
                    break;
                }
            }
            return evalPassed;
        }

        #endregion Evaluation
    }
}