using System.Collections;

namespace ModularMonolith.Shared.Contracts.Errors;

public class MemberErrorCollection : Error, IEnumerable<MemberError>
{
    private readonly List<MemberError> _errors = new();

    public MemberErrorCollection() : base("MEMBER_ERRORS", "One or more errors occurred")
    {
    }

    public void Add(MemberError memberError) => _errors.Add(memberError);

    public IEnumerator<MemberError> GetEnumerator() => _errors.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
