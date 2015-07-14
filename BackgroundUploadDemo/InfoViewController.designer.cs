// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace BackgroundUploadDemo
{
	[Register ("InfoViewController")]
	partial class InfoViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel createdLabel { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel errorLabel { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel HTTPStatusLabel { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel progressLabel { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel sizeLabel { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel stateLabel { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel URLLabel { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel UUIDLabel { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (createdLabel != null) {
				createdLabel.Dispose ();
				createdLabel = null;
			}
			if (errorLabel != null) {
				errorLabel.Dispose ();
				errorLabel = null;
			}
			if (HTTPStatusLabel != null) {
				HTTPStatusLabel.Dispose ();
				HTTPStatusLabel = null;
			}
			if (progressLabel != null) {
				progressLabel.Dispose ();
				progressLabel = null;
			}
			if (sizeLabel != null) {
				sizeLabel.Dispose ();
				sizeLabel = null;
			}
			if (stateLabel != null) {
				stateLabel.Dispose ();
				stateLabel = null;
			}
			if (URLLabel != null) {
				URLLabel.Dispose ();
				URLLabel = null;
			}
			if (UUIDLabel != null) {
				UUIDLabel.Dispose ();
				UUIDLabel = null;
			}
		}
	}
}
