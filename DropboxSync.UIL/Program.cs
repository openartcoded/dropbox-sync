using DropboxSync.UIL;
using Microsoft.Extensions.DependencyInjection;

StartUp.Build();

Console.ForegroundColor = ConsoleColor.Blue;
Console.WriteLine("Dropbox synchronisation started");
Console.ForegroundColor = ConsoleColor.Gray;

BrokerEventListener brokerEventListener = StartUp.ServiceProvider?.GetService<BrokerEventListener>() ??
    throw new NullReferenceException(nameof(BrokerEventListener));

brokerEventListener.Initialize();
brokerEventListener.Start();

Console.ReadKey();