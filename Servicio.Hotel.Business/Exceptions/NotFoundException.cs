using System;

namespace Servicio.Hotel.Business.Exceptions
{
    /// <summary>
    /// Excepción lanzada cuando un recurso solicitado no existe.
    /// </summary>
    public class NotFoundException : Exception
    {
        public NotFoundException() : base() { }

        public NotFoundException(string message) : base(message) { }

        public NotFoundException(string message, Exception innerException)
            : base(message, innerException) { }

        // Constructor útil para indicar tipo de entidad y su identificador
        public NotFoundException(string entityName, object id)
            : base($"No se encontró la entidad '{entityName}' con identificador '{id}'.")
        {
            EntityName = entityName;
            Id = id;
        }

        public string EntityName { get; }
        public object Id { get; }
    }
}