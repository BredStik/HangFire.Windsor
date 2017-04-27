using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.Windsor
{
    public class WindsorJobActivatorScope : JobActivatorScope
    {
        private readonly WindsorJobActivator _activator;
        private readonly List<Object> _resolved = new List<Object>();

        public WindsorJobActivatorScope(WindsorJobActivator activator)
        {
            if (activator == null) throw new ArgumentNullException(nameof(activator));
            _activator = activator;
        }

        public override object Resolve(Type type)
        {
            var instance = _activator.ActivateJob(type);
            _resolved.Add(instance);

            return instance;
        }

        public override void DisposeScope()
        {
            foreach (var instance in _resolved)
            {
                (instance as IDisposable)?.Dispose();
                _activator.ReleaseJob(instance);
            }
        }
    }
}
