using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StructureMap.AutoMocking
{
    public class FakeItEasyAutoMocker<TARGETCLASS> : AutoMocker<TARGETCLASS> where TARGETCLASS : class
    {
        public FakeItEasyAutoMocker()
        {
            _serviceLocator = new FakeItEasyServiceLocator();
            _container = new AutoMockedContainer(_serviceLocator);
        }
    }
}
