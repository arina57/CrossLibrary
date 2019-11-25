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

        public bool Visible => crossView?.Visible ?? false;
      


        private Dictionary<string, ICrossContainerView> containerViewCache = new Dictionary<string, ICrossContainerView>();





        public ICrossContainerView FindCrossContainerView(string containerId) {
            if (!containerViewCache.ContainsKey(containerId)) {
                var matchingContainers = CrossView.FindViewsOfTypeInTree<ICrossContainerView>().Where(v => v.ContainerId == containerId);
                if (matchingContainers.Count() > 1) {
                    throw new Exception($"More than one ICrossContainerView with the id {containerId} exists.");
                }

                var container = matchingContainers.FirstOrDefault();
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

        public  async Task DismissAsync() {
            dismissedTaskCompletionSource = new TaskCompletionSource<bool>();
            
            try {
                Dismiss();
                await dismissedTaskCompletionSource.Task;
            } finally {
                dismissedTaskCompletionSource = null;
            }
        }

        private void DisposeView() {
            crossView?.Dismiss();
            crossView?.Dispose();
            crossView = null;
        }

        public virtual void RefreshUILocale() {
            if(HasCrossView && crossView.ViewCreated) {
                crossView.RefreshUILocale();
                foreach (var container in containerViewCache.Values) {
                    if (container != null && container.SubCrossViewModel != null) {
                        container.SubCrossViewModel.RefreshUILocale();
                    }
                }
            }
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
            RemoveSubViews();
            DisposeView();

        }

        private void RemoveSubViews() {
            foreach (var container in containerViewCache.Values) {
                //container?.SubCrossViewModel?.Dismiss();
                container?.SubCrossViewModel?.DisposeView();
            }
            containerViewCache.Clear();
        }

        public virtual void ViewDisappeared() {
            dismissedTaskCompletionSource?.TrySetResult(true);
        }

        public virtual void ViewDisappearing() {
        }

        public virtual void ViewAppeared() {
        }

        public virtual void ViewAppearing() {
            foreach (var container in containerViewCache.Values) {
                container?.SuperCrossViewAppearing();
            }
            RefreshUILocale();

        }

        bool viewFirstCreated = true;
        private TaskCompletionSource<bool> dismissedTaskCompletionSource;

        public virtual void ViewCreated() {
            if(viewFirstCreated) {
                viewFirstCreated = false;
                ViewFirstCreated();
            }
        }

        public virtual void ViewFirstCreated() {

        }

        public void ViewDisposed() {
            crossView = null;
        }

        public void OnLowMemory() {
            
        }
    }
}
