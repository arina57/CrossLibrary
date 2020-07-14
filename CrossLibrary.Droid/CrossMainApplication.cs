using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using CrossLibrary.Core;
using Plugin.CurrentActivity;

namespace CrossLibrary.Droid {
    
    public abstract class CrossMainApplication : Application, Application.IActivityLifecycleCallbacks {
        public CrossMainApplication(IntPtr handle, JniHandleOwnership transer)
          : base(handle, transer) {
        }

        public override void OnCreate() {
            base.OnCreate();
            RegisterActivityLifecycleCallbacks(this);

            //A great place to initialize Xamarin.Insights and Dependency Services!
        }

        protected abstract CrossApp CrossApp { get; }


        public override void OnTerminate() {
            base.OnTerminate();
            UnregisterActivityLifecycleCallbacks(this);
        }

        public virtual void OnActivityCreated(Activity activity, Bundle savedInstanceState) {
            CrossCurrentActivity.Current.Init(activity, savedInstanceState);
            CrossApp.AppLoaded();
        }

        public virtual void OnActivityDestroyed(Activity activity) {

        }

        public virtual void OnActivityPaused(Activity activity) {
            CrossApp.AppLostFocus();
        }

        public virtual void OnActivityResumed(Activity activity) {
            CrossApp.AppFocused();
        }

        public virtual void OnActivitySaveInstanceState(Activity activity, Bundle outState) {
        }

        public virtual void OnActivityStarted(Activity activity) {
        }

        public virtual void OnActivityStopped(Activity activity) {
        }
    }
}
