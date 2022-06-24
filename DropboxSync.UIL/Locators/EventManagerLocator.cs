using System.Reflection;
using System.Net.Mime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DropboxSync.Helpers;
using DropboxSync.UIL.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DropboxSync.UIL.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace DropboxSync.UIL.Locators
{
    public class EventManagerLocator
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;

        public EventManagerLocator(ILogger<EventManagerLocator> logger, IServiceProvider serviceProvider)
        {
            if (Program.Host is null) throw new NullReferenceException(nameof(Program.Host));

            _serviceProvider = serviceProvider ??
                throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
        }

        public bool RedirectToManager(string eventJson)
        {
            if (string.IsNullOrEmpty(eventJson)) throw new ArgumentNullException(nameof(eventJson));

            List<string> managers = new List<string>();

            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (!string.IsNullOrEmpty(type.Namespace))
                {
                    if (type.Namespace.EndsWith("Managers") &&
                        type.IsClass &&
                        type.Name.Contains("Manager"))
                    {
                        managers.Add(type.Name.Substring(0, type.Name.Length - "Manager".Length));
                    }
                }
            }

            EventModel? eventModel = JsonConvert.DeserializeObject<EventModel>(eventJson);

            if (eventModel is null) throw new NullValueException(nameof(eventModel));

            Type? eventManagerType = null;

            foreach (string manager in managers)
            {
                eventManagerType = Assembly.GetExecutingAssembly().GetTypes()
                    .SingleOrDefault(t => t.IsInterface && t.Name.Contains(manager + "Manager"));

                if (eventManagerType is null) continue;

                // object? managerService = _serviceProvider.GetService(eventManagerType);
                object? managerService = Program.Host?.Services.GetRequiredService(eventManagerType);

                if (managerService is null) throw new NullValueException(nameof(managerService));

                foreach (MemberInfo method in eventManagerType.GetMembers())
                {
                    MethodEventAttribute? attribute = (MethodEventAttribute?)method.GetCustomAttribute(typeof(MethodEventAttribute));

                    if (attribute is null) continue;

                    if (attribute.EventName.Equals(eventModel.EventName))
                    {
                        Type obj = attribute.EventType;

                        string methodName = method.Name;
                        MethodInfo? methodInfo = eventManagerType.GetMethod(methodName);

                        if (methodInfo is null) throw new NullValueException(nameof(methodInfo));

                        object? deserializedObject = JsonConvert.DeserializeObject(eventJson, attribute.EventType);

                        if (deserializedObject is null)
                        {
                            _logger.LogError("{date} | Could not deserialize json \"{json}\" to an object of type \"{type}\"",
                                DateTime.Now, eventJson, attribute.EventType);
                            return false;
                        }

                        if (methodInfo.IsGenericMethod)
                        {
                            MethodInfo? genericMethodInfo = methodInfo.MakeGenericMethod(new[] { deserializedObject.GetType() });

                            if (genericMethodInfo is null) throw new NullValueException(nameof(genericMethodInfo));

                            bool? result = (bool?)genericMethodInfo.Invoke(managerService, new[] { deserializedObject });

                            if (result is null) throw new NullValueException(nameof(result));

                            return (bool)result;
                        }
                        else
                        {
                            bool? result = (bool?)methodInfo.Invoke(managerService, new[] { deserializedObject });

                            if (result is null) throw new NullValueException(nameof(result));

                            return (bool)result;
                        }

                        // return (bool)(methodInfo.Invoke(managerService, new[] { deserializedObject }) ??
                        //     throw new NullValueException(nameof(methodInfo.Name)));
                    }
                }
            }

            if (eventManagerType is null)
            {
                _logger.LogError("{date} | Could not find any manager for event type : {e}",
                    DateTime.Now, eventModel.EventName);
                return false;
            }



            return false;
        }
    }
}