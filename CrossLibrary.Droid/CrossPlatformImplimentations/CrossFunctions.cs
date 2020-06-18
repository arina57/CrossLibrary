using System;
using Android.Content;
using Android.Widget;
using System.Globalization;
using Java.Util;
using System.IO;
using CrossLibrary.Interfaces;
using Plugin.CurrentActivity;
using CrossLibrary.Droid.CrossPlatformImplimentations;
using CrossLibrary.Dependency;

[assembly: CrossDependency(typeof(CrossFunctions))]
namespace CrossLibrary.Droid.CrossPlatformImplimentations {
    /// <summary>
    /// Android Implimentation of ICrossFunctions
    /// </summary>
    public class CrossFunctions : ICrossFunctions {
        public string GetBundleName() {
            return CrossCurrentActivity.Current.Activity.PackageName;
        }

        public void GoToTopPage() {
            var intent = new Intent(CrossCurrentActivity.Current.Activity, CrossCurrentActivity.Current.Activity.GetType());
            intent.AddFlags(ActivityFlags.ClearTop);
            CrossCurrentActivity.Current.Activity.StartActivity(intent);
            //PlatformGlobal.TopActivity.StartActivity(intent);
        }

        public CultureInfo GetDefaultCulture() {
            if(string.IsNullOrWhiteSpace(Locale.Default.Language)) {
                return new CultureInfo("en");
            } else {
                try {
                    return new CultureInfo(Locale.Default.Language);
                } catch {
                    return new CultureInfo("en");
                }
            }
        }



        public string GetLocalDatabaseFilePath(string databaseFileName) {
            string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            return Path.Combine(path, databaseFileName);
        }


        public void ShowMessageLong(string message) {
            Toast.MakeText(CrossCurrentActivity.Current.Activity, message, ToastLength.Long).Show();
        }

        public void ShowMessageShort(string message) {
            Toast.MakeText(CrossCurrentActivity.Current.Activity, message, ToastLength.Short).Show();
        }



        public ICrossView GetCrossView(Type implementorType, string storyBoardId) {
            return Activator.CreateInstance(implementorType) as ICrossView;
            
        }

        public ICrossView GetCrossView(Type implementorType, string storyBoardId, string storyboardName) {
            return Activator.CreateInstance(implementorType) as ICrossView;
        }

        public ICrossView GetCrossView(Type implementorType) {
            return Activator.CreateInstance(implementorType) as ICrossView;
        }
    }
}