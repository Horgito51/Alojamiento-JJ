using System;

namespace Servicio.Hotel.Business.Exceptions
{
    /// <summary>
    /// Excepción lanzada cuando ocurre un error de negocio (reglas de negocio, operaciones inválidas, etc.)
    /// </summary>
    public class BusinessException : Exception
    {
        public BusinessException() : base() { }

        public BusinessException(string message) : base(message) { }

        public BusinessException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}