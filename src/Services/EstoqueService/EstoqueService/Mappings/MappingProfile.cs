using AutoMapper;
using Shared.DTOs;
using Shared.Models;

namespace EstoqueService.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Produto mappings
            CreateMap<Produto, ProdutoResponseDto>();
            CreateMap<ProdutoCreateDto, Produto>();
            CreateMap<ProdutoUpdateDto, Produto>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}