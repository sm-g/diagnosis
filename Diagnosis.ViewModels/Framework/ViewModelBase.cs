using System;
using System.Diagnostics;

namespace Diagnosis.ViewModels
{
    public abstract class ViewModelBase : Diagnosis.Core.NotifyPropertyChangedBase, IDisposable
    {

        #region IDisposable

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Free other state (managed objects).
                }
                // Free your own state (unmanaged objects).
                // Set large fields to null.
                disposed = true;
#if DEBUG
                string msg = string.Format("    Finalized {0} ({1}) ({2}) ", this.GetType().Name, this.ToString(), this.GetHashCode());
                System.Diagnostics.Debug.Print(msg);
#endif
            }
        }

        ~ViewModelBase()
        {
            Dispose(false);
        }

        #endregion IDisposable Members
    }
    
    public class VmBaseEventArgs : EventArgs
    {
        public readonly ViewModelBase vm;

        [DebuggerStepThrough]
        public VmBaseEventArgs(ViewModelBase vm)
        {
            this.vm = vm;
        }
    }
}