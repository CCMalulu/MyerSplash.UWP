﻿using MyerSplash.ViewModel;
using MyerSplashShared.Utils;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MyerSplash.View.Uc
{
    public sealed partial class DrawerControl : UserControl
    {
        private MainViewModel MainVM
        {
            get
            {
                return this.DataContext as MainViewModel;
            }
        }

        public DrawerControl()
        {
            this.InitializeComponent();

            FullscreenBtn.Visibility = Visibility.Visible;
            Window.Current.SizeChanged += Current_SizeChanged;

            if (DeviceUtil.IsXbox)
            {
                FullscreenBtn.Visibility = Visibility.Collapsed;
            }
        }

        private void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            if (!ApplicationView.GetForCurrentView().IsFullScreenMode)
            {
                FullscreenIcon.Symbol = Symbol.FullScreen;
            }
            else
            {
                FullscreenIcon.Symbol = Symbol.BackToWindow;
            }
        }
    }
}