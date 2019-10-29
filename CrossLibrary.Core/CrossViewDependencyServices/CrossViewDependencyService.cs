using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xamarin.Forms.Internals;
using Xamarin.Forms;
using CrossLibrary;
using CrossLibrary.Interfaces;

namespace CrossLibrary.Dependency {
    public static class CrossViewDependencyService {
        static bool initialized;
        private static IEnumerable<Type> crossViews;
        static readonly object dependencyLock = new object();
        static readonly object initializeLock = new object();

        static readonly List<CrossViewImplementorInfo> dependencyTypes = new List<CrossViewImplementorInfo>();
        static readonly Dictionary<Type, DependencyData> dependencyImplementations = new Dictionary<Type, DependencyData>();

        public enum DependencyFetchTarget {
            GlobalInstance,
            NewInstance
        }

        public static ICrossView CreateCrossView(CrossViewModel viewModel, DependencyFetchTarget fetchTarget = DependencyFetchTarget.NewInstance) {
            Initialize();
            Type viewModelType = viewModel.GetType();
            DependencyData dependencyImplementation;
            lock (dependencyLock) {
                Type targetType = typeof(ICrossView<>).MakeGenericType(viewModelType);
                dependencyImplementation = GetDependencyImplementation(targetType);
            }

            if (dependencyImplementation == null) {
                return null;
            }

            dynamic crossView;
            if (fetchTarget == DependencyFetchTarget.GlobalInstance && dependencyImplementation.GlobalInstance != null) {
                return dependencyImplementation.GlobalInstance;
            }

            if (string.IsNullOrWhiteSpace(dependencyImplementation.StoryBoardIdentifier)) {
                crossView = Activator.CreateInstance(dependencyImplementation.ImplementorType);
            } else {
                crossView = CommonFunctions.CrossFunctions.GetCrossView(dependencyImplementation.ImplementorType,
                    dependencyImplementation.StoryBoardIdentifier,
                    dependencyImplementation.StoryBoardName);
            }
            dynamic castViewModel = Convert.ChangeType(viewModel, viewModelType);
            crossView.Prepare(castViewModel);
            if (fetchTarget == DependencyFetchTarget.GlobalInstance) {
                dependencyImplementation.GlobalInstance = crossView;
            }


            return crossView;
        }

        private static DependencyData GetDependencyImplementation(Type targetType) {
            if (!dependencyImplementations.ContainsKey(targetType)) {
                var implementorInfo = FindImplementor(targetType);
                DependencyData dependencyImplementation;
                if (implementorInfo != null) {
                    dependencyImplementation = new DependencyData(implementorInfo);
                } else {
                    using (new DebugHelper.Timer()) {
                        //Do extra search for type that wasn't registered. This shouldn't excute if it was.
                        //This is a slower search
                        var type = FindType(targetType);
                        dependencyImplementation = type != null ? new DependencyData(type) : null;
                    }
                }
                dependencyImplementations[targetType] = dependencyImplementation;
            }
            return dependencyImplementations[targetType];
        }




        public static void Register<T>() where T : class {
            Type type = typeof(T);
            if (!dependencyTypes.Any(info => info.Implementor == type)) {
                dependencyTypes.Add(new CrossViewImplementorInfo(type));
            }
        }

        public static void Register<T, TImpl>() where T : class where TImpl : class, T {
            Type targetType = typeof(T);
            Type implementorType = typeof(TImpl);
            if (!dependencyTypes.Any(info => info.Implementor == targetType)) {
                dependencyTypes.Add(new CrossViewImplementorInfo(targetType));
            }

            lock (dependencyLock) {
                dependencyImplementations[targetType] = new DependencyData { ImplementorType = implementorType };
            }
        }

        static CrossViewImplementorInfo FindImplementor(Type target) {
            return dependencyTypes.FirstOrDefault(t => target.IsAssignableFrom(t.Implementor));
        }

        static Type FindType(Type target) {
            return crossViews.FirstOrDefault(t => target.IsAssignableFrom(t));
        }

        static void Initialize() {
            if (initialized) {
                return;
            }

            lock (initializeLock) {
                if (initialized) {
                    return;
                }

                Assembly[] assemblies = Device.GetAssemblies();

                crossViews = assemblies.SelectMany(assembly => assembly.GetTypes()).Where(type => typeof(ICrossView).IsAssignableFrom(type));


                if (Registrar.ExtraAssemblies != null) {
                    assemblies = assemblies.Union(Registrar.ExtraAssemblies).ToArray();
                }

                Initialize(assemblies);
            }
        }

        internal static void Initialize(Assembly[] assemblies) {
            if (initialized) {
                return;
            }

            lock (initializeLock) {
                if (initialized) {
                    return;
                }

                Type targetAttrType = typeof(CrossViewAttribute);

                // Don't use LINQ for performance reasons
                // Naive implementation can easily take over a second to run
                foreach (Assembly assembly in assemblies) {
                    object[] attributes;
                    try {
#if NETSTANDARD2_0
						attributes = assembly.GetCustomAttributes(targetAttrType, true);
#else
                        attributes = assembly.GetCustomAttributes(targetAttrType).ToArray();
#endif
                    } catch (System.IO.FileNotFoundException) {
                        // Sometimes the previewer doesn't actually have everything required for these loads to work
                        Log.Warning(nameof(Registrar), "Could not load assembly: {0} for Attibute {1} | Some renderers may not be loaded", assembly.FullName, targetAttrType.FullName);
                        continue;
                    }

                    var length = attributes.Length;
                    if (length == 0) {
                        continue;
                    }

                    for (int i = 0; i < length; i++) {
                        CrossViewAttribute attribute = (CrossViewAttribute)attributes[i];
                        if (!dependencyTypes.Contains(attribute.DependencyInfo)) {
                            dependencyTypes.Add(attribute.DependencyInfo);
                        }
                    }
                }

                initialized = true;
            }
        }

        class DependencyData {

            public DependencyData() {
            }

            public DependencyData(Type implementorType, string storyBoardIdentifier = "", string storyBoardName = "") {
                ImplementorType = implementorType;
                StoryBoardIdentifier = storyBoardIdentifier;
                StoryBoardName = storyBoardName;
            }

            public DependencyData(CrossViewImplementorInfo crossViewImplementorInfo) {
                ImplementorType = crossViewImplementorInfo.Implementor;
                StoryBoardIdentifier = crossViewImplementorInfo.StoryBoardIdentifier;
                StoryBoardName = crossViewImplementorInfo.StoryBoardName;
            }

            public ICrossView GlobalInstance { get; set; }

            public Type ImplementorType { get; set; }

            public string StoryBoardIdentifier { get; set; }
            public string StoryBoardName { get; internal set; }
        }
    }
}