using System;
using System.Diagnostics;

namespace Diagnosis.App.ViewModels
{
    public abstract class ViewModelBase : Diagnosis.Core.NotifyPropertyChangedBase, IDisposable
    {
        #region IDisposable Members

        public void Dispose()
        {
            this.OnDispose();
        }

        protected virtual void OnDispose()
        {
        }

#if DEBUG

        ~ViewModelBase()
        {
            string msg = string.Format("    Finalized {0} ({1}) ({2}) ", this.GetType().Name, this.ToString(), this.GetHashCode());
            System.Diagnostics.Debug.Print(msg);
        }

#endif

        #endregion IDisposable Members
    }

    public delegate void VmBaseEventHandler(object sender, VmBaseEventArgs e);

    public class VmBaseEventArgs : EventArgs
    {
        public ViewModelBase vm;

        [DebuggerStepThrough]
        public VmBaseEventArgs(ViewModelBase vm)
        {
            this.vm = vm;
        }
    }
}