namespace NIC_clinica_project.Repositories
{
    public interface ICrudRepository<TEditDto, TListDto> 
        where TEditDto : class 
        where TListDto : class
    {
        bool IsSortable(string? orderParam);

        Task<bool> IsValid(TEditDto dto);
        Task<List<TListDto>> GetAll(int page, int pageSize, string orderParam);
        Task<TEditDto?> GetById(int id);
        Task<int> Create(TEditDto dto);
        Task<bool> Update(TEditDto dto);
        Task<bool> Delete(int id);
    }
}
