using Diagnosis.Data.Repositories;
using Diagnosis.Models;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.App.ViewModels
{
    public class CategoryManager : ViewModelBase
    {
        private ICategoryRepository repository;

        public static CategoryViewModel NoCategory = new CategoryViewModel(new Category() { Title = "", Order = int.MaxValue });

        public ObservableCollection<CategoryViewModel> Categories
        {
            get;
            private set;
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