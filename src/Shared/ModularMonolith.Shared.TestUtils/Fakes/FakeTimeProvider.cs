namespace ModularMonolith.Shared.TestUtils.Fakes;

public class FakeTimeProvider : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => 
        new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
}
