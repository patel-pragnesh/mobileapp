using System;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using MvvmCross.Base;
using Toggl.Multivac.Extensions;
using UIKit;

namespace Toggl.Daneel.Views
{
    [Register(nameof(HueSaturationPickerView))]
    public sealed class HueSaturationPickerView : UIView
    {
        private readonly CALayer opacityLayer = new CALayer();
        
        private const byte circleDiameter = 30;
        private const byte circleRadius = circleDiameter / 2;
        private static readonly CGColor circleColor = UIColor.White.CGColor;

        private const byte outerCircleDiameter = 32;
        private const byte outerCircleRadius = outerCircleDiameter / 2;
        private static readonly CGColor outerCircleColor = UIColor.Black.ColorWithAlpha(0.3f).CGColor;

        public event EventHandler HueChanged;
        public event EventHandler SaturationChanged;

        public UIColor Color { get; set; }

        public float Hue { get; set; }

        public float Saturation { get; set; }

        private float value;
        public float Value
        {
            get => value;
            set
            {
                if (this.value == value) return;
                this.value = value;

                opacityLayer.BackgroundColor = UIColor.Black.ColorWithAlpha(complement(Value)).CGColor;
                SetNeedsDisplay();
            }
        }

        public HueSaturationPickerView(IntPtr handle)
            : base(handle)
        {
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);

            var touch = touches.AnyObject as UITouch;
            var point = touch.LocationInView(this);
            updateLocation(point);
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            base.TouchesMoved(touches, evt);

            var touch = touches.AnyObject as UITouch;
            var point = touch.LocationInView(this);
            updateLocation(point);
        }

        private void updateLocation(CGPoint touchPoint)
        {
            var width = Frame.Width;
            var height = Frame.Height;
            var pointX = touchPoint.X.Clamp(0, width);
            var pointY = touchPoint.Y.Clamp(0, height);

            Hue = (float)(pointX / width);
            Saturation = complement((float)(pointY / height));

            HueChanged.Raise(this);
            SaturationChanged.Raise(this);

            SetNeedsDisplay();
        }

        public override void Draw(CGRect rect)
        {
            base.Draw(rect);

            var context = UIGraphics.GetCurrentContext();

            var x = (Frame.Width * Hue) - circleRadius;
            var y = Frame.Height * complement(Saturation) - circleRadius;

            var circle = new CGRect(x, y, circleDiameter, circleDiameter);
            var path = UIBezierPath.FromRoundedRect(circle, circleRadius);
            path.LineWidth = 2;
            context.AddPath(path.CGPath);
            context.SetStrokeColor(circleColor);
            context.SetFillColor(UIColor.FromHSB(Hue, Saturation, Value).CGColor);
            context.DrawPath(CGPathDrawingMode.FillStroke);

            var outerCircle = new CGRect(x - 1, y - 1, outerCircleDiameter, outerCircleDiameter);
            var outerPath = UIBezierPath.FromRoundedRect(outerCircle, outerCircleRadius);
            outerPath.LineWidth = 1;
            context.AddPath(outerPath.CGPath);
            context.SetStrokeColor(outerCircleColor);
            context.DrawPath(CGPathDrawingMode.Stroke);
        }

        public void InitializeLayers(CGRect layerFrame)
        {
            opacityLayer.CornerRadius = 3;
            opacityLayer.Frame = layerFrame;
            opacityLayer.BackgroundColor = UIColor.Black.ColorWithAlpha(complement(Value)).CGColor;
            Layer.InsertSublayer(opacityLayer, 0);

            const float step = 1 / 6f;
            Layer.InsertSublayer(new CAGradientLayer
            {
                BorderWidth = 1,
                CornerRadius = 3,
                Frame = layerFrame,
                StartPoint = new CGPoint(0, 0),
                EndPoint = new CGPoint(1, 0),
                BorderColor = UIColor.Black.ColorWithAlpha(0.08f).CGColor,
                Locations = new NSNumber[] { 0, step * 1, step * 2, step * 3, step * 4, step * 5, 1.0 },
                Colors = new CGColor[]
                {
                    UIColor.Red.CGColor, UIColor.Yellow.CGColor, UIColor.Green.CGColor,
                    UIColor.Cyan.CGColor, UIColor.Blue.CGColor, UIColor.Magenta.CGColor, UIColor.Red.CGColor
                },
                Mask = new CAGradientLayer
                {
                    Frame = layerFrame,
                    Colors = new CGColor[] { UIColor.Black.CGColor, UIColor.Clear.CGColor },
                    Locations = new NSNumber[] { 0.0, 1.0 }
                }
            }, 0);

            SetNeedsDisplay();
        }

        private float complement(float number) => Math.Abs(number - 1);
    }
}
