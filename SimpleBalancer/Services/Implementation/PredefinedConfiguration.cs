using k8s.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using SimpleBalancer.Services.Abstraction;
using System;
using System.Collections.Generic;

namespace SimpleBalancer.Services.Implementation
{
    internal static class PredefinedConfiguration
    {
        public static bool HasPredefinedConfiguration(KubeConfigException exception, IWebHostEnvironment env)
        {
            var isSupportedException = exception.Message.StartsWith("unable to load in-cluster");
            var isSupportedEnvironment = env.IsDevelopment() || env.IsEnvironment("Test");
            return isSupportedException && isSupportedEnvironment;
        }

        public static IReadOnlyList<EndpointEntry> GetEndpointEntries(IWebHostEnvironment env)
        {
            return env.EnvironmentName switch
            {
                "Development" => GetDevelopmentConfiguration(),
                "Test" => GetTestConfiguration(),
                _ => throw new ArgumentException($"Environment {env.EnvironmentName} unknown"),
            };
        }

        private static IReadOnlyList<EndpointEntry> GetTestConfiguration()
        {
            return new EndpointEntry[]
            {
                new EndpointEntry("10.1.6.120", 80),
                new EndpointEntry("10.1.6.121", 80),
                new EndpointEntry("10.1.6.122", 80)
            };
        }

        private static IReadOnlyList<EndpointEntry> GetDevelopmentConfiguration()
        {
            return new EndpointEntry[]
            {
                new EndpointEntry("127.0.0.1", 8000)
            };
        }
    }
}
