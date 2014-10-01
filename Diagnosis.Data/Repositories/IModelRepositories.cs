using Diagnosis.Models;
using System.Collections.Generic;

namespace Diagnosis.Data.Repositories
{
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
