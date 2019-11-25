using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreGraphics;
using CrossLibrary.Interfaces;
using Foundation;
using UIKit;

namespace CrossLibrary.iOS.Views {
    [Register("CrossContainerView")]
    public class CrossContainerView : UIView, ICrossContainerView {


        public CrossContainerView() : base() {
        }

        public CrossContainerView(NSCoder coder) : base(coder) {
        }

        public CrossContainerView(CGRect frame) : base(frame) {
        }

        protected CrossContainerView(NSObjectFlag t) : base(t) {
        }

        protected internal CrossContainerView(IntPtr handle) : base(handle) {
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

        public override void AwakeFromNib() {
            base.AwakeFromNib();

        }

        public void SuperCrossViewAppearing() {
            if (SubCrossViewModel != null && (this.Subviews == null || !Subviews.Any(view => view == SubCrossViewModel.CrossView))) {
                SubCrossViewModel.ShowIn(this);
            }

        }

        public override void MovedToWindow() {
            base.MovedToWindow();
            if (SubCrossViewModel != null && (this.Subviews.Length == 0 && !Subviews.Any(view => view == SubCrossViewModel.CrossView))) {
                SubCrossViewModel.ShowIn(this);
            }
        }

        public void ShowView<TViewModel>(TViewModel crossViewModel) where TViewModel : CrossViewModel {
            RemoveView();
            SubCrossViewModel = crossViewModel;
            crossViewModel.ShowIn(this);
        }


        public override bool PointInside(CGPoint point, UIEvent uievent) {
            if(Subviews.Any(subview => !subview.Hidden && subview.UserInteractionEnabled && subview.PointInside(ConvertPointToView(point, subview), uievent))) {
                return true;
            } else {
                return false;
            }
        }
    }
}