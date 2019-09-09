using System;
using System.Threading.Tasks;

namespace CrossLibrary.Interfaces {
    public interface ICrossView : IDisposable{
        Task ShowOverAsync();
        Task ShowAsync();

        void ShowOver();
        void Show();

        void Dismiss();

        void RefreshUILocale();

    }
    public interface ICrossView<TParameter> : ICrossView where TParameter : CrossViewModel {
        TParameter ViewModel { get; }
        void Prepare(TParameter parameter);
    }

}
