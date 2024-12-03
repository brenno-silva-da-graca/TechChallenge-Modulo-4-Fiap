﻿using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Text;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContatosController : ControllerBase
    {
        private readonly IContatoCadastro _contatoCadastro;
        private readonly IConnectionFactory _rabbitConnectionFactory;

        public ContatosController(IContatoCadastro contatoCadastro, IConnectionFactory rabbitConnectionFactory)
        {
            string hostNameRabbitMQ = Environment.GetEnvironmentVariable("RabbitMQ__Host") ?? "rabbitmq-service";
            string portRabbitMQ = Environment.GetEnvironmentVariable("RabbitMQ__Port") ?? "5672";
            _contatoCadastro = contatoCadastro;
            _rabbitConnectionFactory = new ConnectionFactory { HostName = hostNameRabbitMQ, Port = int.Parse(portRabbitMQ) };
        }

        [HttpGet("Listar")]
        public IActionResult GetContato()
        {
            return Ok(_contatoCadastro.ListarContatos());
        }

        [HttpGet("ListarPorDDD")]
        public IActionResult ListarPorDDD(int NumDDD)
        {
            return Ok(_contatoCadastro.ListarPorDDD(NumDDD));
        }

        [HttpPost("Inserir")]
        public IActionResult PostContato([FromBody] Contato dadosContato)
        {
            using var connection = _rabbitConnectionFactory.CreateConnection();
            using var channel = connection.CreateModel();
            
            channel.QueueDeclare(queue: "PostContato",
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

            var body = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(dadosContato));

            channel.BasicPublish(exchange: string.Empty,
                                 routingKey: "PostContato",
                                 basicProperties: null,
                                 body: body);

            return Ok(dadosContato);
        }

        [HttpPut("Atualizar")]
        public IActionResult PutContato([FromBody] Contato dadosContato, int id)
        {
            var contatoDTO = new ContatoDTO{
                Id = id,
                Nome = dadosContato.Nome,
                Email=dadosContato.Email,
                Telefone = dadosContato.Telefone
            };

            using var connection = _rabbitConnectionFactory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "PatchContato",
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

            var body = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(contatoDTO));

            channel.BasicPublish(exchange: string.Empty,
                                 routingKey: "PatchContato",
                                 basicProperties: null,
                                 body: body);

            return Ok(dadosContato);
        }

        [HttpDelete("Deletar")]
        public IActionResult DeleteContato(int Id)
        {
            using var connection = _rabbitConnectionFactory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "DeleteContato",
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

            var body = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(Id));

            channel.BasicPublish(exchange: string.Empty,
                                 routingKey: "DeleteContato",
                                 basicProperties: null,
                                 body: body);

            return Ok(Id);
        }
    }
}
