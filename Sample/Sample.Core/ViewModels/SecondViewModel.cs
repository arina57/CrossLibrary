using System;
namespace Sample.Core.ViewModels {
    public class SecondViewModel : CrossLibrary.CrossViewModel {

        private string labelText = "Second View";
        public string LabelText {
            get => labelText;
            set {
                labelText = value;
                ProperyChanged(() => LabelText);
            }
        }
        
        private string buttonText = "Click me";
        public string ButtonText {
            get => buttonText;
            set {
                buttonText = value;
                ProperyChanged(() => ButtonText);
            }
        }

        private int clicks = 0;

        public void Button_Clicked(object sender, EventArgs e) {
            clicks++;
            ButtonText = $"{clicks} Clicks";
        }

        public SecondViewModel() {
            
        }
    }
}
