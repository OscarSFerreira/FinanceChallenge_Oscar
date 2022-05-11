using AutoMapper;
using DesafioFinanceiro_Oscar.Domain.DTO_s;
using DesafioFinanceiro_Oscar.Domain.Entities;
using DesafioFinanceiro_Oscar.Domain.Entities.Messages;
using DesafioFinanceiro_Oscar.Domain.Validators;
using DesafioFinanceiro_Oscar.Infrastructure.Repository.BankRecordRepository;
using DesafioFinanceiro_Oscar.Infrastructure.Repository.DocumentRepository;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DocumentAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController : ControllerBase
    {

        private readonly IDocumentRepository _documentRepository;
        private readonly IBankRecordRepository _bankRecordRepository;
        private readonly IMapper _mapper;

        public BankRecord bank = new BankRecord();
        public Document doc = new Document();

        public DocumentController(IMapper mapper, IDocumentRepository documentRepository, IBankRecordRepository bankRecordRepository)
        {
            _mapper = mapper;
            _documentRepository = documentRepository;
            _bankRecordRepository = bankRecordRepository;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] DocumentDTO input)
        {
            try
            {

                var mapperDoc = _mapper.Map(input, doc);

                var validator = new DocumentValidator();
                var valid = validator.Validate(mapperDoc);

                if (valid.IsValid)
                {
                    if (mapperDoc.Paid == true)
                    {
                        var type = new DesafioFinanceiro_Oscar.Domain.Entities.Type();
                        if (mapperDoc.Operation == Operation.Entry)
                        {
                            type = DesafioFinanceiro_Oscar.Domain.Entities.Type.Payment;
                        }
                        else/*(mapperDoc.Operation == Operation.Exit)*/
                        {
                            type = DesafioFinanceiro_Oscar.Domain.Entities.Type.Receive;
                        }

                        var response = await _bankRecordRepository.CreateBankRecord(Origin.Document, mapperDoc.Id, $"Financial Transaction id: {mapperDoc.Id}",
                            type, mapperDoc.Total);

                        if (!response.IsSuccessStatusCode)
                        {
                            var result = _bankRecordRepository.BadRequestMessage(bank, response.Content.ToString());
                            return StatusCode((int)HttpStatusCode.BadRequest, result);
                        }

                    }

                    await _documentRepository.AddAsync(mapperDoc);

                }
                else
                {
                    var news = new ErrorMessage<Document>(HttpStatusCode.BadRequest.GetHashCode().ToString(), valid.Errors.ConvertAll(x => x.ErrorMessage.ToString()), doc);
                    return StatusCode((int)HttpStatusCode.BadRequest, news);
                }

                return Ok(mapperDoc);

            }
            catch (Exception ex)
            {
                var result = _documentRepository.BadRequestMessage(doc, ex.Message.ToString());
                return StatusCode((int)HttpStatusCode.BadRequest, result);
            }

        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PageParameter parameters)
        {
            try
            {
                var document = _documentRepository.GetAllWithPaging(parameters).OrderBy(doc => doc.Id).ToList();

                if (document.Count == 0)
                {
                    var result = _documentRepository.NotFoundMessage(doc);
                    return StatusCode((int)HttpStatusCode.NotFound, result);
                }

                return Ok(document);
            }
            catch (Exception ex)
            {
                var result = _documentRepository.BadRequestMessage(doc, ex.Message.ToString());
                return StatusCode((int)HttpStatusCode.BadRequest, result);
            }

        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var document = await _documentRepository.GetByIdAsync(id);

                if (document == null)
                {
                    var result = _documentRepository.NotFoundMessage(doc);
                    return StatusCode((int)HttpStatusCode.NotFound, result);
                }
                else
                {
                    return Ok(document);
                }
            }
            catch (Exception ex)
            {
                var result = _documentRepository.BadRequestMessage(doc, ex.Message.ToString());
                return StatusCode((int)HttpStatusCode.BadRequest, result);
            }
        }

        [HttpPut("ChangeDocument/{id}")]
        public async Task<IActionResult> ChangeDocument(Guid id, [FromBody] DocumentDTO input)
        {
            try //validator
            {
                var document = await _documentRepository.GetByIdAsync(id); 
                var totalValueOld = document.Total;

                if (document == null)
                {
                    var result = _documentRepository.NotFoundMessage(doc);
                    return StatusCode((int)HttpStatusCode.NotFound, result);
                }

                if (document.Paid == true && input.Paid == false)
                {
                    var result = _bankRecordRepository.BadRequestMessage(bank, "You can't change the state of a already payed Document");
                    return StatusCode((int)HttpStatusCode.BadRequest, result);
                }

                var mapperDoc = _mapper.Map(input, document);

                var validator = new DocumentValidator();
                var valid = validator.Validate(mapperDoc);

                var TotalUpdated = document.Total - totalValueOld;

                if (valid.IsValid)
                {

                    if (document.Paid == false && input.Paid == true || TotalUpdated != totalValueOld && document.Paid == true)
                    {
                        string description = $"Diference Transaction in Document id: {document.Id}";
                        var type = DesafioFinanceiro_Oscar.Domain.Entities.Type.Revert;
                        decimal total = TotalUpdated;

                        if (document.Paid == false && input.Paid == true)
                        {
                            description = $"Financial Transaction id: {document.Id}";
                            type = DesafioFinanceiro_Oscar.Domain.Entities.Type.Receive;
                            total = input.Total;
                        }

                        var response = await _bankRecordRepository.CreateBankRecord(Origin.Document, document.Id, description,
                                type, total);

                        if (!response.IsSuccessStatusCode)
                        {
                            var result = _bankRecordRepository.BadRequestMessage(bank, response.Content.ToString());
                            return StatusCode((int)HttpStatusCode.BadRequest, result);
                        }
                    }

                    await _documentRepository.UpdateAsync(mapperDoc);

                    return Ok(mapperDoc);
                }
                else
                {
                    var news = new ErrorMessage<Document>(HttpStatusCode.BadRequest.GetHashCode().ToString(), valid.Errors.ConvertAll(x => x.ErrorMessage.ToString()), doc);
                    return StatusCode((int)HttpStatusCode.BadRequest, news);
                }

            }
            catch (Exception ex)
            {
                var result = _documentRepository.BadRequestMessage(doc, ex.Message.ToString());
                return StatusCode((int)HttpStatusCode.BadRequest, result);
            }

        }

        [HttpPut("changeState/{id}")]
        public async Task<IActionResult> ChangeState(Guid id, bool Status)
        {
            try
            {
                var document = await _documentRepository.GetByIdAsync(id);

                if (document == null)
                {
                    var result = _documentRepository.NotFoundMessage(doc);
                    return StatusCode((int)HttpStatusCode.NotFound, result);
                }

                if (document.Paid == true)
                {
                    var result = _documentRepository.BadRequestMessage(doc, "You can only delete a finalized Document!");
                    return StatusCode((int)HttpStatusCode.BadRequest, result);
                }

                document.Paid = Status;

                await _documentRepository.UpdateAsync(document);

                if (document.Paid == true)
                {

                    var response = await _bankRecordRepository.CreateBankRecord(Origin.Document, document.Id, $"Financial Transaction id: {document.Id}",
                         DesafioFinanceiro_Oscar.Domain.Entities.Type.Receive, document.Total);

                    if (!response.IsSuccessStatusCode)
                    {
                        var result = _bankRecordRepository.BadRequestMessage(bank, response.Content.ToString());
                        return StatusCode((int)HttpStatusCode.BadRequest, result);
                    }
                }

                return Ok(document);

            }
            catch (Exception ex)
            {
                var result = _documentRepository.BadRequestMessage(doc, ex.Message.ToString());
                return StatusCode((int)HttpStatusCode.BadRequest, result);
            }
            
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            try
            {
                var document = await _documentRepository.GetByIdAsync(id);

                if (document == null)
                {
                    var result = _documentRepository.NotFoundMessage(doc);
                    return StatusCode((int)HttpStatusCode.NotFound, result);
                }
                else
                {
                    await _documentRepository.DeleteAsync(document);
                }

                if (document.Paid == true)
                {

                    var response = await _bankRecordRepository.CreateBankRecord(Origin.Document, document.Id, $"Revert Document order id: {document.Id}",
                        DesafioFinanceiro_Oscar.Domain.Entities.Type.Revert, -document.Total);

                    if (!response.IsSuccessStatusCode)
                    {
                        var result = _bankRecordRepository.BadRequestMessage(bank, response.Content.ToString());
                        return StatusCode((int)HttpStatusCode.BadRequest, result);
                    }

                }

                return Ok(document);

            }
            catch (Exception ex)
            {
                var result = _documentRepository.BadRequestMessage(doc, ex.Message.ToString());
                return StatusCode((int)HttpStatusCode.BadRequest, result);
            }

        }

    }

}