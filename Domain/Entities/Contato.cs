﻿using System.Text.Json.Serialization;

namespace Domain.Entities
{
    public class Contato
    {
        [JsonIgnore]
        public int Id { get; set; }

        public string Nome { get; set; }

        public string Telefone { get; set; }

        public string Email { get; set; }

        [JsonIgnore]
        public int DDDID { get; set; }
    }
}
