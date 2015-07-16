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

		FileUpload upload;

		public FileUpload Upload
		{
			get {
				return upload;
			}
			set {
				upload = value;
				if (value != null)
				{
					this.lbl1.Text = this.Upload.CreationDate.ToLongDateString ();
					this.lbl2.Text = this.Upload.LocalFilePath;
				}
				else
				{
					this.lbl1.Text = "";
					this.lbl2.Text = "";
				}
			}
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
					this.Upload.Remove(deleteFile: false);
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
