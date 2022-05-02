using DesafioFinanceiro_Oscar.Domain.Entities;
using System.Collections.Generic;

namespace DesafioFinanceiro_Oscar.Domain.ViewModel
{
    public class BankRecordViewModel
    {

        public List<BankRecord> BankRecords { get; set; }

        public decimal Total { get; set; }

    }

}
