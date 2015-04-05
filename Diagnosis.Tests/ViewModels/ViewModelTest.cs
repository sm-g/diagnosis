using Diagnosis.Data;
using Diagnosis.Models;
using Diagnosis.ViewModels;
using Diagnosis.ViewModels.Autocomplete;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading;

namespace Diagnosis.Tests.ViewModels
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
