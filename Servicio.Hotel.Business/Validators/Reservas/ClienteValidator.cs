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
            else if (cliente.TipoIdentificacion.Length > 20)
                errors["TipoIdentificacion"] = new[] { "El tipo de identificación no puede exceder 20 caracteres." };

            if (string.IsNullOrWhiteSpace(cliente.NumeroIdentificacion))
                errors["NumeroIdentificacion"] = new[] { "El número de identificación es obligatorio." };
            else if (cliente.NumeroIdentificacion.Length > 30)
                errors["NumeroIdentificacion"] = new[] { "El número de identificación no puede exceder 30 caracteres." };

            if (string.IsNullOrWhiteSpace(cliente.Nombres))
                errors["Nombres"] = new[] { "Los nombres son obligatorios." };
            else if (cliente.Nombres.Length > 160)
                errors["Nombres"] = new[] { "Los nombres no pueden exceder 160 caracteres." };

            if (string.IsNullOrWhiteSpace(cliente.Correo))
                errors["Correo"] = new[] { "El correo electrónico es obligatorio." };
            else if (!cliente.Correo.Contains("@") || !cliente.Correo.Contains("."))
                errors["Correo"] = new[] { "El correo electrónico no es válido." };
            else if (cliente.Correo.Length > 150)
                errors["Correo"] = new[] { "El correo no puede exceder 150 caracteres." };

            if (string.IsNullOrWhiteSpace(cliente.Telefono))
                errors["Telefono"] = new[] { "El teléfono es obligatorio." };
            else if (cliente.Telefono.Length > 30)
                errors["Telefono"] = new[] { "El teléfono no puede exceder 30 caracteres." };

            if (string.IsNullOrWhiteSpace(cliente.Direccion))
                errors["Direccion"] = new[] { "La dirección es obligatoria." };
            else if (cliente.Direccion.Length > 250)
                errors["Direccion"] = new[] { "La dirección no puede exceder 250 caracteres." };

            if (errors.Count > 0)
                throw new ValidationException("CLI-002", errors);
        }
    }
}