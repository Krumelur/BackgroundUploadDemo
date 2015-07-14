using Foundation;
using UIKit;
using System.Diagnostics;
using System;

namespace BackgroundUploadDemo
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
	[Register ("AppDelegate")]
	public class AppDelegate : UIApplicationDelegate
	{
		public const string HOST_ADDRESS = "127.0.0.1";
		public const int HOST_PORT = 9000;
		public const string DEFAULT_SESSION_IDENTIFIER = "net.csharx.backgroundsessionidentifier";

		public FileUploadManager Manager { get; private set; }

		// class-level declarations

		public override UIWindow Window
		{
			get;
			set;
		}

		public override bool FinishedLaunching (UIApplication application, NSDictionary launchOptions)
		{
			this.Manager = new FileUploadManager()
			{
				// Set to null to disable background upload for testing purposes.
				SessionIdentifier = DEFAULT_SESSION_IDENTIFIER
			};

			this.Manager.StartAsync();

			return true;
		}

		public Action BackgroundCompletionHandler
		{
			get;
			set;
		}

		public override void HandleEventsForBackgroundUrl (UIApplication application, string sessionIdentifier, System.Action completionHandler)
		{
			Console.WriteLine($"HandleEventsForBackgroundUrl() for identifier '{sessionIdentifier}'.");

			Debug.Assert(this.Manager != null, $"Expecting {nameof(FileUploadManager)} to be initialized!");
			Debug.Assert(this.Manager.SessionIdentifier == sessionIdentifier, "Session identifiers mismatch!");

			Debug.Assert(this.BackgroundCompletionHandler == null, "Expecting background handler to be NULL, otherwise app was woken up twice or did not reset the handler!");
			this.BackgroundCompletionHandler = completionHandler;
		}
	}
}


