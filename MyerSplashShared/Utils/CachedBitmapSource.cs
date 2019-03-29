﻿using JP.Utils.Data;
using JP.Utils.Debug;
using MyerSplashShared.Data;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace MyerSplashShared.Utils
{
    public class CachedBitmapSource : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private WeakReference<BitmapImage> _bitmapRef;
        [IgnoreDataMember]
        public WeakReference<BitmapImage> BitmapRef
        {
            get
            {
                return _bitmapRef;
            }
            private set
            {
                if (_bitmapRef != value)
                {
                    _bitmapRef = value;
                    RaisePropertyChanged(nameof(Bitmap));
                }
            }
        }

        public BitmapImage Bitmap
        {
            get
            {
                BitmapImage image = null;
                BitmapRef?.TryGetTarget(out image);
                return image;
            }
        }

        public string RemoteUrl { get; set; }

        public string LocalPath { get; set; }

        public string ExpectedFileName { get; set; }

        [IgnoreDataMember]
        public StorageFile File { get; set; }

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task LoadBitmapAsync(bool setBitmap = true)
        {
            if (!string.IsNullOrEmpty(LocalPath))
            {
                try
                {
                    var file = await StorageFile.GetFileFromPathAsync(LocalPath);
                    await SetImageSourceAsync(file as StorageFile);
                    return;
                }
                catch (FileNotFoundException)
                {
                }
            }
            await DownloadFromRemoteUrlAsync(setBitmap);
        }

        private async Task DownloadFromRemoteUrlAsync(bool setBitmap = true)
        {
            var cachedFolder = ApplicationData.Current.TemporaryFolder;

            if (!string.IsNullOrEmpty(ExpectedFileName))
            {
                var file = await cachedFolder.TryGetFileAsync(ExpectedFileName);
                if (file != null)
                {
                    LocalPath = file.Path;
                    File = file;
                    await SetImageSourceAsync(file);
                    return;
                }
            }
            else
            {
                ExpectedFileName = GenerateRandomFileName();
            }

            if (string.IsNullOrEmpty(RemoteUrl))
            {
                return;
            }

            using (var stream = await ImageDownloader.GetEncodedImageFromUrlAsync(this.RemoteUrl,
                CancellationTokenSourceFactory.CreateDefault().Create().Token))
            {
                var file = await SaveEncodedImageToFileAsync(stream.AsStreamForRead(), ExpectedFileName, cachedFolder);
                if (file != null)
                {
                    LocalPath = file.Path;
                    File = file;
                }
                if (stream != null && setBitmap)
                {
                    stream.Seek(0);
                    await SetImageSourceAsync(stream);
                }
            }
        }

        public async Task SetImageSourceAsync(IRandomAccessStream source)
        {
            var bitmap = new BitmapImage();
            await bitmap.SetSourceAsync(source);
            BitmapRef = new WeakReference<BitmapImage>(bitmap);
        }

        public async Task SetImageSourceAsync(StorageFile file)
        {
            using (var fs = await file.OpenAsync(FileAccessMode.Read))
            {
                await SetImageSourceAsync(fs);
            }
        }

        private string GenerateRandomFileName()
        {
            return DateTime.Now.ToFileTime().ToString() + ".jpg";
        }

        private async Task<StorageFile> SaveEncodedImageToFileAsync(Stream stream, string expectedFileName,
            StorageFolder destinationFolder)
        {
            try
            {
                var file = await destinationFolder.CreateFileAsync(expectedFileName, CreationCollisionOption.ReplaceExisting);
                using (var fileStream = await file.OpenStreamForWriteAsync())
                {
                    await stream.AsInputStream().AsStreamForRead().CopyToAsync(fileStream);
                }
                return file;
            }
            catch (Exception e)
            {
                var task = Logger.LogAsync(e);
                return null;
            }
        }
    }
}