using System;

namespace Hangfire.Windsor.Tests.Stubs
{
    public class JobWithScopedDependency
    {
        public ExampleDependency Dependency { get; set; }

        public bool IsDisposed { get; set; }

        public JobWithScopedDependency(ExampleDependency dependency)
        {
            Dependency = dependency;
        }
    }
}