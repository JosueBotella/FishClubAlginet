using ErrorOr;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FishClubAlginet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApiController : ControllerBase
{
    // Este método es el que llamamos desde los controladores hijos: return Problem(errors);
    protected IActionResult Problem(List<Error> errors)
    {
        // Si no hay errores (no debería pasar si llegamos aquí), devolvemos error genérico.
        if (errors.Count is 0)
        {
            return Problem();
        }

        // Si TODOS los errores son de validación, devolvemos un 400 con el detalle de campos.
        if (errors.All(error => error.Type == ErrorType.Validation))
        {
            return ValidationProblem(errors);
        }

        // Si son otros tipos de errores (ej: usuario no encontrado), miramos el primero para decidir el código HTTP.
        return Problem(errors[0]);
    }

    private IActionResult Problem(Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        return Problem(statusCode: statusCode, title: error.Description);
    }

    private IActionResult ValidationProblem(List<Error> errors)
    {
        var modelStateDictionary = new ModelStateDictionary();

        foreach (var error in errors)
        {
            modelStateDictionary.AddModelError(error.Code, error.Description);
        }

        return ValidationProblem(modelStateDictionary);
    }
}
