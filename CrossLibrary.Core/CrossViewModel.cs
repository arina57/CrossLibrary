using CrossLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CrossLibrary {
    public class CrossViewModel {
        public bool HasCrossView => crossView != null;
        protected virtual string ViewClassId { get; set; } = string.Empty;
        internal ICrossView crossView;
        public ICrossView CrossView {
            get {
                if (crossView == null) {
                    //trys to find the appropriate View that goes with this view model
                    crossView = Dependency.CrossViewDependencyService.CreateCrossView(this, id: ViewClassId); 
                }
                return crossView;
            }
        }

        private Dictionary<string, ICrossContainerView> containerViewCache = new Dictionary<string, ICrossContainerView>();




        public ICrossContainerView FindCrossContainerView(string containerId) {
            if (!containerViewCache.ContainsKey(containerId)) {
                var container = CrossView.FindViewsOfTypeInTree<ICrossContainerView>().FirstOrDefault(v => v.ContainerId == containerId);
                //if (container == null) {
                //    throw new Exception($"Container view with Id {containerId} could not be found");
                //}
                containerViewCache[containerId] = container;
            }
            return containerViewCache[containerId];
        }


        public void Dismiss() {
            crossView?.Dismiss();
        }

        private void DisposeView() {
            crossView?.Dispose();
            crossView = null;

        }

        public virtual void RefreshUILocale() {
            crossView?.RefreshUILocale();
        }

        public void Show() {
            //CrossView.Show();
            CrossView.ShowAsync();
        }

        public async Task ShowAsync() {
            await CrossView.ShowAsync();
        }

        public void ShowOver() {
            //CrossView.ShowOver();
            CrossView.ShowOverAsync();
        }

        public async Task ShowOverAsync() {
            await CrossView.ShowOverAsync();
        }

        public virtual void ViewDestroy() {
            foreach (var container in containerViewCache.Values) {
                container?.SubCrossViewModel?.Dismiss();
            }
            containerViewCache.Clear();
            DisposeView();

        }

        public virtual void ViewDisappeared() {
        }

        public virtual void ViewDisappearing() {
        }

        public virtual void ViewAppeared() {
        }

        public virtual void ViewAppearing() {
            RefreshUILocale();
        }

        public virtual void ViewCreated() {
        }
    }
}
