// WARNING
//
// This file has been generated automatically by Rider IDE
//   to store outlets and actions made in Xcode.
// If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace TestTool.macOS
{
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		AppKit.NSView audioSourceView { get; set; }

		[Outlet]
		AppKit.NSSlider freqSlider { get; set; }

		[Action ("freqChanged:")]
		partial void freqChanged (Foundation.NSObject sender);

		void ReleaseDesignerOutlets ()
		{
			if (audioSourceView != null) {
				audioSourceView.Dispose ();
				audioSourceView = null;
			}

			if (freqSlider != null) {
				freqSlider.Dispose ();
				freqSlider = null;
			}

		}
	}
}
