using System;
using System.Linq;
using LightBDD.Configuration;
using LightBDD.Core.Coordination;
using LightBDD.Core.Extensibility;
using LightBDD.SummaryGeneration;
using LightBDD.SummaryGeneration.Configuration;

namespace LightBDD.Integration.XUnit2
{
    internal class XUnit2FeatureCoordinator : FeatureCoordinator
    {
        public static FeatureCoordinator GetInstance()
        {
            if (Instance == null)
                throw new InvalidOperationException(string.Format("{0} is not defined in the project. Please ensure that following attribute, or attribute extending it is defined at assembly level: [assembly:{0}]", nameof(LightBddScopeAttribute)));
            if (Instance.IsDisposed)
                throw new InvalidOperationException(string.Format("LightBdd scenario test execution is already finished. Please ensure that no tests are executed outside of assembly execution scope specified by {0} attribute.", nameof(LightBddScopeAttribute)));
            return Instance;
        }

        public XUnit2FeatureCoordinator(BddRunnerFactory runnerFactory, IFeatureAggregator featureAggregator) : base(runnerFactory, featureAggregator)
        {
        }

        internal static void InstallSelf(LightBddConfiguration configuration)
        {
            Install(new XUnit2FeatureCoordinator(new XUnit2BddRunnerFactory(configuration), new FeatureSummaryGenerator(configuration.Get<SummaryWritersConfiguration>().ToArray())));
        }
    }
}