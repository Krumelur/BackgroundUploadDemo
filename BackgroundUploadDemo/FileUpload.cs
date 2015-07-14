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

		public FileUpload (NSUrlRequest request, string uniqueId, string localFilePath, DateTime creationDate, FileUploadManager manager)
		{
			this.request = request;
			this.UniqueId = uniqueId;
			this.creationDate = creationDate;
			this.Manager = manager;
			this.progress = 0f;
			this.localFilePath = localFilePath;
		}

		NSUrlRequest request;

		DateTime creationDate;
		NSUrlSessionUploadTask uploadTask;
		float progress;
		NSHttpUrlResponse response;
		NSError error;
		string localFilePath;

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

			result = this.request != null;

			if (result)
			{
				result = !string.IsNullOrWhiteSpace(this.UniqueId);
			}
			if (result)
			{
				result = !string.IsNullOrWhiteSpace (this.localFilePath);
			}
			if (result)
			{
				result = this.localUrl != null;
			}
			if (result)
			{
				result = this.Manager != null;
			}
			if (result)
			{
				result = this.creationDate != DateTime.MinValue && this.creationDate != DateTime.MaxValue;
			}
			if (result)
			{
				result = this.progress >= 0f && this.progress <= 1f;
			}
			if (result)
			{
				result = this.response != null && this.State == STATE.Uploaded;
			}
			if (result)
			{
				result = this.error != null && this.State == STATE.Failed;
			}
			if (result && includeTask)
			{
				result = this.uploadTask != null && (this.State == STATE.Started || this.State == STATE.Stopping);
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

		public void Remove()
		{
			Debug.Assert(this.IsStateValid(), "Current state is not valid when removing upload!");
			this.Manager.RemoveUpload(this);
			this.Manager = null;
		}
	}
}

