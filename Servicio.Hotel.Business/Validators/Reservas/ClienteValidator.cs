using System.Collections.Generic;
using Servicio.Hotel.Business.DTOs.Reservas;
using Servicio.Hotel.Business.Exceptions;

namespace Servicio.Hotel.Business.Validators.Reservas
{
    public static class ClienteValidator
    {
        public static void Validate(ClienteDTO cliente)
        {
            if (cliente == null)
                throw new ValidationException("CLI-001", "El cliente no puede ser nulo.");

            var errors = new Dictionary<string, string[]>();

            if (string.IsNullOrWhiteSpace(cliente.TipoIdentificacion))
                errors["TipoIdentificacion"] = new[] { "El tipo de identificación es obligatorio." };

            if (string.IsNullOrWhiteSpace(cliente.NumeroIdentificacion))
                errors["NumeroIdentificacion"] = new[] { "El número de identificación es obligatorio." };

            if (string.IsNullOrWhiteSpace(cliente.Nombres))
                errors["Nombres"] = new[] { "Los nombres son obligatorios." };

            if (string.IsNullOrWhiteSpace(cliente.Correo))
                errors["Correo"] = new[] { "El correo electrónico es obligatorio." };

            if (string.IsNullOrWhiteSpace(cliente.Telefono))
                errors["Telefono"] = new[] { "El teléfono es obligatorio." };

            if (errors.Count > 0)
                throw new ValidationException("CLI-002", errors);
        }
    }
}
