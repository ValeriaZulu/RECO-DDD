using System.Threading.Tasks;

namespace RECO.Domain.Interfaces
{
    /// <summary>
    /// Domain-level abstraction for publishing domain events.
    /// Implementations belong to Infrastructure. Keeping this as an interface follows DIP from the constitution.
    /// </summary>
    public interface IDomainEventDispatcher
    {
        Task PublishAsync<TEvent>(TEvent @event) where TEvent : class;
    }
}
