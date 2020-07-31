namespace OneSms.Droid.Server.Models
{
    public class SimInfo
    {

        public SimInfo(int id, string displayName, string iccId, int slot)
        {
            Id = id;
            DisplayName = displayName;
            IccId = iccId;
            Slot = slot;
        }

        public int Id { get; set; }

        public string DisplayName { get; set; }

        public string IccId { get; set; }

        public int Slot { get; set; }

        public override string ToString() => $"SimInfo{{Id:{Id}, DisplayName:{DisplayName}, IccId:{IccId}, Slot:{Slot}}}";
    }
}