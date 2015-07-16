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
			NSOperationQueue.MainQueue.AddOperation (() => {
				this.Manager.DidFinishBackgroundEvents?.Invoke (this, session);
			});
		}

		public override void DidSendBodyData (NSUrlSession session, NSUrlSessionTask task, long bytesSent, long totalBytesSent, long totalBytesExpectedToSend)
		{
			var fileUpload = this.Manager.GetUploadByTask (task);
			/*
			#pragma unused(bytesSent)
			FileUpload *    upload;
			double          newProgress;

			assert(session == self.session);
			assert(totalBytesExpectedToSend != NSURLSessionTransferSizeUnknown);        // because we provided a file to upload, NSURLSession should 
			// always give us meaningful progress
			upload = [self uploadForTask:task];
			if (upload != nil) {
				newProgress = (double) totalBytesSent / (double) totalBytesExpectedToSend;
				if (upload.progress != newProgress) {
					upload.progress = newProgress;
				}
			}*/
		}

		public override void DidCompleteWithError (NSUrlSession session, NSUrlSessionTask task, NSError error)
		{
			
		}
	}


}

