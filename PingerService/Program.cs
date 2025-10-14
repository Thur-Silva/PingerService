using PingerService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        // Adicionar o HttpClient para fazer as requisições
        services.AddHttpClient(); 
        
        // Registrar o nosso serviço de background KeepAlive
        services.AddHostedService<KeepAliveService>();
    })
    .Build()
    .Run();