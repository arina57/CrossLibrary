using UIKit;
using CoreGraphics;

namespace SharedLibrary.iOS.CustomControls {
    class UIPaddingLabel :UILabel {
        private float _padding = 0f;


        private float paddingTop = 0f;
        private float paddingLeft = 0f;
        private float paddingBottom = 0f;
        private float paddingRight = 0f;

        public float Padding {
            get {
                return _padding;
            } set {
                _padding = value;
                paddingTop = _padding;
                paddingLeft = _padding;
                paddingBottom = _padding;
                paddingRight = _padding;
            }
        }


        public override void DrawText(CGRect rect) {
            var insets = new UIEdgeInsets(paddingTop, paddingLeft, paddingBottom, paddingRight);
            base.DrawText(rect);
        }

        public override CGSize IntrinsicContentSize {
            get {
                var intrinsicContentSize = base.IntrinsicContentSize;
                intrinsicContentSize.Height += paddingTop + paddingBottom;
                intrinsicContentSize.Width += paddingLeft + paddingRight;
                return intrinsicContentSize;
            }
        }

    }
}