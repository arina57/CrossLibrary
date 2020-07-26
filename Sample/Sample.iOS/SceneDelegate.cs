using System;
using CrossLibrary.Core;
using Foundation;
using UIKit;

namespace NewSingleViewTemplate {
    [Register("SceneDelegate")]
    public class SceneDelegate : CrossLibrary.iOS.CrossSceneDelegate {



        protected override CrossApp CrossApp => new Sample.Core.SampleCrossApp();

    
    }
}
