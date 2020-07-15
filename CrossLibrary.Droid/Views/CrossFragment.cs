using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Android.OS;
using Android.Views;
using Android.Views.Animations;
//using Android.Views.Animations;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Fragment.App;
using CrossLibrary.Interfaces;
using Plugin.CurrentActivity;

namespace CrossLibrary.Droid.Views {

    public abstract class PlatformCrossView<TViewModel> : CrossFragment<TViewModel>, ICrossView, ICrossView<TViewModel> where TViewModel : CrossViewModel {
    }

    public abstract class CrossFragment : CrossFragment<CrossViewModel>, ICrossView, ICrossView<CrossViewModel> {
    }

    /// <summary>
    /// Fragment for easier showing and dismissing
    /// </summary>
    public abstract class CrossFragment<TViewModel> : Fragment, ICrossView, ICrossView<TViewModel> where TViewModel : CrossViewModel {
        TaskCompletionSource<bool> dismissedTaskCompletionSource;
        TaskCompletionSource<object> loadedTaskCompletionSource = new TaskCompletionSource<object>();

        private bool first = true;
        public bool AboutToBeShown { get; private set; }
        public DateTime ResumeTime { get; private set; } = DateTime.Now;

        public string UnqueId { get; } = Guid.NewGuid().ToString();
        public TimeSpan TimeSinceResume => DateTime.Now.Subtract(ResumeTime);




        private AppCompatActivity AppCompatActivity => CrossCurrentActivity.Current.Activity as AppCompatActivity;

        public bool Closing { get; private set; } = false;


        public void ShowInPlaceOf(Fragment parent) {
            if (!this.AboutToBeShown && !this.IsAdded) {
                this.AboutToBeShown = true;
                FragmentTransaction ft = parent.FragmentManager.BeginTransaction();
                ft.Replace(((ViewGroup)parent.View.Parent).Id, this);
                ft.AddToBackStack(this.UnqueId);
                ft.Commit();
            }
        }




        public void ShowIn(ViewGroup containerView) {
            if (!this.AboutToBeShown && !this.IsAdded) {
                this.AboutToBeShown = true;

                FragmentTransaction ft = AppCompatActivity.SupportFragmentManager.BeginTransaction();
                ft.SetCustomAnimations(Resource.Animation.fade_in_fast, Resource.Animation.fade_out_fast, Resource.Animation.fade_in_fast, Resource.Animation.fade_out_fast);
                ft.Replace(containerView.Id, this)
                    .Commit();
            }
        }

        public void ShowIn(int containerViewId) {
            if (!this.AboutToBeShown && !this.IsAdded) {
                this.AboutToBeShown = true;
                FragmentTransaction ft = AppCompatActivity.SupportFragmentManager.BeginTransaction();
                ft.SetCustomAnimations(Resource.Animation.fade_in_fast, Resource.Animation.fade_out_fast, Resource.Animation.fade_in_fast, Resource.Animation.fade_out_fast);
                ft.Replace(containerViewId, this)
                    .Commit();
            }
        }


        public void Show(bool animated = true) {
            if (!this.AboutToBeShown && !this.IsAdded) {
                this.AboutToBeShown = true;
                FragmentTransaction ft = AppCompatActivity.SupportFragmentManager.BeginTransaction();
                if(animated) {
                    ft.SetCustomAnimations(Resource.Animation.slide_in_from_right, Resource.Animation.slide_out_to_left, Resource.Animation.slide_in_from_left, Resource.Animation.slide_out_to_right);
                } else {
                    ft.SetCustomAnimations(Resource.Animation.slide_in_from_right, Resource.Animation.slide_out_to_left, Resource.Animation.slide_in_from_left, Resource.Animation.slide_out_to_right);
                    ft.SetCustomAnimations(Resource.Animation.none, Resource.Animation.none);
                }
                
                //ft.SetCustomAnimations(Resource.Animation.SlideInFromBottom, Resource.Animation.FadeOutFast, Resource.Animation.FadeInFast, Resource.Animation.SlideOutBottom);
                ft.Replace(Android.Resource.Id.Content, this)
                    .AddToBackStack(this.UnqueId)
                    .Commit();
            }
        }

        public void ShowOver(bool animated = true) {
            if (!this.AboutToBeShown && !this.IsAdded) {
                this.AboutToBeShown = true;
                FragmentTransaction ft = AppCompatActivity.SupportFragmentManager.BeginTransaction();
                if (animated) {
                    ft.SetCustomAnimations(Resource.Animation.fade_in_fast, Resource.Animation.fade_out_fast, Resource.Animation.fade_in_fast, Resource.Animation.fade_out_fast);
                } else {
                    ft.SetCustomAnimations(Resource.Animation.none, Resource.Animation.fade_out_fast, Resource.Animation.none, Resource.Animation.fade_out_fast);
                }
                
                ft.Add(Android.Resource.Id.Content, this)
                    .AddToBackStack(this.UnqueId)
                    .Commit();
            }
        }


        public async Task ShowAsync(bool animated = true) {
            dismissedTaskCompletionSource = new TaskCompletionSource<bool>();
            if (!this.AboutToBeShown && !this.IsAdded) {
                try {
                    Show(animated);
                    await dismissedTaskCompletionSource.Task;
                } finally {
                    dismissedTaskCompletionSource = null;
                }
            }
        }

        public async Task ShowOverAsync(bool animated = true) {
            dismissedTaskCompletionSource = new TaskCompletionSource<bool>();
            if (!this.AboutToBeShown && !this.IsAdded) {

                try {
                    ShowOver(animated);
                    await dismissedTaskCompletionSource.Task;
                } finally {
                    dismissedTaskCompletionSource = null;
                }
            }
        }





        public virtual void RefreshUILocale() { }


        public IEnumerable<T> FindViewsOfTypeInTree<T>() where T : class {
            using (new DebugHelper.Timer()) {
                return this.View.FindViewsOfTypeInTree<T>();
            }
        }




        public virtual void Dismiss() {
            if (!Closing) {
                Closing = true;
                FragmentTransaction ft = AppCompatActivity.SupportFragmentManager.BeginTransaction();
                ft.Remove(this).Commit();
                AppCompatActivity.SupportFragmentManager.PopBackStack(UnqueId, FragmentManager.PopBackStackInclusive);
            }
        }





        public override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);

        }

        public TViewModel ViewModel { get; private set; }

        public bool ViewCreated { get; private set; } = false;

        public bool Visible => this.Visible;

        public virtual void Prepare(TViewModel model) {
            if (this.ViewModel != null) {
                throw new Exception("Prepare should only be run once");
            }
            this.ViewModel = model;
        }

        public override void OnActivityCreated(Bundle savedInstanceState) {
            base.OnActivityCreated(savedInstanceState);
            ViewCreated = true;
            ViewModel?.ViewCreated();
        }

        public override void OnStart() {
            base.OnStart();
            ViewModel?.ViewAppearing();
        }



        public override void OnPause() {
            base.OnPause();
            ViewModel?.ViewDisappearing();
        }

        public override void OnStop() {
            base.OnStop();
            ViewModel?.ViewDisappeared();
        }

        public override void OnDestroy() {
            base.OnDestroy();
            Closing = true;
            ViewModel?.ViewDestroy();
        }

        public virtual void OnFirstOnResume() {
        }

        public override void OnResume() {
            base.OnResume();
            AboutToBeShown = false;
            Closing = false;
            if (first) {
                first = false;
                OnFirstOnResume();
            }
            RefreshUILocale();
            ResumeTime = DateTime.Now;
            ViewModel?.ViewAppeared();
        }
        public override void OnDestroyView() {
            base.OnDestroyView();
            ViewCreated = false;
            AboutToBeShown = false;
            dismissedTaskCompletionSource?.TrySetResult(true);
        }

        public override void OnLowMemory() {
            base.OnLowMemory();
            ViewModel.OnLowMemory();
            GC.Collect(); //Shouldn't have to do this, should be done automaticall but heap keeps growing until OOM
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
        /// <param name="textView"></param>
        /// <param name="binding"></param>
        /// <returns>Returns action, so it can be unbound later</returns>
        public Action<string> BindText(TextView textView, Expression<Func<TViewModel, string>> binding) {
            return ViewModel.Bind(value => textView.Text = value, binding);
        }

        /// <summary>
        /// Binds the visiblity of a view to a view model property
        /// </summary>
        /// <param name="view"></param>
        /// <param name="binding"></param>
        /// <returns></returns>
        public Action<bool> BindVisiblitiy(View view, Expression<Func<TViewModel, bool>> binding) {
            return ViewModel.Bind(value => view.Visibility = value ? ViewStates.Visible : ViewStates.Invisible, binding);
        }

        /// <summary>
        /// Binds views alpha to view model property
        /// </summary>
        /// <param name="view"></param>
        /// <param name="binding"></param>
        /// <returns></returns>
        public Action<float> BindAlpha(View view, Expression<Func<TViewModel, float>> binding) {
            return ViewModel.Bind(value => view.Alpha = value, binding);
        }


        public EventHandler BindClick(View view, Func<TViewModel, EventHandler> binding) {
            var action = binding.Invoke(ViewModel);
            view.Click += action;
            BoundClickActions.Add(view, action);
            return action;
        }

        Dictionary<View, EventHandler> BoundClickActions = new Dictionary<View, EventHandler>();

        public void Unbind(View view, EventHandler binding) {
            view.Click -= binding;
            BoundClickActions.Remove(view);
        }




        /// <summary>
        /// Unbinds all property bound to specified action
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="actionReference"></param>z
        public void Unbind<T>(Action<T> actionReference) {
            ViewModel.Unbind(actionReference);
        }



        /// <summary>
        /// Removes all bindings
        /// </summary>
        public void UnbindAll() {
            UnbindAllClicks();
            ViewModel.UnbindAll();
        }

        public void UnbindAllClicks() {
            foreach (var clickAction in BoundClickActions) {
                clickAction.Key.Click -= clickAction.Value;
            }
        }
    }




}