using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using LightBDD.Core.Execution.Results;
using LightBDD.Core.Extensibility;
using LightBDD.Core.UnitTests.TestableIntegration;
using NUnit.Framework;

namespace LightBDD.Core.UnitTests
{
    [TestFixture]
    public class CoreBddRunner_time_measurement_tests
    {
        private IBddRunner _runner;
        private static readonly TimeSpan UtcNowClockPrecision = TimeSpan.FromMilliseconds(15);

        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            _runner = new TestableBddRunner(GetType());
        }

        #endregion

        [Test]
        public void It_should_capture_execution_time_for_successful_scenario()
        {
            AssertScenarioExecutionTime(() => _runner.TestScenario(Step_one, Step_two));
        }

        [Test]
        public void It_should_capture_execution_time_for_failed_scenario()
        {
            AssertScenarioExecutionTime(() => _runner.TestScenario(Step_one, Step_throwing_exception, Step_two));
        }

        private void AssertScenarioExecutionTime(Action runScenario)
        {
            var watch = new Stopwatch();

            var startTime = DateTimeOffset.UtcNow;
            watch.Start();
            try { runScenario(); }
            catch { }
            watch.Stop();

            var result = _runner.Integrate().GetFeatureResult().GetScenarios().Single();

            FormatTime("Measure time", new ExecutionTime(startTime, watch.Elapsed));
            FormatTime("Scenario time", result.ExecutionTime);

            Assert.That(result.ExecutionTime, Is.Not.Null);
            Assert.That(result.ExecutionTime.Duration, Is.LessThanOrEqualTo(watch.Elapsed));
            Assert.That(result.ExecutionTime.Start, Is
                .GreaterThanOrEqualTo(startTime)
                .And
                .LessThan(startTime.Add(watch.Elapsed).Add(UtcNowClockPrecision)));

            AssertStepsExecutionTimesAreDoneInOrder();
        }

        private static void FormatTime(string label, ExecutionTime time)
        {
            if (time == null)
                return;
            TestContext.Out.WriteLine("{0}: {1:HH\\:mm\\:ss.fff} + {2} -> {3:HH\\:mm\\:ss.fff}", label, time.Start, time.Duration, time.End);
        }

        private void AssertStepsExecutionTimesAreDoneInOrder()
        {
            var scenario = _runner.Integrate().GetFeatureResult().GetScenarios().Single();
            var steps = scenario.GetSteps().ToArray();
            for (int i = 0; i < steps.Length; ++i)
            {
                FormatTime("Step result", steps[i].ExecutionTime);
                if (steps[i].Status == ExecutionStatus.NotRun)
                {
                    Assert.That(steps[i].ExecutionTime, Is.Null);
                    continue;
                }

                Assert.That(steps[i].ExecutionTime, Is.Not.Null);

                if (i == 0)
                    Assert.That(steps[i].ExecutionTime.Start, Is.GreaterThanOrEqualTo(scenario.ExecutionTime.Start));
                else
                    Assert.That(steps[i].ExecutionTime.Start, Is.GreaterThanOrEqualTo(steps[i - 1].ExecutionTime.End - UtcNowClockPrecision));

                if (i == steps.Length - 1)
                    Assert.That(steps[i].ExecutionTime.End, Is.LessThanOrEqualTo(scenario.ExecutionTime.End + UtcNowClockPrecision));
            }
        }

        private static void Step_two()
        {
            Thread.Sleep(50);
        }

        private static void Step_one()
        {
            Thread.Sleep(50);
        }

        private static void Step_throwing_exception()
        {
            throw new NotImplementedException();
        }
    }
}