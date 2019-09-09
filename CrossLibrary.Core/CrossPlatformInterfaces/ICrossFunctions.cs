using System;
using System.Globalization;
using System.Threading.Tasks;


namespace CrossLibrary.Interfaces {

    /// <summary>
    /// Misc cross platform functions
    /// </summary>
    public interface ICrossFunctions {

        string GetBundleName();
        void GoToTopPage();

        string GetLocalDatabaseFilePath(string databaseFileName);

        CultureInfo GetDefaultCulture();

        void ShowMessageLong(string message);
        void ShowMessageShort(string message);

        ICrossView GetCrossView(Type implementorType, string storyBoardId, string storyboardName);
        ICrossView GetCrossView(Type implementorType, string storyBoardId);
        ICrossView GetCrossView(Type implementorType);
    }
}
