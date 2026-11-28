using MaximTechnology.Entity;

namespace MaximTechnology.Dto;

public record DriverResponse(Driver Driver, int Distance, List<PointRecord> Route);