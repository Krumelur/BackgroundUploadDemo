using System;
using Foundation;
using System.Diagnostics;

namespace BackgroundUploadDemo
{
	public class FileUploadDelegate : NSUrlSessionDataDelegate
	{
		public FileUploadDelegate (FileUploadManager manager)
		{
			this.Manager = manager;	
		}

		WeakReference<FileUploadManager> weakManager;

		public FileUploadManager Manager
		{
			get
			{
				FileUploadManager manager = null;
				this.weakManager.TryGetTarget(out manager);
				return manager;
			}
			set
			{
				this.weakManager = new WeakReference<FileUploadManager>(value);
			}
		}

		public override void DidFinishEventsForBackgroundSession (NSUrlSession session)
		{
			this.Manager.OnDidFinishBackgroundEvents(session);

			this.Manager.ActiveUploads.OnCollectionChanged();
		}

		public override void DidSendBodyData (NSUrlSession session, NSUrlSessionTask task, long bytesSent, long totalBytesSent, long totalBytesExpectedToSend)
		{
			var fileUpload = this.Manager.GetUploadByTask (task);
			Debug.Assert(fileUpload != null, "Could not find FileUpload object for task!");
			if(fileUpload == null)
			{
				return;
			}

			fileUpload.Progress = (float)totalBytesSent / (float)totalBytesExpectedToSend;

			this.Manager.ActiveUploads.OnCollectionChanged();
		}

		public override void DidCompleteWithError (NSUrlSession session, NSUrlSessionTask task, NSError error)
		{

			var fileUpload = this.Manager.GetUploadByTask (task);
			Debug.Assert(fileUpload != null, "Could not find FileUpload object for task!");
			if(fileUpload == null)
			{
				return;
			}

			Debug.Assert(fileUpload.State == FileUpload.STATE.Started || fileUpload.State == FileUpload.STATE.Stopping, "Upload is in invalid state!");

			var urlErrorCode = NSUrlError.Unknown;

			if(error != null)
			{
				Enum.TryParse<NSUrlError>(error.Code.ToString(), out urlErrorCode);
			}

			if(error == null)
			{
				fileUpload.Progress = 0f;
				fileUpload.Error = null;
				fileUpload.Response = fileUpload.UploadTask.Response as NSHttpUrlResponse;
				fileUpload.UploadTask = null;
				fileUpload.State = FileUpload.STATE.Uploaded;
				Console.WriteLine($"Completed upload {fileUpload}.");
			}
			else if(urlErrorCode == NSUrlError.Cancelled)
			{
				fileUpload.Error = null;
				fileUpload.UploadTask = null;
				fileUpload.State = FileUpload.STATE.Stopped;
			}
			else
			{
				// Upload was stopped by the network.
				fileUpload.Error = error;
				fileUpload.UploadTask = null;
				fileUpload.State = FileUpload.STATE.Failed;
				Console.WriteLine($"Upload failed: {fileUpload}");
			}

			fileUpload.IsStateValid();

			this.Manager.ActiveUploads.OnCollectionChanged();
		}
	}


}

