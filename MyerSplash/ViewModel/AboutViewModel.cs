﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using JP.Utils.Debug;
using JP.Utils.Helper;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Email;
using Windows.ApplicationModel.Resources;
using Windows.System;

namespace MyerSplash.ViewModel
{
    public class AboutViewModel : ViewModelBase
    {
        private string _version;
        public string Version
        {
            get => _version;
            set
            {
                if (_version != value)
                {
                    _version = value;
                    RaisePropertyChanged(() => Version);
                }
            }
        }

        private RelayCommand _visitGitHubCommand;
        public RelayCommand VisitGitHubCommand
        {
            get
            {
                if (_visitGitHubCommand != null) return _visitGitHubCommand;
                return _visitGitHubCommand = new RelayCommand(async () =>
                  {
                      await Launcher.LaunchUriAsync(new Uri("https://github.com/JuniperPhoton/MyerSplashUWP"));
                  });
            }
        }

        private RelayCommand _visitTwitterCommand;
        public RelayCommand VisitTwitterCommand
        {
            get
            {
                if (_visitTwitterCommand != null) return _visitTwitterCommand;
                return _visitTwitterCommand = new RelayCommand(async () =>
                  {
                      await Launcher.LaunchUriAsync(new Uri("https://twitter.com/JuniperPhoton"));
                  });
            }
        }

        private RelayCommand _visitWeiboCommand;
        public RelayCommand VisitWeiboCommand
        {
            get
            {
                if (_visitWeiboCommand != null) return _visitWeiboCommand;
                return _visitWeiboCommand = new RelayCommand(async () =>
                {
                    await Launcher.LaunchUriAsync(new Uri("http://weibo.com/photon/"));
                });
            }
        }

        private RelayCommand _feedbackCommand;
        public RelayCommand FeedbackCommand
        {
            get
            {
                if (_feedbackCommand != null) return _feedbackCommand;
                return _feedbackCommand = new RelayCommand(async () =>
                  {
                      var rec = new EmailRecipient("dengweichao@hotmail.com");
                      var mes = new EmailMessage();
                      mes.To.Add(rec);
                      var attach = await Logger.GetLogFileAttachementAsync();
                      if (attach != null)
                      {
                          mes.Attachments.Add(attach);
                      }
                      var platform = DeviceHelper.IsDesktop ? "PC" : "Mobile";

                      mes.Subject = $"MyerSplash for Windows 10 {platform}, {Version} feedback, {DeviceHelper.OSVersion}";
                      mes.Body = ResourceLoader.GetForCurrentView().GetString("EmailBody");

                      await EmailManager.ShowComposeNewEmailAsync(mes);
                  });
            }
        }

        private RelayCommand _rateCommand;
        public RelayCommand RateCommand
        {
            get
            {
                if (_rateCommand != null) return _rateCommand;
                return _rateCommand = new RelayCommand(async () =>
                  {
                      await Launcher.LaunchUriAsync(new Uri("ms-windows-store://review/?PFN=" + Package.Current.Id.FamilyName));
                  });
            }
        }

        public AboutViewModel()
        {
            Version = App.GetAppVersion();
        }
    }
}