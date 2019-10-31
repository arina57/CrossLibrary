using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreGraphics;
using CrossLibrary.Interfaces;
using Foundation;
using UIKit;

namespace CrossLibrary.iOS.Views {
    [Register("CrossConainerView")]
    public class CrossConainerView : UIView, ICrossContainerView {


        public CrossConainerView() : base() {
        }

        public CrossConainerView(NSCoder coder) : base(coder) {
        }

        public CrossConainerView(CGRect frame) : base(frame) {
        }

        protected CrossConainerView(NSObjectFlag t) : base(t) {
        }

        protected internal CrossConainerView(IntPtr handle) : base(handle) {
        }

        public string ContainerId => this.AccessibilityIdentifier;

        public CrossViewModel SubCrossViewModel { get; private set; }

        public void RemoveAllViews() {
            foreach (var view in this.Subviews) {
                view.RemoveFromSuperview();
            }
        }

        public void RemoveView() {
            SubCrossViewModel?.Dismiss();
            SubCrossViewModel = null;
        }

        public void ShowView<TViewModel>(TViewModel crossViewModel) where TViewModel : CrossViewModel {
            RemoveView();
            SubCrossViewModel = crossViewModel;
            if (crossViewModel.CrossView is UIViewController viewController) {
                var parentViewController = this.FindViewController();
                parentViewController.AddChildViewController(viewController);
                this.TranslatesAutoresizingMaskIntoConstraints = false;
                this.AddSubview(viewController.View);
                viewController.View.FillParentContraints();
                viewController.DidMoveToParentViewController(parentViewController);
            } else if (crossViewModel.CrossView is UIView view) {
                this.AddSubview(view);
                view.FillParentContraints();
            } else {
                throw new Exception("No case for crossview of that type");
            }
        }
    }
}