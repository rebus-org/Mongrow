using System;
using System.Collections.Generic;
using System.Linq;

namespace Mongrow.Steps
{
    public static class GetSteps
    {
        public static IEnumerable<IStep> FromAssemblyOf<T>()
        {
            return typeof(T).Assembly.GetTypes()
                .Where(type => typeof(IStep).IsAssignableFrom(type)
                               && type.GetConstructors().Any(c => c.GetParameters().Length == 0))
                .Select(type => (IStep) Activator.CreateInstance(type));
        }
    }
}