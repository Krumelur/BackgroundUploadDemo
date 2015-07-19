using System;
using Foundation;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace BackgroundUploadDemo
{
	public class FileUploadManager
	{
		public FileUploadManager ()
		{
			this.ActiveUploads = new UploadCollection();
		}

		public event EventHandler<NSUrlSession> DidFinishBackgroundEvents;

		internal void OnDidFinishBackgroundEvents (NSUrlSession session)
		{
			NSOperationQueue.MainQueue.AddOperation (() => {
				this.DidFinishBackgroundEvents?.Invoke(this, session);
			});
		}

		NSUrlSession session;

		public UploadCollection ActiveUploads
		{
			get;
			private set;
		}

		/// <summary>
		/// The background session identifier. Set to NULL to use an ephemeral session that won't work in the background.
		/// </summary>
		/// <value>The session identifier.</value>
		public string SessionIdentifier
		{
			get;
			set;
		}

		public async Task StartAsync()
		{
			Debug.Assert(this.session == null, "Session already initialized!");

			// Create our view of the world based on the on-disk data structures.
			this.RestoreAllUploadsInWorkDirectory();

			NSUrlSessionConfiguration config;
			if (!string.IsNullOrWhiteSpace(this.SessionIdentifier))
			{
				Console.WriteLine($"Creating background session with identifier '{this.SessionIdentifier}'");
				config = NSUrlSessionConfiguration.CreateBackgroundSessionConfiguration(this.SessionIdentifier);
			}
			else
			{
				Console.WriteLine("Creating ephemeral session configuration.");
				config = NSUrlSessionConfiguration.EphemeralSessionConfiguration;
			}    


			// In our case we don't want any (NSURLCache-level) caching to get in the way 
			// of our tests, so we always disable the cache.
			config.RequestCachePolicy = NSUrlRequestCachePolicy.ReloadIgnoringCacheData;
			config.Discretionary = true;

			this.session = NSUrlSession.FromConfiguration(config, new FileUploadDelegate(this), NSOperationQueue.MainQueue);

			config.Dispose();

			// This is where things get wacky.  From the point that we create the session (in the previous 
			// line) to the point where the block passed to -getTasksWithCompletionHandler: runs, we can 
			// be getting delegate callbacks for tasks whose corresponding upload objects are in the wrong 
			// state (specifically, the task property isn't set and, in some cases, the state might be wrong). 
			// A lot of the logic in -syncUploadTasks: and, especially -uploadForTask:, is designed to 
			// compensate for that oddity.
			var activeTasks = await this.session.GetTasks2Async();
			var activeUploadTasks = activeTasks.UploadTasks;
			NSOperationQueue.MainQueue.AddOperation(() => {
				this.SyncUploadTasks(activeUploadTasks);
			});

			Console.WriteLine("FileUploadManager did start.");
		}

		public void Stop()
		{
			if (this.session == null)
			{
				return;
			}

			Console.WriteLine("FileUploadManager will stop.");

			foreach(var upload in this.ActiveUploads)
			{
				upload.Manager = null;
			}
			this.ActiveUploads.Clear();
			this.session.InvalidateAndCancel();
			this.session.Dispose();
			this.session = null;

			Console.WriteLine("FileUploadManager did stop.");
		}


		/// <summary>
		/// Helper to add an upload to the list of pending uploads.
		/// </summary>
		/// <param name="upload">Upload to add.</param>
		void AddUpload(FileUpload upload)
		{
			if(upload == null)
			{
				return;
			}
			Debug.Assert(!string.IsNullOrWhiteSpace(upload.UniqueId), "Added upload must have a unique ID!");
			this.ActiveUploads.Add(upload);
		}

		void SyncUploadTasks(NSUrlSessionTask[] uploadTasks)
		{
			
		}

		void RestoreAllUploadsInWorkDirectory()
		{}

		public FileUpload CreateFileUpload(NSUrlRequest request, string localFilename)
		{
			Debug.Assert (this.session != null, "Session is required to create upload task!");
			Debug.Assert(request != null, "Cannot create upload without request!");
			Debug.Assert(!string.IsNullOrWhiteSpace(localFilename), "Cannot upload non-existing file!");

			Console.WriteLine ($"Creating upload task for file '{localFilename}'.");

			var upload = new FileUpload(
				request: request,
				uniqueId: System.Guid.NewGuid().ToString(),
				localFilePath: localFilename,
				creationDate: DateTime.Now,
				manager: this);
			
			Debug.Assert (upload.State == FileUpload.STATE.Stopped || upload.State == FileUpload.STATE.Failed, "Invalid state of file upload object!");

			Console.WriteLine ($"Adding active upload with ID '{upload.UniqueId}'.");
			this.ActiveUploads.Add (upload);

			return upload;
		}

		internal void StartUpload(FileUpload upload)
		{
			upload.UploadTask = this.session.CreateUploadTask (upload.Request, NSUrl.FromFilename(upload.LocalFilePath));
			upload.Error = null;
			upload.State = FileUpload.STATE.Started;

			Debug.Assert (upload.IsStateValid(), "Invalid state of upload/upload task!");

			upload.UploadTask.Resume ();
		}

		internal void StopUpload(FileUpload upload)
		{
			Debug.Assert (this.session != null, "Session is required!");
			Debug.Assert (upload.State == FileUpload.STATE.Started, "Only started uploads can be stopped!");

			upload.State = FileUpload.STATE.Stopping;
			Debug.Assert (upload.IsStateValid (), "Invalid state after trying to stop upload!");
			upload.UploadTask.Cancel ();
		}

		internal void RemoveUpload(FileUpload upload, bool deleteFile)
		{
			Debug.Assert (this.session != null, "Session is required!");
			Debug.Assert (upload.State == FileUpload.STATE.Stopped, "Only stopped uploads can be removed!");

			var activeUpload = this.GetUploadByUniqueId (upload.UniqueId);
			Debug.Assert (activeUpload != null, "Requested upload not found in active uploads!");

			this.ActiveUploads.Remove (activeUpload);

			if (deleteFile)
			{
				try
				{
					File.Delete(upload.LocalFilePath);
				}
				catch(Exception ex)
				{
					Console.WriteLine ($"Failed to delete local file at '{upload.LocalFilePath}: {ex}");
				}
			}

		}

		public FileUpload GetUploadByUniqueId(string uniqueId)
		{
			var upload = this.ActiveUploads.FirstOrDefault (d => d.UniqueId == uniqueId);
			return upload;
		}

		public FileUpload GetUploadByTask(NSUrlSessionTask task)
		{
			var upload = this.ActiveUploads.FirstOrDefault (d => d.UniqueId == task.OriginalRequest.Headers["fileupload_unique_id"].ToString());
			return upload;
		}
	}
}

