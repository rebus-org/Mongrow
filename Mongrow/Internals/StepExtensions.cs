using System.Linq;
using System.Reflection;
using Mongrow.Steps;

namespace Mongrow.Internals
{
    static class StepExtensions
    {
        public static StepId GetId(this IStep step) => step.GetType().GetCustomAttributes().OfType<StepAttribute>().First().GetId();
    }
}