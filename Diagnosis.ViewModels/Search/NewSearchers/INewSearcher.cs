using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Diagnosis.Data.Repositories;
using Diagnosis.Data.Specs;

namespace Diagnosis.ViewModels.Search
{
    public interface INewSearcher<out T>
    {
        IEnumerable<T> Search(string q);
    }
}
