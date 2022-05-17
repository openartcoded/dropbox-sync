using AutoMapper;
using DropboxSync.BLL.Entities;
using DropboxSync.UIL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxSync.UIL.Profiles
{
    public class ExpenseProfile : Profile
    {
        public ExpenseProfile()
        {
            CreateMap<ExpenseReceivedModel, ExpenseEntity>()
                .ForMember(
                    dest => dest.Id,
                    opt => opt.MapFrom(src => Guid.Parse(src.ExpenseId)))
                .ForMember(
                    dest => dest.Name,
                    opt => opt.MapFrom(src => src.Name))
                .ForMember(
                    dest => dest.CreatedAt,
                    opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(
                    dest => dest.UpdatedAt,
                    opt => opt.MapFrom(src => DateTime.Now));
        }
    }
}
