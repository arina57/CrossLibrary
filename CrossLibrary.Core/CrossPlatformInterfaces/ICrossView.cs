﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CrossLibrary.Interfaces {
    public interface ICrossView : IDisposable{
        Task ShowOverAsync();
        Task ShowAsync();

        void ShowOver();
        void Show();
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
