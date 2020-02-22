using System;

using AppKit;
using CoreGraphics;
using Foundation;
using KSynthesizer;
using KSynthesizer.Sources;
using ObjCRuntime;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Xamarin.Mac;
using TestTool.macOS.Views;

namespace TestTool.macOS
{
    public partial class ViewController : NSViewController
    {
        private AudioSourceView view = new AudioSourceView();
        
        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            audioSourceView.WantsLayer = true;
            audioSourceView.Layer.BackgroundColor = CGColor.CreateSrgb(1, 0, 0, 1);
            audioSourceView.AddSubview(view);
        }

        partial void freqChanged(NSObject sender)
        {
            view.Frequency = freqSlider.IntValue;
        }

        public override NSObject RepresentedObject
        {
            get
            {
                return base.RepresentedObject;
            }
            set
            {
                base.RepresentedObject = value;
                // Update the view, if already loaded.
            }
        }
    }
}
