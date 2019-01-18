using System;
using Mongrow.Internals;
// ReSharper disable RedundantAttributeUsageProperty

namespace Mongrow.Steps
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class StepAttribute : Attribute
    {
        public int Number { get; }
        public string BranchSpec { get; }

        public StepAttribute(int number, string branchSpec = "master")
        {
            Number = number;
            BranchSpec = branchSpec;
        }

        internal StepId GetId() => new StepId(Number, BranchSpec);
    }
}