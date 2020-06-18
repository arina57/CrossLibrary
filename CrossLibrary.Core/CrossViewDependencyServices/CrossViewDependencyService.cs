using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
                try {
                    dependencyImplementation = GetDependencyImplementation(targetType, id);
                } catch (InvalidOperationException ex) {
                    var message = $"Could not find single matching dependency implementation for view model: {viewModelType.Name}. ";
                    message += string.IsNullOrEmpty(id) ? string.Empty : $"with ID: {id}. ";
                    message += ex.Message;
                    throw new Exception(message, ex);
                }
                
            }

            dynamic crossView;
            if (fetchTarget == DependencyFetchTarget.GlobalInstance && dependencyImplementation.GlobalInstance != null && dependencyImplementation.GlobalInstance is ICrossView crossViewCast) {
                return crossViewCast;
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
            return (ICrossView)crossView;
        }


        /// <summary>
        /// Gets dependency injected type - for non cross views
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fetchTarget"></param>
        /// <returns></returns>
        public static T Get<T>(DependencyFetchTarget fetchTarget = DependencyFetchTarget.GlobalInstance) where T : class {
            Initialize();

            DependencyData dependencyImplementation;
            lock (dependencyLock) {
                Type targetType = typeof(T);
                //if (!dependencyImplementations.TryGetValue(targetType, out dependencyImplementation)) {
                //    Type implementor = FindImplementor(targetType);
                //    dependencyImplementations[targetType] = (dependencyImplementation = implementor != null ? new DependencyData { ImplementorType = implementor } : null);
                //}
                dependencyImplementation = GetDependencyImplementation(targetType);
            }

            if (dependencyImplementation == null)
                return null;

            if (fetchTarget == DependencyFetchTarget.GlobalInstance) {
                if (dependencyImplementation.GlobalInstance == null) {
                    lock (dependencyImplementation) {
                        if (dependencyImplementation.GlobalInstance == null) {
                            dependencyImplementation.GlobalInstance = Activator.CreateInstance(dependencyImplementation.ImplementorType);
                        }
                    }
                }
                return (T)dependencyImplementation.GlobalInstance;
            }
            return (T)Activator.CreateInstance(dependencyImplementation.ImplementorType);
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
                .Where(info => target.IsAssignableFrom(info.Implementor) && id == info.Id)
                //.GroupBy(info => new { info.Implementor, info.Id })
                //.Select(group => group.FirstOrDefault())
                .Select(info => new DependencyData(info));
                
                

            return assignable.Single();
        }



        public static void Register<T>(string id = "", string storyBoardIdentifier = "", string storyboardName = "") where T : class {
            Type type = typeof(T);
            if (!dependencyTypes.Any(info => info.Implementor == type)) {
                dependencyTypes.Add(new CrossViewImplementorInfo(type, storyBoardIdentifier, storyboardName, id));
            }
        }

        public static void Register<T, TImpl>(string id = "", string storyBoardIdentifier = "", string storyboardName = "") where T : class where TImpl : class, T {
            Type targetType = typeof(T);
            Type implementorType = typeof(TImpl);
            if (!dependencyTypes.Any(info => info.Implementor == targetType)) {
                dependencyTypes.Add(new CrossViewImplementorInfo(targetType, storyBoardIdentifier, storyboardName, id));
            }
            lock (dependencyLock) {
                dependencyImplementations[(targetType, id)] = new DependencyData { ImplementorType = implementorType, Id = id };
            }
        }

        /// <summary>
        /// If an assembly is not referenced anywhere it will not be included
        /// in the list of assemblies to search for depenency injection types.
        /// </summary>
        /// <param name="assembly"></param>
        public static void ExplicitlyIncludeAssembly(Assembly assembly) {
            //This is not really needed because simply adding a reference should added it to the list of assemblies.
            explicitlyIncludeAssembly.Add(assembly);
        }

        private static List<Assembly> explicitlyIncludeAssembly = new List<Assembly>();

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

                //Assembly[] deviceAssemblies = Device.GetAssemblies();
                Assembly[] deviceAssemblies = AppDomain.CurrentDomain.GetAssemblies();


                var allAsseblies = deviceAssemblies.Union(explicitlyIncludeAssembly);
                //if (Registrar.ExtraAssemblies != null) {
                //    allAsseblies = allAsseblies.Union(Registrar.ExtraAssemblies);
                //}


                Initialize(allAsseblies.ToArray());
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


                Type targetAttrType = typeof(CrossDependencyAttribute);


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
                        //Log.Warning(nameof(Registrar), "Could not load assembly: {0} for Attibute {1} | Some renderers may not be loaded", assembly.FullName, targetAttrType.FullName);
                        if(Debugger.IsAttached) {
                            Debugger.Break();
                        }
                        continue;
                    }

                    var length = attributes.Length;
                    //if (length == 0) {
                    //    continue;
                    //}

                    ///Tracks the registered types, so they aren't added twice
                    var registeredTypes = new List<Type>();
                    for (int i = 0; i < length; i++) {
                        CrossDependencyAttribute attribute = (CrossDependencyAttribute)attributes[i];
                        if (!dependencyTypes.Contains(attribute.DependencyInfo)) {
                            dependencyTypes.Add(attribute.DependencyInfo);
                            registeredTypes.Add(attribute.DependencyInfo.Implementor);
                        }
                    }

                    //Find all crossViews in assembly
                    var allTypes = assembly.GetTypes();
                    foreach (var type in allTypes) {
                        if (typeof(ICrossView).IsAssignableFrom(type) && !registeredTypes.Contains(type)) {
                            dependencyTypes.Add(new CrossViewImplementorInfo(type));
                        }
                    }


                }

                initialized = true;
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

            public object GlobalInstance { get; set; }

            public Type ImplementorType { get; set; }

            public string StoryBoardIdentifier { get; set; } = string.Empty;
            public string StoryBoardName { get; internal set; } = string.Empty;
            public string Id { get; set; } = string.Empty;
        }
    }
}
