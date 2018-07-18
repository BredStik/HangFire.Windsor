using System;

namespace Hangfire.Windsor.Tests.Stubs
{
    public class ExampleDependency: IDisposable
    {
        public void Dispose()
        {
            IsDisposed = true;
        }

        public bool IsDisposed { get; set; }
    }
}