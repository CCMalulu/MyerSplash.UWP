﻿using JP.Utils.UI;
using MyerSplash.Model;
using MyerSplashCustomControl;
using System;
using System.Numerics;
using System.Threading;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace MyerSplash.UC
{
    public sealed partial class PhotoDetailControl : UserControl
    {
        public event Action OnHideControl;

        private Compositor _compositor;
        private Visual _detailGridVisual;
        private Visual _borderGridVisual;
        private Visual _downloadBtnVisual;
        private Visual _shareBtnVisual;
        private Visual _infoGridVisual;
        private Visual _downloadingHintGridVisual;
        private Visual _loadingPath;
        private Visual _okVisual;

        private CancellationTokenSource _cts;

        public UnsplashImage CurrentImage
        {
            get { return (UnsplashImage)GetValue(UnsplashImageProperty); }
            set { SetValue(UnsplashImageProperty, value); }
        }

        public static readonly DependencyProperty UnsplashImageProperty =
            DependencyProperty.Register("CurrentImage", typeof(UnsplashImage), typeof(PhotoDetailControl), new PropertyMetadata(null, OnImageChanged));

        private DataTransferManager _dataTransferManager;

        public bool IsShown { get; set; }

        private static void OnImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as PhotoDetailControl;
            var currentImage = e.NewValue as UnsplashImage;
            control.LargeImage.Source = currentImage.ListImageBitmap.Bitmap;
            control.InfoGrid.Background = currentImage.MajorColor;
            control.NameTB.Text = currentImage.Owner.Name;
            if (ColorConverter.IsLight(currentImage.MajorColor.Color))
            {
                control.NameTB.Foreground = new SolidColorBrush(Colors.Black);
                control.ByTB.Foreground = new SolidColorBrush(Colors.Black);
                control.CopyUrlBorder.Background = new SolidColorBrush(Colors.Black);
                control.CopyUrlTB.Foreground = new SolidColorBrush(Colors.White);
            }
            else
            {
                control.NameTB.Foreground = new SolidColorBrush(Colors.White);
                control.ByTB.Foreground = new SolidColorBrush(Colors.White);
                control.CopyUrlBorder.Background = new SolidColorBrush(Colors.White);
                control.CopyUrlTB.Foreground = new SolidColorBrush(Colors.Black);
            }
        }

        public PhotoDetailControl()
        {
            InitializeComponent();
            InitComposition();

            _dataTransferManager = DataTransferManager.GetForCurrentView();
            _dataTransferManager.DataRequested += _dataTransferManager_DataRequested;
        }

        private async void _dataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequestDeferral deferral = args.Request.GetDeferral();
            sender.TargetApplicationChosen += (s, e) =>
              {
                  deferral.Complete();
              };
            await CurrentImage.SetDataRequestDataAsync(args.Request);
            deferral.Complete();
        }

        private void InitComposition()
        {
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            _detailGridVisual = ElementCompositionPreview.GetElementVisual(DetailGrid);
            _borderGridVisual = ElementCompositionPreview.GetElementVisual(MaskBorder);
            _downloadBtnVisual = ElementCompositionPreview.GetElementVisual(DownloadBtn);
            _infoGridVisual = ElementCompositionPreview.GetElementVisual(InfoGrid);
            _downloadingHintGridVisual = ElementCompositionPreview.GetElementVisual(LoadingHintGrid);
            _loadingPath = ElementCompositionPreview.GetElementVisual(LoadingPath);
            _okVisual = ElementCompositionPreview.GetElementVisual(OKBtn);
            _shareBtnVisual = ElementCompositionPreview.GetElementVisual(ShareBtn);

            ResetVisualInitState();
        }

        private void ResetVisualInitState()
        {
            _infoGridVisual.Offset = new Vector3(0f, -100f, 0);
            _downloadBtnVisual.Offset = new Vector3(100f, 0f, 0f);
            _shareBtnVisual.Offset = new Vector3(150f, 0f, 0f);
            _detailGridVisual.Opacity = 0;
            _okVisual.Offset = new Vector3(100f, 0f, 0f);
            _downloadingHintGridVisual.Offset = new Vector3(100f, 0f, 0f);

            StartLoadingAnimation();
        }

        private void MaskBorder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            HideDetailControl();
        }

        public void HideDetailControl()
        {
            ToggleDownloadBtnAnimation(false);
            ToggleShareBtnAnimation(false);
            ToggleOkBtnAnimation(false);
            ToggleDownloadingBtnAnimation(false);

            var batch = _compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            ToggleInfoGridAnimation(false);
            batch.Completed += (s, ex) =>
            {
                OnHideControl?.Invoke();
                ToggleDetailGridAnimation(false);
            };
            batch.End();
        }

        public void ToggleDetailGridAnimation(bool show)
        {
            IsShown = show;

            DetailGrid.Visibility = Visibility.Visible;

            var fadeAnimation = _compositor.CreateScalarKeyFrameAnimation();
            fadeAnimation.InsertKeyFrame(1f, show ? 1f : 0f);
            fadeAnimation.Duration = TimeSpan.FromMilliseconds(show ? 500 : 300);
            fadeAnimation.DelayTime = TimeSpan.FromMilliseconds(show ? 300 : 0);

            var batch = _compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            _detailGridVisual.StartAnimation("Opacity", fadeAnimation);

            if (show)
            {
                ToggleDownloadBtnAnimation(true);
                ToggleShareBtnAnimation(true);
                ToggleInfoGridAnimation(true);
            }

            batch.Completed += (sender, e) =>
            {
                if (!show)
                {
                    ResetVisualInitState();
                    DetailGrid.Visibility = Visibility.Collapsed;
                }
            };
            batch.End();
        }

        private void ToggleDownloadBtnAnimation(bool show)
        {
            var offsetAnimation = _compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.InsertKeyFrame(1f, new Vector3(show ? 0f : 100f, 0f, 0f));
            offsetAnimation.Duration = TimeSpan.FromMilliseconds(1000);
            offsetAnimation.DelayTime = TimeSpan.FromMilliseconds(show ? 500 : 0);

            _downloadBtnVisual.StartAnimation("Offset", offsetAnimation);
        }

        private void ToggleShareBtnAnimation(bool show)
        {
            var offsetAnimation = _compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.InsertKeyFrame(1f, new Vector3(show ? 0f : 150f, 0f, 0f));
            offsetAnimation.Duration = TimeSpan.FromMilliseconds(show ? 1000 : 400);
            offsetAnimation.DelayTime = TimeSpan.FromMilliseconds(show ? 400 : 0);

            _shareBtnVisual.StartAnimation("Offset", offsetAnimation);
        }

        private void ToggleInfoGridAnimation(bool show)
        {
            var offsetAnimation = _compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.InsertKeyFrame(1f, new Vector3(0f, show ? 0f : -100f, 0f));
            offsetAnimation.Duration = TimeSpan.FromMilliseconds(500);
            offsetAnimation.DelayTime = TimeSpan.FromMilliseconds(show ? 500 : 0);

            _infoGridVisual.StartAnimation("Offset", offsetAnimation);
        }

        private void DetailGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.DetailContentGrid.Height = this.DetailContentGrid.ActualWidth / 1.5 + 100;
            this.DetailContentGrid.Clip = new RectangleGeometry()
            { Rect = new Rect(0, 0, DetailContentGrid.ActualWidth, DetailContentGrid.Height) };
        }

        #region Download animation
        private void ToggleDownloadingBtnAnimation(bool show)
        {
            var offsetAnimation = _compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.InsertKeyFrame(1f, new Vector3(show ? 0f : 100f, 0f, 0f));
            offsetAnimation.Duration = TimeSpan.FromMilliseconds(500);
            offsetAnimation.DelayTime = TimeSpan.FromMilliseconds(show ? 200 : 0);

            _downloadingHintGridVisual.StartAnimation("Offset", offsetAnimation);
        }

        private void ToggleOkBtnAnimation(bool show)
        {
            var offsetAnimation = _compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.InsertKeyFrame(1f, new Vector3(show ? 0f : 100f, 0f, 0f));
            offsetAnimation.Duration = TimeSpan.FromMilliseconds(500);
            offsetAnimation.DelayTime = TimeSpan.FromMilliseconds(show ? 200 : 0);

            _okVisual.StartAnimation("Offset", offsetAnimation);
        }

        private async void DownloadBtn_Click(object sender, RoutedEventArgs e)
        {
            ToggleDownloadBtnAnimation(false);
            ToggleDownloadingBtnAnimation(true);

            try
            {
                _cts = new CancellationTokenSource();
                await this.CurrentImage.DownloadFullImageAsync(_cts.Token);
                ToggleDownloadingBtnAnimation(false);

                //Still in this page
                if (IsShown)
                {
                    ToggleOkBtnAnimation(true);
                    ToastService.SendToast("Saved :D", TimeSpan.FromMilliseconds(1000));
                }
            }
            catch (OperationCanceledException)
            {
                ToggleDownloadBtnAnimation(true);
                ToggleDownloadingBtnAnimation(false);
                ToggleOkBtnAnimation(false);
                ToastService.SendToast("Cancelled", TimeSpan.FromMilliseconds(1000));
            }
            catch (Exception)
            {
                ToggleDownloadBtnAnimation(true);
                ToggleDownloadingBtnAnimation(false);
                ToggleOkBtnAnimation(false);
                ToastService.SendToast("Exception throws.", TimeSpan.FromMilliseconds(1000));
            }
        }

        private void StartLoadingAnimation()
        {
            var rotateAnimation = _compositor.CreateScalarKeyFrameAnimation();
            rotateAnimation.InsertKeyFrame(1, 360f, _compositor.CreateLinearEasingFunction());
            rotateAnimation.Duration = TimeSpan.FromMilliseconds(800);
            rotateAnimation.IterationBehavior = AnimationIterationBehavior.Forever;

            _loadingPath.CenterPoint = new Vector3((float)LoadingPath.ActualWidth / 2, (float)LoadingPath.ActualHeight / 2, 0f);

            _loadingPath.RotationAngleInDegrees = 0f;
            _loadingPath.StopAnimation("RotationAngleInDegrees");
            _loadingPath.StartAnimation("RotationAngleInDegrees", rotateAnimation);
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();
        }
        #endregion

        private void ShareBtn_Click(object sender, RoutedEventArgs e)
        {
            DataTransferManager.ShowShareUI();
        }

        private void CopyURLBtn_Click(object sender, RoutedEventArgs e)
        {
            DataPackage dataPackage = new DataPackage();
            dataPackage.SetText(CurrentImage.GetSaveImageUrlFromSettings());
            Clipboard.SetContent(dataPackage);
            ToastService.SendToast("Copied.");
        }

        private void DetailGrid_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (Math.Abs(e.Cumulative.Translation.Y) > 30)
            {
                MaskBorder_Tapped(null, null);
            }
        }

        private void InfoPlaceHolderGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var grid = sender as Grid;
            grid.Clip = new RectangleGeometry() { Rect = new Rect(0, 0, e.NewSize.Width, e.NewSize.Height) };
        }
    }
}
