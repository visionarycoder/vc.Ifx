using Azure;
using Azure.Data.Tables;

namespace VisionaryCoder.Framework.Tests.Data.AzureTables;

[TestClass]
public sealed class BatchTests
{
    [TestMethod]
    public async Task BatchesPreserveOrderAndSnapshotActionMetadataWithConditionalEtags()
    {
        var table = new TableFake();
        using var provider = TableFixture.Provider(table);
        using var cancellation = new CancellationTokenSource();
        TableTransactionAction[] actions =
        [
            new(TableTransactionActionType.Add, TableFixture.Entity("add")),
            new(TableTransactionActionType.UpdateMerge, TableFixture.Entity("merge")),
            new(TableTransactionActionType.UpdateReplace, TableFixture.Entity("replace"), ETag.All),
            new(TableTransactionActionType.Delete, TableFixture.Entity("delete")),
            new(TableTransactionActionType.UpsertMerge, TableFixture.Entity("upsert-merge")),
            new(TableTransactionActionType.UpsertReplace, TableFixture.Entity("upsert-replace"))
        ];
        provider.SubmitBatch(actions);
        var submitted = table.Batch!;
        submitted.Select(action => action.ActionType).Should().Equal(actions.Select(action => action.ActionType));
        submitted[0].Should().NotBeSameAs(actions[0]);
        submitted[0].Entity.Should().BeSameAs(actions[0].Entity);
        submitted[1].ETag.Should().Be(actions[1].Entity.ETag);
        submitted[2].ETag.Should().Be(ETag.All);
        submitted[3].ETag.Should().Be(actions[3].Entity.ETag);
        await provider.SubmitBatchAsync(actions, cancellation.Token);
        table.Calls.Last().Operation.Should().Be("batchAsync");
        table.Calls.Last().Token.Should().Be(cancellation.Token);
    }

    [TestMethod]
    public async Task InvalidBatchesFailBeforeIo()
    {
        var table = new TableFake();
        using var provider = TableFixture.Provider(table, TableFixture.Options(batch: 2));
        var first = new TableTransactionAction(TableTransactionActionType.Add, TableFixture.Entity());
        IEnumerable<TableTransactionAction>[] invalid =
        [
            null!, [], [null!], [first, first],
            [first, new(TableTransactionActionType.Add, TableFixture.Entity("second", "other"))],
            [new((TableTransactionActionType)999, TableFixture.Entity())],
            [new(TableTransactionActionType.UpdateReplace, new TableEntity("partition", "row"))]
        ];
        foreach (var batch in invalid)
        {
            Action submit = () => provider.SubmitBatch(batch);
            Func<Task> submitAsync = () => provider.SubmitBatchAsync(batch);
            submit.Should().Throw<ArgumentException>();
            await submitAsync.Should().ThrowAsync<ArgumentException>();
        }
        TableTransactionAction[] oversized = [first,
            new(TableTransactionActionType.Add, TableFixture.Entity("second")),
            new(TableTransactionActionType.Add, TableFixture.Entity("third"))];
        Action tooMany = () => provider.SubmitBatch(oversized);
        Func<Task> tooManyAsync = () => provider.SubmitBatchAsync(oversized);
        tooMany.Should().Throw<InvalidOperationException>();
        await tooManyAsync.Should().ThrowAsync<InvalidOperationException>();
        table.Calls.Should().BeEmpty();
    }

    [TestMethod]
    public async Task BatchCancellationIsCheckedDuringEnumerationAndAfterSdkCompletion()
    {
        var table = new TableFake();
        using var provider = TableFixture.Provider(table);
        using var cancellation = new CancellationTokenSource();
        IEnumerable<TableTransactionAction> CanceledActions()
        {
            cancellation.Cancel();
            yield return new(TableTransactionActionType.Add, TableFixture.Entity());
        }
        Func<Task> canceled = () => provider.SubmitBatchAsync(CanceledActions(), cancellation.Token);
        await canceled.Should().ThrowAsync<OperationCanceledException>();
        table.Calls.Should().BeEmpty();
        using var completionCancellation = new CancellationTokenSource();
        table.OnCall = name => completionCancellation.Cancel();
        Func<Task> completion = () => provider.SubmitBatchAsync(
            [new(TableTransactionActionType.Add, TableFixture.Entity())], completionCancellation.Token);
        await completion.Should().ThrowAsync<OperationCanceledException>();
    }

    [TestMethod]
    public async Task TransactionFailuresPropagateWithoutSplittingOrRetrying()
    {
        var failure = new RequestFailedException(409, "conflict");
        var table = new TableFake { Failure = failure };
        using var provider = TableFixture.Provider(table);
        TableTransactionAction[] actions = [new(TableTransactionActionType.Add, TableFixture.Entity())];
        Action submit = () => provider.SubmitBatch(actions);
        Func<Task> submitAsync = () => provider.SubmitBatchAsync(actions);
        submit.Should().Throw<RequestFailedException>().Which.Should().BeSameAs(failure);
        (await submitAsync.Should().ThrowAsync<RequestFailedException>()).Which.Should().BeSameAs(failure);
        table.Calls.Should().HaveCount(2);
    }
}
