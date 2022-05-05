using AutoMapper;
using DesafioFinanceiro_Oscar.Domain.DTO_s;
using DesafioFinanceiro_Oscar.Domain.Entities;
using DesafioFinanceiro_Oscar.Domain.Validators;
using DesafioFinanceiro_Oscar.Domain.ViewModel;
using DesafioFinanceiro_Oscar.Infrastructure.Repository.BankRecordRepository;
using DesafioFinanceiro_Oscar.Infrastructure.Repository.BuyRequestRepository;
using DesafioFinanceiro_Oscar.Infrastructure.Repository.DocumentRepository;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace BankRequestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BankRequestController : ControllerBase
    {

        private readonly IBankRecordRepository _bankRecordRepository;
        private readonly IDocumentRepository _documentRepository;
        private readonly IBuyRequestRepository _buyRequestRepository;
        private readonly IMapper _mapper;

        public BankRequestController(IBankRecordRepository bankRecordRepository, IDocumentRepository documentRepository, IMapper mapper, IBuyRequestRepository buyRequestRepository)
        {
            _mapper = mapper;
            _bankRecordRepository = bankRecordRepository;
            _documentRepository = documentRepository;
            _buyRequestRepository = buyRequestRepository;
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

                mapper.OriginId = null;
                mapper.Origin = Origin.Null;

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

        [HttpGet("GetByRequestIdOrDocumentId")]
        public async Task<IActionResult> GetByIdOrDocId(Guid RequestId, Guid DocId)
        {
            try
            {
                BuyRequest request;
                Document document;

                if (RequestId != Guid.Empty)
                {
                    request = await _buyRequestRepository.GetByIdAsync(RequestId);
                    return request == null ? NoContent() : Ok(request);
                }
                else
                {
                    document = await _documentRepository.GetByIdAsync(DocId);
                    return document == null ? NoContent() : Ok(document);
                }

            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [HttpPut("ChangeBankRequest/{id}")]
        public async Task<IActionResult> ChangeBankRequest(Guid id, [FromBody] BankRecordDTO bankRecord)
        {
            try
            {
                BankRecord bank = new BankRecord();

                var bankRecordUpdate = await _bankRecordRepository.GetByIdAsync(id);
                if (bankRecordUpdate == null)
                {
                    //List<string> returnList = new List<string>();
                    //returnList.Add("No data available with this id in database");
                    //var news = new BankRecordErrorMessage(HttpStatusCode.NotFound.GetHashCode().ToString(), returnList, null);
                    //return StatusCode((int)HttpStatusCode.NotFound, news);
                    return BadRequest();
                }

                if (bankRecordUpdate.OriginId != null)
                {
                    return BadRequest("Não é possivel alterar");
                }

                var mapBankRecord = _mapper.Map(bankRecord, bankRecordUpdate);

                var validator = new BankRecordValidator();
                var validation = validator.Validate(mapBankRecord);

                if (validation.IsValid)
                {
                    await _bankRecordRepository.UpdateAsync(bankRecordUpdate);
                    return Ok(bankRecordUpdate);
                }
                else
                {
                    //var news = new BankRecordErrorMessage(HttpStatusCode.BadRequest.GetHashCode().ToString(),
                    //    validation.Errors.ConvertAll(x => x.ErrorMessage.ToString()), bankRecordUpdate);
                    //return StatusCode((int)HttpStatusCode.BadRequest, news);
                    return BadRequest("Nop");
                }
            }
            catch (Exception ex)
            {
                //List<string> returnList = new List<string>();
                //returnList.Add(ex.Message);
                //var news = new BankRecordErrorMessage(HttpStatusCode.BadRequest.GetHashCode().ToString(), returnList, null);
                //return StatusCode((int)HttpStatusCode.BadRequest, news);
                return BadRequest();
            }
        }

    }
}
