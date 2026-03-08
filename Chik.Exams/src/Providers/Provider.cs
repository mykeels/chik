using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Chik.Exams
{
    public static class Provider
    {
        private static IServiceProvider _instance = new ServiceCollection().BuildServiceProvider();
        private static List<Type> _trackedTypes = new List<Type>();

        internal static void ResetTrackedTypes()
        {
            _trackedTypes = new List<Type>();
        }

        /// <summary>
        /// Gets the current global <see cref="IServiceProvider"/> instance used for service resolution.
        /// </summary>
        public static IServiceProvider Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// Sets the global <see cref="IServiceProvider"/> instance to the specified value.
        /// </summary>
        /// <param name="instance">The <see cref="IServiceProvider"/> to set as the global instance.</param>
        public static IServiceProvider SetInstance(IServiceProvider instance)
        {
            _instance = instance;
            return _instance;
        }

        /// <summary>
        /// Gets a service of type <typeparamref name="T"/> from the global service provider.
        /// </summary>
        /// <typeparam name="T">The type of service to retrieve.</typeparam>
        /// <returns>The service instance if found; otherwise, <c>null</c>.</returns>
        public static T? GetService<T>()
        {
            return _instance.GetService<T>();
        }

        /// <summary>
        /// Gets a required service of type <typeparamref name="T"/> from the global service provider.
        /// </summary>
        /// <typeparam name="T">The type of service to retrieve.</typeparam>
        /// <returns>The service instance.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the service is not found.</exception>
        public static T GetRequiredService<T>() where T : notnull
        {
            return _instance.GetRequiredService<T>();
        }

        /// <summary>
        /// Gets the global <see cref="ILogger"/> instance from the service provider.
        /// </summary>
        public static ILogger Logger
        {
            get { return GetRequiredService<ILogger>(); }
        }

        /// <summary>
        /// Gets all services that are being tracked by the provider, attempting to resolve each one.
        /// </summary>
        /// <param name="provider">The <see cref="IServiceProvider"/> to use for resolution.</param>
        /// <returns>A tuple containing a list of successfully resolved services and a list of exceptions for failed resolutions.</returns>
        public static (List<(Type, object)>, List<Exception>) GetTrackedServices(
            this IServiceProvider provider
        )
        {
            return GetTrackedServices(provider.GetRequiredService);
        }

        /// <summary>
        /// Asserts that all tracked services can be resolved from the provider. Throws an <see cref="AggregateException"/> if any cannot be resolved.
        /// </summary>
        /// <param name="provider">The <see cref="IServiceProvider"/> to use for resolution.</param>
        public static void AssertTrackedServices(this IServiceProvider provider)
        {
            var (services, exceptions) = GetTrackedServices(provider);
            if (exceptions.Any())
            {
                throw new AggregateException(exceptions);
            }
        }

        /// <summary>
        /// Gets all tracked services using the specified service resolver function.
        /// </summary>
        /// <param name="getRequiredService">A function to resolve a service by type. If null, uses the global provider.</param>
        /// <returns>A tuple containing a list of successfully resolved services and a list of exceptions for failed resolutions.</returns>
        internal static (List<(Type, object)>, List<Exception>) GetTrackedServices(
            Func<Type, object>? getRequiredService = null
        )
        {
            var services = new List<(Type, object)>();
            var exceptions = new List<Exception>();
            foreach (var type in _trackedTypes)
            {
                try
                {
                    getRequiredService ??= (type) =>
                    {
                        return _instance.GetRequiredService(type);
                    };
                    var service = getRequiredService(type);
                    services.Add((type, service));
                }
                catch (Exception ex)
                {
                    var newEx = new Exception($"Failed to resolve service: {type.FullName}", ex);
                    newEx.Data["Type"] = type.FullName;
                    exceptions.Add(newEx);
                }
            }
            return (services, exceptions);
        }

        /// <summary>
        /// Registers and tracks a scoped service of the specified interface and implementation types.
        /// </summary>
        /// <typeparam name="TInterface">The interface type to register.</typeparam>
        /// <typeparam name="TService">The implementation type to register.</typeparam>
        /// <param name="services">The service collection to add the service to.</param>
        /// <param name="factory">An optional factory for creating the service instance.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection TrackScoped<TInterface, TService>(
            this IServiceCollection services,
            Func<IServiceProvider, TService> factory
        )
            where TService : class, TInterface
            where TInterface : class
        {
            _trackedTypes.Add(typeof(TInterface));
            if (factory == null)
            {
                services.AddScoped<TInterface, TService>();
            }
            else
            {
                services.AddScoped<TInterface, TService>(provider => factory(provider));
            }
            return services;
        }

        /// <summary>
        /// Registers and tracks a scoped service of the specified type.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="services">The service collection to add the service to.</param>
        /// <param name="factory">An optional factory for creating the service instance.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection TrackScoped<TService>(
            this IServiceCollection services,
            Func<IServiceProvider, TService> factory
        )
            where TService : class
        {
            _trackedTypes.Add(typeof(TService));
            if (factory == null)
            {
                services.AddScoped<TService>();
            }
            else
            {
                services.AddScoped(provider => factory(provider));
            }
            return services;
        }

        /// <summary>
        /// Registers and tracks a scoped service of the specified interface and implementation types using a parameterless factory.
        /// </summary>
        /// <typeparam name="TInterface">The interface type to register.</typeparam>
        /// <typeparam name="TService">The implementation type to register.</typeparam>
        /// <param name="services">The service collection to add the service to.</param>
        /// <param name="factory">A parameterless factory for creating the service instance.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection TrackScoped<TInterface, TService>(
            this IServiceCollection services,
            Func<TService> factory
        )
            where TService : class, TInterface
            where TInterface : class
        {
            return TrackScoped<TInterface, TService>(services, provider => factory());
        }

        /// <summary>
        /// Registers and tracks a scoped service of the specified type using a parameterless factory.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="services">The service collection to add the service to.</param>
        /// <param name="factory">A parameterless factory for creating the service instance.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection TrackScoped<TService>(
            this IServiceCollection services,
            Func<TService> factory
        )
            where TService : class
        {
            return TrackScoped(services, provider => factory());
        }

        /// <summary>
        /// Registers and tracks a scoped service of the specified type using the provided instance.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="services">The service collection to add the service to.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection TrackScoped<TService>(
            this IServiceCollection services
        )
            where TService : class
        {
            _trackedTypes.Add(typeof(TService));
            services.AddScoped<TService>();
            return services;
        }


        /// <summary>
        /// Registers and tracks a scoped service of the specified type using the provided instance.
        /// </summary>
        /// <typeparam name="TInterface">The interface type to register.</typeparam>
        /// <typeparam name="TService">The implementation type to register.</typeparam>
        /// <param name="services">The service collection to add the service to.</param>
        /// <param name="service">The service instance to register.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection TrackScoped<TInterface, TService>(
            this IServiceCollection services
        )
            where TService : class, TInterface
            where TInterface : class
        {
            _trackedTypes.Add(typeof(TInterface));
            services.AddScoped<TInterface, TService>();
            return services;
        }

        /// <summary>
        /// Registers and tracks a singleton service of the specified interface and implementation types.
        /// </summary>
        /// <typeparam name="TInterface">The interface type to register.</typeparam>
        /// <typeparam name="TService">The implementation type to register.</typeparam>
        /// <param name="services">The service collection to add the service to.</param>
        /// <param name="factory">An optional factory for creating the service instance.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection TrackSingleton<TInterface, TService>(
            this IServiceCollection services,
            Func<IServiceProvider, TService> factory
        )
            where TService : class, TInterface
            where TInterface : class
        {
            _trackedTypes.Add(typeof(TInterface));
            if (factory == null)
            {
                services.AddSingleton<TInterface, TService>();
            }
            else
            {
                services.AddSingleton<TInterface, TService>(provider => factory(provider));
            }
            return services;
        }

        /// <summary>
        /// Registers and tracks a singleton service of the specified type.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="services">The service collection to add the service to.</param>
        /// <param name="factory">An optional factory for creating the service instance.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection TrackSingleton<TService>(
            this IServiceCollection services,
            Func<IServiceProvider, TService> factory
        )
            where TService : class
        {
            _trackedTypes.Add(typeof(TService));
            if (factory == null)
            {
                services.AddSingleton<TService>();
            }
            else
            {
                services.AddSingleton(provider => factory(provider));
            }
            return services;
        }

        /// <summary>
        /// Registers and tracks a singleton service of the specified interface and implementation types using a parameterless factory.
        /// </summary>
        /// <typeparam name="TInterface">The interface type to register.</typeparam>
        /// <typeparam name="TService">The implementation type to register.</typeparam>
        /// <param name="services">The service collection to add the service to.</param>
        /// <param name="factory">A parameterless factory for creating the service instance.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection TrackSingleton<TInterface, TService>(
            this IServiceCollection services,
            Func<TService> factory
        )
            where TService : class, TInterface
            where TInterface : class
        {
            return TrackSingleton<TInterface, TService>(services, provider => factory());
        }

        /// <summary>
        /// Registers and tracks a singleton service of the specified type using a parameterless factory.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="services">The service collection to add the service to.</param>
        /// <param name="factory">A parameterless factory for creating the service instance.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection TrackSingleton<TService>(
            this IServiceCollection services,
            Func<TService> factory
        )
            where TService : class
        {
            return TrackSingleton(services, provider => factory());
        }

        /// <summary>
        /// Registers and tracks a singleton service of the specified type using the provided instance.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="services">The service collection to add the service to.</param>
        /// <param name="service">The service instance to register.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection TrackSingleton<TService>(
            this IServiceCollection services,
            TService service
        )
            where TService : class
        {
            _trackedTypes.Add(typeof(TService));
            services.AddSingleton(service);
            return services;
        }

        /// <summary>
        /// Registers and tracks a singleton service of the specified type using the provided instance.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="services">The service collection to add the service to.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection TrackSingleton<TService>(
            this IServiceCollection services
        )
            where TService : class
        {
            _trackedTypes.Add(typeof(TService));
            services.AddSingleton<TService>();
            return services;
        }

        /// <summary>
        /// Registers and tracks a singleton service of the specified interface and implementation types using the provided instance.
        /// </summary>
        /// <typeparam name="TInterface">The interface type to register.</typeparam>
        /// <typeparam name="TService">The implementation type to register.</typeparam>
        /// <param name="services">The service collection to add the service to.</param>
        /// <param name="service">The service instance to register.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection TrackSingleton<TInterface, TService>(
            this IServiceCollection services,
            TService service
        )
            where TService : class, TInterface
            where TInterface : class
        {
            _trackedTypes.Add(typeof(TInterface));
            services.AddSingleton<TInterface>(service);
            return services;
        }

        /// <summary>
        /// Registers and tracks a singleton service of the specified type using the provided instance.
        /// </summary>
        /// <typeparam name="TInterface">The interface type to register.</typeparam>
        /// <typeparam name="TService">The implementation type to register.</typeparam>
        /// <param name="services">The service collection to add the service to.</param>
        /// <param name="service">The service instance to register.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection TrackSingleton<TInterface, TService>(
            this IServiceCollection services
        )
            where TService : class, TInterface
            where TInterface : class
        {
            _trackedTypes.Add(typeof(TInterface));
            services.AddSingleton<TInterface, TService>();
            return services;
        }

        /// <summary>
        /// Registers and tracks a transient service of the specified interface and implementation types.
        /// </summary>
        /// <typeparam name="TInterface">The interface type to register.</typeparam>
        /// <typeparam name="TService">The implementation type to register.</typeparam>
        /// <param name="services">The service collection to add the service to.</param>
        /// <param name="factory">An optional factory for creating the service instance.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection TrackTransient<TInterface, TService>(
            this IServiceCollection services,
            Func<IServiceProvider, TService> factory
        )
            where TService : class, TInterface
            where TInterface : class
        {
            _trackedTypes.Add(typeof(TInterface));
            if (factory == null)
            {
                services.AddTransient<TInterface, TService>();
            }
            else
            {
                services.AddTransient<TInterface, TService>(provider => factory(provider));
            }
            return services;
        }

        /// <summary>
        /// Registers and tracks a transient service of the specified type.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="services">The service collection to add the service to.</param>
        /// <param name="factory">An optional factory for creating the service instance.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection TrackTransient<TService>(
            this IServiceCollection services,
            Func<IServiceProvider, TService> factory
        )
            where TService : class
        {
            _trackedTypes.Add(typeof(TService));
            if (factory == null)
            {
                services.AddTransient<TService>();
            }
            else
            {
                services.AddTransient(provider => factory(provider));
            }
            return services;
        }

        /// <summary>
        /// Registers and tracks a transient service of the specified interface and implementation types using a parameterless factory.
        /// </summary>
        /// <typeparam name="TInterface">The interface type to register.</typeparam>
        /// <typeparam name="TService">The implementation type to register.</typeparam>
        /// <param name="services">The service collection to add the service to.</param>
        /// <param name="factory">A parameterless factory for creating the service instance.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection TrackTransient<TInterface, TService>(
            this IServiceCollection services,
            Func<TService> factory
        )
            where TService : class, TInterface
            where TInterface : class
        {
            return TrackTransient<TInterface, TService>(services, provider => factory());
        }

        /// <summary>
        /// Registers and tracks a transient service of the specified type using a parameterless factory.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="services">The service collection to add the service to.</param>
        /// <param name="factory">A parameterless factory for creating the service instance.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection TrackTransient<TService>(
            this IServiceCollection services,
            Func<TService> factory
        )
            where TService : class
        {
            return TrackTransient<TService>(services, provider => factory());
        }

        /// <summary>
        /// Registers and tracks a transient service of the specified type using the provided instance.
        /// </summary>
        /// <typeparam name="TService">The service type to register.</typeparam>
        /// <param name="services">The service collection to add the service to.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection TrackTransient<TService>(
            this IServiceCollection services
        )
            where TService : class
        {
            _trackedTypes.Add(typeof(TService));
            services.AddTransient<TService>();
            return services;
        }


        /// <summary>
        /// Registers and tracks a transient service of the specified type using the provided instance.
        /// </summary>
        /// <typeparam name="TInterface">The interface type to register.</typeparam>
        /// <typeparam name="TService">The implementation type to register.</typeparam>
        /// <param name="services">The service collection to add the service to.</param>
        /// <param name="service">The service instance to register.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection TrackTransient<TInterface, TService>(
            this IServiceCollection services
        )
            where TService : class, TInterface
            where TInterface : class
        {
            _trackedTypes.Add(typeof(TInterface));
            services.AddTransient<TInterface, TService>();
            return services;
        }

    }
}
