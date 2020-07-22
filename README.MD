# CrossLibrary

CrossLibrary is a library to help with MVVM in Xamarin.Android and Xamarin.iOS

---

## Installation

Add to your Xamarin.Android, Xamarin.iOS and 
### Nuget:
```
nuget install com.arina57.crosslibrary
```
### Git
```
git clone https://github.com/arina57/CrossLibrary.git
```
---

## Setup

In your shared project create a class that inherits `CrossLibrary.Core.CrossApp`

```C#
    public class MyCrossApp : CrossLibrary.Core.CrossApp {
        public MyCrossApp() {
        }

        public override void AppLoaded() {
        }
    }
```

## Android

In your Android project Create / Edit a MainApplication class that inherits from `CrossLibrary.Droid.CrossMainApplication` and create an override for `CrossApp` that points to your class above.

```C#
    [Application]
    public class MainApplication : CrossLibrary.Droid.CrossMainApplication {
        public MainApplication(IntPtr handle, JniHandleOwnership transer) 
            : base(handle, transer) {

        }

        protected override CrossApp CrossApp => new Core.MyCrossApp();
    }
```

Create / Edit a the main launcher activity to inherit from `CrossLibrary.Droid.Views.CrossActivity`
Or `CrossLibrary.Droid.Views.CrossActivity` can directly be used as the main launcher in the manifest.

```C#
[Activity(Label = "@string/app_name", Theme = "@style/splashScreenTheme", MainLauncher = true)]
    public class MainActivity : CrossLibrary.Droid.Views.CrossActivity {

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetTheme(Resource.Style.AppTheme_NoActionBar);
            SetContentView(CrossLibrary.Droid.Resource.Layout.default_activity_layout);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults) {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
```

## iOS

Create / Edit SceneDelegate that inherits from `CrossLibrary.iOS.CrossApp`

```C#
    [Register("SceneDelegate")]
    public class SceneDelegate : CrossSceneDelegate {
        protected override CrossApp CrossApp => new Core.MyCrossApp();
    }
```

