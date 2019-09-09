using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using static UIKit.NSLayoutAttribute;
using static UIKit.NSLayoutRelation;
using Foundation;
using SharedLibrary.iOS.CustomControls;
using UIKit;

namespace CrossLibrary.iOS {
    public class PlatformFunctions {
        public static UINavigationController GetNavigationController() {
            if (UIApplication.SharedApplication.KeyWindow.RootViewController is UINavigationController navigationController) {
                return navigationController;
            } else {
                return UIApplication.SharedApplication.KeyWindow.RootViewController.NavigationController;
            }
        }


        public static CultureInfo GetFormattingLanguage() {
            try {
                //find if English or Japanese are in preferred languages and return the heights ranked on.
                foreach (var culture in NSLocale.PreferredLanguages) {
                    var code = culture.Substring(0, 2);
                    if (code.ToLower() == "en" || code.ToLower() == "ja") {
                        return CultureInfo.GetCultureInfo(code);
                    }
                }
                //if neither English or Japanese are a preferred langauge, return top language
                return CultureInfo.GetCultureInfo(NSLocale.PreferredLanguages[0].Substring(0, 2));
            } catch (CultureNotFoundException) {
                return CultureInfo.GetCultureInfo(NSLocale.CurrentLocale.LanguageCode);
            } catch {
                return CultureInfo.GetCultureInfo("en");
            }
        }


        public static UIViewController GetTopViewController() {
            var vc = GetTopViewController(UIApplication.SharedApplication.KeyWindow.RootViewController);
            if (vc is UINavigationController navigationController) {
                return navigationController.TopViewController;
            } else {
                return vc;
            }
        }




        public static UIViewController GetTopViewController(UIViewController viewController) {
            var presentedViewController = viewController.PresentedViewController;
            if (presentedViewController == null) {
                return viewController;
            } else {
                return GetTopViewController(presentedViewController);
            }

        }



        public static void ShowToast(string message, int duration = 1000) {
            var toastLabel = new UIPaddingLabel();
            toastLabel.Padding = 10;
            toastLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            toastLabel.BackgroundColor = UIColor.DarkGray;
            toastLabel.TextColor = UIColor.White;
            toastLabel.TextAlignment = UITextAlignment.Center;
            toastLabel.Lines = 0;
            toastLabel.Text = message;

            toastLabel.Alpha = 0.8f;
            toastLabel.Layer.CornerRadius = 20;
            toastLabel.ClipsToBounds = true;
            var containerView = UIApplication.SharedApplication.Delegate.GetWindow();
            containerView.AddSubview(toastLabel);
            containerView.AddConstraint(NSLayoutConstraint.Create(toastLabel, Left, GreaterThanOrEqual, containerView, Left, 1, 20));
            containerView.AddConstraint(NSLayoutConstraint.Create(toastLabel, Right, LessThanOrEqual, containerView, Right, 1, -20));
            containerView.AddConstraint(NSLayoutConstraint.Create(toastLabel, Bottom, Equal, containerView, Bottom, 0.4f, 0));
            containerView.AddConstraint(NSLayoutConstraint.Create(toastLabel, CenterX, Equal, containerView, CenterX, 1, 0));

            UIView.Animate(0.5, duration / 1000f, UIViewAnimationOptions.TransitionNone, () => toastLabel.Alpha = 0.0f, () => {
                toastLabel.RemoveFromSuperview();
                toastLabel.Dispose();
                toastLabel = null;
            });
        }


    }
}