namespace Cosm.Net.Models;
public record Height(
    long RevisionNumber,
    long RevisionHeight
)
{
    public static Height Zero { get; } = new Height(0, 0);
}
