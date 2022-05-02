using DesafioFinanceiro_Oscar.Domain.Entities;
using System;

namespace DesafioFinanceiro_Oscar.Domain.DTO_s
{
    public class DocumentDTO
    {

        public string Number { get; set; }
        public DateTime Date { get; set; }
        public DocType DocType { get; set; }
        public Operation Operation { get; set; }
        public bool Paid { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string Description { get; set; }
        public decimal Total { get; set; }
        public string? Observation { get; set; }

    }
}
