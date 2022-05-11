using AutoMapper;
using DesafioFinanceiro_Oscar.Domain.DTO_s;
using DesafioFinanceiro_Oscar.Domain.Entities;
using DesafioFinanceiro_Oscar.Domain.Entities.Messages;
using DesafioFinanceiro_Oscar.Domain.Validators;
using DesafioFinanceiro_Oscar.Infrastructure.Repository.BankRecordRepository;
using DesafioFinanceiro_Oscar.Infrastructure.Repository.BuyRequestRepository;
using DesafioFinanceiro_Oscar.Infrastructure.Repository.ProductRequestRepository;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace BuyRequestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BuyRequestController : ControllerBase
    {

        private readonly IBuyRequestRepository _buyRequestRepository;
        private readonly IProductRequestRepository _productRequestRepository;
        private readonly IBankRecordRepository _bankRecordRepository;
        private readonly IMapper _mapper;

        public BuyRequest buyReq = new BuyRequest();
        public ProductRequest prodReq = new ProductRequest();
        public BankRecord bank = new BankRecord();

        public BuyRequestController(IBuyRequestRepository buyRequestRepository, IProductRequestRepository productRequestRepository, IMapper mapper, IBankRecordRepository bankRecordRepository)
        {
            _mapper = mapper;
            _bankRecordRepository = bankRecordRepository;
            _buyRequestRepository = buyRequestRepository;
            _productRequestRepository = productRequestRepository;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] BuyRequestDTO buyinput)
        {
            try
            {

                var mapperBuy = _mapper.Map(buyinput, buyReq);

                var buyValidator = new BuyRequestValidator();
                var buyValid = buyValidator.Validate(mapperBuy);

                var lastProdType = buyinput.Products.FirstOrDefault().ProductCategory;

                if (buyValid.IsValid)
                {
                    await _buyRequestRepository.AddAsync(mapperBuy);
                    foreach (var product in buyinput.Products)
                    {
                        if (product.ProductCategory != lastProdType)
                        {
                            var result = _buyRequestRepository.BadRequestMessage(buyReq, "A Request can't have 2 different item category!");
                            return StatusCode((int)HttpStatusCode.BadRequest, result);
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
                            var news = new ErrorMessage<ProductRequest>(HttpStatusCode.BadRequest.GetHashCode().ToString(), prodValid.Errors.ConvertAll(x => x.ErrorMessage.ToString()), prodReq);
                            return StatusCode((int)HttpStatusCode.BadRequest, news);
                        }

                    }

                    mapperBuy.Status = Status.Received;
                    mapperBuy.TotalPricing = buyReq.ProductPrices - (buyReq.ProductPrices * (buyReq.Discount / 100));

                    await _buyRequestRepository.UpdateAsync(mapperBuy);
                    return Ok(mapperBuy);

                }
                else
                {
                    var news = new ErrorMessage<BuyRequest>(HttpStatusCode.BadRequest.GetHashCode().ToString(), buyValid.Errors.ConvertAll(x => x.ErrorMessage.ToString()), buyReq);
                    return StatusCode((int)HttpStatusCode.BadRequest, news);
                }

            }
            catch (Exception ex)
            {
                var result = _buyRequestRepository.BadRequestMessage(buyReq, ex.Message.ToString());
                return StatusCode((int)HttpStatusCode.BadRequest, result);
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
                    var result = _buyRequestRepository.NotFoundMessage(buyReq);
                    return StatusCode((int)HttpStatusCode.NotFound, result);
                }
                else
                {
                    return Ok(bankrec);
                }

            }
            catch (Exception ex)
            {
                var result = _buyRequestRepository.BadRequestMessage(buyReq, ex.Message.ToString());
                return StatusCode((int)HttpStatusCode.BadRequest, result);
            }

        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PageParameter parameters)
        {
            try
            {

                var buyRequest = _buyRequestRepository.GetAllWithPaging(parameters).OrderBy(buy => buy.Id).ToList();

                if (buyRequest.Count == 0)
                {
                    var result = _buyRequestRepository.NotFoundMessage(buyReq);
                    return StatusCode((int)HttpStatusCode.NotFound, result);
                }

                return Ok(buyRequest);
            }
            catch (Exception ex)
            {
                var result = _buyRequestRepository.BadRequestMessage(buyReq, ex.Message.ToString());
                return StatusCode((int)HttpStatusCode.BadRequest, result);
            }

        }

        [HttpGet("GetByClientIdAsync/{clientId}")]
        public async Task<IActionResult> GetByClientIdAsync(Guid clientId)
        {
            try
            {
                var record = await _buyRequestRepository.GetByClientIdAsync(clientId);

                if (record == null)
                {
                    var result = _buyRequestRepository.NotFoundMessage(buyReq);
                    return StatusCode((int)HttpStatusCode.NotFound, result);
                }
                else
                {
                    return Ok(record);
                }

            }
            catch (Exception ex)
            {
                var result = _buyRequestRepository.BadRequestMessage(buyReq, ex.Message.ToString());
                return StatusCode((int)HttpStatusCode.BadRequest, result);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] BuyRequestDTO buyinput)
        {

            try
            {
                var request = await _buyRequestRepository.GetByIdAsync(id);
                var products = _productRequestRepository.GetAllByRequestId(id).ToList();

                if (request == null || products == null)
                {
                    var result = _buyRequestRepository.NotFoundMessage(buyReq);
                    return StatusCode((int)HttpStatusCode.NotFound, result);
                }

                if (request.Status == Status.Finalized && buyinput.Status != Status.Finalized)
                {
                    var result = _buyRequestRepository.BadRequestMessage(buyReq, "You can only delete a finalized Request!");
                    return StatusCode((int)HttpStatusCode.BadRequest, result);
                }

                if (buyinput.Products.FirstOrDefault().ProductCategory == ProductCategory.Physical)
                {
                    if (buyinput.Status == Status.WaitingDownload)
                    {
                        var result = _buyRequestRepository.BadRequestMessage(buyReq, "A Physical product can't be set to Waiting To Download status!");
                        return StatusCode((int)HttpStatusCode.BadRequest, result);
                    }
                }
                else
                {
                    if (buyinput.Status == Status.WaitingDelivery)
                    {
                        var result = _buyRequestRepository.BadRequestMessage(buyReq, "A Digital product can't be set to Waiting To Delivery status!");
                        return StatusCode((int)HttpStatusCode.BadRequest, result);
                    }
                }

                var oldStatus = request.Status;
                var totalValueOld = request.TotalPricing;
                int smallerAmount = products.Count();
                request.ProductPrices = 0;

                if (products.Count() < buyinput.Products.Count())
                {
                    for (int i = products.Count(); i < buyinput.Products.Count(); i++)
                    {

                        var mapperProd = _mapper.Map(buyinput.Products[i], prodReq);

                        mapperProd.RequestId = id;
                        mapperProd.Id = Guid.NewGuid();
                        mapperProd.ProductId = Guid.NewGuid();
                        mapperProd.Total = mapperProd.ProductQuantity * mapperProd.ProductPrice;
                        request.ProductPrices += mapperProd.Total;

                        var prodValidator = new ProductRequestValidator();
                        var prodValid = prodValidator.Validate(mapperProd);

                        if (prodValid.IsValid)
                        {
                            await _productRequestRepository.AddAsync(mapperProd);
                        }
                        else
                        {
                            var news = new ErrorMessage<ProductRequest>(HttpStatusCode.BadRequest.GetHashCode().ToString(), prodValid.Errors.ConvertAll(x => x.ErrorMessage.ToString()), prodReq);
                            return StatusCode((int)HttpStatusCode.BadRequest, news);
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


                for (int i = 0; i < smallerAmount; i++)
                {
                    products[i].Total = buyinput.Products[i].ProductPrice * buyinput.Products[i].ProductQuantity;
                    var mapperProd = _mapper.Map(buyinput.Products[i], products[i]);
                    mapperProd.ProductId = Guid.NewGuid();

                    await _productRequestRepository.UpdateAsync(mapperProd);

                    request.ProductPrices += products[i].Total;
                }

                request.TotalPricing = request.ProductPrices - (request.ProductPrices * (request.Discount / 100));

                var mapperBuy = _mapper.Map(buyinput, request);

                var buyValidator = new BuyRequestValidator();
                var buyValid = buyValidator.Validate(mapperBuy);

                if (buyValid.IsValid)
                {
                    await _buyRequestRepository.UpdateAsync(mapperBuy);
                }
                else
                {
                    var news = new ErrorMessage<ProductRequest>(HttpStatusCode.BadRequest.GetHashCode().ToString(), buyValid.Errors.ConvertAll(x => x.ErrorMessage.ToString()), prodReq);
                    return StatusCode((int)HttpStatusCode.BadRequest, news);
                }


                if (request.Status == Status.Finalized)
                {
                    var type = DesafioFinanceiro_Oscar.Domain.Entities.Type.Receive;
                    var recentValue = mapperBuy.TotalPricing; //valor recente (total)
                    string description = $"Financial transaction order id: {request.Id}";

                    if (mapperBuy.Status == oldStatus && mapperBuy.Status == Status.Finalized && totalValueOld > mapperBuy.TotalPricing)
                    {
                        description = $"Diference purchase order id: {request.Id}";
                        recentValue = mapperBuy.TotalPricing - totalValueOld;
                        type = DesafioFinanceiro_Oscar.Domain.Entities.Type.Payment;
                    }

                    var response = await _bankRecordRepository.CreateBankRecord(Origin.PurchaseRequest, mapperBuy.Id, description,
                        type, recentValue);

                    if (!response.IsSuccessStatusCode)
                    {
                        var result = _bankRecordRepository.BadRequestMessage(bank, response.Content.ToString());
                        return StatusCode((int)HttpStatusCode.BadRequest, result);
                    }
                }

                return Ok(mapperBuy);

            }
            catch (Exception ex)
            {
                var result = _buyRequestRepository.BadRequestMessage(buyReq, ex.Message.ToString());
                return StatusCode((int)HttpStatusCode.BadRequest, result);
            }

        }

        [HttpPut("changeState/{id}")]
        public async Task<IActionResult> ChangeState(Guid id, Status state)
        {
            try
            {
                var request = await _buyRequestRepository.GetByIdAsync(id);
                var product = _productRequestRepository.GetAllByRequestId(id).FirstOrDefault();

                if (request == null && product == null)
                {
                    var result = _buyRequestRepository.NotFoundMessage(buyReq);
                    return StatusCode((int)HttpStatusCode.NotFound, result);
                }

                if (request.Status == Status.Finalized)
                {
                    var result = _buyRequestRepository.BadRequestMessage(buyReq, "You can only delete a finalized Request!");
                    return StatusCode((int)HttpStatusCode.BadRequest, result);
                }

                if (product.ProductCategory == ProductCategory.Physical)
                {
                    if (state == Status.WaitingDownload)
                    {
                        var result = _buyRequestRepository.BadRequestMessage(buyReq, "A Physical product can't be set to Waiting To Download status!");
                        return StatusCode((int)HttpStatusCode.BadRequest, result);
                    }
                }
                else
                {
                    if (state == Status.WaitingDelivery)
                    {
                        var result = _buyRequestRepository.BadRequestMessage(buyReq, "A Digital product can't be set to Waiting To Delivery status!");
                        return StatusCode((int)HttpStatusCode.BadRequest, result);
                    }
                }

                request.Status = state;

                await _buyRequestRepository.UpdateAsync(request);

                if (request.Status == Status.Finalized)
                {

                    var response = await _bankRecordRepository.CreateBankRecord(Origin.PurchaseRequest, request.Id, $"Purshase order id: {request.Id}",
                        DesafioFinanceiro_Oscar.Domain.Entities.Type.Receive, request.TotalPricing);

                    if (!response.IsSuccessStatusCode)
                    {
                        var result = _bankRecordRepository.BadRequestMessage(bank, response.Content.ToString());
                        return StatusCode((int)HttpStatusCode.BadRequest, result);
                    }

                }

                return Ok(request);
            }
            catch (Exception ex)
            {
                var result = _buyRequestRepository.BadRequestMessage(buyReq, ex.Message.ToString());
                return StatusCode((int)HttpStatusCode.BadRequest, result);
            }

        }

        [HttpDelete]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            try
            {
                var buyRequest = await _buyRequestRepository.GetByIdAsync(id);

                if (buyRequest == null)
                {
                    var result = _buyRequestRepository.NotFoundMessage(buyReq);
                    return StatusCode((int)HttpStatusCode.NotFound, result);
                }

                await _buyRequestRepository.DeleteAsync(buyRequest);

                if (buyRequest.Status == Status.Finalized)
                {

                    var response = await _bankRecordRepository.CreateBankRecord(Origin.PurchaseRequest, id, $"Revert Purshase order id: {buyRequest.Id}",
                        DesafioFinanceiro_Oscar.Domain.Entities.Type.Revert, -buyRequest.TotalPricing);

                    if (!response.IsSuccessStatusCode)
                    {
                        var result = _bankRecordRepository.BadRequestMessage(bank, response.Content.ToString());
                        return StatusCode((int)HttpStatusCode.BadRequest, result);
                    }

                }

                return Ok(buyRequest);

            }
            catch (Exception ex)
            {
                var result = _buyRequestRepository.BadRequestMessage(buyReq, ex.Message.ToString());
                return StatusCode((int)HttpStatusCode.BadRequest, result);
            }

        }

    }

}
