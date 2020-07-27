using System;
using Sample.Core.ViewModels;
using UIKit;

namespace Sample.iOS.Views {
    public partial class FirstView : CrossLibrary.iOS.Views.CrossUIViewController<FirstViewModel> {
        public FirstView() : base("FirstView", null) {
        }

        public override void ViewDidLoad() {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.

            BindText(Label, vm => vm.LabelText);
            BindText(Button, vm => vm.ButtonText);
            BindClick(Button, vm => vm.Button_Clicked);
        }

        public override void DidReceiveMemoryWarning() {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}

