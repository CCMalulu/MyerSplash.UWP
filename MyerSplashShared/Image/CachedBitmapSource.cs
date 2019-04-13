﻿using JP.Utils.Debug;
using MyerSplashShared.Data;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace MyerSplashShared.Image
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

        private readonly DiskCacheSupplier _cacheSupplier = DiskCacheSupplier.Instance;
        private readonly ICacheKeyFactory _cacheKeyFactory = CacheKeyFactory.GetDefault();

        private CancellationTokenSource _cancelTokenSource;

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void CancelDownload()
        {
            if (_cancelTokenSource != null)
            {
                Debug.WriteLine("======Cancel download======");
                _cancelTokenSource.Cancel();
            }
        }

        public async Task LoadBitmapAsync(bool setBitmap = true)
        {
            if (string.IsNullOrEmpty(RemoteUrl))
            {
                return;
            }

            var cacheKey = _cacheKeyFactory.ProvideKey(RemoteUrl);
            var file = await _cacheSupplier.TryGetCacheAsync(cacheKey);
            if (file != null)
            {
                await SetImageSourceAsync(file);
                return;
            }

            _cancelTokenSource = CancellationTokenSourceFactory.CreateDefault(ImageDownloader.TIMEOUT_MILLIS).Create();

            using (var stream = await ImageDownloader.GetEncodedImageFromUrlAsync(RemoteUrl, _cancelTokenSource.Token))
            {
                _cancelTokenSource.Dispose();
                _cancelTokenSource = null;

                var savedFile = await SaveEncodedImageToFileAsync(stream.AsStreamForRead());
                if (stream != null && setBitmap)
                {
                    stream.Seek(0);
                    await SetImageSourceAsync(stream);
                }
            }
        }

        private async Task SetImageSourceAsync(IRandomAccessStream source)
        {
            var bitmap = new BitmapImage();
            await bitmap.SetSourceAsync(source);
            BitmapRef = new WeakReference<BitmapImage>(bitmap);
        }

        private async Task SetImageSourceAsync(StorageFile file)
        {
            using (var fs = await file.OpenAsync(FileAccessMode.Read))
            {
                await SetImageSourceAsync(fs);
            }
        }

        private async Task<StorageFile> SaveEncodedImageToFileAsync(Stream stream)
        {
            try
            {
                var cacheKey = _cacheKeyFactory.ProvideKey(RemoteUrl);

                var file = await _cacheSupplier.GetFileToSaveAsync(cacheKey);
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