using DevTrackR.API.Entities;
using DevTrackR.API.Models;
using DevTrackR.API.Persistence;
using DevTrackR.API.Persistence.Repository;
using Microsoft.AspNetCore.Mvc;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace DevTrackR.API.Controllers
{
    [Route("api/packages")]
    public class PackagesController : ControllerBase
    {
        private readonly IPackageRepository _repository;
        private readonly ISendGridClient _client;

        public PackagesController(IPackageRepository repository, ISendGridClient client)
        {
            _repository = repository;
            _client = client;
        }

        //Get api/packages
        [HttpGet]
        public IActionResult GetAll()
        {
            var packages = _repository.GetAll();
            return Ok(packages);
        }

        //GET api/packages/{code}
        [HttpGet("{code}")]
        public IActionResult GetByCode(string code)
        {
            var package = _repository.GetByCode(code);

            if (package == null)
            {
                return NotFound();
            }

            return Ok(package);
        }
        //POST api/packages
        /// <summary>
        /// Cadastro de Pacote
        /// </summary>
        /// <remarks>
        /// {
        ///  "title": "Pacote Cartas Colecionaveis",
        ///  "weight": "1.8,
        ///  "senderName": "Sandokan Alves"
        ///  "senderEmail": "sitey97720@dakcans.com"
        /// }
        /// </remarks>
        /// <param name="model">Dados do Pacote</param>
        /// <returns>Objeto recém-criado</returns>
        /// <response code="201">Cadastro Realizado com Sucesso</response>
        /// <response code="400">Dados estão invalidos</response>
        /// 
        [HttpPost]
        public async Task<IActionResult> Post(AddPackageInputModel model)
        {
            if (model.Title.Length < 10)
            {
                return BadRequest("Title must be at least 10 characters long.");
            }

            var package = new Package(model.Title, model.Weight);

            _repository.Add(package);
            var message = new SendGridMessage
            {
                From = new EmailAddress("sitey97720@dakcans.com", "Tec"),
                Subject = "Your package was dispatched",
                PlainTextContent = $"Your package with code {package.Code} was dispatched."
            };
            message.AddTo(model.SenderEmail, model.SenderName);
            await _client.SendEmailAsync(message);

            return CreatedAtAction(
                "GetByCode",
                 new { code = package.Code },
                  package);
        }


        //POST api/packages/{code}/updates
        [HttpPost("{code}/updates")]
        public async Task<IActionResult> PostUpdate(string code, AddPackageUpdateInputModel model)
        {
            var package = _repository.GetByCode(code);

            if (package == null)
            {
                return NotFound();
            }
            package.AddUpdate(model.Status, model.Delivered);

            _repository.Update(package);
            var messages = new SendGridMessage
            {
                From = new EmailAddress("sitey97720@dakcans.com", "Tec"),
                Subject = "Your package was dispatched",
                PlainTextContent = $"Your package with code {package.Code} was dispatched."
            };
            messages.AddTo(model.SenderEmail, model.SenderName);
            await _client.SendEmailAsync(messages);

            return NoContent();

        }

    }
}