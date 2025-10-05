using AutoMapper;
using eCommerceApp.Application.DTOs.Category;

namespace eCommerceApp.MVC.Models.ViewModels
{
    public class ViewModelProfile : Profile
    {
        public ViewModelProfile()
        {
            CreateMap<CategoryDto, CategoryViewModel>();
            CreateMap<CategoryWithProductCountDto, CategoryViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.CategoryId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.CategoryName));
        }
    }
}
