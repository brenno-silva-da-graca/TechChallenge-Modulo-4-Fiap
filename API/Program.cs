using Prometheus;
using System.Data;
using API.Services;
using System.Data.SqlClient;
using Application.Interfaces;
using Infrastructure.Repositories;
using TechChallenge_Contatos.Repository;
using RabbitMQ.Client;

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddScoped<IContatoCadastro, ContatoRepository>();
            builder.Services.AddScoped<IDDDCadastro, DDDRepository>();
            builder.Services.AddScoped<IConnectionFactory, ConnectionFactory>();

            var stringConexao = Environment.GetEnvironmentVariable("ConnectionStringSQL") ?? configuration.GetValue<string>("ConnectionStringSQL"); 
            
            builder.Services.AddScoped<IDbConnection>((conexao) => new SqlConnection(stringConexao));

            builder.Services.AddSingleton<MetricsService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            //Para testes apenas
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseAuthorization();

            // Adicione o middleware de m�tricas do Prometheus
            app.UseHttpMetrics(); // Middleware para m�tricas HTTP

            app.MapControllers();

            // Endpoint de m�tricas do Prometheus
            app.MapMetrics();

            app.UseMetricServer();

            app.Run();
        }
    }
}

public partial class Program { }