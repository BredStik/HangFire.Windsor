using Castle.MicroKernel;
using System;
using Castle.MicroKernel.Lifestyle;

namespace Hangfire.Windsor
{
    /// <summary>
    /// Hangfire Job Activator based on Castle Windsor IoC Container.
    /// </summary>
    public class WindsorJobActivator: JobActivator
    {
        private readonly IKernel _kernel;

        /// <summary>
        /// Initializes new instance of WindsorJobActivator with a Windsor Kernel
        /// </summary>
        /// <param name="kernel">Kernel that will be used to create instance
        /// of classes during job activation process.</param>
        public WindsorJobActivator(IKernel kernel)
        {
            if (kernel == null) throw new ArgumentNullException(nameof(kernel));

            _kernel = kernel;
        }

        /// <summary>
        /// Activates a job of a given type using the Windsor Kernel
        /// </summary>
        /// <param name="jobType">Type of job to activate</param>
        /// <returns></returns>
        public override object ActivateJob(Type jobType)
        {
            return _kernel.Resolve(jobType);
        }

        [Obsolete]
        public override JobActivatorScope BeginScope()
        {
            return CreateScope();
        }

        private WindsorJobActivatorScope CreateScope()
        {
            // support scoped lifestyles (https://github.com/castleproject/Windsor/blob/master/docs/lifestyles.md#scoped)
            return new WindsorJobActivatorScope(this, _kernel.BeginScope());
        }

        public override JobActivatorScope BeginScope(JobActivatorContext context)
        {
            return CreateScope();
        }        

        public void ReleaseJob(object job)
        {
            _kernel.ReleaseComponent(job);
        }
    }
}
