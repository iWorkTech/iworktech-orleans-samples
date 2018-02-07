using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;

namespace iWorkTech.Orleans.Interfaces
{
    public interface ICertGrain : IGrainWithStringKey
    {
        Task<Immutable<byte[]>> GetCertificate();
        Task UpdateCertificate(byte[] certData);
    }
}
