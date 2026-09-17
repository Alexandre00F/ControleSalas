using System.ComponentModel.DataAnnotations;

public static class ValidationHelper
{
    public static Dictionary<string, string[]> ValidarModelo(object modelo)
    {
        var contexto = new ValidationContext(modelo);

        var resultados = new List<ValidationResult>();

        Validator.TryValidateObject(
            modelo,
            contexto,
            resultados,
            validateAllProperties: true
        );

        return resultados
            .GroupBy(r => r.MemberNames.FirstOrDefault() ?? "")
            .ToDictionary(
                g => g.Key,
                g => g.Select(
                    r => r.ErrorMessage ?? "Valor inválido"
                ).ToArray()
            );
    }
}