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
	[Register ("UploadCell")]
	partial class UploadCell
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton btn { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel lbl1 { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UILabel lbl2 { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (btn != null) {
				btn.Dispose ();
				btn = null;
			}
			if (lbl1 != null) {
				lbl1.Dispose ();
				lbl1 = null;
			}
			if (lbl2 != null) {
				lbl2.Dispose ();
				lbl2 = null;
			}
		}
	}
}
