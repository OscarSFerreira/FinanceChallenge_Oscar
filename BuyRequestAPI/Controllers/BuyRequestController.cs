using AutoMapper;
using DesafioFinanceiro_Oscar.Domain.DTO_s;
using DesafioFinanceiro_Oscar.Domain.Entities;
using DesafioFinanceiro_Oscar.Domain.Validators;
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

                var buyValidator = new BuyRequestValidator();
                var buyValid = buyValidator.Validate(mapperBuy);

                var lastProdType = buyinput.Products.FirstOrDefault().ProductCategory;

                if (buyValid.IsValid)
                {

                    foreach (var product in buyinput.Products)
                    {
                        if (product.ProductCategory != lastProdType)
                        {
                            return BadRequest("A Request can't have 2 different item category!");
                        }

                        var mapperProd = _mapper.Map(product, prodReq);

                        var prodValidator = new ProductRequestValidator();
                        var prodValid = prodValidator.Validate(mapperProd);

                        if (prodValid.IsValid)
                        {
                            mapperProd.RequestId = mapperBuy.Id;
                            mapperProd.Id = Guid.NewGuid();
                            mapperProd.ProductId = Guid.NewGuid();

                            await _productRequestRepository.AddAsync(mapperProd);

                            buyReq.ProductPrices += prodReq.Total;
                        }
                        else
                        {
                            var msg = prodValid.Errors.ConvertAll(err => err.ErrorMessage.ToString());
                            return BadRequest(msg);
                        }

                    }

                    mapperBuy.Status = Status.Received;

                    await _buyRequestRepository.AddAsync(mapperBuy);
                    return Ok(mapperBuy);

                }
                else
                {
                    var msg = buyValid.Errors.ConvertAll(err => err.ErrorMessage.ToString());
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
                ProductRequest prodReq = new ProductRequest();

                var request = await _buyRequestRepository.GetByIdAsync(id);
                var products = _productRequestRepository.GetAllByRequestId(id).ToList();
                var oldStatus = request.Status;
                var totalValueOld = request.TotalPricing;

                if (request == null || products == null)
                {
                    return NotFound();
                }

                if (request.Status == Status.Finalized && buyinput.Status != Status.Finalized)
                {
                    return BadRequest("Only Delete");
                }

                if (buyinput.Products.FirstOrDefault().ProductCategory == ProductCategory.Physical)
                {
                    if (buyinput.Status == Status.WaitingDownload)
                    {
                        return BadRequest("A Physical product can't be set to Waiting To Download status!");
                    }
                }
                else
                {
                    if (buyinput.Status == Status.WaitingDelivery)
                    {
                        return BadRequest("A Digital product can't be set to Waiting To Delivery status!");
                    }
                }

                int smallerAmount = products.Count();

                if (products.Count() < buyinput.Products.Count())
                {
                    for (int i = products.Count(); i < buyinput.Products.Count(); i++)
                    {

                        var mapperProd = _mapper.Map(buyinput.Products[i], prodReq);

                        mapperProd.RequestId = id;
                        mapperProd.Id = Guid.NewGuid();
                        mapperProd.ProductId = Guid.NewGuid();

                        mapperProd.Total = mapperProd.ProductQuantity * mapperProd.ProductPrice;

                        var prodValidator = new ProductRequestValidator();
                        var prodValid = prodValidator.Validate(mapperProd);

                        if (prodValid.IsValid)
                        {
                            await _productRequestRepository.UpdateAsync(mapperProd);
                        }
                        else
                        {
                            var msg = prodValid.Errors.ConvertAll(err => err.ErrorMessage.ToString());
                            return BadRequest(msg);
                        }
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

                //if (request.Status == Status.Finalized)
                //{
                //    var client = new HttpClient();
                //    string ApiUrl = "https://localhost:44359/api/BankRequest";

                //    var bankRecord = new BankRecordDTO()
                //    {
                //        Origin = Origin.PurchaseRequest,
                //        OriginId = Guid.NewGuid(),
                //        Description = $"Purshase order id: {request.Id}",
                //        Type = DesafioFinanceiro_Oscar.Domain.Entities.Type.Payment,
                //        Amount = -request.TotalPricing
                //    };

                //    var response = await client.PostAsJsonAsync(ApiUrl, bankRecord);
                //    if (!response.IsSuccessStatusCode)
                //    {
                //        return BadRequest(response.Content.ToString());
                //    }

                //}

                var buyValidator = new BuyRequestValidator();
                var buyValid = buyValidator.Validate(mapperBuy);

                if (buyValid.IsValid)
                {
                    await _buyRequestRepository.UpdateAsync(mapperBuy);
                }
                else
                {
                    var msg = buyValid.Errors.ConvertAll(err => err.ErrorMessage.ToString());
                    return BadRequest(msg);
                }

                var recentValue = mapperBuy.TotalPricing; //valor recente (total)

                if (request.Status == Status.Finalized)
                {
                    var totalUpdated = mapperBuy.TotalPricing - totalValueOld;
                    var type = DesafioFinanceiro_Oscar.Domain.Entities.Type.Receive;
                    var amount = totalUpdated;
                    string description = $"Financial transaction order id: {request.Id}";

                    if (mapperBuy.Status == oldStatus && mapperBuy.Status == Status.Finalized && totalValueOld > mapperBuy.TotalPricing)
                    {
                        description = $"Diference purchase order id: {request.Id}";
                        amount = -totalUpdated;
                        type = DesafioFinanceiro_Oscar.Domain.Entities.Type.Payment;
                    }

                    var client = new HttpClient();
                    string ApiUrl = "https://localhost:44359/api/BankRequest";

                    var bankRecord = new BankRecordDTO()
                    {
                        Origin = Origin.PurchaseRequest,
                        OriginId = Guid.NewGuid(),
                        Description = description,
                        Type = type,
                        Amount = totalUpdated
                    };

                    var response = await client.PostAsJsonAsync(ApiUrl, bankRecord);
                    if (!response.IsSuccessStatusCode)
                        return BadRequest(response.Content.ToString());
                }

                return Ok(mapperBuy);

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
            var product = _productRequestRepository.GetAllByRequestId(id).FirstOrDefault();

            if (request == null)
            {
                return NotFound();
            }

            if (request.Status == Status.Finalized)
            {
                return Ok("Only Delete");
            }

            if (product.ProductCategory == ProductCategory.Physical)
            {
                if (state == Status.WaitingDownload)
                {
                    return BadRequest("A Physical product can't be set to WaitingDownload status!");
                }
            }
            else
            {
                if (state == Status.WaitingDelivery)
                {
                    return BadRequest("A Digital product can't be set to WaitingDelivery status!");
                }
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
                    Type = DesafioFinanceiro_Oscar.Domain.Entities.Type.Receive,
                    Amount = request.TotalPricing
                };

                var response = await client.PostAsJsonAsync(ApiUrl, bankRecord);
                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest(response.Content.ToString());
                }

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
