using AutoMapper;
using DesafioFinanceiro_Oscar.Domain.DTO_s;
using DesafioFinanceiro_Oscar.Domain.Entities;
using DesafioFinanceiro_Oscar.Domain.Validators;
using DesafioFinanceiro_Oscar.Domain.ViewModel;
using DesafioFinanceiro_Oscar.Infrastructure.Repository.BankRecordRepository;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BankRequest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BankRequestController : ControllerBase
    {
        private readonly IBankRecordRepository _bankRecordRepository;
        private readonly IMapper _mapper;

        public BankRequestController(IBankRecordRepository bankRecordRepository, IMapper mapper)
        {
            _mapper = mapper;
            _bankRecordRepository = bankRecordRepository;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] BankRecordDTO input)
        {

            try
            {
                BankRecord rec = new BankRecord();
                var mapper = _mapper.Map(input, rec);

                var validator = new BankRecordValidator();
                var valid = validator.Validate(mapper);

                if (valid.IsValid)
                {
                    await _bankRecordRepository.AddAsync(mapper);
                    return Ok(mapper);
                }
                else
                {
                    var msg = valid.Errors.ConvertAll(err => err.ErrorMessage.ToString());
                    return BadRequest(msg);
                }

            }
            catch (Exception)
            {
                return BadRequest();
            }

        }

        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                var bankrec = _bankRecordRepository.GetAll();

                if (bankrec.Count() == 0)
                {
                    return NoContent();
                }
                else
                {
                    return Ok(bankrec);
                }

            }
            catch (Exception)
            {
                return BadRequest();
            }

        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var bankrec = await _bankRecordRepository.GetByIdAsync(id);

                if (bankrec == null)
                {
                    return NoContent();
                }
                else
                {
                    return Ok(bankrec);
                }

            }
            catch (Exception)
            {
                return BadRequest();
            }

        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {

                BankRecordViewModel bankRecord = new BankRecordViewModel();

                bankRecord.BankRecords = _bankRecordRepository.GetAll().ToList();
                bankRecord.Total = bankRecord.BankRecords.Sum(prod => prod.Amount);

                return Ok(bankRecord);
            }
            catch (Exception)
            {

                return BadRequest();
            }
        }

    }

}
