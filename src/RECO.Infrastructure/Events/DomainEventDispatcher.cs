using System.Threading.Tasks;
using RECO.Domain.Interfaces;
using MediatR;

namespace RECO.Infrastructure.Events
{
    public class DomainEventDispatcher : IDomainEventDispatcher
    {
        private readonly IMediator _mediator;

        public DomainEventDispatcher(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : class
        {
            await _mediator.Publish(@event);
        }
    }
}
