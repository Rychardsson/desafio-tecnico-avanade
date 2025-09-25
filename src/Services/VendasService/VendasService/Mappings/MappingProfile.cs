using AutoMapper;
using Shared.DTOs;
using Shared.Models;

namespace VendasService.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Pedido mappings
            CreateMap<Pedido, PedidoResponseDto>();
            CreateMap<PedidoCreateDto, Pedido>()
                .ForMember(dest => dest.Itens, opt => opt.MapFrom(src => src.Itens));
            
            // ItemPedido mappings
            CreateMap<ItemPedido, ItemPedidoResponseDto>();
            CreateMap<ItemPedidoDto, ItemPedido>()
                .ForMember(dest => dest.NomeProduto, opt => opt.Ignore()) // Será preenchido pelo serviço
                .ForMember(dest => dest.PrecoUnitario, opt => opt.Ignore()); // Será preenchido pelo serviço
        }
    }
}
