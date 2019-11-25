using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CrossLibrary.Droid.Views;
using CrossLibrary.Interfaces;

namespace CrossLibrary.Droid {
    public static class PlatformFunctions {
        public static List<View> GetAllSubViewsInTree(this ViewGroup viewGroup) {
            var views = new List<View>();
            var childCount = viewGroup.ChildCount;
            for (int i = 0; i < childCount; i++) {
                var view = viewGroup.GetChildAt(i);
                views.Add(view);
                if (view is ViewGroup subViewGroup) {
                    views.AddRange(subViewGroup.GetAllSubViewsInTree());
                }
            }
            return views;
        }

        public static IEnumerable<T> FindViewsOfTypeInTree<T>(this View view) where T : class{
            return view.GetSubViews().RecursiveSelect(v => v.GetSubViews()).Where(v => v is T).Select(v => v as T);
        }


        public static IEnumerable<View> GetSubViews(this View view) {
            if ( view is ViewGroup viewGroup) {
                var childCount = viewGroup.ChildCount;
                for (int i = 0; i < childCount; i++) {
                    var subView = viewGroup.GetChildAt(i);
                    yield return subView;
                }
            } else {
                yield break;
            }
            
        }

        public static void Hidden(this View view, bool hidden) {
            view.Visibility = hidden ? ViewStates.Invisible : ViewStates.Visible;
        }

        public static void Gone(this View view, bool gone) {
            view.Visibility = gone ? ViewStates.Gone : ViewStates.Visible;
        }
    }
}