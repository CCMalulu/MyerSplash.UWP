﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

namespace MyerSplashCustomControl
{
    [ContentProperty(Name = "Items")]
    public class TopNavigationControl : Control
    {
        private const float ANIMATION_MILLIS = 500;

        private StackPanel _rootPanel;
        private Border _navigationBorder;
        private Visual _borderVisual;
        private Compositor _compositor;

        public Collection<Object> Items
        {
            get;
        }

        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int),
                typeof(TopNavigationControl), new PropertyMetadata(1, (s, r) =>
                {
                    TopNavigationControl target = s as TopNavigationControl;
                    target.UpdateSelectedSlider();
                }));

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register("ItemTemplate", typeof(DataTemplate),
                typeof(TopNavigationControl), new PropertyMetadata(null, (s, r) =>
                {
                    TopNavigationControl target = s as TopNavigationControl;
                    target.UpdateViews();
                }));

        public Brush SliderBrush
        {
            get { return (Brush)GetValue(SliderBrushProperty); }
            set { SetValue(SliderBrushProperty, value); }
        }

        public static readonly DependencyProperty SliderBrushProperty =
            DependencyProperty.Register("SliderBrush", typeof(Brush),
                typeof(TopNavigationControl), new PropertyMetadata(null));

        protected virtual void OnItemTemplateChanged(DataTemplate oldValue, DataTemplate newValue)
        {
            UpdateViews();
        }

        public TopNavigationControl()
        {
            this.DefaultStyleKey = typeof(TopNavigationControl);
            var items = new ObservableCollection<object>();
            items.CollectionChanged += Items_CollectionChanged;
            Items = items;
        }

        protected virtual DependencyObject GetContainerForItemOverride()
        {
            var cp = new ContentPresenter
            {
                Background = new SolidColorBrush(Colors.Transparent)
            };
            return cp;
        }

        protected virtual bool IsItemItsOwnContainerOverride(Object item)
        {
            return item is UIElement;
        }

        protected virtual void PrepareContainerForItemOverride(DependencyObject container, Object item)
        {
            if (container is ContentControl)
            {
                ContentControl control = container as ContentControl;
                control.Content = item;
                control.ContentTemplate = ItemTemplate;
            }
            else if (container is ContentPresenter)
            {
                ContentPresenter presenter = container as ContentPresenter;
                presenter.Content = item;
                presenter.ContentTemplate = ItemTemplate;
            }
        }

        private void UpdateSelectedSlider()
        {
            if (_rootPanel == null) return;
            if (SelectedIndex < 0 || SelectedIndex >= _rootPanel.Children.Count)
            {
                _borderVisual.Opacity = 0f;
                return;
            }

            _borderVisual.Opacity = 1f;

            for (var i = 0; i < _rootPanel.Children.Count; i++)
            {
                var e = _rootPanel.Children[i];
                var visual = ElementCompositionPreview.GetElementVisual(e);
                visual.Opacity = i == SelectedIndex ? 1f : 0.5f;
            }

            UIElement element = _rootPanel.Children[SelectedIndex];
            if (element is ContentPresenter)
            {
                if (!(VisualTreeHelper.GetChild(element, 0) is FrameworkElement child)) return;

                var paddingLeft = 0.0;
                var paddingRight = 0.0;

                if (child is TextBlock)
                {
                    paddingLeft = (child as TextBlock).Padding.Left;
                    paddingRight = (child as TextBlock).Padding.Right;
                }
                else if (child is Control)
                {
                    paddingLeft = (child as Control).Padding.Left;
                    paddingLeft = (child as Control).Padding.Right;
                }

                var transform = child.TransformToVisual(_rootPanel);
                var point = transform.TransformPoint(new Point(paddingLeft, child.ActualHeight));

                _borderVisual.Offset = new Vector3((float)point.X, 0, 0);

                var scale = (child.ActualWidth - paddingLeft - paddingRight) / _navigationBorder.ActualWidth;

                _borderVisual.Scale = new Vector3((float)scale, 1f, 1f);
            }
        }

        private void UpdateViews()
        {
            if (_rootPanel == null) return;

            foreach (var child in _rootPanel.Children)
            {
                child.PointerPressed -= Element_PointerPressed;
            }

            _rootPanel.Children.Clear();

            for (var i = 0; i < Items.Count; i++)
            {
                var item = Items[i];

                DependencyObject container;
                if (IsItemItsOwnContainerOverride(item))
                {
                    container = item as DependencyObject;
                }
                else
                {
                    container = GetContainerForItemOverride();
                    PrepareContainerForItemOverride(container, item);
                }

                if (container is UIElement)
                {
                    var element = container as UIElement;
                    element.PointerPressed += Element_PointerPressed;

                    var implicitAnimationCollection = _compositor.CreateImplicitAnimationCollection();
                    implicitAnimationCollection.Add("Opacity", CreateOpacityAnimation());

                    var visual = ElementCompositionPreview.GetElementVisual(element);
                    visual.ImplicitAnimations = implicitAnimationCollection;

                    if (SelectedIndex != i)
                    {
                        visual.Opacity = 0.5f;
                    }

                    _rootPanel.Children.Add(container as UIElement);
                }
            }
        }

        private ScalarKeyFrameAnimation CreateOpacityAnimation()
        {
            var opacityAnimation = _compositor.CreateScalarKeyFrameAnimation();
            opacityAnimation.Target = "Opacity";
            opacityAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            opacityAnimation.Duration = TimeSpan.FromMilliseconds(ANIMATION_MILLIS);

            return opacityAnimation;
        }

        private Vector3KeyFrameAnimation CreateOffsetAnimation()
        {
            var offsetAnimation = _compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.Target = "Offset";
            offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            offsetAnimation.Duration = TimeSpan.FromMilliseconds(ANIMATION_MILLIS);

            return offsetAnimation;
        }

        private Vector3KeyFrameAnimation CreateScaleAnimation()
        {
            var offsetAnimation = _compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.Target = "Scale";
            offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            offsetAnimation.Duration = TimeSpan.FromMilliseconds(ANIMATION_MILLIS);

            return offsetAnimation;
        }

        private void Element_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            SelectedIndex = _rootPanel.Children.IndexOf(sender as UIElement);
        }

        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateViews();
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _rootPanel = GetTemplateChild("RootPanel") as StackPanel;
            _rootPanel.SizeChanged += _rootPanel_SizeChanged;

            _navigationBorder = GetTemplateChild("NavigationBorder") as Border;

            _borderVisual = ElementCompositionPreview.GetElementVisual(_navigationBorder);
            _compositor = _borderVisual.Compositor;

            var implicitAnimationCollection = _compositor.CreateImplicitAnimationCollection();
            implicitAnimationCollection.Add("Offset", CreateOffsetAnimation());
            implicitAnimationCollection.Add("Opacity", CreateOpacityAnimation());
            implicitAnimationCollection.Add("Scale", CreateScaleAnimation());
            _borderVisual.ImplicitAnimations = implicitAnimationCollection;

            UpdateViews();
        }

        private void _rootPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateSelectedSlider();
        }
    }
}
