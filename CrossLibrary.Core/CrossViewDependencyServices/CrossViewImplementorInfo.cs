using System;
namespace CrossLibrary.Dependency {
    internal class CrossViewImplementorInfo {
        internal Type Implementor { get; private set; }
        internal string StoryBoardIdentifier { get; private set; }
        internal string StoryBoardName { get; private set; }

        public CrossViewImplementorInfo(Type implementorType, string storyBoardIdentifier = "", string storyboardName = "") {
            Implementor = implementorType;
            StoryBoardIdentifier = storyBoardIdentifier;
            StoryBoardName = storyboardName;
        }
    }
}
