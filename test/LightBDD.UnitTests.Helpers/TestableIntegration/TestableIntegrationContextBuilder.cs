using System;
using LightBDD.Core.Configuration;
using LightBDD.Core.Extensibility;
using LightBDD.Core.Extensibility.Execution;
using LightBDD.Core.Formatting;
using LightBDD.Core.Notification;
using LightBDD.Core.Results;
using LightBDD.Framework.Formatting;
using LightBDD.Framework.Notification;

namespace LightBDD.UnitTests.Helpers.TestableIntegration
{
    public class TestableIntegrationContextBuilder
    {
        private INameFormatter _nameFormatter;
        private Func<INameFormatter, IMetadataProvider> _metadataProvider;
        private Func<Exception, ExecutionStatus> _exceptionToStatusMapper;
        private IFeatureProgressNotifier _featureProgressNotifier;
        private Func<object, IScenarioProgressNotifier> _scenarioProgressNotifierProvider;
        private IExecutionExtensions _executionExtensions;

        private TestableIntegrationContextBuilder()
        {
            _nameFormatter = new DefaultNameFormatter();
            _metadataProvider = nameFormatter => new TestMetadataProvider(nameFormatter);
            _exceptionToStatusMapper = ex => ex is CustomIgnoreException ? ExecutionStatus.Ignored : ExecutionStatus.Failed;
            _featureProgressNotifier = NoProgressNotifier.Default;
            _scenarioProgressNotifierProvider = feature => NoProgressNotifier.Default;
            _executionExtensions = new ExecutionExtensionsConfiguration().EnableStepExtension<StepCommentHelper>();
        }

        public TestableIntegrationContextBuilder WithNameFormatter(INameFormatter formatter)
        {
            _nameFormatter = formatter;
            return this;
        }

        public TestableIntegrationContextBuilder WithMetadataProvider(Func<INameFormatter, IMetadataProvider> provider)
        {
            _metadataProvider = provider;
            return this;
        }

        public TestableIntegrationContextBuilder WithExceptionToStatusMapper(Func<Exception, ExecutionStatus> mapper)
        {
            _exceptionToStatusMapper = mapper;
            return this;
        }

        public TestableIntegrationContextBuilder WithFeatureProgressNotifier(IFeatureProgressNotifier notifier)
        {
            _featureProgressNotifier = notifier;
            return this;
        }

        public TestableIntegrationContextBuilder WithScenarioProgressNotifierProvider(Func<object, IScenarioProgressNotifier> provider)
        {
            _scenarioProgressNotifierProvider = provider;
            return this;
        }
        public TestableIntegrationContextBuilder WithExecutionExtensions(IExecutionExtensions executionExtensions)
        {
            _executionExtensions = executionExtensions;
            return this;
        }

        public static TestableIntegrationContextBuilder Default()
        {
            return new TestableIntegrationContextBuilder();
        }

        public IIntegrationContext Build()
        {
            return new TestableIntegrationContext(_nameFormatter, _metadataProvider(_nameFormatter), _exceptionToStatusMapper, _featureProgressNotifier, _scenarioProgressNotifierProvider, _executionExtensions);
        }
    }
}