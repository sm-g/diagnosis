using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Diagnosis.Common.Util
{
    /// <summary>
    /// Usage:
    /// using (PerformanceTester.Start((elapsed) => Console.WriteLine(elapsed)))
    /// {
    ///    code to test
    /// }
    /// </summary>
    public sealed class PerformanceTester : IDisposable
    {
        private Stopwatch _stopwatch = new Stopwatch();
        private Action<TimeSpan> _callback;

        public PerformanceTester(Action<TimeSpan> callback)
        {
            _callback = callback;
            _stopwatch.Start();
        }

        public static PerformanceTester Start(Action<TimeSpan> callback)
        {
            return new PerformanceTester(callback);
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            if (_callback != null)
                _callback(Result);
        }

        public TimeSpan Result
        {
            get { return _stopwatch.Elapsed; }
        }
    }
}
