using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Castle.MicroKernel;
using Castle.MicroKernel.Lifestyle;
using FakeItEasy;
using Castle.Windsor;
using Castle.MicroKernel.Registration;
using Hangfire.Windsor.Tests.Stubs;

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
        public void ActivatorScope_should_release_and_dispose_job_when_disposed()
        {
            var container = new WindsorContainer();

            IDisposableJob job = null;

            container.Register(
                Component.For<IDisposableJob>()
                    .UsingFactoryMethod(A.Fake<IDisposableJob>)
                    .OnDestroy(x => { job = x; })
                    .LifeStyle.Transient
                );
            
            // For details about how this works see:
            // https://github.com/HangfireIO/Hangfire/blob/129707d66fde24dc6379fb9d6b15fa0b8ca48605/src/Hangfire.Core/Server/CoreBackgroundJobPerformer.cs#L47
            var activator = new WindsorJobActivator(container.Kernel);

            // Start a new job scope
            using (var scope = activator.BeginScope(null))
            {
                // Resolve the new job
                var resolvedJob = scope.Resolve(typeof(IDisposableJob)) as IDisposableJob;

                // As long as we're in the scope, windsor should track the component
                Assert.IsTrue(container.Kernel.ReleasePolicy.HasTrack(resolvedJob));
            }

            // When we leave the scope windsor should stop tracking the resolved job
            Assert.IsFalse(container.Kernel.ReleasePolicy.HasTrack(job));

            A.CallTo(() => job.Dispose()).MustHaveHappened();
        }

        [TestMethod]
        public void ActivatorScope_should_release_and_dispose_scoped_job_when_disposed()
        {
            var container = new WindsorContainer();

            IDisposableJob job = null;

            container.Register(
                Component.For<IDisposableJob>()
                    .UsingFactoryMethod(A.Fake<IDisposableJob>)
                    .OnDestroy(x => { job = x; })
                    .LifestyleScoped()
                );
            
            // For details about how this works see:
            // https://github.com/HangfireIO/Hangfire/blob/129707d66fde24dc6379fb9d6b15fa0b8ca48605/src/Hangfire.Core/Server/CoreBackgroundJobPerformer.cs#L47
            var activator = new WindsorJobActivator(container.Kernel);

            // Start a new job scope
            using (var scope = activator.BeginScope(null))
            {
                // Resolve the new job
                var resolvedJob = scope.Resolve(typeof(IDisposableJob)) as IDisposableJob;
                Assert.IsNotNull(resolvedJob);
                
            }

            Assert.IsNotNull(job, "job was not released by container");

            A.CallTo(() => job.Dispose()).MustHaveHappened();
        }

        [TestMethod]
        public void Experiment_windsor_scoped_service_registration()
        {
            using (var container = new WindsorContainer())
            {
                ExampleDependency releasedDependency = null;
                container.Register(
                    Component.For<JobWithScopedDependency>().LifestyleTransient(),
                    Component.For<ExampleDependency>()
                        .OnDestroy(x => { releasedDependency = x; })
                        .LifestyleScoped()
                );

                using (container.BeginScope())
                {
                    var job1 = container.Resolve<JobWithScopedDependency>();
                    var job2 = container.Resolve<JobWithScopedDependency>();
                    Assert.AreNotSame(job1, job2, 
                        "assumed JobWithScopedDependency has transient lifestyle");

                    Assert.AreSame(job1.Dependency, job2.Dependency, 
                        "assumed ExampleDependency has scoped lifestyle");

                    // sanity check
                    Assert.IsNull(releasedDependency);
                }

                Assert.IsNotNull(releasedDependency, "service was not released by container");
                Assert.IsTrue(releasedDependency.IsDisposed, "service was not disposed");
            }
        }

        [TestMethod]
        public void ActivatorScope_should_release_scoped_service_dependencies_when_disposed()
        {
            using (var container = new WindsorContainer())
            {
                ExampleDependency releasedDependency = null;
                container.Register(
                    Component.For<JobWithScopedDependency>().LifestyleTransient(),
                    Component.For<ExampleDependency>()
                        .OnDestroy(x => { releasedDependency = x; })
                        .LifestyleScoped()
                );

                var activator = new WindsorJobActivator(container.Kernel);

                using (var scope = activator.BeginScope(null))
                {
                    // Resolve the new job
                    var job = scope.Resolve(typeof(JobWithScopedDependency)) as JobWithScopedDependency;
                    Assert.IsNotNull(job);

                    Assert.AreSame(job.Dependency, container.Resolve<ExampleDependency>(),
                        "assumed ExampleDependency has scoped lifestyle");

                    // sanity check
                    Assert.IsNull(releasedDependency);
                }

                Assert.IsNotNull(releasedDependency, "service was not released by container");
                Assert.IsTrue(releasedDependency.IsDisposed, "service was not disposed");
            }
        }
    }
}