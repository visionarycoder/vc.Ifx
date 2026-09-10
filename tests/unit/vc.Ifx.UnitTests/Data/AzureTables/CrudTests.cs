using Azure;
using Azure.Data.Tables;

namespace VisionaryCoder.Framework.Tests.Data.AzureTables;

[TestClass]
public sealed class CrudTests
{
    [TestMethod]
    public async Task CrudPreservesEntitiesKeysModesEtagsAndTokens()
    {
        var table = new TableFake();
        using var provider = TableFixture.Provider(table);
        using var cancellation = new CancellationTokenSource();
        var entity = TableFixture.Entity();
        var explicitTag = new ETag("explicit");
        provider.AddEntity(entity);
        await provider.AddEntityAsync(entity, cancellation.Token);
        provider.UpdateEntity(entity);
        await provider.UpdateEntityAsync(entity, explicitTag, TableUpdateMode.Merge, cancellation.Token);
        provider.UpsertEntity(entity, TableUpdateMode.Merge);
        await provider.UpsertEntityAsync(entity, cancellationToken: cancellation.Token);
        provider.DeleteEntity("partition", "row", ETag.All);
        await provider.DeleteEntityAsync("partition", "row", explicitTag, cancellation.Token);
        ((object?)provider.GetEntity<TableEntity>("partition", "row")).Should().BeSameAs(table.Result);
        ((object?)await provider.GetEntityAsync<TableEntity>("partition", "row", cancellation.Token)).Should().BeSameAs(table.Result);

        table.Calls.Select(call => call.Operation).Should().Equal("add", "addAsync", "update", "updateAsync",
            "upsert", "upsertAsync", "delete", "deleteAsync", "get", "getAsync");
        table.Calls.Take(6).Should().OnlyContain(call => ReferenceEquals(call.Entity, entity));
        table.Calls.Where(call => call.Operation.EndsWith("Async", StringComparison.Ordinal))
            .Should().OnlyContain(call => call.Token == cancellation.Token);
        table.Calls[2].Tag.Should().Be(entity.ETag);
        table.Calls[2].Mode.Should().Be(TableUpdateMode.Replace);
        table.Calls[3].Tag.Should().Be(explicitTag);
        table.Calls[3].Mode.Should().Be(TableUpdateMode.Merge);
        table.Calls[4].Mode.Should().Be(TableUpdateMode.Merge);
        table.Calls[5].Mode.Should().Be(TableUpdateMode.Replace);
        table.Calls[6].Tag.Should().Be(ETag.All);
        table.Partition.Should().Be("partition");
        table.Row.Should().Be("row");
        entity.ETag.Should().Be(new ETag("entity-tag"));
        entity["Value"].Should().Be(42);
        table.Creates.Should().Be(0);
    }

    [TestMethod]
    public async Task EmptyOrOmittedEtagsCannotAccidentallyEnableUnconditionalWrites()
    {
        var table = new TableFake();
        using var provider = TableFixture.Provider(table);
        var entity = TableFixture.Entity();
        provider.UpdateEntity(entity, new ETag(""));
        table.Calls.Single().Tag.Should().Be(entity.ETag);

        foreach (ETag tag in new[] { default(ETag), new ETag(""), ETag.All })
        {
            entity.ETag = tag;
            Action update = () => provider.UpdateEntity(entity);
            Func<Task> updateAsync = () => provider.UpdateEntityAsync(entity);
            update.Should().Throw<ArgumentException>();
            await updateAsync.Should().ThrowAsync<ArgumentException>();
        }

        Action delete = () => provider.DeleteEntity("partition", "row");
        Func<Task> deleteAsync = () => provider.DeleteEntityAsync("partition", "row", new ETag(""));
        delete.Should().Throw<ArgumentException>();
        await deleteAsync.Should().ThrowAsync<ArgumentException>();
        provider.UpdateEntity(entity, ETag.All);
        table.Calls.Last().Tag.Should().Be(ETag.All);
        using var unconditional = TableFixture.Provider(table, TableFixture.Options(concurrency: false));
        unconditional.UpdateEntity(entity);
        unconditional.DeleteEntity("partition", "row");
        table.Calls.TakeLast(2).Should().OnlyContain(call => call.Tag == ETag.All);
    }

    [TestMethod]
    public async Task ReadsTranslateOnly404ToNull()
    {
        var table = new TableFake { Failure = new RequestFailedException(404, "missing") };
        using var provider = TableFixture.Provider(table);
        provider.GetEntity<TableEntity>("partition", "row").Should().BeNull();
        (await provider.GetEntityAsync<TableEntity>("partition", "row")).Should().BeNull();
        foreach (int status in new[] { 403, 409, 412, 429, 500 })
        {
            var failure = new RequestFailedException(status, "failure");
            table.Failure = failure;
            Action read = () => provider.GetEntity<TableEntity>("partition", "row");
            Func<Task> readAsync = () => provider.GetEntityAsync<TableEntity>("partition", "row");
            read.Should().Throw<RequestFailedException>().Which.Should().BeSameAs(failure);
            (await readAsync.Should().ThrowAsync<RequestFailedException>()).Which.Should().BeSameAs(failure);
        }
        table.Calls.Should().HaveCount(12);
    }

    [TestMethod]
    public async Task WriteConflictsAndFailuresPropagateWithoutExtraRetries()
    {
        var failure = new RequestFailedException(412, "conflict");
        var table = new TableFake { Failure = failure };
        using var provider = TableFixture.Provider(table);
        var entity = TableFixture.Entity();
        Action[] sync = [() => provider.AddEntity(entity), () => provider.UpdateEntity(entity),
            () => provider.UpsertEntity(entity), () => provider.DeleteEntity("partition", "row", ETag.All)];
        Func<Task>[] asyncCalls = [() => provider.AddEntityAsync(entity), () => provider.UpdateEntityAsync(entity),
            () => provider.UpsertEntityAsync(entity), () => provider.DeleteEntityAsync("partition", "row", ETag.All)];
        foreach (Action action in sync) action.Should().Throw<RequestFailedException>().Which.Should().BeSameAs(failure);
        foreach (Func<Task> action in asyncCalls)
            (await action.Should().ThrowAsync<RequestFailedException>()).Which.Should().BeSameAs(failure);
        table.Calls.Should().HaveCount(8);
    }

    [TestMethod]
    public async Task InvalidEntitiesKeysAndModesFailBeforeIo()
    {
        var table = new TableFake();
        using var provider = TableFixture.Provider(table);
        Action nullEntity = () => provider.AddEntity<TableEntity>(null!);
        nullEntity.Should().Throw<ArgumentNullException>();
        foreach (string? key in new[] { null, new string('a', 1025), "/", "\\", "#", "?", "\0", "\u001f", "\u007f", "\u009f" })
        {
            Action partition = () => provider.GetEntity<TableEntity>(key!, "row");
            Action row = () => provider.GetEntity<TableEntity>("partition", key!);
            partition.Should().Throw<ArgumentException>();
            row.Should().Throw<ArgumentException>();
        }
        var entity = TableFixture.Entity();
        Action invalidUpdate = () => provider.UpdateEntity(entity, mode: (TableUpdateMode)99);
        Action invalidUpsert = () => provider.UpsertEntity(entity, (TableUpdateMode)99);
        Func<Task> invalidAsync = () => provider.UpsertEntityAsync(entity, (TableUpdateMode)99);
        invalidUpdate.Should().Throw<ArgumentOutOfRangeException>();
        invalidUpsert.Should().Throw<ArgumentOutOfRangeException>();
        await invalidAsync.Should().ThrowAsync<ArgumentOutOfRangeException>();
        table.Calls.Should().BeEmpty();
        foreach (string key in new[] { "", " ", "a'b", new string('a', 1024), "\u0020", "\u007e", "\u00a0" })
            provider.AddEntity(TableFixture.Entity(key, key));
        table.Calls.Should().HaveCount(7);
    }

    [TestMethod]
    public async Task AsyncOperationsHonorCancellationBeforeIoAndAfterSdkCompletion()
    {
        var entity = TableFixture.Entity();
        for (int operation = 0; operation < 5; operation++)
        {
            foreach (bool cancelDuring in new[] { false, true })
            {
                var table = new TableFake();
                using var provider = TableFixture.Provider(table);
                using var cancellation = new CancellationTokenSource();
                if (cancelDuring) table.OnCall = name => cancellation.Cancel();
                else cancellation.Cancel();
                Func<Task> call = operation switch
                {
                    0 => () => provider.AddEntityAsync(entity, cancellation.Token),
                    1 => () => provider.UpdateEntityAsync(entity, cancellationToken: cancellation.Token),
                    2 => () => provider.UpsertEntityAsync(entity, cancellationToken: cancellation.Token),
                    3 => () => provider.DeleteEntityAsync("partition", "row", ETag.All, cancellation.Token),
                    _ => () => provider.GetEntityAsync<TableEntity>("partition", "row", cancellation.Token)
                };
                (await call.Should().ThrowAsync<OperationCanceledException>()).Which.CancellationToken.Should().Be(cancellation.Token);
                table.Calls.Should().HaveCount(cancelDuring ? 1 : 0);
            }
        }
        var missing = new TableFake { Failure = new RequestFailedException(404, "missing") };
        using var missingProvider = TableFixture.Provider(missing);
        using var missingCancellation = new CancellationTokenSource();
        missing.OnCall = name => missingCancellation.Cancel();
        Func<Task> missingRead = () => missingProvider.GetEntityAsync<TableEntity>("partition", "row", missingCancellation.Token);
        await missingRead.Should().ThrowAsync<OperationCanceledException>();
    }
}
