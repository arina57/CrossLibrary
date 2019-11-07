using System;


namespace CrossLibrary.Dependency { 
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public sealed class CrossViewAttribute : Attribute
	{
		public CrossViewAttribute(Type implementorType, string storyBoardIdentifier = "", string storyboardName = "", string id = "")
		{
            DependencyInfo = new CrossViewImplementorInfo(implementorType, storyBoardIdentifier, storyboardName, id);
        }

        internal CrossViewImplementorInfo DependencyInfo { get; private set; }

    }
}