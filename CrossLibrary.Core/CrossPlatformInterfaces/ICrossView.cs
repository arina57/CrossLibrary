using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CrossLibrary.Interfaces {
    public interface ICrossView : IDisposable{
        Task ShowOverAsync(bool animated = true);
        Task ShowAsync(bool animated = true);

        void ShowOver(bool animated = true);
        void Show(bool animated = true);
        void Dismiss();

        void RefreshUILocale();
        bool ViewCreated { get; }
        bool Visible { get; }
        IEnumerable<T> FindViewsOfTypeInTree<T>() where T : class;
        void UnbindAllClicks();
    }
    public interface ICrossView<TParameter> : ICrossView where TParameter : CrossViewModel {
        TParameter ViewModel { get; }
        void Prepare(TParameter viewModel);
    }

}
