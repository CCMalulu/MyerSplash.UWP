﻿using GalaSoft.MvvmLight;
using JP.API;
using JP.Utils.Data.Json;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.UI.Xaml.Media.Imaging;
using System;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using JP.Utils.UI;
using Windows.UI;

namespace MyerSplash.Model
{
    public class UnsplashImage : ViewModelBase
    {
        public string ID { get; set; }

        public string RawImageUrl { get; set; }

        public string FullImageUrl { get; set; }

        public string RegularImageUrl { get; set; }

        public string SmallImageUrl { get; set; }

        public string ThumbImageUrl { get; set; }

        private BitmapImage _listImageBitmap;
        public BitmapImage ListImageBitmap
        {
            get
            {
                return _listImageBitmap;
            }
            set
            {
                if (_listImageBitmap != value)
                {
                    _listImageBitmap = value;
                    RaisePropertyChanged(() => ListImageBitmap);
                }
            }
        }

        public string ListImageCachedFilePath { get; set; }

        private BitmapImage _largeBitmap;
        public BitmapImage LargeBitmap
        {
            get
            {
                return _largeBitmap;
            }
            set
            {
                if (_largeBitmap != value)
                {
                    _largeBitmap = value;
                    RaisePropertyChanged(() => LargeBitmap);
                }
            }
        }

        private SolidColorBrush _majorColor;
        public SolidColorBrush MajorColor
        {
            get
            {
                return _majorColor;
            }
            set
            {
                if (_majorColor != value)
                {
                    _majorColor = value;
                    RaisePropertyChanged(() => MajorColor);
                }
            }
        }

        private SolidColorBrush _backColor;
        public SolidColorBrush BackColor
        {
            get
            {
                return _backColor;
            }
            set
            {
                if (_backColor != value)
                {
                    _backColor = value;
                    RaisePropertyChanged(() => BackColor);
                }
            }
        }

        public string ColorValue { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }

        private UnsplashUser _owner;
        public UnsplashUser Owner
        {
            get
            {
                return _owner;
            }
            set
            {
                if (_owner != value)
                {
                    _owner = value;
                    RaisePropertyChanged(() => Owner);
                }
            }
        }

        public UnsplashImage()
        {
            
        }

        public async Task DownloadImgForList()
        {
            var url = GetListImageUrlFromSettings();
            if (string.IsNullOrEmpty(url)) return;
            var file = await App.CacheUtilInstance.DownloadImageAsync(url);
            ListImageCachedFilePath = file.Path;
            using (var stream = await file.OpenAsync(FileAccessMode.Read))
            {
                ListImageBitmap = new BitmapImage();
                await ListImageBitmap.SetSourceAsync(stream);
            }
        }

        public string GetListImageUrlFromSettings()
        {
            var quality = App.AppSettings.LoadQuality;
            switch(quality)
            {
                case 0: return RegularImageUrl;
                case 1:return SmallImageUrl;
                case 2:return ThumbImageUrl;
                default:return "";
            }
        }

        public string GetSaveImageUrlFromSettings()
        {
            var quality = App.AppSettings.SaveQuality;
            switch (quality)
            {
                case 0: return RawImageUrl;
                case 1: return FullImageUrl;
                case 2: return RegularImageUrl;
                default: return "";
            }
        }

        public async Task DownloadFullImage()
        {
            var url = GetSaveImageUrlFromSettings();
            if (string.IsNullOrEmpty(url)) return;
            var folder =await KnownFolders.PicturesLibrary.CreateFolderAsync("MyerSplash", CreationCollisionOption.OpenIfExists);
            using (var stream = await APIHelper.GetIRandomAccessStreamFromUrlAsync(url))
            {
                var newFile = await folder.CreateFileAsync($"{ID}.jpg", CreationCollisionOption.GenerateUniqueName);
                var bytes = new byte[stream.AsStream().Length];
                await stream.AsStream().ReadAsync(bytes, 0, (int)stream.AsStream().Length);
                await FileIO.WriteBytesAsync(newFile, bytes);
            }
        }

        public static ObservableCollection<UnsplashImage> ParseListFromJson(string json)
        {
            var list = new ObservableCollection<UnsplashImage>();
            var array = JsonArray.Parse(json);
            foreach (var item in array)
            {
                var unsplashImage = ParseObjectFromJson(item.ToString());
                list.Add(unsplashImage);
            }
            return list;
        }

        private static UnsplashImage ParseObjectFromJson(string json)
        {
            var obj = JsonObject.Parse(json);
            var urls = JsonParser.GetJsonObjFromJsonObj(obj, "urls");
            var smallImageUrl = JsonParser.GetStringFromJsonObj(urls, "small");
            var fullImageUrl = JsonParser.GetStringFromJsonObj(urls, "full");
            var regularImageUrl = JsonParser.GetStringFromJsonObj(urls, "regular");
            var thumbImageUrl = JsonParser.GetStringFromJsonObj(urls, "thumb");
            var rawImageUrl = JsonParser.GetStringFromJsonObj(urls, "raw");
            var color = JsonParser.GetStringFromJsonObj(obj, "color");
            var width = JsonParser.GetNumberFromJsonObj(obj, "width");
            var height = JsonParser.GetNumberFromJsonObj(obj, "height");
            var userObj = JsonParser.GetJsonObjFromJsonObj(obj, "user");
            var userName = JsonParser.GetStringFromJsonObj(userObj, "name");
            var id = JsonParser.GetStringFromJsonObj(obj, "id");

            var img = new UnsplashImage();
            img.SmallImageUrl = smallImageUrl;
            img.FullImageUrl = fullImageUrl;
            img.RegularImageUrl = regularImageUrl;
            img.ThumbImageUrl = thumbImageUrl;
            img.RawImageUrl = rawImageUrl;
            img.ColorValue = color;
            img.Width = width;
            img.Height = height;
            img.Owner = new UnsplashUser() { Name = userName };
            img.ID = id;

            return img;
        }
    }
}