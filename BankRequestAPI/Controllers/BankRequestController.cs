using AutoMapper;
using DesafioFinanceiro_Oscar.Domain.DTO_s;
using DesafioFinanceiro_Oscar.Domain.Entities;
using DesafioFinanceiro_Oscar.Domain.Entities.Messages;
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
            BankRecord bank = new BankRecord();
            try
            {
                var mapper = _mapper.Map(input, bank);

                var validator = new BankRecordValidator();
                var valid = validator.Validate(mapper);

                if (input.Origin == Origin.Null)
                {
                    mapper.OriginId = null;
                }

                if (valid.IsValid)
                {
                    await _bankRecordRepository.AddAsync(mapper);
                    return Ok(mapper);
                }
                else
                {
                    var news = new ErrorMessage<BankRecord>(HttpStatusCode.BadRequest.GetHashCode().ToString(), valid.Errors.ConvertAll(x => x.ErrorMessage.ToString()), bank);
                    return StatusCode((int)HttpStatusCode.BadRequest, news);
                }

            }
            catch (Exception ex)
            {
                var result = _bankRecordRepository.BadRequestMessage(bank, ex.Message.ToString());
                return StatusCode((int)HttpStatusCode.BadRequest, result);
            }

        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            BankRecord bank = new BankRecord();
            try
            {
                var bankrec = await _bankRecordRepository.GetByIdAsync(id);

                if (bankrec == null)
                {
                    var result = _bankRecordRepository.NotFoundMessage(bank);
                    return StatusCode((int)HttpStatusCode.NotFound, result);
                }
                else
                {
                    return Ok(bankrec);
                }

            }
            catch (Exception ex)
            {
                var result = _bankRecordRepository.BadRequestMessage(bank, ex.Message.ToString());
                return StatusCode((int)HttpStatusCode.BadRequest, result);
            }

        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PageParameter parameters)
        {
            BankRecord bank = new BankRecord();
            try
            {
                BankRecordViewModel bankRecord = new BankRecordViewModel();

                bankRecord.BankRecords = _bankRecordRepository.GetAllWithPaging(parameters).OrderBy(rec => rec.Id).ToList();

                if (bankRecord.BankRecords.Count == 0)
                {
                    var result = _bankRecordRepository.NotFoundMessage(bank);
                    return StatusCode((int)HttpStatusCode.NotFound, result);
                }

                bankRecord.Total = bankRecord.BankRecords.Sum(prod => prod.Amount);

                return Ok(bankRecord);
            }
            catch (Exception ex)
            {
                var result = _bankRecordRepository.BadRequestMessage(bank, ex.Message.ToString());
                return StatusCode((int)HttpStatusCode.BadRequest, result);
            }

        }

        [HttpGet("GetByRequestIdOrDocumentId")]
        public async Task<IActionResult> GetByIdOrDocId(Guid RequestId, Guid DocId)
        {
            try
            {
                
                if (RequestId != Guid.Empty)
                {
                    BuyRequest bank = new BuyRequest();

                    bank = await _buyRequestRepository.GetByIdAsync(RequestId);

                    if (bank == null)
                    {
                        var result = _buyRequestRepository.NotFoundMessage(bank);
                        return StatusCode((int)HttpStatusCode.NotFound, result);
                    }
                    return Ok(bank);
                }
                else
                {
                    Document document = new Document();

                    document = await _documentRepository.GetByIdAsync(DocId);

                    if (document == null)
                    {
                        var result = _documentRepository.NotFoundMessage(document);
                        return StatusCode((int)HttpStatusCode.NotFound, result);
                    }
                    return Ok(document);
                }

            }
            catch (Exception ex)
            {
                BankRecord bank = new BankRecord();
                var result = _bankRecordRepository.BadRequestMessage(bank, ex.Message.ToString());
                return StatusCode((int)HttpStatusCode.BadRequest, result);
            }
        }

        [HttpPut("ChangeBankRequest/{id}")]
        public async Task<IActionResult> ChangeBankRequest(Guid id, [FromBody] BankRecordDTO bankRecord)
        {
            BankRecord bank = new BankRecord();
            try
            {
                var bankReqUpdate = await _bankRecordRepository.GetByIdAsync(id);
                if (bankReqUpdate == null)
                {
                    var result = _bankRecordRepository.NotFoundMessage(bank);
                    return StatusCode((int)HttpStatusCode.NotFound, result);
                }

                if (bankReqUpdate.OriginId != null)
                {
                    var result = _bankRecordRepository.BadRequestMessage(bank, "The permissions do not allow you to change this data!");
                    return StatusCode((int)HttpStatusCode.BadRequest, result);
                }

                var mapBankRecord = _mapper.Map(bankRecord, bankReqUpdate);

                var validator = new BankRecordValidator();
                var valid = validator.Validate(mapBankRecord);

                if (valid.IsValid)
                {
                    await _bankRecordRepository.UpdateAsync(bankReqUpdate);
                    return Ok(bankReqUpdate);
                }
                else
                {
                    var news = new ErrorMessage<BankRecord>(HttpStatusCode.BadRequest.GetHashCode().ToString(), valid.Errors.ConvertAll(x => x.ErrorMessage.ToString()), bank);
                    return StatusCode((int)HttpStatusCode.BadRequest, news);
                }
            }
            catch (Exception ex)
            {
                var result = _bankRecordRepository.BadRequestMessage(bank, ex.Message.ToString());
                return StatusCode((int)HttpStatusCode.BadRequest, result);
            }
        }

    }
}
