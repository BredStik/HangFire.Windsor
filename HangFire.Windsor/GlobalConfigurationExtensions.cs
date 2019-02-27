using System;
using Hangfire.Annotations;
using Castle.MicroKernel;

namespace Hangfire.Windsor
{
    public static class GlobalConfigurationExtensions
    {
        public static IGlobalConfiguration<WindsorJobActivator> UseWindsorActivator(
            [NotNull] this IGlobalConfiguration configuration,
            [NotNull] IKernel kernel)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            if (kernel == null) throw new ArgumentNullException("kernel");

            return configuration.UseActivator(new WindsorJobActivator(kernel));
        }
    }
}