using AutoMapper;
using DesafioFinanceiro_Oscar.Domain.DTO_s;
using DesafioFinanceiro_Oscar.Domain.Entities;
using DesafioFinanceiro_Oscar.Infrastructure.Repository.BuyRequestRepository;
using DesafioFinanceiro_Oscar.Infrastructure.Repository.ProductRequestRepository;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BuyRequestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BuyRequestController : ControllerBase
    {

        private readonly IBuyRequestRepository _buyRequestRepository;
        private readonly IProductRequestRepository _productRequestRepository;
        private readonly IMapper _mapper;

        public BuyRequestController(IBuyRequestRepository buyRequestRepository, IProductRequestRepository productRequestRepository, IMapper mapper)
        {
            _mapper = mapper;
            _buyRequestRepository = buyRequestRepository;
            _productRequestRepository = productRequestRepository;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] BuyRequestDTO buyinput)
        {

            try
            {
                BuyRequest buyReq = new BuyRequest();
                ProductRequest prodReq = new ProductRequest();

                var mapperBuy = _mapper.Map(buyinput, buyReq);

                foreach (var product in buyinput.Products)
                {
                    prodReq.Total = product.ProductQuantity * product.ProductPrice;

                    var mapperProd = _mapper.Map(product, prodReq);

                    mapperProd.RequestId = mapperBuy.Id;
                    mapperProd.Id = Guid.NewGuid();
                    mapperProd.ProductId = Guid.NewGuid();

                    await _productRequestRepository.AddAsync(mapperProd);

                    buyReq.ProductPrices += product.ProductPrice;
                }

                //buyReq.TotalPricing = buyReq.ProductPrices

                //var validator = new BankRecordValidator();
                //var valid = validator.Validate(mapper);

                //if (valid.IsValid)
                //{
                await _buyRequestRepository.AddAsync(mapperBuy);
                return Ok(mapperBuy);
                //}
                //else
                //{
                //var msg = valid.Errors.ConvertAll(err => err.ErrorMessage.ToString());
                //return BadRequest(/*msg*/);
                //}

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
                var bankrec = await _buyRequestRepository.GetByIdAsync(id);

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

                var result = _buyRequestRepository.GetAll();

                return Ok(result);
            }
            catch (Exception)
            {
                return BadRequest();
            }

        }

        [HttpGet("GetByClientIdAsync/{clientId}")]
        public async Task<IActionResult> GetByClientIdAsync(Guid clientId)
        {
            try
            {
                var record = await _buyRequestRepository.GetByClientIdAsync(clientId);

                if (record == null)
                    return NoContent();
                else
                    return Ok(record);

            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] BuyRequestDTO buyinput)
        {

            try
            {
                //BuyRequest buyReq = new BuyRequest();
                ProductRequest prodReq = new ProductRequest();

                //var mapperBuy = _mapper.Map(buyinput, buyReq);

                var request = await _buyRequestRepository.GetByIdAsync(id);
                var products = _productRequestRepository.GetAllByRequestId(id).ToList();

                int smallerAmount = 0;

                if (products.Count() == buyinput.Products.Count())
                {
                    smallerAmount = products.Count();
                }
                else if (products.Count() < buyinput.Products.Count())
                {
                    smallerAmount = products.Count();

                    for (int i = products.Count(); i < buyinput.Products.Count(); i++)
                    {

                        var mapperProd = _mapper.Map(buyinput.Products[i], prodReq);

                        mapperProd.RequestId = id;
                        mapperProd.Id = Guid.NewGuid();
                        mapperProd.ProductId = Guid.NewGuid();

                        mapperProd.Total = mapperProd.ProductQuantity * mapperProd.ProductPrice;

                        await _productRequestRepository.UpdateAsync(mapperProd);

                    }

                }
                else if (products.Count() > buyinput.Products.Count())
                {
                    smallerAmount = buyinput.Products.Count();

                    for (int i = buyinput.Products.Count(); i < products.Count(); i++)
                    {
                        await _productRequestRepository.DeleteAsync(products[i]);
                    }

                }

                request.ProductPrices = 0;

                for (int i = 0; i < smallerAmount; i++)
                {
                    products[i].Total = buyinput.Products[i].ProductPrice * buyinput.Products[i].ProductQuantity;
                    var mapperProd = _mapper.Map(buyinput.Products[i], products[i]);
                    mapperProd.ProductId = Guid.NewGuid();

                    await _productRequestRepository.UpdateAsync(mapperProd);

                    request.ProductPrices += products[i].Total;
                }

                request.TotalPricing = request.ProductPrices * (request.ProductPrices * (request.Discount / 100));

                var mapperBuy = _mapper.Map(buyinput, request);
                await _buyRequestRepository.UpdateAsync(mapperBuy);
                return Ok(mapperBuy);


                //if (products.Count() > buyinput.Products.Count())
                //{
                //    for (int i = buyinput.Products.Count(); i <= products.Count(); i++)
                //    {
                //        await _productRequestRepository.DeleteAsync(products[i]);
                //    }
                //}

                //if (products.Count() > buyinput.Products.Count())
                //{
                //    for (int i = buyinput.Products.Count(); i <= products.Count(); i++)
                //    {
                //        var mapperProd = _mapper.Map(buyinput.Products[i], prodReq);
                //        await _productRequestRepository.AddAsync(mapperProd);
                //    }
                //}

                //await _buyRequestRepository.AddAsync(mapperBuy);
                //return Ok(mapperBuy);

            }
            catch (Exception)
            {
                return BadRequest();
            }

        }

        [HttpPut("changeState/{id}")]
        public async Task<IActionResult> ChangeState(Guid id, Status state)
        {
            var request = await _buyRequestRepository.GetByIdAsync(id);
            if (request == null)
            {
                return NotFound();
            }
            if (request.Status == Status.Finalized)
            {
                return Ok("Only Delete");
            }
            request.Status = state;

            await _buyRequestRepository.UpdateAsync(request);

            if (request.Status == Status.Finalized)
            {
                var client = new HttpClient();
                string ApiUrl = "https://localhost:44359/api/BankRequest";

                var bankRecord = new BankRecordDTO()
                {
                    Origin = Origin.PurchaseRequest,
                    OriginId = Guid.NewGuid(),
                    Description = $"Purshase order id: {request.Id}",
                    Type = DesafioFinanceiro_Oscar.Domain.Entities.Type.Payment,
                    Amount = -request.TotalPricing
                };

                var response = await client.PostAsJsonAsync(ApiUrl, bankRecord);
                if (!response.IsSuccessStatusCode)
                    return BadRequest(response.Content.ToString());
            }

            return Ok(request);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            try
            {
                var result = await _buyRequestRepository.GetByIdAsync(id);
                var productCascadeDelete = _productRequestRepository.GetAllByRequestId(id);

                if (result == null)
                {
                    return NoContent();
                }
                else
                {
                    foreach (var product in productCascadeDelete)
                    {
                        _productRequestRepository.DeleteAsync(product);
                    }
                }
                //_productRequestRepository.DeleteAsync(productCascadeDelete);

                await _buyRequestRepository.DeleteAsync(result);

                if (result.Status == Status.Finalized)
                {
                    
                    var client = new HttpClient();
                    string ApiUrl = "https://localhost:44359/api/BankRequest";

                    var bankRecord = new BankRecordDTO()
                    {
                        Origin = Origin.Document,
                        OriginId = Guid.NewGuid(),
                        Description = $"Revert Purshase order id: {result.Id}",
                        Type = DesafioFinanceiro_Oscar.Domain.Entities.Type.Revert,
                        Amount = -result.TotalPricing
                    };

                    var response = await client.PostAsJsonAsync(ApiUrl, bankRecord);
                    if (!response.IsSuccessStatusCode)
                    {
                        return BadRequest(response.Content.ToString());
                    }

                }
                return Ok(result);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

    }
}
