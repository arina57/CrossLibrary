using System;
using System.Diagnostics;

namespace CrossLibrary.DebugHelper {

	public sealed class Timer : IDisposable {

		public string Message { get; set; } = string.Empty;

		private Stopwatch stopwatch = new Stopwatch();


		public long TimeElapsed { get { return stopwatch.ElapsedMilliseconds; }  }

		public Timer() {
			stopwatch.Start();
		}
		public Timer(string message, bool startMessage = true) {
            this.Message = message;
            if (startMessage) {
                Debug.WriteLine("Timer started - " + this.Message);
            }
			stopwatch.Start();
		}

		public void Dispose() {
			stopwatch.Stop();
			Debug.WriteLine("Timer finished in "+ stopwatch.ElapsedMilliseconds + "ms - " + Message );
		}


	}
}
