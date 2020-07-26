using System;
using Android.OS;
using Android.Views;
using Android.Widget;
using Sample.Core.ViewModels;

namespace Sample.Driod.Views {
    public class SecondView : CrossLibrary.Droid.Views.CrossFragment<SecondViewModel> {
        public override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
            var view = inflater.Inflate(Resource.Layout.second_view, container, false);

            var button = view.FindViewById<Button>(Resource.Id.button);
            var label = view.FindViewById<TextView>(Resource.Id.label);

            BindText(button, vm => vm.ButtonText);
            BindClick(button, vm => vm.Button_Clicked);
            BindText(label, vm => vm.LabelText);


            return view;
        }


        public SecondView() {
        }


    }
}
