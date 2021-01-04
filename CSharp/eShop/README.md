# eShop example
The *eShop* example is a simple .NET project that demonstrates using CargoChain.

### Project stack
- ASP.NET Core MVC
- LiteDB for application storage (Embedded NoSQL database for .NET)
- CargoChain [C# Enterprise SDK](https://github.com/CargoChain/doc/wiki/C%23-SDK)

**What you will learn:**
- How to integrate CargoChain in your .NET project
- How to use the CargoChain [C# Enterprise SDK](https://github.com/CargoChain/doc/wiki/C%23-SDK)
- How to authenticate and get CargoChain access tokens in your application ([see the CargoChain authentication documentation](https://github.com/CargoChain/doc/wiki/V2-Authentication#service-based))
- How CargoChain help you to synchronize data between applications using [subscriptions](https://github.com/CargoChain/doc/wiki/V2-API#subscriptions)

## eShop applications
The *eShop* example is really basic shop that allows to add products by specifying a name, a description and a price. Then you can order the available products. When a product is ordered, the carrier specifies the position of the product and finally the product is delivered.

The *eShop* example is based on two web applications:
- **The Shop app**: the user can add products and order them
- **The Carrier app**: when a product is ordered, the carrier user can specify the position of the product (and also an optional temperature) and can specify that the product has been delivered.

### Applications workflow
The workflow of the applications is essentially based on the state of a product:
State | Description
---- | ----
`Available` | the product has been created and can be ordered by the user
`Ordered` | the product has been ordered and the carrier has to deliver it
`Delivered`| the product has been delivered to the buyer

Here is a full example that demonstrates how the _eShop_ applications work.

#### Add a new product (Shop app)


![Shop index](./images/01_shop_index.jpg)


## Visual Studio solution


## How to run the eShop example

### CargoChain Setup

### Server deployment with public access

### localhost public access
 