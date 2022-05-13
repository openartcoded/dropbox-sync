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
    public class InvoiceProfile : Profile
    {
        public InvoiceProfile()
        {
            CreateMap<InvoiceGeneratedModel, InvoiceEntity>()
                .ForMember(
                    dest => dest.Id,
                    opt => opt.MapFrom(src => Guid.Parse(src.InvoiceId)))
                .ForMember(
                    dest => dest.InvoiceDate,
                    opt => opt.MapFrom(src => ConvertToDateOnly(src.DateOfInvoice)))
                .ForMember(
                    dest => dest.DueDate,
                    opt => opt.MapFrom(src => ConvertToDateOnly(src.DueDate)))
                .ForMember(
                    dest => dest.Deleted,
                    opt => opt.MapFrom(src => false))
                .ForMember(
                    dest => dest.CreatedAt,
                    opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(
                    dest => dest.UpdatedAt,
                    opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(
                    dest => dest.Upload,
                    opt => opt.Ignore())
                .ForMember(
                    dest => dest.UploadId,
                    opt => opt.Ignore());
        }

        private static DateOnly ConvertToDateOnly(long timespanTick)
        {
            DateTime d = new DateTime(timespanTick);
            return new DateOnly(d.Year, d.Month, d.Day);
        }
    }
}
