namespace EmailCreator.Business.Exceptions;

public sealed class DuplicateCompanyDomainException : Exception
{
    public DuplicateCompanyDomainException(string domain)
        : base($"'{domain}' domaini zaten kayıtlı.")
    {
        Domain = domain;
    }

    public string Domain { get; }
}
