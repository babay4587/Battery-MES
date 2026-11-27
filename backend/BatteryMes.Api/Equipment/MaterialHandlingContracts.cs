namespace BatteryMes.Api.Equipment;

public sealed class MaterialLoadRequest
{
    public string? Software { get; set; }
    public List<MaterialLoadEquipment>? EquipmentInfo { get; set; }
}

public sealed class MaterialLoadEquipment
{
    public string? EquipmentCode { get; set; }
    public string? ReceiptNumber { get; set; }
    public bool ResultFlag { get; set; }
    public List<MaterialLoadMaterial>? MaterialInfo { get; set; }
}

public sealed class MaterialLoadMaterial
{
    public string? LabelNo { get; set; }
    public string? Location { get; set; }
}

public sealed class MaterialUnloadRequest
{
    public string? Software { get; set; }
    public List<MaterialUnloadEquipment>? EquipmentInfo { get; set; }
}

public sealed class MaterialUnloadEquipment
{
    public string? EquipmentCode { get; set; }
    public string? ReceiptNumber { get; set; }
    public bool ResultFlag { get; set; }
    public List<MaterialUnloadMaterial>? MaterialInfo { get; set; }
}

public sealed class MaterialUnloadMaterial
{
    public string? LabelNo { get; set; }
    public string? MaterialQuantity { get; set; }
    public string? UomCode { get; set; }
}

public sealed class PartLoadRequest
{
    public string? Software { get; set; }
    public List<PartLoadEquipment>? EquipmentInfo { get; set; }
}

public sealed class PartLoadEquipment
{
    public string? EquipmentCode { get; set; }
    public string? ReceiptNumber { get; set; }
    public bool ResultFlag { get; set; }
    public List<PartLoadPart>? SpartInfo { get; set; }
}

public sealed class PartLoadPart
{
    public string? SpartID { get; set; }
    public string? PartDesc { get; set; }
    public string? Location { get; set; }
    public string? UseLifetime { get; set; }
    public string? WarningLife { get; set; }
}

public sealed class PartUnloadRequest
{
    public string? Software { get; set; }
    public List<PartUnloadEquipment>? EquipmentInfo { get; set; }
}

public sealed class PartUnloadEquipment
{
    public string? EquipmentCode { get; set; }
    public string? ReceiptNumber { get; set; }
    public List<PartLifeItem>? Partlifetimeinfo { get; set; }
}

public sealed class PartLifeRequest
{
    public string? Software { get; set; }
    public List<PartLifeEquipment>? EquipmentInfo { get; set; }
}

public sealed class PartLifeEquipment
{
    public string? EquipmentCode { get; set; }
    public string? EmployeeNo { get; set; }
    public List<PartLifeItem>? Partlifetimeinfo { get; set; }
}

public sealed class PartLifeItem
{
    public string? PartNo { get; set; }
    public string? PartDesc { get; set; }
    public string? UseLifetime { get; set; }
}

public sealed class MaterialCheckRequest
{
    public string? Software { get; set; }
    public List<MaterialCheckEquipment>? EquipmentInfo { get; set; }
}

public sealed class MaterialCheckEquipment
{
    public string? EquipmentCode { get; set; }
    public string? ProcessNo { get; set; }
    public List<MaterialCheckMaterial>? MaterialInfo { get; set; }
}

public sealed class MaterialCheckMaterial
{
    public string? LabelNo { get; set; }
    public string? Location { get; set; }
}

public sealed class SeedReceiptRequest
{
    public string? EquipmentCode { get; set; }
    public string? ReceiptNumber { get; set; }
}

public sealed class SeedMaterialCheckRequest
{
    public string? EquipmentCode { get; set; }
    public string? LabelNo { get; set; }
    public string? ProcessNo { get; set; }
}

public sealed class SeedPartLifeRequest
{
    public string? EquipmentCode { get; set; }
    public string? PartNo { get; set; }
}
