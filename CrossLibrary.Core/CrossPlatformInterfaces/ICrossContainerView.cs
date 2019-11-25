using CrossLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrossLibrary.Interfaces {
    public interface ICrossContainerView {

        CrossViewModel SubCrossViewModel { get; }
        void ShowView<TViewModel>(TViewModel crossViewModel) where TViewModel : CrossViewModel;
        void RemoveView();
        void RemoveAllViews();
        string ContainerId { get; }
        void SuperCrossViewAppearing();
    }
}
