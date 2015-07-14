using System;
using Foundation;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;

namespace BackgroundUploadDemo
{
	public class FileUploadManager
	{
		public FileUploadManager (string baseDir)
		{
			this.baseDir = baseDir;

			if(string.IsNullOrWhiteSpace(baseDir))
			{
				this.baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "..", "Library", "Caches", "uploads");
				if(!Directory.Exists(this.baseDir))
				{
					Directory.CreateDirectory(this.baseDir);
				}
			}
			Console.WriteLine($"Uploading will happen from folder: '{baseDir}'");
		}

		string baseDir;
		NSUrlSession session;

		// maps from unique ID to upload object
		Dictionary<string, FileUpload> activeUploads = new Dictionary<string, FileUpload>();

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

			this.session = NSUrlSession.FromConfiguration(config, new FileUploadDelegate(), NSOperationQueue.MainQueue);

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

			Console.WriteLine($"nameof(FileUploadManager) did start.");
		}

		public void Stop()
		{
			if (this.session == null)
			{
				return;
			}

			Console.WriteLine($"nameof(FileUploadManager) will stop.");

			foreach(var upload in this.activeUploads.Values)
			{
				upload.Manager = null;
			}
			this.activeUploads.Clear();
			this.session.InvalidateAndCancel();
			this.session.Dispose();
			this.session = null;

			Console.WriteLine($"nameof(FileUploadManager) did stop.");
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
			this.activeUploads.Add(upload.UniqueId, upload);
		}

		public void CreateUpload(NSUrlRequest request, string localFilename)
		{
			Debug.Assert(request != null, "Cannot create upload without request!");
			Debug.Assert(!string.IsNullOrWhiteSpace(localFilename), "Cannot upload non-existing file!");

			var upload = new FileUpload(
				request: request,
				uniqueId: System.Guid.NewGuid().ToString(),
				localFilePath: Path.Combine(this.baseDir, localFilename),
				creationDate: DateTime.Now,
				manager: this);
		}


		/*
		 * 
		- (FileUpload *)createUploadWithRequest:(NSURLRequest *)request fileURL:(NSURL *)fileURL
{
    NSUUID *        uploadUUID;
    NSURL *         uploadDirURL;
    NSDate *        creationDate;
    FileUpload *    upload;
    
    NSParameterAssert(fileURL != nil);
    NSParameterAssert(request != nil);
    NSParameterAssert(self.session != nil);
    
    upload = nil;
    
    uploadUUID = [NSUUID UUID];
    creationDate = [NSDate date];
    
    // Create a upload directory containing our immutable info, including a hard link 
    // to the file to upload.
    
    uploadDirURL = [self createImmutableInfoForOriginalURL:fileURL request:request uploadUUID:uploadUUID creationDate:creationDate];
    
    // Create a upload object to match.

    if (uploadDirURL != nil) {

        upload = [[FileUpload alloc] initWithRequest:request uploadUUID:uploadUUID uploadDirURL:uploadDirURL originalURL:fileURL creationDate:creationDate manager:self];

        // Add it to our uploads dictionary (and hence to the public uploads set).
        
        [self addUpload:upload];

        [self logWithFormat:@"did create %@ for URL %@", upload, [request URL]];
    }
    
    return upload;
}
*/




		void SyncUploadTasks(NSUrlSessionTask[] uploadTasks)
		{
			
		}

		/*

		- (BOOL)restoreUploadFromUploadDirectoryURL:(NSURL *)uploadDirURL
		{
			BOOL            success;
			NSUUID *        uploadUUID;
			NSDictionary *  immutableInfo;
			NSData *        requestData;
			NSURLRequest *  request;
			NSData *        originalURLData;
			NSURL *         originalURL;
			NSNumber *      creationDateNum;
			NSDate *        creationDate;

			// First try to restore the immutable info.

			uploadUUID = [[NSUUID alloc] initWithUUIDString:[[uploadDirURL lastPathComponent] substringFromIndex:[kUploadDirectoryPrefix length]]];
			success = (uploadUUID != nil);
			if (success) {
				immutableInfo = [[NSDictionary alloc] initWithContentsOfURL:[uploadDirURL URLByAppendingPathComponent:kImmutableInfoFileName]];
				success = (immutableInfo != nil);
			}
			if (success) {
				requestData = [immutableInfo objectForKey:kImmutableInfoRequestDataKey];
				success = [requestData isKindOfClass:[NSData class]];
			}
			if (success) {
				request = [NSKeyedUnarchiver unarchiveObjectWithData:requestData];
				success = [request isKindOfClass:[NSURLRequest class]];
			}
			if (success) {
				originalURLData = [immutableInfo objectForKey:kImmutableInfoOriginalURLDataKey];
				success = [originalURLData isKindOfClass:[NSData class]];
			}
			if (success) {
				originalURL = [NSKeyedUnarchiver unarchiveObjectWithData:originalURLData];
				success = [originalURL isKindOfClass:[NSURL class]];
			}
			if (success) {
				creationDateNum = [immutableInfo objectForKey:kImmutableInfoCreationDateNumKey];
				success = [creationDateNum isKindOfClass:[NSNumber class]];
			}
			if (success) {
				creationDate = [NSDate dateWithTimeIntervalSinceReferenceDate:[creationDateNum doubleValue]];
			}

			// Then restore the mutable info.  From here on we can't fail, in that if the mutable info 
			// is bogus we just start the upload from scratch.

			if (success) {
				FileUpload *        upload;
				NSString *          mutableInfoLogStr;

				upload = [[FileUpload alloc] initWithRequest:request uploadUUID:uploadUUID uploadDirURL:uploadDirURL originalURL:originalURL creationDate:creationDate manager:self];

				// Try to restore the mutable state.  If that fails, re-create the upload 
				// and let's start from scratch based on the immutable state.

				success = [self restoreMutableInfoForFileUpload:upload];
				if (success) {
					mutableInfoLogStr = @" (including mutable info)";
				} else {
					mutableInfoLogStr = @" (without mutable info)";
					upload = [[FileUpload alloc] initWithRequest:request uploadUUID:uploadUUID uploadDirURL:uploadDirURL originalURL:originalURL creationDate:creationDate manager:self];
					success = YES;
				}

				// Add it to our uploads dictionary (and hence to the public uploads set).

				[self addUpload:upload];

				[self logWithFormat:@"did restore %@%@ for %@ from %@", upload, mutableInfoLogStr, [request URL], uploadDirURL];
			} else {
				[self logWithFormat:@"did not restore from %@", uploadDirURL];
			}

			return success;
		}

		- (void)restoreAllUploadsInWorkDirectory
		{
			for (NSURL * itemURL in [[NSFileManager defaultManager] contentsOfDirectoryAtURL:self.workDirectoryURL includingPropertiesForKeys:@[NSURLIsDirectoryKey] options:NSDirectoryEnumerationSkipsSubdirectoryDescendants error:NULL]) {
				BOOL            success;
				NSError *       error;
				NSNumber *      isDirectory;

				isDirectory = nil;

				success = [itemURL getResourceValue:&isDirectory forKey:NSURLIsDirectoryKey error:&error];
				assert(success);

				if ( [isDirectory boolValue] && [[itemURL lastPathComponent] hasPrefix:kUploadDirectoryPrefix] ) {

					success = [self restoreUploadFromUploadDirectoryURL:itemURL];

					// The above only returns NO if the upload directory is completely bogus.  In that case, 
					// we delete it so that it doesn't trouble us again in the future.

					if ( ! success ) {
						success = [[NSFileManager defaultManager] removeItemAtURL:itemURL error:&error];
						assert(success);
					}
				}
			}
		}
*/

		void RestoreAllUploadsInWorkDirectory()
		{}

		public void StartUpload(FileUpload upload)
		{}

		public void StopUpload(FileUpload upload)
		{}

		public void RemoveUpload(FileUpload upload)
		{}
	}
}

