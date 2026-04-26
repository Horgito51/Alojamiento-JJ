using Servicio.Hotel.API.Models.Responses.Public;
using Servicio.Hotel.Business.DTOs.Alojamiento;
using Servicio.Hotel.Business.DTOs.Seguridad;

namespace Servicio.Hotel.API.Models.Responses.Public
{
    public static class PublicDtoMapper
    {
        public static UsuarioPublicDto ToPublicDto(this UsuarioDTO dto) => new()
        {
            UsuarioGuid = dto.UsuarioGuid,
            Username = dto.Username,
            Nombres = dto.Nombres,
            Apellidos = dto.Apellidos,
            EstadoUsuario = dto.EstadoUsuario,
            Activo = dto.Activo
        };

        public static RolPublicDto ToPublicDto(this RolDTO dto) => new()
        {
            RolGuid = dto.RolGuid,
            NombreRol = dto.NombreRol,
            DescripcionRol = dto.DescripcionRol,
            EstadoRol = dto.EstadoRol,
            Activo = dto.Activo
        };

        public static SucursalPublicDto ToPublicDto(this SucursalDTO dto) => new()
        {
            SucursalGuid = dto.SucursalGuid,
            CodigoSucursal = dto.CodigoSucursal,
            NombreSucursal = dto.NombreSucursal,
            DescripcionSucursal = dto.DescripcionSucursal,
            TipoAlojamiento = dto.TipoAlojamiento,
            Estrellas = dto.Estrellas,
            CategoriaViaje = dto.CategoriaViaje,
            Pais = dto.Pais,
            Provincia = dto.Provincia,
            Ciudad = dto.Ciudad,
            Direccion = dto.Direccion,
            Telefono = dto.Telefono,
            Correo = dto.Correo,
            HoraCheckin = dto.HoraCheckin,
            HoraCheckout = dto.HoraCheckout,
            CheckinAnticipado = dto.CheckinAnticipado,
            CheckoutTardio = dto.CheckoutTardio,
            AceptaNinos = dto.AceptaNinos,
            PermiteMascotas = dto.PermiteMascotas,
            SePermiteFumar = dto.SePermiteFumar,
            EstadoSucursal = dto.EstadoSucursal
        };

        public static TipoHabitacionPublicDto ToPublicDto(this TipoHabitacionDTO dto) => new()
        {
            TipoHabitacionGuid = dto.TipoHabitacionGuid,
            Slug = dto.Slug,
            CodigoTipoHabitacion = dto.CodigoTipoHabitacion,
            NombreTipoHabitacion = dto.NombreTipoHabitacion,
            Descripcion = dto.Descripcion,
            CapacidadAdultos = dto.CapacidadAdultos,
            CapacidadNinos = dto.CapacidadNinos,
            CapacidadTotal = dto.CapacidadTotal,
            TipoCama = dto.TipoCama,
            AreaM2 = dto.AreaM2,
            PermiteEventos = dto.PermiteEventos,
            PermiteReservaPublica = dto.PermiteReservaPublica,
            EstadoTipoHabitacion = dto.EstadoTipoHabitacion
        };
    }
}
