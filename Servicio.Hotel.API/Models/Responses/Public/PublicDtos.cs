using System;

namespace Servicio.Hotel.API.Models.Responses.Public
{
    public sealed class UsuarioPublicDto
    {
        public Guid UsuarioGuid { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string EstadoUsuario { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }

    public sealed class RolPublicDto
    {
        public Guid RolGuid { get; set; }
        public string NombreRol { get; set; } = string.Empty;
        public string? DescripcionRol { get; set; }
        public string EstadoRol { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }

    public sealed class SucursalPublicDto
    {
        public Guid SucursalGuid { get; set; }
        public string CodigoSucursal { get; set; } = string.Empty;
        public string NombreSucursal { get; set; } = string.Empty;
        public string? DescripcionSucursal { get; set; }
        public string TipoAlojamiento { get; set; } = string.Empty;
        public int? Estrellas { get; set; }
        public string? CategoriaViaje { get; set; }
        public string Pais { get; set; } = string.Empty;
        public string? Provincia { get; set; }
        public string Ciudad { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string? HoraCheckin { get; set; }
        public string? HoraCheckout { get; set; }
        public bool CheckinAnticipado { get; set; }
        public bool CheckoutTardio { get; set; }
        public bool AceptaNinos { get; set; }
        public bool PermiteMascotas { get; set; }
        public bool SePermiteFumar { get; set; }
        public string EstadoSucursal { get; set; } = string.Empty;
    }

    public sealed class TipoHabitacionPublicDto
    {
        public Guid TipoHabitacionGuid { get; set; }
        public string Slug { get; set; } = string.Empty;
        public string CodigoTipoHabitacion { get; set; } = string.Empty;
        public string NombreTipoHabitacion { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int CapacidadAdultos { get; set; }
        public int CapacidadNinos { get; set; }
        public int CapacidadTotal { get; set; }
        public string? TipoCama { get; set; }
        public decimal? AreaM2 { get; set; }
        public bool PermiteEventos { get; set; }
        public bool PermiteReservaPublica { get; set; }
        public string EstadoTipoHabitacion { get; set; } = string.Empty;
    }

    public sealed class HabitacionPublicDto
    {
        public int IdHabitacion { get; set; }
        public Guid HabitacionGuid { get; set; }
        public string NumeroHabitacion { get; set; } = string.Empty;
        public int? Piso { get; set; }
        public int CapacidadHabitacion { get; set; }
        public decimal PrecioBase { get; set; }
        public string? DescripcionHabitacion { get; set; }
        public string EstadoHabitacion { get; set; } = string.Empty;
        public Guid SucursalGuid { get; set; }
        public Guid TipoHabitacionGuid { get; set; }
        public string TipoHabitacionSlug { get; set; } = string.Empty;
        public string? ImagenUrl { get; set; }
    }
}
