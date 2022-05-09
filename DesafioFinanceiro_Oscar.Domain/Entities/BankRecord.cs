using System;

namespace DesafioFinanceiro_Oscar.Domain.Entities
{

    public class BankRecord
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Origin? Origin { get; set; }
        public Guid? OriginId { get; set; }
        public string Description { get; set; }
        public Type Type { get; set; }
        public decimal Amount { get; set; }

    }
}
