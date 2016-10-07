﻿using JP.Utils.Data.Json;
using System;
using System.Collections.ObjectModel;
using Windows.Data.Json;

namespace MyerSplash.Model
{
    public class UnsplashFeaturedImage : UnsplashImageBase
    {
        public UnsplashFeaturedImage()
        {

        }

        public static ObservableCollection<UnsplashFeaturedImage> ParseListFromJson(string json)
        {
            var list = new ObservableCollection<UnsplashFeaturedImage>();
            var array = JsonArray.Parse(json);
            foreach (var item in array)
            {
                var image = new UnsplashFeaturedImage();
                image.ParseObjectFromJsonString(item.ToString());
                list.Add(image);
            }
            return list;
        }

        public override void ParseObjectFromJsonString(string json)
        {
            var obj = JsonObject.Parse(json);
            ParseObjectFromJsonObject(obj);
        }

        public override void ParseObjectFromJsonObject(JsonObject obj)
        {
            var isFeatured = JsonParser.GetBooleanFromJsonObj(obj, "featured", false);

            var coverPhoto = JsonParser.GetJsonObjFromJsonObj(obj, "cover_photo");

            var urls = JsonParser.GetJsonObjFromJsonObj(coverPhoto, "urls");
            var smallImageUrl = JsonParser.GetStringFromJsonObj(urls, "small");
            var fullImageUrl = JsonParser.GetStringFromJsonObj(urls, "full");
            var regularImageUrl = JsonParser.GetStringFromJsonObj(urls, "regular");
            var thumbImageUrl = JsonParser.GetStringFromJsonObj(urls, "thumb");
            var rawImageUrl = JsonParser.GetStringFromJsonObj(urls, "raw");
            var color = JsonParser.GetStringFromJsonObj(coverPhoto, "color");
            var width = JsonParser.GetNumberFromJsonObj(coverPhoto, "width");
            var height = JsonParser.GetNumberFromJsonObj(coverPhoto, "height");
            var userObj = JsonParser.GetJsonObjFromJsonObj(coverPhoto, "user");
            var id = JsonParser.GetStringFromJsonObj(coverPhoto, "id");
            var likes = JsonParser.GetNumberFromJsonObj(coverPhoto, "likes");
            var time = JsonParser.GetStringFromJsonObj(coverPhoto, "created_at");
            var title = JsonParser.GetStringFromJsonObj(obj, "title");

            this.Title = title;
            this.SmallImageUrl = smallImageUrl;
            this.FullImageUrl = fullImageUrl;
            this.RegularImageUrl = regularImageUrl;
            this.ThumbImageUrl = thumbImageUrl;
            this.RawImageUrl = rawImageUrl;
            this.ColorValue = color;
            this.Width = width;
            this.Height = height;
            this.Owner = new UnsplashUser();
            this.Owner.ParseObjectFromJsonObject(userObj);
            this.ID = id;
            this.Likes = (int)likes;
            this.CreateTime = DateTime.Parse(time);
        }
    }
}
