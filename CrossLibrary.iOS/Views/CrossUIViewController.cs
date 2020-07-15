using System;
using System.Collections.Generic;
using Foundation;
using UIKit;
using System.Threading.Tasks;
using CrossLibrary.Interfaces;
using System.Linq;
using System.Linq.Expressions;

namespace CrossLibrary.iOS.Views {
    public abstract class PlatformCrossView<TViewModel> : CrossUIViewController<TViewModel>, ICrossView, ICrossView<TViewModel> where TViewModel : CrossViewModel { }

        public abstract class CrossUIViewController<TViewModel> : UIViewController, ICrossView, ICrossView<TViewModel> where TViewModel : CrossViewModel {



        TaskCompletionSource<bool> dismissedTaskCompletionSource;
        TaskCompletionSource<object> loadedTaskCompletionSource = new TaskCompletionSource<object>();
        



        public bool Visible => this.ViewIfLoaded != null && this.ViewIfLoaded.Window != null;






        public TViewModel ViewModel { get; private set; }

        public virtual bool ViewCreated { get; protected set; } = false;

        /// <summary>
        /// Called during depenancy injection in CrossViewDependencyServices from Dynamic class
        /// </summary>
        /// <param name="model"></param>
        public virtual void Prepare(TViewModel model) {
            if (this.ViewModel != null) {
                throw new Exception("Prepare should only be run once");
            }
            this.ViewModel = model;
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


  




        public void Show(bool animated = true) {
            ModalPresentationStyle = UIModalPresentationStyle.Popover;
            ModalTransitionStyle = UIModalTransitionStyle.CoverVertical;
            ModalPresentationCapturesStatusBarAppearance = true;
            var nav = PlatformFunctions.GetNavigationController();
            nav.PushViewController(this, animated);
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

        private void ShowOver(UIViewController parent, bool animated = true) {
            ModalPresentationStyle = UIModalPresentationStyle.OverCurrentContext;
            ModalTransitionStyle = UIModalTransitionStyle.CrossDissolve;
            parent.PresentViewController(this, animated, () => { });
        }



        private async Task ShowOverAsync(UIViewController parent, bool animated = true) {
            ModalPresentationStyle = UIModalPresentationStyle.OverCurrentContext;
            ModalTransitionStyle = UIModalTransitionStyle.CrossDissolve;
            dismissedTaskCompletionSource = new TaskCompletionSource<bool>();
            try {
                parent.PresentViewController(this, animated, () => { });
                await dismissedTaskCompletionSource.Task;
            } finally {
                dismissedTaskCompletionSource = null;
            }
        }

        public async Task ShowAsync(bool animated = true) {
            dismissedTaskCompletionSource = new TaskCompletionSource<bool>();
            try {
                this.Show(animated);
                await dismissedTaskCompletionSource.Task;
            } finally {
                dismissedTaskCompletionSource = null;
            }
        }

        public async Task ShowOverAsync(bool animated = true) {
            await ShowOverAsync(PlatformFunctions.GetTopViewController(), animated);
        }

        public void ShowOver(bool animated = true) {
            ShowOver(PlatformFunctions.GetTopViewController(), animated);
        }




        public virtual void Dismiss() {
            if (PlatformFunctions.GetTopViewController() == this) {
                NavigationController?.PopViewController(true);
            } else {
                this.View?.RemoveFromSuperview();
                this.RemoveFromParentViewController();
            }
            this.DismissViewController(true, null);
                
            
        }

        public virtual void RefreshUILocale() { }

        public IEnumerable<T> FindViewsOfTypeInTree<T>() where T : class {
            return this.View.FindViewsOfTypeInTree<T>();
        }








        public override void ViewWillUnload() {
            base.ViewDidUnload();
            ViewModel?.ViewDestroy();

        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
            ViewModel.ViewDisposed();
        }

    

        public override void DidReceiveMemoryWarning() {
            base.DidReceiveMemoryWarning();
            ViewModel.OnLowMemory();
        }

  


        
        public override void ViewDidLoad() {
            ViewCreated = true;
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
            ViewCreated = false;
            
        }

        public override void DidMoveToParentViewController(UIViewController parent) {
            base.DidMoveToParentViewController(parent);
            ViewCreated = true;
        }


        public override void ViewWillDisappear(bool animated) {
            base.ViewWillDisappear(animated);
            if (IsBeingDismissed || IsMovingFromParentViewController) {

            }
            ViewModel?.ViewDisappearing();
        }

        public override void ViewDidDisappear(bool animated) {
            base.ViewDidDisappear(animated);
            if (IsBeingDismissed || IsMovingFromParentViewController) {
                dismissedTaskCompletionSource?.TrySetResult(true);
                GC.Collect(); //should I be doing this? probably not
            }
            ViewModel?.ViewDisappeared();
        }

        public override void ViewWillAppear(bool animated) {
            base.ViewWillAppear(animated);
            ViewModel?.ViewAppearing();
        }


        /// <summary>
        /// Binds an action to a view model property.
        /// eg Bind(value => TextView.Text = value, viewModel => viewModel.TextValue)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="binding"></param>
        /// <returns></returns>
        public Action<T> Bind<T>(Action<T> action, Expression<Func<TViewModel, T>> binding) {
            return ViewModel.Bind(action, binding);
        }

        /// <summary>
        /// Binds text to View Model property.
        /// eg Bind(label, viewModel => viewModel.LabelText)
        /// </summary>
        /// <param name="label"></param>
        /// <param name="binding"></param>
        /// <returns>Returns action, so it can be unbound later</returns>
        public Action<string> BindText(UILabel label, Expression<Func<TViewModel, string>> binding) {
            return ViewModel.Bind(value => label.Text = value, binding);
        }

        public Action<string> BindText(UIButton label, Expression<Func<TViewModel, string>> binding) {
            return ViewModel.Bind(value => label.SetTitle(value, UIControlState.Normal), binding);
        }


        /// <summary>
        /// Binds the visiblity of a view to a view model property
        /// </summary>
        /// <param name="view"></param>
        /// <param name="binding"></param>
        /// <returns></returns>
        public Action<bool> BindVisiblitiy(UIView view, Expression<Func<TViewModel, bool>> binding) {
            return ViewModel.Bind(value => view.Hidden = !value, binding);
        }

        /// <summary>
        /// Binds views alpha to view model property
        /// </summary>
        /// <param name="view"></param>
        /// <param name="binding"></param>
        /// <returns></returns>
        public Action<float> BindAlpha(UIView view, Expression<Func<TViewModel, float>> binding) {
            return ViewModel.Bind(value => view.Alpha = value, binding);
        }

        public UITapGestureRecognizer BindClick(UIView view, Func<TViewModel, EventHandler> binding) {
            var action = binding.Invoke(ViewModel);
            var gestureRecogniser = new UITapGestureRecognizer(() => action.Invoke(view, EventArgs.Empty));
            view.AddGestureRecognizer(gestureRecogniser);
            boundClickActions.Add(view, gestureRecogniser);
            return gestureRecogniser;
        }



        Dictionary<UIView, UIGestureRecognizer> boundClickActions = new Dictionary<UIView, UIGestureRecognizer>();

        public void Unbind(UIView view, UITapGestureRecognizer binding) {
            view.RemoveGestureRecognizer(binding);
            binding.Dispose();
            boundClickActions.Remove(view);
        }



        /// <summary>
        /// Unbinds all property bound to specified action
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="actionReference"></param>
        public void Unbind<T>(Action<T> actionReference) {
            ViewModel.Unbind(actionReference);
        }

        /// <summary>
        /// Removes all bindings
        /// </summary>
        public void UnbindAll() => ViewModel.UnbindAll();

        public void UnbindAllClicks() {
            foreach (var clickAction in boundClickActions) {
                clickAction.Key.RemoveGestureRecognizer(clickAction.Value);
                clickAction.Value.Dispose();
            }
            boundClickActions.Clear();
        }
    }



}