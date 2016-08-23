﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using JP.Utils.Data;
using JP.Utils.Framework;
using MyerSplash.Common;
using MyerSplash.LiveTile;
using MyerSplash.Model;
using MyerSplash.View;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using JP.Utils.UI;
using MyerSplashShared.API;
using System.Linq;
using System.Diagnostics;
using MyerSplashCustomControl;

namespace MyerSplash.ViewModel
{
    public class MainViewModel : ViewModelBase, INavigable
    {
        private const string FEATURED_CATEGORY_NAME = "FEATURED";
        private const string NEW_CATEGORY_NAME = "NEW";

        private ImageDataViewModel _mainDataVM;
        public ImageDataViewModel MainDataVM
        {
            get
            {
                return _mainDataVM;
            }
            set
            {
                if (_mainDataVM != value)
                {
                    _mainDataVM = value;
                    RaisePropertyChanged(() => MainDataVM);
                }
            }
        }

        private ObservableCollection<UnsplashImageBase> _mainList;
        public ObservableCollection<UnsplashImageBase> MainList
        {
            get
            {
                return _mainList;
            }
            set
            {
                if (_mainList != value)
                {
                    _mainList = value;
                    RaisePropertyChanged(() => MainList);
                }
            }
        }

        private ObservableCollection<UnsplashCategory> _categories;
        public ObservableCollection<UnsplashCategory> Categories
        {
            get
            {
                return _categories;
            }
            set
            {
                if (_categories != value)
                {
                    _categories = value;
                    RaisePropertyChanged(() => Categories);
                }
            }
        }

        public bool IsInView { get; set; }

        public bool IsFirstActived { get; set; } = true;

        private RelayCommand _searchCommand;
        public RelayCommand SearchCommand
        {
            get
            {
                if (_searchCommand != null) return _searchCommand;
                return _searchCommand = new RelayCommand(() =>
                  {
                      ToastService.SendToast("Still working on this.");
                  });
            }
        }


        private RelayCommand _refreshCommand;
        public RelayCommand RefreshCommand
        {
            get
            {
                if (_refreshCommand != null) return _refreshCommand;
                return _refreshCommand = new RelayCommand(async () =>
                  {
                      await RefreshAllAsync();
                  });
            }
        }

        private RelayCommand _retryCommand;
        public RelayCommand RetryCommand
        {
            get
            {
                if (_retryCommand != null) return _retryCommand;
                return _retryCommand = new RelayCommand(async () =>
                  {
                      ShowFooterLoading = Visibility.Visible;
                      ShowFooterReloadGrid = Visibility.Collapsed;
                      await MainDataVM.RetryAsync();
                  });
            }
        }

        private RelayCommand _openDrawerCommand;
        public RelayCommand OpenDrawerCommand
        {
            get
            {
                if (_openDrawerCommand != null) return _openDrawerCommand;
                return _openDrawerCommand = new RelayCommand(() =>
                  {
                      DrawerOpened = !DrawerOpened;
                      if (DrawerOpened)
                      {
                          NavigationService.HistoryOperationsBeyondFrame.Push(() =>
                          {
                              if (DrawerOpened)
                              {
                                  DrawerOpened = false;
                                  return true;
                              }
                              else return false;
                          });
                      }
                  });
            }
        }

        private bool _drawerOpened;
        public bool DrawerOpened
        {
            get
            {
                return _drawerOpened;
            }
            set
            {
                if (_drawerOpened != value)
                {
                    _drawerOpened = value;
                    RaisePropertyChanged(() => DrawerOpened);
                }
            }
        }

        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get
            {
                return _isRefreshing;
            }
            set
            {
                if (_isRefreshing != value)
                {
                    _isRefreshing = value;
                    RaisePropertyChanged(() => IsRefreshing);
                }
            }
        }

        private Visibility _showFooterLoading;
        public Visibility ShowFooterLoading
        {
            get
            {
                return _showFooterLoading;
            }
            set
            {
                if (_showFooterLoading != value)
                {
                    _showFooterLoading = value;
                    RaisePropertyChanged(() => ShowFooterLoading);
                }
            }
        }

        private Visibility _showNoItemHint;
        public Visibility ShowNoItemHint
        {
            get
            {
                return _showNoItemHint;
            }
            set
            {
                if (_showNoItemHint != value)
                {
                    _showNoItemHint = value;
                    RaisePropertyChanged(() => ShowNoItemHint);
                }
            }
        }

        private Visibility _showFooterReloadGrid;
        public Visibility ShowFooterReloadGrid
        {
            get
            {
                return _showFooterReloadGrid;
            }
            set
            {
                if (_showFooterReloadGrid != value)
                {
                    _showFooterReloadGrid = value;
                    RaisePropertyChanged(() => ShowFooterReloadGrid);
                }
            }
        }

        private RelayCommand _goToSettingsCommand;
        public RelayCommand GoToSettingsCommand
        {
            get
            {
                if (_goToSettingsCommand != null) return _goToSettingsCommand;
                return _goToSettingsCommand = new RelayCommand(async () =>
                  {
                      DrawerOpened = false;
                      await NavigationService.NaivgateToPageAsync(typeof(SettingsPage));
                  });
            }
        }

        private RelayCommand _goToAboutCommand;
        public RelayCommand GoToAboutCommand
        {
            get
            {
                if (_goToAboutCommand != null) return _goToAboutCommand;
                return _goToAboutCommand = new RelayCommand(async () =>
                  {
                      DrawerOpened = false;
                      await NavigationService.NaivgateToPageAsync(typeof(AboutPage));
                  });
            }
        }

        private int _selectedIndex;
        public int SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }
            set
            {
                if (_selectedIndex != value)
                {
                    _selectedIndex = value;
                    RaisePropertyChanged(() => SelectedIndex);
                    RaisePropertyChanged(() => SelectedTitle);
                    DrawerOpened = false;
                    if (value == 0)
                    {
                        MainDataVM = new ImageDataViewModel(this, UrlHelper.GetNewImages, false);
                    }
                    else if (value == 1)
                    {
                        MainDataVM = new ImageDataViewModel(this, UrlHelper.GetFeaturedImages, true);
                    }
                    else if (Categories?.Count > 0)
                    {
                        MainDataVM = new ImageDataViewModel(this, Categories[value].RequestUrl, false);
                    }
                    if (MainDataVM != null)
                    {
                        var task = RefreshListAsync();
                    }
                }
            }
        }

        public string SelectedTitle
        {
            get
            {
                if (Categories?.Count > 0)
                {
                    return Categories[SelectedIndex].Title.ToUpper();
                }
                else return FEATURED_CATEGORY_NAME;
            }
        }

        public MainViewModel()
        {
            MainList = new ObservableCollection<UnsplashImageBase>();

            ShowFooterLoading = Visibility.Collapsed;
            ShowNoItemHint = Visibility.Collapsed;
            ShowFooterReloadGrid = Visibility.Collapsed;

            App.MainVM = this;

            SelectedIndex = -1;
        }

        private async Task RestoreMainListDataAsync()
        {
            var file = await CacheUtil.GetCachedFileFolder().TryGetFileAsync(CachedFileNames.MainListFileName);
            if (file != null)
            {
                var list = await SerializerHelper.DeserializeFromJsonByFile<IncrementalLoadingCollection<UnsplashImage>>(CachedFileNames.MainListFileName, CacheUtil.GetCachedFileFolder());
                if (list != null)
                {
                    this.MainDataVM = new ImageDataViewModel(this, UrlHelper.GetFeaturedImages, true);
                    list.ToList().ForEach(s => MainDataVM.DataList.Add(s));

                    for (int i = 0; i < MainDataVM.DataList.Count; i++)
                    {
                        var item = MainDataVM.DataList[i];
                        if (i % 2 == 0) item.BackColor = App.Current.Resources["ImageBackBrush1"] as SolidColorBrush;
                        else item.BackColor = App.Current.Resources["ImageBackBrush2"] as SolidColorBrush;
                        var task = item.RestoreDataAsync();
                    }
                    await UpdateLiveTileAsync();
                }
                else MainDataVM = new ImageDataViewModel(this, UrlHelper.GetFeaturedImages, true);
            }
            else MainDataVM = new ImageDataViewModel(this, UrlHelper.GetFeaturedImages, true);
        }

        private async Task RefreshAllAsync()
        {
            var task1 = GetCategoriesAsync();
            await RefreshListAsync();
            await SaveMainListDataAsync();
            await UpdateLiveTileAsync();
        }

        private async Task RefreshListAsync()
        {
            MainDataVM.MainVM = this;
            IsRefreshing = true;
            await MainDataVM.RefreshAsync();
            IsRefreshing = false;
            MainList = MainDataVM.DataList;
        }

        private void UpdateNoItemHint()
        {
            if (MainList?.Count > 0) ShowNoItemHint = Visibility.Collapsed;
            else ShowNoItemHint = Visibility.Visible;
        }

        private async Task GetCategoriesAsync()
        {
            if (Categories?.Count > 0) return;

            var result = await CloudService.GetCategories(CTSFactory.MakeCTS(20000).Token);
            if (result.IsRequestSuccessful)
            {
                var list = UnsplashCategory.GenerateListFromJson(result.JsonSrc);
                this.Categories = list;
                this.Categories.Insert(0, new UnsplashCategory()
                {
                    Title = "Featured",
                });
                this.Categories.Insert(0, new UnsplashCategory()
                {
                    Title = "New",
                });
                SelectedIndex = 1;
            }
        }

        private async Task SaveMainListDataAsync()
        {
            if (this.MainDataVM.DataList?.Count > 0)
            {
                await SerializerHelper.SerializerToJson<IncrementalLoadingCollection<UnsplashImage>>(this.MainDataVM.DataList, CachedFileNames.MainListFileName, CacheUtil.GetCachedFileFolder());
                //if (MainList?.ToList().FirstOrDefault()?.ID != MainDataVM?.DataList?.FirstOrDefault()?.ID && SelectedIndex == 0)
                //{
                //    MainList = MainDataVM.DataList;
                //}
            }
        }
        private async Task UpdateLiveTileAsync()
        {
            var list = new List<string>();

            if (MainList == null) return;

            foreach (var item in MainList)
            {
                list.Add(item.ListImageBitmap.LocalPath);
            }
            if (App.AppSettings.EnableTile && list.Count > 0)
            {
                Debug.WriteLine("About to update tile.");
                await LiveTileUpdater.UpdateImagesTileAsync(list);
            }
        }

        public void Activate(object param)
        {

        }

        public void Deactivate(object param)
        {

        }

        public async void OnLoaded()
        {
            if (IsFirstActived)
            {
                IsFirstActived = false;
                await RestoreMainListDataAsync();
                await RefreshAllAsync();
            }
        }
    }
}
