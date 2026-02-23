namespace SDNet.Models
{
    public sealed class HardwareTask : SDTask
    {
        public string EquipmentModel { get; set; } = string.Empty;
        public string AssetNumber { get; set; } = string.Empty;

        public override string TaskTypeName => SDTaskTypes.HardwareTask;

        public override object Clone()
        {
            var clone = new HardwareTask();
            CopyBaseTo(clone);
            clone.EquipmentModel = EquipmentModel;
            clone.AssetNumber = AssetNumber;
            return clone;
        }
    }
}
