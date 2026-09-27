namespace Core.Domain;

public sealed class Customer
{
    public string Id { get; }
    public string Name { get; }
    public string Email { get; }

    private Customer(string id, string name, string email)
    {
        Id = id;
        Name = name;
        Email = email;
    }

    public static Customer Create(string id, string name, string email)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор клієнта не може бути порожнім", nameof(id));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Ім'я клієнта не може бути порожнім", nameof(name));

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new ArgumentException("Email клієнта має неправильний формат", nameof(email));

        return new Customer(id.Trim(), name.Trim(), email.Trim());
    }
}