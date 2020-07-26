using System;
namespace Sample.Core.ViewModels {
    public class FirstViewModel : CrossLibrary.CrossViewModel {
        public FirstViewModel() {
        }

        public string LabelText => "Hello world";
        public string ButtonText => "SecondView";

        public void Button_Clicked(object sender, EventArgs e) {
            var secondView = new SecondViewModel();
            secondView.Show();
        }
    }
}
