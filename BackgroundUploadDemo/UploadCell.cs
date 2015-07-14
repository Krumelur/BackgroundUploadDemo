using Foundation;
using System;
using UIKit;
using CoreGraphics;

namespace BackgroundUploadDemo
{
	partial class UploadCell : UITableViewCell
	{
		public UploadCell (IntPtr handle) : base (handle)
		{
		}

		public FileUpload Upload
		{
			get;
			set;
		}

		public override void AwakeFromNib ()
		{
			this.btn.SetBackgroundImage(CreateButtonImage(), UIControlState.Normal);
		}

		partial void HandleButtonClick (UIButton sender)
		{

			switch (this.Upload.State)
			{
				case FileUpload.STATE.Started :
					this.Upload.Stop();
					break;
				case FileUpload.STATE.Stopping :
					// Do nothing.
					break;
				case FileUpload.STATE.Stopped :
					this.Upload.Start();
					break;
				case FileUpload.STATE.Uploaded :
					this.Upload.Remove();
					break;
				case FileUpload.STATE.Failed :
					this.Upload.Start();
					break;
			}

		}

		static UIImage CreateButtonImage()
		{
			UIImage buttonImage;

			UIGraphics.BeginImageContextWithOptions(new CGSize(10, 10), false, UIScreen.MainScreen.Scale);
			UIColor.Blue.SetFill();
			UIBezierPath.FromRoundedRect(new CGRect(0, 0, 10, 10), 3).Fill();
			buttonImage = UIGraphics.GetImageFromCurrentImageContext().CreateResizableImage(new UIEdgeInsets(4, 4, 4, 4));
			UIGraphics.EndImageContext();

			return buttonImage;
		}
	}
}
