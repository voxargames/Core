using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using VoxarGames.DependecyInjection;

public class CommonBehaviour : MonoBehaviour
{
    #region Constructors

    static CommonBehaviour()
    {
        Initialize();
    }

    public CommonBehaviour()
    {
        this.InjectServices(_serviceProvider);
    }

    #endregion

    #region Unity Engine Startup Methods

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnBeforeSceneLoadRuntimeMethod()
    {
        if (_startupClass != null)
            InvokeMethod("BeforeSceneLoad", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void OnAfterSceneLoadRuntimeMethod()
    {
        if (_startupClass != null)
            InvokeMethod("AfterSceneLoad", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
    }

    #endregion

    #region Private Fields & Properties

    private static Type _startupClass;
    private static IServiceProvider _serviceProvider;

    #endregion

    #region Public Fields & Properties

    #endregion

    private static void Initialize()
    {
        if (_serviceProvider != null)
            return;

        // Instantiate and configure the default services
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton(serviceProvider =>
        {
            return _serviceProvider;
        });

        // Search in the assembly for a class with name Startup
        _startupClass = FindStartupClass();

        // If startup class exist, invoke the method ConfigureServices
        // to configure the user services
        if (_startupClass != null)
            InvokeMethod("ConfigureServices", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy, serviceCollection);

        // Build the service provider
        _serviceProvider = serviceCollection.BuildServiceProvider();

        // If startup class exist, invoke the method ConfigurePipeline
        // to define the pipile of CommonBehaviour
        if (_startupClass != null)
            InvokeMethod("ConfigurePipeline", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
    }

    private static Type FindStartupClass()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var types = assembly.GetExportedTypes();
        return types.FirstOrDefault(type => string.Equals(type.Name, "Startup", StringComparison.InvariantCultureIgnoreCase));
    }

    private static void InvokeMethod(string methodName, BindingFlags bindingFlags, params object[] args)
    {
        var method = _startupClass.GetMethod(methodName, bindingFlags);

        if (method == null)
        {
            Debug.Log($"Method {methodName} not found in the Startup class.");
            return;
        }           

        var argsList = args.ToList();

        var methodParameters = method.GetParameters();
        var parameters = new object[methodParameters.Length];

        for (int i = 0; i < methodParameters.Length; i++)
        {
            var paramType = methodParameters[i].ParameterType;
            var service = _serviceProvider?.GetService(paramType);

            if (service != null)
            {
                parameters[i] = service;
            }
            else
            {
                var arg = argsList.FirstOrDefault(x => x.GetType() == paramType || (paramType.IsInterface && x.GetType().GetInterfaces().Contains(paramType)));

                if (arg != null)
                {
                    parameters[i] = arg;
                    argsList.Remove(arg);
                }
                else
                {
                    throw new Exception("Service not found!");
                }
            }            
        }

        method.Invoke(null, parameters);
    }
}