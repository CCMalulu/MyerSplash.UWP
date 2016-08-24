﻿using MyerSplash.Common;
using MyerSplash.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace MyerSplash.View
{
    public sealed partial class AboutPage : BindablePage
    {
        private AboutViewModel AboutVM { get; set; }

        public AboutPage()
        {
            this.InitializeComponent();
            this.DataContext = AboutVM = new AboutViewModel();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            TitleBarHelper.SetUpThemeTitleBar();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            TitleBarHelper.SetUpDarkTitleBar();
        }
    }
}
