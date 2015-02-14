// from AvalonDock.Controls

using System;

namespace Diagnosis.Common.Util
{
    public class ReentrantFlag
    {
        public sealed class ReentrantFlagHandler : IDisposable
        {
            private ReentrantFlag owner;

            public ReentrantFlagHandler(ReentrantFlag owner)
            {
                this.owner = owner;
                owner.flag = true;
            }

            public void Dispose()
            {
                owner.flag = false;
                owner.handler = null;
            }
        }

        private bool flag = false;
        private ReentrantFlagHandler handler;

        /// <summary>
        /// Set flag, disallow reenter before dispose.
        /// </summary>
        /// <returns></returns>
        public ReentrantFlagHandler Enter()
        {
            if (flag)
                throw new InvalidOperationException();
            handler = new ReentrantFlagHandler(this);
            return handler;
        }

        /// <summary>
        /// Same as Enter, but if called after Enter (or self), return fake handler.
        /// This allows to check one flag many times, with only one reset at last dispose.
        /// </summary>
        /// <returns></returns>
        public ReentrantFlagHandler Join()
        {
            ReentrantFlag f;

            if (flag)
                f = new ReentrantFlag();
            else
                f = this;

            return new ReentrantFlagHandler(f);
        }

        public bool CanEnter
        {
            get { return !flag; }
        }
    }
}