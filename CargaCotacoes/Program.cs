using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace CargaCotacoes
{
    class Program
    {
        static void Main()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.json");
            var configuration = builder.Build();


            Console.WriteLine("Imagem do site de testes no Docker Hub: " +
                "renatogroffe/site-indicadores-economia-nginx");
            Console.WriteLine("Iniciando a extração das cotações...");

            var seleniumConfigurations = new SeleniumConfigurations();
            new ConfigureFromConfigurationOptions<SeleniumConfigurations>(
                configuration.GetSection("SeleniumConfigurations"))
                    .Configure(seleniumConfigurations);

            var pagina = new PaginaCotacoes(seleniumConfigurations);
            pagina.CarregarPagina();
            var cotacoesMoedasEstrangeiras =
                pagina.ObterCotacoesMoedasEstrangeiras();
            var cotacaoBitcoin =
                pagina.ObterCotacaoBitcoin();
            pagina.Fechar();

            Console.WriteLine("Gravando cotações...");
            var cotacoesContext = new CotacoesContext(configuration);

            cotacoesContext
                .IncluirCotacoes<CotacaoMoedaEstrangeira>(cotacoesMoedasEstrangeiras);
            cotacoesContext.IncluirCotacoes<CotacaoBitcoin>(
                new CotacaoBitcoin[] { cotacaoBitcoin });
            Console.WriteLine("Carga de cotações realizada no Redis...");
        }
    }
}