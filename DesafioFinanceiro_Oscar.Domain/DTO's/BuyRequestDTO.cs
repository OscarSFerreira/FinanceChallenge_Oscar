using DesafioFinanceiro_Oscar.Domain.Entities;
using System;
using System.Collections.Generic;

namespace DesafioFinanceiro_Oscar.Domain.DTO_s
{
    public class BuyRequestDTO
    {

        public long Code { get; set; }
        public DateTime Date { get; set; }
        public DateTime DeliveryDate { get; set; }
        public List<ProductRequestDTO> Products { get; set; }
        public Guid ClientId { get; set; }
        public string ClientDescription { get; set; }
        public string ClientEmail { get; set; }
        public string ClientPhone { get; set; }
        public Status Status { get; set; } //Enum
        public string? Street { get; set; }
        public string? StreetNum { get; set; }
        public string? Sector { get; set; }
        public string? Complement { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public decimal Discount { get; set; }
        public decimal CostPrice { get; set; } 

    }
}
