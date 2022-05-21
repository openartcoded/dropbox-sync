using AutoMapper;
using DropboxSync.BLL.Entities;
using DropboxSync.Helpers;
using DropboxSync.UIL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Profiles
{
    public class DossierProfile : Profile
    {
        public DossierProfile()
        {
            CreateMap<DossierCreateModel, DossierEntity>()
                .ForMember(
                    dest => dest.Id,
                    opt => opt.MapFrom(src => Guid.Parse(src.DossierId)))
                .ForMember(
                    dest => dest.Name,
                    opt => opt.MapFrom(src => src.Name))
                .ForMember(
                    dest => dest.Description,
                    opt => opt.MapFrom(src => src.Description))
                .ForMember(
                    dest => dest.CreatedAt,
                    opt => opt.MapFrom(src => DateTimeHelper.FromUnixTimestamp(src.Timestamp)))
                .ForMember(
                    dest => dest.UpdatedAt,
                    opt => opt.MapFrom(src => DateTimeHelper.FromUnixTimestamp(src.Timestamp)))
                .ForMember(
                    dest => dest.IsClosed,
                    opt => opt.MapFrom(src => false));
        }
    }
}
