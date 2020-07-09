using System;
namespace CrossLibrary.Core {
    public abstract class CrossApp {
        public CrossApp() {
        }

        public virtual void AppLoaded() {

        }

        public virtual void AppLostFocus() {

        }

        public virtual void AppFocused() {

        }
    }
}
