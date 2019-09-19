using CrossLibrary.Interfaces;
using System;
using System.Threading.Tasks;

namespace CrossLibrary {
    public class CrossViewModel {

        private ICrossView crossView;
        public ICrossView CrossView {
            get {
                if (crossView == null) {
                    //trys to find the appropriate View that goes with this view model
                    crossView = Dependency.CrossViewDependencyService.CreateCrossView(this); 
                }
                return crossView;
            }
        }



        public void Dismiss() {
            crossView?.Dismiss();
        }

        private void DisposeView() {
            crossView?.Dispose();
            crossView = null;
        }

        public void RefreshUILocale() {
            CrossView?.RefreshUILocale();
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
            DisposeView();
        }

        public virtual void ViewDisappeared() {
        }

        public virtual void ViewDisappearing() {
        }

        public virtual void ViewAppeared() {
        }

        public virtual void ViewAppearing() {
        }

        public virtual void ViewCreated() {
        }
    }
}
