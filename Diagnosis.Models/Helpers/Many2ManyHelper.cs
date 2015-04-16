using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Diagnosis.Models
{
    class Many2ManyHelper<TM2M, TValue>
    {
        IList<TValue> cache;
        private Func<TM2M, TValue> selector;
        private Func<TM2M, bool> where;
        private ISet<TM2M> set;

        public Many2ManyHelper(ISet<TM2M> set, Func<TM2M, bool> where, Func<TM2M, TValue> selector)
        {
            this.set = set;
            this.where = where;
            this.selector = selector;
        }
        public IEnumerable<TValue> Values
        {
            get
            {
                return cache ?? (cache = set
                   .Where(x => where(x))
                   .Select(x => selector(x))
                   .ToList());
            }
        }
        public void Reset()
        {
            cache = null;
        }

        public bool Add(TM2M m2m)
        {
            if (set.Add(m2m))
            {
                Reset();
                return true;
            }
            return false;
        }

        public bool Remove(TM2M m2m)
        {
            if (set.Remove(m2m))
            {
                Reset();
                return true;
            }
            return false;
        }
        public bool Add(TValue e)
        {
            var m2m = set.Where(x => selector(x).Equals(e)).FirstOrDefault();

            if (m2m != null && set.Add(m2m))
            {
                Reset();
                return true;
            }
            return false;
        }
        public bool Remove(TValue e)
        {
            var m2m = set.Where(x => selector(x).Equals(e)).FirstOrDefault();

            if (m2m != null && set.Remove(m2m))
            {
                Reset();
                return true;
            }
            return false;
        }
    }
}
