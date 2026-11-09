using NIC_clinica_project.Models;

namespace NIC_clinica_project.Mappers
{
    public static class ClientMapper
    {
        private static TDto BaseMapping<TDto>(Client clientFrom, TDto clientTo) where TDto : ClientDto
        {
            clientTo.id = clientFrom.id;
            clientTo.firstName = clientFrom.firstName;
            clientTo.lastName = clientFrom.lastName;
            clientTo.otchestvo = clientFrom.otchestvo;
            clientTo.adress = clientFrom.adress;
            clientTo.birthday = clientFrom.birthday;
            clientTo.gender = clientFrom.gender;
            return clientTo;
        }
        
        public static ClientsDtoWithKeys ConvertToDtoWithKeys (this Client client)
        {
            var newDto = BaseMapping(client, new ClientsDtoWithKeys());
            newDto.regionId = client.regionId;
            return newDto;
        }

        public static ClientsDtoWithValues ConvertToDtoWithValues(this Client client)
        {
            var newDto = BaseMapping(client, new ClientsDtoWithValues());
            newDto.regionNumber = client.region.number;

            return newDto;
        }

        public static Client ToEfEntity(this ClientsDtoWithKeys dto)
        {
            var client = new Client();
            dto.ConvertToEfEntity(client);
            return client;
        }

        public static void ConvertToEfEntity(this ClientsDtoWithKeys dto, Client client)
        {
            client.firstName = dto.firstName;
            client.lastName = dto.lastName;
            client.otchestvo = dto.otchestvo;
            client.adress = dto.adress;
            client.birthday = dto.birthday;
            client.gender = dto.gender;
            client.regionId = dto.regionId;
        }

    }
}
