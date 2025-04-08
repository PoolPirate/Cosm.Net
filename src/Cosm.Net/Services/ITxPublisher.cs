using Cosm.Net.Models.Tx;
using Cosm.Net.Tx;

namespace Cosm.Net.Services;
public interface ITxPublisher
{
    public Task<TxSubmission> PublishTxAsync(ISignedCosmTx tx, DateTime? deadline, CancellationToken cancellationToken);
}

public interface ITxPublisher<TConfiguration> : ITxPublisher;