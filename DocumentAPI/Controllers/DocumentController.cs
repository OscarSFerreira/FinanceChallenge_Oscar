using AutoMapper;
using DesafioFinanceiro_Oscar.Domain.DTO_s;
using DesafioFinanceiro_Oscar.Domain.Entities;
using DesafioFinanceiro_Oscar.Domain.Validators;
using DesafioFinanceiro_Oscar.Infrastructure.Repository.DocumentRepository;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
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
        private readonly IMapper _mapper;

        public DocumentController(IMapper mapper, IDocumentRepository documentRepository)
        {
            _mapper = mapper;
            _documentRepository = documentRepository;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] DocumentDTO input)
        {
            try
            {
                var document = new Document();

                var mapperDoc = _mapper.Map(input, document);

                var validator = new DocumentValidator();
                var valid = validator.Validate(mapperDoc);

                if (valid.IsValid)
                {

                    if (mapperDoc.Paid == true)
                    {
                        var client = new HttpClient();
                        string ApiUrl = "https://localhost:44359/api/BankRequest";

                        BankRecordDTO bankrecordaux = new BankRecordDTO();

                        if (mapperDoc.Operation == Operation.Entry)
                        {
                            var bankRecord = new BankRecordDTO()
                            {
                                Origin = Origin.Document,
                                OriginId = Guid.NewGuid(),
                                Description = $"Financial Transaction (id: {mapperDoc.Id})",
                                Type = DesafioFinanceiro_Oscar.Domain.Entities.Type.Receive,
                                Amount = mapperDoc.Total
                            };
                            bankrecordaux = bankRecord;
                        }
                        else/*(mapperDoc.Operation == Operation.Exit)*/
                        {
                            var bankRecord = new BankRecordDTO()
                            {
                                Origin = Origin.Document,
                                OriginId = Guid.NewGuid(),
                                Description = $"Financial Transaction (id: {mapperDoc.Id})",
                                Type = DesafioFinanceiro_Oscar.Domain.Entities.Type.Payment,
                                Amount = mapperDoc.Total
                            };
                            bankrecordaux = bankRecord;
                        }

                        var response = await client.PostAsJsonAsync(ApiUrl, bankrecordaux);
                        if (!response.IsSuccessStatusCode)
                        {
                            return BadRequest(response.Content.ToString());
                        }

                    }

                    await _documentRepository.AddAsync(mapperDoc);

                }
                else
                {
                    var msg = valid.Errors.ConvertAll(err => err.ErrorMessage.ToString());
                    return BadRequest(msg);
                }

                return Ok(mapperDoc);

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

                var result = _documentRepository.GetAll().ToList();

                if (result.Count() == 0)
                {
                    return NoContent();
                }

                return Ok(result);
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
                var result = await _documentRepository.GetByIdAsync(id);

                if (result == null)
                {
                    return NoContent();
                }
                else
                {
                    return Ok(result);
                }
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [HttpPut("ChangeDocument/{id}")]
        public async Task<IActionResult> ChangeDocument(Guid id, [FromBody] DocumentDTO input)
        {

            try
            {
                var document = await _documentRepository.GetByIdAsync(id); // try catch e apagar o document
                var totalValueOld = document.Total;

                if (document == null)
                {
                    return NotFound();
                }
                
                var mapperDocument = _mapper.Map(input, document);

                var TotalUpdated = document.Total - totalValueOld;

                if (document.Paid == true)
                {
                    if (TotalUpdated != totalValueOld)
                    {
                        var client = new HttpClient();
                        string ApiUrl = "https://localhost:44359/api/BankRequest";

                        var bankRecord = new BankRecordDTO()
                        {
                            Origin = Origin.Document,
                            OriginId = Guid.NewGuid(),
                            Description = $"Diference Transaction in Document id: {document.Id}",
                            Type = DesafioFinanceiro_Oscar.Domain.Entities.Type.Revert,
                            Amount = TotalUpdated
                        };

                        var response = await client.PostAsJsonAsync(ApiUrl, bankRecord);
                        if (!response.IsSuccessStatusCode)
                        {
                            return BadRequest(response.Content.ToString());
                        }
                    }
                }

                await _documentRepository.UpdateAsync(mapperDocument);

                return Ok(mapperDocument);

            }
            catch (Exception)
            {
                return BadRequest();
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
                    return NotFound();
                }

                if (document.Paid == true)
                {
                    return Ok("Only Delete");
                }

                document.Paid = Status;

                await _documentRepository.UpdateAsync(document);

                var client = new HttpClient();
                string ApiUrl = "https://localhost:44359/api/BankRequest";

                if (document.Paid == true)
                {
                    var bankRecord = new BankRecordDTO()
                    {
                        Origin = Origin.Document,
                        OriginId = Guid.NewGuid(),
                        Description = $"Financial Transaction (id: {document.Id})",
                        Type = DesafioFinanceiro_Oscar.Domain.Entities.Type.Receive,
                        Amount = document.Total
                    };

                    var response = await client.PostAsJsonAsync(ApiUrl, bankRecord);
                    if (!response.IsSuccessStatusCode)
                    {
                        return BadRequest(response.Content.ToString()); ;
                    }
                }

                return Ok(document);

            }
            catch (Exception)
            {
                return BadRequest();
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
                    return NoContent();
                }
                else
                {
                    await _documentRepository.DeleteAsync(document);
                }

                if (document.Paid == true)
                {
                    var client = new HttpClient();
                    string ApiUrl = "https://localhost:44359/api/BankRequest";

                    var bankRecord = new BankRecordDTO()
                    {
                        Origin = Origin.Document,
                        OriginId = Guid.NewGuid(),
                        Description = $"Revert Document order id: {document.Id}",
                        Type = DesafioFinanceiro_Oscar.Domain.Entities.Type.Revert,
                        Amount = document.Total
                    };

                    var response = await client.PostAsJsonAsync(ApiUrl, bankRecord);
                    if (!response.IsSuccessStatusCode)
                    {
                        return BadRequest(response.Content.ToString());
                    }

                }

                return Ok(document);

            }
            catch (Exception)
            {
                return BadRequest();
            }

        }

    }

}
