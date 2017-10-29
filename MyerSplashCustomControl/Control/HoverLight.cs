﻿using JP.Utils.UI;
using System;
using System.Numerics;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace MyerSplashCustomControl.Control
{
    public class HoverLight : XamlLight
    {
        private ExpressionAnimation _lightPositionExpression;
        private Vector3KeyFrameAnimation _offsetAnimation;
        private static readonly string Id = typeof(HoverLight).FullName;

        protected override void OnConnected(UIElement targetElement)
        {
            Compositor compositor = Window.Current.Compositor;

            // Create SpotLight and set its properties
            PointLight pointLight = compositor.CreatePointLight();
            //spotLight.InnerConeAngleInDegrees = 45f;
            pointLight.Color = "#44FFFFFF".ToColor();
            pointLight.LinearAttenuation = 0.007f;
            pointLight.ConstantAttenuation = 0.007f;
            pointLight.Intensity = 1f;
            //spotLight.OuterConeAngleInDegrees = 30f;
            //spotLight.ConstantAttenuation = 0.2f;
            //spotLight.LinearAttenuation = 1.253f;
            //spotLight.QuadraticAttenuation = 0.18f;

            // Associate CompositionLight with XamlLight
            CompositionLight = pointLight;

            // Define resting position Animation
            Vector3 restingPosition = new Vector3(200, 200, 400);
            CubicBezierEasingFunction cbEasing = compositor.CreateCubicBezierEasingFunction(new Vector2(0.3f, 0.7f), new Vector2(0.9f, 0.5f));
            _offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
            _offsetAnimation.InsertKeyFrame(1, restingPosition, cbEasing);
            _offsetAnimation.Duration = TimeSpan.FromSeconds(0.5f);

            pointLight.Offset = restingPosition;

            // Define expression animation that relates light's offset to pointer position
            CompositionPropertySet hoverPosition = ElementCompositionPreview.GetPointerPositionPropertySet(targetElement);
            _lightPositionExpression = compositor.CreateExpressionAnimation("Vector3(hover.Position.X, hover.Position.Y, height)");
            _lightPositionExpression.SetReferenceParameter("hover", hoverPosition);
            _lightPositionExpression.SetScalarParameter("height", 180.0f);

            // Configure pointer entered/ exited events
            targetElement.PointerMoved += TargetElement_PointerMoved;
            targetElement.PointerExited += TargetElement_PointerExited;

            // Add UIElement to the Light's Targets
            HoverLight.AddTargetElement(GetId(), targetElement);
        }

        private void MoveToRestingPosition()
        {
            if (CompositionLight != null)
            {
                // Start animation on SpotLight's Offset
                CompositionLight.StartAnimation("Offset", _offsetAnimation);
            }
        }

        private void TargetElement_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (CompositionLight != null)
            {
                // touch input is still UI thread-bound as of the Creator's Update
                if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
                {
                    Vector2 offset = e.GetCurrentPoint((UIElement)sender).Position.ToVector2();
                    (CompositionLight as SpotLight).Offset = new Vector3(offset.X, offset.Y, 15);
                }
                else
                {
                    // Get the pointer's current position from the property and bind the SpotLight's X-Y Offset
                    CompositionLight.StartAnimation("Offset", _lightPositionExpression);
                }
            }
        }

        private void TargetElement_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            // Move to resting state when pointer leaves targeted UIElement
            MoveToRestingPosition();
        }

        protected override void OnDisconnected(UIElement oldElement)
        {
            // Dispose Light and Composition resources when it is removed from the tree
            HoverLight.RemoveTargetElement(GetId(), oldElement);
            CompositionLight.Dispose();
            _lightPositionExpression.Dispose();
            _offsetAnimation.Dispose();
        }

        protected override string GetId()
        {
            return Id;
        }
    }
}