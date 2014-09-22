﻿using System;
using System.Linq;
using LightBDD.Notification;
using LightBDD.Results;
using LightBDD.Results.Implementation;
using LightBDD.UnitTests.Helpers;
using NUnit.Framework;
using Rhino.Mocks;

namespace LightBDD.UnitTests
{
    [TestFixture]
    public class BDD_runner_parameterized_tests
    {
        private AbstractBDDRunner _subject;
        private IProgressNotifier _progressNotifier;

        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            _progressNotifier = MockRepository.GenerateMock<IProgressNotifier>();
            _subject = new TestableBDDRunner(GetType(), _progressNotifier);
        }

        #endregion

        [Test]
        [Label("LABEL-57")]
        public void Should_collect_scenario_result_for_parameterized_steps()
        {
            _subject.RunFormalizedScenario(
                given => Product_is_available_in_product_storage("wooden desk"),
                when => Customer_orders_this_product(),
                then => Customer_receives_invoice_for_product_in_amount_pounds("wooden desk", 62),
                then => Product_is_sent_to_customer());

            var result = _subject.Result.Scenarios.Single();
            Assert.That(result.Name, Is.EqualTo("Should collect scenario result for parameterized steps"));
            Assert.That(result.Status, Is.EqualTo(ResultStatus.Passed));
            Assert.That(result.Label, Is.EqualTo("LABEL-57"));
            Assert.That(result.Steps, Is.EqualTo(new[]
            {
                new StepResult(1, "GIVEN Product \"wooden desk\" is available in product storage", ResultStatus.Passed),
                new StepResult(2, "WHEN Customer orders this product", ResultStatus.Passed),
                new StepResult(3, "THEN Customer receives invoice for product \"wooden desk\" in amount \"62\" pounds", ResultStatus.Passed),
                new StepResult(4, "THEN Product is sent to customer", ResultStatus.Passed)
            }));
        }

        [Test]
        public void Should_collect_scenario_result_with_steps_parameters_inserted_in_proper_places()
        {
            _subject.RunFormalizedScenario(
                replace => Method_with_parameters_where_param_has_PARAM_value("abc"),
                insert => Method_with_parameters_where_param_is_first_param_on_list("abc_123"),
                others => Method_with_parameters_where_param_is_VALUE("abc", 5, "other param"));

            var result = _subject.Result.Scenarios.Single();
            Assert.That(result.Steps, Is.EqualTo(new[]
            {
                new StepResult(1, "REPLACE Method with parameters where param has \"abc\" value", ResultStatus.Passed),
                new StepResult(2, "INSERT Method with parameters where param \"abc_123\" is first param on list", ResultStatus.Passed),
                new StepResult(3, "OTHERS Method with parameters where param \"abc\" is \"5\" [lastParam: \"other param\"]", ResultStatus.Passed)
            }));
        }

        [Test]
        [TestCase("abc")]
        [TestCase("prod-A")]
        public void Should_capture_method_parameter(string product)
        {
            _subject.RunFormalizedScenario(call => Product_is_available_in_product_storage(product));

            var result = _subject.Result.Scenarios.Single();
            Assert.That(result.Steps, Is.EqualTo(new[]
            {
                new StepResult(1, string.Format("CALL Product \"{0}\" is available in product storage",product), ResultStatus.Passed)
            }));
        }

        [Test]
        public void Should_capture_local_variable()
        {
            string name = GetType().Name;
            _subject.RunFormalizedScenario(call => Product_is_available_in_product_storage(name));

            var result = _subject.Result.Scenarios.Single();
            Assert.That(result.Steps, Is.EqualTo(new[]
            {
                new StepResult(1, string.Format("CALL Product \"{0}\" is available in product storage",name), ResultStatus.Passed)
            }));
        }

        [Test]
        public void Should_capture_method_call()
        {
            _subject.RunFormalizedScenario(call => Product_is_available_in_product_storage(GetType().ToString()));

            var result = _subject.Result.Scenarios.Single();
            Assert.That(result.Steps, Is.EqualTo(new[]
            {
                new StepResult(1, string.Format("CALL Product \"{0}\" is available in product storage",GetType()), ResultStatus.Passed)
            }));
        }

        [Test]
        public void Should_not_allow_steps_with_ref_parameters()
        {
            int x = 5;
            var ex = Assert.Throws<ArgumentException>(() => _subject.RunFormalizedScenario(when => Set_value(ref x, 3)));
            Assert.That(ex.Message, Is.StringStarting("Steps accepting ref or out parameters are not supported:"));
        }

        [Test]
        public void Should_not_allow_steps_with_out_parameters()
        {
            int x;
            var ex = Assert.Throws<ArgumentException>(() => _subject.RunFormalizedScenario(when => int.TryParse("abc", out x)));
            Assert.That(ex.Message, Is.StringStarting("Steps accepting ref or out parameters are not supported:"));
        }

        [Test]
        public void Should_capture_parameters_just_before_call_and_evaluate_them_once()
        {
            int x = 0;
            _subject.RunFormalizedScenario(
                then => Values_equal(1, Add(ref x)),
                then => Values_equal(2, Add(ref x)),
                then => Values_equal(3, Add(ref x)));

            var result = _subject.Result.Scenarios.Single();
            Assert.That(result.Steps, Is.EqualTo(new[]
            {
                new StepResult(1, "THEN Values equal [expected: \"1\"] [actual: \"1\"]", ResultStatus.Passed),
                new StepResult(2, "THEN Values equal [expected: \"2\"] [actual: \"2\"]", ResultStatus.Passed),
                new StepResult(3, "THEN Values equal [expected: \"3\"] [actual: \"3\"]", ResultStatus.Passed)
            }));
        }

        [Test]
        [Label("LABEL-73")]
        public void Should_collect_scenario_result_for_parameterized_steps_executed_on_context()
        {
            _subject.RunFormalizedScenario<GWTContext>(
                (given, ctx) => ctx.Product_is_available_in_product_storage("wooden desk"),
                (when, ctx) => ctx.Customer_orders_this_product(),
                (then, ctx) => ctx.Customer_receives_invoice_for_product_in_amount_pounds("wooden desk", 62),
                (then, ctx) => ctx.Product_is_sent_to_customer());

            var result = _subject.Result.Scenarios.Single();
            Assert.That(result.Name, Is.EqualTo("Should collect scenario result for parameterized steps executed on context"));
            Assert.That(result.Status, Is.EqualTo(ResultStatus.Passed));
            Assert.That(result.Label, Is.EqualTo("LABEL-73"));
            Assert.That(result.Steps, Is.EqualTo(new[]
            {
                new StepResult(1, "GIVEN Product \"wooden desk\" is available in product storage", ResultStatus.Passed),
                new StepResult(2, "WHEN Customer orders this product", ResultStatus.Passed),
                new StepResult(3, "THEN Customer receives invoice for product \"wooden desk\" in amount \"62\" pounds", ResultStatus.Passed),
                new StepResult(4, "THEN Product is sent to customer", ResultStatus.Passed)
            }));
        }

        private int Add(ref int x)
        {
            return ++x;
        }

        private void Values_equal(int expected, int actual)
        {
            Assert.That(actual, Is.EqualTo(expected));
        }

        private void Set_value(ref int i, int val)
        {
            i = val;
        }

        private void Method_with_parameters_where_param_is_VALUE(string param, int value, string lastParam)
        {
        }

        private void Method_with_parameters_where_param_is_first_param_on_list(string param)
        {
        }

        private void Method_with_parameters_where_param_has_PARAM_value(string param)
        {
        }

        private void Product_is_available_in_product_storage(string product)
        {
        }

        private void Customer_orders_this_product()
        {
        }

        private void Customer_receives_invoice_for_product_in_amount_pounds(string product, int amount)
        {
        }

        private void Product_is_sent_to_customer()
        {
        }
    }

    class GWTContext
    {
        public void Product_is_available_in_product_storage(string product)
        {

        }

        public void Customer_orders_this_product()
        {

        }

        public void Customer_receives_invoice_for_product_in_amount_pounds(string product, int amount)
        {
        }

        public void Product_is_sent_to_customer()
        {
        }
    }
}