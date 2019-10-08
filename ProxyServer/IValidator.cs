
namespace DoctorProxy
{
    public interface IValidator
    {
        bool IsValid(string username, string password);
    }
}
