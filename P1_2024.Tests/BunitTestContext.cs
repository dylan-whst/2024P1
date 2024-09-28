using MudBlazor;
using MudBlazor.Services;
using NUnit.Framework;

namespace BlazorTestProject1;

public abstract class BunitTestContext : TestContextWrapper
{
    [SetUp]
    public void Setup()
    {
        TestContext = new Bunit.TestContext();

        JSInterop.SetupVoid("setup", _ => true);
        JSInterop.SetupVoid("mudPopover.initialize", _ => true);
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
    }
}
