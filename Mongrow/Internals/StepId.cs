using System;
using System.Linq;

namespace Mongrow.Internals
{
    class StepId
    {
        public int Number { get; }
        public string BranchSpec { get; }

        public StepId(int number, string branchSpec)
        {
            Number = number;
            BranchSpec = branchSpec;
        }

        public override string ToString() => $"{Number}/{BranchSpec}";

        protected bool Equals(StepId other)
        {
            return Number == other.Number
                   && string.Equals(BranchSpec, other.BranchSpec);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((StepId)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Number * 397) ^ BranchSpec.GetHashCode();
            }
        }

        public static bool operator ==(StepId left, StepId right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(StepId left, StepId right)
        {
            return !Equals(left, right);
        }

        public static StepId Parse(string str)
        {
            var parts = str.Split('/');

            if (parts.Length < 2 || !int.TryParse(parts.First(), out var number))
            {
                throw new FormatException($"The string '{str}' cannot be parsed into a step ID! Step IDs must consist of at least two parts on the form <number>/<text>, e.g. like this: '23/master'");
            }

            return new StepId(number, string.Join("/", parts.Skip(1)));
        }
    }
}