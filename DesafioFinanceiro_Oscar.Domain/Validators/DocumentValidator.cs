using DesafioFinanceiro_Oscar.Domain.Entities;
using FluentValidation;
using System;

namespace DesafioFinanceiro_Oscar.Domain.Validators
{
    public class DocumentValidator : AbstractValidator<Document>
    {

        public DocumentValidator()
        {

            RuleFor(x => x.Number)
               .NotEmpty().WithMessage("Number field is required");

            RuleFor(x => x.Date)
               .NotNull().WithMessage("Date field is required");

            RuleFor(x => x.DocType)
               .NotNull().WithMessage("Document Type field is required")
               .IsInEnum().WithMessage("Invalid Type");

            RuleFor(x => x.Operation)
               .NotNull().WithMessage("Operation field is required")
               .IsInEnum().WithMessage("Invalid Type")
               .When(x => x.Operation == Operation.Entry && x.Total < 0, ApplyConditionTo.CurrentValidator).WithMessage("Total Amount must be positive!")
               .When(x => x.Operation == Operation.Exit && x.Total > 0, ApplyConditionTo.CurrentValidator).WithMessage("Total Amount must be negative!");

            RuleFor(x => x.Paid)
               .NotEmpty().WithMessage("Paid field is required");

            RuleFor(x => x.Description)
              .NotEmpty().WithMessage("Description field is required");

        }
        

    }
}
