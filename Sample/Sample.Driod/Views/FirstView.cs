
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Sample.Core.ViewModels;

namespace Sample.Driod.Views {
    public class FirstView : CrossLibrary.Droid.Views.CrossFragment<FirstViewModel> {
        public override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);
            var view = inflater.Inflate(Resource.Layout.first_view, container, false);

            var label = view.FindViewById<TextView>(Resource.Id.label);
            var button = view.FindViewById<Button>(Resource.Id.button);

            BindText(label, vm => vm.LabelText);
            BindText(button, vm => vm.ButtonText);
            BindClick(button, vm => vm.Button_Clicked);

            return view;
        }
    }
}
