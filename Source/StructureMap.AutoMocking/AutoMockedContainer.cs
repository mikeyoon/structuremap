using System;
using StructureMap.Graph;
using StructureMap.Pipeline;
using System.Linq;

namespace StructureMap.AutoMocking
{
    public class AutoMockedContainer : Container
    {
        private readonly ServiceLocator _locator;

        public AutoMockedContainer()
            : this(new RhinoMocksAAAServiceLocator())
        {
        }

        public AutoMockedContainer(ServiceLocator locator)
        {
            _locator = locator;

            onMissingFactory = delegate(Type pluginType, ProfileManager profileManager)
            {
                //Modified to inject concrete classes that have virtual methods
                if (!pluginType.IsAbstract && pluginType.IsClass && 
                    !pluginType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).Where(p => p.IsVirtual).Any())
                {
                    return null;
                }

                var factory = new InstanceFactory(new PluginFamily(pluginType));

                try
                {
                    object service = _locator.Service(pluginType);

                    var instance = new ObjectInstance(service);

                    profileManager.SetDefault(pluginType, instance);
                }
                catch (Exception)
                {
                    // ignore errors
                }

                return factory;
            };
        }
    }
}