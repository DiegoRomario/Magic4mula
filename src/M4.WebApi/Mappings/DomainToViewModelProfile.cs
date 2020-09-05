using AutoMapper;
using M4.Domain.Entities;
using M4.WebApi.Models;

namespace M4.WebApi.Mappings
{
    public class DomainToViewModelProfile : Profile
    {
        public DomainToViewModelProfile()
        {
            CreateMap<Acao, AcaoClassificacao>();            
        }
    }
}
