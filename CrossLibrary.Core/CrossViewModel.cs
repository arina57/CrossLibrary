using CrossLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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

        public Task ViewCreatedTask { get; private set; }

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

        private void RemoveView() {
            crossView?.Dismiss();
            //crossView?.Dispose();
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
                RefreshBindings();
            }
        }

        public void Show(bool animated = true) {
            CrossView.Show(animated);
            //CrossView.ShowAsync();
        }

        public async Task ShowAsync(bool animated = true) {
            await CrossView.ShowAsync(animated);
        }

        public void ShowOver(bool animated = true) {
            //CrossView.ShowOver();
            CrossView.ShowOverAsync(animated);
        }

        public async Task ShowOverAsync(bool animated = true) {
            await CrossView.ShowOverAsync(animated);
        }

        public virtual void ViewDestroy() {
            CrossView.UnbindAllClicks();
            RemoveSubViews();
            RemoveView();
            UnbindAll();
            
        }


        private void RemoveSubViews() {
            foreach (var container in containerViewCache.Values) {
                //container?.SubCrossViewModel?.Dismiss();
                container?.SubCrossViewModel?.RemoveView();
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
            //code will continue here without waiting for ViewCreatedAsync to complete
            ViewCreatedTask = ViewCreatedAsync();
        }
        /// <summary>
        /// This may not be finished before other lifecycle methods are run.
        /// If you need this to be done before doing something in another
        /// lifecycle method the await ViewCreatedTask
        /// </summary>
        /// <returns></returns>
        public virtual Task ViewCreatedAsync() {
            return Task.CompletedTask;
        }

        public virtual void ViewFirstCreated() {
        }

        public void ViewDisposed() {
            crossView = null;
        }

        public void OnLowMemory() {
            
        }

        /// <summary>
        /// Perform actions bound to that property
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        public void ProperyChanged<T>(Expression<Func<T>> property) {
            var memberExpression = property.Body as MemberExpression;
            var propertyName = (memberExpression.Member as PropertyInfo).Name;
            
            //Find matching actions for item
            var matchingActions = actions.Where(item => item.Item1 == propertyName && item.Item2 is Action<T> && item.Item3 is Func<T>);
            foreach (var item in matchingActions) {
                var value = (item.Item3 as Func<T>).Invoke();
                var action = item.Item2 as Action<T>;
                action.Invoke(value);
            }
        }

        /// <summary>
        /// Bind action to view model property.
        /// eg Bind(value => TextView.Text = value, viewModel => viewModel.TextValue)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TViewModel"></typeparam>
        /// <param name="action"></param>
        /// <param name="binding"></param>
        /// <returns></returns>
        public Action<T> Bind<T, TViewModel>(Action<T> action, Expression<Func<TViewModel, T>> binding) where TViewModel : CrossViewModel {
            

            if (this is TViewModel viewModel) {
                var propertyName = GetPropertyName(binding);
                //Compile expression on binding, compiling is expensive so do it as little as possible.
                var compiledBinding = binding.Compile();
                Func<T> convertedBinding = () => compiledBinding.Invoke(viewModel);
                var actionPair = (propertyName, action, convertedBinding);
                //remove action if it is already bound to the same property
                actions.RemoveAll(item => (item.Item2 as Action<T>) == action && item.Item1 == propertyName);
                actions.Add(actionPair);

                //assign on bind.
                var value = convertedBinding.Invoke();
                action.Invoke(value);

                return action;
            } else {
                throw new Exception("ViewModel must be of the same type as TViewModel");
            }


        }

        

        /// <summary>
        /// Unbind all properties from matching action.
        /// Must be reference to originally bound Action.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="actionRefernce"></param>
        public void Unbind<T>(Action<T> actionRefernce) {
            actions.RemoveAll(a => (a.Item2 as Action<T>) == (actionRefernce));
        }


        /// <summary>
        /// Unbind action for property
        /// Must be reference to originally bound Action.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TViewModel"></typeparam>
        /// <param name="actionRefernce"></param>
        /// <param name="binding"></param>
        public void Unbind<T, TViewModel>(Action<T> actionRefernce, Expression<Func<TViewModel, T>> binding) where TViewModel : CrossViewModel {
            var propertyName = GetPropertyName(binding);
            actions.RemoveAll(a => (a.Item2 as Action<T>) == (actionRefernce) && a.Item1 == propertyName);
        }

        /// <summary>
        /// Unbind all action for given property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TViewModel"></typeparam>
        /// <param name="binding"></param>
        public void Unbind<T, TViewModel>(Expression<Func<TViewModel, T>> binding) where TViewModel : CrossViewModel {
            var propertyName = GetPropertyName(binding);
            actions.RemoveAll(a => a.Item1 == propertyName);
        }

        /// <summary>
        /// Actions and property.
        /// Property name, binding Action<T>, compiled Func<T>
        /// </summary>
        List<(string, object, object)> actions = new List<(string, object, object)>();


        /// <summary>
        /// Unbinds all bound actions and properties
        /// </summary>
        public void UnbindAll() {
            actions.Clear();
        }

        /// <summary>
        /// Gets the string name for a property
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="binding"></param>
        /// <returns></returns>
        private static string GetPropertyName<TViewModel, T>(Expression<Func<TViewModel, T>> binding) where TViewModel : CrossViewModel {
            var body = binding.Body;
            if(body is MemberExpression memberExpression) {
                var propertyName = (memberExpression.Member as PropertyInfo).Name;
                return propertyName;
            } else {
                throw new Exception("must be a property");
            }
            
        }

        public void RefreshBindings() {
            foreach (var item in actions) {
                var value = (item.Item3 as dynamic).Invoke();
                var action = item.Item2 as dynamic;
                action.Invoke(value);
            }
        }
    }
}
