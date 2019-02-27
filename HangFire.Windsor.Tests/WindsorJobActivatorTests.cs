using System;
using Castle.MicroKernel;
using Castle.MicroKernel.Lifestyle;
using FakeItEasy;
using Castle.Windsor;
using Castle.MicroKernel.Registration;
using Hangfire.Windsor.Tests.Stubs;
using Xunit;

namespace Hangfire.Windsor.Tests
{
    public class WindsorJobActivatorTests
    {
        [Fact]
        public void New_activator_given_null_kernel_should_throw_ArgumentNullException()
        {
            try
            {
                var activator = new WindsorJobActivator(null);
            }
            catch(ArgumentNullException exc)
            {

                Assert.NotNull(exc);
                return;
            }

            throw new Exception("Should have thrown ArgumentNullException");            
        }

        [Fact]
        public void New_activator_given_kernel_should_call_kernel_to_resolve_job_type()
        {
            var jobType = typeof(JobStub);
            var actualJob = new JobStub();

            var kernel = A.Fake<IKernel>();
            A.CallTo(() => kernel.Resolve(jobType)).Returns(actualJob);

            var activator = new WindsorJobActivator(kernel);

            var returnedJob = activator.ActivateJob(jobType);

            A.CallTo(() => kernel.Resolve(jobType)).MustHaveHappened();

            Assert.Equal(actualJob, returnedJob);
        }

        [Fact]
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
                Assert.True(container.Kernel.ReleasePolicy.HasTrack(resolvedJob));
            }

            // When we leave the scope windsor should stop tracking the resolved job
            Assert.False(container.Kernel.ReleasePolicy.HasTrack(job));

            A.CallTo(() => job.Dispose()).MustHaveHappened();
        }

        [Fact]
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
                Assert.NotNull(resolvedJob);
                
            }

            Assert.NotNull(job);

            A.CallTo(() => job.Dispose()).MustHaveHappened();
        }

        [Fact]
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
                    Assert.NotSame(job1, job2);

                    Assert.Same(job1.Dependency, job2.Dependency);

                    // sanity check
                    Assert.Null(releasedDependency);
                }

                Assert.NotNull(releasedDependency);
                Assert.True(releasedDependency.IsDisposed);
            }
        }

        [Fact]
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
                    Assert.NotNull(job);

                    Assert.Same(job.Dependency, container.Resolve<ExampleDependency>());

                    // sanity check
                    Assert.Null(releasedDependency);
                }

                Assert.NotNull(releasedDependency);
                Assert.True(releasedDependency.IsDisposed);
            }
        }
    }
}