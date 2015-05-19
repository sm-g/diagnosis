using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Diagnosis.Common.Types;

namespace Diagnosis.Common
{
    /// <summary>
    /// Хранит историю состояний некоторого объекта. Без подряд идущих одинаковых состояний.
    /// Перед началом - состояние в начале, после конца - в конце.
    /// </summary>
    public class History<T> : NotifyPropertyChangedBase where T : class
    {
        private int current;
        private List<T> list;

        public History()
        {
            list = new List<T>();
            current = -1;
        }

        public int LastVersion
        {
            get { return list.Count - 1; }
        }
        public int CurrentVersion
        {
            get { return current; }
            private set
            {
                current = value;
                OnPropertyChanged(() => CurrentVersion);
                OnPropertyChanged(() => CurrentState);
                OnPropertyChanged(() => CurrentIsFirst);
                OnPropertyChanged(() => CurrentIsLast);
            }
        }

        public bool IsEmpty { get { return current == -1; } }
        public bool CurrentIsFirst { get { return IsEmpty || current == 0; } }
        public bool CurrentIsLast { get { return CurrentVersion == LastVersion; } }

        public T CurrentState
        {
            get
            {
                return list.Any() ? list[current] : null;
            }
        }

        public bool Memorize(T state)
        {
            // удаляем неактуальные состояния
            if (current != LastVersion)
            {
                var odd = list.Skip(current + 1).Count();
                list.RemoveRange(current + 1, odd);
            }
            // сохраняяем только новое состояние
            if (!state.Equals(list.LastOrDefault()))
            {
                list.Add(state);
                CurrentVersion = LastVersion;
                OnPropertyChanged(() => LastVersion);
                OnPropertyChanged(() => IsEmpty);
                return true;
            }
            CurrentVersion = LastVersion;
            OnPropertyChanged(() => LastVersion);
            return false;
        }

        public T MoveBack()
        {
            if (IsEmpty)
                return null;

            CurrentVersion--;
            if (current < 0)
            {
                current = 0;
            }
            return list[current];
        }

        public T MoveForward()
        {
            if (IsEmpty)
                return null;

            CurrentVersion++;
            if (current > LastVersion)
            {
                current = LastVersion;
            }
            return list[current];
        }
        public override string ToString()
        {
            return string.Format("current#{0},\n{1}", current, string.Join("\n", list));
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(current >= -1);
            Contract.Invariant(current <= list.Count - 1);
            Contract.Invariant(CurrentVersion <= LastVersion);
            Contract.Invariant(list.Count < 2 || Enumerable.Range(0, list.Count - 2).All(x => list[x] != list[x + 1])); // Без подряд идущих одинаковых состояний
        }
    }
}