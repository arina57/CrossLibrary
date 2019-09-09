using System;
using System.Collections.Generic;
using Foundation;
using UIKit;
using System.Threading.Tasks;
using CrossLibrary.Interfaces;

namespace CrossLibrary.iOS.Views {
    public abstract class CrossUIViewController<TViewModel> : UIViewController, ICrossView, ICrossView<TViewModel> where TViewModel : CrossViewModel {

        TaskCompletionSource<bool> dismissedTaskCompletionSource;
        TaskCompletionSource<object> loadedTaskCompletionSource = new TaskCompletionSource<object>();



        /// <summary>
        /// These will run when view has been popped
        /// </summary>
        protected List<Action> cleanupActions = new List<Action>();
        /// <summary>
        /// These will be disposed when vuew has been popped
        /// </summary>
        protected List<IDisposable> DisposableObjects = new List<IDisposable>();

        public bool Visible => this.ViewIfLoaded != null && this.ViewIfLoaded.Window != null;



        public bool UseToolbar { get; set; } = true;



        public TViewModel ViewModel { get; private set; }
        public virtual void Prepare(TViewModel model) {
            if (this.ViewModel != null) {
                throw new Exception("Prepare should only be run once");
            }
            this.ViewModel = model;
        }


        public void AddUnsubscriptionAction(Action unsubscription) {
            cleanupActions.Add(unsubscription);
        }

        public CrossUIViewController() : base() {
        }

        public CrossUIViewController(NSCoder coder) : base(coder) {
        }

        public CrossUIViewController(string nibName, NSBundle bundle) : base(nibName, bundle) {
        }

        protected CrossUIViewController(NSObjectFlag t) : base(t) {
        }

        protected CrossUIViewController(IntPtr handle) : base(handle) {
        }


        private void Settings_LangaugeChanged(object sender, EventArgs e) {
            if (Visible) {
                RefreshUILocale();
            }

        }

        protected void AddCleanupAction(Action action) {
            cleanupActions.Add(action);
        }




        public void Show() {
            ModalPresentationStyle = UIModalPresentationStyle.Popover;
            ModalTransitionStyle = UIModalTransitionStyle.CoverVertical;
            ModalPresentationCapturesStatusBarAppearance = true;
            PlatformFunctions.GetNavigationController().PushViewController(this, true);
        }


        public async Task WhenShownAsync() {
            ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
            ModalTransitionStyle = UIModalTransitionStyle.CoverVertical;
            TaskCompletionSource<object> presentedTaskCompletionSource = new TaskCompletionSource<object>();
            PlatformFunctions.GetTopViewController().PresentViewController(this, true, () => {
                presentedTaskCompletionSource.TrySetResult(null);
            });
            await presentedTaskCompletionSource.Task;
        }

        private void ShowOver(UIViewController parent) {
            ModalPresentationStyle = UIModalPresentationStyle.OverCurrentContext;
            ModalTransitionStyle = UIModalTransitionStyle.CrossDissolve;
            parent.PresentViewController(this, true, () => { });
        }



        private async Task ShowOverAsync(UIViewController parent) {
            ModalPresentationStyle = UIModalPresentationStyle.OverCurrentContext;
            ModalTransitionStyle = UIModalTransitionStyle.CrossDissolve;
            dismissedTaskCompletionSource = new TaskCompletionSource<bool>();
            try {
                parent.PresentViewController(this, true, () => { });
                await dismissedTaskCompletionSource.Task;
            } finally {
                dismissedTaskCompletionSource = null;
            }
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

        public async Task ShowOverAsync() {
            await ShowOverAsync(PlatformFunctions.GetTopViewController());
        }

        public void ShowOver() {
            ShowOver(PlatformFunctions.GetTopViewController());
        }




        public virtual void Dismiss() {
            NavigationController?.PopViewController(true);
            this.DismissViewController(true, null);

        }

        public abstract void RefreshUILocale();

        /// <summary>
        /// Cleans up after every time the view did disappear
        /// </summary>
        public virtual void CleanUpAfterDisappearing() {

        }


        /// <summary>
        /// Invokes all actions for deregistering events
        /// </summary>
        public virtual void DoCleanupActions() {
            for (int i = cleanupActions.Count - 1; i >= 0; i--) {
                cleanupActions[i]?.Invoke();
            }
            cleanupActions.Clear();
            //eventActions = null;
        }


        /// <summary>
        /// Cleans up after view did disappear and it's moving from parent view or, the parent view is being dismissed
        /// </summary>
        public virtual void CleanUpAfterDismiss() {
            DoCleanupActions();
            foreach (var disposeObject in DisposableObjects) {
                disposeObject?.Dispose();
            }
            DisposableObjects?.Clear();
            this.Dispose();
        }


        public override void DidReceiveMemoryWarning() {
            base.DidReceiveMemoryWarning();
        }

        public void AddTapActionToReleaseOnDispose(UIView view, Action action) {
            var gestureRecognizer = new UITapGestureRecognizer(action);
            DisposableObjects.Add(gestureRecognizer);
            view.AddGestureRecognizer(gestureRecognizer);
            cleanupActions.Add(() => view.RemoveGestureRecognizer(gestureRecognizer));

        }



        public override void ViewDidLoad() {
            base.ViewDidLoad();
            loadedTaskCompletionSource.TrySetResult(null);
            ViewModel?.ViewCreated();
            // Perform any additional setup after loading the view, typically from a nib.
        }



        

        public override void ViewDidAppear(bool animated) {
            base.ViewDidAppear(animated);
            ViewModel?.ViewAppeared();
        }


        public override void RemoveFromParentViewController() {
            base.RemoveFromParentViewController();
            ViewModel?.ViewDestroy();
        }




        public override void ViewWillDisappear(bool animated) {
            base.ViewWillDisappear(animated);
            if (IsBeingDismissed || IsMovingFromParentViewController) {

            }
            ViewModel?.ViewDisappearing();
        }

        public override void ViewDidDisappear(bool animated) {
            base.ViewDidDisappear(animated);
            CleanUpAfterDisappearing();
            if (IsBeingDismissed || IsMovingFromParentViewController) {
                CleanUpAfterDismiss();
                dismissedTaskCompletionSource?.TrySetResult(true);
                GC.Collect(); //should I be doing this? probably not
            }
            ViewModel?.ViewDisappeared();
        }

        public override void ViewWillAppear(bool animated) {
            base.ViewWillAppear(animated);
            this.NavigationController?.SetToolbarHidden(!UseToolbar, false);
            RefreshUILocale();
            ViewModel?.ViewAppearing();
        }

    }



}