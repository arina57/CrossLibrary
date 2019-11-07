using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xamarin.Forms.Internals;
using Xamarin.Forms;
using CrossLibrary;
using CrossLibrary.Interfaces;
using System.Diagnostics;

namespace CrossLibrary.Dependency {
    public static class CrossViewDependencyService {
        static bool initialized;
        //private static List<Type> crossViews;
        static readonly object dependencyLock = new object();
        static readonly object initializeLock = new object();

        static readonly List<CrossViewImplementorInfo> dependencyTypes = new List<CrossViewImplementorInfo>();

        static readonly Dictionary<(Type, string), DependencyData> dependencyImplementations = new Dictionary<(Type, string), DependencyData>();

        public enum DependencyFetchTarget {
            GlobalInstance,
            NewInstance
        }


        /// <summary>
        /// Create a cross view from a view model, with the id specified
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="fetchTarget"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ICrossView CreateCrossView(CrossViewModel viewModel, DependencyFetchTarget fetchTarget = DependencyFetchTarget.NewInstance, string id = "") {
            Initialize();
            Type viewModelType = viewModel.GetType();
            DependencyData dependencyImplementation;
            lock (dependencyLock) {
                Type targetType = typeof(ICrossView<>).MakeGenericType(viewModelType);
                dependencyImplementation = GetDependencyImplementation(targetType, id);
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

        private static DependencyData GetDependencyImplementation(Type targetType, string id = "") {
            if (!dependencyImplementations.ContainsKey((targetType, id))) {
                var dependencyData = FindImplementor(targetType, id);
                dependencyImplementations[(targetType, id)] = dependencyData;
            }
            return dependencyImplementations[(targetType, id)];
        }

        static DependencyData FindImplementor(Type target, string id = "") {
            //Find all registered classes that match the id and type
            var assignable = dependencyTypes
                .Where(t => target.IsAssignableFrom(t.Implementor) && id == t.Id)
                .Select(info => new DependencyData(info));

      
           
            if (Debugger.IsAttached) {
                if (assignable.Count() > 1) {
                    //If there are more than one class that is assignable from the type
                    //and both have the same id. stop if the debugger is attached.
                    //this shouldnt happen.
                    Debugger.Break();
                }
            }
            return assignable.FirstOrDefault();
        }


        public static void Register<T>(string id = "") where T : class {
            Type type = typeof(T);
            if (!dependencyTypes.Any(info => info.Implementor == type)) {
                dependencyTypes.Add(new CrossViewImplementorInfo(type, id: id));
            }
        }

        public static void Register<T, TImpl>(string id = "") where T : class where TImpl : class, T {
            Type targetType = typeof(T);
            Type implementorType = typeof(TImpl);
            if (!dependencyTypes.Any(info => info.Implementor == targetType)) {
                dependencyTypes.Add(new CrossViewImplementorInfo(targetType, id: id));
            }
            lock (dependencyLock) {
                dependencyImplementations[(targetType, id)] = new DependencyData { ImplementorType = implementorType, Id = id };
            }
        }




        /// <summary>
        /// Get's all the registed classes and classes of type crossview
        /// </summary>
        static void Initialize() {
            if (initialized) {
                return;
            }

            lock (initializeLock) {
                if (initialized) {
                    return;
                }

                Assembly[] assemblies = Device.GetAssemblies();

                


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

                using (new DebugHelper.Timer()) {

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

                        ///Tracks the registered types, so they aren't added twice
                        var registeredTypes = new List<Type>();
                        for (int i = 0; i < length; i++) {
                            CrossViewAttribute attribute = (CrossViewAttribute)attributes[i];
                            if (!dependencyTypes.Contains(attribute.DependencyInfo)) {
                                dependencyTypes.Add(attribute.DependencyInfo);
                                registeredTypes.Add(attribute.DependencyInfo.Implementor);
                            }
                        }

                        using (new DebugHelper.Timer()) {
                            //Find all crossViews in assembly
                            foreach (var type in assembly.GetTypes()) {
                                if (typeof(ICrossView).IsAssignableFrom(type) && !registeredTypes.Contains(type)) {
                                    dependencyTypes.Add(new CrossViewImplementorInfo(type));
                                }
                            }
                        }


                    }

                    initialized = true;
                }
            }
        }

        class DependencyData {

            public DependencyData() {
            }

            public DependencyData(Type implementorType) {
                ImplementorType = implementorType;
            }

            public DependencyData(CrossViewImplementorInfo crossViewImplementorInfo) {
                ImplementorType = crossViewImplementorInfo.Implementor;
                StoryBoardIdentifier = crossViewImplementorInfo.StoryBoardIdentifier;
                StoryBoardName = crossViewImplementorInfo.StoryBoardName;
                Id = crossViewImplementorInfo.Id;
            }

            public ICrossView GlobalInstance { get; set; }

            public Type ImplementorType { get; set; }

            public string StoryBoardIdentifier { get; set; } = string.Empty;
            public string StoryBoardName { get; internal set; } = string.Empty;
            public string Id { get; set; } = string.Empty;
        }
    }
}
