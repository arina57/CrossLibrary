using System;
using Sample.Core.ViewModels;
using UIKit;

namespace Sample.iOS.Views {
    public partial class SecondView : CrossLibrary.iOS.Views.CrossUIViewController<SecondViewModel> {
        public SecondView() : base("SecondView", null) {
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

