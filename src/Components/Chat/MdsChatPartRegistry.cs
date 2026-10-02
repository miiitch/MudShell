using Microsoft.AspNetCore.Components;

namespace MudShell.Components.Chat;

/// <summary>
/// Maps a part kind to the Blazor component that renders it. Create one per page or agent and pass it to
/// <see cref="MdsChatMessage.Registry"/> or <see cref="MdsChatPartRenderer.Registry"/>.
/// </summary>
public sealed class MdsChatPartRegistry
{
    /// <summary>A registered kind: the component to render, the payload type it expects and the parameter that receives it.</summary>
    public sealed record Registration(Type Component, Type PayloadType, string ParameterName);

    private readonly Dictionary<string, Registration> _registrations = new(StringComparer.Ordinal);

    /// <summary>Kinds currently registered.</summary>
    public IReadOnlyCollection<string> Kinds => _registrations.Keys;

    /// <summary>
    /// Registers <typeparamref name="TComponent"/> for <paramref name="kind"/>. The payload is passed to its
    /// <paramref name="parameterName"/> parameter. Registering the same kind again replaces the earlier entry.
    /// </summary>
    public MdsChatPartRegistry Register<TComponent, TPayload>(string kind, string parameterName = "Payload")
        where TComponent : IComponent
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);
        ArgumentException.ThrowIfNullOrWhiteSpace(parameterName);
        _registrations[kind] = new Registration(typeof(TComponent), typeof(TPayload), parameterName);
        return this;
    }

    /// <summary>
    /// Finds the registration for <paramref name="part"/>. Returns false when the kind is unknown or the payload
    /// is not of the registered type, so the caller can fall back instead of throwing at render time.
    /// </summary>
    public bool TryResolve(MdsChatPart part, out Registration registration)
    {
        if (_registrations.TryGetValue(part.Kind, out registration!)
            && (part.Payload is null ? !registration.PayloadType.IsValueType : registration.PayloadType.IsInstanceOfType(part.Payload)))
        {
            return true;
        }

        registration = null!;
        return false;
    }
}
