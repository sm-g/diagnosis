// from AvalonDock.Controls

using System;

namespace Diagnosis.Common.Util
{
    public class ReentrantFlag
    {
        public class _ReentrantFlagHandler : IDisposable
        {
            private ReentrantFlag _owner;

            public _ReentrantFlagHandler(ReentrantFlag owner)
            {
                _owner = owner;
                _owner._flag = true;
            }

            public void Dispose()
            {
                _owner._flag = false;
            }
        }

        private bool _flag = false;

        public _ReentrantFlagHandler Enter()
        {
            if (_flag)
                throw new InvalidOperationException();
            return new _ReentrantFlagHandler(this);
        }

        public bool CanEnter
        {
            get { return !_flag; }
        }
    }
}