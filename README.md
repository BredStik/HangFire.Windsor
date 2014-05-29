HangFire.Windsor
================



[HangFire](http://hangfire.io) background job activator based on 
[Castle Windsor](http://docs.castleproject.org/Windsor.MainPage.ashx) IoC Container. 
Installation
--------------

HangFire.Windsor is available as a NuGet Package. Type the following
command into NuGet Package Manager Console window to install it:

```
Install-Package HangFire.Windsor
```

Usage
------

In order to use the library, you should register it as your
JobActivator class:

```csharp
// Global.asax.cs or other file that initializes Windsor bindings.
public partial class MyApplication : System.Web.HttpApplication
{
    protected void Application_Start()
    {
    var container = new WindsorContainer();            

    /* Register types */
    /* container.Register(Component.For<ISomeInterface>().ImplementedBy<SomeImplementation>()); */
		
		JobActivator.Current = new WindsorJobActivator(container.Kernel);
    }
}
```
