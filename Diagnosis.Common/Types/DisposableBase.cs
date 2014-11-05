using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Diagnosis.Common
{
    public abstract class DisposableBase : IDisposable
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
                string msg = string.Format("    Disposed {0} ({1}) ({2}) ", this.GetType().Name, this.ToString(), this.GetHashCode());
                Debug.Print(msg);
#endif
            }
        }

        ~DisposableBase()
        {
            Dispose(false);
#if DEBUG
            string msg = string.Format("!!! Finalized {0} ({1}) ({2}) ", this.GetType().Name, this.ToString(), this.GetHashCode());
            Debug.Print(msg);
#endif
        }

        #endregion IDisposable Members
    }
}
