using App.Application.Commands;
using App.Application.DTOs;
using App.Domain.DB.Model;
using AutoMapper;

namespace App.Application.Utilities
{
    public class MapperConfig
    {
        public static MapperConfiguration? config;
        public static AutoMapper.Mapper InitializeAutomapper()
        {
            if (config == null)
                config = new MapperConfiguration(cfg =>
              {
                  #region Bank
                  cfg.CreateMap<Bank, BankDTO>().ReverseMap();
                  cfg.CreateMap<Bank, BankAddCommand>().ReverseMap();
                  cfg.CreateMap<Bank, BankEditCommand>().ReverseMap();
                  cfg.CreateMap<Bank, BankDeleteCommand>().ReverseMap();
                  #endregion


              });


            return new Mapper(config);

        }
    }

}
