using Diagnosis.Models;
using System.Collections.Generic;

namespace Diagnosis.Data.Repositories
{
    public interface IPropertyRepository : IRepository<Property>
    {
        Property GetByTitle(string title);
    }

    public interface IWordRepository : IRepository<Word>
    {
        Word GetByTitle(string title);
    }

    public interface IIcdChapterRepository : IRepository<IcdChapter>
    {
        IcdChapter GetByTitle(string title);
        IcdChapter GetByCode(string code);
    }
}
