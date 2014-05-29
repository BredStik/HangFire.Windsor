using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Castle.MicroKernel;
using FakeItEasy;

namespace HangFire.Windsor.Tests
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
    }

    public class JobStub
    { 
        
    }
}
