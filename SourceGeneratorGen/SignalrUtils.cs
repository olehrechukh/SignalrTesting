using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SourceGeneratorGen
{
    public class SignalrUtils
    {
        public static string GenerateClient(string hubName, string @namespace, List<HubMethod> hubMethods)
        {
            var systemReferences = new[]
                {"System", "System.Threading.Tasks", "Microsoft.AspNetCore.SignalR.Client"};

            var allReferences = hubMethods
                .SelectMany(hubMethod => hubMethod.Parameters)
                .Select(parameter => parameter.Namespace)
                .Concat(systemReferences)
                .Distinct()
                .OrderBy(reference => reference)
                .ToList();

            var references = string.Join(Environment.NewLine, allReferences.Select(reference => $"using {reference};"));
            var methods = string.Join(Environment.NewLine, hubMethods.Select(GenerateMethod));

            var result = $@"{references}

namespace {@namespace}
{{
    public class {hubName}
    {{
        private readonly HubConnection _connection;

        public {hubName} (HubConnection connection)
        {{
            _connection = connection;
        }}
        {methods}
    }}
}}";
            return result;
        }

        private static string GenerateMethod(HubMethod method)
        {
            var parameters = string.Join(", ",
                method.Parameters.Select(parameter => $"{parameter.Type} {parameter.Name}"));

            var inputParameters = $"\"{method.Name}\", " + string.Join(", ",
                method.Parameters.Select(parameter => parameter.Name));

            var result = $@"
        public Task {method.Name}({parameters})
        {{
            return _connection.SendAsync({inputParameters});
        }}";

            return result;
        }
    }

    public class HubMethod
    {
        public string ReturnValue { get; init; }
        public string Name { get; init; }
        public IEnumerable<Parameter> Parameters { get; init; }
    }

    public class Parameter
    {
        public string Type { get; init; }
        public string Name { get; init; }
        public string Namespace { get; init; }
    }
}