using System.Linq;
using System.Reflection;
using Mongrow.Steps;

namespace Mongrow.Internals
{
    static class StepExtensions
    {
        public static StepId GetId(this IStep step) => GetAttribute(step).GetId();

        public static string GetDescription(this IStep step) => GetAttribute(step).Decription ?? "";

        static StepAttribute GetAttribute(IStep step) => step.GetType().GetCustomAttributes().OfType<StepAttribute>().First();
    }
}