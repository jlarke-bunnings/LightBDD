﻿using NUnit.Framework;

namespace LightBDD.Example.Features
{
    [FeatureDescription(
@"In order to pay for products
As a customer
I want to receive invoice for bought items")]
    [TestFixture]
    public partial class Invoice_feature
    {
        [Test]
        public void Receiving_invoice_for_products()
        {
            Runner.RunFormalizedScenario(
                given => Product_is_available_in_products_storage("wooden desk"),
                and => Product_is_available_in_products_storage("wooden shelf"),
                when => Customer_buys_product("wooden desk"),
                and => Customer_buys_product("wooden shelf"),
                then => Invoice_is_sent_to_customer(),
                and => Invoice_contains_product_with_price_of_AMOUNT_pounds("wooden desk", 62),
                and => Invoice_contains_product_with_price_of_AMOUNT_pounds("wooden shelf", 37));
        }
    }
}