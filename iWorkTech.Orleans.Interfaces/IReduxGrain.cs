using System.Threading.Tasks;

namespace iWorkTech.Orleans.Interfaces
{
    public interface IAction
    {
    }
    

    public interface IReduxGrain<TState> 
    {
        Task<TState> GetState();
        Task<IAction> Dispatch(IAction action);
    }
}
