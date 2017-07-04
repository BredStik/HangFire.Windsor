using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Castle.MicroKernel;
using FakeItEasy;
using Castle.Windsor;
using Castle.MicroKernel.Registration;

namespace Hangfire.Windsor.Tests
{
    [TestClass]
    public class WindsorJobActivatorTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void New_activator_given_null_kernel_should_throw_ArgumentNullException()
        {
            var activator = new WindsorJobActivator(null);
        }

        [TestMethod]
        public void New_activator_given_kernel_should_call_kernel_to_resolve_job_type()
        {
            var jobType = typeof(JobStub);
            var actualJob = new JobStub();

            var kernel = A.Fake<IKernel>();
            A.CallTo(() => kernel.Resolve(jobType)).Returns(actualJob);

            var activator = new WindsorJobActivator(kernel);

            var returnedJob = activator.ActivateJob(jobType);

            A.CallTo(() => kernel.Resolve(jobType)).MustHaveHappened();

            Assert.AreEqual(actualJob, returnedJob);
        }

        [TestMethod]
        public void Activator_should_release_job_when_disposed()
        {
            var container = new WindsorContainer();
            container.Register(Component.For<DisposibleJob>().ImplementedBy<DisposibleJob>().LifeStyle.Transient);

            DisposibleJob job = null;

            // For details about how this works see:
            // https://github.com/HangfireIO/Hangfire/blob/129707d66fde24dc6379fb9d6b15fa0b8ca48605/src/Hangfire.Core/Server/CoreBackgroundJobPerformer.cs#L47
            var activator = new WindsorJobActivator(container.Kernel);

            // Start a new job scope
            using (var scope = activator.BeginScope(null))
            {
                // Resolve the new job
                job = (DisposibleJob)scope.Resolve(typeof(DisposibleJob));

                // As long as we're in the scope, windsor should track the component
                Assert.IsTrue(container.Kernel.ReleasePolicy.HasTrack(job));
            }

            // When we leave the scope windsor should stop tracking the resolved job
            Assert.IsFalse(container.Kernel.ReleasePolicy.HasTrack(job));
        }
    }
}

public class JobStub
{

}

public class DisposibleJob : IDisposable
{
    public void Dispose()
    {
        // Ignored
    }
}
