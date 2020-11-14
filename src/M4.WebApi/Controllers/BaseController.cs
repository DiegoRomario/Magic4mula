using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace M4.WebApi.Controllers
{
    [ApiController]
    public abstract class BaseController : Controller
    {
        protected ICollection<string> Erros = new List<string>();

        protected ActionResult BaseResponse(object result = null, HttpStatusCode statusCodeSuccess = HttpStatusCode.OK, HttpStatusCode statusCodeErro = HttpStatusCode.BadRequest )
        {
            if (OperacaoValida()) return StatusCode((int)statusCodeSuccess, result);

            return StatusCode((int)statusCodeErro, new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                {"messages", Erros.ToArray() }
            }));
        }

        protected ActionResult BaseResponse(string message, HttpStatusCode statusCodeSuccess = HttpStatusCode.OK, HttpStatusCode statusCodeErro = HttpStatusCode.BadRequest)
        {
            if (OperacaoValida()) return StatusCode((int)statusCodeSuccess, new { message});

            return StatusCode((int)statusCodeErro, new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                {"messages", Erros.ToArray() }
            }));
        }



        protected ActionResult BaseResponse(ModelStateDictionary modelState)
        {
            var erros = modelState.Values.SelectMany(e => e.Errors);
            foreach (var erro in erros)            
                AdicionarErro(erro.ErrorMessage);            

            return BaseResponse();
        }

        protected bool OperacaoValida() => !Erros.Any();

        protected void AdicionarErro(string erro) => Erros.Add(erro);

    }
}
