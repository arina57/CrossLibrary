using System;
using AndroidX.AppCompat.App;
using static CrossLibrary.Droid.Enums;

namespace CrossLibrary.Droid.Views {
    public class CrossActivity : AppCompatActivity, ICrossActivity {
		public ActivityBackAction BackAction { get; set; } =  ActivityBackAction.Normal;



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
