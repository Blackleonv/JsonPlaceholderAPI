using Microsoft.EntityFrameworkCore;
using JsonPlaceholderAPI.Models;  // User ve Address sınıflarının bulunduğu namespace

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;  // Varsayılan değer atandı
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int AddressId { get; set; }

    public virtual Address Address { get; set; } = new Address();  // Virtual olarak tanımlandı ve varsayılan olarak boş bir Address nesnesi atandı

    public string Phone { get; set; } = string.Empty;  // Varsayılan değer atandı
    public string Website { get; set; } = string.Empty;
}

public class Address
{
    public int Id { get; set; }  // Birincil Anahtar
    public string Street { get; set; } = string.Empty;
    public string Suite { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Zipcode { get; set; } = string.Empty;

    public virtual User User { get; set; }  // Virtual olarak tanımlandı
}
