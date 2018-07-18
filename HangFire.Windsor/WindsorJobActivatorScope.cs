using System;
using System.Collections.Generic;

namespace Hangfire.Windsor
{
    public class WindsorJobActivatorScope : JobActivatorScope
    {
        private readonly WindsorJobActivator _activator;
        private readonly IDisposable _containerScope;
        private readonly IList<object> _resolved = new List<object>();

        public WindsorJobActivatorScope(WindsorJobActivator activator, IDisposable containerScope)
        {
            if (activator == null) throw new ArgumentNullException(nameof(activator));
            if (containerScope == null) throw new ArgumentNullException(nameof(containerScope));

            _activator = activator;
            _containerScope = containerScope;
        }

        public override object Resolve(Type type)
        {
            var instance = _activator.ActivateJob(type);
            _resolved.Add(instance);

            return instance;
        }

        public override void DisposeScope()
        {
            try
            {
                foreach (var instance in _resolved)
                {
                    _activator.ReleaseJob(instance);
                }
            }
            finally
            {
                _containerScope.Dispose();
            }
        }
    }
}
