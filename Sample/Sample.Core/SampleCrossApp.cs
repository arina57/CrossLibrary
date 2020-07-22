using System;
using Sample.Core.ViewModels;

namespace Sample.Core {
    public class SampleCrossApp : CrossLibrary.Core.CrossApp  {
        public override void AppLoaded() {
            var firstView = new FirstViewModel();
            firstView.Show();
        }

    }

}
