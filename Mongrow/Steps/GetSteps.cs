using System;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable UnusedMember.Global

namespace Mongrow.Steps;

public static class GetSteps
{
    public static IEnumerable<IStep> FromAssemblyOf<T>()
    {
        return typeof(T).Assembly.GetTypes()
            .Where(IsStepType)
            .Select(CreateStepInstance);
    }

    public static IEnumerable<IStep> FromNamespaceOf<TStep>() where TStep : IStep
    {
        var ns = typeof(TStep).Namespace;
        var prefix = $"{ns}.";

        bool IsInCorrectNamespace(Type type) => type.Namespace != null
                                                && (string.Equals(ns, type.Namespace)
                                                    || type.Namespace.StartsWith(prefix));

        return typeof(TStep).Assembly.GetTypes()
            .Where(IsStepType)
            .Where(IsInCorrectNamespace)
            .Select(CreateStepInstance);
    }

    static IStep CreateStepInstance(Type type)
    {
        try
        {
            return (IStep) Activator.CreateInstance(type);
        }
        catch (Exception exception)
        {
            throw new ArgumentException($"Could not create instance of step {type}", exception);
        }
    }

    static bool IsStepType(Type type) => typeof(IStep).IsAssignableFrom(type) && type.GetConstructors().Any(c => c.GetParameters().Length == 0);
}