using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace CargaCotacoes
{
    public class CotacoesContext
    {
        private IConfiguration _config;

        public CotacoesContext(IConfiguration config)
        {
            _config = config;
        }

        public void IncluirCotacoes<T>(IEnumerable<T> dadosCotacoes)
        {
            var conexaoRedis = ConnectionMultiplexer.Connect(
                _config.GetConnectionString("RedisServer"));

            var dbRedis = conexaoRedis.GetDatabase();
            foreach (var cotacao in dadosCotacoes)
            {
                string key;
                if (cotacao is CotacaoMoedaEstrangeira cotacaoMoedaEstr)
                    key = cotacaoMoedaEstr.Codigo;
                else if (cotacao is CotacaoBitcoin)
                    key = "BITCOIN";
                else
                    key = string.Empty;

                dbRedis.StringSet(
                    "Cotacao-" + key,
                    JsonSerializer.Serialize(cotacao),
                    expiry: null);
            }
        }
    }
}