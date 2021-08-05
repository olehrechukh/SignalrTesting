using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Channels;
using System.Threading.Tasks;
using DotLiquid;
using Microsoft.AspNetCore.SignalR;

namespace SignalrTesting.Tests.Utils
{
    public class SignalrUtils
    {
        public static string GenerateClient<THub>() where THub : Hub => GenerateClient(typeof(THub));

        private static string GenerateClient(Type type)
        {

            var enumerateHubMethods = GetOperationMethods(type, false).ToList();

            var hubMethods = enumerateHubMethods
                .Select(GetHubMethod)
                .ToList();

            var references = hubMethods
                .SelectMany(x => x.Parameters)
                .Select(x => x.Namespace)
                .Where(x => x != "System")
                .Distinct()
                .ToList();

            var template = Template.Parse(File.ReadAllText(@"Utils\FileTemplate.liquid")); // Parses and compiles the template

            var name = $"{type.Name}Client";
            var fromAnonymousObject = Hash.FromAnonymousObject(new {hubMethods, references, name});
            var render = template.Render(fromAnonymousObject);

            return render;
        }

        private static IEnumerable<MethodInfo> GetOperationMethods(Type type, bool checkReturnValue) =>
            type.GetTypeInfo().GetRuntimeMethods().Where(m =>
            {
                var methodReturnValue = ReturnsValue(m.ReturnType);

                return m.IsPublic &&
                       m.IsSpecialName == false &&
                       m.DeclaringType != typeof(Hub<>) &&
                       m.DeclaringType != typeof(Hub<>) &&
                       m.DeclaringType != typeof(object) &&
                       !ForbiddenOperations.Contains(m.Name) &&
                       (checkReturnValue ? methodReturnValue : !methodReturnValue);
            });

        private static bool ReturnsValue(Type returnType)
        {
            return returnType.IsGenericType && ReturnsValues.Contains(returnType.GetGenericTypeDefinition());
        }

        private static readonly List<Type> ReturnsValues = new()
        {
            typeof(ChannelReader<>), typeof(IAsyncEnumerable<>), typeof(Task<>)
        };

        private static List<string> ForbiddenOperations { get; } = typeof(Hub).GetRuntimeMethods()
            .Concat(typeof(Hub<>).GetRuntimeMethods()).Select(x => x.Name).Distinct().ToList();

        private static HubMethod GetHubMethod(MethodInfo method) => new()
        {
            ReturnValue = GetReturnValue(method),
            Name = method.Name,
            Parameters = method.GetParameters().Select(info => new Parameter
            {
                Name = info.Name, Type = info.ParameterType.Name, Namespace = info.ParameterType.Namespace
            }).ToList()
        };

        private static string GetReturnValue(MethodInfo method) => method.ReturnType.ToString();

        private class HubMethod : Drop
        {
            public string ReturnValue { get; init; }
            public string Name { get; init; }
            public List<Parameter> Parameters { get; init; }
        }

        private class Parameter : Drop
        {
            public string Type { get; init; }
            public string Name { get; init; }

            public string Namespace { get; init; }
        }
    }
}