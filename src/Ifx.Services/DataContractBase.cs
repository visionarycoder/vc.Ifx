using System.Runtime.Serialization;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).

#pragma warning disable DerivedClasses
#pragma warning disable ClassWithData

namespace vc.Ifx.Services;

/// <summary>
/// Represents the base class for classes being passed across service boundaries.
/// </summary>
[DataContract]
public abstract class DataContractBase : IExtensibleDataObject, IEquatable<DataContractBase>
{
    /// <summary>
    /// Gets or sets the unique identifier for the data contract.
    /// </summary>
    [DataMember]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the universally unique identifier (UUID) for the data contract.
    /// </summary>
    [DataMember]
    public Guid Uuid { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the extension data for future compatibility.
    /// </summary>
    [IgnoreDataMember]
    public ExtensionDataObject ExtensionData { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp of the data contract.
    /// </summary>
    [DataMember]
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last updated timestamp of the data contract.
    /// </summary>
    [DataMember]
    public DateTime UpdatedOn { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets a value indicating whether the data contract is marked as deleted.
    /// </summary>
    [DataMember]
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Determines whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">The other object to compare with.</param>
    /// <returns><c>true</c> if the objects are equal; otherwise, <c>false</c>.</returns>
    public bool Equals(DataContractBase? other)
    {
        if (other is null) return false;
        return Id == other.Id && Uuid == other.Uuid;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with.</param>
    /// <returns><c>true</c> if the objects are equal; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is DataContractBase other && Equals(other);
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Uuid);
    }

    /// <summary>
    /// Serializes the current object to a JSON string.
    /// </summary>
    /// <returns>The JSON representation of the object.</returns>
    public string ToJson()
    {
        return System.Text.Json.JsonSerializer.Serialize(this);
    }

    /// <summary>
    /// Deserializes a JSON string to an instance of <see cref="DataContractBase"/>.
    /// </summary>
    /// <typeparam name="T">The type of the data contract.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>An instance of the data contract.</returns>
    public static T FromJson<T>(string json) where T : DataContractBase
    {
        return System.Text.Json.JsonSerializer.Deserialize<T>(json)
            ?? throw new InvalidOperationException("Failed to deserialize JSON.");
    }
}
