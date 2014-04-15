using Diagnosis.Data.Repositories;
using Diagnosis.Models;
using EventAggregator;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;

namespace Diagnosis.App.ViewModels
{
    public class CategoryManager : ViewModelBase
    {
        private ICategoryRepository repository;

        public ObservableCollection<CategoryViewModel> Categories
        {
            get;
            private set;
        }

        public CategoryViewModel Find(string title)
        {
            return Categories.Where(w => w.Name == title).SingleOrDefault();
        }

        public CategoryViewModel GetByModel(Category cat)
        {
            return Categories.FirstOrDefault(a => a.category == cat);
        }

        public CategoryManager(ICategoryRepository repo)
        {
            Contract.Requires(repo != null);

            repository = repo;

            var all = repository.GetAll().Select(s => new CategoryViewModel(s));

            Categories = new ObservableCollection<CategoryViewModel>(all);
        }
    }
}