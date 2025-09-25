using AutoMapper;
using Shared.DTOs;
using Shared.Models;

namespace AuthService.Profiles;

public class AuthProfile : Profile
{
    public AuthProfile()
    {
        CreateMap<Usuario, UsuarioDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Nome, opt => opt.MapFrom(src => src.Nome))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
            .ForMember(dest => dest.CriadoEm, opt => opt.MapFrom(src => src.CriadoEm));

        CreateMap<CreateUsuarioDto, Usuario>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CriadoEm, opt => opt.Ignore())
            .ForMember(dest => dest.Senha, opt => opt.Ignore()); // A senha será hasheada no serviço

        CreateMap<UpdateUsuarioDto, Usuario>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CriadoEm, opt => opt.Ignore())
            .ForMember(dest => dest.Senha, opt => opt.Ignore()) // A senha será hasheada no serviço
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
    }
}
