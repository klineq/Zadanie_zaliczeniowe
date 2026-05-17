namespace SpaceBaseLogistics.Models;

// [WYMÓG 1: Klasy]
public class ResourceStack
{
    public Resource Resource { get; }
    public double Amount { get; set; }

    public ResourceStack(Resource resource, double amount)
    {
        Resource = resource;
        Amount = amount;
    }

    // [WYMÓG 10: Przeciążanie operatorów]
    public static ResourceStack operator +(ResourceStack left, ResourceStack right)
    {
        if (left.Resource.Name != right.Resource.Name)
        {
            throw new InvalidOperationException(
                $"Nie można sumować różnych surowców: {left.Resource.Name} i {right.Resource.Name}.");
        }

        return new ResourceStack(left.Resource, left.Amount + right.Amount);
    }

    public override string ToString() => $"{Amount:F1} {Resource.Unit} {Resource.Name}";
}
