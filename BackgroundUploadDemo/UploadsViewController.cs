using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.ObjectModel;
using CoreGraphics;

namespace BackgroundUploadDemo
{
	partial class UploadsViewController : UITableViewController
	{
		public UploadsViewController (IntPtr handle) : base (handle)
		{
		}

		FileUploadManager Manager => ((AppDelegate)(UIApplication.SharedApplication.Delegate)).Manager;

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			var uploadCell = this.TableView.DequeueReusableCell("cell");
			this.TableView.RowHeight = uploadCell.Frame.Height;
		}

		UIActionSheet sheet;

		ObservableCollection<FileUpload> uploads = new ObservableCollection<FileUpload>();

		public override nint RowsInSection (UITableView tableview, nint section)
		{
			return this.uploads.Count;
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			var upload = this.uploads[indexPath.Row];

			var uploadCell = (UploadCell)tableView.DequeueReusableCell("cell");
			uploadCell.Upload = upload;

			return uploadCell;
		}

		public override void CellDisplayingEnded (UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
		{
			((UploadCell)cell).Upload = null;
		}

		partial void HandleAddUpload (UIBarButtonItem sender)
		{
			this.sheet = new UIActionSheet ("Add Upload", null, "Cancel", null, "S (Bytes)", "M (Kilobytes)", "L (Megabytes)", "XL (Gigabytes)");
			this.sheet.Dismissed += this.HandleSelectUpload;
			this.sheet.ShowFrom (this.NavigationItem.RightBarButtonItem, true);
		}

		void HandleSelectUpload (object sender, UIButtonEventArgs args)
		{
			this.sheet.Dismissed -= this.HandleSelectUpload;
			this.sheet = null;

			NSUrl fileURL = args.ButtonIndex <= 3 ? GetUrlForTestImage (args.ButtonIndex + 1) : null;
			var hostUrl = NSUrl.FromString ($"http://{AppDelegate.HOST_ADDRESS}:{AppDelegate.HOST_PORT}");


			var request = new NSMutableUrlRequest(hostUrl)
			{
				HttpMethod = "PUT"
			};

			var keys = new object[] { "Content-Type" };
			var objects = new object[] { "image/png" };
			request.Headers = NSDictionary.FromObjectsAndKeys (objects, keys);


			// TODO: Create upload request
			// void) [self.manager createUploadWithRequest:request fileURL:fileURL];
		}

		/// <summary>
		/// Gets the URL for test image.
		/// In order to fully test uploads, we need some really big files.  Rather than carry 
		/// these files around in our binary, we synthesise them.  Specifically, for each test image, 
		/// we expand the image by an order of magnitude, based on its image number.  That is, image 1 
		/// is not expanded, image 2 gets expanded 10 times, and so on.  We expand the image by simply 
		/// copying it to the temporary directory, writing the same data to the file over and over again.
		/// </summary>
		/// <returns>The URL for test image.</returns>
		/// <param name="imageNum">Image number.</param>
		static NSUrl GetUrlForTestImage (nint imageNum)
		{
			if (imageNum < 1 || imageNum > 4)
			{
				return null;
			}

			var expFactor = (nint)Math.Pow (10, imageNum - 1);

			var tmpPath = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), "..", "tmp");
			var fullExpFilename = Path.Combine (tmpPath, $"bigimage_{imageNum}.png");

			if (!File.Exists (fullExpFilename))
			{
				Console.WriteLine("Creating test data...please be patient!");
				using (var sourceStream = File.Open ($"TestImage{imageNum}.png", FileMode.Open, FileAccess.Read))
				using (var targetStream = File.Open (fullExpFilename, FileMode.Create))
				{
					for (int counter = 0; counter < expFactor; counter++)
					{
						sourceStream.CopyTo (targetStream);
						sourceStream.Seek (0, SeekOrigin.Begin);
					}
				}
			}

			Console.WriteLine($"Using tmp file at '{fullExpFilename}'");

			return NSUrl.FromFilename (fullExpFilename);
		}

		partial void HandleQuitApp (UIBarButtonItem sender)
		{
			Exit (3);	
		}

		/// <summary>
		/// Import private API to allow exiting app manually.
		/// For demo purposes only! Do not use this in productive apps!
		/// </summary>
		/// <param name="status">Status.</param>
		[DllImport ("__Internal", EntryPoint = "exit")]
		public static extern void Exit (int status);
	}
}
