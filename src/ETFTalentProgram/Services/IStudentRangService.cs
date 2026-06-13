using ETFTalentProgram.ViewModels;

namespace ETFTalentProgram.Services
{
    public interface IStudentRangService
    {
        Task<IReadOnlyList<StudentRangViewModel>> GetRangListaAsync();
    }
}
