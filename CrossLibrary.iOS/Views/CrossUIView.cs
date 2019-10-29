using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreGraphics;
using CrossLibrary.Interfaces;
using Foundation;
using UIKit;

namespace CrossLibrary.iOS.Views {
    public abstract class CrossUIView<TViewModel> : UIView, ICrossView, ICrossView<TViewModel> where TViewModel : CrossViewModel {
        public TViewModel ViewModel { get; private set; }
        TaskCompletionSource<bool> dismissedTaskCompletionSource;
        TaskCompletionSource<object> loadedTaskCompletionSource = new TaskCompletionSource<object>();

        public CrossUIView() {
        }

        public CrossUIView(NSCoder coder) : base(coder) {
        }

        protected CrossUIView(NSObjectFlag t) : base(t) {
        }

        protected internal CrossUIView(IntPtr handle) : base(handle) {
        }

        public CrossUIView(CGRect frame) : base(frame) {
        }

        public override void WillMoveToSuperview(UIView newsuper) {
            base.WillMoveToSuperview(newsuper);
            ViewModel?.ViewAppearing();
        }

        public override void MovedToSuperview() {
            base.MovedToSuperview();
            ViewModel?.ViewAppeared();
        }
        public override void RemoveFromSuperview() {
            base.RemoveFromSuperview();
            ViewModel?.ViewDisappearing();
            dismissedTaskCompletionSource?.TrySetResult(true);
            ViewModel?.ViewDisappeared();
            ViewModel?.ViewDestroy();
        }

        public void Dismiss() {
            this.RemoveFromSuperview();
        }

        public override void AwakeFromNib() {
            base.AwakeFromNib();
            loadedTaskCompletionSource.TrySetResult(null);
            ViewModel?.ViewCreated();
        }

        

        public IEnumerable<T> FindViewsOfTypeInTree<T>() where T : class {
            return this.FindViewsOfTypeInTree<T>();
        }

        public void Prepare(TViewModel viewModel) {
            if (this.ViewModel != null) {
                throw new Exception("Prepare should only be run once");
            }
            this.ViewModel = viewModel;
        }

        public abstract void RefreshUILocale();

        public void Show() {
            PlatformFunctions.GetTopViewController().View.AddSubview(this);
            this.TranslatesAutoresizingMaskIntoConstraints = false;
            this.FillParentContraints();
        }

        public async Task ShowAsync() {
            dismissedTaskCompletionSource = new TaskCompletionSource<bool>();
            try {
                this.Show();
                await dismissedTaskCompletionSource.Task;
            } finally {
                dismissedTaskCompletionSource = null;
            }
        }

        public void ShowOver() {
            Show();
        }


        public async Task ShowOverAsync() {
            await ShowAsync();
        }
    }
}