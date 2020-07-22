using System;
using Android.App;
using Android.OS;
using AndroidX.AppCompat.App;
using static CrossLibrary.Droid.Enums;

namespace CrossLibrary.Droid.Views {
    public abstract class CrossActivity : AppCompatActivity, ICrossActivity {
		public ActivityBackAction BackAction { get; set; } =  ActivityBackAction.Normal;

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            

            SetContentView(Resource.Layout.default_activity_layout);


        }

        public override void OnBackPressed() {
            switch (BackAction) {
                case ActivityBackAction.Normal:
                    base.OnBackPressed();
                    break;
                case ActivityBackAction.CloseActivity:
                    Finish();
                    break;
                case ActivityBackAction.None:
                    break;
            }
        }
    }
}
