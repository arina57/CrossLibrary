using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using CrossLibrary.Interfaces;
using Plugin.CurrentActivity;

namespace CrossLibrary.Droid.Views {
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

        /// <summary>
        /// These will be disposed when vuew has been popped
        /// </summary>
        protected List<IDisposable> DisposableObjects = new List<IDisposable>();

        //protected AppCompatActivityExtra AppCompatActivityExtra => Activity as AppCompatActivityExtra;
        /// <summary>
        /// Actions to unsubscribe events on destroy
        /// </summary>
        protected List<Action> eventUnsubscritionActions = new List<Action>();
        /// <summary>
        /// IDisposable to dispose on destroy
        /// </summary>
        protected List<IDisposable> disposableObjects = new List<IDisposable>();


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


        public void Show() {
            if (!this.AboutToBeShown && !this.IsAdded) {
                this.AboutToBeShown = true;
                FragmentTransaction ft = AppCompatActivity.SupportFragmentManager.BeginTransaction();
                ft.SetCustomAnimations(Resource.Animation.slide_in_from_right, Resource.Animation.slide_out_to_left, Resource.Animation.slide_in_from_left, Resource.Animation.slide_out_to_right);
                //ft.SetCustomAnimations(Resource.Animation.SlideInFromBottom, Resource.Animation.FadeOutFast, Resource.Animation.FadeInFast, Resource.Animation.SlideOutBottom);
                ft.Replace(Android.Resource.Id.Content, this)
                    .AddToBackStack(this.UnqueId)
                    .Commit();
            }
        }

        public void ShowOver() {
            if (!this.AboutToBeShown && !this.IsAdded) {
                this.AboutToBeShown = true;
                FragmentTransaction ft = AppCompatActivity.SupportFragmentManager.BeginTransaction();
                ft.SetCustomAnimations(Resource.Animation.fade_in_fast, Resource.Animation.fade_out_fast, Resource.Animation.fade_in_fast, Resource.Animation.fade_out_fast);
                ft.Add(Android.Resource.Id.Content, this)
                    .AddToBackStack(this.UnqueId)
                    .Commit();
            }
        }


        public async Task ShowAsync() {
            dismissedTaskCompletionSource = new TaskCompletionSource<bool>();
            if (!this.AboutToBeShown && !this.IsAdded) {
                try {
                    Show();
                    await dismissedTaskCompletionSource.Task;
                } finally {
                    dismissedTaskCompletionSource = null;
                }
            }
        }

        public async Task ShowOverAsync() {
            dismissedTaskCompletionSource = new TaskCompletionSource<bool>();
            if (!this.AboutToBeShown && !this.IsAdded) {

                try {
                    ShowOver();
                    await dismissedTaskCompletionSource.Task;
                } finally {
                    dismissedTaskCompletionSource = null;
                }
            }
        }





        public abstract void RefreshUILocale();





        public virtual void Dismiss() {
            if (!Closing) {
                Closing = true;
                FragmentTransaction ft = AppCompatActivity.SupportFragmentManager.BeginTransaction();
                ft.Remove(this).Commit();
                AppCompatActivity.SupportFragmentManager.PopBackStack(UnqueId, FragmentManager.PopBackStackInclusive);
            }
        }

        //public virtual void Remove() {
        //    FragmentTransaction ft = PlatformGlobal.CurrentActivityExtra.SupportFragmentManager.BeginTransaction();
        //    ft.Remove(this).Commit();
        //    PlatformGlobal.CurrentActivityExtra.SupportFragmentManager.PopBackStack(UnqueId, FragmentManager.PopBackStackInclusive);
        //}


        protected override void Dispose(bool disposing) {
            foreach (var disposeObject in DisposableObjects) {
                if (disposeObject is ImageView imageView) {
                    imageView.Drawable.Dispose();
                    imageView.SetImageDrawable(null);
                }
                disposeObject?.Dispose();
            }
            DisposableObjects?.Clear();
            base.Dispose(disposing);
        }

        public override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);

        }

        public TViewModel ViewModel { get; private set; }
        public virtual void Prepare(TViewModel model) {
            if (this.ViewModel != null) {
                throw new Exception("Prepare should only be run once");
            }
            this.ViewModel = model;
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
            eventUnsubscritionActions.ForEach(action => action.Invoke());
            disposableObjects.ForEach(disposableObject => disposableObject.Dispose());
            //GC.Collect(); //Shouldn't have to do this, should be done automaticall but heap keeps growing until OOM
            AboutToBeShown = false;
            dismissedTaskCompletionSource?.TrySetResult(true);
        }

        public override void OnLowMemory() {
            base.OnLowMemory();
            GC.Collect(); //Shouldn't have to do this, should be done automaticall but heap keeps growing until OOM
        }

    }




}