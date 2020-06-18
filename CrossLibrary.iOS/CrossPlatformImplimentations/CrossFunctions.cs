using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CrossLibrary.Dependency;
using CrossLibrary.Interfaces;
using CrossLibrary.iOS;
using Foundation;
using SharedLibrary.iOS.InterfaceImplimentations;
using UIKit;

[assembly: CrossDependency(typeof(CrossFunctions))]
namespace SharedLibrary.iOS.InterfaceImplimentations {

    /// <summary>
    /// iOS Implimentation of ICrossFunctions
    /// </summary>
    public class CrossFunctions : ICrossFunctions {
        public string GetBundleName() {
            return NSBundle.MainBundle.BundleIdentifier;
        }

        public void GoToTopPage() {
            PlatformFunctions.GetNavigationController()?.PopToRootViewController(true);
        }


        public CultureInfo GetDefaultCulture() {
            return PlatformFunctions.GetFormattingLanguage();
        }



        public string GetLocalDatabaseFilePath(string databaseFileName) {
            string docFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string libFolder = Path.Combine(docFolder, "..", "Library", "Databases");

            if (!Directory.Exists(libFolder)) {
                Directory.CreateDirectory(libFolder);
            }

            return Path.Combine(libFolder, databaseFileName);
        }


        const int longDelay = 3500;
        const int shortDelay = 2000;


        public void ShowMessageLong(string message) {
            PlatformFunctions.ShowToast(message, longDelay);
        }
        public void ShowMessageShort(string message) {
            PlatformFunctions.ShowToast(message, shortDelay);
        }


        public ICrossView GetCrossView(Type implementorType, string storyBoardId, string storyboardName) {
            if (string.IsNullOrWhiteSpace(storyBoardId)) {
                return Activator.CreateInstance(implementorType) as ICrossView;
            } else {
                storyboardName = string.IsNullOrWhiteSpace(storyboardName) ? "Main" : storyboardName;
                var storyboard = UIStoryboard.FromName(storyboardName, null);
                return storyboard.InstantiateViewController(storyBoardId) as ICrossView;
            }
        }

        public ICrossView GetCrossView(Type implementorType, string storyBoardId) {
            return GetCrossView(implementorType, storyBoardId, string.Empty);
        }

        public ICrossView GetCrossView(Type implementorType) {
            return GetCrossView(implementorType, string.Empty, string.Empty);
        }
    }
}