using System;
using Foundation;
using System.Diagnostics;

namespace BackgroundUploadDemo
{
	public class FileUpload
	{
		public enum STATE
		{
			Started,
			Stopping,
			Stopped,
			Uploaded,
			Failed
		}

		internal FileUpload (NSUrlRequest request, string uniqueId, string localFilePath, DateTime creationDate, FileUploadManager manager)
		{
			var requestCopy = (NSMutableUrlRequest)request.MutableCopy();

			// Add the unique ID to the headers of the request. When our upload delegate gets called,
			// we can acccess the request and from that get back to the FileUpload object.
			var existingHeaders = NSMutableDictionary.FromDictionary (request.Headers);
			existingHeaders.Add ((NSString)"fileupload_unique_id", (NSString)uniqueId);
			requestCopy.Headers = existingHeaders;

			this.Request = requestCopy;
			this.UniqueId = uniqueId;
			this.CreationDate = creationDate;
			this.Manager = manager;
			this.progress = 0f;
			this.LocalFilePath = localFilePath;
		}

		public override int GetHashCode ()
		{
			return this.UniqueId.GetHashCode ();
		}

		public override bool Equals (object obj)
		{
			if (obj == null)
			{
				return false;
			}

			return ((FileUpload)obj).UniqueId == this.UniqueId;
		}

		public NSUrlRequest Request
		{
			get;
		}

		public string LocalFilePath
		{
			get;
		}

		public DateTime CreationDate
		{
			get;
		}

		public NSUrlSessionResponse Response
		{
			get;
			set;
		}

		public NSError Error
		{
			get;
			set;
		}

		public NSUrlSessionUploadTask UploadTask
		{
			get;
			set;
		}

		float progress;


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

		public string UniqueId
		{
			get;
			private set;
		}

		public STATE State
		{
			get;
			set;
		}



		public bool IsStateValid(bool includeTask = true)
		{
			bool result;

			result = this.Request != null;

			if (result)
			{
				result = !string.IsNullOrWhiteSpace(this.UniqueId);
			}
			if (result)
			{
				result = !string.IsNullOrWhiteSpace (this.LocalFilePath);
			}
			if (result)
			{
				result = this.Manager != null;
			}
			if (result)
			{
				result = this.CreationDate != DateTime.MinValue && this.CreationDate != DateTime.MaxValue;
			}
			if (result)
			{
				result = this.progress >= 0f && this.progress <= 1f;
			}
			if (result)
			{
				result = this.Response != null && this.State == STATE.Uploaded;
			}
			if (result)
			{
				result = this.Error != null && this.State == STATE.Failed;
			}
			if (result && includeTask)
			{
				result = this.UploadTask != null && (this.State == STATE.Started || this.State == STATE.Stopping);
			}
			return result;
		}


		public void Start()
		{
			Debug.Assert(this.IsStateValid(), "Current state is not valid when starting upload!");
			this.Manager.StartUpload(this);
		}

		public void Stop()
		{
			Debug.Assert(this.IsStateValid(), "Current state is not valid when stopping upload!");
			this.Manager.StopUpload(this);
		}

		public void Remove(bool deleteFile)
		{
			Debug.Assert(this.IsStateValid(), "Current state is not valid when removing upload!");
			this.Manager.RemoveUpload(this, deleteFile);
			this.Manager = null;
		}
	}
}

