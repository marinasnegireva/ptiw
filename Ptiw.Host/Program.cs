using Ptiw.Host;

try
{
    var builder = HostHelper.GetBuilder();

    HostHelper.ValidateConfigs();

    var host = builder.Build();

    await host.RunAsync();
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
}