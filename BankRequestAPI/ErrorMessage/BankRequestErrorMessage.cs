using DesafioFinanceiro_Oscar.Domain.Entities;
using System.Collections.Generic;

namespace BankRequestAPI.ErrorMessage
{
    public class BankRequestErrorMessage : ErrorMessage<BankRecord>
    {

        public BankRequestErrorMessage(string code, List<string> message, BankRecord contract) : base(code, message, contract)
        {

        }

    }
}
