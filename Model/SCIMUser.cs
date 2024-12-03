public class SCIMUser
{
    public string? UserName { get; set; }
    public Name? Name { get; set; }
    public string? DisplayName { get; set; }
    public string? NickName { get; set; }
    public bool? Active { get; set; }
    public string? Password { get; set; }
    public List<Email>? Emails { get; set; }
    public List<PhoneNumber>? PhoneNumbers { get; set; }
}

public class Name
{
    public string? Formatted { get; set; }
    public string? FamilyName { get; set; }
    public string? GivenName { get; set; }
    public string? MiddleName { get; set; }
    public string? HonorificPrefix { get; set; }
    public string? HonorificSuffix { get; set; }
}

public class Email
{
    public string? Value { get; set; }
    public string? Display { get; set; }
    public string? Type { get; set; }  
    public bool? Primary { get; set; }
}

public class PhoneNumber
{
    public string? Value { get; set; }
    public string? Display { get; set; }
    public string? Type { get; set; }  
    public bool? Primary { get; set; }
}
