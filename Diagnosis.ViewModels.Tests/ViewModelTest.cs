using Diagnosis.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading;

namespace Diagnosis.ViewModels.Tests
{
    [TestClass]
    public abstract class ViewModelTest : InMemoryDatabaseTest
    {
        [TestInitialize]
        public void ViewModelTestInit()
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
        }
    }
}