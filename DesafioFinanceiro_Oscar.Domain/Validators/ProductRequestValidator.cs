using DesafioFinanceiro_Oscar.Domain.Entities;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesafioFinanceiro_Oscar.Domain.Validators
{
    public class ProductRequestValidator : AbstractValidator<ProductRequest>
    {

        public ProductRequestValidator()
        {

            RuleFor(x => x.ProductDescription)
              .NotEmpty().WithMessage("Product Description field is required");

            RuleFor(x => x.ProductCategory)
               .NotNull().WithMessage("Operation field is required")
               .IsInEnum().WithMessage("Invalid Product Category");
               
               //When(x => x.ProductCategory == ProductCategory.Physical, () =>
               //{
               //    RuleFor(x => x.ProductCategory == );
               //});

            RuleFor(x => x.ProductQuantity)
               .NotNull().WithMessage("Product Quantity field is required")
               .GreaterThan(0).WithMessage("Product Quantity must be higher than 0");

            RuleFor(x => x.ProductPrice)
               .NotNull().WithMessage("Product Price field is required")
               .GreaterThan(0).WithMessage("Product Price must be higher than 0");

            RuleFor(x => x.Total)
               .NotNull().WithMessage("Total field is required")
               .GreaterThan(0).WithMessage("Total must be higher than 0");

        }

    }
}
