using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using CrossLibrary.Interfaces;
using Plugin.CurrentActivity;
using AndroidX.Fragment.App;
using AndroidX.AppCompat.App;

namespace CrossLibrary.Droid.Views {
    [Register("crosslibrary.droid.views.CrossContainerView")]
    public class CrossContainerView : FrameLayout, ICrossContainerView {

        public CrossViewModel SubCrossViewModel { get; private set; }
        public string ContainerId { get; private set; }


        public CrossContainerView(Context context, IAttributeSet attrs) :
            base(context, attrs) {
            Init(attrs);
        }

        public CrossContainerView(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle) {
            Init(attrs);
        }

        public void ShowView<TViewModel>(TViewModel crossViewModel) where TViewModel : CrossViewModel {
            RemoveView();
            //RemoveAllViews();
            SubCrossViewModel = crossViewModel;
            if (crossViewModel.CrossView == null) {
                throw new Exception($"CrossView is null");
            } else if (crossViewModel.CrossView is Fragment fragment) {
                FragmentTransaction ft = ((AppCompatActivity)CrossCurrentActivity.Current.Activity).SupportFragmentManager.BeginTransaction();
                ft.SetCustomAnimations(Resource.Animation.fade_in_fast, Resource.Animation.fade_out_fast, Resource.Animation.fade_in_fast, Resource.Animation.fade_out_fast);
                ft.Add(this.Id, fragment);
                ft.Commit();
            } else {
                throw new Exception($"No case for crossview of that type");
            }

        }

        public void RemoveView() {
            SubCrossViewModel?.Dismiss();
            SubCrossViewModel = null;
        }

        public override void RemoveAllViews() {
            RemoveView();
        }

        /// <summary>
        /// Init with custom XML properties
        /// </summary>
        /// <param name="attrs"></param>
        private void Init(IAttributeSet attrs) {
            if (attrs != null) {
                TypedArray attrsArray = Context.ObtainStyledAttributes(attrs, Resource.Styleable.CrossContainerView);
                var containerId = attrsArray.GetString(Resource.Styleable.CrossContainerView_containerId);
                if (containerId != null) {
                    ContainerId = containerId;
                }
            }
        }

        public void SuperCrossViewAppearing() {
        }
    }
}