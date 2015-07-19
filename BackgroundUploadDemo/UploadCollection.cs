using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace BackgroundUploadDemo
{
	public class UploadCollection : ObservableCollection<FileUpload>
	{
		public UploadCollection () : base()
		{
		}

		public void OnCollectionChanged()
		{
			base.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}
	}
}

