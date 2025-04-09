namespace MineIntoTheDeep.Models.Pioches
{
    public abstract class Pioche(int damages)
    {
        // Instance variables
        public Guid Id { get; init; } = Guid.NewGuid();
        public int Damages { get; protected init; } = damages;

        // Overrides
        public override string ToString()
        {
            return $"Pioche : Damages {Damages}";
        }
    }
}