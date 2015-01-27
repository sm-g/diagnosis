using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.Common.Util
{
    public class FlagActionWrapper<T>
    {
        private bool flag = false;

        private T param;

        private Action<T> act;

        public FlagActionWrapper(Action<T> act)
        {
            Contract.Requires(act != null);

            this.act = act;
        }

        public bool CanEnter
        {
            get { return !flag; }
        }

        public ActionWrapperHandler Enter(T param)
        {
            Contract.Requires(param != null);

            if (flag)
                throw new InvalidOperationException();

            this.param = param;
            return new ActionWrapperHandler(this);
        }

        /// <summary>
        /// Same as Enter, but if called after Enter (or self), return fake handler.
        /// This allows to check one flag many times, with only one reset at last dispose.
        /// </summary>
        /// <returns></returns>
        public ActionWrapperHandler Join(T param)
        {
            FlagActionWrapper<T> f;

            if (flag)
                f = new FlagActionWrapper<T>((p) => { });
            else
                f = this;

            return new ActionWrapperHandler(f);
        }

        public class ActionWrapperHandler : IDisposable
        {
            private FlagActionWrapper<T> owner;

            public ActionWrapperHandler(FlagActionWrapper<T> owner)
            {
                this.owner = owner;
                owner.flag = true;
            }

            public void Dispose()
            {
                owner.act(owner.param);
                owner.flag = false;
            }
        }
    }

    public class FlagActionWrapper
    {
        private bool flag = false;

        private Action act;

        public FlagActionWrapper(Action act)
        {
            Contract.Requires(act != null);

            this.act = act;
        }

        public bool CanEnter
        {
            get { return !flag; }
        }

        public ActionWrapperHandler Enter()
        {
            if (flag)
                throw new InvalidOperationException();

            return new ActionWrapperHandler(this);
        }

        /// <summary>
        /// Same as Enter, but if called after Enter (or self), return fake handler.
        /// This allows to check one flag many times, with only one reset at last dispose.
        /// </summary>
        /// <returns></returns>
        public ActionWrapperHandler Join()
        {
            FlagActionWrapper f;

            if (flag)
                f = new FlagActionWrapper(() => { });
            else
                f = this;

            return new ActionWrapperHandler(f);
        }

        public class ActionWrapperHandler : IDisposable
        {
            private FlagActionWrapper owner;

            public ActionWrapperHandler(FlagActionWrapper owner)
            {
                this.owner = owner;
                owner.flag = true;
            }

            public void Dispose()
            {
                owner.act();
                owner.flag = false;
            }
        }
    }
}