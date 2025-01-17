using System.Collections.Concurrent;
using LightBDD.Core.Configuration;
using LightBDD.Framework.Configuration;
using LightBDD.Framework.Notification;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LightBDD.MsTest2.UnitTests
{
    [TestClass]
    public class ConfiguredLightBddScope
    {
        public static readonly ConcurrentQueue<string> CapturedNotifications = new ConcurrentQueue<string>();

        [AssemblyInitialize]
        public static void Setup(TestContext testContext)
        {
            LightBddScope.Initialize(OnConfigure);
        }

        [AssemblyCleanup]
        public static void Cleanup() { LightBddScope.Cleanup(); }

        private static void OnConfigure(LightBddConfiguration configuration)
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