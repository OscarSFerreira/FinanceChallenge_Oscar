using AutoMapper;
using DesafioFinanceiro_Oscar.Domain.DTO_s;
using DesafioFinanceiro_Oscar.Domain.Entities;

namespace DesafioFinanceiro_Oscar.Domain.Mapping
{
    public class MappingProfile : Profile
    {

        public MappingProfile()
        {
            CreateMap<BankRecord, BankRecordDTO>().ReverseMap();
            CreateMap<BuyRequest, BuyRequestDTO>().ReverseMap();
            CreateMap<ProductRequest, ProductRequestDTO>().ReverseMap();
            CreateMap<Document, DocumentDTO>().ReverseMap();
        }

    }
}
