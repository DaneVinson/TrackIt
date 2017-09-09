using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackIt.Domain.Model.Base
{
    /// <summary>
    /// Abstract class to provide a robust mechanism for implementing IDisposable.  
    /// Based on: https://lostechies.com/chrispatterson/2012/11/29/idisposable-done-right/
    /// </summary>
    public abstract class CleanDisposable : IDisposable
    {
        public CleanDisposable()
        { }

        ~CleanDisposable()
        {
            Dispose(false);
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (Disposed) { return; }

            if (disposing) { DisposeManagedResources(); }

            ReleaseUnmanagedResources();

            Disposed = true;
        }

        protected abstract void DisposeManagedResources();

        protected virtual void ReleaseUnmanagedResources()
        { }


        private bool Disposed { get; set; }
    }
}
