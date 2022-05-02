using DesafioFinanceiro_Oscar.Domain.Entities;

namespace DesafioFinanceiro_Oscar.Domain.DTO_s
{
    public class ProductRequestDTO
    {

        public string ProductDescription { get; set; }
        public ProductCategory ProductCategory { get; set; } //enum
        public decimal ProductQuantity { get; set; }
        public decimal ProductPrice { get; set; } //valor*quantidade

    }
}
