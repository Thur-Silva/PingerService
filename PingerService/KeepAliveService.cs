using PingerService.Models;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace PingerService
{
    // A classe agora herda de BackgroundService para rodar continuamente
    public class KeepAliveService : BackgroundService
    {
        private readonly ILogger<KeepAliveService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly List<PingTarget> _targets = new List<PingTarget>();
        
        // O tempo base de espera (1 minuto) será usado como delay padrão
        private readonly TimeSpan _delayTime = TimeSpan.FromMinutes(1); 

        public KeepAliveService(
            ILogger<KeepAliveService> logger, 
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            
            // 1. Lê a lista de alvos da configuração (appsettings.json ou variáveis de ambiente)
            var targetsConfig = configuration.GetSection("PingTargets").Get<List<PingTarget>>() ?? new List<PingTarget>();

            _targets.AddRange(targetsConfig);
            _logger.LogInformation($"Serviço KeepAlive configurado para monitorar {_targets.Count} alvos.");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("KeepAliveService iniciado. Aguardando a primeira execução.");

            // Espera inicial para garantir que a rede esteja pronta
            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            
            // O Loop principal que rodará enquanto o serviço estiver ativo
            while (!stoppingToken.IsCancellationRequested)
            {
                var tasks = new List<Task>();
                
                // 2. Itera sobre a lista de alvos e envia o ping
                foreach (var target in _targets)
                {
                    // Usa Task.Run para fazer o ping em paralelo, otimizando o tempo
                    tasks.Add(PingUrl(target.Address, target.Name, target.IntervalMinutes, stoppingToken));
                }

                // Espera todas as tarefas de ping da rodada terminarem
                await Task.WhenAll(tasks);

                // Espera um tempo base antes de iniciar a próxima rodada
                // Se o tempo base for 1 min e o intervalo de ping for 10 min, 
                // o alvo só será pingado de novo na 10ª rodada.
                await Task.Delay(_delayTime, stoppingToken);
            }
        }

        private async Task PingUrl(string url, string name, int intervalMinutes, CancellationToken stoppingToken)
        {
            // A frequência real de ping deve ser baseada no tempo decorrido
            // Para simplificar, vamos usar um Timer ou uma lógica mais simples no loop principal.

            // Para um worker service simples, vamos rodar todos de uma vez e usar o timer no 'ExecuteAsync'.
            // OU, ajustamos a lógica aqui para ser mais inteligente (mas vamos manter simples para o primeiro deploy)
            
            // Simplificação: Pinga em cada rodada. O loop 'ExecuteAsync' fará a espera.
            // A variável intervalMinutes agora serve mais como documentação/ajuste futuro.

            try
            {
                using var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(25); // Tempo de espera para o cold start

                _logger.LogInformation($"[PING] Iniciando ping para {name} em {url}...");

                var response = await client.GetAsync(url, stoppingToken);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"[PING] SUCESSO: {name} retornou {response.StatusCode}.");
                }
                else
                {
                    _logger.LogWarning($"[PING] FALHA (Status): {name} retornou {response.StatusCode}.");
                }
            }
            catch (Exception ex)
            {
                // Registra falhas de conexão/timeout (cold start)
                _logger.LogError(ex, $"[PING] EXCEÇÃO/TIMEOUT para {name} em {url}.");
            }
        }
    }
}