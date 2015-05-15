using Diagnosis.Common;
using Diagnosis.Data.Sync;
using Diagnosis.Models;
using NHibernate;
using NHibernate.Linq;
using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;

namespace Diagnosis.ViewModels.Screens
{
    /// <summary>
    /// При изменении сущности проверяет, что в БД есть другая с таким же значением.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ExistanceTester<T> : IDisposable
         where T : IEntity
    {
        private IExistTestable vm;
        private T editing;
        private Func<T, bool> extraTest;
        private ISession session;
        private Expression<Func<T, bool>> expr;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <param name="vm"></param>
        /// <param name="session"></param>
        /// <param name="customEqTest"></param>
        /// <param name="extraTest">Для сущности с таким же значением также верно это.</param>
        public ExistanceTester(T e, IExistTestable vm, ISession session, Expression<Func<T, bool>> customEqTest = null, Func<T, bool> extraTest = null)
        {
            Contract.Requires(e != null);
            Contract.Requires(vm != null);
            Contract.Requires(session != null);

            this.editing = e;
            this.vm = vm;
            this.session = session;

            this.expr = customEqTest ?? RHFactory.Create<T>().EqualsByVal(e);
            this.extraTest = extraTest ?? (x => true);
            // eqByVal = expr.Compile();

            e.PropertyChanged += e_PropertyChanged;
        }

        public void Test()
        {
            TestExisting();
        }

        public void Dispose()
        {
            editing.PropertyChanged -= e_PropertyChanged;
        }

        private void TestExisting()
        {
            //vm.HasExistingValue = existing.Any(ex => eqByVal(ex) && !editing.Equals(ex) && extraTest(ex));
            var existing = session.Query<T>()
                .Where(expr)
                .Where(x => x.Id != editing.Id)
                .ToList();
            vm.HasExistingValue = existing.Any() && existing.Any(ex => extraTest(ex));
        }

        private void e_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (vm.TestExistingFor.Contains(e.PropertyName))
            {
                TestExisting();
            }
        }
    }
}