﻿using System.Collections.Concurrent;
using LightBDD.Core.Configuration;
using LightBDD.Framework.Configuration;
using LightBDD.Framework.Notification;
using LightBDD.NUnit3.UnitTests;

[assembly: ConfiguredLightBddScope]
namespace LightBDD.NUnit3.UnitTests
{
    internal class ConfiguredLightBddScope : LightBddScopeAttribute
    {
        public static readonly ConcurrentQueue<string> CapturedNotifications = new ConcurrentQueue<string>();
        protected override void OnConfigure(LightBddConfiguration configuration)
        {
            configuration.ReportWritersConfiguration()
                .Clear();


            configuration.FeatureProgressNotifierConfiguration()
                .ClearNotifiers();

            configuration.ScenarioProgressNotifierConfiguration()
                .ClearNotifierProviders()
                .AppendNotifierProviders(() => new DefaultProgressNotifier(x => CapturedNotifications.Enqueue(x)));
        }
    }
}